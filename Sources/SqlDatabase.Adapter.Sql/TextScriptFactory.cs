using SqlDatabase.FileSystem;

namespace SqlDatabase.Adapter.Sql;

public class TextScriptFactory : IScriptFactory
{
    private readonly ISqlTextReader _textReader;

    public TextScriptFactory(ISqlTextReader textReader)
    {
        _textReader = textReader;
    }

    public bool IsSupported(IFile file)
    {
        return ".sql".Equals(file.Extension, StringComparison.OrdinalIgnoreCase);
    }

    public IScript FromFile(IFile file)
    {
        return new TextScript(file.Name, file.OpenRead, _textReader);
    }
}