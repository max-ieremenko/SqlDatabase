using SqlDatabase.FileSystem;

namespace SqlDatabase.Scripts;

public interface IScriptFactory
{
    bool IsSupported(string fileName);

    IScript FromFile(IFile file);
}