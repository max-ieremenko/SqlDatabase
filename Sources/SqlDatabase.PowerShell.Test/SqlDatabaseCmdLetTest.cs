using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Management.Automation.Runspaces;
using Moq;
using NUnit.Framework;
using SqlDatabase.Configuration;
using Command = System.Management.Automation.Runspaces.Command;

namespace SqlDatabase.PowerShell
{
    [TestFixture]
    public class SqlDatabaseCmdLetTest
    {
        private readonly IList<CommandLine> _commandLines = new List<CommandLine>();
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
                .Setup(p => p.ExecuteCommand(It.IsNotNull<CommandLine>()))
                .Callback<CommandLine>(cmd => _commandLines.Add(cmd));

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
            _invokeSqlDatabase.Parameters.Add(nameof(SqlDatabaseCmdLet.Transaction), TransactionMode.PerStep.ToString());
            _invokeSqlDatabase.Parameters.Add(nameof(SqlDatabaseCmdLet.Configuration), "app.config");
            _invokeSqlDatabase.Parameters.Add(nameof(SqlDatabaseCmdLet.Var), new[] { "x=1", "y=2" });

            _powerShell.Invoke();

            Assert.AreEqual(1, _commandLines.Count);
            var commandLine = _commandLines[0];

            Assert.IsNotNull(commandLine);
            Assert.AreEqual(Configuration.Command.Upgrade, commandLine.Command);
            Assert.AreEqual(dataBase, commandLine.Connection.ToString());
            Assert.AreEqual(2, commandLine.Scripts.Count);
            Assert.AreEqual(from1, commandLine.Scripts[0]);
            Assert.AreEqual(from2, commandLine.Scripts[1]);
            Assert.AreEqual(TransactionMode.PerStep, commandLine.Transaction);
            Assert.AreEqual("app.config", commandLine.ConfigurationFile);

            CollectionAssert.AreEquivalent(new[] { "x", "y" }, commandLine.Variables.Keys);
            Assert.AreEqual("1", commandLine.Variables["x"]);
            Assert.AreEqual("2", commandLine.Variables["y"]);
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

            Assert.AreEqual(2, _commandLines.Count);

            Assert.AreEqual(dataBase.ToString(), _commandLines[0].Connection.ToString());
            Assert.AreEqual(1, _commandLines[0].Scripts.Count);
            Assert.AreEqual(from1, _commandLines[0].Scripts[0]);

            Assert.AreEqual(dataBase.ToString(), _commandLines[1].Connection.ToString());
            Assert.AreEqual(1, _commandLines[1].Scripts.Count);
            Assert.AreEqual(from2, _commandLines[1].Scripts[0]);
        }

        private sealed class SomeSqlDatabaseCmdLet : SqlDatabaseCmdLet
        {
            public SomeSqlDatabaseCmdLet()
                : base(SqlDatabase.Configuration.Command.Upgrade)
            {
            }
        }
    }
}
