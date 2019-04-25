using NUnit.Framework;
using Shouldly;
using SqlDatabase.Configuration;
using SqlDatabase.PowerShell.TestApi;

namespace SqlDatabase.PowerShell
{
    [TestFixture]
    public class UpgradeCmdLetTest : SqlDatabaseCmdLetTest<UpgradeCmdLet>
    {
        [Test]
        [TestCase("Update-SqlDatabase")]
        [TestCase("Upgrade-SqlDatabase")]
        public void BuildCommandLine(string commandName)
        {
            var commandLines = InvokeCommand(
                commandName,
                c =>
                {
                    c.Parameters.Add(nameof(UpgradeCmdLet.Database), "connection string");
                    c.Parameters.Add(nameof(UpgradeCmdLet.From), new[] { "file 1", "file 2" });
                    c.Parameters.Add(nameof(UpgradeCmdLet.Transaction), TransactionMode.PerStep);
                    c.Parameters.Add(nameof(UpgradeCmdLet.Configuration), "app.config");
                    c.Parameters.Add(nameof(UpgradeCmdLet.Var), new[] { "x=1", "y=2" });
                });

            commandLines.Length.ShouldBe(1);
            var commandLine = commandLines[0];

            commandLine.Command.ShouldBe(CommandLineFactory.CommandUpgrade);
            commandLine.Connection.ShouldBe("connection string");
            commandLine.Scripts.ShouldBe(new[] { "file 1", "file 2" });
            commandLine.Transaction.ShouldBe(TransactionMode.PerStep);
            commandLine.ConfigurationFile.ShouldBe("app.config");

            commandLine.Variables.Keys.ShouldBe(new[] { "x", "y" });
            commandLine.Variables["x"].ShouldBe("1");
            commandLine.Variables["y"].ShouldBe("2");
        }

        [Test]
        [TestCase("Update-SqlDatabase")]
        [TestCase("Upgrade-SqlDatabase")]
        public void BuildPipeCommandLine(string commandName)
        {
            var commandLines = InvokeCommandPipeLine(
                commandName,
                c => c.Parameters.Add(nameof(UpgradeCmdLet.Database), "connection string"),
                "file 1",
                "file 2");

            commandLines.Length.ShouldBe(2);

            commandLines[0].Command.ShouldBe(CommandLineFactory.CommandUpgrade);
            commandLines[0].Connection.ShouldBe("connection string");
            commandLines[0].Scripts.ShouldBe(new[] { "file 1" });
            commandLines[0].InLineScript.Count.ShouldBe(0);

            commandLines[1].Command.ShouldBe(CommandLineFactory.CommandUpgrade);
            commandLines[1].Connection.ShouldBe("connection string");
            commandLines[1].Scripts.ShouldBe(new[] { "file 2" });
            commandLines[1].InLineScript.Count.ShouldBe(0);
        }
    }
}