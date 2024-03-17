using NUnit.Framework;
using Shouldly;
using SqlDatabase.TestApi;

namespace SqlDatabase.FileSystem;

[TestFixture]
public class FileSystemFactoryTest
{
    [Test]
    public void NewFileSystemFolder()
    {
        using (var dir = new TempDirectory("Content.zip"))
        {
            var folder = FileSystemFactory.FileSystemInfoFromPath(dir.Location);
            folder.ShouldBeOfType<FileSystemFolder>();
        }
    }

    [Test]
    [TestCase("Content.zip", null)]
    [TestCase(@"Content.zip\2", "22.txt")]
    [TestCase(@"Content.zip\2\2.2", "2.2.txt")]
    [TestCase(@"Content.zip\inner.zip", "11.txt")]
    [TestCase(@"Content.zip\inner.zip\2", "22.txt")]
    public void NewZipFolder(string path, string? fileName)
    {
        using (var dir = new TempDirectory())
        {
            dir.CopyFileFromResources("Content.zip");

            var folder = FileSystemFactory.FileSystemInfoFromPath(Path.Combine(dir.Location, path));
            folder.ShouldNotBeNull();

            var files = ((IFolder)folder).GetFiles().ToList();
            if (fileName != null)
            {
                files.Count.ShouldBe(1);
                files[0].Name.ShouldBe(fileName);
            }
        }
    }

    [Test]
    [TestCase("Content.nupkg", null)]
    [TestCase(@"Content.nupkg\2", "22.txt")]
    [TestCase(@"Content.nupkg\inner.zip", "11.txt")]
    public void NewNuGetFolder(string path, string? fileName)
    {
        using (var dir = new TempDirectory())
        {
            dir.CopyFileFromResources("Content.zip");
            File.Move(Path.Combine(dir.Location, "Content.zip"), Path.Combine(dir.Location, "Content.nupkg"));

            var folder = FileSystemFactory.FileSystemInfoFromPath(Path.Combine(dir.Location, path));
            folder.ShouldNotBeNull();

            var files = ((IFolder)folder).GetFiles().ToList();
            if (fileName != null)
            {
                files.Count.ShouldBe(1);
                files[0].Name.ShouldBe(fileName);
            }
        }
    }

    [Test]
    [TestCase(@"{0E4E24C7-E12A-483A-BC8F-E90BC49FD798}")]
    [TestCase(@"c:\{0E4E24C7-E12A-483A-BC8F-E90BC49FD798}")]
    [TestCase(@"c:\{0E4E24C7-E12A-483A-BC8F-E90BC49FD798}\11")]
    public void NotFound(string path)
    {
        Assert.Throws<IOException>(() => FileSystemFactory.FileSystemInfoFromPath(path));
    }

    [Test]
    [TestCase(@"Content.zip\xxx")]
    [TestCase(@"Content.zip\2\xxx")]
    [TestCase(@"Content.zip\2\33.txt")]
    [TestCase(@"Content.zip\inner.zip\xxx")]
    public void ZipNotFound(string path)
    {
        using (var dir = new TempDirectory())
        {
            dir.CopyFileFromResources("Content.zip");

            var fullPath = Path.Combine(dir.Location, path);
            Should.Throw<IOException>(() => FileSystemFactory.FileSystemInfoFromPath(fullPath));
        }
    }

    [Test]
    public void NewFileSystemFile()
    {
        using (var dir = new TempDirectory())
        {
            var fileName = Path.Combine(dir.Location, "11.txt");
            File.WriteAllBytes(fileName, [1]);

            var file = FileSystemFactory.FileSystemInfoFromPath(fileName);
            file.ShouldBeOfType<FileSystemFile>();
        }
    }

    [Test]
    [TestCase(@"Content.zip\11.txt")]
    [TestCase(@"Content.zip\2\22.txt")]
    [TestCase(@"Content.zip\inner.zip\11.txt")]
    public void NewZipFile(string fileName)
    {
        using (var dir = new TempDirectory())
        {
            dir.CopyFileFromResources("Content.zip");

            var file = FileSystemFactory.FileSystemInfoFromPath(Path.Combine(dir.Location, fileName));
            file.ShouldBeOfType<ZipFolderFile>();

            ((IFile)file).OpenRead().Dispose();
        }
    }
}