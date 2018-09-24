using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell
{
    internal sealed class SqlDatabaseProgram : ISqlDatabaseProgram
    {
        public void ExecuteCommand(CommandLine command)
        {
            Program.ExecuteCommand(command);
        }
    }
}
