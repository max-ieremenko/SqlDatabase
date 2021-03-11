using System;
using System.IO;
using System.Text;
using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Configuration;
using SqlDatabase.Scripts.PowerShellInternal;
using SqlDatabase.TestApi;

namespace SqlDatabase.Scripts
{
    [TestFixture]
    public class ScriptFactoryTest
    {
        private ScriptFactory _sut;
        private Mock<IPowerShellFactory> _powerShellFactory;
        private AppConfiguration _configuration;

        [SetUp]
        public void BeforeEachTest()
        {
            _configuration = new AppConfiguration();
            _powerShellFactory = new Mock<IPowerShellFactory>(MockBehavior.Strict);

            _sut = new ScriptFactory
            {
                Configuration = _configuration,
                PowerShellFactory = _powerShellFactory.Object
            };
        }

        [Test]
        public void FromSqlFile()
        {
            var file = FileFactory.File("11.sql", "some script");

            _sut.IsSupported(file.Name).ShouldBeTrue();

            var script = _sut.FromFile(file).ShouldBeOfType<TextScript>();

            script.DisplayName.ShouldBe("11.sql");
            new StreamReader(script.ReadSqlContent()).ReadToEnd().ShouldBe("some script");
        }

        [Test]
        public void FromDllFile()
        {
            var file = FileFactory.File(
                "11.dll",
                new byte[] { 1, 2, 3 },
                FileFactory.Folder("name", FileFactory.File("11.txt", "3, 2, 1")));

            _sut.IsSupported(file.Name).ShouldBeTrue();

            var script = _sut.FromFile(file).ShouldBeOfType<AssemblyScript>();

            script.DisplayName.ShouldBe("11.dll");
            script.ReadAssemblyContent().ShouldBe(new byte[] { 1, 2, 3 });
            new StreamReader(script.ReadDescriptionContent()).ReadToEnd().ShouldBe("3, 2, 1");
        }

        [Test]
        public void FromExeFile()
        {
            var file = FileFactory.File(
                "11.exe",
                new byte[] { 1, 2, 3 },
                FileFactory.Folder("name"));

            _sut.IsSupported(file.Name).ShouldBeTrue();

            var script = _sut.FromFile(file).ShouldBeOfType<AssemblyScript>();

            script.DisplayName.ShouldBe("11.exe");
            script.ReadAssemblyContent().ShouldBe(new byte[] { 1, 2, 3 });
            script.ReadDescriptionContent().ShouldBeNull();
        }

        [Test]
        public void FromPs1File()
        {
            var file = FileFactory.File(
                "11.ps1",
                "some script",
                FileFactory.Folder("name", FileFactory.File("11.txt", "3, 2, 1")));

            _powerShellFactory
                .Setup(f => f.Request());

            _sut.IsSupported(file.Name).ShouldBeTrue();

            var script = _sut.FromFile(file).ShouldBeOfType<PowerShellScript>();

            script.PowerShellFactory.ShouldBe(_powerShellFactory.Object);
            script.DisplayName.ShouldBe("11.ps1");
            new StreamReader(script.ReadScriptContent()).ReadToEnd().ShouldBe("some script");
            new StreamReader(script.ReadDescriptionContent()).ReadToEnd().ShouldBe("3, 2, 1");
            _powerShellFactory.VerifyAll();
        }

        [Test]
        public void FromPs1FileNotSupported()
        {
            var file = FileFactory.File("11.ps1", "some script");
            _sut.PowerShellFactory = null;

            _sut.IsSupported(file.Name).ShouldBeFalse();

            Assert.Throws<NotSupportedException>(() => _sut.FromFile(file));
        }

        [Test]
        public void FromFileNotSupported()
        {
            var file = FileFactory.File("11.txt");

            _sut.IsSupported(file.Name).ShouldBeFalse();
            Assert.Throws<NotSupportedException>(() => _sut.FromFile(file));
        }
    }
}
