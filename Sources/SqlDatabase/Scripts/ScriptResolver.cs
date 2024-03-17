using SqlDatabase.Adapter;
using SqlDatabase.FileSystem;

namespace SqlDatabase.Scripts;

internal sealed class ScriptResolver : IScriptResolver, IScriptFactory
{
    public ScriptResolver(IScriptFactory[] factories)
    {
        Factories = factories;
    }

    public IScriptFactory[] Factories { get; }

    public bool IsSupported(IFile file) => FindSupported(file) != null;

    public IScript FromFile(IFile file)
    {
        var factory = FindSupported(file);
        if (factory == null)
        {
            throw new NotSupportedException($"File [{file.Name}] cannot be used as script.");
        }

        return factory.FromFile(file);
    }

    public void InitializeEnvironment(ILogger logger, IEnumerable<IScript> scripts)
    {
        var environments = new List<IScriptEnvironment>(Factories.Length);
        for (var i = 0; i < Factories.Length; i++)
        {
            if (Factories[i] is IScriptEnvironment env)
            {
                environments.Add(env);
            }
        }

        if (environments.Count == 0)
        {
            return;
        }

        foreach (var script in scripts)
        {
            for (var i = 0; i < environments.Count; i++)
            {
                var env = environments[i];
                if (env.IsSupported(script))
                {
                    env.Initialize(logger);
                    environments.RemoveAt(i);
                    break;
                }
            }

            if (environments.Count == 0)
            {
                return;
            }
        }
    }

    private IScriptFactory? FindSupported(IFile file)
    {
        for (var i = 0; i < Factories.Length; i++)
        {
            var result = Factories[i];
            if (result.IsSupported(file))
            {
                return result;
            }
        }

        return null;
    }
}