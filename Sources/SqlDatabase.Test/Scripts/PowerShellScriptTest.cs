using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Scripts.PowerShellInternal;

namespace SqlDatabase.Scripts;

[TestFixture]
public class PowerShellScriptTest
{
    private PowerShellScript _sut = null!;
    private Mock<IPowerShell> _powerShell = null!;
    private Mock<IVariables> _variables = null!;
    private Mock<IDbCommand> _command = null!;
    private Mock<ILogger> _log = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _variables = new Mock<IVariables>(MockBehavior.Strict);
        _command = new Mock<IDbCommand>(MockBehavior.Strict);
        _powerShell = new Mock<IPowerShell>(MockBehavior.Strict);
        _log = new Mock<ILogger>(MockBehavior.Strict);

        var factory = new Mock<IPowerShellFactory>(MockBehavior.Strict);
        factory
            .Setup(f => f.Create())
            .Returns(_powerShell.Object);

        _sut = new PowerShellScript(null!, null!, null!, factory.Object);
    }

    [Test]
    public void Execute()
    {
        _powerShell
            .Setup(p => p.Invoke("script content", _log.Object, It.IsNotNull<KeyValuePair<string, object?>[]>()))
            .Callback<string, ILogger, KeyValuePair<string, object>[]>((_, _, parameters) =>
            {
                parameters.Length.ShouldBe(2);
                parameters[0].Key.ShouldBe(PowerShellScript.ParameterCommand);
                parameters[0].Value.ShouldBe(_command.Object);
                parameters[1].Key.ShouldBe(PowerShellScript.ParameterVariables);
                parameters[1].Value.ShouldBeOfType<VariablesProxy>();
            });

        _sut.ReadScriptContent = () => new MemoryStream(Encoding.UTF8.GetBytes("script content"));

        _sut.Execute(_command.Object, _variables.Object, _log.Object);

        _powerShell.VerifyAll();
    }

    [Test]
    public void ExecuteWhatIf()
    {
        _powerShell
            .Setup(p => p.SupportsShouldProcess("script content"))
            .Returns(true);
        _powerShell
            .Setup(p => p.Invoke("script content", _log.Object, It.IsNotNull<KeyValuePair<string, object?>[]>()))
            .Callback<string, ILogger, KeyValuePair<string, object>[]>((_, _, parameters) =>
            {
                parameters.Length.ShouldBe(3);
                parameters[0].Key.ShouldBe(PowerShellScript.ParameterCommand);
                parameters[0].Value.ShouldBeNull();
                parameters[1].Key.ShouldBe(PowerShellScript.ParameterVariables);
                parameters[1].Value.ShouldBeOfType<VariablesProxy>();
                parameters[2].Key.ShouldBe(PowerShellScript.ParameterWhatIf);
                parameters[2].Value.ShouldBeNull();
            });

        _sut.ReadScriptContent = () => new MemoryStream(Encoding.UTF8.GetBytes("script content"));

        _sut.Execute(null, _variables.Object, _log.Object);

        _powerShell.VerifyAll();
    }

    [Test]
    public void ExecuteIgnoreWhatIf()
    {
        _powerShell
            .Setup(p => p.SupportsShouldProcess("script content"))
            .Returns(false);
        _log
            .Setup(l => l.Info(It.IsNotNull<string>()));

        _sut.ReadScriptContent = () => new MemoryStream(Encoding.UTF8.GetBytes("script content"));

        _sut.Execute(null, _variables.Object, _log.Object);

        _powerShell.VerifyAll();
        _log.VerifyAll();
    }

    [Test]
    public void GetDependencies()
    {
        var description = Encoding.Default.GetBytes(@"
-- module dependency: a 1.0
-- module dependency: b 1.0");

        _sut.ReadDescriptionContent = () => new MemoryStream(description);

        var actual = _sut.GetDependencies();

        actual.ShouldBe(new[]
        {
            new ScriptDependency("a", new Version("1.0")),
            new ScriptDependency("b", new Version("1.0"))
        });
    }

    [Test]
    public void GetDependenciesNoDescription()
    {
        _sut.ReadDescriptionContent = () => null;

        var actual = _sut.GetDependencies();

        actual.ShouldBeEmpty();
    }
}