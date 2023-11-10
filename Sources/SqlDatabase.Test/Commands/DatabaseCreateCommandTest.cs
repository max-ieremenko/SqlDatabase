using System;
using Moq;
using NUnit.Framework;
using SqlDatabase.Adapter;
using SqlDatabase.Configuration;
using SqlDatabase.Scripts;
using SqlDatabase.Sequence;
using SqlDatabase.TestApi;

namespace SqlDatabase.Commands;

[TestFixture]
public class DatabaseCreateCommandTest
{
    private DatabaseCreateCommand _sut = null!;
    private Mock<IDatabase> _database = null!;
    private Mock<ICreateScriptSequence> _scriptSequence = null!;
    private Mock<IScriptResolver> _scriptResolver = null!;
    private Mock<ILogger> _log = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        var adapter = new Mock<IDatabaseAdapter>(MockBehavior.Strict);
        adapter
            .Setup(a => a.GetUserFriendlyConnectionString())
            .Returns("greet");

        _database = new Mock<IDatabase>(MockBehavior.Strict);
        _database.SetupGet(d => d.Adapter).Returns(adapter.Object);
        _database.Setup(d => d.GetServerVersion()).Returns("sql server 1.0");

        _scriptSequence = new Mock<ICreateScriptSequence>(MockBehavior.Strict);

        _scriptResolver = new Mock<IScriptResolver>(MockBehavior.Strict);

        _log = new Mock<ILogger>(MockBehavior.Strict);
        _log.Setup(l => l.Indent()).Returns((IDisposable)null!);
        _log
            .Setup(l => l.Error(It.IsAny<string>()))
            .Callback<string>(m =>
            {
                TestOutput.WriteLine("Error: {0}", m);
            });
        _log
            .Setup(l => l.Info(It.IsAny<string>()))
            .Callback<string>(m =>
            {
                TestOutput.WriteLine("Info: {0}", m);
            });

        _sut = new DatabaseCreateCommand(
            _scriptSequence.Object,
            _scriptResolver.Object,
            _database.Object,
            _log.Object);
    }

    [Test]
    public void ScriptsNotFound()
    {
        _scriptSequence.Setup(s => s.BuildSequence()).Returns(Array.Empty<IScript>());

        Assert.Throws<ConfigurationErrorsException>(_sut.Execute);

        _scriptSequence.VerifyAll();
    }

    [Test]
    public void ExecuteSequence()
    {
        var step1 = new Mock<IScript>(MockBehavior.Strict);
        step1.SetupGet(s => s.DisplayName).Returns("step 1");

        var step2 = new Mock<IScript>(MockBehavior.Strict);
        step2.SetupGet(s => s.DisplayName).Returns("step 2");

        var sequence = new[] { step1.Object, step2.Object };

        _scriptResolver
            .Setup(f => f.InitializeEnvironment(_log.Object, sequence));

        _database
            .Setup(d => d.Execute(step1.Object))
            .Callback(() => _database.Setup(d => d.Execute(step2.Object)));

        _scriptSequence
            .Setup(s => s.BuildSequence())
            .Returns(sequence);

        _sut.Execute();

        _database.VerifyAll();
        _scriptSequence.VerifyAll();
        _scriptResolver.VerifyAll();
    }

    [Test]
    public void StopExecutionOnError()
    {
        var step1 = new Mock<IScript>(MockBehavior.Strict);
        step1.SetupGet(s => s.DisplayName).Returns("step 1");

        var step2 = new Mock<IScript>(MockBehavior.Strict);
        step2.SetupGet(s => s.DisplayName).Returns("step 2");

        var sequence = new[] { step1.Object, step2.Object };

        _scriptResolver
            .Setup(f => f.InitializeEnvironment(_log.Object, sequence));

        _database
            .Setup(d => d.Execute(step1.Object))
            .Throws<InvalidOperationException>();

        _scriptSequence
            .Setup(s => s.BuildSequence())
            .Returns(sequence);

        Assert.Throws<InvalidOperationException>(_sut.Execute);

        _database.VerifyAll();
        _scriptSequence.VerifyAll();
        _scriptResolver.VerifyAll();
    }
}