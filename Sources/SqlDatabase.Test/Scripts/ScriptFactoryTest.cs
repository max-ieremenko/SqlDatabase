using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Configuration;
using SqlDatabase.TestApi;

namespace SqlDatabase.Scripts
{
    [TestFixture]
    public class ScriptFactoryTest
    {
        private ScriptFactory _sut;
        private AppConfiguration _configuration;

        [SetUp]
        public void BeforeEachTest()
        {
            _configuration = new AppConfiguration();
            _sut = new ScriptFactory { Configuration = _configuration };
        }

        [Test]
        public void FromSqlFile()
        {
            var file = FileFactory.File("11.sql", Encoding.UTF8.GetBytes("some script"));

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
                FileFactory.Folder("name", FileFactory.File("11.txt", new byte[] { 3, 2, 1 })));

            _sut.IsSupported(file.Name).ShouldBeTrue();

            var script = _sut.FromFile(file).ShouldBeOfType<AssemblyScript>();

            script.DisplayName.ShouldBe("11.dll");
            script.ReadAssemblyContent().ShouldBe(new byte[] { 1, 2, 3 });
            script.ReadDescriptionContent().ShouldBe(new byte[] { 3, 2, 1 });
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
        public void FromFileNotSupported()
        {
            var file = FileFactory.File("11.txt");

            _sut.IsSupported(file.Name).ShouldBeFalse();
            Assert.Throws<NotSupportedException>(() => _sut.FromFile(file));
        }
    }
}
