using System.IO;

namespace SqlDatabase.IO;

public interface IFile : IFileSystemInfo
{
    IFolder GetParent();

    Stream OpenRead();
}