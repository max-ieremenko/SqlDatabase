using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Adapter;
using SqlDatabase.Commands;
using SqlDatabase.FileSystem;
using SqlDatabase.Scripts;
using SqlDatabase.TestApi;

namespace SqlDatabase.Configuration;

[TestFixture]
public class ExecuteCommandLineTest
{
    private Mock<ILogger> _log = null!;
    private Mock<IFileSystemFactory> _fs = null!;
    private ExecuteCommandLine _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _log = new Mock<ILogger>(MockBehavior.Strict);
        _fs = new Mock<IFileSystemFactory>(MockBehavior.Strict);

        _sut = new ExecuteCommandLine { FileSystemFactory = _fs.Object };
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
            .Setup(f => f.FromContent("from2.sql", "drop 1"))
            .Returns(sql.Object);

        _sut.Parse(new CommandLine(
            new Arg("database", "Data Source=.;Initial Catalog=test"),
            new Arg("from", @"c:\folder"),
            new Arg("fromSql", "drop 1"),
            new Arg("varX", "1 2 3"),
            new Arg("varY", "value"),
            new Arg("configuration", "app.config"),
            new Arg("transaction", "perStep"),
#if !NET472
            new Arg("usePowerShell", @"c:\PowerShell"),
#endif
            new Arg("whatIf")));

        _sut.Scripts.Count.ShouldBe(2);
        _sut.Scripts[0].ShouldBe(folder.Object);
        _sut.Scripts[1].ShouldBe(sql.Object);

        _sut.ConnectionString.ShouldBe("Data Source=.;Initial Catalog=test");

        _sut.Variables.Keys.ShouldBe(new[] { "X", "Y" });
        _sut.Variables["x"].ShouldBe("1 2 3");
        _sut.Variables["y"].ShouldBe("value");

        _sut.ConfigurationFile.ShouldBe("app.config");

        _sut.Transaction.ShouldBe(TransactionMode.PerStep);

#if !NET472
        _sut.UsePowerShell.ShouldBe(@"c:\PowerShell");
#endif

        _sut.WhatIf.ShouldBeTrue();
    }

    [Test]
    public void CreateCommand()
    {
        _sut.WhatIf = true;
        _sut.ConnectionString = MsSqlQuery.ConnectionString;
        _sut.UsePowerShell = @"c:\PowerShell";

        var actual = _sut
            .CreateCommand(_log.Object)
            .ShouldBeOfType<DatabaseExecuteCommand>();

        actual.Log.ShouldBe(_log.Object);
        var database = actual.Database.ShouldBeOfType<Database>();
        database.WhatIf.ShouldBeTrue();

        var scriptFactory = actual.ScriptSequence.ShouldBeOfType<CreateScriptSequence>().ScriptFactory.ShouldBeOfType<ScriptFactory>();
        scriptFactory.PowerShellFactory!.InstallationPath.ShouldBe(@"c:\PowerShell");

        actual.PowerShellFactory.ShouldBe(scriptFactory.PowerShellFactory);
    }
}