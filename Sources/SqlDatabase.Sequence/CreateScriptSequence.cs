using SqlDatabase.Adapter;
using SqlDatabase.FileSystem;

namespace SqlDatabase.Sequence;

public sealed class CreateScriptSequence : ICreateScriptSequence
{
    public CreateScriptSequence(IList<IFileSystemInfo> sources, IScriptFactory scriptFactory)
    {
        Sources = sources;
        ScriptFactory = scriptFactory;
    }

    public IList<IFileSystemInfo> Sources { get; }

    public IScriptFactory ScriptFactory { get; }

    public IList<IScript> BuildSequence()
    {
        var result = new List<IScript>();

        foreach (var source in Sources)
        {
            if (source is IFolder folder)
            {
                Build(folder, null, ScriptFactory, result);
            }
            else if ((source is IFile file) && ScriptFactory.IsSupported(file))
            {
                result.Add(ScriptFactory.FromFile(file));
            }
        }

        return result;
    }

    private static void Build(
        IFolder root,
        string? fullPath,
        IScriptFactory factory,
        List<IScript> scripts)
    {
        fullPath = fullPath == null ? root.Name : Path.Combine(fullPath, root.Name);

        var files = root
            .GetFiles()
            .Where(factory.IsSupported)
            .OrderBy(i => i.Name)
            .Select(factory.FromFile);

        foreach (var file in files)
        {
            file.DisplayName = Path.Combine(fullPath, file.DisplayName);
            scripts.Add(file);
        }

        var folders = root
            .GetFolders()
            .OrderBy(i => i.Name);
        foreach (var subFolder in folders)
        {
            Build(subFolder, fullPath, factory, scripts);
        }
    }
}