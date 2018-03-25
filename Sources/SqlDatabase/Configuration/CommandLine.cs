using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace SqlDatabase.Configuration
{
    internal sealed class CommandLine
    {
        private const string ArgDatabase = "-database";
        private const string ArgScripts = "-from";
        private const string ArgTransaction = "-transaction";
        private const string ArgVariable = "-var";

        public Command Command { get; private set; }

        public SqlConnectionStringBuilder Connection { get; private set; }

        public TransactionMode Transaction { get; private set; }

        public string Scripts { get; private set; }

        public IDictionary<string, string> Variables { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public static CommandLine Parse(params string[] args)
        {
            var result = new CommandLine();

            if (!Enum.TryParse<Command>(args[0], true, out var command) || command == Command.Unknown)
            {
                throw new InvalidCommandException(args[0], "Unknown command [{0}].".FormatWith(args[0]));
            }

            result.Command = command;

            for (var i = 1; i < args.Length; i++)
            {
                var arg = args[i];
                if (!ApplyArg(result, arg))
                {
                    throw new InvalidCommandException(arg, "Unknown argument [{0}].".FormatWith(arg));
                }
            }

            if (result.Connection == null)
            {
                throw new InvalidCommandException("Argument {0} is not specified.".FormatWith(ArgDatabase));
            }

            if (string.IsNullOrEmpty(result.Scripts))
            {
                throw new InvalidCommandException("Argument {0} is not specified.".FormatWith(ArgScripts));
            }

            if (result.Command == Command.Create && result.Transaction != TransactionMode.None)
            {
                throw new InvalidCommandException(ArgTransaction, "Transaction mode is not supported.");
            }

            return result;
        }

        private static bool ApplyArg(CommandLine cmd, string arg)
        {
            var index = arg.IndexOf("=", StringComparison.OrdinalIgnoreCase);
            if (index <= 0 || index == arg.Length - 1)
            {
                return false;
            }

            var key = arg.Substring(0, index).Trim();
            var value = arg.Substring(index + 1).Trim();

            if (ArgDatabase.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    cmd.Connection = new SqlConnectionStringBuilder(value);
                }
                catch (ArgumentException ex)
                {
                    throw new InvalidCommandException(value, "Invalid connection string value.", ex);
                }

                return true;
            }

            if (ArgScripts.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                cmd.Scripts = value;
                return true;
            }

            if (ArgTransaction.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                if (!Enum.TryParse<TransactionMode>(value, true, out var mode))
                {
                    throw new InvalidCommandException(value, "Unknown transaction mode [{0}].".FormatWith(value));
                }

                cmd.Transaction = mode;
                return true;
            }

            if (key.StartsWith(ArgVariable, StringComparison.OrdinalIgnoreCase))
            {
                var name = key.Substring(ArgVariable.Length).Trim();
                if (name.Length == 0)
                {
                    throw new InvalidCommandException(key, "Invalid variable name [{0}].".FormatWith(key));
                }

                if (cmd.Variables.ContainsKey(name))
                {
                    throw new InvalidCommandException(key, "Variable with name [{0}] is duplicated.".FormatWith(name));
                }

                cmd.Variables.Add(name, value);
                return true;
            }

            return false;
        }
    }
}