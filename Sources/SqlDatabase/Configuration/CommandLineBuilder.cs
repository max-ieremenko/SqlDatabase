using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace SqlDatabase.Configuration
{
    internal sealed class CommandLineBuilder
    {
        private const string ArgDatabase = "-database";
        private const string ArgScripts = "-from";
        private const string ArgTransaction = "-transaction";
        private const string ArgVariable = "-var";

        internal CommandLine Line { get; } = new CommandLine();

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
            Line.Scripts = value;

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

        public CommandLine Build()
        {
            if (Line.Connection == null)
            {
                throw new InvalidCommandException("Argument {0} is not specified.".FormatWith(ArgDatabase));
            }

            if (string.IsNullOrEmpty(Line.Scripts))
            {
                throw new InvalidCommandException("Argument {0} is not specified.".FormatWith(ArgScripts));
            }

            if (Line.Command == Command.Create && Line.Transaction != TransactionMode.None)
            {
                throw new InvalidCommandException(ArgTransaction, "Transaction mode is not supported.");
            }

            return Line;
        }

        public string[] BuildArray()
        {
            var cmd = Build();

            var result = new List<string>
            {
                cmd.Command.ToString(),
                "{0}={1}".FormatWith(ArgDatabase, cmd.Connection),
                "{0}={1}".FormatWith(ArgScripts, cmd.Scripts)
            };

            if (cmd.Transaction == default(TransactionMode))
            {
                result.Add("{0}={1}".FormatWith(ArgTransaction, cmd.Transaction));
            }

            foreach (var entry in cmd.Variables)
            {
                result.Add("{0}{1}={2}".FormatWith(ArgVariable, entry.Key, entry.Value));
            }

            return result.ToArray();
        }

        private static bool SplitArg(string keyValue, out string key, out string value)
        {
            key = null;
            value = null;

            var index = keyValue.IndexOf("=", StringComparison.OrdinalIgnoreCase);
            if (index <= 0 || index == keyValue.Length - 1)
            {
                return false;
            }

            key = keyValue.Substring(0, index).Trim();
            value = keyValue.Substring(index + 1).Trim();

            return true;
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

            return false;
        }
    }
}