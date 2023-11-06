using System;
using Moq;
using NUnit.Framework;
using SqlDatabase.Adapter;
using SqlDatabase.Scripts;

namespace SqlDatabase.Commands;

[TestFixture]
public class DatabaseUpgradeCommandTest
{
    private DatabaseUpgradeCommand _sut = null!;
    private Mock<IDatabase> _database = null!;
    private Mock<IUpgradeScriptSequence> _scriptSequence = null!;
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

        _scriptSequence = new Mock<IUpgradeScriptSequence>(MockBehavior.Strict);

        _scriptResolver = new Mock<IScriptResolver>(MockBehavior.Strict);

        _log = new Mock<ILogger>(MockBehavior.Strict);
        _log.Setup(l => l.Indent()).Returns((IDisposable)null!);
        _log
            .Setup(l => l.Error(It.IsAny<string>()))
            .Callback<string>(m =>
            {
                Console.WriteLine("Error: {0}", m);
            });
        _log
            .Setup(l => l.Info(It.IsAny<string>()))
            .Callback<string>(m =>
            {
                Console.WriteLine("Info: {0}", m);
            });

        _sut = new DatabaseUpgradeCommand(
            _scriptSequence.Object,
            _scriptResolver.Object,
            _database.Object,
            _log.Object);
    }

    [Test]
    public void DatabaseIsUpToDate()
    {
        _scriptSequence.Setup(s => s.BuildSequence()).Returns(Array.Empty<ScriptStep>());

        _sut.Execute();

        _database.VerifyAll();
        _scriptSequence.VerifyAll();
    }

    [Test]
    public void ExecuteSequence()
    {
        var currentVersion = new Version("1.0");

        var updateTo2 = new Mock<IScript>(MockBehavior.Strict);
        updateTo2.SetupGet(s => s.DisplayName).Returns("2.0");

        var updateTo3 = new Mock<IScript>(MockBehavior.Strict);
        updateTo3.SetupGet(s => s.DisplayName).Returns("3.0");

        var stepTo2 = new ScriptStep("module1", currentVersion, new Version("2.0"), updateTo2.Object);
        var stepTo3 = new ScriptStep("module2", new Version("2.0"), new Version("3.0"), updateTo3.Object);

        _scriptResolver
            .Setup(f => f.InitializeEnvironment(_log.Object, new[] { stepTo2.Script, stepTo3.Script }));

        _database
            .Setup(d => d.Execute(updateTo2.Object, "module1", stepTo2.From, stepTo2.To))
            .Callback(() => _database.Setup(d => d.Execute(updateTo3.Object, "module2", stepTo3.From, stepTo3.To)));

        _scriptSequence.Setup(s => s.BuildSequence()).Returns(new[] { stepTo2, stepTo3 });

        _sut.Execute();

        _database.VerifyAll();
        _scriptSequence.VerifyAll();
        _scriptResolver.VerifyAll();
    }

    [Test]
    public void StopExecutionOnError()
    {
        var currentVersion = new Version("1.0");

        var updateTo2 = new Mock<IScript>(MockBehavior.Strict);
        updateTo2.SetupGet(s => s.DisplayName).Returns("2.0");

        var updateTo3 = new Mock<IScript>(MockBehavior.Strict);
        updateTo3.SetupGet(s => s.DisplayName).Returns("3.0");

        var stepTo2 = new ScriptStep(string.Empty, currentVersion, new Version("2.0"), updateTo2.Object);
        var stepTo3 = new ScriptStep(string.Empty, new Version("2.0"), new Version("3.0"), updateTo3.Object);

        _scriptResolver
            .Setup(f => f.InitializeEnvironment(_log.Object, new[] { stepTo2.Script, stepTo3.Script }));

        _database.Setup(d => d.Execute(updateTo2.Object, string.Empty, stepTo2.From, stepTo2.To)).Throws<InvalidOperationException>();

        _scriptSequence.Setup(s => s.BuildSequence()).Returns(new[] { stepTo2, stepTo3 });

        Assert.Throws<InvalidOperationException>(_sut.Execute);

        _database.VerifyAll();
        _scriptSequence.VerifyAll();
        _scriptResolver.VerifyAll();
    }
}