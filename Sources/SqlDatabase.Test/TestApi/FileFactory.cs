using System.IO;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using SqlDatabase.IO;

namespace SqlDatabase.TestApi;

internal static class FileFactory
{
    public static IFile File(string name, byte[] content, IFolder parent)
    {
        var file = new Mock<IFile>(MockBehavior.Strict);

        file.SetupGet(f => f.Name).Returns(name);

        if (content != null)
        {
            file.Setup(f => f.OpenRead()).Returns(new MemoryStream(content));
        }

        file.Setup(f => f.GetParent()).Returns(parent);

        return file.Object;
    }

    public static IFile File(string name, byte[] content = null) => File(name, content, null);

    public static IFile File(string name, string content) => File(name, content, null);

    public static IFile File(string name, string content, IFolder parent)
    {
        return File(
            name,
            string.IsNullOrEmpty(content) ? new byte[0] : Encoding.UTF8.GetBytes(content),
            parent);
    }

    public static IFolder Folder(string name, params IFileSystemInfo[] content)
    {
        var folder = new Mock<IFolder>(MockBehavior.Strict);

        folder.SetupGet(f => f.Name).Returns(name);

        var files = content.OfType<IFile>().ToArray();
        folder.Setup(f => f.GetFiles()).Returns(files);

        var subFolders = content.OfType<IFolder>().ToArray();
        folder.Setup(f => f.GetFolders()).Returns(subFolders);

        return folder.Object;
    }

    public static string ReadAllText(this IFile file)
    {
        Assert.IsNotNull(file);

        using (var stream = file.OpenRead())
        using (var reader = new StreamReader(stream))
        {
            return reader.ReadToEnd();
        }
    }
}