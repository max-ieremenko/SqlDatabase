using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell
{
    [TestFixture]
    public class SqlDatabaseProgramNet452Test
    {
        private Mock<ICmdlet> _owner;
        private IList<string> _output;
        private IList<string> _outputErrors;
        private SqlDatabaseProgramNet452 _sut;

        [SetUp]
        public void BeforeEachTest()
        {
            _output = new List<string>();
            _outputErrors = new List<string>();

            _owner = new Mock<ICmdlet>(MockBehavior.Strict);
            _owner
                .Setup(o => o.WriteLine(It.IsNotNull<string>()))
                .Callback<string>(_output.Add);
            _owner
                .Setup(o => o.WriteErrorLine(It.IsNotNull<string>()))
                .Callback<string>(_outputErrors.Add);

            _sut = new SqlDatabaseProgramNet452(_owner.Object);
        }

        [Test]
        public void EchoCommandLine()
        {
            var command = new GenericCommandLineBuilder()
                .SetCommand(CommandLineFactory.CommandEcho)
                .SetConfigurationFile("config file")
                .SetConnection("connection")
                .SetScripts("script 1")
                .SetVariable("var1", "value 1\\")
                .Build();

            _sut.ExecuteCommand(command);

            foreach (var i in _output)
            {
                Console.WriteLine(i);
            }

            _output.Count.ShouldBe(6);

            CommandLineParser.PreFormatOutputLogs(_output).ShouldBeTrue();
            var actual = new CommandLineParser().Parse(_output.ToArray());

            actual.Args[0].IsPair.ShouldBe(false);
            actual.Args[0].Value.ShouldBe(CommandLineFactory.CommandEcho);

            actual.Args[1].Key.ShouldBe("database");
            actual.Args[1].Value.ShouldBe("connection");

            actual.Args[2].Key.ShouldBe("from");
            actual.Args[2].Value.ShouldBe("script 1");

            actual.Args[3].Key.ShouldBe("configuration");
            actual.Args[3].Value.ShouldBe("config file");

            actual.Args[4].Key.ShouldBe("varvar1");
            actual.Args[4].Value.ShouldBe("value 1\\");
        }

        [Test]
        public void ValidateErrors()
        {
            var command = new GenericCommandLineBuilder()
                .SetCommand("Unknown")
                .SetConnection("Data Source=.;Initial Catalog=SqlDatabaseTest")
                .SetScripts("script 1")
                .Build();

            _sut.ExecuteCommand(command);

            _owner.Verify();
            _outputErrors.Count.ShouldBe(1);
        }
    }
}
