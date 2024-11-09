using SqlDatabase.CommandLine;

namespace SqlDatabase.PowerShell.Internal;

internal interface ISqlDatabaseProgram
{
    void ExecuteCommand(ICommandLine command);
}