using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.FileSystem;
using SqlDatabase.TestApi;

namespace SqlDatabase.Configuration;

[TestFixture]
public class ConfigurationManagerTest
{
    public const string SomeConfiguration = @"
<configuration>
  <sqlDatabase getCurrentVersion='expected' />
</configuration>
";

    private Mock<IFileSystemFactory> _fileSystem = null!;
    private ConfigurationManager _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _fileSystem = new Mock<IFileSystemFactory>(MockBehavior.Strict);
        _sut = new ConfigurationManager(_fileSystem.Object);
    }

    [Test]
    public void LoadFromDefaultConfiguration()
    {
        var actual = _sut.LoadFrom(null);

        actual.Variables.Keys.ShouldBe(new[] { nameof(ConfigurationManagerTest) });
        actual.Variables[nameof(ConfigurationManagerTest)].ShouldBe("LoadFromCurrentConfiguration");
    }

    [Test]
    public void LoadFromEmptyFile()
    {
        const string FileName = "app.config";

        _fileSystem
            .Setup(f => f.FileSystemInfoFromPath(FileName))
            .Returns(FileFactory.File(FileName, "<configuration />"));

        var actual = _sut.LoadFrom(FileName);

        actual.ShouldNotBeNull();
    }

    [Test]
    public void LoadFromFile()
    {
        const string FileName = "app.config";

        _fileSystem
            .Setup(f => f.FileSystemInfoFromPath(FileName))
            .Returns(FileFactory.File(FileName, SomeConfiguration));

        var actual = _sut.LoadFrom(FileName);

        actual.GetCurrentVersionScript.ShouldBe("expected");
    }

    [Test]
    [TestCase(ConfigurationManager.Name1)]
    [TestCase(ConfigurationManager.Name2)]
    public void LoadFromDirectory(string fileName)
    {
        const string DirectoryName = "some/path";

        _fileSystem
            .Setup(f => f.FileSystemInfoFromPath(DirectoryName))
            .Returns(FileFactory.Folder(
                "path",
                FileFactory.File(fileName, SomeConfiguration)));

        var actual = _sut.LoadFrom(DirectoryName);

        actual.GetCurrentVersionScript.ShouldBe("expected");
    }

    [Test]
    public void NotFoundInDirectory()
    {
        const string DirectoryName = "some/path";

        _fileSystem
            .Setup(f => f.FileSystemInfoFromPath(DirectoryName))
            .Returns(FileFactory.Folder("path"));

        Assert.Throws<FileNotFoundException>(() => _sut.LoadFrom(DirectoryName));
    }

    [Test]
    public void LoadInvalidConfiguration()
    {
        const string FileName = "app.config";

        _fileSystem
            .Setup(f => f.FileSystemInfoFromPath(FileName))
            .Returns(FileFactory.File(FileName, "<configuration>"));

        Assert.Throws<ConfigurationErrorsException>(() => _sut.LoadFrom(FileName));
    }
}