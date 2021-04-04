using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Configuration;
using SqlDatabase.TestApi;

namespace SqlDatabase.Scripts
{
    [TestFixture]
    public class DatabaseTest
    {
        private const string ModuleName = "SomeModuleName";
        private const string SelectModuleVersion = "SELECT value from sys.fn_listextendedproperty('version-{{ModuleName}}', default, default, default, default, default, default)";
        private const string UpdateModuleVersion = "EXEC sys.sp_updateextendedproperty @name=N'version-{{ModuleName}}', @value=N'{{TargetVersion}}'";

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
                Log = log.Object,
                Configuration = new AppConfiguration()
            };
        }

        [Test]
        public void GetCurrentVersionDefault()
        {
            string expected;
            using (var c = Query.Open())
            {
                expected = c.ExecuteScalar<string>(new AppConfiguration().GetCurrentVersionScript);
            }

            var actual = _sut.GetCurrentVersion(null);
            Assert.AreEqual(new Version(expected), actual);
        }

        [Test]
        public void GetCurrentVersionModuleName()
        {
            _sut.Configuration.GetCurrentVersionScript = SelectModuleVersion;
            _sut.Configuration.SetCurrentVersionScript = UpdateModuleVersion;

            string expected;
            using (var c = Query.Open())
            {
                expected = c.ExecuteScalar<string>(SelectModuleVersion.Replace("{{ModuleName}}", ModuleName));
            }

            var actual = _sut.GetCurrentVersion(ModuleName);
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

            _sut.Execute(script.Object, string.Empty, new Version("1.0"), new Version("1.0"));
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

            _sut.Execute(script.Object, string.Empty, new Version("1.0"), new Version("1.0"));

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

            Assert.Throws<InvalidOperationException>(() => _sut.Execute(script.Object, string.Empty, new Version("1.0"), new Version("1.0")));
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
                    Assert.AreEqual("module name", vars.GetValue("ModuleName"));
                    Assert.AreEqual("1.0", vars.GetValue("CurrentVersion"));
                    Assert.AreEqual("2.0", vars.GetValue("TargetVersion"));
                });

            _sut.Execute(script.Object, "module name", new Version("1.0"), new Version("2.0"));
            script.VerifyAll();
        }

        [Test]
        public void ExecuteUpgradeWhatIf()
        {
            var script = new Mock<IScript>(MockBehavior.Strict);
            script
                .Setup(s => s.Execute(null, It.IsNotNull<IVariables>(), It.IsNotNull<ILogger>()));

            _sut.WhatIf = true;
            _sut.Execute(script.Object, "module name", new Version("1.0"), new Version("2.0"));
            script.VerifyAll();
        }

        [Test]
        public void ExecuteUpgradeChangeDatabaseVersionNoModules()
        {
            var versionFrom = _sut.GetCurrentVersion(null);
            var versionTo = new Version(versionFrom.Major + 1, 0);

            var script = new Mock<IScript>(MockBehavior.Strict);
            script.Setup(s => s.Execute(It.IsNotNull<IDbCommand>(), It.IsNotNull<IVariables>(), It.IsNotNull<ILogger>()));

            _sut.Execute(script.Object, null, versionFrom, versionTo);
            script.VerifyAll();

            Assert.AreEqual(versionTo, _sut.GetCurrentVersion(null));
        }

        [Test]
        public void ExecuteUpgradeChangeDatabaseVersionModuleName()
        {
            _sut.Configuration.GetCurrentVersionScript = SelectModuleVersion;
            _sut.Configuration.SetCurrentVersionScript = UpdateModuleVersion;

            var versionFrom = _sut.GetCurrentVersion(ModuleName);
            var versionTo = new Version(versionFrom.Major + 1, 0);

            var script = new Mock<IScript>(MockBehavior.Strict);
            script.Setup(s => s.Execute(It.IsNotNull<IDbCommand>(), It.IsNotNull<IVariables>(), It.IsNotNull<ILogger>()));

            _sut.Execute(script.Object, ModuleName, versionFrom, versionTo);
            script.VerifyAll();

            Assert.AreEqual(versionTo, _sut.GetCurrentVersion(ModuleName));
        }

        [Test]
        public void ExecuteUpgradeChangeDatabaseVersionValidateVersion()
        {
            _sut.Configuration.GetCurrentVersionScript = "SELECT '3.0'";
            _sut.Configuration.SetCurrentVersionScript = "SELECT 1";

            var script = new Mock<IScript>(MockBehavior.Strict);
            script.Setup(s => s.Execute(It.IsNotNull<IDbCommand>(), It.IsNotNull<IVariables>(), It.IsNotNull<ILogger>()));

            var ex = Assert.Throws<InvalidOperationException>(() => _sut.Execute(script.Object, null, new Version("1.0"), new Version("2.0")));
            script.VerifyAll();

            ex.Message.ShouldContain("3.0");
            ex.Message.ShouldContain("2.0");
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

            _sut.Execute(script.Object, null, new Version("1.0"), new Version("2.0"));

            script.VerifyAll();
        }

        [Test]
        public void ExecuteNoTransactionValidateCommand()
        {
            var script = new Mock<IScript>(MockBehavior.Strict);
            script
                .Setup(s => s.Execute(It.IsNotNull<IDbCommand>(), It.IsNotNull<IVariables>(), It.IsNotNull<ILogger>()))
                .Callback<IDbCommand, IVariables, ILogger>((cmd, _, s) =>
                {
                    cmd.CommandTimeout.ShouldBe(0);
                    cmd.Transaction.ShouldBeNull();
                    cmd.CommandType.ShouldBe(CommandType.Text);
                    cmd.Connection.ShouldNotBeNull();
                    cmd.Connection.State.ShouldBe(ConnectionState.Open);

                    cmd.CommandText = "select DB_NAME()";
                    cmd.ExecuteScalar().ShouldBeOfType<string>().ShouldBe(Query.DatabaseName, StringCompareShould.IgnoreCase);
                });

            _sut.Execute(script.Object);

            script.VerifyAll();
        }

        [Test]
        public void ExecuteTransactionPerStepValidateCommand()
        {
            var script = new Mock<IScript>(MockBehavior.Strict);
            script
                .Setup(s => s.Execute(It.IsNotNull<IDbCommand>(), It.IsNotNull<IVariables>(), It.IsNotNull<ILogger>()))
                .Callback<IDbCommand, IVariables, ILogger>((cmd, _, s) =>
                {
                    cmd.CommandTimeout.ShouldBe(0);
                    cmd.Transaction.ShouldNotBeNull();
                    cmd.CommandType.ShouldBe(CommandType.Text);
                    cmd.Connection.ShouldNotBeNull();
                    cmd.Connection.State.ShouldBe(ConnectionState.Open);

                    cmd.CommandText = "select DB_NAME()";
                    cmd.ExecuteScalar().ShouldBeOfType<string>().ShouldBe(Query.DatabaseName, StringCompareShould.IgnoreCase);
                });

            _sut.Transaction = TransactionMode.PerStep;
            _sut.Execute(script.Object);

            script.VerifyAll();
        }

        [Test]
        public void ExecuteTransactionPerStepRollbackOnError()
        {
            var script = new Mock<IScript>(MockBehavior.Strict);
            script
                .Setup(s => s.Execute(It.IsNotNull<IDbCommand>(), It.IsNotNull<IVariables>(), It.IsNotNull<ILogger>()))
                .Callback<IDbCommand, IVariables, ILogger>((cmd, _, s) =>
                {
                    cmd.CommandText = "create table dbo.t1( Id INT )";
                    cmd.ExecuteNonQuery();

                    throw new InvalidOperationException();
                });

            _sut.Transaction = TransactionMode.PerStep;
            Assert.Throws<InvalidOperationException>(() => _sut.Execute(script.Object));

            script.VerifyAll();

            using (var c = Query.Open())
            {
                Assert.IsNull(c.ExecuteScalar<string>("select OBJECT_ID('dbo.t1')"));
            }
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

        [Test]
        public void ExecuteWhatIf()
        {
            var script = new Mock<IScript>(MockBehavior.Strict);
            script
                .Setup(s => s.Execute(null, It.IsNotNull<IVariables>(), It.IsNotNull<ILogger>()));

            _sut.WhatIf = true;
            _sut.Execute(script.Object);
            script.VerifyAll();
        }
    }
}