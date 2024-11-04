using Moq;
using NUnit.Framework;
using SqlDatabase.Adapter;
using SqlDatabase.Adapter.Sql.Export;
using SqlDatabase.Scripts;
using SqlDatabase.Sequence;
using SqlDatabase.TestApi;

namespace SqlDatabase.Commands;

[TestFixture]
public class DatabaseExportCommandTest
{
    private DatabaseExportCommand _sut = null!;
    private Mock<IDatabase> _database = null!;
    private Mock<ICreateScriptSequence> _scriptSequence = null!;
    private Mock<IScriptResolver> _scriptResolver = null!;
    private Mock<ILogger> _logger = null!;
    private Mock<IValueDataReader> _valueReader = null!;
    private Mock<IDataExporter> _exporter = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _valueReader = new Mock<IValueDataReader>(MockBehavior.Strict);

        var adapter = new Mock<IDatabaseAdapter>(MockBehavior.Strict);
        adapter
            .Setup(a => a.GetUserFriendlyConnectionString())
            .Returns("host; database");
        adapter
            .Setup(a => a.CreateSqlWriter(It.IsAny<TextWriter>()))
            .Returns<TextWriter>(output => new Mock<SqlWriterBase>(MockBehavior.Loose, output) { CallBase = true }.Object);
        adapter
            .Setup(a => a.CreateValueDataReader())
            .Returns(_valueReader.Object);

        _database = new Mock<IDatabase>(MockBehavior.Strict);
        _database.SetupGet(d => d.Adapter).Returns(adapter.Object);
        _database.Setup(d => d.GetServerVersion(false)).Returns("sql server 1.0");

        _scriptSequence = new Mock<ICreateScriptSequence>(MockBehavior.Strict);

        _scriptResolver = new Mock<IScriptResolver>(MockBehavior.Strict);

        _logger = new Mock<ILogger>(MockBehavior.Strict);
        _logger.Setup(l => l.Indent()).Returns((IDisposable)null!);
        _logger
            .Setup(l => l.Info(It.IsAny<string>()))
            .Callback<string>(m =>
            {
                TestOutput.WriteLine("Info: {0}", m);
            });

        _exporter = new Mock<IDataExporter>(MockBehavior.Strict);
        _exporter
            .SetupProperty(e => e.Output);
        _exporter
            .SetupSet(e => e.Log = _logger.Object);

        _sut = new DatabaseExportCommand(
            _scriptSequence.Object,
            _scriptResolver.Object,
            () => Console.Out,
            _database.Object,
            _logger.Object)
        {
            ExporterFactory = () => _exporter.Object,
        };
    }

    [Test]
    public void ExportOneScript()
    {
        var script = new Mock<IScript>(MockBehavior.Strict);
        script.SetupGet(s => s.DisplayName).Returns("display name");

        var sequence = new[] { script.Object };

        _scriptResolver
            .Setup(f => f.InitializeEnvironment(_logger.Object, sequence));

        var reader = new Mock<IDataReader>(MockBehavior.Strict);
        reader
            .Setup(r => r.NextResult())
            .Returns(false);

        _database
            .Setup(d => d.ExecuteReader(script.Object))
            .Returns([reader.Object]);

        _exporter
            .Setup(e => e.Export(reader.Object, _valueReader.Object, "dbo.SqlDatabaseExport"));

        _scriptSequence
            .Setup(s => s.BuildSequence())
            .Returns(sequence);

        _sut.Execute();

        _database.VerifyAll();
        script.VerifyAll();
        _exporter.VerifyAll();
        _scriptResolver.VerifyAll();
    }

    [Test]
    public void ReaderNextResult()
    {
        var script = new Mock<IScript>(MockBehavior.Strict);
        script.SetupGet(s => s.DisplayName).Returns("display name");

        var sequence = new[] { script.Object };

        _scriptResolver
            .Setup(f => f.InitializeEnvironment(_logger.Object, sequence));

        var reader = new Mock<IDataReader>(MockBehavior.Strict);
        reader
            .Setup(r => r.NextResult())
            .Callback(() => reader.Setup(r => r.NextResult()).Returns(false))
            .Returns(true);

        _database
            .Setup(d => d.ExecuteReader(script.Object))
            .Returns(new[] { reader.Object });

        _exporter
            .Setup(e => e.Export(reader.Object, _valueReader.Object, "dbo.SqlDatabaseExport"));

        _exporter
            .Setup(e => e.Export(reader.Object, _valueReader.Object, "dbo.SqlDatabaseExport_2"));

        _scriptSequence
            .Setup(s => s.BuildSequence())
            .Returns(sequence);

        _sut.Execute();

        _database.VerifyAll();
        script.VerifyAll();
        _exporter.VerifyAll();
        _scriptResolver.VerifyAll();
    }
}