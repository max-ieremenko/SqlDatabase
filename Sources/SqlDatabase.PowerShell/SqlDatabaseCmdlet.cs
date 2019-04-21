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

        [Parameter(Position = 2, ValueFromPipeline = true, HelpMessage = "A path to a folder or zip archive with sql scripts or path to a file.")]
        [Alias("f")]
        public string[] From { get; set; }

        [Parameter(HelpMessage = "Sql script text.")]
        [Alias("s")]
        public string[] FromSql { get; set; }

        [Parameter(Position = 3, HelpMessage = "Transaction mode. Possible values: none, perStep. Default is none.")]
        [Alias("t")]
        public TransactionMode Transaction { get; set; }

        [Parameter(Position = 4, HelpMessage = "Path to application configuration file. Default is current SqlDatabase.exe.config.")]
        [Alias("c")]
        public string Configuration { get; set; }

        [Parameter(ValueFromRemainingArguments = true, HelpMessage = "set a variable in format \"[name of variable]=[value of variable]\".")]
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
                .SetConnection(Database)
                .SetTransaction(Transaction)
                .SetConfigurationFile(Configuration);

            if (From != null && From.Length > 0)
            {
                foreach (var from in From)
                {
                    cmd.SetScripts(from);
                }
            }

            if (FromSql != null && FromSql.Length > 0)
            {
                foreach (var from in FromSql)
                {
                    cmd.SetInLineScript(from);
                }
            }

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