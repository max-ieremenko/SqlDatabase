using System;
using System.IO;
using System.Linq;
using Dapper;
using Moq;
using Npgsql;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Adapter;
using SqlDatabase.Configuration;
using SqlDatabase.Scripts;
using SqlDatabase.Scripts.PgSql;
using SqlDatabase.TestApi;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace SqlDatabase.IntegrationTests.PgSql;

[TestFixture]
public class ProgramTest
{
    private readonly string _connectionString = new NpgsqlConnectionStringBuilder(PgSqlQuery.ConnectionString) { Database = "sqldatabasetest_it" }.ToString();

    private string _scriptsLocation = null!;
    private AppConfiguration _configuration = null!;
    private TempFile _logFile = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        TestPowerShellHost.GetOrCreateFactory();

        _scriptsLocation = ConfigurationManager.AppSettings["PgSql.IntegrationTestsScriptsLocation"]!;
        if (!Path.IsPathRooted(_scriptsLocation))
        {
            _scriptsLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _scriptsLocation);
        }

        _configuration = new AppConfiguration();

        _logFile = new TempFile(".log");
    }

    [TearDown]
    public void AfterEachTest()
    {
        FileAssert.Exists(_logFile.Location);
        var fileContent = File.ReadAllLines(_logFile.Location);
        _logFile.Dispose();

        fileContent.ShouldNotBeEmpty();
    }

    [Test]
    [Order(1)]

    public void CreateDatabase()
    {
        var args = new GenericCommandLineBuilder()
            .SetCommand(CommandLineFactory.CommandCreate)
            .SetConnection(_connectionString)
            .SetScripts(Path.Combine(_scriptsLocation, "new"))
            .SetVariable("JohnCity", "London")
            .SetVariable("MariaCity", "Paris")
            .SetLogFileName(_logFile.Location)
            .BuildArray();

        Program.Main(args).ShouldBe(0);

        const string Sql = @"
SELECT person.id, person.name, person_address.city
FROM demo.person person
     INNER JOIN demo.person_address person_address ON (person_address.person_id = person.id)
ORDER BY person.id";

        CreateDatabaseObject().GetCurrentVersion(null).ShouldBe(new Version("1.2"));

        using (var c = new NpgsqlConnection(_connectionString))
        {
            c.Open();

            var rows = c.Query(Sql).ToList();
            Assert.AreEqual(2, rows.Count);

            Assert.AreEqual(1, rows[0].id);
            Assert.AreEqual("John", rows[0].name);
            Assert.AreEqual("London", rows[0].city);

            Assert.AreEqual(2, rows[1].id);
            Assert.AreEqual("Maria", rows[1].name);
            Assert.AreEqual("Paris", rows[1].city);
        }
    }

    [Test]
    [Order(2)]
    public void UpgradeDatabase()
    {
        var args = new GenericCommandLineBuilder()
            .SetCommand(CommandLineFactory.CommandUpgrade)
            .SetConnection(_connectionString)
            .SetScripts(Path.Combine(_scriptsLocation, "upgrade"))
            .SetConfigurationFile(Path.Combine(_scriptsLocation, "Upgrade", "SqlDatabase.exe.config"))
            .SetVariable("JohnSecondName", "Smitt")
            .SetVariable("MariaSecondName", "X")
            .SetLogFileName(_logFile.Location)
            .BuildArray();

        Program.Main(args).ShouldBe(0);

        const string Sql = @"
SELECT person.id, person.second_name
FROM demo.person person
ORDER BY person.id";

        CreateDatabaseObject().GetCurrentVersion(null).ShouldBe(new Version("2.1"));

        using (var c = new NpgsqlConnection(_connectionString))
        {
            c.Open();
            var rows = c.Query(Sql).ToList();
            Assert.AreEqual(2, rows.Count);

            Assert.AreEqual(1, rows[0].id);
            Assert.AreEqual("Smitt", rows[0].second_name);

            Assert.AreEqual(2, rows[1].id);
            Assert.AreEqual("X", rows[1].second_name);
        }
    }

    [Test]
    [Order(3)]
    public void UpgradeDatabaseModularity()
    {
        var args = new GenericCommandLineBuilder()
            .SetCommand(CommandLineFactory.CommandUpgrade)
            .SetConnection(_connectionString)
            .SetScripts(Path.Combine(_scriptsLocation, "UpgradeModularity"))
            .SetConfigurationFile(Path.Combine(_scriptsLocation, "UpgradeModularity", "SqlDatabase.exe.config"))
            .SetLogFileName(_logFile.Location);

        Program.Main(args.BuildArray()).ShouldBe(0);

        const string Sql = @"
SELECT p.name, a.city
FROM module_a.person p
     LEFT JOIN module_b.person_address a ON a.person_id = p.id
ORDER BY p.name";

        var configuration = new SqlDatabase.Configuration.ConfigurationManager();
        configuration.LoadFrom(args.Line.ConfigurationFile);

        var db = CreateDatabaseObject(configuration.SqlDatabase);
        db.GetCurrentVersion("ModuleA").ShouldBe(new Version("2.0"));
        db.GetCurrentVersion("ModuleB").ShouldBe(new Version("1.1"));
        db.GetCurrentVersion("ModuleC").ShouldBe(new Version("2.0"));

        using (var c = new NpgsqlConnection(_connectionString))
        {
            c.Open();
            var rows = c.Query(Sql).ToList();
            rows.Count.ShouldBe(2);

            Assert.AreEqual("John", rows[0].name);
            Assert.AreEqual("London", rows[0].city);

            Assert.AreEqual("Maria", rows[1].name);
            Assert.IsNull(rows[1].city);
        }
    }

    [Test]
    [Order(4)]
    public void ExportDataToConsole()
    {
        // export
        var args = new GenericCommandLineBuilder()
            .SetCommand(CommandLineFactory.CommandExport)
            .SetConnection(_connectionString)
            .SetScripts(Path.Combine(_scriptsLocation, @"Export\export.sql"))
            .SetExportToTable("public.exported_data1")
            .BuildArray();

        int exitCode;
        string output;
        using (var console = new TempConsoleOut())
        {
            exitCode = Program.Main(args);
            output = console.GetOutput();
        }

        Console.WriteLine(output);
        exitCode.ShouldBe(0);

        // exec
        InvokeExecuteCommand(b => b.SetInLineScript(output));

        // test
        using (var c = new NpgsqlConnection(_connectionString))
        {
            c.Open();

            var test = c.ExecuteScalar("SELECT COUNT(1) FROM public.exported_data1");
            test.ShouldBe(2);
        }
    }

    [Test]
    [Order(5)]
    public void ExportDataToFile()
    {
        using (var output = new TempFile(".sql"))
        {
            // export
            var args = new GenericCommandLineBuilder()
                .SetCommand(CommandLineFactory.CommandExport)
                .SetConnection(_connectionString)
                .SetScripts(Path.Combine(_scriptsLocation, @"Export\export.sql"))
                .SetExportToTable("public.exported_data2")
                .SetExportToFile(output.Location)
                .BuildArray();

            Program.Main(args).ShouldBe(0);
            Console.WriteLine(File.ReadAllText(output.Location));

            // exec
            InvokeExecuteCommand(b => b.SetScripts(output.Location));
        }

        // test
        using (var c = new NpgsqlConnection(_connectionString))
        {
            c.Open();

            var test = c.ExecuteScalar("SELECT COUNT(1) FROM public.exported_data2");
            test.ShouldBe(2);
        }
    }

    [Test]
    [Order(6)]
    public void ExecuteScript()
    {
        InvokeExecuteCommand(b =>
            b.SetScripts(Path.Combine(_scriptsLocation, "execute", "drop.database.ps1")));

        var sql = "SELECT 1 FROM PG_DATABASE WHERE LOWER(DATNAME) = LOWER('{0}')".FormatWith(new NpgsqlConnectionStringBuilder(_connectionString).Database);

        using (var c = new NpgsqlConnection(PgSqlQuery.ConnectionString))
        {
            c.Open();

            var test = c.ExecuteScalar(sql);
            Assert.IsNull(test);
        }
    }

    private void InvokeExecuteCommand(Action<GenericCommandLineBuilder> builder)
    {
        var cmd = new GenericCommandLineBuilder()
            .SetCommand(CommandLineFactory.CommandExecute)
            .SetConnection(_connectionString)
            .SetLogFileName(_logFile.Location);

        builder(cmd);
        var args = cmd.BuildArray();

        Program.Main(args).ShouldBe(0);
    }

    private IDatabase CreateDatabaseObject(AppConfiguration? configuration = null)
    {
        var log = new Mock<ILogger>(MockBehavior.Strict).Object;
        var adapter = new PgSqlDatabaseAdapter(_connectionString, configuration ?? _configuration, log);
        return new Database(adapter, log, TransactionMode.None, false);
    }
}