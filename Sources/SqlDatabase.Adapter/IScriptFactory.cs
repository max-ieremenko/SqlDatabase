using SqlDatabase.FileSystem;

namespace SqlDatabase.Adapter;

public interface IScriptFactory
{
    bool IsSupported(IFile file);

    IScript FromFile(IFile file);
}