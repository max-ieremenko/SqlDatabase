using System;
using Moq;
using NUnit.Framework;
using SqlDatabase.Scripts;

namespace SqlDatabase
{
    [TestFixture]
    public class SequentialUpgradeTest
    {
        private SequentialUpgrade _sut;
        private Mock<IDatabase> _database;
        private Mock<IScriptSequence> _scriptSequence;

        [SetUp]
        public void BeforeEachTest()
        {
            _database = new Mock<IDatabase>(MockBehavior.Strict);
            _scriptSequence = new Mock<IScriptSequence>(MockBehavior.Strict);

            var log = new Mock<ILogger>(MockBehavior.Strict);
            log
                .Setup(l => l.Error(It.IsAny<string>()))
                .Callback<string>(m =>
                {
                    Console.WriteLine("Error: {0}", m);
                });
            log
                .Setup(l => l.Info(It.IsAny<string>()))
                .Callback<string>(m =>
                {
                    Console.WriteLine("Info: {0}", m);
                });

            _sut = new SequentialUpgrade
            {
                Database = _database.Object,
                Log = log.Object,
                ScriptSequence = _scriptSequence.Object
            };
        }

        [Test]
        public void DatabaseIsUptodate()
        {
            var currentVersion = new Version("1.0");

            _database.Setup(d => d.GetCurrentVersion()).Returns(currentVersion);
            _scriptSequence.Setup(s => s.BuildSequence(currentVersion)).Returns(new ScriptStep[0]);

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

            var stepTo2 = new ScriptStep(currentVersion, new Version("2.0"), updateTo2.Object);
            var stepTo3 = new ScriptStep(new Version("2.0"), new Version("3.0"), updateTo3.Object);

            _database.Setup(d => d.BeforeUpgrade());
            _database.Setup(d => d.GetCurrentVersion()).Returns(currentVersion);
            _database
                .Setup(d => d.ExecuteUpgrade(updateTo2.Object, stepTo2.From, stepTo2.To))
                .Callback(() => _database.Setup(d => d.ExecuteUpgrade(updateTo3.Object, stepTo3.From, stepTo3.To)));

            _scriptSequence.Setup(s => s.BuildSequence(currentVersion)).Returns(new[] { stepTo2, stepTo3 });

            _sut.Execute();

            _database.VerifyAll();
            _scriptSequence.VerifyAll();
        }

        [Test]
        public void StopExecutionOnError()
        {
            var currentVersion = new Version("1.0");

            var updateTo2 = new Mock<IScript>(MockBehavior.Strict);
            updateTo2.SetupGet(s => s.DisplayName).Returns("2.0");

            var updateTo3 = new Mock<IScript>(MockBehavior.Strict);
            updateTo3.SetupGet(s => s.DisplayName).Returns("3.0");

            var stepTo2 = new ScriptStep(currentVersion, new Version("2.0"), updateTo2.Object);
            var stepTo3 = new ScriptStep(new Version("2.0"), new Version("3.0"), updateTo3.Object);

            _database.Setup(d => d.BeforeUpgrade());
            _database.Setup(d => d.GetCurrentVersion()).Returns(currentVersion);
            _database.Setup(d => d.ExecuteUpgrade(updateTo2.Object, stepTo2.From, stepTo2.To)).Throws<InvalidOperationException>();

            _scriptSequence.Setup(s => s.BuildSequence(currentVersion)).Returns(new[] { stepTo2, stepTo3 });

            Assert.Throws<InvalidOperationException>(_sut.Execute);

            _database.VerifyAll();
            _scriptSequence.VerifyAll();
        }
    }
}