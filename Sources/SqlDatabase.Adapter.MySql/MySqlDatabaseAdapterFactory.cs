using System.Data.Common;
using MySqlConnector;

namespace SqlDatabase.Adapter.MySql;

public static class MySqlDatabaseAdapterFactory
{
    public static bool CanBe(string connectionString)
    {
        if (!IsMsSql(connectionString))
        {
            return false;
        }

        var builder = new DbConnectionStringBuilder(false) { ConnectionString = connectionString };
        return builder.ContainsKey("Server") && builder.ContainsKey("Database");
    }

    public static IDatabaseAdapter CreateAdapter(
        string connectionString,
        string? getCurrentVersionScript,
        string? setCurrentVersionScript,
        ILogger log)
    {
        return new MySqlDatabaseAdapter(
            connectionString,
            string.IsNullOrWhiteSpace(getCurrentVersionScript) ? MySqlDefaults.DefaultSelectVersion : getCurrentVersionScript!,
            string.IsNullOrWhiteSpace(setCurrentVersionScript) ? MySqlDefaults.DefaultUpdateVersion : setCurrentVersionScript!,
            log);
    }

    private static bool IsMsSql(string connectionString)
    {
        var builder = new MySqlConnectionStringBuilder();

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