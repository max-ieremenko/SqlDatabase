using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;

namespace SqlDatabase.Scripts.AssemblyInternal
{
    [TestFixture]
    public partial class EntryPointResolverTest
    {
        private EntryPointResolver _sut;
        private IList<string> _logErrorOutput;

        [SetUp]
        public void BeforeEachTest()
        {
            _logErrorOutput = new List<string>();

            var log = new Mock<ILogger>(MockBehavior.Strict);
            log
                .Setup(l => l.Info(It.IsAny<string>()))
                .Callback<string>(m =>
                {
                    Console.WriteLine("Info: {0}", m);
                });
            log
                .Setup(l => l.Error(It.IsAny<string>()))
                .Callback<string>(m =>
                {
                    Console.WriteLine("Error: {0}", m);
                    _logErrorOutput.Add(m);
                });

            _sut = new EntryPointResolver { Log = log.Object };
        }

        [Test]
        public void ResolveFromExample()
        {
            _sut.ExecutorClassName = nameof(ExampleSqlDatabaseScript);
            _sut.ExecutorMethodName = nameof(ExampleSqlDatabaseScript.Execute);

            var actual = _sut.Resolve(GetType().Assembly);
            CollectionAssert.IsEmpty(_logErrorOutput);
            Assert.IsInstanceOf<DefaultEntryPoint>(actual);

            var entryPoint = (DefaultEntryPoint)actual;
            Assert.IsNotNull(entryPoint.Log);
            Assert.IsInstanceOf<ExampleSqlDatabaseScript>(entryPoint.ScriptInstance);
            Assert.AreEqual(nameof(ExampleSqlDatabaseScript.Execute), entryPoint.Method.Method.Name);
        }

        [Test]
        public void FailToCreateInstance()
        {
            _sut.ExecutorClassName = nameof(DatabaseScriptWithInvalidConstructor);
            _sut.ExecutorMethodName = nameof(DatabaseScriptWithInvalidConstructor.Execute);

            Assert.IsNull(_sut.Resolve(GetType().Assembly));
            CollectionAssert.IsNotEmpty(_logErrorOutput);
        }
    }
}
