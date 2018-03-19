using System.IO;
using System.Linq;
using NUnit.Framework;
using SqlDatabase.TestApi;

namespace SqlDatabase.IO
{
    [TestFixture]
    public class FileSystemFolderTest
    {
        [Test]
        public void Ctor()
        {
            var folder = new FileSystemFolder(@"d:\11\22");
            Assert.AreEqual("22", folder.Name);
        }

        [Test]
        public void GetFiles()
        {
            using (var dir = new TempDirectory())
            {
                var folder = new FileSystemFolder(dir.Location);
                Assert.AreEqual(0, folder.GetFiles().Count());

                dir.CopyFileFromResources("Content.zip");
                Directory.CreateDirectory(Path.Combine(dir.Location, "11.txt"));

                File.WriteAllText(Path.Combine(dir.Location, "22.txt"), string.Empty);
                File.WriteAllText(Path.Combine(dir.Location, "33.txt"), string.Empty);

                var files = folder.GetFiles().ToArray();
                CollectionAssert.AreEquivalent(new[] { "22.txt", "33.txt" }, files.Select(i => i.Name).ToArray());
            }
        }

        [Test]
        public void GetFolders()
        {
            using (var dir = new TempDirectory())
            {
                var folder = new FileSystemFolder(dir.Location);
                Assert.AreEqual(0, folder.GetFolders().Count());

                File.WriteAllText(Path.Combine(dir.Location, "11.txt"), string.Empty);

                dir.CopyFileFromResources("Content.zip");
                Directory.CreateDirectory(Path.Combine(dir.Location, "22.txt"));
                Directory.CreateDirectory(Path.Combine(dir.Location, "33"));

                var folders = folder.GetFolders().ToArray();
                CollectionAssert.AreEquivalent(new[] { "22.txt", "33", "Content.zip" }, folders.Select(i => i.Name).ToArray());
            }
        }
    }
}