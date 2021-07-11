using System;
using System.Data;
using System.IO;
using MySqlConnector;
using SqlDatabase.Configuration;
using SqlDatabase.Export;

namespace SqlDatabase.Scripts.MySql
{
    internal sealed class MySqlDatabaseAdapter : IDatabaseAdapter
    {
        public const string DefaultSelectVersion = "SELECT version FROM version WHERE module_name = 'database'";
        public const string DefaultUpdateVersion = "UPDATE version SET version='{{TargetVersion}}' WHERE module_name = 'database'";

        private readonly string _connectionString;
        private readonly string _connectionStringMaster;
        private readonly AppConfiguration _configuration;
        private readonly ILogger _log;
        private readonly MySqlInfoMessageEventHandler _onConnectionInfoMessage;

        public MySqlDatabaseAdapter(
            string connectionString,
            AppConfiguration configuration,
            ILogger log)
        {
            _configuration = configuration;
            _log = log;

            var builder = new MySqlConnectionStringBuilder(connectionString);

            DatabaseName = builder.Database;
            _connectionString = builder.ToString();

            builder.Database = null;
            _connectionStringMaster = builder.ToString();

            _onConnectionInfoMessage = OnConnectionInfoMessage;
        }

        public string DatabaseName { get; }

        public string GetUserFriendlyConnectionString()
        {
            var cs = new MySqlConnectionStringBuilder(_connectionString);
            return "database [{0}] on [{1}]".FormatWith(cs.Database, cs.Server);
        }

        public ISqlTextReader CreateSqlTextReader() => new MySqlTextReader();

        public SqlWriterBase CreateSqlWriter(TextWriter output) => new MySqlWriter(output);

        public IDbConnection CreateConnection(bool switchToMaster)
        {
            var connectionString = switchToMaster ? _connectionStringMaster : _connectionString;

            var connection = new MySqlConnection(connectionString);
            connection.InfoMessage += _onConnectionInfoMessage;

            return connection;
        }

        public string GetServerVersionSelectScript() => "SELECT concat(@@version_comment, ', ', version(), ', ', @@version_compile_os)";

        public string GetDatabaseExistsScript(string databaseName) => "SELECT 1 FROM information_schema.schemata WHERE LOWER(schema_name) = LOWER('{0}')".FormatWith(databaseName);

        public string GetVersionSelectScript()
        {
            var script = _configuration.MySql.GetCurrentVersionScript;
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
            var script = _configuration.MySql.SetCurrentVersionScript;
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

        private void OnConnectionInfoMessage(object sender, MySqlInfoMessageEventArgs args)
        {
            for (var i = 0; i < args.Errors.Count; i++)
            {
                var error = args.Errors[i];
                _log.Info("{0}: {1}".FormatWith(error.Level, error.Message));
            }
        }
    }
}
