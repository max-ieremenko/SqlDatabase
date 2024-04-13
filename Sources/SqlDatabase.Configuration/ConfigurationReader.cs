using System.Xml;

namespace SqlDatabase.Configuration;

internal static class ConfigurationReader
{
    public static AppConfiguration Read(Stream source)
    {
        var doc = new XmlDocument();
        doc.Load(source);

        var root = doc.SelectSingleNode($"configuration/{AppConfiguration.SectionName}");

        var result = new AppConfiguration();
        if (root == null)
        {
            return result;
        }

        result.GetCurrentVersionScript = root.GetAttribute(AppConfiguration.PropertyGetCurrentVersionScript);
        result.SetCurrentVersionScript = root.GetAttribute(AppConfiguration.PropertySetCurrentVersionScript);

        ReadAssemblyScript(root.SelectSingleNode(AppConfiguration.PropertyAssemblyScript), result.AssemblyScript);
        ReadVariables(root.SelectSingleNode(AppConfiguration.PropertyVariables), result.Variables);
        ReadDatabase(root.SelectSingleNode(AppConfiguration.PropertyMsSql), result.MsSql);
        ReadDatabase(root.SelectSingleNode(AppConfiguration.PropertyMySql), result.MySql);
        ReadDatabase(root.SelectSingleNode(AppConfiguration.PropertyPgSql), result.PgSql);

        return result;
    }

    private static void ReadVariables(XmlNode? source, Dictionary<string, string> destination)
    {
        var entries = source?.SelectNodes("add");
        if (entries == null)
        {
            return;
        }

        foreach (XmlNode entry in entries)
        {
            var name = entry.GetAttribute("name");
            var value = entry.GetAttribute("value");

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ConfigurationErrorsException($"The element {GetPath(entry)} does not contain expected attribute [name].");
            }

            if (destination.ContainsKey(name!))
            {
                throw new ConfigurationErrorsException($"The element {GetPath(entry)} is duplicated, [name = {name}].");
            }

            destination.Add(name!, value ?? string.Empty);
        }
    }

    private static void ReadAssemblyScript(XmlNode? source, AssemblyScriptConfiguration destination)
    {
        destination.ClassName = source.GetAttribute(AssemblyScriptConfiguration.PropertyClassName);
        destination.MethodName = source.GetAttribute(AssemblyScriptConfiguration.PropertyMethodName);
    }

    private static void ReadDatabase(XmlNode? source, DatabaseConfiguration destination)
    {
        destination.GetCurrentVersionScript = source.GetAttribute(DatabaseConfiguration.PropertyGetCurrentVersionScript);
        destination.SetCurrentVersionScript = source.GetAttribute(DatabaseConfiguration.PropertySetCurrentVersionScript);
        ReadVariables(source?.SelectSingleNode(DatabaseConfiguration.PropertyVariables), destination.Variables);
    }

    private static string? GetAttribute(this XmlNode? node, string name)
    {
        return node?.Attributes?[name]?.Value;
    }

    private static string GetPath(XmlNode node)
    {
        var result = new StringBuilder();

        XmlNode? current = node;
        while (current != null)
        {
            if (result.Length > 0)
            {
                result.Insert(0, '/');
            }

            result.Insert(0, current.Name);
            current = node.ParentNode;
        }

        return result.ToString();
    }
}