using System.Configuration;
using MySqlConnector;

namespace SqlDatabase.TestApi;

internal static class MySqlQuery
{
    public static string ConnectionString => ConfigurationManager.ConnectionStrings["mysql"].ConnectionString;

    public static string DatabaseName => new MySqlConnectionStringBuilder(ConnectionString).Database;

    public static MySqlConnection Open()
    {
        var con = new MySqlConnection(ConnectionString);
        con.Open();

        return con;
    }

    public static object ExecuteScalar(string sql)
    {
        using (var connection = Open())
        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = sql;
            return cmd.ExecuteScalar();
        }
    }
}