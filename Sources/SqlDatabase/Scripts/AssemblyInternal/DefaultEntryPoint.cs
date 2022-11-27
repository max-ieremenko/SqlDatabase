using System;
using System.Collections.Generic;
using System.Data;

namespace SqlDatabase.Scripts.AssemblyInternal;

internal sealed class DefaultEntryPoint : IEntryPoint
{
    public ILogger Log { get; set; }

    public object ScriptInstance { get; set; }

    public Action<IDbCommand, IReadOnlyDictionary<string, string>> Method { get; set; }

    public bool Execute(IDbCommand command, IReadOnlyDictionary<string, string> variables)
    {
        var result = false;

        try
        {
            Method(command, variables);
            result = true;
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
        finally
        {
            (ScriptInstance as IDisposable)?.Dispose();
        }

        return result;
    }
}