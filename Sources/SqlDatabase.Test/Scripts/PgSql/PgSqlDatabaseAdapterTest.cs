using System;
using System.Collections.Generic;
using System.Data;
using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Configuration;
using SqlDatabase.TestApi;

namespace SqlDatabase.Scripts.PgSql;

[TestFixture]
public class PgSqlDatabaseAdapterTest
{
    private const string ModuleName = "SomeModuleName";
    private const string SelectModuleVersion = "SELECT version FROM public.version WHERE module_name = '{{ModuleName}}'";
    private const string UpdateModuleVersion = "UPDATE public.version SET version='{{TargetVersion}}' WHERE module_name = '{{ModuleName}}'";

    private PgSqlDatabaseAdapter _sut = null!;
    private AppConfiguration _configuration = null!;
    private IList<string> _logOutput = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _logOutput = new List<string>();
        var log = new Mock<ILogger>(MockBehavior.Strict);
        log
            .Setup(l => l.Info(It.IsAny<string>()))
            .Callback<string>(m =>
            {
                Console.WriteLine("Info: {0}", m);
                _logOutput.Add(m);
            });

        _configuration = new AppConfiguration();

        _sut = new PgSqlDatabaseAdapter(PgSqlQuery.ConnectionString, _configuration, log.Object);
    }

    [Test]
    public void CreateConnectionToTargetDatabase()
    {
        using (var connection = _sut.CreateConnection(false))
        {
            connection.State.ShouldBe(ConnectionState.Closed);

            connection.Open();
            connection.Database.ShouldBe(_sut.DatabaseName);
        }
    }

    [Test]
    public void CreateConnectionToMaster()
    {
        using (var connection = _sut.CreateConnection(true))
        {
            connection.State.ShouldBe(ConnectionState.Closed);

            connection.Open();
            connection.Database.ShouldBe("postgres");
        }
    }

    [Test]
    public void GetServerVersionSelectScript()
    {
        using (var connection = _sut.CreateConnection(false))
        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = _sut.GetServerVersionSelectScript();
            connection.Open();

            var actual = cmd.ExecuteScalar();
            Console.WriteLine(actual);

            actual.ShouldBeOfType<string>().ShouldNotBeNullOrWhiteSpace();
        }
    }

    [Test]
    [TestCase("postgres", true)]
    [TestCase("POSTGRES", true)]
    [TestCase("unknown database", false)]
    [TestCase(null, true)]
    public void GetDatabaseExistsScript(string databaseName, bool expected)
    {
        using (var connection = _sut.CreateConnection(true))
        using (var cmd = connection.CreateCommand())
        {
            connection.Open();

            cmd.CommandText = _sut.GetDatabaseExistsScript(databaseName ?? _sut.DatabaseName);
            var actual = cmd.ExecuteScalar()?.ToString();

            if (expected)
            {
                actual.ShouldNotBeNullOrWhiteSpace();
            }
            else
            {
                actual.ShouldBeNullOrEmpty();
            }
        }
    }

    [Test]
    public void SqlOutputIntoLog()
    {
        using (var connection = _sut.CreateConnection(false))
        {
            connection.Open();

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
DO $$
BEGIN
RAISE NOTICE 'hello';
END
$$;";
                cmd.ExecuteNonQuery();

                _logOutput.Count.ShouldBe(1);
                _logOutput[0].ShouldBe("NOTICE: hello");
            }
        }
    }

    [Test]
    public void GetSetVersionScriptDefault()
    {
        _sut.GetVersionSelectScript().ShouldBe(PgSqlDatabaseAdapter.DefaultSelectVersion);
        _sut.GetVersionUpdateScript().ShouldBe(PgSqlDatabaseAdapter.DefaultUpdateVersion);

        using (var connection = _sut.CreateConnection(false))
        {
            connection.Open();
            using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.Transaction = transaction;

                    cmd.CommandText = _sut.GetVersionUpdateScript().Replace("{{TargetVersion}}", "new version");
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = _sut.GetVersionSelectScript();

                    var actual = cmd.ExecuteScalar();
                    actual.ShouldBeOfType<string>().ShouldBe("new version");
                }

                transaction.Rollback();
            }
        }
    }

    [Test]
    public void GetSetVersionScriptModuleName()
    {
        _configuration.GetCurrentVersionScript = SelectModuleVersion;
        _configuration.SetCurrentVersionScript = UpdateModuleVersion;

        _sut.GetVersionSelectScript().ShouldBe(SelectModuleVersion);
        _sut.GetVersionUpdateScript().ShouldBe(UpdateModuleVersion);

        using (var connection = _sut.CreateConnection(false))
        {
            connection.Open();
            using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.Transaction = transaction;

                    cmd.CommandText = _sut
                        .GetVersionUpdateScript()
                        .Replace("{{TargetVersion}}", "new version")
                        .Replace("{{ModuleName}}", ModuleName);
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = _sut.GetVersionSelectScript().Replace("{{ModuleName}}", ModuleName);

                    var actual = cmd.ExecuteScalar();
                    actual.ShouldBeOfType<string>().ShouldBe("new version");
                }

                transaction.Rollback();
            }
        }
    }
}