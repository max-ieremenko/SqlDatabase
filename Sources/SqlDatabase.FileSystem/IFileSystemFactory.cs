namespace SqlDatabase.FileSystem;

public interface IFileSystemFactory
{
    IFileSystemInfo FileSystemInfoFromPath(string? path);

    IFileSystemInfo FromContent(string name, string content);
}