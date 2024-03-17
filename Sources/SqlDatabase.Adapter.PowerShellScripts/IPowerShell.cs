namespace SqlDatabase.Adapter.PowerShellScripts;

internal interface IPowerShell
{
    bool SupportsShouldProcess(string script);

    void Invoke(string script, ILogger logger, params KeyValuePair<string, object?>[] parameters);
}