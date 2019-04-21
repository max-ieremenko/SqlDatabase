namespace SqlDatabase.IO
{
    internal interface IFileSystemFactory
    {
        IFileSystemInfo FileSystemInfoFromPath(string path);
    }
}
