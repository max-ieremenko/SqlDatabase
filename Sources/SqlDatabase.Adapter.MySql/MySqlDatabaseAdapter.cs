using MySqlConnector;

namespace SqlDatabase.Adapter.MySql;

internal sealed class MySqlDatabaseAdapter : IDatabaseAdapter
{
    private readonly string _connectionString;
    private readonly string _connectionStringMaster;
    private readonly ILogger _log;
    private readonly MySqlInfoMessageEventHandler _onConnectionInfoMessage;

    public MySqlDatabaseAdapter(
        string connectionString,
        string getCurrentVersionScript,
        string setCurrentVersionScript,
        ILogger log)
    {
        GetCurrentVersionScript = getCurrentVersionScript;
        SetCurrentVersionScript = setCurrentVersionScript;
        _log = log;

        var builder = new MySqlConnectionStringBuilder(connectionString);

        DatabaseName = builder.Database;
        _connectionString = builder.ToString();

        builder.Database = null;
        _connectionStringMaster = builder.ToString();

        _onConnectionInfoMessage = OnConnectionInfoMessage;
    }

    public string DatabaseName { get; }

    public string GetCurrentVersionScript { get; internal set; }

    public string SetCurrentVersionScript { get; internal set; }

    public string GetUserFriendlyConnectionString()
    {
        var cs = new MySqlConnectionStringBuilder(_connectionString);
        return $"database [{cs.Database}] on [{cs.Server}]";
    }

    public ISqlTextReader CreateSqlTextReader() => new MySqlTextReader();

    public SqlWriterBase CreateSqlWriter(TextWriter output) => new MySqlWriter(output);

    public IValueDataReader CreateValueDataReader() => new MySqlValueDataReader();

    public IDbConnection CreateConnection(bool switchToMaster)
    {
        var connectionString = switchToMaster ? _connectionStringMaster : _connectionString;

        var connection = new MySqlConnection(connectionString);
        connection.InfoMessage += _onConnectionInfoMessage;

        return connection;
    }

    public string GetServerVersionSelectScript() => "SELECT concat(@@version_comment, ', ', version(), ', ', @@version_compile_os)";

    public string GetDatabaseExistsScript(string databaseName) => $"SELECT 1 FROM information_schema.schemata WHERE LOWER(schema_name) = LOWER('{databaseName}')";

    public string GetVersionSelectScript() => GetCurrentVersionScript;

    public string GetVersionUpdateScript() => SetCurrentVersionScript;

    private void OnConnectionInfoMessage(object sender, MySqlInfoMessageEventArgs args)
    {
        for (var i = 0; i < args.Errors.Count; i++)
        {
            var error = args.Errors[i];
            _log.Info($"{error.Level}: {error.Message}");
        }
    }
}