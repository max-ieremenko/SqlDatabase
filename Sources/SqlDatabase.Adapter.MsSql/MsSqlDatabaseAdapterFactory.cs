using System;
using System.Data.Common;
using System.Data.SqlClient;

namespace SqlDatabase.Adapter.MsSql;

public static class MsSqlDatabaseAdapterFactory
{
    public static bool CanBe(string connectionString)
    {
        if (!IsMsSql(connectionString))
        {
            return false;
        }

        var builder = new DbConnectionStringBuilder(false) { ConnectionString = connectionString };
        return builder.ContainsKey("Data Source") && builder.ContainsKey("Initial Catalog");
    }

    public static IDatabaseAdapter CreateAdapter(
        string connectionString,
        string? getCurrentVersionScript,
        string? setCurrentVersionScript,
        ILogger log)
    {
        return new MsSqlDatabaseAdapter(
            connectionString,
            string.IsNullOrWhiteSpace(getCurrentVersionScript) ? MsSqlDefaults.DefaultSelectVersion : getCurrentVersionScript!,
            string.IsNullOrWhiteSpace(setCurrentVersionScript) ? MsSqlDefaults.DefaultUpdateVersion : setCurrentVersionScript!,
            log);
    }

    private static bool IsMsSql(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder();

        try
        {
            builder.ConnectionString = connectionString;
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}