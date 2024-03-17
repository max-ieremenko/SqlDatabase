using SqlDatabase.FileSystem;

namespace SqlDatabase.Adapter.PowerShellScripts;

public sealed class PowerShellScriptFactory : IScriptFactory, IScriptEnvironment
{
    private readonly IPowerShellFactory _powerShell;

    public PowerShellScriptFactory(string? installationPath)
        : this(PowerShellFactory.Create(installationPath))
    {
    }

    internal PowerShellScriptFactory(IPowerShellFactory powerShell)
    {
        _powerShell = powerShell;
    }

    public bool IsSupported(IFile file)
    {
        return ".ps1".Equals(file.Extension, StringComparison.OrdinalIgnoreCase);
    }

    public IScript FromFile(IFile file)
    {
        return new PowerShellScript(
            file.Name,
            file.OpenRead,
            CreateScriptDescriptionReader(file),
            _powerShell);
    }

    public bool IsSupported(IScript script) => script is PowerShellScript;

    public void Initialize(ILogger logger) => _powerShell.Initialize(logger);

    private static Func<Stream?> CreateScriptDescriptionReader(IFile file)
    {
        return () =>
        {
            var parent = file.GetParent();
            if (parent == null)
            {
                return null;
            }

            var descriptionName = Path.GetFileNameWithoutExtension(file.Name) + ".txt";
            var description = parent.GetFiles().FirstOrDefault(i => string.Equals(descriptionName, i.Name, StringComparison.OrdinalIgnoreCase));

            return description?.OpenRead();
        };
    }
}