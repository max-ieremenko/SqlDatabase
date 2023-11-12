using System;
using System.Collections.Generic;
using System.Data;

namespace SqlDatabase.Adapter.AssemblyScripts;

internal sealed class DefaultEntryPoint : IEntryPoint
{
    public DefaultEntryPoint(ILogger log, object scriptInstance, Action<IDbCommand, IReadOnlyDictionary<string, string?>> method)
    {
        Log = log;
        ScriptInstance = scriptInstance;
        Method = method;
    }

    public ILogger Log { get; }

    public object ScriptInstance { get; internal set; }

    public Action<IDbCommand, IReadOnlyDictionary<string, string?>> Method { get; internal set; }

    public bool Execute(IDbCommand command, IReadOnlyDictionary<string, string?> variables)
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