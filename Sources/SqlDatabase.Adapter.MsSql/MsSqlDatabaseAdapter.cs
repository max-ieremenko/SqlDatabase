using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace SqlDatabase.Adapter.MsSql;

internal sealed class MsSqlDatabaseAdapter : IDatabaseAdapter
{
    private readonly string _connectionString;
    private readonly string _connectionStringMaster;
    private readonly ILogger _log;
    private readonly SqlInfoMessageEventHandler _onConnectionInfoMessage;

    public MsSqlDatabaseAdapter(
        string connectionString,
        string getCurrentVersionScript,
        string setCurrentVersionScript,
        ILogger log)
    {
        GetCurrentVersionScript = getCurrentVersionScript;
        SetCurrentVersionScript = setCurrentVersionScript;
        _connectionString = connectionString;
        _log = log;

        var builder = new SqlConnectionStringBuilder(connectionString);
        DatabaseName = builder.InitialCatalog;
        builder.InitialCatalog = "master";
        _connectionStringMaster = builder.ToString();

        _onConnectionInfoMessage = OnConnectionInfoMessage;
    }

    public string DatabaseName { get; }

    public string GetCurrentVersionScript { get; internal set; }

    public string SetCurrentVersionScript { get; internal set; }

    public string GetUserFriendlyConnectionString()
    {
        var cs = new SqlConnectionStringBuilder(_connectionString);
        return $"database [{cs.InitialCatalog}] on [{cs.DataSource}]";
    }

    public ISqlTextReader CreateSqlTextReader() => new MsSqlTextReader();

    public SqlWriterBase CreateSqlWriter(TextWriter output) => new MsSqlWriter(output);

    public IDbConnection CreateConnection(bool switchToMaster)
    {
        var connectionString = switchToMaster ? _connectionStringMaster : _connectionString;

        var connection = new SqlConnection(connectionString);
        connection.InfoMessage += _onConnectionInfoMessage;

        return connection;
    }

    public string GetServerVersionSelectScript() => "select @@version";

    public string GetDatabaseExistsScript(string databaseName) => $"SELECT 1 FROM sys.databases WHERE Name=N'{databaseName}'";

    public string GetVersionSelectScript() => GetCurrentVersionScript;

    public string GetVersionUpdateScript() => SetCurrentVersionScript;

    private void OnConnectionInfoMessage(object sender, SqlInfoMessageEventArgs e)
    {
        _log.Info($"output: {e}");
    }
}