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
            var database = CreateDatabase(logger, configuration, TransactionMode.None, WhatIf);

            var sequence = new CreateScriptSequence
            {
                ScriptFactory = new ScriptFactory
                {
                    AssemblyScriptConfiguration = configuration.SqlDatabase.AssemblyScript,
                    PowerShellFactory = powerShellFactory,
                    TextReader = database.Adapter.CreateSqlTextReader()
                },
                Sources = Scripts.ToArray()
            };

            return new DatabaseCreateCommand
            {
                Log = logger,
                Database = database,
                ScriptSequence = sequence,
                PowerShellFactory = powerShellFactory
            };
        }

        protected override bool ParseArg(Arg arg)
        {
#if NETCOREAPP || NET5_0_OR_GREATER
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
