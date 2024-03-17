namespace SqlDatabase.Adapter.AssemblyScripts;

internal interface ISubDomain : IDisposable
{
    void Initialize();

    void Unload();

    bool ResolveScriptExecutor(string className, string methodName);

    bool Execute(IDbCommand command, IVariables variables);
}