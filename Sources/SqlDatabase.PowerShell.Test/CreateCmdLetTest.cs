using System.IO;
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
                    c.Parameters.Add(nameof(CreateCmdLet.Log), "log.txt");
                });

            commandLines.Length.ShouldBe(1);
            var commandLine = commandLines[0];

            commandLine.Command.ShouldBe(CommandLineFactory.CommandCreate);
            commandLine.Connection.ShouldBe("connection string");

            commandLine.Scripts.Count.ShouldBe(2);
            Path.IsPathRooted(commandLine.Scripts[0]).ShouldBeTrue();
            Path.GetFileName(commandLine.Scripts[0]).ShouldBe("file 1");
            Path.IsPathRooted(commandLine.Scripts[1]).ShouldBeTrue();
            Path.GetFileName(commandLine.Scripts[1]).ShouldBe("file 2");

            Path.IsPathRooted(commandLine.ConfigurationFile).ShouldBeTrue();
            Path.GetFileName(commandLine.ConfigurationFile).ShouldBe("app.config");

            Path.IsPathRooted(commandLine.LogFileName).ShouldBeTrue();
            Path.GetFileName(commandLine.LogFileName).ShouldBe("log.txt");

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
            commandLines[0].Scripts.Count.ShouldBe(1);
            Path.IsPathRooted(commandLines[0].Scripts[0]).ShouldBeTrue();
            Path.GetFileName(commandLines[0].Scripts[0]).ShouldBe("file 1");
            commandLines[0].InLineScript.Count.ShouldBe(0);

            commandLines[1].Command.ShouldBe(CommandLineFactory.CommandCreate);
            commandLines[1].Connection.ShouldBe("connection string");
            commandLines[1].Scripts.Count.ShouldBe(1);
            Path.IsPathRooted(commandLines[1].Scripts[0]).ShouldBeTrue();
            Path.GetFileName(commandLines[1].Scripts[0]).ShouldBe("file 2");
            commandLines[1].InLineScript.Count.ShouldBe(0);
        }
    }
}
