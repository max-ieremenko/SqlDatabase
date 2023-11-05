using System.IO;

namespace SqlDatabase.IO;

internal sealed class FileSystemFile : IFile
{
    public FileSystemFile(string location)
    {
        Location = location;
        Name = Path.GetFileName(location);
    }

    public string Name { get; }

    public string Location { get; }

    public IFolder GetParent()
    {
        return new FileSystemFolder(Path.GetDirectoryName(Location)!);
    }

    public Stream OpenRead()
    {
        return File.OpenRead(Location);
    }
}