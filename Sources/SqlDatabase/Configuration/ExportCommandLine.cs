using System;
using System.IO;
using System.Linq;
using SqlDatabase.Commands;
using SqlDatabase.Export;
using SqlDatabase.Scripts;

namespace SqlDatabase.Configuration
{
    internal sealed class ExportCommandLine : CommandLineBase
    {
        public string DestinationTableName { get; set; }

        public string DestinationFileName { get; set; }

        public override ICommand CreateCommand(ILogger logger)
        {
            var configuration = new ConfigurationManager();
            configuration.LoadFrom(ConfigurationFile);

            var sequence = new CreateScriptSequence
            {
                ScriptFactory = new ScriptFactory { Configuration = configuration.SqlDatabase },
                Sources = Scripts.ToArray()
            };

            return new DatabaseExportCommand
            {
                Log = WrapLogger(logger),
                OpenOutput = CreateOutput(),
                Database = CreateDatabase(logger, configuration, TransactionMode.None, false),
                ScriptSequence = sequence,
                DestinationTableName = DestinationTableName
            };
        }

        internal Func<TextWriter> CreateOutput()
        {
            var fileName = DestinationFileName;

            if (string.IsNullOrEmpty(fileName))
            {
                return () => Console.Out;
            }

            return () => new StreamWriter(fileName, false);
        }

        internal ILogger WrapLogger(ILogger logger)
        {
            return string.IsNullOrEmpty(DestinationFileName) ? new DataExportLogger(logger) : logger;
        }

        protected override bool ParseArg(Arg arg)
        {
            if (Arg.ExportToTable.Equals(arg.Key, StringComparison.OrdinalIgnoreCase))
            {
                DestinationTableName = arg.Value;
                return true;
            }

            if (Arg.ExportToFile.Equals(arg.Key, StringComparison.OrdinalIgnoreCase))
            {
                DestinationFileName = arg.Value;
                return true;
            }

            if (Arg.InLineScript.Equals(arg.Key, StringComparison.OrdinalIgnoreCase))
            {
                SetInLineScript(arg.Value);
                return true;
            }

            return false;
        }
    }
}
