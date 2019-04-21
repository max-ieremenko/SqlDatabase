using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Management.Automation.Runspaces;
using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Configuration;
using Command = System.Management.Automation.Runspaces.Command;

namespace SqlDatabase.PowerShell
{
    [TestFixture]
    public class SqlDatabaseCmdLetTest
    {
        private const string Command = "Some Command";

        private readonly IList<GenericCommandLine> _commandLines = new List<GenericCommandLine>();
        private Runspace _runSpace;
        private System.Management.Automation.PowerShell _powerShell;
        private Command _invokeSqlDatabase;

        [SetUp]
        public void BeforeEachTest()
        {
            var sessionState = InitialSessionState.CreateDefault();
            sessionState.Commands.Add(new SessionStateCmdletEntry("Test-SqlDatabase", typeof(SomeSqlDatabaseCmdLet), null));

            _runSpace = RunspaceFactory.CreateRunspace(sessionState);
            _runSpace.Open();

            _powerShell = System.Management.Automation.PowerShell.Create();
            _powerShell.Runspace = _runSpace;

            _invokeSqlDatabase = new Command("Test-SqlDatabase");
            _powerShell.Commands.AddCommand(_invokeSqlDatabase);

            var program = new Mock<ISqlDatabaseProgram>(MockBehavior.Strict);
            program
                .Setup(p => p.ExecuteCommand(It.IsNotNull<GenericCommandLine>()))
                .Callback<GenericCommandLine>(cmd => _commandLines.Add(cmd));

            _commandLines.Clear();
            SqlDatabaseCmdLet.Program = program.Object;
        }

        [TearDown]
        public void AfterEachTest()
        {
            SqlDatabaseCmdLet.Program = null;

            foreach (var row in _powerShell.Streams.Information)
            {
                Console.WriteLine(row);
            }

            _powerShell?.Dispose();
            _runSpace?.Dispose();
        }

        [Test]
        public void InvokeValidateCommandLine()
        {
            var dataBase = new SqlConnectionStringBuilder
            {
                DataSource = ".",
                InitialCatalog = "abc"
            }.ToString();

            var from1 = GetType().Assembly.Location;
            var from2 = Path.GetDirectoryName(from1);

            _invokeSqlDatabase.Parameters.Add(nameof(SqlDatabaseCmdLet.Database), dataBase);
            _invokeSqlDatabase.Parameters.Add(nameof(SqlDatabaseCmdLet.From), new[] { from1, from2 });
            _invokeSqlDatabase.Parameters.Add(nameof(SqlDatabaseCmdLet.FromSql), new[] { "sql text 1", "sql text 2" });
            _invokeSqlDatabase.Parameters.Add(nameof(SqlDatabaseCmdLet.Transaction), TransactionMode.PerStep.ToString());
            _invokeSqlDatabase.Parameters.Add(nameof(SqlDatabaseCmdLet.Configuration), "app.config");
            _invokeSqlDatabase.Parameters.Add(nameof(SqlDatabaseCmdLet.Var), new[] { "x=1", "y=2" });

            _powerShell.Invoke();

            _commandLines.Count.ShouldBe(1);

            var commandLine = _commandLines[0];

            commandLine.ShouldNotBeNull();
            commandLine.Command.ShouldBe(Command);
            commandLine.Connection.ToString().ShouldBe(dataBase);
            commandLine.Transaction.ShouldBe(TransactionMode.PerStep);
            commandLine.ConfigurationFile.ShouldBe("app.config");

            commandLine.Scripts.Count.ShouldBe(2);
            commandLine.Scripts[0].ShouldBe(from1);
            commandLine.Scripts[1].ShouldBe(from2);

            commandLine.InLineScript.Count.ShouldBe(2);
            commandLine.InLineScript[0].ShouldBe("sql text 1");
            commandLine.InLineScript[1].ShouldBe("sql text 2");

            commandLine.Variables.Keys.ShouldBe(new[] { "x", "y" });
            commandLine.Variables["x"].ShouldBe("1");
            commandLine.Variables["y"].ShouldBe("2");
        }

        [Test]
        public void InvokeInPipeLine()
        {
            var dataBase = new SqlConnectionStringBuilder
            {
                DataSource = ".",
                InitialCatalog = "abc"
            }.ToString();

            var from1 = GetType().Assembly.Location;
            var from2 = typeof(SqlDatabaseCmdLet).Assembly.Location;

            _invokeSqlDatabase.Parameters.Add(nameof(SqlDatabaseCmdLet.Database), dataBase);
            _powerShell.Invoke(new[] { from1, from2 });

            _commandLines.Count.ShouldBe(2);

            _commandLines[0].Connection.ToString().ShouldBe(dataBase);
            _commandLines[0].Scripts.Count.ShouldBe(1);
            _commandLines[0].Scripts[0].ShouldBe(from1);

            _commandLines[1].Connection.ToString().ShouldBe(dataBase);
            _commandLines[1].Scripts.Count.ShouldBe(1);
            _commandLines[1].Scripts[0].ShouldBe(from2);
        }

        private sealed class SomeSqlDatabaseCmdLet : SqlDatabaseCmdLet
        {
            public SomeSqlDatabaseCmdLet()
                : base(Command)
            {
            }
        }
    }
}
