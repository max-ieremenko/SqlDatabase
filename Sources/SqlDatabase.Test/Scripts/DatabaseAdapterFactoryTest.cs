using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Configuration;
using SqlDatabase.Scripts.MsSql;
using SqlDatabase.Scripts.MySql;
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
        [TestCaseSource(nameof(GetCreateAdapterCases))]
        public void CreateAdapter(string connectionString, string databaseName, Type expected)
        {
            var actual = DatabaseAdapterFactory.CreateAdapter(connectionString, _configuration, _log);

            actual.ShouldBeOfType(expected);
            actual.DatabaseName.ShouldBe(databaseName);
        }

        private static IEnumerable<TestCaseData> GetCreateAdapterCases()
        {
            yield return new TestCaseData(
                MsSqlQuery.ConnectionString,
                MsSqlQuery.DatabaseName,
                typeof(MsSqlDatabaseAdapter))
            {
                TestName = "MsSql"
            };

            yield return new TestCaseData(
                PgSqlQuery.ConnectionString,
                PgSqlQuery.DatabaseName,
                typeof(PgSqlDatabaseAdapter))
            {
                TestName = "PgSql"
            };

            yield return new TestCaseData(
                MySqlQuery.ConnectionString,
                MySqlQuery.DatabaseName,
                typeof(MySqlDatabaseAdapter))
            {
                TestName = "MySql"
            };
        }
    }
}
