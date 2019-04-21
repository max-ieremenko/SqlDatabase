﻿using System;
using System.Data.SqlClient;
using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Commands;
using SqlDatabase.IO;
using SqlDatabase.Scripts;

namespace SqlDatabase.Configuration
{
    [TestFixture]
    public class CreateCommandLineTest
    {
        private Mock<ILogger> _log;
        private Mock<IFileSystemFactory> _fs;
        private CreateCommandLine _sut;

        [SetUp]
        public void BeforeEachTest()
        {
            _log = new Mock<ILogger>(MockBehavior.Strict);
            _fs = new Mock<IFileSystemFactory>(MockBehavior.Strict);

            _sut = new CreateCommandLine { FileSystemFactory = _fs.Object };
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
                new Arg("configuration", "app.config")));

            _sut.Scripts.ShouldBe(new[] { folder.Object });

            _sut.Connection.ShouldNotBeNull();
            _sut.Connection.DataSource.ShouldBe(".");
            _sut.Connection.InitialCatalog.ShouldBe("test");

            _sut.Variables.Keys.ShouldBe(new[] { "X", "Y" });
            _sut.Variables["x"].ShouldBe("1 2 3");
            _sut.Variables["y"].ShouldBe("value");

            _sut.ConfigurationFile.ShouldBe("app.config");
        }

        [Test]
        public void CreateCommand()
        {
            _sut.Connection = new SqlConnectionStringBuilder();

            var actual = _sut
                .CreateCommand(_log.Object)
                .ShouldBeOfType<DatabaseCreateCommand>();

            actual.Log.ShouldBe(_log.Object);
            actual.Database.ShouldBeOfType<Database>();
            actual.ScriptSequence.ShouldBeOfType<CreateScriptSequence>();
        }

        [Test]
        public void DoesNotSupportTransaction()
        {
            _sut.Transaction = TransactionMode.PerStep;

            Assert.Throws<NotSupportedException>(_sut.Validate);
        }
    }
}
