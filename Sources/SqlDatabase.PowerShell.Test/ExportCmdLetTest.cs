using NUnit.Framework;
using Shouldly;
using SqlDatabase.Configuration;
using SqlDatabase.PowerShell.TestApi;

namespace SqlDatabase.PowerShell
{
    [TestFixture]
    public class ExportCmdLetTest : SqlDatabaseCmdLetTest<ExportCmdLet>
    {
        [Test]
        public void BuildCommandLine()
        {
            var commandLines = InvokeCommand(
                "Export-SqlDatabase",
                c =>
                {
                    c.Parameters.Add(nameof(ExportCmdLet.Database), "connection string");
                    c.Parameters.Add(nameof(ExportCmdLet.From), new[] { "file 1", "file 2" });
                    c.Parameters.Add(nameof(ExportCmdLet.FromSql), new[] { "sql text 1", "sql text 2" });
                    c.Parameters.Add(nameof(ExportCmdLet.ToFile), "to file");
                    c.Parameters.Add(nameof(ExportCmdLet.ToTable), "to table");
                    c.Parameters.Add(nameof(ExportCmdLet.Configuration), "app.config");
                    c.Parameters.Add(nameof(ExportCmdLet.Var), new[] { "x=1", "y=2" });
                });

            commandLines.Length.ShouldBe(1);
            var commandLine = commandLines[0];

            commandLine.Command.ShouldBe(CommandLineFactory.CommandExport);
            commandLine.Connection.ShouldBe("connection string");
            commandLine.Scripts.ShouldBe(new[] { "file 1", "file 2" });
            commandLine.InLineScript.ShouldBe(new[] { "sql text 1", "sql text 2" });
            commandLine.ExportToFile.ShouldBe("to file");
            commandLine.ExportToTable.ShouldBe("to table");
            commandLine.ConfigurationFile.ShouldBe("app.config");

            commandLine.Variables.Keys.ShouldBe(new[] { "x", "y" });
            commandLine.Variables["x"].ShouldBe("1");
            commandLine.Variables["y"].ShouldBe("2");
        }
    }
}
