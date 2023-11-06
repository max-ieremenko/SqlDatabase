using System.Data;
using System.IO;
using Npgsql;
using SqlDatabase.Adapter;
using SqlDatabase.Configuration;
using SqlDatabase.Export;

namespace SqlDatabase.Scripts.PgSql;

internal sealed class PgSqlDatabaseAdapter : IDatabaseAdapter
{
    public const string DefaultSelectVersion = "SELECT version FROM public.version WHERE module_name = 'database'";
    public const string DefaultUpdateVersion = "UPDATE public.version SET version='{{TargetVersion}}' WHERE module_name = 'database'";

    private readonly string _connectionString;
    private readonly string _connectionStringMaster;
    private readonly AppConfiguration _configuration;
    private readonly ILogger _log;
    private readonly NoticeEventHandler _onConnectionNotice;

    public PgSqlDatabaseAdapter(
        string connectionString,
        AppConfiguration configuration,
        ILogger log)
    {
        _configuration = configuration;
        _log = log;

        var builder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            // force disable pooling, see IT 01.drop.ps1; 02.create.sql
            Pooling = false
        };

        DatabaseName = builder.Database;
        _connectionString = builder.ToString();

        builder.Database = null;
        _connectionStringMaster = builder.ToString();

        _onConnectionNotice = OnConnectionNotice;
    }

    public string DatabaseName { get; }

    public string GetUserFriendlyConnectionString()
    {
        var cs = new NpgsqlConnectionStringBuilder(_connectionString);
        return "database [{0}] on [{1}]".FormatWith(cs.Database, cs.Host);
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

    public string GetDatabaseExistsScript(string databaseName) => "SELECT 1 FROM PG_DATABASE WHERE LOWER(DATNAME) = LOWER('{0}')".FormatWith(databaseName);

    public string GetVersionSelectScript()
    {
        var script = _configuration.PgSql.GetCurrentVersionScript;
        if (string.IsNullOrWhiteSpace(script))
        {
            script = _configuration.GetCurrentVersionScript;
        }

        if (string.IsNullOrWhiteSpace(script))
        {
            script = DefaultSelectVersion;
        }

        return script!;
    }

    public string GetVersionUpdateScript()
    {
        var script = _configuration.PgSql.SetCurrentVersionScript;
        if (string.IsNullOrWhiteSpace(script))
        {
            script = _configuration.SetCurrentVersionScript;
        }

        if (string.IsNullOrWhiteSpace(script))
        {
            script = DefaultUpdateVersion;
        }

        return script!;
    }

    private void OnConnectionNotice(object sender, NpgsqlNoticeEventArgs e)
    {
        _log.Info("{0}: {1}".FormatWith(e.Notice.Severity, e.Notice.MessageText));
    }
}