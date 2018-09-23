using System.IO;
using System.Management.Automation;
using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell
{
    [Cmdlet(VerbsLifecycle.Invoke, "SqlDatabase")]
    public sealed class ExecuteCmdLet : Cmdlet
    {
        private CommandLineBuilder _commandLineBuilder;

        [Parameter(Mandatory = true, Position = 1, HelpMessage = "Connection string to target database.")]
        [Alias("d")]
        public string Database { get; set; }

        [Parameter(Mandatory = true, Position = 2, ValueFromPipeline = true, HelpMessage = "Scripts file.")]
        [Alias("f")]
        public FileInfo From { get; set; }

        [Parameter(Position = 3, HelpMessage = "Transaction mode. Possible values: none, perStep. Default is none.")]
        [Alias("t")]
        public TransactionMode Transaction { get; set; }

        [Parameter(ValueFromRemainingArguments = true, HelpMessage = "set a variable in format \"[name of variable]=[value of variable]\".")]
        [Alias("v")]
        public string[] Var { get; set; }

        protected override void BeginProcessing()
        {
            _commandLineBuilder = new CommandLineBuilder()
                .SetCommand(Command.Execute)
                .SetConnection(Database)
                .SetTransaction(Transaction);

            if (Var != null && Var.Length > 0)
            {
                foreach (var value in Var)
                {
                    _commandLineBuilder.SetVariable(value);
                }
            }
        }

        protected override void ProcessRecord()
        {
            var cmd = _commandLineBuilder
                .SetScripts(From.FullName)
                .Build();

            Program.ExecuteCommand(cmd);
        }
    }
}