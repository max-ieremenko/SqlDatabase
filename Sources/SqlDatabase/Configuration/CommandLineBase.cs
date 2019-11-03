using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using SqlDatabase.Commands;
using SqlDatabase.IO;
using SqlDatabase.Scripts;

namespace SqlDatabase.Configuration
{
    internal abstract class CommandLineBase : ICommandLine
    {
        public SqlConnectionStringBuilder Connection { get; set; }

        public IList<IFileSystemInfo> Scripts { get; } = new List<IFileSystemInfo>();

        public IDictionary<string, string> Variables { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public string ConfigurationFile { get; set; }

        public IFileSystemFactory FileSystemFactory { get; set; } = new FileSystemFactory();

        public void Parse(CommandLine args)
        {
            foreach (var arg in args.Args)
            {
                ApplyArg(arg);
            }

            if (Connection == null)
            {
                throw new InvalidCommandLineException("Options {0} is not specified.".FormatWith(Arg.Database));
            }

            if (Scripts.Count == 0)
            {
                throw new InvalidCommandLineException("Options {0} is not specified.".FormatWith(Arg.Scripts));
            }

            Validate();
        }

        public abstract ICommand CreateCommand(ILogger logger);

        internal Database CreateDatabase(ILogger logger, IConfigurationManager configuration, TransactionMode transaction, bool whatIf)
        {
            var database = new Database
            {
                ConnectionString = Connection.ToString(),
                Log = logger,
                Configuration = configuration.SqlDatabase,
                Transaction = transaction,
                WhatIf = whatIf
            };

            var configurationVariables = configuration.SqlDatabase.Variables;
            foreach (var name in configurationVariables.AllKeys)
            {
                database.Variables.SetValue(VariableSource.ConfigurationFile, name, configurationVariables[name].Value);
            }

            foreach (var entry in Variables)
            {
                database.Variables.SetValue(VariableSource.CommandLine, entry.Key, entry.Value);
            }

            var invalidNames = database
                .Variables
                .GetNames()
                .OrderBy(i => i)
                .Where(i => !SqlScriptVariableParser.IsValidVariableName(i))
                .Select(i => "[{0}]".FormatWith(i))
                .ToList();

            if (invalidNames.Count == 1)
            {
                throw new InvalidOperationException("The variable name {0} is invalid.".FormatWith(invalidNames[0]));
            }

            if (invalidNames.Count > 1)
            {
                throw new InvalidOperationException("The following variable names are invalid: {0}.".FormatWith(string.Join(", ", invalidNames)));
            }

            return database;
        }

        protected internal virtual void Validate()
        {
        }

        protected static bool TryParseSwitchParameter(Arg arg, string parameterName, out bool value)
        {
            if (parameterName.Equals(arg.Key, StringComparison.OrdinalIgnoreCase))
            {
                value = string.IsNullOrEmpty(arg.Value) || bool.Parse(arg.Value);
                return true;
            }

            if (!arg.IsPair && parameterName.Equals(arg.Value, StringComparison.OrdinalIgnoreCase))
            {
                value = true;
                return true;
            }

            value = false;
            return false;
        }

        protected static bool TryParseWhatIf(Arg arg, out bool whatIf) => TryParseSwitchParameter(arg, Arg.WhatIf, out whatIf);

        protected virtual bool ParseArg(Arg arg)
        {
            return false;
        }

        protected void SetInLineScript(string value)
        {
            var index = Scripts.Count + 1;
            var script = FileSystemFactory.FromContent("from{0}.sql".FormatWith(index), value);

            Scripts.Add(script);
        }

        private void ApplyArg(Arg arg)
        {
            bool isParsed;
            try
            {
                isParsed = (arg.IsPair && TryParseKnownPair(arg)) || ParseArg(arg);
            }
            catch (InvalidCommandLineException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidCommandLineException("Fail to parse option [{0}].".FormatWith(arg), ex);
            }

            if (!isParsed)
            {
                throw new InvalidCommandLineException("Unknown option [{0}].".FormatWith(arg));
            }
        }

        private bool TryParseKnownPair(Arg arg)
        {
            if (Arg.Database.Equals(arg.Key, StringComparison.OrdinalIgnoreCase))
            {
                SetConnection(arg.Value);
                return true;
            }

            if (Arg.Scripts.Equals(arg.Key, StringComparison.OrdinalIgnoreCase))
            {
                SetScripts(arg.Value);
                return true;
            }

            if (arg.Key.StartsWith(Arg.Variable, StringComparison.OrdinalIgnoreCase))
            {
                SetVariable(arg.Key.Substring(Arg.Variable.Length), arg.Value);
                return true;
            }

            if (Arg.Configuration.Equals(arg.Key, StringComparison.OrdinalIgnoreCase))
            {
                SetConfigurationFile(arg.Value);
                return true;
            }

            return false;
        }

        private void SetConnection(string connectionString)
        {
            try
            {
                Connection = new SqlConnectionStringBuilder(connectionString);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidCommandLineException(Arg.Database, "Invalid connection string value.", ex);
            }
        }

        private void SetScripts(string value)
        {
            Scripts.Add(FileSystemFactory.FileSystemInfoFromPath(value));
        }

        private void SetVariable(string name, string value)
        {
            name = name?.Trim();
            if (string.IsNullOrEmpty(name))
            {
                throw new InvalidCommandLineException(Arg.Variable, "Invalid variable name [{0}].".FormatWith(name));
            }

            if (Variables.ContainsKey(name))
            {
                throw new InvalidCommandLineException(Arg.Variable, "Variable with name [{0}] is duplicated.".FormatWith(name));
            }

            Variables.Add(name, value);
        }

        private void SetConfigurationFile(string configurationFile)
        {
            ConfigurationFile = configurationFile;
        }
    }
}
