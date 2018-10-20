using System.IO;

namespace SqlDatabase.IO
{
    public interface IFile : IFileSystemInfo
    {
        Stream OpenRead();
    }
}