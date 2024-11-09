using Npgsql;
using NpgsqlTypes;
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

    // NpgsqlTsQuery.Parse(value)
    public static NpgsqlTsQuery ParseTsQuery(string value) => (NpgsqlTsQuery)ExecuteScalar($"SELECT '{value}'::tsquery")!;

    // NpgsqlTsVector.Parse(value)
    public static NpgsqlTsVector ParseTsVector(string value) => (NpgsqlTsVector)ExecuteScalar($"SELECT '{value}'::tsvector")!;
}