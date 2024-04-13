using Moq;
using NUnit.Framework;
using Shouldly;

namespace SqlDatabase.Adapter.AssemblyScripts;

[TestFixture]
public class AssemblyScriptTest
{
    private AssemblyScript _sut = null!;
    private Mock<IVariables> _variables = null!;
    private Mock<ILogger> _log = null!;
    private Mock<IDbCommand> _command = null!;

    private IList<string> _logOutput = null!;
    private IList<string> _executedScripts = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _variables = new Mock<IVariables>(MockBehavior.Strict);

        _logOutput = new List<string>();
        _log = new Mock<ILogger>(MockBehavior.Strict);
        _log
            .Setup(l => l.Info(It.IsAny<string>()))
            .Callback<string>(_logOutput.Add);

        _executedScripts = new List<string>();
        _command = new Mock<IDbCommand>(MockBehavior.Strict);
        _command.SetupProperty(c => c.CommandText);
        _command
            .Setup(c => c.ExecuteNonQuery())
            .Callback(() => _executedScripts.Add(_command.Object.CommandText))
            .Returns(0);

#if NET472
        var frameworkVersion = FrameworkVersion.Net472;
#else
        var frameworkVersion = FrameworkVersion.Net6;
#endif
        _sut = new AssemblyScript(
            frameworkVersion,
            "dummy",
            null,
            null,
            null!,
            null!);
    }

    [Test]
    public void ExecuteExampleMsSql()
    {
        _sut.ClassName = "MsSql.SqlDatabaseScript";

        _variables
            .Setup(v => v.GetValue("DatabaseName"))
            .Returns("dbName");
        _variables
            .Setup(v => v.GetValue("CurrentVersion"))
            .Returns("1.0");
        _variables
            .Setup(v => v.GetValue("TargetVersion"))
            .Returns("2.0");

        _sut.DisplayName = "2.1_2.2.dll";
        _sut.ReadAssemblyContent = () => File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "2.1_2.2.dll"));

        using (new ConsoleListener(_log.Object))
        {
            _sut.Execute(new DbCommandStub(_command.Object), _variables.Object, _log.Object);
        }

        _logOutput.ShouldContain("start execution");

        _executedScripts.ShouldContain("print 'upgrade database dbName from version 1.0 to 2.0'");

        _executedScripts.ShouldContain("create table dbo.DemoTable (Id INT)");
        _executedScripts.ShouldContain("print 'drop table DemoTable'");
        _executedScripts.ShouldContain("drop table dbo.DemoTable");

        _logOutput.ShouldContain("finish execution");
    }

    [Test]
    public void ExecuteExamplePgSql()
    {
        _sut.ClassName = "PgSql.SqlDatabaseScript";

        _variables
            .Setup(v => v.GetValue("DatabaseName"))
            .Returns("dbName");
        _variables
            .Setup(v => v.GetValue("CurrentVersion"))
            .Returns("1.0");
        _variables
            .Setup(v => v.GetValue("TargetVersion"))
            .Returns("2.0");

        _sut.DisplayName = "2.1_2.2.dll";
        _sut.ReadAssemblyContent = () => File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "2.1_2.2.dll"));

        using (new ConsoleListener(_log.Object))
        {
            _sut.Execute(new DbCommandStub(_command.Object), _variables.Object, _log.Object);
        }

        _logOutput.ShouldContain("start execution");

        _executedScripts.Count.ShouldBe(4);
        _executedScripts[0].ShouldContain("upgrade database dbName from version 1.0 to 2.0");
        _executedScripts[1].ShouldContain("create table public.demo_table (id integer)");
        _executedScripts[2].ShouldContain("'drop table demo_table'");
        _executedScripts[3].ShouldContain("drop table public.demo_table");

        _logOutput.ShouldContain("finish execution");
    }

    [Test]
    public void FailToResolveExecutor()
    {
        var domain = new Mock<ISubDomain>(MockBehavior.Strict);
        domain
            .Setup(d => d.ResolveScriptExecutor(_sut.ClassName, _sut.MethodName))
            .Returns(false);

        Assert.Throws<InvalidOperationException>(() => _sut.ResolveScriptExecutor(domain.Object));
    }

    [Test]
    public void FailOnExecute()
    {
        var domain = new Mock<ISubDomain>(MockBehavior.Strict);
        domain
            .Setup(d => d.Execute(_command.Object, _variables.Object))
            .Returns(false);

        Assert.Throws<InvalidOperationException>(() => _sut.Execute(domain.Object, _command.Object, _variables.Object));
    }

    [Test]
    public void ExecuteWhatIf()
    {
        var domain = new Mock<ISubDomain>(MockBehavior.Strict);

        _sut.Execute(domain.Object, null, _variables.Object);
    }

    [Test]
    public void GetDependencies()
    {
        _sut.ReadDescriptionContent = () => new MemoryStream(Encoding.Default.GetBytes("content"));

        var actual = _sut.GetDependencies();

        actual.ShouldNotBeNull();
        actual.ReadToEnd().ShouldBe("content");
    }

    [Test]
    public void GetDependenciesNoDescription()
    {
        _sut.ReadDescriptionContent = () => null;

        var actual = _sut.GetDependencies();

        actual.ShouldBeNull();
    }
}