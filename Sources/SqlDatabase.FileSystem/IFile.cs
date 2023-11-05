using System.IO;

namespace SqlDatabase.FileSystem;

public interface IFile : IFileSystemInfo
{
    IFolder? GetParent();

    Stream OpenRead();
}