using SqlDatabase.Adapter;
using SqlDatabase.CommandLine;
using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell.Internal;

public static class CmdLetExecutor
{
    // only for tests
    internal static ISqlDatabaseProgram? Program { get; set; }

    public static void RunCreate(IDictionary<string, object?> param, string currentDirectory, Action<string> writeInfo, Action<string> writeError)
    {
        var commandLine = new CreateCommandLine
        {
            Database = (string)param[nameof(CreateCommandLine.Database)]!,
            Configuration = (string?)param[nameof(CreateCommandLine.Configuration)],
            Log = (string?)param[nameof(CreateCommandLine.Log)],
            WhatIf = (bool)param[nameof(CreateCommandLine.WhatIf)]!
        };

        CommandLineTools.AppendFrom(commandLine.From, false, (string[]?)param[nameof(CreateCommandLine.From)]);
        CommandLineTools.AppendVariables(commandLine.Variables, (string[]?)param[nameof(CreateCommandLine.Variables)]);

        Run(commandLine, currentDirectory, writeInfo, writeError);
    }

    public static void RunExecute(IDictionary<string, object?> param, string currentDirectory, Action<string> writeInfo, Action<string> writeError)
    {
        var commandLine = new ExecuteCommandLine
        {
            Database = (string)param[nameof(ExecuteCommandLine.Database)]!,
            Transaction = (TransactionMode)((int)param[nameof(ExecuteCommandLine.Transaction)]!),
            Configuration = (string?)param[nameof(ExecuteCommandLine.Configuration)],
            Log = (string?)param[nameof(ExecuteCommandLine.Log)],
            WhatIf = (bool)param[nameof(ExecuteCommandLine.WhatIf)]!
        };

        CommandLineTools.AppendFrom(commandLine.From, false, (string[]?)param[nameof(ExecuteCommandLine.From)]);
        CommandLineTools.AppendFrom(commandLine.From, true, (string[]?)param["FromSql"]);
        CommandLineTools.AppendVariables(commandLine.Variables, (string[]?)param[nameof(ExecuteCommandLine.Variables)]);

        Run(commandLine, currentDirectory, writeInfo, writeError);
    }

    public static void RunExport(IDictionary<string, object?> param, string currentDirectory, Action<string> writeInfo, Action<string> writeError)
    {
        var commandLine = new ExportCommandLine
        {
            Database = (string)param[nameof(ExportCommandLine.Database)]!,
            Configuration = (string?)param[nameof(ExportCommandLine.Configuration)],
            DestinationFileName = (string?)param[nameof(ExportCommandLine.DestinationFileName)],
            DestinationTableName = (string?)param[nameof(ExportCommandLine.DestinationTableName)],
            Log = (string?)param[nameof(ExecuteCommandLine.Log)]
        };

        CommandLineTools.AppendFrom(commandLine.From, false, (string[]?)param[nameof(ExportCommandLine.From)]);
        CommandLineTools.AppendFrom(commandLine.From, true, (string[]?)param["FromSql"]);
        CommandLineTools.AppendVariables(commandLine.Variables, (string[]?)param[nameof(ExportCommandLine.Variables)]);

        Run(commandLine, currentDirectory, writeInfo, writeError);
    }

    public static void RunUpdate(IDictionary<string, object?> param, string currentDirectory, Action<string> writeInfo, Action<string> writeError)
    {
        var commandLine = new UpgradeCommandLine
        {
            Database = (string)param[nameof(UpgradeCommandLine.Database)]!,
            Transaction = (TransactionMode)((int)param[nameof(UpgradeCommandLine.Transaction)]!),
            Configuration = (string?)param[nameof(UpgradeCommandLine.Configuration)],
            Log = (string?)param[nameof(UpgradeCommandLine.Log)],
            WhatIf = (bool)param[nameof(UpgradeCommandLine.WhatIf)]!,
            FolderAsModuleName = (bool)param[nameof(UpgradeCommandLine.FolderAsModuleName)]!
        };

        CommandLineTools.AppendFrom(commandLine.From, false, (string[]?)param[nameof(UpgradeCommandLine.From)]);
        CommandLineTools.AppendVariables(commandLine.Variables, (string[]?)param[nameof(UpgradeCommandLine.Variables)]);

        Run(commandLine, currentDirectory, writeInfo, writeError);
    }

    public static string GetDefaultConfigurationFile() => ConfigurationManager.GetDefaultConfigurationFile();

    private static void Run(ICommandLine commandLine, string currentDirectory, Action<string> writeInfo, Action<string> writeError)
    {
        var program = Program ?? new SqlDatabaseProgram(new CmdLetLogger(writeInfo, writeError), currentDirectory);
        program.ExecuteCommand(commandLine);
    }
}