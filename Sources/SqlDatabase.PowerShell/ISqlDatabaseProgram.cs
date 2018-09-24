using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell
{
    internal interface ISqlDatabaseProgram
    {
        void ExecuteCommand(CommandLine command);
    }
}