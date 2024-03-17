namespace SqlDatabase.FileSystem;

public interface IFile : IFileSystemInfo
{
    string Extension { get; }

    IFolder? GetParent();

    Stream OpenRead();
}