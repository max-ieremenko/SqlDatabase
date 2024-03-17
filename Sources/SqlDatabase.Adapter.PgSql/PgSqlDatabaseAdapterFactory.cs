using System.Data.Common;
using Npgsql;

namespace SqlDatabase.Adapter.PgSql;

public static class PgSqlDatabaseAdapterFactory
{
    public static bool CanBe(string connectionString)
    {
        if (!IsPgSql(connectionString))
        {
            return false;
        }

        var builder = new DbConnectionStringBuilder(false) { ConnectionString = connectionString };
        return builder.ContainsKey("Host") && builder.ContainsKey("Database");
    }

    public static IDatabaseAdapter CreateAdapter(
        string connectionString,
        string? getCurrentVersionScript,
        string? setCurrentVersionScript,
        ILogger log)
    {
        return new PgSqlDatabaseAdapter(
            connectionString,
            string.IsNullOrWhiteSpace(getCurrentVersionScript) ? PgSqlDefaults.DefaultSelectVersion : getCurrentVersionScript!,
            string.IsNullOrWhiteSpace(setCurrentVersionScript) ? PgSqlDefaults.DefaultUpdateVersion : setCurrentVersionScript!,
            log);
    }

    private static bool IsPgSql(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder();

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