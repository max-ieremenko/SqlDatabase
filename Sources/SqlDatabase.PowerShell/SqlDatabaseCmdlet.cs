using System.Management.Automation;
using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell
{
    public abstract class SqlDatabaseCmdLet : PSCmdlet
    {
        private readonly Command _command;

        protected SqlDatabaseCmdLet(Command command)
        {
            _command = command;
        }

        [Parameter(Mandatory = true, Position = 1, HelpMessage = "Connection string to target database.")]
        [Alias("d")]
        public string Database { get; set; }

        [Parameter(Mandatory = true, Position = 2, ValueFromPipeline = true, HelpMessage = "Scripts file.")]
        [Alias("f")]
        public string[] From { get; set; }

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

        protected override void ProcessRecord()
        {
            var cmd = new CommandLineBuilder()
                .SetCommand(_command)
                .SetConnection(Database)
                .SetTransaction(Transaction)
                .SetConfigurationFile(Configuration);

            foreach (var from in From)
            {
                cmd.SetScripts(from);
            }

            if (Var != null && Var.Length > 0)
            {
                foreach (var value in Var)
                {
                    cmd.SetVariable(value);
                }
            }

            ResolveProgram().ExecuteCommand(cmd.Build());
        }

        private ISqlDatabaseProgram ResolveProgram()
        {
            return Program ?? new SqlDatabaseProgram(this);
        }
    }
}