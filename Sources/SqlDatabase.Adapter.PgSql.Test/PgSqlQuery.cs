using Npgsql;
using SqlDatabase.TestApi;

namespace SqlDatabase.Adapter.PgSql;

internal static class PgSqlQuery
{
    public static string GetConnectionString()
    {
        return ConfigurationExtensions.GetConnectionString("pgsql");
    }

    public static NpgsqlConnection Open()
    {
        var con = new NpgsqlConnection(GetConnectionString());
        con.Open();

        return con;
    }

    public static object? ExecuteScalar(string sql)
    {
        using (var connection = Open())
        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = sql;
            return cmd.ExecuteScalar();
        }
    }
}