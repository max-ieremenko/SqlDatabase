using System.Management.Automation;
using SqlDatabase.CommandLine;
using SqlDatabase.PowerShell.Internal;

namespace SqlDatabase.PowerShell;

[Cmdlet(VerbsCommon.New, "SqlDatabase")]
[Alias("Create-SqlDatabase")]
public sealed class CreateCmdLet : PSCmdlet
{
    [Parameter(Mandatory = true, Position = 1, HelpMessage = "Connection string to target database.")]
    [Alias("d")]
    public string Database { get; set; } = null!;

    [Parameter(Mandatory = true, Position = 2, ValueFromPipeline = true, HelpMessage = "A path to a folder or zip archive with sql scripts or path to a sql script file. Repeat -from to setup several sources.")]
    [Alias("f")]
    public string[] From { get; set; } = null!;

    [Parameter(Position = 3, HelpMessage = "A path to application configuration file. Default is current SqlDatabase.exe.config.")]
    [Alias("c")]
    public string? Configuration { get; set; }

    [Parameter(Position = 4, HelpMessage = "Shows what would happen if the command runs. The command is not run.")]
    public SwitchParameter WhatIf { get; set; }

    [Parameter(ValueFromRemainingArguments = true, HelpMessage = "Set a variable in format \"[name of variable]=[value of variable]\".")]
    [Alias("v")]
    public string[]? Var { get; set; }

    [Parameter(HelpMessage = "Optional path to log file.")]
    public string? Log { get; set; }

    protected override void ProcessRecord()
    {
        var param = new Dictionary<string, object?>
        {
            { nameof(CreateCommandLine.Database), Database },
            { nameof(CreateCommandLine.Configuration), Configuration },
            { nameof(CreateCommandLine.Log), Log },
            { nameof(CreateCommandLine.WhatIf), (bool)WhatIf },
            { nameof(CreateCommandLine.From), From },
            { nameof(CreateCommandLine.Variables), Var }
        };
        PowerShellCommand.Execute(this, nameof(CmdLetExecutor.RunCreate), param);
    }
}