namespace SqlDatabase.FileSystem;

internal sealed class InLineScriptFile : IFile
{
    public InLineScriptFile(string name, string content)
    {
        Name = name;
        Content = content;
        Extension = Path.GetExtension(name);
    }

    public string Name { get; }

    public string Content { get; }

    public string Extension { get; }

    public IFolder? GetParent() => null;

    public Stream OpenRead()
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(Content));
    }
}