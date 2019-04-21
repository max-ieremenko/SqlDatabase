using System;
using SqlDatabase.Commands;
using SqlDatabase.Export;
using SqlDatabase.IO;
using SqlDatabase.Scripts;

namespace SqlDatabase.Configuration
{
    internal sealed class ExportCommandLine : CommandLineBase
    {
        public override ICommand CreateCommand(ILogger logger)
        {
            var configuration = new ConfigurationManager();
            configuration.LoadFrom(ConfigurationFile);

            var sequence = new CreateScriptSequence
            {
                ScriptFactory = new ScriptFactory { Configuration = configuration.SqlDatabase }
            };

            foreach (var script in Scripts)
            {
                sequence.Sources.Add(FileSystemFactory.FileSystemInfoFromPath(script));
            }

            logger = new DataExportLogger(logger);

            return new DatabaseExportCommand
            {
                Log = logger,
                OpenOutput = () => Console.Out,
                Database = CreateDatabase(logger, configuration),
                ScriptSequence = sequence
            };
        }

        protected internal override void Validate()
        {
            if (Transaction != TransactionMode.None)
            {
                throw new NotSupportedException("Transaction mode is not supported.");
            }
        }
    }
}
