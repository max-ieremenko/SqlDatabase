using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using SqlDatabase.Adapter;
using SqlDatabase.FileSystem;
using SqlDatabase.TestApi;

namespace SqlDatabase.Scripts;

[TestFixture]
public class CreateScriptSequenceTest
{
    private CreateScriptSequence _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        var scriptFactory = new Mock<IScriptFactory>(MockBehavior.Strict);
        scriptFactory
            .Setup(f => f.IsSupported(It.IsAny<IFile>()))
            .Returns<IFile>(f => ".sql".Equals(f.Extension) || ".exe".Equals(f.Extension));

        scriptFactory
            .Setup(s => s.FromFile(It.IsNotNull<IFile>()))
            .Returns<IFile>(file =>
            {
                var script = new Mock<IScript>(MockBehavior.Strict);
                script.SetupProperty(s => s.DisplayName, file.Name);

                return script.Object;
            });

        _sut = new CreateScriptSequence(new List<IFileSystemInfo>(), scriptFactory.Object);
    }

    [Test]
    public void BuildSequenceFromOneFolder()
    {
        var folderX = new[]
        {
            FileFactory.File("a.sql"),
            FileFactory.File("x.sql"),
            FileFactory.File("ignore")
        };

        var folderA = new[]
        {
            FileFactory.File("x.exe"),
            FileFactory.File("a.exe"),
            FileFactory.File("ignore")
        };

        var files = new[]
        {
            FileFactory.File("x.sql"),
            FileFactory.File("a.exe"),
            FileFactory.File("ignore")
        };

        var content = new IFileSystemInfo[]
            {
                FileFactory.Folder("x", folderX),
                FileFactory.Folder("a", folderA)
            }
            .Concat(files)
            .ToArray();

        _sut.Sources.Add(FileFactory.Folder("root", content));

        var actual = _sut.BuildSequence();

        // sorted A-Z, first files then folders
        CollectionAssert.AreEqual(
            new[]
            {
                @"root\" + files[1].Name,
                @"root\" + files[0].Name,
                @"root\a\" + folderA[1].Name,
                @"root\a\" + folderA[0].Name,
                @"root\x\" + folderX[0].Name,
                @"root\x\" + folderX[1].Name
            },
            actual.Select(i => i.DisplayName).ToArray());
    }

    [Test]
    public void BuildSequenceFromFolderAndFile()
    {
        _sut.Sources.Add(FileFactory.Folder("root", FileFactory.File("20.sql"), FileFactory.File("10.sql")));
        _sut.Sources.Add(FileFactory.File("02.sql"));
        _sut.Sources.Add(FileFactory.File("01.sql"));
        _sut.Sources.Add(FileFactory.File("ignore"));

        var actual = _sut.BuildSequence();

        // sorted A-Z, first files then folders
        CollectionAssert.AreEqual(
            new[] { @"root\10.sql", @"root\20.sql", "02.sql", "01.sql" },
            actual.Select(i => i.DisplayName).ToArray());
    }
}