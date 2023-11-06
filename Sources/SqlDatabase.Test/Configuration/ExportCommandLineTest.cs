using System.IO;
using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Adapter;
using SqlDatabase.Commands;
using SqlDatabase.Export;
using SqlDatabase.FileSystem;
using SqlDatabase.Scripts;
using SqlDatabase.TestApi;

namespace SqlDatabase.Configuration;

[TestFixture]
public class ExportCommandLineTest
{
    private Mock<ILogger> _log = null!;
    private Mock<IFileSystemFactory> _fs = null!;
    private ExportCommandLine _sut = null!;

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
        var sql = new Mock<IFileSystemInfo>(MockBehavior.Strict);
        _fs
            .Setup(f => f.FileSystemInfoFromPath(@"c:\folder"))
            .Returns(folder.Object);
        _fs
            .Setup(f => f.FromContent("from1.sql", "select 1"))
            .Returns(sql.Object);

        _sut.Parse(new CommandLine(
            new Arg("database", "Data Source=.;Initial Catalog=test"),
            new Arg("fromSql", "select 1"),
            new Arg("from", @"c:\folder"),
            new Arg("toTable", "dbo.ExportedData"),
            new Arg("toFile", "file path")));

        _sut.Scripts.Count.ShouldBe(2);
        _sut.Scripts[0].ShouldBe(sql.Object);
        _sut.Scripts[1].ShouldBe(folder.Object);

        _sut.ConnectionString.ShouldBe("Data Source=.;Initial Catalog=test");
        _sut.DestinationTableName.ShouldBe("dbo.ExportedData");
        _sut.DestinationFileName.ShouldBe("file path");
    }

    [Test]
    public void CreateCommand()
    {
        _sut.ConnectionString = MsSqlQuery.ConnectionString;
        _sut.DestinationTableName = "table 1";

        var actual = _sut
            .CreateCommand(_log.Object)
            .ShouldBeOfType<DatabaseExportCommand>();

        actual.Log.ShouldNotBe(_log.Object);
        actual.Database.ShouldBeOfType<Database>();
        actual.ScriptSequence.ShouldBeOfType<CreateScriptSequence>();
        actual.OpenOutput.ShouldNotBeNull();
        actual.DestinationTableName.ShouldBe("table 1");
    }

    [Test]
    public void CreateOutputConsole()
    {
        var actual = _sut.CreateOutput();

        string output;
        using (var console = new TempConsoleOut())
        {
            using (var writer = actual())
            {
                writer.Write("hello");
            }

            output = console.GetOutput();
        }

        output.ShouldBe("hello");
    }

    [Test]
    public void WrapLoggerConsole()
    {
        _sut.WrapLogger(_log.Object).ShouldBeOfType<DataExportLogger>();
    }

    [Test]
    public void CreateOutputFile()
    {
        using (var file = new TempFile(".sql"))
        {
            _sut.DestinationFileName = file.Location;

            var actual = _sut.CreateOutput();

            using (var writer = actual())
            {
                writer.Write("hello 1");
            }

            FileAssert.Exists(file.Location);
            File.ReadAllText(file.Location).ShouldBe("hello 1");

            using (var writer = actual())
            {
                writer.Write("hello 2");
            }

            File.ReadAllText(file.Location).ShouldBe("hello 2");
        }
    }

    [Test]
    public void WrapLoggerFile()
    {
        _sut.DestinationFileName = "file";

        _sut.WrapLogger(_log.Object).ShouldBe(_log.Object);
    }
}