namespace SqlDatabase.Adapter.AssemblyScripts;

internal sealed class AssemblyScript : IScript
{
    private const string DefaultClassName = "SqlDatabaseScript";
    private const string DefaultMethodName = "Execute";

    public AssemblyScript(
        string displayName,
        string? className,
        string? methodName,
        Func<byte[]> readAssemblyContent,
        Func<Stream?> readDescriptionContent)
    {
        DisplayName = displayName;
        ClassName = string.IsNullOrWhiteSpace(className) ? DefaultClassName : className!;
        MethodName = string.IsNullOrWhiteSpace(methodName) ? DefaultMethodName : methodName!;
        ReadAssemblyContent = readAssemblyContent;
        ReadDescriptionContent = readDescriptionContent;
    }

    public string DisplayName { get; set; }

    public string ClassName { get; set; }

    public string MethodName { get; set; }

    public Func<byte[]> ReadAssemblyContent { get; internal set; }

    public Func<Stream?> ReadDescriptionContent { get; internal set; }

    public void Execute(IDbCommand? command, IVariables variables, ILogger logger)
    {
        var domain = SubDomainFactory.Create(logger, DisplayName, ReadAssemblyContent);

        using (domain)
        {
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

    public TextReader? GetDependencies()
    {
        var description = ReadDescriptionContent();
        if (description == null)
        {
            return null;
        }

        return new StreamReader(description);
    }

    internal void ResolveScriptExecutor(ISubDomain domain)
    {
        if (!domain.ResolveScriptExecutor(ClassName, MethodName))
        {
            throw new InvalidOperationException("Fail to resolve script executor.");
        }
    }

    internal void Execute(ISubDomain domain, IDbCommand? command, IVariables variables)
    {
        if (command != null && !domain.Execute(command, variables))
        {
            throw new InvalidOperationException("Errors during script execution.");
        }
    }
}