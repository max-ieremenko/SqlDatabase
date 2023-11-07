using System.Data.SqlClient;
using SqlDatabase.TestApi;

namespace SqlDatabase.Adapter.MsSql;

internal static class MsSqlQuery
{
    public static string GetConnectionString()
    {
        return ConfigurationExtensions.GetConnectionString("mssql");
    }

    public static SqlConnection Open()
    {
        var con = new SqlConnection(GetConnectionString());
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