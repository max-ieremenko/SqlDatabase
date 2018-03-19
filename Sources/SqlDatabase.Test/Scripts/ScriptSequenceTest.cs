using System;
using System.IO;
using System.Linq;
using Moq;
using NUnit.Framework;
using SqlDatabase.IO;
using SqlDatabase.TestApi;

namespace SqlDatabase.Scripts
{
    [TestFixture]
    public class ScriptSequenceTest
    {
        private ScriptSequence _sut;
        private Mock<IFolder> _root;

        [SetUp]
        public void BeforeEachTest()
        {
            _root = new Mock<IFolder>(MockBehavior.Strict);

            var scriptFactory = new Mock<IScriptFactory>(MockBehavior.Strict);
            scriptFactory
                .Setup(f => f.IsSupported(It.IsAny<string>()))
                .Returns<string>(s => ".sql".Equals(Path.GetExtension(s)) || ".exe".Equals(Path.GetExtension(s)));

            scriptFactory
                .Setup(s => s.FromFile(It.IsNotNull<IFile>()))
                .Returns<IFile>(file =>
                {
                    var script = new Mock<IScript>(MockBehavior.Strict);
                    script.SetupGet(s => s.DisplayName).Returns(file.Name);
                    return script.Object;
                });

            _sut = new ScriptSequence
            {
                Root = _root.Object,
                ScriptFactory = scriptFactory.Object
            };
        }

        [Test]
        public void EmptySequence()
        {
            _root.Setup(r => r.GetFolders()).Returns(new IFolder[0]);
            _root.Setup(r => r.GetFiles()).Returns(new IFile[0]);

            var actual = _sut.BuildSequence(new Version("1.0"));
            Assert.AreEqual(0, actual.Count);

            _root.VerifyAll();
        }

        [Test]
        public void IsUptodate()
        {
            _root.Setup(r => r.GetFolders()).Returns(new IFolder[0]);
            _root.Setup(r => r.GetFiles()).Returns(new[] { FileFactory.File("1.0_2.0.sql") });

            var actual = _sut.BuildSequence(new Version("2.0"));
            Assert.AreEqual(0, actual.Count);

            _root.VerifyAll();
        }

        [Test]
        public void UpdateNotFound()
        {
            _root.Setup(r => r.GetFolders()).Returns(new IFolder[0]);
            _root.Setup(r => r.GetFiles()).Returns(new[] { FileFactory.File("1.0_2.0.sql") });

            var ex = Assert.Throws<InvalidOperationException>(() => _sut.BuildSequence(new Version("3.0")));
            StringAssert.Contains("3.0", ex.Message);

            _root.VerifyAll();
        }

        [Test]
        public void LongFileSequence()
        {
            _root.Setup(r => r.GetFolders()).Returns(new IFolder[0]);
            _root.Setup(r => r.GetFiles()).Returns(new[]
            {
                FileFactory.File("1.0_2.0.sql"),
                FileFactory.File("2.0_5.0.sql"),
                FileFactory.File("5.0_5.1.sql"),
                FileFactory.File("5.1_6.0.sql"),

                FileFactory.File("3.0_6.0.sql"),
            });

            var actual = _sut.BuildSequence(new Version("2.0"));
            CollectionAssert.AreEqual(
                new[] { "2.0_5.0.sql", "5.0_5.1.sql", "5.1_6.0.sql" },
                actual.Select(i => i.Script.DisplayName).ToArray());

            _root.VerifyAll();
        }

        [Test]
        public void ShortFileSequence()
        {
            _root.Setup(r => r.GetFolders()).Returns(new IFolder[0]);
            _root.Setup(r => r.GetFiles()).Returns(new[]
            {
                FileFactory.File("1.0_2.0.sql"),
                FileFactory.File("2.0_5.0.sql"),
                FileFactory.File("5.0_5.1.sql"),
                FileFactory.File("5.1_6.0.sql"),

                FileFactory.File("2.0_6.0.sql"),
            });

            var actual = _sut.BuildSequence(new Version("2.0"));
            CollectionAssert.AreEqual(
                new[] { "2.0_6.0.sql" },
                actual.Select(i => i.Script.DisplayName).ToArray());

            _root.VerifyAll();
        }

        [Test]
        public void MixedSequence()
        {
            _root.Setup(r => r.GetFolders()).Returns(new[]
            {
                FileFactory.Folder(
                    "1.0_5.0.zip",
                    FileFactory.File("1.0_2.0.sql"),
                    FileFactory.File("2.0_2.1.sql"),
                    FileFactory.File("2.1_5.0.sql"))
            });
            _root.Setup(r => r.GetFiles()).Returns(new[]
            {
                FileFactory.File("5.0_5.1.sql")
            });

            var actual = _sut.BuildSequence(new Version("1.0"));

            CollectionAssert.AreEqual(
                new[] { "1.0_2.0.sql", "2.0_2.1.sql", "2.1_5.0.sql", "5.0_5.1.sql" },
                actual.Select(i => i.Script.DisplayName).ToArray());

            _root.VerifyAll();
        }

        [Test]
        public void SelectMaxStep()
        {
            _root.Setup(r => r.GetFolders()).Returns(new IFolder[0]);
            _root.Setup(r => r.GetFiles()).Returns(new[]
            {
                FileFactory.File("1.0_2.0.sql"),
                FileFactory.File("1.0_3.0.sql")
            });

            var actual = _sut.BuildSequence(new Version("1.0"));
            CollectionAssert.AreEqual(
                new[] { "1.0_3.0.sql" },
                actual.Select(i => i.Script.DisplayName).ToArray());

            _root.VerifyAll();
        }

        [Test]
        public void IgnoreFiles()
        {
            _root.Setup(r => r.GetFolders()).Returns(new IFolder[0]);
            _root.Setup(r => r.GetFiles()).Returns(new[]
            {
                FileFactory.File("1.0_2.0.txt"),
                FileFactory.File("1.0_2.0.sql")
            });

            var actual = _sut.BuildSequence(new Version("1.0"));
            CollectionAssert.AreEqual(
                new[] { "1.0_2.0.sql" },
                actual.Select(i => i.Script.DisplayName).ToArray());

            _root.VerifyAll();
        }

        [Test]
        public void DuplicatedStep()
        {
            _root.Setup(r => r.GetFolders()).Returns(new IFolder[0]);
            _root.Setup(r => r.GetFiles()).Returns(new[]
            {
                FileFactory.File("1.0_2.0.exe"),
                FileFactory.File("1.0_2.0.sql")
            });

            var ex = Assert.Throws<InvalidOperationException>(() => _sut.BuildSequence(new Version("1.0")));
            StringAssert.Contains("1.0_2.0.exe", ex.Message);
            StringAssert.Contains("1.0_2.0.sql", ex.Message);

            _root.VerifyAll();
        }

        [Test]
        [TestCase("1.0_1.1.sql", "1.0", "1.1")]
        [TestCase("1.0.1.2_1.2.1.sql", "1.0.1.2", "1.2.1")]
        [TestCase("1_2.sql", "1.0", "2.0")]
        [TestCase("1.0.sql", null, null)]
        [TestCase("xxx.sql", null, null)]
        [TestCase("xxx_1.0.sql", null, null)]
        [TestCase("1.0_xxx.sql", null, null)]
        [TestCase("2.0_1.0.sql", null, null)]
        public void ParseName(string name, string from, string to)
        {
            var version = ScriptSequence.ParseName(name);

            if (from == null)
            {
                Assert.IsNull(version);
            }
            else
            {
                Assert.IsNotNull(version);
                Assert.AreEqual(new Version(from), version.From);
                Assert.AreEqual(new Version(to), version.To);
            }
        }
    }
}