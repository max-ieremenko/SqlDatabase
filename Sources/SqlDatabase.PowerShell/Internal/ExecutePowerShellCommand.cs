using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell.Internal
{
    internal sealed class ExecutePowerShellCommand : PowerShellCommandBase
    {
        public ExecutePowerShellCommand(ExecuteCmdLet cmdLet)
            : base(cmdLet)
        {
        }

        public new ExecuteCmdLet Cmdlet => (ExecuteCmdLet)base.Cmdlet;

        protected override void BuildCommandLine(GenericCommandLineBuilder builder)
        {
            builder
                .SetCommand(CommandLineFactory.CommandExecute)
                .SetConnection(Cmdlet.Database)
                .SetLogFileName(Cmdlet.RootPath(Cmdlet.Log));

            if (Cmdlet.Var != null)
            {
                for (var i = 0; i < Cmdlet.Var.Length; i++)
                {
                    builder.SetVariable(Cmdlet.Var[i]);
                }
            }

            Cmdlet.AppendFrom(Cmdlet.From, builder);

            if (Cmdlet.FromSql != null)
            {
                for (var i = 0; i < Cmdlet.FromSql.Length; i++)
                {
                    builder.SetInLineScript(Cmdlet.FromSql[i]);
                }
            }

            builder
                .SetTransaction((TransactionMode)Cmdlet.Transaction)
                .SetConfigurationFile(Cmdlet.RootPath(Cmdlet.Configuration))
                .SetWhatIf(Cmdlet.WhatIf);
        }
    }
}