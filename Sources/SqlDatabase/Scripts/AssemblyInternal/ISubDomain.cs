using System;
using System.Data;

namespace SqlDatabase.Scripts.AssemblyInternal;

internal interface ISubDomain : IDisposable
{
    void Initialize();

    void Unload();

    bool ResolveScriptExecutor(string className, string methodName);

    bool Execute(IDbCommand command, IVariables variables);
}