using SqlDatabase.Commands;
using SqlDatabase.IO;
using SqlDatabase.Scripts;

namespace SqlDatabase.Configuration
{
    internal sealed class ExecuteCommandLine : CommandLineBase
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

            return new DatabaseExecuteCommand
            {
                Log = logger,
                Database = CreateDatabase(logger, configuration),
                ScriptSequence = sequence
            };
        }
    }
}
