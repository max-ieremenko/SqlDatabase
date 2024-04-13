using SqlDatabase.FileSystem;

namespace SqlDatabase.Configuration;

public sealed class ConfigurationManager
{
    internal const string Name1 = "SqlDatabase.exe.config";
    internal const string Name2 = "SqlDatabase.dll.config";

    private readonly IFileSystemFactory _fileSystem;

    public ConfigurationManager(IFileSystemFactory fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public static string GetDefaultConfigurationFile() => GetDefaultFile().GetFullName();

    public AppConfiguration LoadFrom(string? configurationFile)
    {
        var source = string.IsNullOrWhiteSpace(configurationFile) ? GetDefaultFile() : GetFile(_fileSystem, configurationFile);

        try
        {
            using (var stream = source.OpenRead())
            {
                return ConfigurationReader.Read(stream);
            }
        }
        catch (Exception ex) when (ex is not IOException)
        {
            throw new ConfigurationErrorsException($"Fail to load configuration from [{configurationFile}].", ex);
        }
    }

    private static IFile GetDefaultFile()
    {
        var fileSystem = new FileSystemFactory(Path.GetDirectoryName(typeof(ConfigurationManager).Assembly.Location)!);
        return GetFile(fileSystem, null);
    }

    private static IFile GetFile(IFileSystemFactory fileSystem, string? configurationFile)
    {
        var location = fileSystem.FileSystemInfoFromPath(configurationFile);
        if (location is IFile file)
        {
            return file;
        }

        return FindFile((IFolder)location);
    }

    private static IFile FindFile(IFolder folder)
    {
        var file = folder
            .GetFiles()
            .Where(i => Name1.Equals(i.Name, StringComparison.OrdinalIgnoreCase) || Name2.Equals(i.Name, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(i => i.Name, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();

        if (file == null)
        {
            throw new FileNotFoundException($"Configuration file {Name2} not found in {folder.GetFullName()}.");
        }

        return file;
    }
}