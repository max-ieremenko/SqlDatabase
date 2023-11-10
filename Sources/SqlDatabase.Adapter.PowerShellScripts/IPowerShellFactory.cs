namespace SqlDatabase.Adapter.PowerShellScripts;

internal interface IPowerShellFactory
{
    void Initialize(ILogger logger);

    IPowerShell Create();
}