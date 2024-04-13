using NUnit.Framework;
using Shouldly;
using SqlDatabase.CommandLine;
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
        var actual = commandLines[0].ShouldBeOfType<ExportCommandLine>();

        actual.Database.ShouldBe("connection string");
        actual.From.ShouldBe([
            new(false, "file 1"),
            new(false, "file 2"),
            new(true, "sql text 1"),
            new(true, "sql text 2")
        ]);
        actual.DestinationTableName.ShouldBe("to table");
        actual.DestinationFileName.ShouldBe("to file");
        actual.Configuration.ShouldBe("app.config");
        actual.Log.ShouldBe("log.txt");

        actual.Variables.Keys.ShouldBe(new[] { "x", "y" });
        actual.Variables["x"].ShouldBe("1");
        actual.Variables["y"].ShouldBe("2");
    }
}