using System;
using System.Data;

namespace SqlDatabase.Scripts.AssemblyInternal
{
    internal interface ISubDomain : IDisposable
    {
        ILogger Logger { get; set; }

        string AssemblyFileName { get; set; }

        Func<byte[]> ReadAssemblyContent { get; set; }

        void Initialize();

        void Unload();

        bool ResolveScriptExecutor(string className, string methodName);

        bool Execute(IDbCommand command, IVariables variables);
    }
}
