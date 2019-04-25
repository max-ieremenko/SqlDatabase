using System;
using System.Configuration;
using System.Data.SqlClient;
using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.IO;

namespace SqlDatabase.Configuration
{
    [TestFixture]
    public class CommandLineBaseTest
    {
        private Mock<ILogger> _log;
        private Mock<IConfigurationManager> _configurationManager;
        private AppConfiguration _configuration;
        private Mock<IFileSystemFactory> _fs;
        private CommandLineBase _sut;

        [SetUp]
        public void BeforeEachTest()
        {
            _log = new Mock<ILogger>(MockBehavior.Strict);

            _configuration = new AppConfiguration();

            _configurationManager = new Mock<IConfigurationManager>(MockBehavior.Strict);
            _configurationManager
                .SetupGet(c => c.SqlDatabase)
                .Returns(_configuration);

            _fs = new Mock<IFileSystemFactory>(MockBehavior.Strict);

            _sut = new Mock<CommandLineBase> { CallBase = true }.Object;
            _sut.Connection = new SqlConnectionStringBuilder();
            _sut.FileSystemFactory = _fs.Object;
        }

        [Test]
        public void CreateDatabase()
        {
            var actual = _sut.CreateDatabase(_log.Object, _configurationManager.Object, TransactionMode.PerStep);

            actual.Log.ShouldBe(_log.Object);
            actual.ConnectionString.ShouldNotBeNull();
            actual.Configuration.ShouldBe(_configuration);
            actual.Transaction.ShouldBe(TransactionMode.PerStep);
        }

        [Test]
        public void CreateDatabaseApplyVariables()
        {
            _sut.Variables.Add("a", "1");
            _sut.Variables.Add("b", "2");

            _configuration.Variables.Add(new NameValueConfigurationElement("b", "2.2"));
            _configuration.Variables.Add(new NameValueConfigurationElement("c", "3"));

            var actual = _sut.CreateDatabase(_log.Object, _configurationManager.Object, TransactionMode.None);

            Assert.AreEqual("1", actual.Variables.GetValue("a"));
            Assert.AreEqual("2", actual.Variables.GetValue("b"));
            Assert.AreEqual("3", actual.Variables.GetValue("c"));
        }

        [Test]
        public void CreateDatabaseValidateVariables()
        {
            _sut.Variables.Add("a b", "1");

            _configuration.Variables.Add(new NameValueConfigurationElement("c d", "1"));

            var ex = Assert.Throws<InvalidOperationException>(() => _sut.CreateDatabase(_log.Object, _configurationManager.Object, TransactionMode.None));

            ex.Message.ShouldContain("a b");
            ex.Message.ShouldContain("c d");
        }

        [Test]
        public void ParseFrom()
        {
            var file = new Mock<IFileSystemInfo>(MockBehavior.Strict);
            _fs
                .Setup(f => f.FileSystemInfoFromPath(@"c:\11.sql"))
                .Returns(file.Object);

            _sut.Parse(new CommandLine(new Arg("from", @"c:\11.sql")));

            _sut.Scripts.ShouldBe(new[] { file.Object });
        }
    }
}
