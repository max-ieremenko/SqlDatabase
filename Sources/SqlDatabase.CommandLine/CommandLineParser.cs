using SqlDatabase.Adapter;
using SqlDatabase.CommandLine.Internal;

namespace SqlDatabase.CommandLine;

public static class CommandLineParser
{
    private const string CommandUpgrade = "Upgrade";
    private const string CommandCreate = "Create";
    private const string CommandExecute = "Execute";
    private const string CommandExport = "Export";

    public static bool HelpRequested(string[] args, out string? command)
    {
        command = null;

        if (args.Length == 0)
        {
            return true;
        }

        // -h
        // execute
        // execute -h
        if (args.Length == 1)
        {
            if (IsHelpArg(args[0]))
            {
                return true;
            }

            return IsCommand(args[0], out command);
        }

        if (!IsCommand(args[0], out command))
        {
            return false;
        }

        return IsHelpArg(args[1]) && args.Length == 2;
    }

    public static ICommandLine Parse(HostedRuntime runtime, string[] args)
    {
        if (Arg.TryParse(args[0], out _))
        {
            throw new InvalidCommandLineException("<command> not found.");
        }

        var command = args[0];
        var commandArgs = args.Skip(1);

        if (CommandCreate.Equals(command, StringComparison.OrdinalIgnoreCase))
        {
            return ParseCreate(runtime, commandArgs);
        }

        if (CommandExecute.Equals(command, StringComparison.OrdinalIgnoreCase))
        {
            return ParseExecute(runtime, commandArgs);
        }

        if (CommandUpgrade.Equals(command, StringComparison.OrdinalIgnoreCase))
        {
            return ParseUpgrade(runtime, commandArgs);
        }

        if (CommandExport.Equals(command, StringComparison.OrdinalIgnoreCase))
        {
            return ParseExport(commandArgs);
        }

        throw new InvalidCommandLineException($"Unknown command [{args[0]}].");
    }

    public static void AddVariable(Dictionary<string, string> target, string nameValue)
    {
        if (!Arg.TryParse(ArgNames.Sign + nameValue, out var arg))
        {
            throw new InvalidCommandLineException($"Invalid variable value definition [{nameValue}].");
        }

        if (target.ContainsKey(arg.Key))
        {
            throw new InvalidCommandLineException($"Variable [{arg.Key}] is duplicated.");
        }

        target.Add(arg.Key, arg.Value ?? string.Empty);
    }

    private static bool IsCommand(string value, out string? command)
    {
        if (CommandUpgrade.Equals(value, StringComparison.OrdinalIgnoreCase)
            || CommandCreate.Equals(value, StringComparison.OrdinalIgnoreCase)
            || CommandExecute.Equals(value, StringComparison.OrdinalIgnoreCase)
            || CommandExport.Equals(value, StringComparison.OrdinalIgnoreCase))
        {
            command = value;
            return true;
        }

        command = null;
        return false;
    }

    private static bool IsHelpArg(string value) =>
        Arg.TryParse(value, out var arg)
        && arg.Value == null
        && (arg.Is(ArgNames.Help) || arg.Is(ArgNames.HelpShort));

    private static CreateCommandLine ParseCreate(HostedRuntime runtime, IEnumerable<string> args) =>
        new CommandLineBinder<CreateCommandLine>(new())
            .BindDatabase((i, value) => i.Database = value)
            .BindScripts(i => i.From)
            .BindVariables(i => i.Variables)
            .BindConfiguration((i, value) => i.Configuration = value)
            .BindLog((i, value) => i.Log = value)
            .BindUsePowerShell(runtime, (i, value) => i.UsePowerShell = value)
            .BindWhatIf((i, value) => i.WhatIf = value)
            .Build(args);

    private static ExecuteCommandLine ParseExecute(HostedRuntime runtime, IEnumerable<string> args) =>
        new CommandLineBinder<ExecuteCommandLine>(new())
            .BindDatabase((i, value) => i.Database = value)
            .BindScripts(i => i.From)
            .BindTransaction((i, value) => i.Transaction = value)
            .BindVariables(i => i.Variables)
            .BindConfiguration((i, value) => i.Configuration = value)
            .BindLog((i, value) => i.Log = value)
            .BindUsePowerShell(runtime, (i, value) => i.UsePowerShell = value)
            .BindWhatIf((i, value) => i.WhatIf = value)
            .Build(args);

    private static UpgradeCommandLine ParseUpgrade(HostedRuntime runtime, IEnumerable<string> args) =>
        new CommandLineBinder<UpgradeCommandLine>(new())
            .BindDatabase((i, value) => i.Database = value)
            .BindScripts(i => i.From)
            .BindTransaction((i, value) => i.Transaction = value)
            .BindVariables(i => i.Variables)
            .BindConfiguration((i, value) => i.Configuration = value)
            .BindLog((i, value) => i.Log = value)
            .BindUsePowerShell(runtime, (i, value) => i.UsePowerShell = value)
            .BindWhatIf((i, value) => i.WhatIf = value)
            .BindFolderAsModuleName((i, value) => i.FolderAsModuleName = value)
            .Build(args);

    private static ExportCommandLine ParseExport(IEnumerable<string> args) =>
        new CommandLineBinder<ExportCommandLine>(new())
            .BindDatabase((i, value) => i.Database = value)
            .BindScripts(i => i.From)
            .BindExportToTable((i, value) => i.DestinationTableName = value)
            .BindExportToFile((i, value) => i.DestinationFileName = value)
            .BindVariables(i => i.Variables)
            .BindConfiguration((i, value) => i.Configuration = value)
            .BindLog((i, value) => i.Log = value)
            .Build(args);
}