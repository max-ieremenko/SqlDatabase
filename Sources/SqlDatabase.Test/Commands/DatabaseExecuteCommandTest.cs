using System;
using Moq;
using NUnit.Framework;
using SqlDatabase.Adapter;
using SqlDatabase.Scripts;

namespace SqlDatabase.Commands;

[TestFixture]
public class DatabaseExecuteCommandTest
{
    private DatabaseExecuteCommand _sut = null!;
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
            .Setup(l => l.Info(It.IsAny<string>()))
            .Callback<string>(m =>
            {
                Console.WriteLine("Info: {0}", m);
            });

        _sut = new DatabaseExecuteCommand(
            _scriptSequence.Object,
            _scriptResolver.Object,
            _database.Object,
            _log.Object);
    }

    [Test]
    public void ExecuteOneScript()
    {
        var script1 = new Mock<IScript>(MockBehavior.Strict);
        script1.SetupGet(s => s.DisplayName).Returns("step 1");

        var script2 = new Mock<IScript>(MockBehavior.Strict);
        script2.SetupGet(s => s.DisplayName).Returns("step 2");

        var sequence = new[] { script1.Object, script2.Object };

        _scriptResolver
            .Setup(f => f.InitializeEnvironment(_log.Object, sequence));

        _database
            .Setup(d => d.Execute(script1.Object))
            .Callback(() => _database.Setup(d => d.Execute(script2.Object)));

        _scriptSequence.Setup(s => s.BuildSequence()).Returns(sequence);

        _sut.Execute();

        _database.VerifyAll();
        script1.VerifyAll();
        script2.VerifyAll();
        _scriptResolver.VerifyAll();
    }
}