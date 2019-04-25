using System.Management.Automation;
using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell
{
    public abstract class SqlDatabaseCmdLet : PSCmdlet
    {
        private readonly string _command;

        protected SqlDatabaseCmdLet(string command)
        {
            _command = command;
        }

        [Parameter(Mandatory = true, Position = 1, HelpMessage = "Connection string to target database.")]
        [Alias("d")]
        public string Database { get; set; }

        [Parameter(ValueFromRemainingArguments = true, HelpMessage = "Set a variable in format \"[name of variable]=[value of variable]\".")]
        [Alias("v")]
        public string[] Var { get; set; }

        // only for tests
        internal static ISqlDatabaseProgram Program { get; set; }

        internal virtual void BuildCommandLine(GenericCommandLineBuilder cmd)
        {
        }

        protected sealed override void ProcessRecord()
        {
            var cmd = new GenericCommandLineBuilder()
                .SetCommand(_command)
                .SetConnection(Database);

            if (Var != null && Var.Length > 0)
            {
                foreach (var value in Var)
                {
                    cmd.SetVariable(value);
                }
            }

            BuildCommandLine(cmd);
            ResolveProgram().ExecuteCommand(cmd.Build());
        }

        private ISqlDatabaseProgram ResolveProgram()
        {
            return Program ?? new SqlDatabaseProgram(new PowerShellCmdlet(this));
        }
    }
}