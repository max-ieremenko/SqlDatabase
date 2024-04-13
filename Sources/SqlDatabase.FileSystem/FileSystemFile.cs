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

    public string GetFullName() => Location;

    public IFolder GetParent() => new FileSystemFolder(Path.GetDirectoryName(Location)!);

    public Stream OpenRead() => File.OpenRead(Location);
}