using System.IO;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.TestApi;

namespace SqlDatabase.IO
{
    [TestFixture]
    public class FileSystemFileTest
    {
        [Test]
        public void GetParent()
        {
            using (var file = new TempFile(".sql"))
            {
                var sut = new FileSystemFile(file.Location);

                var parent = sut.GetParent().ShouldBeOfType<FileSystemFolder>();
                parent.Location.ShouldBe(Path.GetDirectoryName(file.Location));
            }
        }

        [Test]
        public void GetParentDisk()
        {
            var sut = new FileSystemFile(@"c:\11.txt");

            var parent = sut.GetParent().ShouldBeOfType<FileSystemFolder>();
            parent.Location.ShouldBe(@"c:\");
        }
    }
}
