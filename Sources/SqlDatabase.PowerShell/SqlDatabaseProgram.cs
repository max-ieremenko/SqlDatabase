using System.Management.Automation;
using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell
{
    internal sealed class SqlDatabaseProgram : ISqlDatabaseProgram
    {
        private readonly PSCmdlet _owner;

        public SqlDatabaseProgram(PSCmdlet owner)
        {
            _owner = owner;
        }

        public void ExecuteCommand(CommandLine command)
        {
            var logger = new CmdLetLogger(_owner);
            Program.ExecuteCommand(command, logger);
        }
    }
}
