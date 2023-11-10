using MySqlConnector;
using SqlDatabase.TestApi;

namespace SqlDatabase.Adapter.MySql;

internal static class MySqlQuery
{
    public static string GetConnectionString()
    {
        return ConfigurationExtensions.GetConnectionString("mysql");
    }

    public static MySqlConnection Open()
    {
        var con = new MySqlConnection(GetConnectionString());
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