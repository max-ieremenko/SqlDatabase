namespace SqlDatabase.Adapter.PowerShellScripts;

internal sealed partial class PowerShellFactory : IPowerShellFactory
{
    private bool _initialized;

    private PowerShellFactory(string? installationPath)
    {
        InstallationPath = installationPath;
    }

    public string? InstallationPath { get; private set; }

    public static IPowerShellFactory Create(string? installationPath)
    {
        return new PowerShellFactory(installationPath);
    }

    public void Initialize(ILogger logger)
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        DoInitialize(logger);
    }

    public IPowerShell Create()
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("PowerShell host is not initialized.");
        }

        return new PowerShell();
    }

    partial void DoInitialize(ILogger logger);
}