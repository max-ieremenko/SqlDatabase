using NUnit.Framework;
using Shouldly;
using SqlDatabase.Configuration;
using SqlDatabase.PowerShell.TestApi;

namespace SqlDatabase.PowerShell
{
    [TestFixture]
    public class ExecuteCmdLetTest : SqlDatabaseCmdLetTest<ExecuteCmdLet>
    {
        [Test]
        [TestCase("Invoke-SqlDatabase")]
        [TestCase("Execute-SqlDatabase")]
        public void BuildCommandLine(string commandName)
        {
            var commandLines = InvokeCommand(
                commandName,
                c =>
                {
                    c.Parameters.Add(nameof(ExecuteCmdLet.Database), "connection string");
                    c.Parameters.Add(nameof(ExecuteCmdLet.From), new[] { "file 1", "file 2" });
                    c.Parameters.Add(nameof(ExecuteCmdLet.FromSql), new[] { "sql text 1", "sql text 2" });
                    c.Parameters.Add(nameof(ExecuteCmdLet.Transaction), TransactionMode.PerStep);
                    c.Parameters.Add(nameof(ExecuteCmdLet.Configuration), "app.config");
                    c.Parameters.Add(nameof(ExecuteCmdLet.Var), new[] { "x=1", "y=2" });
                    c.Parameters.Add(nameof(ExecuteCmdLet.WhatIf));
                });

            commandLines.Length.ShouldBe(1);
            var commandLine = commandLines[0];

            commandLine.Command.ShouldBe(CommandLineFactory.CommandExecute);
            commandLine.Connection.ShouldBe("connection string");
            commandLine.Scripts.ShouldBe(new[] { "file 1", "file 2" });
            commandLine.InLineScript.ShouldBe(new[] { "sql text 1", "sql text 2" });
            commandLine.Transaction.ShouldBe(TransactionMode.PerStep);
            commandLine.ConfigurationFile.ShouldBe("app.config");
            commandLine.WhatIf.ShouldBeTrue();

            commandLine.Variables.Keys.ShouldBe(new[] { "x", "y" });
            commandLine.Variables["x"].ShouldBe("1");
            commandLine.Variables["y"].ShouldBe("2");
        }

        [Test]
        [TestCase("Invoke-SqlDatabase")]
        [TestCase("Execute-SqlDatabase")]
        public void BuildPipeCommandLine(string commandName)
        {
            var commandLines = InvokeCommandPipeLine(
                commandName,
                c => c.Parameters.Add(nameof(ExecuteCmdLet.Database), "connection string"),
                "file 1",
                "file 2");

            commandLines.Length.ShouldBe(2);

            commandLines[0].Command.ShouldBe(CommandLineFactory.CommandExecute);
            commandLines[0].Connection.ShouldBe("connection string");
            commandLines[0].Scripts.ShouldBe(new[] { "file 1" });
            commandLines[0].InLineScript.Count.ShouldBe(0);

            commandLines[1].Command.ShouldBe(CommandLineFactory.CommandExecute);
            commandLines[1].Connection.ShouldBe("connection string");
            commandLines[1].Scripts.ShouldBe(new[] { "file 2" });
            commandLines[1].InLineScript.Count.ShouldBe(0);
        }
    }
}