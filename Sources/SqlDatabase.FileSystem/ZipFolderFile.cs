namespace SqlDatabase.FileSystem;

[DebuggerDisplay(@"zip\{EntryName}")]
internal sealed partial class ZipFolderFile : IFile
{
    private readonly ZipFolder _container;
    private readonly IFolder _parent;

    public ZipFolderFile(ZipFolder container, IFolder parent, string entryFullName)
    {
        _container = container;
        _parent = parent;

        EntryFullName = entryFullName;
        Name = Path.GetFileName(entryFullName);
        Extension = Path.GetExtension(Name);
    }

    public string Name { get; }

    public string EntryFullName { get; }

    public string Extension { get; }

    public IFolder GetParent() => _parent;

    public Stream OpenRead()
    {
        var content = _container.OpenRead();
        var entry = content.GetEntry(EntryFullName);

        return new EntryStream(content, entry!.Open());
    }
}