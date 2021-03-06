﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SqlDatabase.Scripts;

namespace SqlDatabase.Commands
{
    internal sealed class DatabaseUpgradeCommand : DatabaseCommandBase
    {
        public IUpgradeScriptSequence ScriptSequence { get; set; }

        public IPowerShellFactory PowerShellFactory { get; set; }

        protected override void Greet(string databaseLocation)
        {
            Log.Info("Upgrade {0}".FormatWith(databaseLocation));
        }

        protected override void ExecuteCore()
        {
            var sequence = ScriptSequence.BuildSequence();
            if (sequence.Count == 0)
            {
                Log.Info("the database is up-to-date.");
                return;
            }

            if (sequence.Any(i => string.IsNullOrEmpty(i.ModuleName)))
            {
                ShowMigrationSequenceShort(sequence);
            }
            else
            {
                ShowMigrationSequenceFull(sequence);
            }

            PowerShellFactory.InitializeIfRequested(Log);

            foreach (var step in sequence)
            {
                var timer = Stopwatch.StartNew();
                if (string.IsNullOrEmpty(step.ModuleName))
                {
                    Log.Info("execute {0} ...".FormatWith(step.Script.DisplayName));
                }
                else
                {
                    Log.Info("execute {0} {1} ...".FormatWith(step.ModuleName, step.Script.DisplayName));
                }

                using (Log.Indent())
                {
                    Database.Execute(step.Script, step.ModuleName, step.From, step.To);
                }

                Log.Info("done in {0}".FormatWith(timer.Elapsed));
            }
        }

        private void ShowMigrationSequenceShort(IList<ScriptStep> sequence)
        {
            var message = new StringBuilder()
                .AppendFormat("sequence: {0}", sequence[0].From);

            foreach (var step in sequence)
            {
                message.AppendFormat(" => {0}", step.To);
            }

            Log.Info(message.ToString());
        }

        private void ShowMigrationSequenceFull(IList<ScriptStep> sequence)
        {
            var message = new StringBuilder()
                .Append("sequence: ");

            for (var i = 0; i < sequence.Count; i++)
            {
                var step = sequence[i];
                if (i > 0)
                {
                    message.Append("; ");
                }

                message.AppendFormat("{0} {1} => {2}", step.ModuleName, step.From, step.To);
            }

            Log.Info(message.ToString());
        }
    }
}