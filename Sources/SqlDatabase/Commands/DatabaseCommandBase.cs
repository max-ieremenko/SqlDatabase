using SqlDatabase.Scripts;

namespace SqlDatabase.Commands
{
    internal abstract class DatabaseCommandBase : ICommand
    {
        public ILogger Log { get; set; }

        public IDatabase Database { get; set; }

        public void Execute()
        {
            Greet(Database.Adapter.GetUserFriendlyConnectionString());
            Log.Info(Database.GetServerVersion());

            ExecuteCore();
        }

        protected abstract void Greet(string databaseLocation);

        protected abstract void ExecuteCore();
    }
}