using System.Linq;
using SqlDatabase.Commands;
using SqlDatabase.Scripts;

namespace SqlDatabase.Configuration
{
    internal sealed class CreateCommandLine : CommandLineBase
    {
        public bool WhatIf { get; set; }

        public override ICommand CreateCommand(ILogger logger)
        {
            var configuration = new ConfigurationManager();
            configuration.LoadFrom(ConfigurationFile);

            var sequence = new CreateScriptSequence
            {
                ScriptFactory = new ScriptFactory { Configuration = configuration.SqlDatabase },
                Sources = Scripts.ToArray()
            };

            return new DatabaseCreateCommand
            {
                Log = logger,
                Database = CreateDatabase(logger, configuration, TransactionMode.None, WhatIf),
                ScriptSequence = sequence
            };
        }

        protected override bool ParseArg(Arg arg)
        {
            if (TryParseWhatIf(arg, out var value))
            {
                WhatIf = value;
                return true;
            }

            return false;
        }
    }
}
