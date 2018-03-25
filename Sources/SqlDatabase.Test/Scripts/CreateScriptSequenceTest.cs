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

            _sut = new CreateScriptSequence
            {
                Root = _root.Object,
                ScriptFactory = scriptFactory.Object
            };
        }

        [Test]
        public void BuildSequence()
        {
            var folderX = new[] { FileFactory.File("a.sql"), FileFactory.File("x.sql"), FileFactory.File("ignore") };
            var folderA = new[] { FileFactory.File("x.exe"), FileFactory.File("a.exe"), FileFactory.File("ignore") };
            var folder = new[] { FileFactory.File("x.sql"), FileFactory.File("a.exe"), FileFactory.File("ignore") };

            _root.Setup(r => r.GetFolders()).Returns(new[]
            {
                FileFactory.Folder("x", folderX),
                FileFactory.Folder("a", folderA),
            });

            _root.Setup(r => r.GetFiles()).Returns(folder);

            var actual = _sut.BuildSequence();

            // sorted A-Z, first files then folders
            CollectionAssert.AreEqual(
                new[] { folder[1].Name, folder[0].Name, folderA[1].Name, folderA[0].Name, folderX[0].Name, folderX[1].Name },
                actual.Select(i => i.DisplayName).ToArray());
        }
    }
}
