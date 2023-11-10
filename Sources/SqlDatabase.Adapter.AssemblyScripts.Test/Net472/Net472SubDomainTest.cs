#if NET472
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.TestApi;

namespace SqlDatabase.Adapter.AssemblyScripts.Net472;

[TestFixture]
public partial class Net472SubDomainTest
{
    private Net472SubDomain _sut = null!;
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
            log.Object,
            GetType().Assembly.Location,
            () => File.ReadAllBytes(GetType().Assembly.Location))
            .ShouldBeOfType<Net472SubDomain>();

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
        _sut.ResolveScriptExecutor(nameof(StepWithSubDomain), nameof(StepWithSubDomain.ShowAppBase)).ShouldBeTrue();
        _sut.Execute(new DbCommandStub(_command.Object), _variables.Object);
        _sut.Unload();
        _sut.Dispose();

        _executedScripts.Count.ShouldBe(2);

        var assemblyFileName = _executedScripts[0];
        FileAssert.DoesNotExist(assemblyFileName);
        Path.GetFileName(GetType().Assembly.Location).ShouldBe(Path.GetFileName(assemblyFileName));

        var appBase = _executedScripts[1];
        DirectoryAssert.DoesNotExist(appBase);
        Path.GetDirectoryName(assemblyFileName).ShouldBe(appBase);
    }

    [Test]
    public void ValidateScriptDomainConfiguration()
    {
        _sut.ResolveScriptExecutor(nameof(StepWithSubDomain), nameof(StepWithSubDomain.ShowConfiguration)).ShouldBeTrue();
        _sut.Execute(new DbCommandStub(_command.Object), _variables.Object);
        _sut.Unload();
        _sut.Dispose();

        _executedScripts.Count.ShouldBe(2);

        var configurationFile = _executedScripts[0];
        configurationFile.ShouldBe(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

        var connectionString = _executedScripts[1];
        connectionString.ShouldBe("do something");
    }

    [Test]
    public void ValidateScriptDomainCreateSubDomain()
    {
        _sut.ResolveScriptExecutor(nameof(StepWithSubDomain), nameof(StepWithSubDomain.Execute)).ShouldBeTrue();
        _sut.Execute(new DbCommandStub(_command.Object), _variables.Object);

        _executedScripts.Count.ShouldBe(1);
        _executedScripts[0].ShouldBe("hello");
    }
}
#endif