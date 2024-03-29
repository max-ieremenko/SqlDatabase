using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Adapter;
using SqlDatabase.Commands;
using SqlDatabase.FileSystem;

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

        _sut = new ExecuteCommandLine
        {
            FileSystemFactory = _fs.Object,
            Runtime = new HostedRuntime(false, true, FrameworkVersion.Net8)
        };
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
            new Arg("usePowerShell", @"c:\PowerShell"),
            new Arg("whatIf")));

        _sut.Scripts.Count.ShouldBe(2);
        _sut.Scripts[0].ShouldBe(folder.Object);
        _sut.Scripts[1].ShouldBe(sql.Object);

        _sut.ConnectionString.ShouldBe("Data Source=.;Initial Catalog=test");

        _sut.Variables.Keys.ShouldBe(["X", "Y"]);
        _sut.Variables["x"].ShouldBe("1 2 3");
        _sut.Variables["y"].ShouldBe("value");

        _sut.ConfigurationFile.ShouldBe("app.config");
        _sut.Transaction.ShouldBe(TransactionMode.PerStep);
        _sut.UsePowerShell.ShouldBe(@"c:\PowerShell");
        _sut.WhatIf.ShouldBeTrue();
    }

    [Test]
    public void CreateCommand()
    {
        _sut.WhatIf = true;
        _sut.ConnectionString = "connection string";
        _sut.Transaction = TransactionMode.PerStep;
        _sut.UsePowerShell = @"c:\PowerShell";

        var builder = new EnvironmentBuilderMock()
            .WithLogger(_log.Object)
            .WithConfiguration(_sut.ConfigurationFile)
            .WithPowerShellScripts(_sut.UsePowerShell)
            .WithAssemblyScripts()
            .WithVariables(_sut.Variables)
            .WithDataBase(_sut.ConnectionString, _sut.Transaction, _sut.WhatIf)
            .WithCreateSequence(_sut.Scripts);

        var actual = _sut
            .CreateCommand(_log.Object, builder.Build())
            .ShouldBeOfType<DatabaseExecuteCommand>();

        builder.VerifyAll();

        actual.Log.ShouldBe(_log.Object);
        actual.Database.ShouldBe(builder.Database);
        actual.ScriptResolver.ShouldBe(builder.ScriptResolver);
        actual.ScriptSequence.ShouldBe(builder.CreateSequence);
    }
}