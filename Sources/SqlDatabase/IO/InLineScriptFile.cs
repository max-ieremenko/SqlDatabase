using System.IO;
using System.Text;

namespace SqlDatabase.IO;

internal sealed class InLineScriptFile : IFile
{
    public InLineScriptFile(string name, string content)
    {
        Name = name;
        Content = content;
    }

    public string Name { get; }

    public string Content { get; }

    public IFolder GetParent() => null;

    public Stream OpenRead()
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(Content));
    }
}