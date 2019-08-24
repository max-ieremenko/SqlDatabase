using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace SqlDatabase.Scripts
{
    [TestFixture]
    public class TextScriptTest
    {
        private Mock<ILogger> _logger;
        private Variables _variables;
        private Mock<IDbCommand> _command;
        private TextScript _sut;

        private IList<string> _logOutput;
        private IList<string> _executedScripts;
        private Mock<IDataReader> _executedReader;

        [SetUp]
        public void BeforeEachTest()
        {
            _variables = new Variables();

            _logOutput = new List<string>();
            _logger = new Mock<ILogger>(MockBehavior.Strict);
            _logger
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

            _executedReader = new Mock<IDataReader>(MockBehavior.Strict);
            _command
                .Setup(c => c.ExecuteReader())
                .Callback(() => _executedScripts.Add(_command.Object.CommandText))
                .Returns(_executedReader.Object);

            _sut = new TextScript();
            _variables.SetValue(VariableSource.CommandLine, "var1", "[some value]");
        }

        [Test]
        public void ExecuteShowVariableReplacement()
        {
            _sut.ReadSqlContent = () => new MemoryStream(Encoding.Default.GetBytes("{{var1}} {{var1}}"));

            _sut.Execute(_command.Object, _variables, _logger.Object);

            Assert.AreEqual(1, _executedScripts.Count);
            Assert.AreEqual("[some value] [some value]", _executedScripts[0]);

            Assert.IsNotNull(_logOutput.Where(i => i.Contains("var1") && i.Contains("[some value]")));
        }

        [Test]
        public void Execute()
        {
            _sut.ReadSqlContent = () => new MemoryStream(Encoding.Default.GetBytes(@"
{{var1}}
go
text2
go"));

            _sut.Execute(_command.Object, _variables, _logger.Object);

            Assert.AreEqual(2, _executedScripts.Count);
            Assert.AreEqual("[some value]", _executedScripts[0]);
            Assert.AreEqual("text2", _executedScripts[1]);
        }

        [Test]
        public void ExecuteReader()
        {
            _sut.ReadSqlContent = () => new MemoryStream(Encoding.Default.GetBytes("select {{var1}}"));
            _executedReader.Setup(r => r.Dispose());

            var actual = _sut.ExecuteReader(_command.Object, _variables, _logger.Object).ToList();

            _executedScripts.Count.ShouldBe(1);
            _executedScripts[0].ShouldBe("select [some value]");

            actual.Count.ShouldBe(1);
            actual[0].ShouldBe(_executedReader.Object);
        }

        [Test]
        public void GetDependencies()
        {
            _sut.ReadSqlContent = () => new MemoryStream(Encoding.Default.GetBytes(@"
-- module dependency: a 1.0
go
-- module dependency: b 1.0
go"));

            var actual = _sut.GetDependencies();

            actual.ShouldBe(new[] { new ScriptDependency("a", new Version("1.0")) });
        }
    }
}
