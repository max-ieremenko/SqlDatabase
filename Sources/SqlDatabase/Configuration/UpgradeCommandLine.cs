using SqlDatabase.Commands;
using SqlDatabase.IO;
using SqlDatabase.Scripts;

namespace SqlDatabase.Configuration
{
    internal sealed class UpgradeCommandLine : CommandLineBase
    {
        public override ICommand CreateCommand(ILogger logger)
        {
            var configuration = new ConfigurationManager();
            configuration.LoadFrom(ConfigurationFile);

            var sequence = new UpgradeScriptSequence
            {
                ScriptFactory = new ScriptFactory { Configuration = configuration.SqlDatabase }
            };

            foreach (var script in Scripts)
            {
                sequence.Sources.Add(FileSystemFactory.FileSystemInfoFromPath(script));
            }

            return new DatabaseUpgradeCommand
            {
                Log = logger,
                Database = CreateDatabase(logger, configuration),
                ScriptSequence = sequence
            };
        }
    }
}
