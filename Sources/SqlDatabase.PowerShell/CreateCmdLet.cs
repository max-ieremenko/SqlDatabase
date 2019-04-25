using System.Management.Automation;
using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell
{
    [Cmdlet(VerbsCommon.New, "SqlDatabase")]
    [Alias(CommandLineFactory.CommandCreate + "-SqlDatabase")]
    public sealed class CreateCmdLet : SqlDatabaseCmdLet
    {
        public CreateCmdLet()
            : base(CommandLineFactory.CommandCreate)
        {
        }

        [Parameter(Mandatory = true, Position = 2, ValueFromPipeline = true, HelpMessage = "A path to a folder or zip archive with sql scripts or path to a sql script file. Repeat -from to setup several sources.")]
        [Alias("f")]
        public string[] From { get; set; }

        [Parameter(Position = 3, HelpMessage = "A path to application configuration file. Default is current SqlDatabase.exe.config.")]
        [Alias("c")]
        public string Configuration { get; set; }

        internal override void BuildCommandLine(GenericCommandLineBuilder cmd)
        {
            if (From != null && From.Length > 0)
            {
                foreach (var from in From)
                {
                    cmd.SetScripts(from);
                }
            }

            cmd
                .SetConfigurationFile(Configuration);
        }
    }
}