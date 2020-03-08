using System;
using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell
{
    internal sealed class SqlDatabaseProgramNetCore : ISqlDatabaseProgram
    {
        private readonly ICmdlet _owner;

        public SqlDatabaseProgramNetCore(ICmdlet owner)
        {
            _owner = owner;
        }

        public void ExecuteCommand(GenericCommandLine command)
        {
            var logger = new CmdLetLogger(_owner);
            var args = new GenericCommandLineBuilder(command).BuildArray(false);

            try
            {
                Program.Run(logger, args);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
