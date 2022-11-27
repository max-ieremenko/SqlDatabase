using System.Configuration;

namespace SqlDatabase.Configuration;

public sealed class AssemblyScriptConfiguration : ConfigurationElement
{
    private const string PropertyClassName = "className";
    private const string PropertyMethodName = "methodName";

    public AssemblyScriptConfiguration()
    {
    }

    public AssemblyScriptConfiguration(string className, string methodName)
    {
        ClassName = className;
        MethodName = methodName;
    }

    [ConfigurationProperty(PropertyClassName, DefaultValue = "SqlDatabaseScript")]
    public string ClassName
    {
        get => (string)this[PropertyClassName];
        set => this[PropertyClassName] = value;
    }

    [ConfigurationProperty(PropertyMethodName, DefaultValue = "Execute")]
    public string MethodName
    {
        get => (string)this[PropertyMethodName];
        set => this[PropertyMethodName] = value;
    }
}