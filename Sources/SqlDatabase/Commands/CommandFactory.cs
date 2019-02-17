using System;
using System.Collections.Generic;
using System.Linq;
using SqlDatabase.Configuration;
using SqlDatabase.IO;
using SqlDatabase.Scripts;

namespace SqlDatabase.Commands
{
    internal sealed class CommandFactory
    {
        public ILogger Log { get; set; }

        public DatabaseCommandBase Resolve(CommandLine commandLine)
        {
            var configuration = new ConfigurationManager();
            configuration.LoadFrom(commandLine.ConfigurationFile);

            switch (commandLine.Command)
            {
                case Command.Upgrade:
                    return ResolveUpgradeCommand(commandLine, configuration);
                case Command.Create:
                    return ResolveCreateCommand(commandLine, configuration);
                case Command.Execute:
                    return ResolveExecuteCommand(commandLine, configuration);
            }

            throw new NotImplementedException("Unexpected command type [{0}].".FormatWith(commandLine.Command));
        }

        internal Database CreateDatabase(CommandLine cmd, IConfigurationManager configuration)
        {
            var database = new Database
            {
                ConnectionString = cmd.Connection.ToString(),
                Log = Log,
                Configuration = configuration.SqlDatabase,
                Transaction = cmd.Transaction
            };

            var configurationVariables = configuration.SqlDatabase.Variables;
            foreach (var name in configurationVariables.AllKeys)
            {
                database.Variables.SetValue(VariableSource.ConfigurationFile, name, configurationVariables[name].Value);
            }

            foreach (var entry in cmd.Variables)
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
            else if (invalidNames.Count > 1)
            {
                throw new InvalidOperationException("The following variable names are invalid: {0}.".FormatWith(string.Join(", ", invalidNames)));
            }

            return database;
        }

        private static void FillSources(IList<IFileSystemInfo> sources, IList<string> scripts)
        {
            foreach (var script in scripts)
            {
                sources.Add(FileSystemFactory.FileSystemInfoFromPath(script));
            }
        }

        private DatabaseExecuteCommand ResolveExecuteCommand(CommandLine commandLine, IConfigurationManager configuration)
        {
            var sequence = new CreateScriptSequence
            {
                ScriptFactory = CreateScriptFactory(configuration)
            };

            FillSources(sequence.Sources, commandLine.Scripts);

            return new DatabaseExecuteCommand
            {
                Log = Log,
                Database = CreateDatabase(commandLine, configuration),
                ScriptSequence = sequence
            };
        }

        private DatabaseUpgradeCommand ResolveUpgradeCommand(CommandLine commandLine, IConfigurationManager configuration)
        {
            var sequence = new UpgradeScriptSequence
            {
                ScriptFactory = CreateScriptFactory(configuration)
            };

            FillSources(sequence.Sources, commandLine.Scripts);

            return new DatabaseUpgradeCommand
            {
                Log = Log,
                Database = CreateDatabase(commandLine, configuration),
                ScriptSequence = sequence
            };
        }

        private DatabaseCreateCommand ResolveCreateCommand(CommandLine commandLine, IConfigurationManager configuration)
        {
            var sequence = new CreateScriptSequence
            {
                ScriptFactory = CreateScriptFactory(configuration)
            };

            FillSources(sequence.Sources, commandLine.Scripts);

            return new DatabaseCreateCommand
            {
                Log = Log,
                Database = CreateDatabase(commandLine, configuration),
                ScriptSequence = sequence
            };
        }

        private IScriptFactory CreateScriptFactory(IConfigurationManager configuration)
        {
            return new ScriptFactory { Configuration = configuration.SqlDatabase };
        }
    }
}
