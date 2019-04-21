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
    }
}