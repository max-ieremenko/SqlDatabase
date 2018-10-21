using System.Configuration;
using NUnit.Framework;
using SqlDatabase.TestApi;

namespace SqlDatabase.Configuration
{
    [TestFixture]
    public class AppConfigurationTest
    {
        private TempDirectory _temp;

        [SetUp]
        public void BeforeEachTest()
        {
            _temp = new TempDirectory();
        }

        [TearDown]
        public void AfterEachTest()
        {
            _temp?.Dispose();
        }

        [Test]
        public void LoadEmpty()
        {
            var configuration = LoadFromResource("AppConfiguration.empty.xml");
            Assert.IsNull(configuration);
        }

        [Test]
        public void LoadDefault()
        {
            var configuration = LoadFromResource("AppConfiguration.default.xml");
            Assert.IsNotNull(configuration);

            Assert.That(configuration.GetCurrentVersionScript, Is.Not.Null.And.Not.Empty);
            Assert.That(configuration.SetCurrentVersionScript, Is.Not.Null.And.Not.Empty);
            Assert.That(configuration.AssemblyScript.ClassName, Is.Not.Null.And.Not.Empty);
            Assert.That(configuration.AssemblyScript.MethodName, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void LoadFull()
        {
            var configuration = LoadFromResource("AppConfiguration.full.xml");
            Assert.IsNotNull(configuration);

            Assert.AreEqual("get-version", configuration.GetCurrentVersionScript);
            Assert.AreEqual("set-version", configuration.SetCurrentVersionScript);
            Assert.AreEqual("method-name", configuration.AssemblyScript.MethodName);
            Assert.AreEqual("class-name", configuration.AssemblyScript.ClassName);
        }

        private AppConfiguration LoadFromResource(string resourceName)
        {
            var fileName = _temp.CopyFileFromResources(resourceName);
            var configuration = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap { ExeConfigFilename = fileName }, ConfigurationUserLevel.None);
            return (AppConfiguration)configuration.GetSection(AppConfiguration.SectionName);
        }
    }
}
