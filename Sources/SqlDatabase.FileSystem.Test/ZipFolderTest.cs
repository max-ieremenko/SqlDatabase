using NUnit.Framework;
using Shouldly;
using SqlDatabase.TestApi;

namespace SqlDatabase.FileSystem;

[TestFixture]
public class ZipFolderTest
{
    private TempDirectory _temp = null!;
    private ZipFolder _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _temp = new TempDirectory();
        _temp.CopyFileFromResources("Content.zip");
        _sut = new ZipFolder(Path.Combine(_temp.Location, "Content.zip"));
    }

    [TearDown]
    public void AfterEachTest()
    {
        _temp.Dispose();
    }

    [Test]
    public void Ctor()
    {
        var folder = new ZipFolder(@"d:\11.zip");
        folder.Name.ShouldBe("11.zip");
    }

    [Test]
    public void GetFolders()
    {
        var subFolders = _sut.GetFolders().OrderBy(i => i.Name).ToArray();

        subFolders.Select(i => i.Name).ShouldBe(["1", "2", "inner.zip"], ignoreOrder: true);
    }

    [Test]
    public void GetFiles()
    {
        var files = _sut.GetFiles().OrderBy(i => i.Name).ToArray();
        files.Select(i => i.Name).ShouldBe(["11.txt"]);

        using (var stream = files[0].OpenRead())
        using (var reader = new StreamReader(stream))
        {
            reader.ReadToEnd().ShouldBe("11");
        }

        files[0].GetParent().ShouldNotBeNull().GetFiles().OrderBy(i => i.Name).First().ShouldBe(files[0]);
    }

    [Test]
    public void GetContentOfSubFolders()
    {
        var subFolders = _sut.GetFolders().OrderBy(i => i.Name).ToArray();

        // 1
        subFolders[0].GetFolders().ShouldBeEmpty();
        subFolders[0].GetFiles().ShouldBeEmpty();

        // 2
        subFolders[1].GetFolders().Count().ShouldBe(1);

        var files = subFolders[1].GetFiles().ToArray();
        files.Select(i => i.Name).ShouldBe(["22.txt"]);

        using (var stream = files[0].OpenRead())
        using (var reader = new StreamReader(stream))
        {
            reader.ReadToEnd().ShouldBe("22");
        }

        files[0].GetParent().ShouldBe(subFolders[1]);
    }

    [Test]
    public void ReadContentOfEmbeddedZip()
    {
        var innerZip = _sut.GetFolders().OrderBy(i => i.Name).Last();

        innerZip.GetFolders().Count().ShouldBe(2);

        var files = innerZip.GetFiles().ToArray();
        files.Select(i => i.Name).ShouldBe(["11.txt"]);

        using (var stream = files[0].OpenRead())
        using (var reader = new StreamReader(stream))
        {
            reader.ReadToEnd().ShouldBe("11");
        }

        files[0].GetParent().ShouldNotBeNull().GetFiles().OrderBy(i => i.Name).First().ShouldBe(files[0]);
    }
}