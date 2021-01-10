using System;
using System.Collections;
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

        internal static void AppendDefaultConfiguration(GenericCommandLine command)
        {
            if (string.IsNullOrEmpty(command.ConfigurationFile))
            {
                command.ConfigurationFile = ConfigurationManager.ResolveDefaultConfigurationFile();
            }
        }

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
                for (var i = 0; i < Var.Length; i++)
                {
                    cmd.SetVariable(Var[i]);
                }
            }

            BuildCommandLine(cmd);

            var command = cmd.Build();
            AppendDefaultConfiguration(command);

            ResolveProgram().ExecuteCommand(command);
        }

        private ISqlDatabaseProgram ResolveProgram()
        {
            if (Program != null)
            {
                return Program;
            }

            if (!this.TryGetPSVersionTable(out var psVersionTable))
            {
                throw new PlatformNotSupportedException("$PSVersionTable is not defined.");
            }

            return SqlDatabaseProgramFactory.CreateProgram(psVersionTable, new PowerShellCmdlet(this));
        }
    }
}