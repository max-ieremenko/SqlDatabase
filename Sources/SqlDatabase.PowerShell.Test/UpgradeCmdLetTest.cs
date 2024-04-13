using NUnit.Framework;
using Shouldly;
using SqlDatabase.Adapter;
using SqlDatabase.CommandLine;
using SqlDatabase.PowerShell.TestApi;

namespace SqlDatabase.PowerShell;

[TestFixture]
public class UpgradeCmdLetTest : SqlDatabaseCmdLetTest<UpgradeCmdLet>
{
    [Test]
    [TestCase("Update-SqlDatabase")]
    [TestCase("Upgrade-SqlDatabase")]
    public void BuildCommandLine(string commandName)
    {
        var commandLines = InvokeSqlDatabase(
            commandName,
            c =>
            {
                c.Parameters.Add(nameof(UpgradeCmdLet.Database), "connection string");
                c.Parameters.Add(nameof(UpgradeCmdLet.From), new[] { "file 1", "file 2" });
                c.Parameters.Add(nameof(UpgradeCmdLet.Transaction), PSTransactionMode.PerStep);
                c.Parameters.Add(nameof(UpgradeCmdLet.Configuration), "app.config");
                c.Parameters.Add(nameof(UpgradeCmdLet.Var), new[] { "x=1", "y=2" });
                c.Parameters.Add(nameof(UpgradeCmdLet.WhatIf));
                c.Parameters.Add(nameof(UpgradeCmdLet.FolderAsModuleName));
                c.Parameters.Add(nameof(CreateCmdLet.Log), "log.txt");
            });

        commandLines.Length.ShouldBe(1);
        var actual = commandLines[0].ShouldBeOfType<UpgradeCommandLine>();

        actual.Database.ShouldBe("connection string");
        actual.From.ShouldBe([new(false, "file 1"), new(false, "file 2")]);
        actual.Transaction.ShouldBe(TransactionMode.PerStep);
        actual.Configuration.ShouldBe("app.config");
        actual.Log.ShouldBe("log.txt");
        actual.UsePowerShell.ShouldBeNull();
        actual.WhatIf.ShouldBeTrue();
        actual.FolderAsModuleName.ShouldBeTrue();

        actual.Variables.Keys.ShouldBe(new[] { "x", "y" });
        actual.Variables["x"].ShouldBe("1");
        actual.Variables["y"].ShouldBe("2");
    }

    [Test]
    [TestCase("Update-SqlDatabase")]
    [TestCase("Upgrade-SqlDatabase")]
    public void BuildPipeCommandLine(string commandName)
    {
        var commandLines = InvokeInvokeSqlDatabasePipeLine(
            commandName,
            c => c.Parameters.Add(nameof(UpgradeCmdLet.Database), "connection string"),
            "file 1",
            "file 2");

        commandLines.Length.ShouldBe(2);

        var actual0 = commandLines[0].ShouldBeOfType<UpgradeCommandLine>();
        actual0.Database.ShouldBe("connection string");
        actual0.From.ShouldBe([new(false, "file 1")]);

        var actual1 = commandLines[1].ShouldBeOfType<UpgradeCommandLine>();
        actual1.Database.ShouldBe("connection string");
        actual1.From.ShouldBe([new(false, "file 2")]);
    }
}