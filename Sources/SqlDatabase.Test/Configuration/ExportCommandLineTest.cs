using System;
using System.Data.SqlClient;
using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Commands;
using SqlDatabase.Scripts;

namespace SqlDatabase.Configuration
{
    [TestFixture]
    public class ExportCommandLineTest
    {
        private Mock<ILogger> _log;
        private ExportCommandLine _sut;

        [SetUp]
        public void BeforeEachTest()
        {
            _log = new Mock<ILogger>(MockBehavior.Strict);

            _sut = new ExportCommandLine();
        }

        [Test]
        public void Parse()
        {
            _sut.Parse(new CommandLine(
                new Arg("database", "Data Source=.;Initial Catalog=test"),
                new Arg("from", @"c:\folder")));

            _sut.Scripts.ShouldBe(new[] { @"c:\folder" });

            _sut.Connection?.DataSource.ShouldBe(".");
            _sut.Connection?.InitialCatalog.ShouldBe("test");
        }

        [Test]
        public void CreateCommand()
        {
            _sut.Connection = new SqlConnectionStringBuilder();

            var actual = _sut
                .CreateCommand(_log.Object)
                .ShouldBeOfType<DatabaseExportCommand>();

            actual.Log.ShouldNotBe(_log.Object);
            actual.Database.ShouldBeOfType<Database>();
            actual.ScriptSequence.ShouldBeOfType<CreateScriptSequence>();
        }

        [Test]
        public void DoesNotSupportTransaction()
        {
            _sut.Transaction = TransactionMode.PerStep;

            Assert.Throws<NotSupportedException>(_sut.Validate);
        }
    }
}
