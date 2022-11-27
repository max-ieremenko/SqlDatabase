using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell.Internal;

internal interface ISqlDatabaseProgram
{
    void ExecuteCommand(GenericCommandLine command);
}