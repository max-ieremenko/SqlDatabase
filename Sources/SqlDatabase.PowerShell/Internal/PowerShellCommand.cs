using System.Management.Automation;
using SqlDatabase.CommandLine;

namespace SqlDatabase.PowerShell.Internal;

internal static class PowerShellCommand
{
    // only for tests
    internal static ISqlDatabaseProgram? Program { get; set; }

    public static void Execute(PSCmdlet cmdlet, ICommandLine command)
    {
        using (var resolver = DependencyResolverFactory.Create(cmdlet))
        {
            resolver.Initialize();

            ResolveProgram(cmdlet).ExecuteCommand(command);
        }
    }

    private static ISqlDatabaseProgram ResolveProgram(PSCmdlet cmdlet)
    {
        if (Program != null)
        {
            return Program;
        }

        return new SqlDatabaseProgram(new CmdLetLogger(cmdlet), cmdlet.GetWorkingDirectory());
    }
}