using System;
using System.Linq;

namespace SqlDatabase.Configuration;

internal sealed class CommandLineFactory
{
    internal const string CommandUpgrade = "Upgrade";
    internal const string CommandCreate = "Create";
    internal const string CommandExecute = "Execute";
    internal const string CommandExport = "Export";
    internal const string CommandEcho = "Echo";

    public CommandLine Args { get; set; }

    public string ActiveCommandName { get; private set; }

    public bool ShowCommandHelp { get; private set; }

    public bool Bind()
    {
        var commandArgs = Args.Args.ToList();
        if (commandArgs.Count == 0)
        {
            return false;
        }

        if (commandArgs[0].IsPair)
        {
            throw new InvalidCommandLineException("<command> not found.");
        }

        ActiveCommandName = commandArgs[0].Value;
        var command = CreateCommand(ActiveCommandName);

        if (command == null)
        {
            throw new InvalidCommandLineException("Unknown command [{0}].".FormatWith(ActiveCommandName));
        }

        commandArgs.RemoveAt(0);

        for (var i = 0; i < commandArgs.Count; i++)
        {
            var arg = commandArgs[i];

            if (arg.IsPair
                && (Arg.Help.Equals(arg.Key, StringComparison.OrdinalIgnoreCase)
                    || Arg.HelpShort.Equals(arg.Key, StringComparison.OrdinalIgnoreCase)))
            {
                ShowCommandHelp = true;
                commandArgs.RemoveAt(i);
                break;
            }
        }

        Args = new CommandLine(commandArgs, Args.Original);
        return true;
    }

    public ICommandLine Resolve()
    {
        var command = CreateCommand(ActiveCommandName);
        command.Parse(Args);
        return command;
    }

    internal static ICommandLine CreateCommand(string name)
    {
        if (CommandCreate.Equals(name, StringComparison.OrdinalIgnoreCase))
        {
            return new CreateCommandLine();
        }

        if (CommandUpgrade.Equals(name, StringComparison.OrdinalIgnoreCase))
        {
            return new UpgradeCommandLine();
        }

        if (CommandExecute.Equals(name, StringComparison.OrdinalIgnoreCase))
        {
            return new ExecuteCommandLine();
        }

        if (CommandExport.Equals(name, StringComparison.OrdinalIgnoreCase))
        {
            return new ExportCommandLine();
        }

        if (CommandEcho.Equals(name, StringComparison.OrdinalIgnoreCase))
        {
            return new EchoCommandLine();
        }

        return null;
    }
}