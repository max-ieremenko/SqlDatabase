using System.Data;
using System.Data.SqlClient;
using System.IO;
using SqlDatabase.Configuration;
using SqlDatabase.Export;

namespace SqlDatabase.Scripts.MsSql
{
    internal sealed class MsSqlDatabaseAdapter : IDatabaseAdapter
    {
        public const string DefaultSelectVersion = "SELECT value from sys.fn_listextendedproperty('version', default, default, default, default, default, default)";
        public const string DefaultUpdateVersion = "EXEC sys.sp_updateextendedproperty @name=N'version', @value=N'{{TargetVersion}}'";

        private readonly string _connectionString;
        private readonly string _connectionStringMaster;
        private readonly AppConfiguration _configuration;
        private readonly ILogger _log;
        private readonly SqlInfoMessageEventHandler _onConnectionInfoMessage;

        public MsSqlDatabaseAdapter(
            string connectionString,
            AppConfiguration configuration,
            ILogger log)
        {
            _connectionString = connectionString;
            _configuration = configuration;
            _log = log;

            var builder = new SqlConnectionStringBuilder(connectionString);
            DatabaseName = builder.InitialCatalog;
            builder.InitialCatalog = "master";
            _connectionStringMaster = builder.ToString();

            _onConnectionInfoMessage = OnConnectionInfoMessage;
        }

        public string DatabaseName { get; }

        public string GetUserFriendlyConnectionString()
        {
            var cs = new SqlConnectionStringBuilder(_connectionString);
            return "database [{0}] on [{1}]".FormatWith(cs.InitialCatalog, cs.DataSource);
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

        public string GetDatabaseExistsScript(string databaseName) => "SELECT 1 FROM sys.databases WHERE Name=N'{0}'".FormatWith(databaseName);

        public string GetVersionSelectScript()
        {
            var script = _configuration.MsSql.GetCurrentVersionScript;
            if (string.IsNullOrWhiteSpace(script))
            {
                script = _configuration.GetCurrentVersionScript;
            }

            if (string.IsNullOrWhiteSpace(script))
            {
                script = DefaultSelectVersion;
            }

            return script;
        }

        public string GetVersionUpdateScript()
        {
            var script = _configuration.MsSql.SetCurrentVersionScript;
            if (string.IsNullOrWhiteSpace(script))
            {
                script = _configuration.SetCurrentVersionScript;
            }

            if (string.IsNullOrWhiteSpace(script))
            {
                script = DefaultUpdateVersion;
            }

            return script;
        }

        private void OnConnectionInfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            _log.Info("output: {0}".FormatWith(e.ToString()));
        }
    }
}
