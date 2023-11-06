using SqlDatabase.Adapter;
using SqlDatabase.Scripts.PowerShellInternal;

namespace SqlDatabase.Scripts;

internal interface IPowerShellFactory
{
    string? InstallationPath { get; }

    void Request();

    void InitializeIfRequested(ILogger logger);

    IPowerShell Create();
}