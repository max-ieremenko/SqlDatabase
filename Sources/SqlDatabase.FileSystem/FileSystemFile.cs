using System.IO;

namespace SqlDatabase.FileSystem;

internal sealed class FileSystemFile : IFile
{
    public FileSystemFile(string location)
    {
        Location = location;
        Name = Path.GetFileName(location);
        Extension = Path.GetExtension(Name);
    }

    public string Name { get; }

    public string Location { get; }

    public string Extension { get; }

    public IFolder GetParent()
    {
        return new FileSystemFolder(Path.GetDirectoryName(Location)!);
    }

    public Stream OpenRead()
    {
        return File.OpenRead(Location);
    }
}