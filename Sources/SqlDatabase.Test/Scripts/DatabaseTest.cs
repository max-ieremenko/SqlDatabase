using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Configuration;

namespace SqlDatabase.Scripts;

[TestFixture]
public class DatabaseTest
{
    private Database _sut = null!;
    private Mock<IDatabaseAdapter> _adapter = null!;
    private Mock<IDbCommand> _command = null!;
    private Mock<IDbConnection> _connection = null!;
    private Mock<IDbTransaction> _transaction = null!;
    private IList<string> _logOutput = null!;

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

        _transaction = new Mock<IDbTransaction>(MockBehavior.Strict);
        _transaction
            .Setup(t => t.Dispose());
        _transaction
            .Setup(t => t.Commit());

        _command = new Mock<IDbCommand>(MockBehavior.Strict);
        _command
            .SetupProperty(c => c.CommandText);
        _command
            .Setup(c => c.Dispose());

        _connection = new Mock<IDbConnection>(MockBehavior.Strict);
        _connection
            .Setup(c => c.Open());
        _connection
            .Setup(c => c.CreateCommand())
            .Returns(_command.Object);
        _connection
            .Setup(c => c.Dispose());

        _adapter = new Mock<IDatabaseAdapter>(MockBehavior.Strict);

        _sut = new Database(_adapter.Object, log.Object, TransactionMode.None, false);
    }

    [Test]
    public void GetCurrentVersion()
    {
        _command
            .SetupProperty(c => c.CommandTimeout, 30);
        _command
            .Setup(c => c.ExecuteScalar())
            .Callback(() =>
            {
                _command.Object.CommandTimeout.ShouldBe(0);
                _command.Object.CommandText.ShouldBe("select 1");
            })
            .Returns("1.1");

        _adapter
            .Setup(a => a.CreateConnection(false))
            .Returns(_connection.Object);
        _adapter
            .Setup(a => a.GetVersionSelectScript())
            .Returns("select 1");

        var actual = _sut.GetCurrentVersion(null);

        actual.ShouldBe(new Version("1.1"));

        _adapter.VerifyAll();
        _connection.VerifyAll();
        _command.VerifyAll();
    }

    [Test]
    public void GetCurrentVersionModuleName()
    {
        _command
            .SetupProperty(c => c.CommandTimeout, 30);
        _command
            .Setup(c => c.ExecuteScalar())
            .Callback(() =>
            {
                _command.Object.CommandTimeout.ShouldBe(0);
                _command.Object.CommandText.ShouldBe("select 'my module name'");
            })
            .Returns("1.1");

        _adapter
            .Setup(a => a.CreateConnection(false))
            .Returns(_connection.Object);
        _adapter
            .Setup(a => a.GetVersionSelectScript())
            .Returns("select '{{ModuleName}}'");

        var actual = _sut.GetCurrentVersion("my module name");

        actual.ShouldBe(new Version("1.1"));

        _adapter.VerifyAll();
        _connection.VerifyAll();
        _command.VerifyAll();
    }

    [Test]
    public void GetCurrentVersionInvalidScript()
    {
        var ex = new Mock<DbException>();

        _command
            .SetupProperty(c => c.CommandTimeout, 30);
        _command
            .Setup(c => c.ExecuteScalar())
            .Throws(ex.Object);

        _adapter
            .Setup(a => a.CreateConnection(false))
            .Returns(_connection.Object);
        _adapter
            .Setup(a => a.GetVersionSelectScript())
            .Returns("select 1");

        var actual = Assert.Throws<InvalidOperationException>(() => _sut.GetCurrentVersion(null));

        actual!.InnerException.ShouldBe(ex.Object);
        actual.Message.ShouldContain("select 1");
    }

    [Test]
    public void GetCurrentVersionInvalidVersion()
    {
        _command
            .SetupProperty(c => c.CommandTimeout, 30);
        _command
            .Setup(c => c.ExecuteScalar())
            .Returns("abc");

        _adapter
            .Setup(a => a.CreateConnection(false))
            .Returns(_connection.Object);
        _adapter
            .Setup(a => a.GetVersionSelectScript())
            .Returns("select 1");

        var actual = Assert.Throws<InvalidOperationException>(() => _sut.GetCurrentVersion(null));

        actual!.Message.ShouldContain("abc");
    }

    [Test]
    public void GetCurrentVersionModuleNameInvalidVersion()
    {
        _command
            .SetupProperty(c => c.CommandTimeout, 30);
        _command
            .Setup(c => c.ExecuteScalar())
            .Returns("abc");

        _adapter
            .Setup(a => a.CreateConnection(false))
            .Returns(_connection.Object);
        _adapter
            .Setup(a => a.GetVersionSelectScript())
            .Returns("select 1");

        var actual = Assert.Throws<InvalidOperationException>(() => _sut.GetCurrentVersion("my module-name"));

        actual!.Message.ShouldContain("abc");
        actual.Message.ShouldContain("my module-name");
    }

    [Test]
    public void GetServerVersion()
    {
        _adapter
            .Setup(a => a.GetServerVersionSelectScript())
            .Returns("select server version");
        _adapter
            .Setup(a => a.CreateConnection(true))
            .Returns(_connection.Object);

        _command
            .Setup(c => c.ExecuteScalar())
            .Callback(() =>
            {
                _command.Object.CommandText.ShouldBe("select server version");
            })
            .Returns("server version");

        var actual = _sut.GetServerVersion();

        actual.ShouldBe("server version");
    }

    [Test]
    [TestCase(TransactionMode.None)]
    [TestCase(TransactionMode.PerStep)]
    public void ExecuteUpgrade(TransactionMode transaction)
    {
        _sut.Transaction = transaction;

        _command
            .SetupProperty(c => c.CommandTimeout, 30);
        _command
            .SetupProperty(c => c.Transaction);

        _connection
            .Setup(c => c.BeginTransaction(IsolationLevel.ReadCommitted))
            .Returns(_transaction.Object);

        _adapter
            .SetupGet(a => a.DatabaseName)
            .Returns("database-name");
        _adapter
            .Setup(a => a.CreateConnection(false))
            .Returns(_connection.Object);

        var script = new Mock<IScript>(MockBehavior.Strict);
        script
            .Setup(s => s.Execute(_command.Object, It.IsNotNull<IVariables>(), It.IsNotNull<ILogger>()))
            .Callback<IDbCommand, IVariables, ILogger>((cmd, variables, s) =>
            {
                cmd.CommandTimeout.ShouldBe(0);

                if (transaction == TransactionMode.PerStep)
                {
                    cmd.Transaction.ShouldBe(_transaction.Object);
                }
                else
                {
                    cmd.Transaction.ShouldBeNull();
                }

                variables.GetValue("DatabaseName").ShouldBe("database-name");
                variables.GetValue("CurrentVersion").ShouldBe("1.0");
                variables.GetValue("TargetVersion").ShouldBe("2.0");
                variables.GetValue("ModuleName").ShouldBe("my module");

                _adapter
                    .Setup(a => a.GetVersionUpdateScript())
                    .Returns("update version");
                _adapter
                    .Setup(a => a.GetVersionSelectScript())
                    .Returns("select version");

                _command
                    .Setup(c => c.ExecuteNonQuery())
                    .Callback(() => _command.Object.CommandText.ShouldBe("update version"))
                    .Returns(0);
                _command
                    .Setup(c => c.ExecuteScalar())
                    .Callback(() => _command.Object.CommandText.ShouldBe("select version"))
                    .Returns("2.0");
            });

        _sut.Execute(script.Object, "my module", new Version("1.0"), new Version("2.0"));

        script.VerifyAll();
        _command.VerifyAll();
    }

    [Test]
    public void ExecuteUpgradeWhatIf()
    {
        _adapter
            .SetupGet(a => a.DatabaseName)
            .Returns("database-name");

        var script = new Mock<IScript>(MockBehavior.Strict);
        script
            .Setup(s => s.Execute(null, It.IsNotNull<IVariables>(), It.IsNotNull<ILogger>()))
            .Callback<IDbCommand, IVariables, ILogger>((cmd, variables, s) =>
            {
                variables.GetValue("DatabaseName").ShouldBe("database-name");
                variables.GetValue("CurrentVersion").ShouldBe("1.0");
                variables.GetValue("TargetVersion").ShouldBe("2.0");
                variables.GetValue("ModuleName").ShouldBe("my module");
            });

        _sut.WhatIf = true;

        _sut.Execute(script.Object, "my module", new Version("1.0"), new Version("2.0"));

        script.VerifyAll();

        _logOutput.Count.ShouldBe(1);
        _logOutput[0].ShouldBe("what-if mode");
    }

    [Test]
    [TestCase(TransactionMode.None)]
    [TestCase(TransactionMode.PerStep)]
    public void Execute(TransactionMode transaction)
    {
        _sut.Transaction = transaction;

        _command
            .SetupProperty(c => c.CommandTimeout, 30);
        _command
            .SetupProperty(c => c.Transaction);

        _connection
            .Setup(c => c.BeginTransaction(IsolationLevel.ReadCommitted))
            .Returns(_transaction.Object);

        _adapter
            .SetupGet(a => a.DatabaseName)
            .Returns("database-name");
        _adapter
            .Setup(a => a.GetDatabaseExistsScript("database-name"))
            .Returns("database exits");
        _adapter
            .Setup(a => a.CreateConnection(true))
            .Returns(_connection.Object);
        _adapter
            .Setup(a => a.CreateConnection(false))
            .Returns(_connection.Object);

        _command
            .Setup(c => c.ExecuteScalar())
            .Callback(() => _command.Object.CommandText.ShouldBe("database exits"))
            .Returns("true");

        var script = new Mock<IScript>(MockBehavior.Strict);
        script
            .Setup(s => s.Execute(_command.Object, It.IsNotNull<IVariables>(), It.IsNotNull<ILogger>()))
            .Callback<IDbCommand, IVariables, ILogger>((cmd, variables, s) =>
            {
                cmd.CommandTimeout.ShouldBe(0);

                if (transaction == TransactionMode.PerStep)
                {
                    cmd.Transaction.ShouldBe(_transaction.Object);
                }
                else
                {
                    cmd.Transaction.ShouldBeNull();
                }

                variables.GetValue("DatabaseName").ShouldBe("database-name");
                variables.GetValue("CurrentVersion").ShouldBeNullOrEmpty();
                variables.GetValue("TargetVersion").ShouldBeNullOrEmpty();
                variables.GetValue("ModuleName").ShouldBeNullOrEmpty();
            });

        _sut.Execute(script.Object);

        script.VerifyAll();
        _command.VerifyAll();
    }

    [Test]
    public void ExecuteDatabaseNotFound()
    {
        _command
            .SetupProperty(c => c.Transaction);
        _command
            .SetupProperty(c => c.CommandTimeout, 30);

        _adapter
            .SetupGet(a => a.DatabaseName)
            .Returns("database-name");
        _adapter
            .Setup(a => a.GetDatabaseExistsScript("database-name"))
            .Returns("database exits");
        _adapter
            .Setup(a => a.CreateConnection(true))
            .Returns(_connection.Object);

        _command
            .Setup(c => c.ExecuteScalar())
            .Callback(() => _command.Object.CommandText.ShouldBe("database exits"))
            .Returns(DBNull.Value);

        var script = new Mock<IScript>(MockBehavior.Strict);
        script
            .Setup(s => s.Execute(_command.Object, It.IsNotNull<IVariables>(), It.IsNotNull<ILogger>()))
            .Callback<IDbCommand, IVariables, ILogger>((cmd, v, s) =>
            {
                cmd.CommandTimeout.ShouldBe(0);
                cmd.Transaction.ShouldBeNull();
            });

        _sut.Execute(script.Object);

        script.VerifyAll();
        _command.VerifyAll();
        _adapter.VerifyAll();
    }

    [Test]
    public void ExecuteWhatIf()
    {
        _adapter
            .SetupGet(a => a.DatabaseName)
            .Returns("database-name");

        var script = new Mock<IScript>(MockBehavior.Strict);
        script
            .Setup(s => s.Execute(null, It.IsNotNull<IVariables>(), It.IsNotNull<ILogger>()))
            .Callback<IDbCommand, IVariables, ILogger>((cmd, variables, s) =>
            {
                variables.GetValue("DatabaseName").ShouldBe("database-name");
                variables.GetValue("CurrentVersion").ShouldBeNullOrEmpty();
                variables.GetValue("TargetVersion").ShouldBeNullOrEmpty();
                variables.GetValue("ModuleName").ShouldBeNullOrEmpty();
            });

        _sut.WhatIf = true;

        _sut.Execute(script.Object);

        script.VerifyAll();

        _logOutput.Count.ShouldBe(1);
        _logOutput[0].ShouldBe("what-if mode");
    }

    [Test]
    public void ExecuteReader()
    {
        _command
            .SetupProperty(c => c.CommandTimeout, 30);

        _adapter
            .SetupGet(a => a.DatabaseName)
            .Returns("database-name");
        _adapter
            .Setup(a => a.CreateConnection(false))
            .Returns(_connection.Object);

        var reader1 = new Mock<IDataReader>(MockBehavior.Strict);
        var reader2 = new Mock<IDataReader>(MockBehavior.Strict);

        var script = new Mock<IScript>(MockBehavior.Strict);
        script
            .Setup(s => s.ExecuteReader(_command.Object, It.IsNotNull<IVariables>(), It.IsNotNull<ILogger>()))
            .Callback<IDbCommand, IVariables, ILogger>((cmd, variables, s) =>
            {
                cmd.CommandTimeout.ShouldBe(0);
                variables.GetValue("DatabaseName").ShouldBe("database-name");
            })
            .Returns(new[] { reader1.Object, reader2.Object });

        var actual = _sut.ExecuteReader(script.Object).ToArray();

        actual.ShouldBe(new[] { reader1.Object, reader2.Object });

        script.VerifyAll();
    }
}