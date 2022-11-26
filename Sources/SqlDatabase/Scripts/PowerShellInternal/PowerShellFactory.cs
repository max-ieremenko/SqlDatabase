using System;

namespace SqlDatabase.Scripts.PowerShellInternal;

internal sealed partial class PowerShellFactory : IPowerShellFactory
{
    private bool _requested;
    private bool _initialized;

    private PowerShellFactory(string installationPath)
    {
        InstallationPath = installationPath;
    }

    public string InstallationPath { get; private set; }

    // only for tests
    internal static IPowerShellFactory SharedTestFactory { get; set; }

    public static IPowerShellFactory Create(string installationPath)
    {
        if (SharedTestFactory != null)
        {
            return SharedTestFactory;
        }

        return new PowerShellFactory(installationPath);
    }

    public void Request()
    {
        _requested = true;
    }

    public void InitializeIfRequested(ILogger logger)
    {
        if (_initialized || !_requested)
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