using System.Linq;
using SqlDatabase.Commands;
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
                ScriptFactory = new ScriptFactory { Configuration = configuration.SqlDatabase },
                Sources = Scripts.ToArray()
            };

            return new DatabaseUpgradeCommand
            {
                Log = logger,
                Database = CreateDatabase(logger, configuration),
                ScriptSequence = sequence
            };
        }
    }
}
