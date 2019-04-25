namespace SqlDatabase.IO
{
    internal interface IFileSystemFactory
    {
        IFileSystemInfo FileSystemInfoFromPath(string path);

        IFileSystemInfo FromContent(string name, string content);
    }
}
