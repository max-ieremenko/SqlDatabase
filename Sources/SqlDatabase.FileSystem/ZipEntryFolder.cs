namespace SqlDatabase.FileSystem;

[DebuggerDisplay("{Name}")]
internal sealed class ZipEntryFolder : IFolder
{
    private readonly string _fullName;

    public ZipEntryFolder(string name, string fullName)
    {
        _fullName = fullName;
        Name = name;

        FolderByName = new Dictionary<string, IFolder>(StringComparer.OrdinalIgnoreCase);
        Files = new List<ZipFolderFile>();
    }

    public string Name { get; }

    public IDictionary<string, IFolder> FolderByName { get; }

    public IList<ZipFolderFile> Files { get; }

    public string GetFullName() => _fullName;

    public IEnumerable<IFolder> GetFolders() => FolderByName.Values;

    public IEnumerable<IFile> GetFiles() => Files;
}