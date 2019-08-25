using System;
using System.Linq;
using SqlDatabase.Commands;
using SqlDatabase.Scripts;
using SqlDatabase.Scripts.UpgradeInternal;

namespace SqlDatabase.Configuration
{
    internal sealed class UpgradeCommandLine : CommandLineBase
    {
        public TransactionMode Transaction { get; set; }

        public bool WhatIf { get; set; }

        public override ICommand CreateCommand(ILogger logger)
        {
            var configuration = new ConfigurationManager();
            configuration.LoadFrom(ConfigurationFile);

            var database = CreateDatabase(logger, configuration, Transaction, WhatIf);

            var sequence = new UpgradeScriptSequence
            {
                ScriptFactory = new ScriptFactory { Configuration = configuration.SqlDatabase },
                VersionResolver = new ModuleVersionResolver { Database = database, Log = logger },
                Sources = Scripts.ToArray(),
                Log = logger,
                WhatIf = WhatIf
            };

            return new DatabaseUpgradeCommand
            {
                Log = logger,
                Database = database,
                ScriptSequence = sequence
            };
        }

        protected override bool ParseArg(Arg arg)
        {
            if (Arg.Transaction.Equals(arg.Key, StringComparison.OrdinalIgnoreCase))
            {
                SetTransaction(arg.Value);
                return true;
            }

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
