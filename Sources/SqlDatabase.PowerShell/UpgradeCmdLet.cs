using System.Management.Automation;
using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell
{
    [Cmdlet(VerbsData.Update, "SqlDatabase")]
    [Alias(CommandLineFactory.CommandUpgrade + "-SqlDatabase")]
    public sealed class UpgradeCmdLet : SqlDatabaseCmdLet
    {
        public UpgradeCmdLet()
            : base(CommandLineFactory.CommandUpgrade)
        {
        }

        [Parameter(Mandatory = true, Position = 2, ValueFromPipeline = true, HelpMessage = "A path to a folder or zip archive with migration steps. Repeat -from to setup several sources.")]
        [Alias("f")]
        public string[] From { get; set; }

        [Parameter(Position = 3, HelpMessage = "Transaction mode. Possible values: none, perStep. Default is none.")]
        [Alias("t")]
        public TransactionMode Transaction { get; set; }

        [Parameter(Position = 4, HelpMessage = "A path to application configuration file. Default is current SqlDatabase.exe.config.")]
        [Alias("c")]
        public string Configuration { get; set; }

        [Parameter(Position = 5, HelpMessage = "Shows what would happen if the command runs. The command is not run.")]
        public SwitchParameter WhatIf { get; set; }

        [Parameter(Position = 6)]
        public SwitchParameter FolderAsModuleName { get; set; }

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
                .SetConfigurationFile(Configuration)
                .SetTransaction(Transaction)
                .SetWhatIf(WhatIf)
                .SetFolderAsModuleName(FolderAsModuleName);
        }
    }
}