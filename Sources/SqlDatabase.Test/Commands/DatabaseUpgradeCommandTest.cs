using System;
using Moq;
using NUnit.Framework;
using SqlDatabase.Scripts;

namespace SqlDatabase.Commands
{
    [TestFixture]
    public class DatabaseUpgradeCommandTest
    {
        private DatabaseUpgradeCommand _sut;
        private Mock<IDatabase> _database;
        private Mock<IUpgradeScriptSequence> _scriptSequence;
        private Mock<IPowerShellFactory> _powerShellFactory;
        private Mock<ILogger> _log;

        [SetUp]
        public void BeforeEachTest()
        {
            _database = new Mock<IDatabase>(MockBehavior.Strict);
            _database.SetupGet(d => d.ConnectionString).Returns(@"Data Source=unknownServer;Initial Catalog=unknownDatabase");
            _database.Setup(d => d.GetServerVersion()).Returns("sql server 1.0");

            _scriptSequence = new Mock<IUpgradeScriptSequence>(MockBehavior.Strict);

            _powerShellFactory = new Mock<IPowerShellFactory>(MockBehavior.Strict);

            _log = new Mock<ILogger>(MockBehavior.Strict);
            _log.Setup(l => l.Indent()).Returns((IDisposable)null);
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

            _sut = new DatabaseUpgradeCommand
            {
                Database = _database.Object,
                Log = _log.Object,
                ScriptSequence = _scriptSequence.Object,
                PowerShellFactory = _powerShellFactory.Object
            };
        }

        [Test]
        public void DatabaseIsUpToDate()
        {
            _scriptSequence.Setup(s => s.BuildSequence()).Returns(new ScriptStep[0]);

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

            _powerShellFactory
                .Setup(f => f.InitializeIfRequested(_log.Object));

            _database
                .Setup(d => d.Execute(updateTo2.Object, "module1", stepTo2.From, stepTo2.To))
                .Callback(() => _database.Setup(d => d.Execute(updateTo3.Object, "module2", stepTo3.From, stepTo3.To)));

            _scriptSequence.Setup(s => s.BuildSequence()).Returns(new[] { stepTo2, stepTo3 });

            _sut.Execute();

            _database.VerifyAll();
            _scriptSequence.VerifyAll();
            _powerShellFactory.VerifyAll();
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

            _powerShellFactory
                .Setup(f => f.InitializeIfRequested(_log.Object));

            _database.Setup(d => d.Execute(updateTo2.Object, string.Empty, stepTo2.From, stepTo2.To)).Throws<InvalidOperationException>();

            _scriptSequence.Setup(s => s.BuildSequence()).Returns(new[] { stepTo2, stepTo3 });

            Assert.Throws<InvalidOperationException>(_sut.Execute);

            _database.VerifyAll();
            _scriptSequence.VerifyAll();
            _powerShellFactory.VerifyAll();
        }
    }
}
