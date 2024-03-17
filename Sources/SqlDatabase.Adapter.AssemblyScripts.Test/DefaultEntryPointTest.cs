using System;
using System.Collections.Generic;
using System.Data;
using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.TestApi;

namespace SqlDatabase.Adapter.AssemblyScripts;

[TestFixture]
public class DefaultEntryPointTest
{
    private DefaultEntryPoint _sut = null!;
    private IList<string> _logOutput = null!;
    private Mock<IDbCommand> _command = null!;
    private Mock<IReadOnlyDictionary<string, string?>> _variables = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _command = new Mock<IDbCommand>(MockBehavior.Strict);
        _variables = new Mock<IReadOnlyDictionary<string, string?>>(MockBehavior.Strict);

        _logOutput = new List<string>();

        var log = new Mock<ILogger>(MockBehavior.Strict);
        log
            .Setup(l => l.Info(It.IsAny<string>()))
            .Callback<string>(m =>
            {
                TestOutput.WriteLine("Info: {0}", m);
                _logOutput.Add(m);
            });
        log
            .Setup(l => l.Error(It.IsAny<string>()))
            .Callback<string>(m =>
            {
                TestOutput.WriteLine("Error: {0}", m);
                _logOutput.Add(m);
            });

        _sut = new DefaultEntryPoint(log.Object, null!, null!);
    }

    [Test]
    public void Execute()
    {
        var executeCounter = 0;

        _sut.Method = (c, v) =>
        {
            _command.Object.ShouldBe(c);
            _variables.Object.ShouldBe(v);

            executeCounter++;
        };

        _sut.Execute(_command.Object, _variables.Object).ShouldBeTrue();

        executeCounter.ShouldBe(1);
        _logOutput.ShouldBeEmpty();
    }

    [Test]
    public void DisposeInstanceOnExecute()
    {
        var instance = new Mock<IDisposable>(MockBehavior.Strict);
        instance.Setup(i => i.Dispose());

        _sut.ScriptInstance = instance.Object;
        _sut.Method = (_, _) =>
        {
        };

        _sut.Execute(_command.Object, _variables.Object).ShouldBeTrue();

        instance.VerifyAll();
    }

    [Test]
    public void ExceptionOnExecute()
    {
        _sut.Method = (_, _) => throw new InvalidOperationException();

        _sut.Execute(_command.Object, _variables.Object).ShouldBeFalse();

        _logOutput.ShouldNotBeEmpty();
    }
}