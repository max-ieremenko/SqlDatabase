using System.Data.SqlClient;
using Moq;
using Npgsql;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Configuration;
using SqlDatabase.Scripts.MsSql;
using SqlDatabase.Scripts.PgSql;
using SqlDatabase.TestApi;

namespace SqlDatabase.Scripts
{
    [TestFixture]
    public class DatabaseAdapterFactoryTest
    {
        private ILogger _log;
        private AppConfiguration _configuration;

        [SetUp]
        public void BeforeEachTest()
        {
            _log = new Mock<ILogger>(MockBehavior.Strict).Object;
            _configuration = new AppConfiguration();
        }

        [Test]
        public void CreateMsSqlAdapter()
        {
            var actual = DatabaseAdapterFactory.CreateAdapter(MsSqlQuery.ConnectionString, _configuration, _log);

            var adapter = actual.ShouldBeOfType<MsSqlDatabaseAdapter>();
            adapter.DatabaseName.ShouldBe(new SqlConnectionStringBuilder(MsSqlQuery.ConnectionString).InitialCatalog);
        }

        [Test]
        public void CreatePgSqlAdapter()
        {
            var actual = DatabaseAdapterFactory.CreateAdapter(PgSqlQuery.ConnectionString, _configuration, _log);

            var adapter = actual.ShouldBeOfType<PgSqlDatabaseAdapter>();
            adapter.DatabaseName.ShouldBe(new NpgsqlConnectionStringBuilder(PgSqlQuery.ConnectionString).Database);
        }
    }
}
