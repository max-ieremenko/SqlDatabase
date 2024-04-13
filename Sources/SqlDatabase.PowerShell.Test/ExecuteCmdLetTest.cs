using NUnit.Framework;
using Shouldly;
using SqlDatabase.Adapter;
using SqlDatabase.CommandLine;
using SqlDatabase.PowerShell.TestApi;

namespace SqlDatabase.PowerShell;

[TestFixture]
public class ExecuteCmdLetTest : SqlDatabaseCmdLetTest<ExecuteCmdLet>
{
    [Test]
    [TestCase("Invoke-SqlDatabase")]
    [TestCase("Execute-SqlDatabase")]
    public void BuildCommandLine(string commandName)
    {
        var commandLines = InvokeSqlDatabase(
            commandName,
            c =>
            {
                c.Parameters.Add(nameof(ExecuteCmdLet.Database), "connection string");
                c.Parameters.Add(nameof(ExecuteCmdLet.From), new[] { "file 1", "file 2" });
                c.Parameters.Add(nameof(ExecuteCmdLet.FromSql), new[] { "sql text 1", "sql text 2" });
                c.Parameters.Add(nameof(ExecuteCmdLet.Transaction), PSTransactionMode.PerStep);
                c.Parameters.Add(nameof(ExecuteCmdLet.Configuration), "app.config");
                c.Parameters.Add(nameof(ExecuteCmdLet.Var), new[] { "x=1", "y=2" });
                c.Parameters.Add(nameof(ExecuteCmdLet.WhatIf));
                c.Parameters.Add(nameof(CreateCmdLet.Log), "log.txt");
            });

        commandLines.Length.ShouldBe(1);
        var actual = commandLines[0].ShouldBeOfType<ExecuteCommandLine>();

        actual.Database.ShouldBe("connection string");
        actual.From.ShouldBe([
            new(false, "file 1"),
            new(false, "file 2"),
            new(true, "sql text 1"),
            new(true, "sql text 2")
        ]);
        actual.Transaction.ShouldBe(TransactionMode.PerStep);
        actual.Configuration.ShouldBe("app.config");
        actual.Log.ShouldBe("log.txt");
        actual.UsePowerShell.ShouldBeNull();
        actual.WhatIf.ShouldBeTrue();

        actual.Variables.Keys.ShouldBe(new[] { "x", "y" });
        actual.Variables["x"].ShouldBe("1");
        actual.Variables["y"].ShouldBe("2");
    }

    [Test]
    [TestCase("Invoke-SqlDatabase")]
    [TestCase("Execute-SqlDatabase")]
    public void BuildPipeCommandLine(string commandName)
    {
        var commandLines = InvokeInvokeSqlDatabasePipeLine(
            commandName,
            c => c.Parameters.Add(nameof(ExecuteCmdLet.Database), "connection string"),
            "file 1",
            "file 2");

        commandLines.Length.ShouldBe(2);

        var actual0 = commandLines[0].ShouldBeOfType<ExecuteCommandLine>();
        actual0.Database.ShouldBe("connection string");
        actual0.From.ShouldBe([new(false, "file 1")]);

        var actual1 = commandLines[1].ShouldBeOfType<ExecuteCommandLine>();
        actual1.Database.ShouldBe("connection string");
        actual1.From.ShouldBe([new(false, "file 2")]);
    }
}