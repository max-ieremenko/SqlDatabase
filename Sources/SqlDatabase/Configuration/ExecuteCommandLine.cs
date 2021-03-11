using System;
using System.Linq;
using SqlDatabase.Commands;
using SqlDatabase.Scripts;
using SqlDatabase.Scripts.PowerShellInternal;

namespace SqlDatabase.Configuration
{
    internal sealed class ExecuteCommandLine : CommandLineBase
    {
        public TransactionMode Transaction { get; set; }

        public string UsePowerShell { get; set; }

        public bool WhatIf { get; set; }

        public override ICommand CreateCommand(ILogger logger)
        {
            var configuration = new ConfigurationManager();
            configuration.LoadFrom(ConfigurationFile);

            var powerShellFactory = PowerShellFactory.Create(UsePowerShell);

            var sequence = new CreateScriptSequence
            {
                ScriptFactory = new ScriptFactory
                {
                    Configuration = configuration.SqlDatabase,
                    PowerShellFactory = powerShellFactory
                },
                Sources = Scripts.ToArray()
            };

            return new DatabaseExecuteCommand
            {
                Log = logger,
                Database = CreateDatabase(logger, configuration, Transaction, WhatIf),
                ScriptSequence = sequence,
                PowerShellFactory = powerShellFactory
            };
        }

        protected override bool ParseArg(Arg arg)
        {
            if (Arg.Transaction.Equals(arg.Key, StringComparison.OrdinalIgnoreCase))
            {
                SetTransaction(arg.Value);
                return true;
            }

            if (Arg.InLineScript.Equals(arg.Key, StringComparison.OrdinalIgnoreCase))
            {
                SetInLineScript(arg.Value);
                return true;
            }

#if NETCOREAPP || NET5_0
            if (Arg.UsePowerShell.Equals(arg.Key, StringComparison.OrdinalIgnoreCase))
            {
                UsePowerShell = arg.Value;
                return true;
            }
#endif
            if (TryParseWhatIf(arg, out var value))
            {
                WhatIf = value;
                return true;
            }

            return false;
        }

        private void SetTransaction(string modeName)
        {
            if (!Enum.TryParse<TransactionMode>(modeName, true, out var mode))
            {
                throw new InvalidCommandLineException(Arg.Transaction, "Unknown transaction mode [{0}].".FormatWith(modeName));
            }

            Transaction = mode;
        }
    }
}
