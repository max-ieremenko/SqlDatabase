using SqlDatabase.Scripts;

namespace SqlDatabase
{
    internal sealed class SequentialUpgrade
    {
        public IDatabase Database { get; set; }

        public IScriptSequence ScriptSequence { get; set; }

        public ILogger Log { get; set; }

        public void Execute()
        {
            Log.Info("get database version");
            var version = Database.GetCurrentVersion();
            Log.Info("current database version is {0}".FormatWith(version));

            var sequences = ScriptSequence.BuildSequence(version);
            if (sequences.Count == 0)
            {
                Log.Info("the database is uptodate.");
                return;
            }

            Database.BeforeUpgrade();

            foreach (var step in sequences)
            {
                Log.Info("execute {0}".FormatWith(step.Script.DisplayName));
                Database.ExecuteUpgrade(step.Script, step.From, step.To);
            }
        }
    }
}