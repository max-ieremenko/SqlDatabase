using System.Configuration;

namespace SqlDatabase.Configuration;

public sealed class DatabaseConfiguration : ConfigurationElement
{
    private const string PropertyGetCurrentVersionScript = "getCurrentVersion";
    private const string PropertySetCurrentVersionScript = "setCurrentVersion";
    private const string PropertyVariables = "variables";

    [ConfigurationProperty(PropertyGetCurrentVersionScript)]
    public string GetCurrentVersionScript
    {
        get => (string)this[PropertyGetCurrentVersionScript];
        set => this[PropertyGetCurrentVersionScript] = value;
    }

    [ConfigurationProperty(PropertySetCurrentVersionScript)]
    public string SetCurrentVersionScript
    {
        get => (string)this[PropertySetCurrentVersionScript];
        set => this[PropertySetCurrentVersionScript] = value;
    }

    [ConfigurationProperty(PropertyVariables)]
    public NameValueConfigurationCollection Variables => (NameValueConfigurationCollection)this[PropertyVariables];
}