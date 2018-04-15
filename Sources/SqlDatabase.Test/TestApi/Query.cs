using System.Configuration;
using System.Data.SqlClient;

namespace SqlDatabase.TestApi
{
    internal static class Query
    {
        public static string ConnectionString => ConfigurationManager.ConnectionStrings["test"].ConnectionString;

        public static string DatabaseName => new SqlConnectionStringBuilder(ConnectionString).InitialCatalog;

        public static SqlConnection Open()
        {
            var con = new SqlConnection(ConnectionString);
            con.Open();

            return con;
        }
    }
}