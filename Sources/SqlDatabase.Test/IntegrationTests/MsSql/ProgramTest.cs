using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Dapper;
using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Configuration;
using SqlDatabase.Scripts;
using SqlDatabase.Scripts.MsSql;
using SqlDatabase.TestApi;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace SqlDatabase.IntegrationTests.MsSql
{
    [TestFixture]
    public class ProgramTest
    {
        private readonly string _connectionString = new SqlConnectionStringBuilder(MsSqlQuery.ConnectionString) { InitialCatalog = "SqlDatabaseIT" }.ToString();

        private string _scriptsLocation;
        private AppConfiguration _configuration;
        private TempFile _logFile;

        [SetUp]
        public void BeforeEachTest()
        {
            TestPowerShellHost.GetOrCreateFactory();

            _scriptsLocation = ConfigurationManager.AppSettings["MsSql.IntegrationTestsScriptsLocation"];
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
                .BuildArray(false);

            Assert.AreEqual(0, Program.Main(args));

            const string Sql = @"
SELECT Person.Id, Person.Name, PersonAddress.City
FROM demo.Person Person
     INNER JOIN demo.PersonAddress PersonAddress ON (PersonAddress.PersonId = Person.Id)
ORDER BY Person.Id";

            Assert.AreEqual(new Version("1.2"), CreateDatabaseObject().GetCurrentVersion(null));

            using (var c = new SqlConnection(_connectionString))
            {
                c.Open();
                var rows = c.Query(Sql).ToList();
                Assert.AreEqual(2, rows.Count);

                Assert.AreEqual(1, rows[0].Id);
                Assert.AreEqual("John", rows[0].Name);
                Assert.AreEqual("London", rows[0].City);

                Assert.AreEqual(2, rows[1].Id);
                Assert.AreEqual("Maria", rows[1].Name);
                Assert.AreEqual("Paris", rows[1].City);
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
                .SetVariable("JohnSecondName", "Smitt")
                .SetVariable("MariaSecondName", "X")
                .SetLogFileName(_logFile.Location)
                .BuildArray(false);

            Assert.AreEqual(0, Program.Main(args));

            const string Sql = @"
SELECT Person.Id, Person.SecondName
FROM demo.Person Person
ORDER BY Person.Id";

            Assert.AreEqual(new Version("2.1"), CreateDatabaseObject().GetCurrentVersion(null));

            using (var c = new SqlConnection(_connectionString))
            {
                c.Open();
                var rows = c.Query(Sql).ToList();
                Assert.AreEqual(2, rows.Count);

                Assert.AreEqual(1, rows[0].Id);
                Assert.AreEqual("Smitt", rows[0].SecondName);

                Assert.AreEqual(2, rows[1].Id);
                Assert.AreEqual("X", rows[1].SecondName);
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

            Program.Main(args.BuildArray(false)).ShouldBe(0);

            const string Sql = @"
SELECT p.Name, a.City
FROM moduleA.Person p
     LEFT JOIN moduleB.PersonAddress a ON a.PersonId = p.Id
ORDER BY p.Name";

            var configuration = new SqlDatabase.Configuration.ConfigurationManager();
            configuration.LoadFrom(args.Line.ConfigurationFile);

            var db = CreateDatabaseObject(configuration.SqlDatabase);
            db.GetCurrentVersion("ModuleA").ShouldBe(new Version("2.0"));
            db.GetCurrentVersion("ModuleB").ShouldBe(new Version("1.1"));
            db.GetCurrentVersion("ModuleC").ShouldBe(new Version("2.0"));

            using (var c = new SqlConnection(_connectionString))
            {
                c.Open();
                var rows = c.Query(Sql).ToList();
                rows.Count.ShouldBe(2);

                Assert.AreEqual("John", rows[0].Name);
                Assert.AreEqual("London", rows[0].City);

                Assert.AreEqual("Maria", rows[1].Name);
                Assert.IsNull(rows[1].City);
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
                .SetExportToTable("dbo.ExportedData1")
                .BuildArray(false);

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
            using (var c = new SqlConnection(_connectionString))
            {
                c.Open();

                var test = c.ExecuteScalar("SELECT COUNT(1) FROM dbo.ExportedData1");
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
                    .SetExportToTable("dbo.ExportedData2")
                    .SetExportToFile(output.Location)
                    .BuildArray(false);

                Program.Main(args).ShouldBe(0);
                Console.WriteLine(File.ReadAllText(output.Location));

                // exec
                InvokeExecuteCommand(b => b.SetScripts(output.Location));
            }

            // test
            using (var c = new SqlConnection(_connectionString))
            {
                c.Open();

                var test = c.ExecuteScalar("SELECT COUNT(1) FROM dbo.ExportedData2");
                test.ShouldBe(2);
            }
        }

        [Test]
        [Order(6)]
        public void ExecuteScript()
        {
            InvokeExecuteCommand(b =>
                b.SetScripts(Path.Combine(_scriptsLocation, "execute", "drop.database.sql")));

            var sql = "SELECT DB_ID('{0}')".FormatWith(new SqlConnectionStringBuilder(_connectionString).InitialCatalog);

            using (var c = new SqlConnection(MsSqlQuery.ConnectionString))
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
            var args = cmd.BuildArray(false);

            Program.Main(args).ShouldBe(0);
        }

        private IDatabase CreateDatabaseObject(AppConfiguration configuration = null)
        {
            return new Database
            {
                Adapter = new MsSqlDatabaseAdapter(_connectionString, configuration ?? _configuration, new Mock<ILogger>(MockBehavior.Strict).Object)
            };
        }
    }
}
