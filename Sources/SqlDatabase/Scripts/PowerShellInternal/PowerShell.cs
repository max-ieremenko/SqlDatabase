using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace SqlDatabase.Scripts.PowerShellInternal;

internal sealed class PowerShell : IPowerShell
{
    public bool SupportsShouldProcess(string script)
    {
        var attributes = ScriptBlock.Create(script).Attributes;
        for (var i = 0; i < attributes.Count; i++)
        {
            if (attributes[i] is CmdletBindingAttribute binding)
            {
                return binding.SupportsShouldProcess;
            }
        }

        return false;
    }

    public void Invoke(string script, ILogger logger, params KeyValuePair<string, object?>[] parameters)
    {
        var sessionState = InitialSessionState.CreateDefault();
        using (var runSpace = RunspaceFactory.CreateRunspace(sessionState))
        {
            runSpace.ThreadOptions = PSThreadOptions.UseCurrentThread;
            runSpace.Open();

            using (var powerShell = System.Management.Automation.PowerShell.Create())
            using (var listener = new PowerShellStreamsListener(powerShell.Streams, logger))
            {
                powerShell.Runspace = runSpace;

                var ps = powerShell.AddScript(script);
                for (var i = 0; i < parameters.Length; i++)
                {
                    var p = parameters[i];
                    if (p.Value == null)
                    {
                        ps = ps.AddParameter(p.Key);
                    }
                    else
                    {
                        ps.AddParameter(p.Key, p.Value);
                    }
                }

                ps.Invoke();

                if (listener.HasErrors)
                {
                    throw new InvalidOperationException("Errors during script execution.");
                }
            }
        }
    }
}