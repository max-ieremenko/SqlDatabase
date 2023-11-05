using System.Collections.Generic;
using System.IO;
using System.Linq;
using SqlDatabase.FileSystem;

namespace SqlDatabase.Scripts;

internal sealed class CreateScriptSequence : ICreateScriptSequence
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
            else if (ScriptFactory.IsSupported(source.Name))
            {
                result.Add(ScriptFactory.FromFile((IFile)source));
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
            .Where(i => factory.IsSupported(i.Name))
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