using System.Configuration;
using SqlDatabase.Scripts;

namespace SqlDatabase.Commands
{
    internal sealed class DatabaseCreateCommand : DatabaseCommandBase
    {
        public ICreateScriptSequence ScriptSequence { get; set; }

        protected override void Greet(string databaseLocation)
        {
            Log.Info("Create {0}".FormatWith(databaseLocation));
        }

        protected override void ExecuteCore()
        {
            var sequences = ScriptSequence.BuildSequence();
            if (sequences.Count == 0)
            {
                throw new ConfigurationErrorsException("scripts to create database not found.");
            }

            foreach (var script in sequences)
            {
                Log.Info("execute {0}".FormatWith(script.DisplayName));
                Database.Execute(script);
            }
        }
    }
}