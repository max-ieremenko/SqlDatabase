using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Adapter;
using SqlDatabase.Commands;
using SqlDatabase.FileSystem;

namespace SqlDatabase.Configuration;

[TestFixture]
public class UpgradeCommandLineTest
{
    private Mock<ILogger> _log = null!;
    private Mock<IFileSystemFactory> _fs = null!;
    private UpgradeCommandLine _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _log = new Mock<ILogger>(MockBehavior.Strict);
        _fs = new Mock<IFileSystemFactory>(MockBehavior.Strict);

        _sut = new UpgradeCommandLine { FileSystemFactory = _fs.Object };
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
            new Arg("varX", "1 2 3"),
            new Arg("varY", "value"),
            new Arg("configuration", "app.config"),
            new Arg("transaction", "perStep"),
            new Arg("folderAsModuleName"),
#if !NET472
            new Arg("usePowerShell", @"c:\PowerShell"),
#endif
            new Arg("whatIf")));

        _sut.Scripts.ShouldBe(new[] { folder.Object });

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
        _sut.FolderAsModuleName.ShouldBeTrue();
    }

    [Test]
    public void CreateCommand()
    {
        _sut.WhatIf = true;
        _sut.FolderAsModuleName = true;
        _sut.Transaction = TransactionMode.PerStep;
        _sut.ConnectionString = "connection string";
        _sut.UsePowerShell = @"c:\PowerShell";

        var builder = new EnvironmentBuilderMock()
            .WithLogger(_log.Object)
            .WithConfiguration(_sut.ConfigurationFile)
            .WithPowerShellScripts(_sut.UsePowerShell)
            .WithAssemblyScripts()
            .WithVariables(_sut.Variables)
            .WithDataBase(_sut.ConnectionString, _sut.Transaction, _sut.WhatIf)
            .WithUpgradeSequence(_sut.Scripts, _sut.FolderAsModuleName);

        var actual = _sut
            .CreateCommand(_log.Object, builder.Build())
            .ShouldBeOfType<DatabaseUpgradeCommand>();

        builder.VerifyAll();

        actual.Log.ShouldBe(_log.Object);
        actual.Database.ShouldBe(builder.Database);
        actual.ScriptResolver.ShouldBe(builder.ScriptResolver);
        actual.ScriptSequence.ShouldBe(builder.UpgradeSequence);
    }
}