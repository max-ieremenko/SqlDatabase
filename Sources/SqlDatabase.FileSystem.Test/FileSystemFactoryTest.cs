using NUnit.Framework;
using Shouldly;
using SqlDatabase.TestApi;

namespace SqlDatabase.FileSystem;

[TestFixture]
public class FileSystemFactoryTest
{
    private TempDirectory _location = null!;
    private FileSystemFactory _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _location = new TempDirectory();
        _sut = new FileSystemFactory(_location.Location);
    }

    [TearDown]
    public void AfterEachTest()
    {
        _location.Dispose();
    }

    [Test]
    public void FileSystemFolderFromFullPath()
    {
        var actual = _sut.FileSystemInfoFromPath(_location.Location).ShouldBeOfType<FileSystemFolder>();
        actual.GetFullName().ShouldBe(_location.Location);
    }

    [Test]
    public void FileSystemFolderFromCurrentDirectory()
    {
        var actual = _sut.FileSystemInfoFromPath(null).ShouldBeOfType<FileSystemFolder>();
        actual.GetFullName().ShouldBe(_location.Location);
    }

    [Test]
    public void FileSystemFolderFromChildDirectory()
    {
        var path = Path.Combine(_location.Location, "child.zip");
        Directory.CreateDirectory(path);

        var actual = _sut.FileSystemInfoFromPath("child.zip").ShouldBeOfType<FileSystemFolder>();
        actual.GetFullName().ShouldBe(path);
    }

    [Test]
    [TestCase("Content.zip", null)]
    [TestCase(@"Content.zip\2", "22.txt")]
    [TestCase(@"Content.zip\2\2.2", "2.2.txt")]
    [TestCase(@"Content.zip\inner.zip", "11.txt")]
    [TestCase(@"Content.zip\inner.zip\2", "22.txt")]
    public void ZipFolderFromFullPath(string path, string? fileName)
    {
        _location.CopyFileFromResources("Content.zip");

        var folder = _sut
            .FileSystemInfoFromPath(Path.Combine(_location.Location, path))
            .ShouldBeAssignableTo<IFolder>()
            .ShouldNotBeNull();

        var files = folder.GetFiles().ToList();
        if (fileName != null)
        {
            files.Count.ShouldBe(1);
            files[0].Name.ShouldBe(fileName);
        }
    }

    [Test]
    [TestCase("Content.nupkg", null)]
    [TestCase(@"Content.nupkg\2", "22.txt")]
    [TestCase(@"Content.nupkg\inner.zip", "11.txt")]
    public void NuGetFolderFromChildPath(string path, string? fileName)
    {
        _location.CopyFileFromResources("Content.zip");
        File.Move(Path.Combine(_location.Location, "Content.zip"), Path.Combine(_location.Location, "Content.nupkg"));

        var folder = _sut
            .FileSystemInfoFromPath(path)
            .ShouldBeAssignableTo<IFolder>()
            .ShouldNotBeNull();

        var files = folder.GetFiles().ToList();
        if (fileName != null)
        {
            files.Count.ShouldBe(1);
            files[0].Name.ShouldBe(fileName);
        }
    }

    [Test]
    [TestCase(@"{0E4E24C7-E12A-483A-BC8F-E90BC49FD798}")]
    [TestCase(@"c:\{0E4E24C7-E12A-483A-BC8F-E90BC49FD798}")]
    [TestCase(@"c:\{0E4E24C7-E12A-483A-BC8F-E90BC49FD798}\11")]
    public void NotFound(string path)
    {
        Assert.Throws<IOException>(() => _sut.FileSystemInfoFromPath(path));
    }

    [Test]
    [TestCase(@"Content.zip\xxx")]
    [TestCase(@"Content.zip\2\xxx")]
    [TestCase(@"Content.zip\2\33.txt")]
    [TestCase(@"Content.zip\inner.zip\xxx")]
    public void ZipNotFound(string path)
    {
        _location.CopyFileFromResources("Content.zip");

        var fullPath = Path.Combine(_location.Location, path);
        Should.Throw<IOException>(() => _sut.FileSystemInfoFromPath(fullPath));

        Should.Throw<IOException>(() => _sut.FileSystemInfoFromPath(path));
    }

    [Test]
    public void FileFromFullPath()
    {
        var fileName = Path.Combine(_location.Location, "11.txt");
        File.WriteAllBytes(fileName, [1]);

        var actual = _sut.FileSystemInfoFromPath(fileName).ShouldBeOfType<FileSystemFile>();

        actual.GetFullName().ShouldBe(fileName);
    }

    [Test]
    public void FileFromChildPath()
    {
        var fileName = Path.Combine(_location.Location, "11.txt");
        File.WriteAllBytes(fileName, [1]);

        var actual = _sut.FileSystemInfoFromPath("11.txt").ShouldBeOfType<FileSystemFile>();

        actual.GetFullName().ShouldBe(fileName);
    }

    [Test]
    [TestCase("Content.zip/11.txt")]
    [TestCase("Content.zip/2/22.txt")]
    [TestCase("Content.zip/inner.zip/11.txt")]
    public void ZipFileFromFullPath(string fileName)
    {
        _location.CopyFileFromResources("Content.zip");

        var fullName = Path.Combine(_location.Location, fileName);

        var actual = _sut.FileSystemInfoFromPath(fullName).ShouldBeOfType<ZipFolderFile>();
        actual.ShouldBeOfType<ZipFolderFile>();

        AssertFullName(actual.GetFullName(), fullName);
        actual.OpenRead().Dispose();
    }

    [Test]
    [TestCase(@"Content.zip\11.txt")]
    [TestCase(@"Content.zip\2\22.txt")]
    [TestCase(@"Content.zip\inner.zip\11.txt")]
    public void ZipFileFromChildPath(string fileName)
    {
        _location.CopyFileFromResources("Content.zip");

        var fullName = Path.Combine(_location.Location, fileName);

        var actual = _sut.FileSystemInfoFromPath(fileName).ShouldBeOfType<ZipFolderFile>();
        actual.ShouldBeOfType<ZipFolderFile>();

        AssertFullName(actual.GetFullName(), fullName);
        actual.OpenRead().Dispose();
    }

    private static void AssertFullName(string actual, string expected)
    {
        actual = actual.Replace(@"\", "/");
        expected = expected.Replace(@"\", "/");
        expected.ShouldBe(actual);
    }
}