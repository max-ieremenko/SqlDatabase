using System.Diagnostics;
using SqlDatabase.Adapter;
using SqlDatabase.Scripts;

namespace SqlDatabase.Commands;

internal sealed class DatabaseExecuteCommand : DatabaseCommandBase
{
    public DatabaseExecuteCommand(
        ICreateScriptSequence scriptSequence,
        IPowerShellFactory powerShellFactory,
        IDatabase database,
        ILogger log)
        : base(database, log)
    {
        ScriptSequence = scriptSequence;
        PowerShellFactory = powerShellFactory;
    }

    public ICreateScriptSequence ScriptSequence { get; }

    public IPowerShellFactory PowerShellFactory { get; }

    protected override void Greet(string databaseLocation)
    {
        Log.Info("Execute script on {0}".FormatWith(databaseLocation));
    }

    protected override void ExecuteCore()
    {
        var sequences = ScriptSequence.BuildSequence();

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