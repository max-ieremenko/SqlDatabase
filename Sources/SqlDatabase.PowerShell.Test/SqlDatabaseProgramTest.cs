using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell
{
    [TestFixture]
    public class SqlDatabaseProgramTest
    {
        private Mock<ICmdlet> _owner;
        private IList<string> _output;
        private IList<string> _outputErrors;
        private SqlDatabaseProgram _sut;

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

            _sut = new SqlDatabaseProgram(_owner.Object);
        }

        [Test]
        public void EchoCommandLine()
        {
            var command = new CommandLineBuilder()
                .SetCommand(Command.Echo)
                .SetConfigurationFile("config file")
                .SetConnection("Data Source=.;Initial Catalog=SqlDatabaseTest")
                .SetScripts("script 1")
                .SetVariable("var1", "value 1\\")
                .Build();

            _sut.ExecuteCommand(command);

            _output.Count.ShouldBe(7);

            var actual = CommandLineBuilder.FromArguments(_output.ToArray());

            actual.Command.ShouldBe(Command.Echo);
            actual.ConfigurationFile.ShouldBe("config file");
            actual.Connection.DataSource.ShouldBe(".");
            actual.Connection.InitialCatalog.ShouldBe("SqlDatabaseTest");
            actual.Scripts.ShouldBe(new[] { "script 1" });
            actual.Variables.Count.ShouldBe(1);
            actual.Variables["var1"].ShouldBe("value 1\\");
            actual.PreFormatOutputLogs.ShouldBe(true);
        }

        [Test]
        public void ValidateErrors()
        {
            var command = new CommandLineBuilder()
                .SetCommand(Command.Unknown)
                .SetConnection("Data Source=.;Initial Catalog=SqlDatabaseTest")
                .SetScripts("script 1")
                .Build();

            _sut.ExecuteCommand(command);

            _owner.Verify();
            _outputErrors.Count.ShouldBe(1);
        }
    }
}
