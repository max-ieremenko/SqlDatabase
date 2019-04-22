using System;
using System.Data;
using Moq;
using NUnit.Framework;
using SqlDatabase.Export;
using SqlDatabase.Scripts;

namespace SqlDatabase.Commands
{
    [TestFixture]
    public class DatabaseExportCommandTest
    {
        private DatabaseExportCommand _sut;
        private Mock<IDatabase> _database;
        private Mock<ICreateScriptSequence> _scriptSequence;
        private Mock<IDataExporter> _exporter;

        [SetUp]
        public void BeforeEachTest()
        {
            _database = new Mock<IDatabase>(MockBehavior.Strict);
            _database.SetupGet(d => d.ConnectionString).Returns(@"Data Source=unknownServer;Initial Catalog=unknownDatabase");
            _database.Setup(d => d.GetServerVersion()).Returns("sql server 1.0");

            _scriptSequence = new Mock<ICreateScriptSequence>(MockBehavior.Strict);

            var log = new Mock<ILogger>(MockBehavior.Strict);
            log.Setup(l => l.Indent()).Returns((IDisposable)null);
            log
                .Setup(l => l.Info(It.IsAny<string>()))
                .Callback<string>(m =>
                {
                    Console.WriteLine("Info: {0}", m);
                });

            _exporter = new Mock<IDataExporter>(MockBehavior.Strict);
            _exporter
                .SetupSet(e => e.Output = It.IsNotNull<SqlWriter>());
            _exporter
                .SetupSet(e => e.Log = log.Object);

            _sut = new DatabaseExportCommand
            {
                Database = _database.Object,
                Log = log.Object,
                ScriptSequence = _scriptSequence.Object,
                ExporterFactory = () => _exporter.Object,
                OpenOutput = () => Console.Out
            };
        }

        [Test]
        public void ExportOneScript()
        {
            var script = new Mock<IScript>(MockBehavior.Strict);
            script.SetupGet(s => s.DisplayName).Returns("display name");

            var reader = new Mock<IDataReader>(MockBehavior.Strict);
            reader
                .Setup(r => r.NextResult())
                .Returns(false);

            _database
                .Setup(d => d.ExecuteReader(script.Object))
                .Returns(new[] { reader.Object });

            _exporter
                .Setup(e => e.Export(reader.Object, "dbo.SqlDatabaseExport"));

            _scriptSequence.Setup(s => s.BuildSequence()).Returns(new[] { script.Object });

            _sut.Execute();

            _database.VerifyAll();
            script.VerifyAll();
            _exporter.VerifyAll();
        }

        [Test]
        public void ReaderNextResult()
        {
            var script = new Mock<IScript>(MockBehavior.Strict);
            script.SetupGet(s => s.DisplayName).Returns("display name");

            var reader = new Mock<IDataReader>(MockBehavior.Strict);
            reader
                .Setup(r => r.NextResult())
                .Callback(() => reader.Setup(r => r.NextResult()).Returns(false))
                .Returns(true);

            _database
                .Setup(d => d.ExecuteReader(script.Object))
                .Returns(new[] { reader.Object });

            _exporter
                .Setup(e => e.Export(reader.Object, "dbo.SqlDatabaseExport"));

            _exporter
                .Setup(e => e.Export(reader.Object, "dbo.SqlDatabaseExport_2"));

            _scriptSequence.Setup(s => s.BuildSequence()).Returns(new[] { script.Object });

            _sut.Execute();

            _database.VerifyAll();
            script.VerifyAll();
            _exporter.VerifyAll();
        }
    }
}
