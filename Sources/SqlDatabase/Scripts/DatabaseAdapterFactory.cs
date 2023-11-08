using System;
using System.Collections.Generic;
using System.Configuration;
using SqlDatabase.Adapter;
using SqlDatabase.Adapter.MsSql;
using SqlDatabase.Adapter.MySql;
using SqlDatabase.Adapter.PgSql;
using SqlDatabase.Configuration;

namespace SqlDatabase.Scripts;

internal static class DatabaseAdapterFactory
{
    public static IDatabaseAdapter CreateAdapter(string connectionString, AppConfiguration configuration, ILogger log)
    {
        // connection strings are compatible
        var factories = new List<Func<string, AppConfiguration, ILogger, IDatabaseAdapter>>(3);

        if (MsSqlDatabaseAdapterFactory.CanBe(connectionString))
        {
            factories.Add(CreateMsSql);
        }

        if (PgSqlDatabaseAdapterFactory.CanBe(connectionString))
        {
            factories.Add(CreatePgSql);
        }

        if (MySqlDatabaseAdapterFactory.CanBe(connectionString))
        {
            factories.Add(CreateMySql);
        }

        if (factories.Count != 1)
        {
            throw new ConfigurationErrorsException("Could not determine the database type from the provided connection string.");
        }

        return factories[0](connectionString, configuration, log);
    }

    private static IDatabaseAdapter CreateMsSql(string connectionString, AppConfiguration configuration, ILogger log)
    {
        var getCurrentVersionScript = configuration.MsSql.GetCurrentVersionScript;
        if (string.IsNullOrWhiteSpace(getCurrentVersionScript))
        {
            getCurrentVersionScript = configuration.GetCurrentVersionScript;
        }

        var setCurrentVersionScript = configuration.MsSql.SetCurrentVersionScript;
        if (string.IsNullOrWhiteSpace(setCurrentVersionScript))
        {
            setCurrentVersionScript = configuration.SetCurrentVersionScript;
        }

        return MsSqlDatabaseAdapterFactory.CreateAdapter(connectionString, getCurrentVersionScript, setCurrentVersionScript, log);
    }

    private static IDatabaseAdapter CreatePgSql(string connectionString, AppConfiguration configuration, ILogger log)
    {
        return PgSqlDatabaseAdapterFactory.CreateAdapter(connectionString, configuration.PgSql.GetCurrentVersionScript, configuration.PgSql.SetCurrentVersionScript, log);
    }

    private static IDatabaseAdapter CreateMySql(string connectionString, AppConfiguration configuration, ILogger log)
    {
        return MySqlDatabaseAdapterFactory.CreateAdapter(connectionString, configuration.MySql.GetCurrentVersionScript, configuration.MySql.SetCurrentVersionScript, log);
    }
}