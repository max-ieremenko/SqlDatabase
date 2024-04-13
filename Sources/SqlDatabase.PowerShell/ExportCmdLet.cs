using System.Management.Automation;
using SqlDatabase.CommandLine;
using SqlDatabase.PowerShell.Internal;

namespace SqlDatabase.PowerShell;

[Cmdlet(VerbsData.Export, "SqlDatabase")]
public sealed class ExportCmdLet : PSCmdlet
{
    [Parameter(Mandatory = true, Position = 1, HelpMessage = "Connection string to target database.")]
    [Alias("d")]
    public string Database { get; set; } = null!;

    [Parameter(Position = 2, HelpMessage = "An sql script to select export data. Repeat -fromSql to setup several scripts.")]
    [Alias("s")]
    public string[]? FromSql { get; set; }

    [Parameter(HelpMessage = "A path to a folder or zip archive with sql scripts or path to a sql script file. Repeat -from to setup several sources.")]
    [Alias("f")]
    public string[]? From { get; set; }

    [Parameter(Position = 3, HelpMessage = "Write sql scripts into a file. By default write into information stream.")]
    public string? ToFile { get; set; }

    [Parameter(Position = 4, HelpMessage = "A path to application configuration file. Default is current SqlDatabase.exe.config.")]
    [Alias("c")]
    public string? Configuration { get; set; }

    [Parameter(HelpMessage = "Setup \"INSERT INTO\" table name. Default is dbo.SqlDatabaseExport.")]
    public string? ToTable { get; set; }

    [Parameter(ValueFromRemainingArguments = true, HelpMessage = "Set a variable in format \"[name of variable]=[value of variable]\".")]
    [Alias("v")]
    public string[]? Var { get; set; }

    [Parameter(HelpMessage = "Optional path to log file.")]
    public string? Log { get; set; }

    protected override void ProcessRecord()
    {
        var commandLine = new ExportCommandLine
        {
            Database = Database,
            Configuration = Configuration,
            DestinationFileName = ToFile,
            DestinationTableName = ToTable,
            Log = Log
        };

        CommandLineTools.AppendFrom(commandLine.From, false, From);
        CommandLineTools.AppendFrom(commandLine.From, true, FromSql);
        CommandLineTools.AppendVariables(commandLine.Variables, Var);

        PowerShellCommand.Execute(this, commandLine);
    }
}