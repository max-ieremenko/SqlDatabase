using System.Collections.Generic;

namespace SqlDatabase.Scripts.PowerShellInternal;

internal interface IPowerShell
{
    bool SupportsShouldProcess(string script);

    void Invoke(string script, ILogger logger, params KeyValuePair<string, object>[] parameters);
}