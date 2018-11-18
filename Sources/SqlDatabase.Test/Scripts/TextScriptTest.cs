using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;

namespace SqlDatabase.Scripts
{
    [TestFixture]
    public class TextScriptTest
    {
        private Mock<ILogger> _logger;
        private Variables _variables;
        private Mock<IDbCommand> _command;

        private IList<string> _logOutput;
        private IList<string> _executedScripts;

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
        }

        [Test]
        public void ExecuteShowVariableReplacement()
        {
            var sut = new TextScript
            {
                ReadSqlContent = () => new MemoryStream(Encoding.Default.GetBytes("{{var1}} {{var1}}"))
            };
            _variables.SetValue(VariableSource.CommandLine, "var1", "[some value]");

            sut.Execute(_command.Object, _variables, _logger.Object);

            _command.VerifyAll();
            Assert.AreEqual(1, _executedScripts.Count);
            Assert.AreEqual("[some value] [some value]", _executedScripts[0]);

            Assert.IsNotNull(_logOutput.Where(i => i.Contains("var1") && i.Contains("[some value]")));
        }

        [Test]
        public void Execute()
        {
            var sut = new TextScript
            {
                ReadSqlContent = () => new MemoryStream(Encoding.Default.GetBytes(@"
{{var1}}
go
text2
go"))
            };
            _variables.SetValue(VariableSource.CommandLine, "var1", "text1");

            sut.Execute(_command.Object, _variables, _logger.Object);

            _command.VerifyAll();

            Assert.AreEqual(2, _executedScripts.Count);
            Assert.AreEqual("text1", _executedScripts[0]);
            Assert.AreEqual("text2", _executedScripts[1]);
        }

        [Test]
        [TestCase("_'{{Value}}'_", "x", "_'x'_")]
        [TestCase("{{Value}}", "x", "x")]
        [TestCase("{{ValuE}}", "x", "x")]
        [TestCase("{{Value}}+{{Value}}", "x", "x+x")]
        [TestCase("{{Value}}\r\n{{Value}}", "x", "x\r\nx")]
        public void ApplyVariables(string sql, string value, string expected)
        {
            var variables = new Mock<IVariables>(MockBehavior.Strict);

            variables
                .Setup(v => v.GetValue(It.IsAny<string>()))
                .Returns<string>(name =>
                {
                    if ("value".Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        return value;
                    }

                    return null;
                });

            var actual = TextScript.ApplyVariables(sql, variables.Object);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [TestCase("[$(value)]", "some name", "[some name]")]
        [TestCase("$(Value)+$(Value)", "x", "x+x")]
        public void ApplySqlCmdVariables(string sql, string value, string expected)
        {
            var variables = new Mock<IVariables>(MockBehavior.Strict);

            variables
                .Setup(v => v.GetValue(It.IsAny<string>()))
                .Returns<string>(name =>
                {
                    if ("value".Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        return value;
                    }

                    return null;
                });

            var actual = TextScript.ApplyVariables(sql, variables.Object);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [TestCase("{{Value}}", "value", "123")]
        [TestCase("{{Value1}}", "value1", "123")]
        [TestCase("{{Value_1}}", "value_1", "123")]
        public void ApplyVariablesVariableNames(string sql, string name, string expected)
        {
            var variables = new Mock<IVariables>(MockBehavior.Strict);

            variables
                .Setup(v => v.GetValue(It.IsAny<string>()))
                .Returns<string>(n =>
                {
                    if (name.Equals(n, StringComparison.OrdinalIgnoreCase))
                    {
                        return "123";
                    }

                    return null;
                });

            var actual = TextScript.ApplyVariables(sql, variables.Object);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ApplyVariablesFailsOnUnknownVariable()
        {
            var variables = new Mock<IVariables>(MockBehavior.Strict);
            variables
                .Setup(v => v.GetValue(It.IsAny<string>()))
                .Returns((string)null);

            var ex = Assert.Throws<InvalidOperationException>(() => TextScript.ApplyVariables("{{Value_1}}", variables.Object));
            StringAssert.Contains("Value_1", ex.Message);
        }
    }
}
