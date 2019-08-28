using NUnit.Framework;
using Shouldly;
using SqlDatabase.Configuration;
using SqlDatabase.PowerShell.TestApi;

namespace SqlDatabase.PowerShell
{
    [TestFixture]
    public class CreateCmdLetTest : SqlDatabaseCmdLetTest<CreateCmdLet>
    {
        [Test]
        [TestCase("New-SqlDatabase")]
        [TestCase("Create-SqlDatabase")]
        public void BuildCommandLine(string commandName)
        {
            var commandLines = InvokeCommand(
                commandName,
                c =>
                {
                    c.Parameters.Add(nameof(CreateCmdLet.Database), "connection string");
                    c.Parameters.Add(nameof(CreateCmdLet.From), new[] { "file 1", "file 2" });
                    c.Parameters.Add(nameof(CreateCmdLet.Configuration), "app.config");
                    c.Parameters.Add(nameof(CreateCmdLet.Var), new[] { "x=1", "y=2" });
                    c.Parameters.Add(nameof(CreateCmdLet.WhatIf));
                });

            commandLines.Length.ShouldBe(1);
            var commandLine = commandLines[0];

            commandLine.Command.ShouldBe(CommandLineFactory.CommandCreate);
            commandLine.Connection.ShouldBe("connection string");
            commandLine.Scripts.ShouldBe(new[] { "file 1", "file 2" });
            commandLine.ConfigurationFile.ShouldBe("app.config");
            commandLine.WhatIf.ShouldBeTrue();

            commandLine.Variables.Keys.ShouldBe(new[] { "x", "y" });
            commandLine.Variables["x"].ShouldBe("1");
            commandLine.Variables["y"].ShouldBe("2");
        }

        [Test]
        [TestCase("New-SqlDatabase")]
        [TestCase("Create-SqlDatabase")]
        public void BuildPipeCommandLine(string commandName)
        {
            var commandLines = InvokeCommandPipeLine(
                commandName,
                c => c.Parameters.Add(nameof(CreateCmdLet.Database), "connection string"),
                "file 1",
                "file 2");

            commandLines.Length.ShouldBe(2);

            commandLines[0].Command.ShouldBe(CommandLineFactory.CommandCreate);
            commandLines[0].Connection.ShouldBe("connection string");
            commandLines[0].Scripts.ShouldBe(new[] { "file 1" });
            commandLines[0].InLineScript.Count.ShouldBe(0);

            commandLines[1].Command.ShouldBe(CommandLineFactory.CommandCreate);
            commandLines[1].Connection.ShouldBe("connection string");
            commandLines[1].Scripts.ShouldBe(new[] { "file 2" });
            commandLines[1].InLineScript.Count.ShouldBe(0);
        }
    }
}
