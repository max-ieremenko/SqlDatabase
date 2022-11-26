using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell.Internal;

internal sealed class UpgradePowerShellCommand : PowerShellCommandBase
{
    public UpgradePowerShellCommand(UpgradeCmdLet cmdLet)
        : base(cmdLet)
    {
    }

    public new UpgradeCmdLet Cmdlet => (UpgradeCmdLet)base.Cmdlet;

    protected override void BuildCommandLine(GenericCommandLineBuilder builder)
    {
        builder
            .SetCommand(CommandLineFactory.CommandUpgrade)
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
            .SetTransaction((TransactionMode)Cmdlet.Transaction)
            .SetWhatIf(Cmdlet.WhatIf)
            .SetFolderAsModuleName(Cmdlet.FolderAsModuleName);
    }
}