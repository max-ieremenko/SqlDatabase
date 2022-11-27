using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using SqlDatabase.Configuration;
using SqlDatabase.Scripts.AssemblyInternal;

namespace SqlDatabase.Scripts;

internal sealed class AssemblyScript : IScript
{
    public string DisplayName { get; set; }

    public Func<byte[]> ReadAssemblyContent { get; set; }

    public Func<Stream> ReadDescriptionContent { get; set; }

    public AssemblyScriptConfiguration Configuration { get; set; }

    public void Execute(IDbCommand command, IVariables variables, ILogger logger)
    {
        var domain = CreateSubDomain();

        using (domain)
        {
            domain.Logger = logger;
            domain.AssemblyFileName = DisplayName;
            domain.ReadAssemblyContent = ReadAssemblyContent;

            try
            {
                domain.Initialize();
                ResolveScriptExecutor(domain);
                Execute(domain, command, variables);
            }
            finally
            {
                domain.Unload();
            }
        }
    }

    public IEnumerable<IDataReader> ExecuteReader(IDbCommand command, IVariables variables, ILogger logger)
    {
        throw new NotSupportedException("Assembly script does not support readers.");
    }

    public IList<ScriptDependency> GetDependencies()
    {
        using (var description = ReadDescriptionContent())
        {
            if (description == null)
            {
                return new ScriptDependency[0];
            }

            using (var reader = new StreamReader(description))
            {
                return DependencyParser.ExtractDependencies(reader, DisplayName).ToArray();
            }
        }
    }

    internal void ResolveScriptExecutor(ISubDomain domain)
    {
        if (!domain.ResolveScriptExecutor(Configuration.ClassName, Configuration.MethodName))
        {
            throw new InvalidOperationException("Fail to resolve script executor.");
        }
    }

    internal void Execute(ISubDomain domain, IDbCommand command, IVariables variables)
    {
        if (command != null && !domain.Execute(command, variables))
        {
            throw new InvalidOperationException("Errors during script execution.");
        }
    }

    private ISubDomain CreateSubDomain()
    {
#if NET452
            return new AssemblyInternal.Net452.Net452SubDomain();
#else
        return new AssemblyInternal.NetCore.NetCoreSubDomain();
#endif
    }
}