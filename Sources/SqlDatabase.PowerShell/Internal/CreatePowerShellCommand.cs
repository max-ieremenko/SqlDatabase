using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell.Internal
{
    internal sealed class CreatePowerShellCommand : PowerShellCommandBase
    {
        public CreatePowerShellCommand(CreateCmdLet cmdlet)
            : base(cmdlet)
        {
        }

        public new CreateCmdLet Cmdlet => (CreateCmdLet)base.Cmdlet;

        protected override void BuildCommandLine(GenericCommandLineBuilder builder)
        {
            builder
                .SetCommand(CommandLineFactory.CommandCreate)
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

            builder
                .SetConfigurationFile(Cmdlet.RootPath(Cmdlet.Configuration))
                .SetWhatIf(Cmdlet.WhatIf);
        }
    }
}