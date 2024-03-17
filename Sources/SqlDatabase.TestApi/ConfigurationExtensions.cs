using System.Diagnostics;
using System.Xml;
using Shouldly;

namespace SqlDatabase.TestApi;

public static class ConfigurationExtensions
{
    public static string GetConnectionString(string name, Type? anchor = null)
    {
        if (anchor == null)
        {
            anchor = new StackTrace().GetFrame(1)!.GetMethod()!.DeclaringType;
        }

        var fileName = Path.Combine(AppContext.BaseDirectory, anchor.Assembly.GetName().Name + ".dll.config");

        var config = new XmlDocument();
        config.Load(fileName);

        var node = config.SelectSingleNode($"configuration/connectionStrings/add[@name = '{name}']")?.Attributes?["connectionString"];
        node.ShouldNotBeNull();

        return node.Value;
    }
}