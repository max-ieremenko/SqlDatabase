using System.Runtime.InteropServices;
using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.TestApi;

namespace SqlDatabase.Adapter.PowerShellScripts;

[TestFixture]
public class PowerShellTest
{
    private IPowerShellFactory _factory = null!;
    private IPowerShell _sut = null!;
    private Mock<ILogger> _logger = null!;
    private Mock<IVariables> _variables = null!;
    private Mock<IDbCommand> _command = null!;
    private List<string> _logOutput = null!;

    [OneTimeSetUp]
    public void BeforeAllTests()
    {
        _logOutput = new List<string>();

        _logger = new Mock<ILogger>(MockBehavior.Strict);
        _logger
            .Setup(l => l.Info(It.IsNotNull<string>()))
            .Callback<string>(m => _logOutput.Add("info: " + m));
        _logger
            .Setup(l => l.Error(It.IsNotNull<string>()))
            .Callback<string>(m => _logOutput.Add("error: " + m));
        _logger
            .Setup(l => l.Indent())
            .Returns((IDisposable)null!);

#if NET472
        var version = FrameworkVersion.Net472;
#elif NET8_0
        var version = FrameworkVersion.Net8;
#else
        var version = FrameworkVersion.Net9;
#endif

        var runtime = new HostedRuntime(false, RuntimeInformation.IsOSPlatform(OSPlatform.Windows), version);

        _factory = new PowerShellFactory(runtime, null);
        _factory.Initialize(_logger.Object);
    }

    [SetUp]
    public void BeforeEachTest()
    {
        _logOutput.Clear();

        _variables = new Mock<IVariables>(MockBehavior.Strict);
        _command = new Mock<IDbCommand>(MockBehavior.Strict);

        _sut = _factory.Create();
    }

    [TearDown]
    public void AfterEachTest()
    {
        foreach (var line in _logOutput)
        {
            TestOutput.WriteLine(line);
        }
    }

    [Test]
    [TestCase("PowerShellTest.ExecuteWhatIfInvoke.ps1", true)]
    [TestCase("PowerShellTest.ExecuteWhatIfIgnore.ps1", false)]
    public void SupportsShouldProcess(string scriptName, bool expected)
    {
        var script = LoadScript(scriptName);

        _sut.SupportsShouldProcess(script).ShouldBe(expected);
    }

    [Test]
    public void HandleOutput()
    {
        var script = LoadScript("PowerShellTest.HandleOutput.ps1");
        InvokeExecute(script, false);

        _logOutput.Count.ShouldBe(3);
        _logOutput[0].ShouldBe("info: hello from Write-Host");
        _logOutput[1].ShouldBe("info: hello from Write-Information");
        _logOutput[2].ShouldBe("info: hello from Write-Warning");
    }

    [Test]
    public void HandleWriteError()
    {
        var script = LoadScript("PowerShellTest.HandleWriteError.ps1");
        Assert.Throws<InvalidOperationException>(() => InvokeExecute(script, false));

        _logOutput.Count.ShouldBe(1);
        _logOutput[0].ShouldBe("error: hello from Write-Error");
    }

    [Test]
    public void HandleThrow()
    {
        var script = LoadScript("PowerShellTest.HandleThrow.ps1");

        var failed = false;
        try
        {
            InvokeExecute(script, false);
        }
        catch (Exception ex)
        {
            failed = true;
            TestOutput.WriteLine(ex);
        }

        failed.ShouldBeTrue();
        _logOutput.ShouldBeEmpty();
    }

    [Test]
    public void ParametersBinding()
    {
        _command
            .SetupProperty(c => c.CommandText);
        _command
            .Setup(c => c.ExecuteNonQuery())
            .Returns(0);

        _variables
            .Setup(v => v.GetValue("DatabaseName"))
            .Returns("test-db");

        var script = LoadScript("PowerShellTest.ParametersBinding.ps1");

        InvokeExecute(script, false);

        _command.Object.CommandText.ShouldBe("database name is test-db");
        _command.VerifyAll();
    }

    [Test]
    public void ExecuteInvalidScript()
    {
        Assert.Throws<InvalidOperationException>(() => InvokeExecute("bla bla", false));

        _logOutput.Count.ShouldBe(1);
        _logOutput[0].ShouldStartWith("error: The term 'bla' is not recognized");
    }

    [Test]
    public void ExecuteEmptyScript()
    {
        InvokeExecute(string.Empty, false);

        _logOutput.ShouldBeEmpty();
    }

    [Test]
    public void ExecuteWhatIfInvoke()
    {
        var script = LoadScript("PowerShellTest.ExecuteWhatIfInvoke.ps1");

        InvokeExecute(script, true);

        _logOutput.Count.ShouldBe(1);
        _logOutput[0].ShouldBe("info: WhatIf accepted");
    }

    private static string LoadScript(string name)
    {
        var anchor = typeof(PowerShellTest);
        using (var stream = anchor.Assembly.GetManifestResourceStream(anchor, name))
        {
            stream.ShouldNotBeNull(name);

            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }

    private void InvokeExecute(string script, bool whatIf)
    {
        var parameters = new KeyValuePair<string, object?>[2 + (whatIf ? 1 : 0)];
        parameters[0] = new KeyValuePair<string, object?>(PowerShellScript.ParameterCommand, _command.Object);
        parameters[1] = new KeyValuePair<string, object?>(PowerShellScript.ParameterVariables, new VariablesProxy(_variables.Object));
        if (whatIf)
        {
            parameters[2] = new KeyValuePair<string, object?>(PowerShellScript.ParameterWhatIf, null);
        }

        _sut.Invoke(script, _logger.Object, parameters);
    }
}