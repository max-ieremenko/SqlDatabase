using SqlDatabase.Scripts;

namespace SqlDatabase.Commands;

internal abstract class DatabaseCommandBase : ICommand
{
    protected DatabaseCommandBase(IDatabase database, ILogger log)
    {
        Database = database;
        Log = log;
    }

    public ILogger Log { get; }

    public IDatabase Database { get; }

    public void Execute()
    {
        Greet(Database.Adapter.GetUserFriendlyConnectionString());
        Log.Info(Database.GetServerVersion());

        ExecuteCore();
    }

    protected abstract void Greet(string databaseLocation);

    protected abstract void ExecuteCore();
}