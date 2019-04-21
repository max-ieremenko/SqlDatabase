using System;
using System.Data.SqlClient;
using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Commands;
using SqlDatabase.IO;
using SqlDatabase.Scripts;

namespace SqlDatabase.Configuration
{
    [TestFixture]
    public class ExportCommandLineTest
    {
        private Mock<ILogger> _log;
        private Mock<IFileSystemFactory> _fs;
        private ExportCommandLine _sut;

        [SetUp]
        public void BeforeEachTest()
        {
            _log = new Mock<ILogger>(MockBehavior.Strict);
            _fs = new Mock<IFileSystemFactory>(MockBehavior.Strict);

            _sut = new ExportCommandLine { FileSystemFactory = _fs.Object };
        }

        [Test]
        public void Parse()
        {
            var folder = new Mock<IFileSystemInfo>(MockBehavior.Strict);
            _fs
                .Setup(f => f.FileSystemInfoFromPath(@"c:\folder"))
                .Returns(folder.Object);

            _sut.Parse(new CommandLine(
                new Arg("database", "Data Source=.;Initial Catalog=test"),
                new Arg("from", @"c:\folder"),
                new Arg("toTable", "dbo.ExportedData")));

            _sut.Scripts.ShouldBe(new[] { folder.Object });

            _sut.Connection?.DataSource.ShouldBe(".");
            _sut.Connection?.InitialCatalog.ShouldBe("test");
            _sut.DestinationTableName.ShouldBe("dbo.ExportedData");
        }

        [Test]
        public void CreateCommand()
        {
            _sut.Connection = new SqlConnectionStringBuilder();
            _sut.DestinationTableName = "table 1";

            var actual = _sut
                .CreateCommand(_log.Object)
                .ShouldBeOfType<DatabaseExportCommand>();

            actual.Log.ShouldNotBe(_log.Object);
            actual.Database.ShouldBeOfType<Database>();
            actual.ScriptSequence.ShouldBeOfType<CreateScriptSequence>();
            actual.DestinationTableName.ShouldBe("table 1");
        }

        [Test]
        public void DoesNotSupportTransaction()
        {
            _sut.Transaction = TransactionMode.PerStep;

            Assert.Throws<NotSupportedException>(_sut.Validate);
        }
    }
}
