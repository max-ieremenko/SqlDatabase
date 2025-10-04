#if !NET472
using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.TestApi;

namespace SqlDatabase.Adapter.AssemblyScripts.NetCore;

[TestFixture]
public partial class NetCoreSubDomainTest
{
    private NetCoreSubDomain _sut = null!;
    private Mock<IVariables> _variables = null!;
    private Mock<IDbCommand> _command = null!;

    private IList<string> _executedScripts = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _variables = new Mock<IVariables>(MockBehavior.Strict);

        var log = new Mock<ILogger>(MockBehavior.Strict);
        log
            .Setup(l => l.Info(It.IsAny<string>()))
            .Callback<string>(m => TestOutput.WriteLine("Info: {0}", m));
        log
            .Setup(l => l.Error(It.IsAny<string>()))
            .Callback<string>(m => TestOutput.WriteLine("Error: {0}", m));

        _executedScripts = new List<string>();
        _command = new Mock<IDbCommand>(MockBehavior.Strict);
        _command.SetupProperty(c => c.CommandText);
        _command
            .Setup(c => c.ExecuteNonQuery())
            .Callback(() => _executedScripts.Add(_command.Object.CommandText))
            .Returns(0);

        _sut = SubDomainFactory.Create(
                FrameworkVersion.Net8,
                log.Object,
                GetType().Assembly.Location,
                () => File.ReadAllBytes(GetType().Assembly.Location))
            .ShouldBeOfType<NetCoreSubDomain>();

        _sut.Initialize();
    }

    [TearDown]
    public void AfterEachTest()
    {
        _sut?.Unload();
        _sut?.Dispose();
    }

    [Test]
    public void ValidateScriptDomainAppBase()
    {
        _sut.ResolveScriptExecutor(nameof(StepWithCoreSubDomain), nameof(StepWithCoreSubDomain.ShowAppBase)).ShouldBeTrue();
        _sut.Execute(new DbCommandStub(_command.Object), _variables.Object);
        _sut.Unload();
        _sut.Dispose();

        _executedScripts.Count.ShouldBe(2);

        var assemblyFileName = _executedScripts[0];
        assemblyFileName.ShouldBeEmpty();

        var appBase = _executedScripts[1];
        appBase.ShouldBe(AppDomain.CurrentDomain.BaseDirectory);
    }
}
#endif