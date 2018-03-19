using System;
using System.Text;
using NUnit.Framework;
using SqlDatabase.TestApi;

namespace SqlDatabase.Scripts
{
    [TestFixture]
    public class ScriptFactoryTest
    {
        private ScriptFactory _sut;

        [SetUp]
        public void BeforeEachTest()
        {
            _sut = new ScriptFactory();
        }

        [Test]
        public void FromSqlFile()
        {
            var file = FileFactory.File("11.sql", Encoding.UTF8.GetBytes("some script"));

            Assert.IsTrue(_sut.IsSupported(file.Name));
            var script = _sut.FromFile(file);

            Assert.IsInstanceOf<TextScript>(script);

            var text = (TextScript)script;
            Assert.AreEqual("11.sql", script.DisplayName);
            Assert.AreEqual("some script", text.Sql);
        }

        [Test]
        public void FromDllFile()
        {
            var file = FileFactory.File("11.dll", new byte[] { 1, 2, 3 });

            Assert.IsTrue(_sut.IsSupported(file.Name));
            var script = _sut.FromFile(file);

            Assert.IsInstanceOf<AssemblyScript>(script);

            var text = (AssemblyScript)script;
            Assert.AreEqual("11.dll", script.DisplayName);
            Assert.AreEqual(new byte[] { 1, 2, 3 }, text.Assembly);
        }

        [Test]
        public void FromExeFile()
        {
            var file = FileFactory.File("11.exe", new byte[] { 1, 2, 3 });

            Assert.IsTrue(_sut.IsSupported(file.Name));
            var script = _sut.FromFile(file);

            Assert.IsInstanceOf<AssemblyScript>(script);

            var text = (AssemblyScript)script;
            Assert.AreEqual("11.exe", script.DisplayName);
            Assert.AreEqual(new byte[] { 1, 2, 3 }, text.Assembly);
        }

        [Test]
        public void FromFileNotSupported()
        {
            var file = FileFactory.File("11.txt");

            Assert.IsFalse(_sut.IsSupported(file.Name));
            Assert.Throws<NotSupportedException>(() => _sut.FromFile(file));
        }
    }
}
