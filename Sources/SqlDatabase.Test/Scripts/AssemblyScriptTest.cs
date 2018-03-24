using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Moq;
using NUnit.Framework;

namespace SqlDatabase.Scripts
{
    [TestFixture]
    public partial class AssemblyScriptTest
    {
        private AssemblyScript _sut;
        private Variables _variables;
        private Mock<ILogger> _log;
        private Mock<IDbCommand> _command;

        private IList<string> _logOutput;
        private IList<string> _executedScripts;

        [SetUp]
        public void BeforeEachTest()
        {
            _variables = new Variables();

            _logOutput = new List<string>();
            _log = new Mock<ILogger>(MockBehavior.Strict);
            _log
                .Setup(l => l.Info(It.IsAny<string>()))
                .Callback<string>(m =>
                {
                    Console.WriteLine("Info: {0}", m);
                    _logOutput.Add(m);
                });

            _executedScripts = new List<string>();
            _command = new Mock<IDbCommand>(MockBehavior.Strict);
            _command.SetupProperty(c => c.CommandText);
            _command
                .Setup(c => c.ExecuteNonQuery())
                .Callback(() => _executedScripts.Add(_command.Object.CommandText))
                .Returns(0);

            _sut = new AssemblyScript
            {
                DisplayName = "2.1_2.2.dll",
                ReadAssemblyContent = () => File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "2.1_2.2.dll"))
            };
        }

        [Test]
        public void ExecuteExample()
        {
            _variables.DatabaseName = "dbName";
            _variables.CurrentVersion = "1.0";
            _variables.TargetVersion = "2.0";

            _sut.Execute(new DbCommandProxy(_command.Object), _variables, _log.Object);

            Assert.IsTrue(_logOutput.Contains("start execution"));

            Assert.IsTrue(_executedScripts.Contains("print 'current database name is dbName'"));
            Assert.IsTrue(_executedScripts.Contains("print 'version from 1.0'"));
            Assert.IsTrue(_executedScripts.Contains("print 'version to 2.0'"));

            Assert.IsTrue(_executedScripts.Contains("create table dbo.DemoTable (Id INT)"));
            Assert.IsTrue(_executedScripts.Contains("print 'drop table DemoTable'"));
            Assert.IsTrue(_executedScripts.Contains("drop table dbo.DemoTable"));

            Assert.IsTrue(_logOutput.Contains("finish execution"));
        }
    }
}
