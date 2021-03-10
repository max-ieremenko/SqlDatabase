using System.Configuration;
using System.Diagnostics;
using SqlDatabase.Scripts;

namespace SqlDatabase.Commands
{
    internal sealed class DatabaseCreateCommand : DatabaseCommandBase
    {
        public ICreateScriptSequence ScriptSequence { get; set; }

        public IPowerShellFactory PowerShellFactory { get; set; }

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

            PowerShellFactory.InitializeIfRequested(Log);

            foreach (var script in sequences)
            {
                var timer = Stopwatch.StartNew();
                Log.Info("execute {0} ...".FormatWith(script.DisplayName));

                using (Log.Indent())
                {
                    Database.Execute(script);
                }

                Log.Info("done in {0}".FormatWith(timer.Elapsed));
            }
        }
    }
}