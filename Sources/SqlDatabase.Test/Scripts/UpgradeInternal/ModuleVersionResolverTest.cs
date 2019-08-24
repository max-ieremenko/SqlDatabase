using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace SqlDatabase.Scripts.UpgradeInternal
{
    [TestFixture]
    public class ModuleVersionResolverTest
    {
        private ModuleVersionResolver _sut;
        private Mock<IDatabase> _database;
        private IList<string> _logOutput;

        [SetUp]
        public void BeforeEachTest()
        {
            _database = new Mock<IDatabase>(MockBehavior.Strict);

            _logOutput = new List<string>();
            var log = new Mock<ILogger>(MockBehavior.Strict);
            log
                .Setup(l => l.Info(It.IsAny<string>()))
                .Callback<string>(m =>
                {
                    Console.WriteLine("Info: {0}", m);
                    _logOutput.Add(m);
                });

            _sut = new ModuleVersionResolver
            {
                Database = _database.Object,
                Log = log.Object
            };
        }

        [Test]
        public void GetCurrentVersionModuleName()
        {
            const string ModuleName = "the-module";
            var moduleVersion = new Version("1.0");

            _database
                .Setup(d => d.GetCurrentVersion(ModuleName))
                .Returns(moduleVersion);

            _sut.GetCurrentVersion(ModuleName).ShouldBe(moduleVersion);

            _database.VerifyAll();
            _logOutput.Count.ShouldBe(2);
            _logOutput[1].ShouldContain(ModuleName);
            _logOutput[1].ShouldContain(moduleVersion.ToString());
        }

        [Test]
        public void GetCurrentVersionNoModule()
        {
            var version = new Version("1.0");

            _database
                .Setup(d => d.GetCurrentVersion(string.Empty))
                .Returns(version);

            _sut.GetCurrentVersion(null).ShouldBe(version);

            _database.VerifyAll();
            _logOutput.Count.ShouldBe(2);
            _logOutput[1].ShouldContain("database version");
            _logOutput[1].ShouldContain(version.ToString());
        }

        [Test]
        public void GetCurrentVersionCache()
        {
            var version = new Version("1.0");

            _database
                .Setup(d => d.GetCurrentVersion(string.Empty))
                .Returns(new Version("1.0"));

            _sut.GetCurrentVersion(null).ShouldBe(version);

            _logOutput.Clear();
            _database
                .Setup(d => d.GetCurrentVersion(string.Empty))
                .Throws<NotSupportedException>();

            _sut.GetCurrentVersion(null).ShouldBe(version);
            _logOutput.ShouldBeEmpty();
        }
    }
}
