using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Moq;
using NUnit.Framework;
using SqlDatabase.Configuration;
using SqlDatabase.TestApi;

namespace SqlDatabase.Scripts
{
    [TestFixture]
    public class DatabaseTest
    {
        private Database _sut;

        private IList<string> _logOutput;

        [SetUp]
        public void BeforeEachTest()
        {
            _logOutput = new List<string>();
            var log = new Mock<ILogger>(MockBehavior.Strict);
            log
                .Setup(l => l.Error(It.IsAny<string>()))
                .Callback<string>(m =>
                {
                    Console.WriteLine("Error: {0}", m);
                    _logOutput.Add(m);
                });
            log
                .Setup(l => l.Info(It.IsAny<string>()))
                .Callback<string>(m =>
                {
                    Console.WriteLine("Info: {0}", m);
                    _logOutput.Add(m);
                });

            _sut = new Database
            {
                ConnectionString = Query.ConnectionString,
                Log = log.Object
            };
        }

        [Test]
        public void GetCurrentVersion()
        {
            string expected;
            using (var c = Query.Open())
            {
                expected = c.ExecuteScalar<string>(new AppConfiguration().GetCurrentVersionScript);
            }

            var actual = _sut.GetCurrentVersion();
            Assert.AreEqual(new Version(expected), actual);
        }

        [Test]
        public void ExecuteUpgradeNoTransactionValidateCommand()
        {
            var script = new Mock<IScript>(MockBehavior.Strict);
            script
                .Setup(s => s.Execute(It.IsNotNull<IDbCommand>(), It.IsNotNull<IVariables>(), It.IsNotNull<ILogger>()))
                .Callback<IDbCommand, IVariables, ILogger>((cmd, _, s) =>
                {
                    Assert.AreEqual(0, cmd.CommandTimeout);
                    Assert.IsNull(cmd.Transaction);
                    Assert.AreEqual(CommandType.Text, cmd.CommandType);
                    Assert.IsNotNull(cmd.Connection);
                    Assert.AreEqual(ConnectionState.Open, cmd.Connection.State);

                    cmd.CommandText = "select DB_NAME()";
                    StringAssert.AreEqualIgnoringCase(Query.DatabaseName, (string)cmd.ExecuteScalar());
                });

            _sut.Execute(script.Object, new Version("1.0"), new Version("1.0"));
            script.VerifyAll();
        }

        [Test]
        public void ExecuteUpgradeTransactionPerStepValidateCommand()
        {
            _sut.Transaction = TransactionMode.PerStep;

            var script = new Mock<IScript>(MockBehavior.Strict);
            script
                .Setup(s => s.Execute(It.IsNotNull<IDbCommand>(), It.IsNotNull<IVariables>(), It.IsNotNull<ILogger>()))
                .Callback<IDbCommand, IVariables, ILogger>((cmd, _, s) =>
                {
                    Assert.AreEqual(0, cmd.CommandTimeout);
                    Assert.IsNotNull(cmd.Transaction);
                    Assert.AreEqual(CommandType.Text, cmd.CommandType);
                    Assert.IsNotNull(cmd.Connection);
                    Assert.AreEqual(ConnectionState.Open, cmd.Connection.State);

                    cmd.CommandText = "select DB_NAME()";
                    StringAssert.AreEqualIgnoringCase(Query.DatabaseName, (string)cmd.ExecuteScalar());
                });

            _sut.Execute(script.Object, new Version("1.0"), new Version("1.0"));
            script.VerifyAll();
        }

        [Test]
        public void ExecuteUpgradeTransactionPerStepRollbackOnError()
        {
            _sut.Transaction = TransactionMode.PerStep;

            var script = new Mock<IScript>(MockBehavior.Strict);
            script
                .Setup(s => s.Execute(It.IsNotNull<IDbCommand>(), It.IsNotNull<IVariables>(), It.IsNotNull<ILogger>()))
                .Callback<IDbCommand, IVariables, ILogger>((cmd, _, s) =>
                {
                    cmd.CommandText = "create table dbo.t1( Id INT )";
                    cmd.ExecuteNonQuery();

                    throw new InvalidOperationException();
                });

            Assert.Throws<InvalidOperationException>(() => _sut.Execute(script.Object, new Version("1.0"), new Version("1.0")));
            script.VerifyAll();

            using (var c = Query.Open())
            {
                Assert.IsNull(c.ExecuteScalar<string>("select OBJECT_ID('dbo.t1')"));
            }
        }

        [Test]
        public void ExecuteUpgradeValidateVariables()
        {
            var script = new Mock<IScript>(MockBehavior.Strict);
            script
                .Setup(s => s.Execute(It.IsNotNull<IDbCommand>(), It.IsNotNull<IVariables>(), It.IsNotNull<ILogger>()))
                .Callback<IDbCommand, IVariables, ILogger>((_, vars, s) =>
                {
                    StringAssert.AreEqualIgnoringCase(Query.DatabaseName, vars.GetValue("DatabaseName"));
                    Assert.AreEqual("1.0", vars.GetValue("CurrentVersion"));
                    Assert.AreEqual("2.0", vars.GetValue("TargetVersion"));
                });

            _sut.Execute(script.Object, new Version("1.0"), new Version("2.0"));
            script.VerifyAll();
        }

        [Test]
        public void ExecuteUpgradeChangeDatabaseVersion()
        {
            var versionFrom = _sut.GetCurrentVersion();
            var versionTo = new Version(versionFrom.Major + 1, 0);

            var script = new Mock<IScript>(MockBehavior.Strict);
            script.Setup(s => s.Execute(It.IsNotNull<IDbCommand>(), It.IsNotNull<IVariables>(), It.IsNotNull<ILogger>()));

            _sut.Execute(script.Object, versionFrom, versionTo);
            script.VerifyAll();

            Assert.AreEqual(versionTo, _sut.GetCurrentVersion());
        }

        [Test]
        public void SqlOutputIntoLog()
        {
            var script = new Mock<IScript>(MockBehavior.Strict);
            script
                .Setup(s => s.Execute(It.IsNotNull<IDbCommand>(), It.IsNotNull<IVariables>(), It.IsNotNull<ILogger>()))
                .Callback<IDbCommand, IVariables, ILogger>((cmd, _, l) =>
                {
                    _logOutput.Clear();
                    cmd.CommandText = "print 'xx1xx'";
                    cmd.ExecuteNonQuery();

                    Assert.AreEqual(1, _logOutput.Count);
                    StringAssert.Contains("xx1xx", _logOutput[0]);

                    _logOutput.Clear();
                    cmd.CommandText = "use master";
                    cmd.ExecuteNonQuery();

                    Assert.AreEqual(1, _logOutput.Count);
                    StringAssert.Contains("master", _logOutput[0]);
                });

            _sut.Execute(script.Object, new Version("1.0"), new Version("2.0"));

            script.VerifyAll();
        }

        [Test]
        public void ExecuteSetTargetDatabase()
        {
            var currentDatabaseName = new SqlConnectionStringBuilder(_sut.ConnectionString).InitialCatalog;

            var script = new Mock<IScript>(MockBehavior.Strict);
            script
                .Setup(s => s.Execute(It.IsNotNull<IDbCommand>(), It.IsNotNull<IVariables>(), It.IsNotNull<ILogger>()))
                .Callback<IDbCommand, IVariables, ILogger>((cmd, vars, s) =>
                {
                    cmd.CommandText = "select db_name()";
                    StringAssert.AreEqualIgnoringCase(currentDatabaseName, (string)cmd.ExecuteScalar());
                });

            _sut.Execute(script.Object);
            script.VerifyAll();
        }

        [Test]
        public void ExecuteSetMasterDatabase()
        {
            var builder = new SqlConnectionStringBuilder(_sut.ConnectionString)
            {
                InitialCatalog = Guid.NewGuid().ToString()
            };

            _sut.ConnectionString = builder.ToString();

            var script = new Mock<IScript>(MockBehavior.Strict);
            script
                .Setup(s => s.Execute(It.IsNotNull<IDbCommand>(), It.IsNotNull<IVariables>(), It.IsNotNull<ILogger>()))
                .Callback<IDbCommand, IVariables, ILogger>((cmd, vars, s) =>
                {
                    cmd.CommandText = "select db_name()";
                    StringAssert.AreEqualIgnoringCase("master", (string)cmd.ExecuteScalar());
                });

            _sut.Execute(script.Object);
            script.VerifyAll();
        }
    }
}