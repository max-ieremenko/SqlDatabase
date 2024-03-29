using SqlDatabase.FileSystem;

namespace SqlDatabase.Configuration;

public sealed class ConfigurationManager : IConfigurationManager
{
    internal const string Name1 = "SqlDatabase.exe.config";
    internal const string Name2 = "SqlDatabase.dll.config";

    public AppConfiguration SqlDatabase { get; private set; } = null!;

    public static string ResolveDefaultConfigurationFile(string probingPath)
    {
        var fileName = ResolveConfigurationFile(probingPath).Name;
        return Path.Combine(probingPath, fileName);
    }

    public void LoadFrom(string? configurationFile)
    {
        IFile source;
        if (string.IsNullOrWhiteSpace(configurationFile))
        {
            source = ResolveConfigurationFile(Path.GetDirectoryName(GetType().Assembly.Location));
        }
        else
        {
            source = ResolveConfigurationFile(configurationFile!);
        }

        try
        {
            using (var stream = source.OpenRead())
            {
                SqlDatabase = ConfigurationReader.Read(stream);
            }
        }
        catch (Exception ex) when ((ex as IOException) == null)
        {
            throw new ConfigurationErrorsException($"Fail to load configuration from [{configurationFile}].", ex);
        }
    }

    private static IFile ResolveConfigurationFile(string? probingPath)
    {
        var info = FileSystemFactory.FileSystemInfoFromPath(probingPath);
        return ResolveFile(info);
    }

    private static IFile ResolveFile(IFileSystemInfo info)
    {
        IFile? file;
        if (info is IFolder folder)
        {
            file = folder
                .GetFiles()
                .Where(i => Name1.Equals(i.Name, StringComparison.OrdinalIgnoreCase) || Name2.Equals(i.Name, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(i => i.Name, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();

            if (file == null)
            {
                throw new FileNotFoundException($"Configuration file {Name2} not found in {info.Name}.");
            }
        }
        else
        {
            file = (IFile)info;
        }

        return file;
    }
}