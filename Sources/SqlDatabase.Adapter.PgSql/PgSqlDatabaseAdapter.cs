using Npgsql;

namespace SqlDatabase.Adapter.PgSql;

internal sealed class PgSqlDatabaseAdapter : IDatabaseAdapter
{
    private readonly string _connectionString;
    private readonly string _connectionStringMaster;
    private readonly ILogger _log;
    private readonly NoticeEventHandler _onConnectionNotice;

    public PgSqlDatabaseAdapter(
        string connectionString,
        string getCurrentVersionScript,
        string setCurrentVersionScript,
        ILogger log)
    {
        GetCurrentVersionScript = getCurrentVersionScript;
        SetCurrentVersionScript = setCurrentVersionScript;
        _log = log;

        var builder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            // force disable pooling, see IT 01.drop.ps1; 02.create.sql
            Pooling = false
        };

        DatabaseName = builder.Database;
        _connectionString = builder.ToString();
        _connectionStringMaster = builder.ToString();
        _onConnectionNotice = OnConnectionNotice;
    }

    public string DatabaseName { get; }

    public string GetCurrentVersionScript { get; internal set; }

    public string SetCurrentVersionScript { get; internal set; }

    public string GetUserFriendlyConnectionString()
    {
        var cs = new NpgsqlConnectionStringBuilder(_connectionString);
        return $"database [{cs.Database}] on [{cs.Host}]";
    }

    public ISqlTextReader CreateSqlTextReader() => new PgSqlTextReader();

    public SqlWriterBase CreateSqlWriter(TextWriter output) => new PgSqlWriter(output);

    public IDbConnection CreateConnection(bool switchToMaster)
    {
        var connectionString = switchToMaster ? _connectionStringMaster : _connectionString;

        var connection = new NpgsqlConnection(connectionString);
        connection.Notice += _onConnectionNotice;

        return connection;
    }

    public string GetServerVersionSelectScript() => "SELECT version();";

    public string GetDatabaseExistsScript(string databaseName) => $"SELECT 1 FROM PG_DATABASE WHERE LOWER(DATNAME) = LOWER('{databaseName}')";

    public string GetVersionSelectScript() => GetCurrentVersionScript;

    public string GetVersionUpdateScript() => SetCurrentVersionScript;

    private void OnConnectionNotice(object sender, NpgsqlNoticeEventArgs e)
    {
        _log.Info($"{e.Notice.Severity}: {e.Notice.MessageText}");
    }
}
