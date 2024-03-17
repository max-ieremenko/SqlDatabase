using NUnit.Framework;
using Shouldly;
using SqlDatabase.TestApi;

namespace SqlDatabase.FileSystem;

[TestFixture]
public class FileSystemFolderTest
{
    [Test]
    public void Ctor()
    {
        var folder = new FileSystemFolder(@"d:\11\22");
        folder.Name.ShouldBe("22");
    }

    [Test]
    public void GetFiles()
    {
        using (var dir = new TempDirectory())
        {
            var folder = new FileSystemFolder(dir.Location);
            folder.GetFiles().ShouldBeEmpty();

            dir.CopyFileFromResources("Content.zip");
            Directory.CreateDirectory(Path.Combine(dir.Location, "11.txt"));

            File.WriteAllText(Path.Combine(dir.Location, "22.txt"), string.Empty);
            File.WriteAllText(Path.Combine(dir.Location, "33.txt"), string.Empty);

            folder.GetFiles().Select(i => i.Name).ShouldBe(["22.txt", "33.txt"], ignoreOrder: true);
        }
    }

    [Test]
    public void GetFolders()
    {
        using (var dir = new TempDirectory())
        {
            var folder = new FileSystemFolder(dir.Location);
            folder.GetFolders().ShouldBeEmpty();

            File.WriteAllText(Path.Combine(dir.Location, "11.txt"), string.Empty);

            dir.CopyFileFromResources("Content.zip");
            Directory.CreateDirectory(Path.Combine(dir.Location, "22.txt"));
            Directory.CreateDirectory(Path.Combine(dir.Location, "33"));

            folder.GetFolders().Select(i => i.Name).ShouldBe(["22.txt", "33", "Content.zip"], ignoreOrder: true);
        }
    }
}