using System;
using System.Collections.Generic;

namespace SqlDatabase.Configuration
{
    internal sealed class CommandLineFactory
    {
        internal const string CommandUpgrade = "Upgrade";
        internal const string CommandCreate = "Create";
        internal const string CommandExecute = "Execute";
        internal const string CommandExport = "Export";
        internal const string CommandEcho = "Echo";

        public ICommandLine Resolve(CommandLine args)
        {
            var commandArgs = new List<Arg>(args.Args);
            var result = FindCommand(commandArgs);

            result.Parse(new CommandLine(commandArgs, args.Original));

            return result;
        }

        internal ICommandLine FindCommand(List<Arg> commandArgs)
        {
            Func<ICommandLine> result = null;
            string name = null;

            for (var i = 0; i < commandArgs.Count; i++)
            {
                var arg = commandArgs[i];
                Func<ICommandLine> factory;

                if (!arg.IsPair && (factory = Resolve(arg.Value)) != null)
                {
                    if (result != null)
                    {
                        throw new InvalidCommandLineException("The <command> is duplicated: [{0}] and [{1}].".FormatWith(name, arg.Value));
                    }

                    result = factory;
                    name = arg.Value;
                    commandArgs.RemoveAt(i);
                    i--;
                }
            }

            if (result == null)
            {
                throw new InvalidCommandLineException("A <command> not found.");
            }

            return result();
        }

        private static Func<ICommandLine> Resolve(string name)
        {
            if (CommandCreate.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return () => new CreateCommandLine();
            }

            if (CommandUpgrade.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return () => new UpgradeCommandLine();
            }

            if (CommandExecute.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return () => new ExecuteCommandLine();
            }

            if (CommandExport.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return () => new ExportCommandLine();
            }

            if (CommandEcho.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return () => new EchoCommandLine();
            }

            return null;
        }
    }
}
