using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using MySqlConnector;
using Npgsql;
using SqlDatabase.Adapter;
using SqlDatabase.Configuration;
using SqlDatabase.Scripts.MsSql;
using SqlDatabase.Scripts.MySql;
using SqlDatabase.Scripts.PgSql;

namespace SqlDatabase.Scripts;

internal static class DatabaseAdapterFactory
{
    public static IDatabaseAdapter CreateAdapter(string connectionString, AppConfiguration configuration, ILogger log)
    {
        // connection strings are compatible
        var factories = new List<Func<string, AppConfiguration, ILogger, IDatabaseAdapter>>(3);

        if (CanBe<SqlConnectionStringBuilder>(connectionString, "Data Source", "Initial Catalog"))
        {
            factories.Add(CreateMsSql);
        }

        if (CanBe<NpgsqlConnectionStringBuilder>(connectionString, "Host", "Database"))
        {
            factories.Add(CreatePgSql);
        }

        if (CanBe<MySqlConnectionStringBuilder>(connectionString, "Server", "Database"))
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
        return new MsSqlDatabaseAdapter(connectionString, configuration, log);
    }

    private static IDatabaseAdapter CreatePgSql(string connectionString, AppConfiguration configuration, ILogger log)
    {
        return new PgSqlDatabaseAdapter(connectionString, configuration, log);
    }

    private static IDatabaseAdapter CreateMySql(string connectionString, AppConfiguration configuration, ILogger log)
    {
        return new MySqlDatabaseAdapter(connectionString, configuration, log);
    }

    private static bool CanBe<TBuilder>(string connectionString, params string[] keywords)
        where TBuilder : DbConnectionStringBuilder, new()
    {
        if (!Is<TBuilder>(connectionString))
        {
            return false;
        }

        var test = new HashSet<string>(keywords, StringComparer.OrdinalIgnoreCase);

        var pairs = connectionString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < pairs.Length; i++)
        {
            var pair = pairs[i].Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
            test.Remove(pair[0]);
        }

        return test.Count == 0;
    }

    private static bool Is<TBuilder>(string connectionString)
        where TBuilder : DbConnectionStringBuilder, new()
    {
        var builder = new TBuilder();

        try
        {
            builder.ConnectionString = connectionString;
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