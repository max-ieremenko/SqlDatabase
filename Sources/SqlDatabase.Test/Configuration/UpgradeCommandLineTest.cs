using System.Data.SqlClient;
using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Commands;
using SqlDatabase.Scripts;

namespace SqlDatabase.Configuration
{
    [TestFixture]
    public class UpgradeCommandLineTest
    {
        private Mock<ILogger> _log;
        private UpgradeCommandLine _sut;

        [SetUp]
        public void BeforeEachTest()
        {
            _log = new Mock<ILogger>(MockBehavior.Strict);

            _sut = new UpgradeCommandLine();
        }

        [Test]
        public void Parse()
        {
            _sut.Parse(new CommandLine(
                new Arg("database", "Data Source=.;Initial Catalog=test"),
                new Arg("from", @"c:\folder"),
                new Arg("varX", "1 2 3"),
                new Arg("varY", "value"),
                new Arg("configuration", "app.config"),
                new Arg("transaction", "perStep")));

            _sut.Scripts.ShouldBe(new[] { @"c:\folder" });

            _sut.Connection.ShouldNotBeNull();
            _sut.Connection.DataSource.ShouldBe(".");
            _sut.Connection.InitialCatalog.ShouldBe("test");

            _sut.Variables.Keys.ShouldBe(new[] { "X", "Y" });
            _sut.Variables["x"].ShouldBe("1 2 3");
            _sut.Variables["y"].ShouldBe("value");

            _sut.ConfigurationFile.ShouldBe("app.config");

            _sut.Transaction.ShouldBe(TransactionMode.PerStep);
        }

        [Test]
        public void CreateCommand()
        {
            _sut.Connection = new SqlConnectionStringBuilder();

            var actual = _sut
                .CreateCommand(_log.Object)
                .ShouldBeOfType<DatabaseUpgradeCommand>();

            actual.Log.ShouldBe(_log.Object);
            actual.Database.ShouldBeOfType<Database>();
            actual.ScriptSequence.ShouldBeOfType<UpgradeScriptSequence>();
        }
    }
}