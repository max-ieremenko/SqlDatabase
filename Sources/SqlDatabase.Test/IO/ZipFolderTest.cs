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

                var subFolders = folder.GetFolders().OrderBy(i => i.Name).ToArray();

                CollectionAssert.AreEqual(
                    new[] { "1", "2", "inner.zip" },
                    subFolders.Select(i => i.Name).ToArray());
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
                CollectionAssert.AreEqual(new[] { "11.txt" }, files.Select(i => i.Name).ToArray());

                using (var stream = files[0].OpenRead())
                using (var reader = new StreamReader(stream))
                {
                    Assert.AreEqual("11", reader.ReadToEnd());
                }
            }
        }

        [Test]
        public void GetContentOfSubFolders()
        {
            using (var dir = new TempDirectory())
            {
                dir.CopyFileFromResources("Content.zip");
                var folder = new ZipFolder(Path.Combine(dir.Location, "Content.zip"));

                var subFolders = folder.GetFolders().OrderBy(i => i.Name).ToArray();

                // 1
                Assert.AreEqual(0, subFolders[0].GetFolders().Count());
                Assert.AreEqual(0, subFolders[0].GetFiles().Count());

                // 2
                Assert.AreEqual(0, subFolders[1].GetFolders().Count());

                var files = subFolders[1].GetFiles().ToArray();
                CollectionAssert.AreEqual(new[] { "22.txt" }, files.Select(i => i.Name).ToArray());

                using (var stream = files[0].OpenRead())
                using (var reader = new StreamReader(stream))
                {
                    Assert.AreEqual("22", reader.ReadToEnd());
                }
            }
        }

        [Test]
        public void ReadContentOfEmbeddedZip()
        {
            using (var dir = new TempDirectory())
            {
                dir.CopyFileFromResources("Content.zip");
                var folder = new ZipFolder(Path.Combine(dir.Location, "Content.zip"));

                var innerZip = folder.GetFolders().OrderBy(i => i.Name).Last();

                Assert.AreEqual(0, innerZip.GetFolders().Count());

                var files = innerZip.GetFiles().ToArray();
                CollectionAssert.AreEqual(new[] { "inner.txt" }, files.Select(i => i.Name).ToArray());

                using (var stream = files[0].OpenRead())
                using (var reader = new StreamReader(stream))
                {
                    Assert.AreEqual("inner text", reader.ReadToEnd());
                }
            }
        }

        [Test]
        [TestCase("11/", null, "11")]
        [TestCase("11.txt", null, "11.txt")]
        [TestCase("11/22/", null, null)]
        [TestCase("11/22.txt", null, null)]
        [TestCase("11/22/33", "11/22/", "33")]
        [TestCase("11/22/33.txt", "11/22/", "33.txt")]
        [TestCase("11/22/33/44", "11/22/", null)]
        [TestCase("11/22/33/44.txt", "11/22/", null)]
        [TestCase("11/", "11/", null)]
        public void GetMyEntryName(string entryFullName, string myLocalName, string expected)
        {
            Assert.AreEqual(expected, ZipFolder.GetMyEntryName(entryFullName, myLocalName));
        }
    }
}
