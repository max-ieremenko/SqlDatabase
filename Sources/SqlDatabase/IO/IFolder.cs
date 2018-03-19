using System.Collections.Generic;

namespace SqlDatabase.IO
{
    public interface IFolder
    {
        string Name { get; }

        IEnumerable<IFolder> GetFolders();

        IEnumerable<IFile> GetFiles();
    }
}