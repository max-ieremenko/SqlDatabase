namespace SqlDatabase.FileSystem;

public interface IFolder : IFileSystemInfo
{
    IEnumerable<IFolder> GetFolders();

    IEnumerable<IFile> GetFiles();
}