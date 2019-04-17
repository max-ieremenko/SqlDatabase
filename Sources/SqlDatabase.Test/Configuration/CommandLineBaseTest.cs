using System;
using System.Configuration;
using System.Data.SqlClient;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace SqlDatabase.Configuration
{
    [TestFixture]
    public class CommandLineBaseTest
    {
        private Mock<ILogger> _log;
        private Mock<IConfigurationManager> _configurationManager;
        private AppConfiguration _configuration;
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

            _sut = new Mock<CommandLineBase> { CallBase = true }.Object;
            _sut.Connection = new SqlConnectionStringBuilder();
        }

        [Test]
        public void CreateDatabase()
        {
            var actual = _sut.CreateDatabase(_log.Object, _configurationManager.Object);

            actual.Log.ShouldBe(_log.Object);
            actual.ConnectionString.ShouldNotBeNull();
            actual.Configuration.ShouldBe(_configuration);
        }

        [Test]
        public void CreateDatabaseApplyVariables()
        {
            _sut.Variables.Add("a", "1");
            _sut.Variables.Add("b", "2");

            _configuration.Variables.Add(new NameValueConfigurationElement("b", "2.2"));
            _configuration.Variables.Add(new NameValueConfigurationElement("c", "3"));

            var actual = _sut.CreateDatabase(_log.Object, _configurationManager.Object);

            Assert.AreEqual("1", actual.Variables.GetValue("a"));
            Assert.AreEqual("2", actual.Variables.GetValue("b"));
            Assert.AreEqual("3", actual.Variables.GetValue("c"));
        }

        [Test]
        public void CreateDatabaseValidateVariables()
        {
            _sut.Variables.Add("a b", "1");

            _configuration.Variables.Add(new NameValueConfigurationElement("c d", "1"));

            var ex = Assert.Throws<InvalidOperationException>(() => _sut.CreateDatabase(_log.Object, _configurationManager.Object));

            ex.Message.ShouldContain("a b");
            ex.Message.ShouldContain("c d");
        }
    }
}
