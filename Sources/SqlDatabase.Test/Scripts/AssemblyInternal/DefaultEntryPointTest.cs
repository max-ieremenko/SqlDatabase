using System;
using System.Collections.Generic;
using System.Data;
using Moq;
using NUnit.Framework;

namespace SqlDatabase.Scripts.AssemblyInternal
{
    [TestFixture]
    public class DefaultEntryPointTest
    {
        private DefaultEntryPoint _sut;
        private IList<string> _logOutput;
        private Mock<IDbCommand> _command;
        private Mock<IReadOnlyDictionary<string, string>> _variables;

        [SetUp]
        public void BeforeEachTest()
        {
            _command = new Mock<IDbCommand>(MockBehavior.Strict);
            _variables = new Mock<IReadOnlyDictionary<string, string>>(MockBehavior.Strict);

            _logOutput = new List<string>();

            var log = new Mock<ILogger>(MockBehavior.Strict);
            log
                .Setup(l => l.Info(It.IsAny<string>()))
                .Callback<string>(m =>
                {
                    Console.WriteLine("Info: {0}", m);
                    _logOutput.Add(m);
                });
            log
                .Setup(l => l.Error(It.IsAny<string>()))
                .Callback<string>(m =>
                {
                    Console.WriteLine("Error: {0}", m);
                    _logOutput.Add(m);
                });

            _sut = new DefaultEntryPoint { Log = log.Object };
        }

        [Test]
        public void Execute()
        {
            var executeCounter = 0;

            _sut.Method = (c, v) =>
            {
                Assert.AreEqual(_command.Object, c);
                Assert.AreEqual(_variables.Object, v);

                executeCounter++;
            };

            Assert.IsTrue(_sut.Execute(_command.Object, _variables.Object));

            Assert.AreEqual(1, executeCounter);
            Assert.AreEqual(0, _logOutput.Count);
        }

        [Test]
        public void DisposeInstanceOnExecute()
        {
            var instance = new Mock<IDisposable>(MockBehavior.Strict);
            instance.Setup(i => i.Dispose());

            _sut.ScriptInstance = instance.Object;
            _sut.Method = (c, v) =>
            {
            };

            Assert.IsTrue(_sut.Execute(_command.Object, _variables.Object));

            instance.VerifyAll();
        }

        [Test]
        public void ExceptionOnExecute()
        {
            _sut.Method = (c, v) => throw new InvalidOperationException();

            Assert.IsFalse(_sut.Execute(_command.Object, _variables.Object));

            Assert.Greater(_logOutput.Count, 0);
        }
    }
}
