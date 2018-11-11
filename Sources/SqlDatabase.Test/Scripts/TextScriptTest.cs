using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
                ReadSqlContent = () => "{{var1}} {{var1}}"
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
                ReadSqlContent = () => @"
{{var1}}
go
text2
go"
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

        [Test]
        public void SplitByGo()
        {
            const string input = @"
1
go

GO

GO

2
GO
3

4

go

5";
            var actual = TextScript.SplitByGo(input).ToArray();
            Assert.AreEqual(4, actual.Length);

            Assert.AreEqual("1", actual[0]);
            Assert.AreEqual("2", actual[1]);
            Assert.AreEqual("3\r\n\r\n4\r\n", actual[2]);
            Assert.AreEqual("5", actual[3]);
        }

        [Test]
        [TestCase("go", true)]
        [TestCase("go go", true)]
        [TestCase(" go ", true)]
        [TestCase(" \tGO \t", true)]
        [TestCase("go\tgo go", true)]
        [TestCase("o", false)]
        [TestCase("fo", false)]
        [TestCase("go pro", false)]
        public void IsGo(string line, bool expected)
        {
            Assert.AreEqual(expected, TextScript.IsGo(line));
        }
    }
}
