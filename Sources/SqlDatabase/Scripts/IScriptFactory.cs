using SqlDatabase.IO;

namespace SqlDatabase.Scripts
{
    public interface IScriptFactory
    {
        bool IsSupported(string fileName);

        IScript FromFile(IFile file);
    }
}