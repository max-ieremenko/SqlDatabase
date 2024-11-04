namespace SqlDatabase.Adapter;

public interface IDatabaseAdapter
{
    string DatabaseName { get; }

    string GetUserFriendlyConnectionString();

    ISqlTextReader CreateSqlTextReader();

    SqlWriterBase CreateSqlWriter(TextWriter output);

    IValueDataReader CreateValueDataReader();

    IDbConnection CreateConnection(bool switchToMaster);

    string GetServerVersionSelectScript();

    string GetDatabaseExistsScript(string databaseName);

    string GetVersionSelectScript();

    string GetVersionUpdateScript();
}