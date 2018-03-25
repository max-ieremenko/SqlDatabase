using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using SqlDatabase.Scripts;

namespace SqlDatabase
{
    [TestFixture]
    public class SequentialCreateTest
    {
        private SequentialCreate _sut;
        private Mock<ICreateDatabase> _database;
        private Mock<ICreateScriptSequence> _scriptSequence;
        private IList<string> _logOutput;

        [SetUp]
        public void BeforeEachTest()
        {
            _database = new Mock<ICreateDatabase>(MockBehavior.Strict);
            _scriptSequence = new Mock<ICreateScriptSequence>(MockBehavior.Strict);

            _logOutput = new List<string>();
            var log = new Mock<ILogger>(MockBehavior.Strict);
            log
                .Setup(l => l.Error(It.IsAny<string>()))
                .Callback<string>(m =>
                {
                    Console.WriteLine("Error: {0}", m);
                    _logOutput.Add(m);
                });
            log
                .Setup(l => l.Info(It.IsAny<string>()))
                .Callback<string>(m =>
                {
                    Console.WriteLine("Info: {0}", m);
                    _logOutput.Add(m);
                });

            _sut = new SequentialCreate
            {
                Database = _database.Object,
                Log = log.Object,
                ScriptSequence = _scriptSequence.Object
            };
        }

        [Test]
        public void ScriptsNotFound()
        {
            _scriptSequence.Setup(s => s.BuildSequence()).Returns(new IScript[0]);

            _sut.Execute();

            _scriptSequence.VerifyAll();
            Assert.AreEqual(1, _logOutput.Count);
        }

        [Test]
        public void ExecuteSequence()
        {
            var step1 = new Mock<IScript>(MockBehavior.Strict);
            step1.SetupGet(s => s.DisplayName).Returns("step 1");

            var step2 = new Mock<IScript>(MockBehavior.Strict);
            step2.SetupGet(s => s.DisplayName).Returns("step 2");

            _database.Setup(d => d.BeforeCreate());
            _database
                .Setup(d => d.Execute(step1.Object))
                .Callback(() => _database.Setup(d => d.Execute(step2.Object)));

            _scriptSequence.Setup(s => s.BuildSequence()).Returns(new[] { step1.Object, step2.Object });

            _sut.Execute();

            _database.VerifyAll();
            _scriptSequence.VerifyAll();
        }

        [Test]
        public void StopExecutionOnError()
        {
            var step1 = new Mock<IScript>(MockBehavior.Strict);
            step1.SetupGet(s => s.DisplayName).Returns("step 1");

            var step2 = new Mock<IScript>(MockBehavior.Strict);
            step2.SetupGet(s => s.DisplayName).Returns("step 2");

            _database.Setup(d => d.BeforeCreate());
            _database
                .Setup(d => d.Execute(step1.Object))
                .Throws<InvalidOperationException>();

            _scriptSequence.Setup(s => s.BuildSequence()).Returns(new[] { step1.Object, step2.Object });

            Assert.Throws<InvalidOperationException>(_sut.Execute);

            _database.VerifyAll();
            _scriptSequence.VerifyAll();
        }
    }
}
