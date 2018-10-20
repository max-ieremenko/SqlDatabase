using System.Collections.Generic;

namespace SqlDatabase.IO
{
    public interface IFolder : IFileSystemInfo
    {
        IEnumerable<IFolder> GetFolders();

        IEnumerable<IFile> GetFiles();
    }
}