using System.Linq;
using SqlDatabase.Commands;
using SqlDatabase.Scripts;
using SqlDatabase.Scripts.PowerShellInternal;

namespace SqlDatabase.Configuration
{
    internal sealed class CreateCommandLine : CommandLineBase
    {
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

            return new DatabaseCreateCommand
            {
                Log = logger,
                Database = CreateDatabase(logger, configuration, TransactionMode.None, WhatIf),
                ScriptSequence = sequence,
                PowerShellFactory = powerShellFactory
            };
        }

        protected override bool ParseArg(Arg arg)
        {
#if NETCOREAPP || NET5_0
            if (Arg.UsePowerShell.Equals(arg.Key, System.StringComparison.OrdinalIgnoreCase))
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
    }
}
