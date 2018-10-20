using SqlDatabase.Scripts;

namespace SqlDatabase.Commands
{
    internal sealed class DatabaseExecuteCommand : DatabaseCommandBase
    {
        public ICreateScriptSequence ScriptSequence { get; set; }

        protected override void Greet(string databaseLocation)
        {
            Log.Info("Execute script on {0}".FormatWith(databaseLocation));
        }

        protected override void ExecuteCore()
        {
            var sequences = ScriptSequence.BuildSequence();

            foreach (var script in sequences)
            {
                Log.Info("execute {0}".FormatWith(script.DisplayName));
                Database.Execute(script);
            }
        }
    }
}