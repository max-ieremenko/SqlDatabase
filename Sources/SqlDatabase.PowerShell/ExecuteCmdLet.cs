using System.Management.Automation;
using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell
{
    [Cmdlet(VerbsLifecycle.Invoke, "SqlDatabase")]
    [Alias(CommandLineFactory.CommandExecute + "-SqlDatabase")]
    public sealed class ExecuteCmdLet : SqlDatabaseCmdLet
    {
        public ExecuteCmdLet()
            : base(CommandLineFactory.CommandExecute)
        {
        }

        [Parameter(Position = 2, ValueFromPipeline = true, HelpMessage = "A path to a folder or zip archive with sql scripts or path to a sql script file. Repeat -from to setup several sources.")]
        [Alias("f")]
        public string[] From { get; set; }

        [Parameter(HelpMessage = "An sql script text. Repeat -fromSql to setup several scripts.")]
        [Alias("s")]
        public string[] FromSql { get; set; }

        [Parameter(Position = 3, HelpMessage = "Transaction mode. Possible values: none, perStep. Default is none.")]
        [Alias("t")]
        public TransactionMode Transaction { get; set; }

        [Parameter(Position = 4, HelpMessage = "A path to application configuration file. Default is current SqlDatabase.exe.config.")]
        [Alias("c")]
        public string Configuration { get; set; }

        [Parameter(Position = 5, HelpMessage = "Shows what would happen if the command runs. The command is not run.")]
        public SwitchParameter WhatIf { get; set; }

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
                .SetTransaction(Transaction)
                .SetConfigurationFile(this.RootPath(Configuration))
                .SetWhatIf(WhatIf);
        }
    }
}