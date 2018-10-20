using System.IO;
using System.Linq;
using Moq;
using NUnit.Framework;
using SqlDatabase.IO;
using SqlDatabase.TestApi;

namespace SqlDatabase.Scripts
{
    [TestFixture]
    public class CreateScriptSequenceTest
    {
        private CreateScriptSequence _sut;

        [SetUp]
        public void BeforeEachTest()
        {
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

            _sut = new CreateScriptSequence
            {
                ScriptFactory = scriptFactory.Object
            };
        }

        [Test]
        public void BuildSequenceFromOneFolder()
        {
            var folderX = new[] { FileFactory.File("a.sql"), FileFactory.File("x.sql"), FileFactory.File("ignore") };
            var folderA = new[] { FileFactory.File("x.exe"), FileFactory.File("a.exe"), FileFactory.File("ignore") };
            var folder = new[] { FileFactory.File("x.sql"), FileFactory.File("a.exe"), FileFactory.File("ignore") };

            var sourceFolder = new Mock<IFolder>();
            sourceFolder.Setup(r => r.GetFolders()).Returns(new[]
            {
                FileFactory.Folder("x", folderX),
                FileFactory.Folder("a", folderA),
            });

            sourceFolder.Setup(r => r.GetFiles()).Returns(folder);

            _sut.Sources.Add(sourceFolder.Object);
            var actual = _sut.BuildSequence();

            // sorted A-Z, first files then folders
            CollectionAssert.AreEqual(
                new[] { folder[1].Name, folder[0].Name, folderA[1].Name, folderA[0].Name, folderX[0].Name, folderX[1].Name },
                actual.Select(i => i.DisplayName).ToArray());
        }

        [Test]
        public void BuildSequenceFromFolderAndFile()
        {
            var sourceFolder = new Mock<IFolder>();
            sourceFolder.Setup(r => r.GetFolders()).Returns(new IFolder[0]);
            sourceFolder.Setup(r => r.GetFiles()).Returns(new[]
            {
                FileFactory.File("20.sql"),
                FileFactory.File("10.sql")
            });

            _sut.Sources.Add(sourceFolder.Object);
            _sut.Sources.Add(FileFactory.File("02.sql"));
            _sut.Sources.Add(FileFactory.File("01.sql"));
            var actual = _sut.BuildSequence();

            // sorted A-Z, first files then folders
            CollectionAssert.AreEqual(
                new[] { "10.sql", "20.sql", "02.sql", "01.sql" },
                actual.Select(i => i.DisplayName).ToArray());
        }
    }
}
