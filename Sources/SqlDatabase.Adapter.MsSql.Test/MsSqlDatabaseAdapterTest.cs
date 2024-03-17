using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.TestApi;

namespace SqlDatabase.Adapter.MsSql;

[TestFixture]
public class MsSqlDatabaseAdapterTest
{
    private const string ModuleName = "SomeModuleName";
    private const string SelectModuleVersion = "SELECT value from sys.fn_listextendedproperty('version-{{ModuleName}}', default, default, default, default, default, default)";
    private const string UpdateModuleVersion = "EXEC sys.sp_updateextendedproperty @name=N'version-{{ModuleName}}', @value=N'{{TargetVersion}}'";

    private MsSqlDatabaseAdapter _sut = null!;
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
                TestOutput.WriteLine("Info: {0}", m);
                _logOutput.Add(m);
            });

        _sut = new MsSqlDatabaseAdapter(MsSqlQuery.GetConnectionString(), null!, null!, log.Object);
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
            connection.Database.ShouldBe("master");
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
            TestOutput.WriteLine(actual);

            actual.ShouldBeOfType<string>().ShouldNotBeNullOrWhiteSpace();
        }
    }

    [Test]
    [TestCase("master", true)]
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
    [TestCase("use master", "master")]
    [TestCase("print 'xx1xx'", "xx1xx")]
    public void SqlOutputIntoLog(string script, string expected)
    {
        using (var connection = _sut.CreateConnection(false))
        {
            connection.Open();

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = script;
                cmd.ExecuteNonQuery();

                _logOutput.Count.ShouldBe(1);
                _logOutput[0].ShouldContain(expected);
            }
        }
    }

    [Test]
    public void GetSetVersionScriptDefault()
    {
        _sut.GetCurrentVersionScript = MsSqlDefaults.DefaultSelectVersion;
        _sut.SetCurrentVersionScript = MsSqlDefaults.DefaultUpdateVersion;

        _sut.GetVersionSelectScript().ShouldBe(MsSqlDefaults.DefaultSelectVersion);
        _sut.GetVersionUpdateScript().ShouldBe(MsSqlDefaults.DefaultUpdateVersion);

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
        _sut.GetCurrentVersionScript = SelectModuleVersion;
        _sut.SetCurrentVersionScript = UpdateModuleVersion;

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