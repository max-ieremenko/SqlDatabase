namespace SqlDatabase
{
    internal sealed class SequentialCreate
    {
        public ICreateDatabase Database { get; set; }

        public ICreateScriptSequence ScriptSequence { get; set; }

        public ILogger Log { get; set; }

        public void Execute()
        {
            var sequences = ScriptSequence.BuildSequence();
            if (sequences.Count == 0)
            {
                Log.Error("scripts to create database not found.");
                return;
            }

            Database.BeforeCreate();

            foreach (var script in sequences)
            {
                Log.Info("execute {0}".FormatWith(script.DisplayName));
                Database.Execute(script);
            }
        }
    }
}
