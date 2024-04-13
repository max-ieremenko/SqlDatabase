using System.Management.Automation;
using SqlDatabase.Adapter;
using SqlDatabase.CommandLine;
using SqlDatabase.PowerShell.Internal;

namespace SqlDatabase.PowerShell;

[Cmdlet(VerbsLifecycle.Invoke, "SqlDatabase")]
[Alias("Execute-SqlDatabase")]
public sealed class ExecuteCmdLet : PSCmdlet
{
    [Parameter(Mandatory = true, Position = 1, HelpMessage = "Connection string to target database.")]
    [Alias("d")]
    public string Database { get; set; } = null!;

    [Parameter(Position = 2, ValueFromPipeline = true, HelpMessage = "A path to a folder or zip archive with sql scripts or path to a sql script file. Repeat -from to setup several sources.")]
    [Alias("f")]
    public string[]? From { get; set; }

    [Parameter(HelpMessage = "An sql script text. Repeat -fromSql to setup several scripts.")]
    [Alias("s")]
    public string[]? FromSql { get; set; }

    [Parameter(Position = 3, HelpMessage = "Transaction mode. Possible values: none, perStep. Default is none.")]
    [Alias("t")]
    public PSTransactionMode Transaction { get; set; }

    [Parameter(Position = 4, HelpMessage = "A path to application configuration file. Default is current SqlDatabase.exe.config.")]
    [Alias("c")]
    public string? Configuration { get; set; }

    [Parameter(Position = 5, HelpMessage = "Shows what would happen if the command runs. The command is not run.")]
    public SwitchParameter WhatIf { get; set; }

    [Parameter(ValueFromRemainingArguments = true, HelpMessage = "Set a variable in format \"[name of variable]=[value of variable]\".")]
    [Alias("v")]
    public string[]? Var { get; set; }

    [Parameter(HelpMessage = "Optional path to log file.")]
    public string? Log { get; set; }

    protected override void ProcessRecord()
    {
        var commandLine = new ExecuteCommandLine
        {
            Database = Database,
            Transaction = (TransactionMode)Transaction,
            Configuration = Configuration,
            Log = Log,
            WhatIf = WhatIf
        };

        CommandLineTools.AppendFrom(commandLine.From, false, From);
        CommandLineTools.AppendFrom(commandLine.From, true, FromSql);
        CommandLineTools.AppendVariables(commandLine.Variables, Var);

        PowerShellCommand.Execute(this, commandLine);
    }
}