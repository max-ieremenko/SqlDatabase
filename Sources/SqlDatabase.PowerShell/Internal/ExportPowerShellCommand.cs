using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell.Internal;

internal sealed class ExportPowerShellCommand : PowerShellCommandBase
{
    public ExportPowerShellCommand(ExportCmdLet cmdLet)
        : base(cmdLet)
    {
    }

    public new ExportCmdLet Cmdlet => (ExportCmdLet)base.Cmdlet;

    protected override void BuildCommandLine(GenericCommandLineBuilder builder)
    {
        builder
            .SetCommand(CommandLineFactory.CommandExport)
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
            .SetConfigurationFile(Cmdlet.RootPath(Cmdlet.Configuration))
            .SetExportToTable(Cmdlet.ToTable)
            .SetExportToFile(Cmdlet.ToFile);
    }
}