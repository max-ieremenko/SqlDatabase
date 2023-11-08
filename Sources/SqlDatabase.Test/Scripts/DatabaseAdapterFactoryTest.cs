using System.Collections.Generic;
using System.Configuration;
using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Adapter;
using SqlDatabase.Configuration;

namespace SqlDatabase.Scripts;

[TestFixture]
public class DatabaseAdapterFactoryTest
{
    private ILogger _log = null!;
    private AppConfiguration _configuration = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _log = new Mock<ILogger>(MockBehavior.Strict).Object;
        _configuration = new AppConfiguration
        {
            GetCurrentVersionScript = "get mssql obsolete",
            SetCurrentVersionScript = "set mssql obsolete",
            MsSql =
            {
                GetCurrentVersionScript = "get mssql",
                SetCurrentVersionScript = "set mssql"
            },
            MySql =
            {
                GetCurrentVersionScript = "get mysql",
                SetCurrentVersionScript = "set mysql"
            },
            PgSql =
            {
                GetCurrentVersionScript = "get pgsql",
                SetCurrentVersionScript = "set pgsql"
            }
        };
    }

    [Test]
    [TestCaseSource(nameof(GetCreateAdapterCases))]
    public void CreateAdapter(
        string connectionString,
        string databaseName,
        string adapterName,
        string versionSelectScript,
        string versionUpdateScript)
    {
        var actual = DatabaseAdapterFactory.CreateAdapter(connectionString, _configuration, _log);

        actual.GetType().Name.ShouldBe(adapterName);
        actual.DatabaseName.ShouldBe(databaseName);
        actual.GetVersionSelectScript().ShouldBe(versionSelectScript);
        actual.GetVersionUpdateScript().ShouldBe(versionUpdateScript);
    }

    [Test]
    public void CompatibleConnectionStrings()
    {
        Should.Throw<ConfigurationErrorsException>(() => DatabaseAdapterFactory.CreateAdapter("Server=localhost;Host=localhost;Database=sqldatabasetest", _configuration, _log));
    }

    [Test]
    public void UnknownConnectionStrings()
    {
        Should.Throw<ConfigurationErrorsException>(() => DatabaseAdapterFactory.CreateAdapter("Server=localhost", _configuration, _log));
    }

    [Test]
    public void MsSqlObsoleteConfiguration()
    {
        _configuration.MsSql.GetCurrentVersionScript = string.Empty;
        _configuration.MsSql.SetCurrentVersionScript = string.Empty;

        var actual = DatabaseAdapterFactory.CreateAdapter("Data Source=.;Initial Catalog=SqlDatabaseTest", _configuration, _log);
        actual.GetType().Name.ShouldBe("MsSqlDatabaseAdapter");
        actual.GetVersionSelectScript().ShouldBe("get mssql obsolete");
        actual.GetVersionUpdateScript().ShouldBe("set mssql obsolete");
    }

    private static IEnumerable<TestCaseData> GetCreateAdapterCases()
    {
        yield return new TestCaseData(
            "Data Source=.;Initial Catalog=SqlDatabaseTest",
            "SqlDatabaseTest",
            "MsSqlDatabaseAdapter",
            "get mssql",
            "set mssql")
        {
            TestName = "MsSql"
        };

        yield return new TestCaseData(
            "Host=localhost;Database=sqldatabasetest;",
            "sqldatabasetest",
            "PgSqlDatabaseAdapter",
            "get pgsql",
            "set pgsql")
        {
            TestName = "PgSql"
        };

        yield return new TestCaseData(
            "Server=localhost;Database=sqldatabasetest",
            "sqldatabasetest",
            "MySqlDatabaseAdapter",
            "get mysql",
            "set mysql")
        {
            TestName = "MySql"
        };
    }
}