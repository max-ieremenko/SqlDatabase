using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Configuration;
using SqlDatabase.Scripts.AssemblyInternal;

namespace SqlDatabase.Scripts
{
    [TestFixture]
    public class AssemblyScriptTest
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
                .Callback<string>(_logOutput.Add);

            _executedScripts = new List<string>();
            _command = new Mock<IDbCommand>(MockBehavior.Strict);
            _command.SetupProperty(c => c.CommandText);
            _command
                .Setup(c => c.ExecuteNonQuery())
                .Callback(() => _executedScripts.Add(_command.Object.CommandText))
                .Returns(0);

            _sut = new AssemblyScript();
            _sut.Configuration = new AssemblyScriptConfiguration();
        }

        [Test]
        public void ExecuteExample()
        {
            _variables.DatabaseName = "dbName";
            _variables.CurrentVersion = "1.0";
            _variables.TargetVersion = "2.0";

            _sut.DisplayName = "2.1_2.2.dll";
            _sut.ReadAssemblyContent = () => File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "2.1_2.2.dll"));

#if !NET452
            using (new ConsoleListener(_log.Object))
#endif
            {
                _sut.Execute(new DbCommandStub(_command.Object), _variables, _log.Object);
            }

            _logOutput.ShouldContain("start execution");

            _executedScripts.ShouldContain("print 'current database name is dbName'");
            _executedScripts.ShouldContain("print 'version from 1.0'");
            _executedScripts.ShouldContain("print 'version to 2.0'");

            _executedScripts.ShouldContain("create table dbo.DemoTable (Id INT)");
            _executedScripts.ShouldContain("print 'drop table DemoTable'");
            _executedScripts.ShouldContain("drop table dbo.DemoTable");

            _logOutput.ShouldContain("finish execution");
        }

        [Test]
        public void FailToResolveExecutor()
        {
            var domain = new Mock<ISubDomain>(MockBehavior.Strict);
            domain
                .Setup(d => d.ResolveScriptExecutor(_sut.Configuration.ClassName, _sut.Configuration.MethodName))
                .Returns(false);

            Assert.Throws<InvalidOperationException>(() => _sut.ResolveScriptExecutor(domain.Object));
        }

        [Test]
        public void FailOnExecute()
        {
            var domain = new Mock<ISubDomain>(MockBehavior.Strict);
            domain
                .Setup(d => d.Execute(_command.Object, _variables))
                .Returns(false);

            Assert.Throws<InvalidOperationException>(() => _sut.Execute(domain.Object, _command.Object, _variables));
        }

        [Test]
        public void ExecuteWhatIf()
        {
            var domain = new Mock<ISubDomain>(MockBehavior.Strict);

            _sut.Execute(domain.Object, null, _variables);
        }

        [Test]
        public void GetDependencies()
        {
            _sut.ReadDescriptionContent = () => Encoding.Default.GetBytes(@"
-- module dependency: a 1.0
-- module dependency: b 1.0");

            var actual = _sut.GetDependencies();

            actual.ShouldBe(new[]
            {
                new ScriptDependency("a", new Version("1.0")),
                new ScriptDependency("b", new Version("1.0"))
            });
        }

        [Test]
        public void GetDependenciesNoDescription()
        {
            _sut.ReadDescriptionContent = () => null;

            var actual = _sut.GetDependencies();

            actual.ShouldBeEmpty();
        }
    }
}
