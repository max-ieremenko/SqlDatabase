using System.Configuration;
using System.Data.SqlClient;

namespace SqlDatabase.TestApi;

internal static class MsSqlQuery
{
    public static string ConnectionString => ConfigurationManager.ConnectionStrings["mssql"].ConnectionString;

    public static string DatabaseName => new SqlConnectionStringBuilder(ConnectionString).InitialCatalog;

    public static SqlConnection Open()
    {
        var con = new SqlConnection(ConnectionString);
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