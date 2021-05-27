using System;
using System.Configuration;
using System.Data.SqlClient;
using Npgsql;
using SqlDatabase.Configuration;
using SqlDatabase.Scripts.MsSql;
using SqlDatabase.Scripts.PgSql;

namespace SqlDatabase.Scripts
{
    internal static class DatabaseAdapterFactory
    {
        public static IDatabaseAdapter CreateAdapter(string connectionString, AppConfiguration configuration, ILogger log)
        {
            if (IsMsSql(connectionString))
            {
                return new MsSqlDatabaseAdapter(connectionString, configuration, log);
            }

            if (IsPgSql(connectionString))
            {
                return new PgSqlDatabaseAdapter(connectionString, configuration, log);
            }

            throw new ConfigurationErrorsException("Fail to define database type from provided connection string.");
        }

        private static bool IsMsSql(string connectionString)
        {
            try
            {
                new SqlConnectionStringBuilder(connectionString);
                return true;
            }
            catch (ArgumentException)
            {
            }
            catch (FormatException)
            {
            }

            return false;
        }

        private static bool IsPgSql(string connectionString)
        {
            try
            {
                new NpgsqlConnectionStringBuilder(connectionString);
                return true;
            }
            catch (ArgumentException)
            {
            }
            catch (FormatException)
            {
            }

            return false;
        }
    }
}
