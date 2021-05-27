using System.Data;
using System.IO;
using SqlDatabase.Export;

namespace SqlDatabase.Scripts
{
    internal interface IDatabaseAdapter
    {
        string DatabaseName { get; }

        string GetUserFriendlyConnectionString();

        ISqlTextReader CreateSqlTextReader();

        SqlWriterBase CreateSqlWriter(TextWriter output);

        IDbConnection CreateConnection(bool switchToMaster);

        string GetServerVersionSelectScript();

        string GetDatabaseExistsScript(string databaseName);

        string GetVersionSelectScript();

        string GetVersionUpdateScript();
    }
}
