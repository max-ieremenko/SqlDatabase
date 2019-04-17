using System.Data.SqlClient;
using SqlDatabase.Scripts;

namespace SqlDatabase.Commands
{
    internal abstract class DatabaseCommandBase : ICommand
    {
        public ILogger Log { get; set; }

        public IDatabase Database { get; set; }

        public void Execute()
        {
            var cs = new SqlConnectionStringBuilder(Database.ConnectionString);
            var databaseLocation = "database [{0}] on [{1}]".FormatWith(cs.InitialCatalog, cs.DataSource);

            Greet(databaseLocation);
            Log.Info(Database.GetServerVersion());

            ExecuteCore();
        }

        protected abstract void Greet(string databaseLocation);

        protected abstract void ExecuteCore();
    }
}