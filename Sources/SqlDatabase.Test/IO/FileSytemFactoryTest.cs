using System.IO;
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
        public void NewZipFolder()
        {
            using (var dir = new TempDirectory())
            {
                dir.CopyFileFromResources("Content.zip");

                var folder = FileSytemFactory.FolderFromPath(Path.Combine(dir.Location, "Content.zip"));
                Assert.IsInstanceOf<ZipFolder>(folder);
            }
        }
    }
}