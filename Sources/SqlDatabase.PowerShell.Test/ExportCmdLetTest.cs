using NUnit.Framework;
using Shouldly;
using SqlDatabase.Configuration;
using SqlDatabase.PowerShell.TestApi;

namespace SqlDatabase.PowerShell;

[TestFixture]
public class ExportCmdLetTest : SqlDatabaseCmdLetTest<ExportCmdLet>
{
    [Test]
    public void BuildCommandLine()
    {
        var commandLines = InvokeSqlDatabase(
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
                c.Parameters.Add(nameof(CreateCmdLet.Log), "log.txt");
            });

        commandLines.Length.ShouldBe(1);
        var commandLine = commandLines[0];

        commandLine.Command.ShouldBe(CommandLineFactory.CommandExport);
        commandLine.Connection.ShouldBe("connection string");

        commandLine.Scripts.Count.ShouldBe(2);
        Path.IsPathRooted(commandLine.Scripts[0]).ShouldBeTrue();
        Path.GetFileName(commandLine.Scripts[0]).ShouldBe("file 1");
        Path.IsPathRooted(commandLine.Scripts[1]).ShouldBeTrue();
        Path.GetFileName(commandLine.Scripts[1]).ShouldBe("file 2");

        commandLine.InLineScript.ShouldBe(new[] { "sql text 1", "sql text 2" });
        commandLine.ExportToFile.ShouldBe("to file");
        commandLine.ExportToTable.ShouldBe("to table");

        Path.IsPathRooted(commandLine.ConfigurationFile).ShouldBeTrue();
        Path.GetFileName(commandLine.ConfigurationFile).ShouldBe("app.config");

        Path.IsPathRooted(commandLine.LogFileName).ShouldBeTrue();
        Path.GetFileName(commandLine.LogFileName).ShouldBe("log.txt");

        commandLine.Variables.Keys.ShouldBe(new[] { "x", "y" });
        commandLine.Variables["x"].ShouldBe("1");
        commandLine.Variables["y"].ShouldBe("2");
    }
}