using System.IO;
using System.Linq;
using NUnit.Framework;
using SqlDatabase.TestApi;

namespace SqlDatabase.IO
{
    [TestFixture]
    public class FileSytemFactoryTest
    {
        [Test]
        public void NewFileSystemFolder()
        {
            using (var dir = new TempDirectory("Content.zip"))
            {
                var folder = FileSytemFactory.FolderFromPath(dir.Location);
                Assert.IsInstanceOf<FileSystemFolder>(folder);
            }
        }

        [Test]
        [TestCase("Content.zip", null)]
        [TestCase(@"Content.zip\2", "22.txt")]
        [TestCase(@"Content.zip\inner.zip", "inner.txt")]
        public void NewZipFolder(string path, string fileName)
        {
            using (var dir = new TempDirectory())
            {
                dir.CopyFileFromResources("Content.zip");

                var folder = FileSytemFactory.FolderFromPath(Path.Combine(dir.Location, path));
                Assert.IsInstanceOf<ZipFolder>(folder);

                var files = folder.GetFiles().ToList();
                if (fileName != null)
                {
                    Assert.AreEqual(1, files.Count);
                    Assert.AreEqual(fileName, files[0].Name);
                }
            }
        }

        [Test]
        [TestCase(@"{0E4E24C7-E12A-483A-BC8F-E90BC49FD798}")]
        [TestCase(@"c:\{0E4E24C7-E12A-483A-BC8F-E90BC49FD798}")]
        [TestCase(@"c:\{0E4E24C7-E12A-483A-BC8F-E90BC49FD798}\11")]
        public void DirectoryNotFound(string path)
        {
            Assert.Throws<DirectoryNotFoundException>(() => FileSytemFactory.FolderFromPath(path));
        }

        [Test]
        [TestCase(@"Content.zip\xxx")]
        [TestCase(@"Content.zip\2\22.txt")]
        [TestCase(@"Content.zip\inner.zip\xxx")]
        public void ZipDirectoryNotFound(string path)
        {
            using (var dir = new TempDirectory())
            {
                dir.CopyFileFromResources("Content.zip");

                var fullPath = Path.Combine(dir.Location, path);
                Assert.Throws<DirectoryNotFoundException>(() => FileSytemFactory.FolderFromPath(fullPath));
            }
        }

        [Test]
        public void NewFileSystemFile()
        {
            using (var dir = new TempDirectory())
            {
                var fileName = Path.Combine(dir.Location, "11.txt");
                File.WriteAllBytes(fileName, new byte[] { 1 });

                var file = FileSytemFactory.FileFromPath(fileName);
                Assert.IsInstanceOf<FileSystemFile>(file);
            }
        }

        [Test]
        [TestCase(@"Content.zip\11.txt")]
        [TestCase(@"Content.zip\2\22.txt")]
        [TestCase(@"Content.zip\inner.zip\inner.txt")]
        public void NewZipFile(string fileName)
        {
            using (var dir = new TempDirectory())
            {
                dir.CopyFileFromResources("Content.zip");

                var file = FileSytemFactory.FileFromPath(Path.Combine(dir.Location, fileName));
                Assert.IsInstanceOf<ZipFolderFile>(file);

                file.OpenRead().Dispose();
            }
        }

        [Test]
        [TestCase(@"{0E4E24C7-E12A-483A-BC8F-E90BC49FD798}")]
        [TestCase(@"c:\{0E4E24C7-E12A-483A-BC8F-E90BC49FD798}")]
        [TestCase(@"c:\{0E4E24C7-E12A-483A-BC8F-E90BC49FD798}\11")]
        public void FileNotFound(string path)
        {
            var ex = Assert.Throws<FileNotFoundException>(() => FileSytemFactory.FileFromPath(path));
            StringAssert.Contains(path, ex.Message);
        }

        [Test]
        [TestCase(@"Content.zip\xxx")]
        [TestCase(@"Content.zip\2")]
        [TestCase(@"Content.zip\2\33.txt")]
        [TestCase(@"Content.zip\inner.zip")]
        [TestCase(@"Content.zip\inner.zip\xxx")]
        public void ZipFileNotFound(string path)
        {
            using (var dir = new TempDirectory())
            {
                dir.CopyFileFromResources("Content.zip");

                var fullPath = Path.Combine(dir.Location, path);
                Assert.Throws<FileNotFoundException>(() => FileSytemFactory.FileFromPath(fullPath));
            }
        }
    }
}