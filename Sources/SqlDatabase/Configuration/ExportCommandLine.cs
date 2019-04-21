using System;
using System.Linq;
using SqlDatabase.Commands;
using SqlDatabase.Export;
using SqlDatabase.Scripts;

namespace SqlDatabase.Configuration
{
    internal sealed class ExportCommandLine : CommandLineBase
    {
        public string DestinationTableName { get; set; }

        public override ICommand CreateCommand(ILogger logger)
        {
            var configuration = new ConfigurationManager();
            configuration.LoadFrom(ConfigurationFile);

            var sequence = new CreateScriptSequence
            {
                ScriptFactory = new ScriptFactory { Configuration = configuration.SqlDatabase },
                Sources = Scripts.ToArray()
            };

            logger = new DataExportLogger(logger);

            return new DatabaseExportCommand
            {
                Log = logger,
                OpenOutput = () => Console.Out,
                Database = CreateDatabase(logger, configuration),
                ScriptSequence = sequence,
                DestinationTableName = DestinationTableName
            };
        }

        protected internal override void Validate()
        {
            if (Transaction != TransactionMode.None)
            {
                throw new NotSupportedException("Transaction mode is not supported.");
            }
        }

        protected override bool ParseArg(Arg arg)
        {
            if (Arg.ExportToTable.Equals(arg.Key, StringComparison.OrdinalIgnoreCase))
            {
                DestinationTableName = arg.Value;
                return true;
            }

            return false;
        }
    }
}
