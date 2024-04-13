using NUnit.Framework;
using Shouldly;
using SqlDatabase.Adapter;
using SqlDatabase.CommandLine.Internal;

namespace SqlDatabase.CommandLine;

[TestFixture]
public class CommandLineParserTest
{
    private readonly HostedRuntime _testRuntime = new(false, false, FrameworkVersion.Net8);

    [Test]
    [TestCase("", null, true)]
    [TestCase("-help", null, true)]
    [TestCase("create", "create", true)]
    [TestCase("export -h", "export", true)]
    [TestCase("execute -help", "execute", true)]
    [TestCase("Upgrade -arg1 -arg2", "Upgrade", false)]
    [TestCase("Upgrade -help -arg1 -arg2", "Upgrade", false)]
    public void HelpRequested(string args, string? expectedCommand, bool expectedHelp)
    {
        CommandLineParser.HelpRequested(args.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries), out var actualCommand).ShouldBe(expectedHelp);

        actualCommand.ShouldBe(expectedCommand);
    }

    [Test]
    public void ParseCreateCommand()
    {
        var args = ToArgs(
            "create",
            new Arg("database", "Data Source=.;Initial Catalog=test"),
            new Arg("from", @"c:\folder"),
            new Arg("varX", "1 2 3"),
            new Arg("varY", "value"),
            new Arg("configuration", "app.config"),
            new Arg("usePowerShell", @"c:\PowerShell"),
            new Arg("whatIf", null));

        var actual = CommandLineParser.Parse(_testRuntime, args).ShouldBeOfType<CreateCommandLine>();

        actual.From.ShouldBe([new(false, @"c:\folder")]);

        actual.Database.ShouldBe("Data Source=.;Initial Catalog=test");

        actual.Variables.Keys.ShouldBe(new[] { "X", "Y" });
        actual.Variables["x"].ShouldBe("1 2 3");
        actual.Variables["y"].ShouldBe("value");

        actual.Configuration.ShouldBe("app.config");
        actual.UsePowerShell.ShouldBe(@"c:\PowerShell");
        actual.WhatIf.ShouldBeTrue();
    }

    [Test]
    public void ParseExecuteCommand()
    {
        var args = ToArgs(
            "execute",
            new Arg("database", "Data Source=.;Initial Catalog=test"),
            new Arg("from", @"c:\folder"),
            new Arg("fromSql", "drop 1"),
            new Arg("varX", "1 2 3"),
            new Arg("varY", "value"),
            new Arg("configuration", "app.config"),
            new Arg("transaction", "perStep"),
            new Arg("usePowerShell", @"c:\PowerShell"),
            new Arg("whatIf", null));

        var actual = CommandLineParser.Parse(_testRuntime, args).ShouldBeOfType<ExecuteCommandLine>();

        actual.From.Count.ShouldBe(2);
        actual.From[0].ShouldBe(new(false, @"c:\folder"));
        actual.From[1].ShouldBe(new(true, "drop 1"));

        actual.Database.ShouldBe("Data Source=.;Initial Catalog=test");

        actual.Variables.Keys.ShouldBe(new[] { "X", "Y" });
        actual.Variables["x"].ShouldBe("1 2 3");
        actual.Variables["y"].ShouldBe("value");

        actual.Configuration.ShouldBe("app.config");
        actual.Transaction.ShouldBe(TransactionMode.PerStep);
        actual.UsePowerShell.ShouldBe(@"c:\PowerShell");
        actual.WhatIf.ShouldBeTrue();
    }

    [Test]
    public void ParseUpgradeCommand()
    {
        var args = ToArgs(
            "upgrade",
            new Arg("database", "Data Source=.;Initial Catalog=test"),
            new Arg("from", @"c:\folder"),
            new Arg("varX", "1 2 3"),
            new Arg("varY", "value"),
            new Arg("configuration", "app.config"),
            new Arg("transaction", "perStep"),
            new Arg("folderAsModuleName", null),
            new Arg("usePowerShell", @"c:\PowerShell"),
            new Arg("whatIf", null));

        var actual = CommandLineParser.Parse(_testRuntime, args).ShouldBeOfType<UpgradeCommandLine>();

        actual.From.ShouldBe([new(false, @"c:\folder")]);

        actual.Database.ShouldBe("Data Source=.;Initial Catalog=test");

        actual.Variables.Keys.ShouldBe(new[] { "X", "Y" });
        actual.Variables["x"].ShouldBe("1 2 3");
        actual.Variables["y"].ShouldBe("value");

        actual.Configuration.ShouldBe("app.config");
        actual.Transaction.ShouldBe(TransactionMode.PerStep);
        actual.UsePowerShell.ShouldBe(@"c:\PowerShell");
        actual.WhatIf.ShouldBeTrue();
        actual.FolderAsModuleName.ShouldBeTrue();
    }

    [Test]
    public void ParseExportCommand()
    {
        var args = ToArgs(
            "export",
            new Arg("database", "Data Source=.;Initial Catalog=test"),
            new Arg("fromSql", "select 1"),
            new Arg("from", @"c:\folder"),
            new Arg("toTable", "dbo.ExportedData"),
            new Arg("toFile", "file path"));

        var actual = CommandLineParser.Parse(_testRuntime, args).ShouldBeOfType<ExportCommandLine>();

        actual.From.Count.ShouldBe(2);
        actual.From[0].ShouldBe(new(true, "select 1"));
        actual.From[1].ShouldBe(new(false, @"c:\folder"));

        actual.Database.ShouldBe("Data Source=.;Initial Catalog=test");
        actual.DestinationTableName.ShouldBe("dbo.ExportedData");
        actual.DestinationFileName.ShouldBe("file path");
    }

    private static string[] ToArgs(string command, params Arg[] args)
    {
        var result = new List<string>(args.Length + 1) { command };
        result.AddRange(args.Select(i => '-' + i.ToString()));
        return result.ToArray();
    }
}