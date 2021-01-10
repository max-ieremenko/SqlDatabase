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

        [Parameter(Position = 2, HelpMessage = "An sql script to select export data. Repeat -fromSql to setup several scripts.")]
        [Alias("s")]
        public string[] FromSql { get; set; }

        [Parameter(HelpMessage = "A path to a folder or zip archive with sql scripts or path to a sql script file. Repeat -from to setup several sources.")]
        [Alias("f")]
        public string[] From { get; set; }

        [Parameter(Position = 3, HelpMessage = "Write sql scripts into a file. By default write into information stream.")]
        public string ToFile { get; set; }

        [Parameter(Position = 4, HelpMessage = "A path to application configuration file. Default is current SqlDatabase.exe.config.")]
        [Alias("c")]
        public string Configuration { get; set; }

        [Parameter(HelpMessage = "Setup \"INSERT INTO\" table name. Default is dbo.SqlDatabaseExport.")]
        public string ToTable { get; set; }

        internal override void BuildCommandLine(GenericCommandLineBuilder cmd)
        {
            this.AppendFrom(From, cmd);

            if (FromSql != null && FromSql.Length > 0)
            {
                foreach (var from in FromSql)
                {
                    cmd.SetInLineScript(from);
                }
            }

            cmd
                .SetConfigurationFile(this.RootPath(Configuration))
                .SetExportToTable(ToTable)
                .SetExportToFile(ToFile);
        }
    }
}