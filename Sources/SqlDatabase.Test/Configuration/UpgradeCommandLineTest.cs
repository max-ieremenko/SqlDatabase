﻿using System.Data.SqlClient;
using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Commands;
using SqlDatabase.IO;
using SqlDatabase.Scripts;

namespace SqlDatabase.Configuration
{
    [TestFixture]
    public class UpgradeCommandLineTest
    {
        private Mock<ILogger> _log;
        private Mock<IFileSystemFactory> _fs;
        private UpgradeCommandLine _sut;

        [SetUp]
        public void BeforeEachTest()
        {
            _log = new Mock<ILogger>(MockBehavior.Strict);
            _fs = new Mock<IFileSystemFactory>(MockBehavior.Strict);

            _sut = new UpgradeCommandLine { FileSystemFactory = _fs.Object };
        }

        [Test]
        public void Parse()
        {
            var folder = new Mock<IFileSystemInfo>(MockBehavior.Strict);
            _fs
                .Setup(f => f.FileSystemInfoFromPath(@"c:\folder"))
                .Returns(folder.Object);

            _sut.Parse(new CommandLine(
                new Arg("database", "Data Source=.;Initial Catalog=test"),
                new Arg("from", @"c:\folder"),
                new Arg("varX", "1 2 3"),
                new Arg("varY", "value"),
                new Arg("configuration", "app.config"),
                new Arg("transaction", "perStep"),
                new Arg("whatIf")));

            _sut.Scripts.ShouldBe(new[] { folder.Object });

            _sut.Connection.ShouldNotBeNull();
            _sut.Connection.DataSource.ShouldBe(".");
            _sut.Connection.InitialCatalog.ShouldBe("test");

            _sut.Variables.Keys.ShouldBe(new[] { "X", "Y" });
            _sut.Variables["x"].ShouldBe("1 2 3");
            _sut.Variables["y"].ShouldBe("value");

            _sut.ConfigurationFile.ShouldBe("app.config");

            _sut.Transaction.ShouldBe(TransactionMode.PerStep);

            _sut.WhatIf.ShouldBeTrue();
        }

        [Test]
        public void CreateCommand()
        {
            _sut.WhatIf = true;
            _sut.Connection = new SqlConnectionStringBuilder();

            var actual = _sut
                .CreateCommand(_log.Object)
                .ShouldBeOfType<DatabaseUpgradeCommand>();

            actual.Log.ShouldBe(_log.Object);
            var database = actual.Database.ShouldBeOfType<Database>();
            database.WhatIf.ShouldBeTrue();

            var sequence = actual.ScriptSequence.ShouldBeOfType<UpgradeScriptSequence>();
            sequence.WhatIf.ShouldBeTrue();
        }
    }
}