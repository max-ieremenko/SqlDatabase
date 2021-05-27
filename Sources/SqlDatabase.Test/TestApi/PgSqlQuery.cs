using System.Configuration;
using Npgsql;

namespace SqlDatabase.TestApi
{
    internal static class PgSqlQuery
    {
        public static string ConnectionString => ConfigurationManager.ConnectionStrings["pgsql"].ConnectionString;

        public static string DatabaseName => new NpgsqlConnectionStringBuilder(ConnectionString).Database;

        public static NpgsqlConnection Open()
        {
            var con = new NpgsqlConnection(ConnectionString);
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
}