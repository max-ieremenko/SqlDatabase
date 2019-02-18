using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace SqlDatabase.Configuration
{
    internal sealed class CommandLineBuilder
    {
        private const string Base64Sign = "+";
        private const string ArgSign = "-";
        private const string ArgDatabase = ArgSign + "database";
        private const string ArgScripts = ArgSign + "from";
        private const string ArgTransaction = ArgSign + "transaction";
        private const string ArgVariable = ArgSign + "var";
        private const string ArgConfiguration = ArgSign + "configuration";
        private const string ArgPreFormatOutputLogs = ArgSign + "preFormatOutputLogs";

        public CommandLineBuilder()
            : this(new CommandLine())
        {
        }

        public CommandLineBuilder(CommandLine line)
        {
            Line = line;
        }

        internal CommandLine Line { get; }

        public static CommandLine FromArguments(params string[] args)
        {
            var builder = new CommandLineBuilder()
                .SetCommand(args[0]);

            for (var i = 1; i < args.Length; i++)
            {
                builder.ParseArgument(args[i]);
            }

            return builder.Build();
        }

        public static bool PreFormatOutputLogs(string[] args)
        {
            foreach (var arg in args)
            {
                if (SplitArg(arg, out var key, out var value)
                    && ArgPreFormatOutputLogs.Equals(key, StringComparison.OrdinalIgnoreCase)
                    && bool.Parse(value))
                {
                    return true;
                }
            }

            return false;
        }

        public CommandLineBuilder SetCommand(string commandName)
        {
            if (!Enum.TryParse<Command>(commandName, true, out var command) || command == Command.Unknown)
            {
                throw new InvalidCommandException(commandName, "Unknown command [{0}].".FormatWith(commandName));
            }

            return SetCommand(command);
        }

        public CommandLineBuilder SetCommand(Command command)
        {
            Line.Command = command;
            return this;
        }

        public CommandLineBuilder SetConnection(string connectionString)
        {
            try
            {
                Line.Connection = new SqlConnectionStringBuilder(connectionString);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidCommandException(ArgDatabase, "Invalid connection string value.", ex);
            }

            return this;
        }

        public CommandLineBuilder SetScripts(string value)
        {
            Line.Scripts.Add(value);

            return this;
        }

        public CommandLineBuilder SetTransaction(string modeName)
        {
            if (!Enum.TryParse<TransactionMode>(modeName, true, out var mode))
            {
                throw new InvalidCommandException(ArgTransaction, "Unknown transaction mode [{0}].".FormatWith(modeName));
            }

            return SetTransaction(mode);
        }

        public CommandLineBuilder SetTransaction(TransactionMode mode)
        {
            Line.Transaction = mode;
            return this;
        }

        public CommandLineBuilder ParseArgument(string arg)
        {
            if (!ApplyArg(arg))
            {
                throw new InvalidCommandException(arg, "Unknown argument [{0}].".FormatWith(arg));
            }

            return this;
        }

        public CommandLineBuilder SetVariable(string name, string value)
        {
            name = name?.Trim();
            if (string.IsNullOrEmpty(name))
            {
                throw new InvalidCommandException(ArgVariable, "Invalid variable name [{0}].".FormatWith(name));
            }

            if (Line.Variables.ContainsKey(name))
            {
                throw new InvalidCommandException(ArgVariable, "Variable with name [{0}] is duplicated.".FormatWith(name));
            }

            Line.Variables.Add(name, value);

            return this;
        }

        public CommandLineBuilder SetVariable(string nameValue)
        {
            if (!SplitArg(nameValue, out var key, out var value))
            {
                throw new InvalidCommandException(ArgVariable, "Invalid variable value definition [{0}].".FormatWith(nameValue));
            }

            return SetVariable(key, value);
        }

        public CommandLineBuilder SetConfigurationFile(string configurationFile)
        {
            Line.ConfigurationFile = configurationFile;
            return this;
        }

        public CommandLineBuilder SetPreFormatOutputLogs(bool value)
        {
            Line.PreFormatOutputLogs = value;
            return this;
        }

        public CommandLine Build()
        {
            if (Line.Connection == null)
            {
                throw new InvalidCommandException("Argument {0} is not specified.".FormatWith(ArgDatabase));
            }

            if (Line.Scripts.Count == 0)
            {
                throw new InvalidCommandException("Argument {0} is not specified.".FormatWith(ArgScripts));
            }

            if (Line.Command == Command.Create && Line.Transaction != TransactionMode.None)
            {
                throw new InvalidCommandException(ArgTransaction, "Transaction mode is not supported.");
            }

            return Line;
        }

        public string[] BuildArray(bool escaped)
        {
            var cmd = Build();

            var result = new List<string>
            {
                cmd.Command.ToString(),
                CombineArg(ArgDatabase, cmd.Connection.ToString(), escaped)
            };

            foreach (var script in cmd.Scripts)
            {
                result.Add(CombineArg(ArgScripts, script, escaped));
            }

            if (cmd.Transaction == default(TransactionMode))
            {
                result.Add(CombineArg(ArgTransaction, cmd.Transaction.ToString(), escaped));
            }

            if (!string.IsNullOrEmpty(cmd.ConfigurationFile))
            {
                result.Add(CombineArg(ArgConfiguration, cmd.ConfigurationFile, escaped));
            }

            foreach (var entry in cmd.Variables)
            {
                result.Add(CombineArg(ArgVariable + entry.Key, entry.Value, escaped));
            }

            if (cmd.PreFormatOutputLogs)
            {
                result.Add(CombineArg(ArgPreFormatOutputLogs, cmd.PreFormatOutputLogs.ToString(), false));
            }

            return result.ToArray();
        }

        private static bool SplitArg(string keyValue, out string key, out string value)
        {
            key = null;
            value = null;

            if (keyValue == null || keyValue.Length < 3)
            {
                return false;
            }

            var index = keyValue.IndexOf("=", StringComparison.OrdinalIgnoreCase);
            if (index <= 0 || index == keyValue.Length - 1)
            {
                return false;
            }

            key = keyValue.Substring(0, index).Trim();
            value = keyValue.Substring(index + 1).Trim();

            if (!key.StartsWith(Base64Sign + ArgSign, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            key = key.Substring(1);

            try
            {
                value = Encoding.UTF8.GetString(Convert.FromBase64String(value));
                return true;
            }
            catch (ArgumentException)
            {
            }
            catch (FormatException)
            {
            }

            return false;
        }

        private static string CombineArg(string key, string value, bool escaped)
        {
            if (escaped && !string.IsNullOrEmpty(value))
            {
                return "{0}{1}={2}".FormatWith(
                    Base64Sign,
                    key,
                    Convert.ToBase64String(Encoding.UTF8.GetBytes(value)));
            }

            return "{0}={1}".FormatWith(key, value);
        }

        private bool ApplyArg(string arg)
        {
            if (!SplitArg(arg, out var key, out var value))
            {
                return false;
            }

            if (ArgDatabase.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                SetConnection(value);
                return true;
            }

            if (ArgScripts.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                SetScripts(value);
                return true;
            }

            if (ArgTransaction.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                SetTransaction(value);
                return true;
            }

            if (key.StartsWith(ArgVariable, StringComparison.OrdinalIgnoreCase))
            {
                SetVariable(key.Substring(ArgVariable.Length), value);
                return true;
            }

            if (ArgConfiguration.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                SetConfigurationFile(value);
                return true;
            }

            if (ArgPreFormatOutputLogs.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                SetPreFormatOutputLogs(bool.Parse(value));
                return true;
            }

            return false;
        }
    }
}