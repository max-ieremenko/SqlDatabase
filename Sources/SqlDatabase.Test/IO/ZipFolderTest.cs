using System.IO;
using System.Linq;
using NUnit.Framework;
using SqlDatabase.TestApi;

namespace SqlDatabase.IO
{
    [TestFixture]
    public class ZipFolderTest
    {
        [Test]
        public void Ctor()
        {
            var folder = new ZipFolder(@"d:\11.zip");
            Assert.AreEqual("11.zip", folder.Name);
        }

        [Test]
        public void GetFolders()
        {
            using (var dir = new TempDirectory())
            {
                dir.CopyFileFromResources("Content.zip");
                var folder = new ZipFolder(Path.Combine(dir.Location, "Content.zip"));

                Assert.AreEqual(0, folder.GetFolders().Count());
            }
        }

        [Test]
        public void GetFiles()
        {
            using (var dir = new TempDirectory())
            {
                dir.CopyFileFromResources("Content.zip");
                var folder = new ZipFolder(Path.Combine(dir.Location, "Content.zip"));

                var files = folder.GetFiles().OrderBy(i => i.Name).ToArray();
                CollectionAssert.AreEqual(new[] { "11.txt", "22.txt" }, files.Select(i => i.Name).ToArray());

                using (var stream = files[0].OpenRead())
                using (var reader = new StreamReader(stream))
                {
                    Assert.AreEqual("11", reader.ReadToEnd());
                }

                using (var stream = files[1].OpenRead())
                using (var reader = new StreamReader(stream))
                {
                    Assert.AreEqual("22", reader.ReadToEnd());
                }
            }
        }
    }
}
