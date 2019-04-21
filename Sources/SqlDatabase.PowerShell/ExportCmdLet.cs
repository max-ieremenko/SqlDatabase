using System.Management.Automation;
using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell
{
    [Cmdlet(VerbsData.Export, "SqlDatabase")]
    public sealed class ExportCmdLet : SqlDatabaseCmdLet
    {
        public ExportCmdLet()
            : base(CommandLineFactory.CommandExport)
        {
        }

        [Parameter(HelpMessage = "Generate INSERT INTO toTable")]
        public string ToTable { get; set; }

        internal override void BuildCommandLine(GenericCommandLineBuilder cmd)
        {
            cmd.SetExportToTable(ToTable);
        }
    }
}