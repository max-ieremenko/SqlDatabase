using System.IO;
using Moq;
using SqlDatabase.IO;

namespace SqlDatabase.TestApi
{
    internal static class FileFactory
    {
        public static IFile File(string name, byte[] content = null)
        {
            var file = new Mock<IFile>(MockBehavior.Strict);

            file.SetupGet(f => f.Name).Returns(name);

            if (content != null)
            {
                file.Setup(f => f.OpenRead()).Returns(new MemoryStream(content));
            }

            return file.Object;
        }

        public static IFolder Folder(string name, params IFile[] files)
        {
            var folder = new Mock<IFolder>(MockBehavior.Strict);

            folder.SetupGet(f => f.Name).Returns(name);
            folder.Setup(f => f.GetFiles()).Returns(files);
            folder.Setup(f => f.GetFolders()).Returns(new IFolder[0]);

            return folder.Object;
        }
    }
}