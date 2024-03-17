namespace SqlDatabase.FileSystem;

public sealed class FileSystemFactory : IFileSystemFactory
{
    public static IFileSystemInfo FileSystemInfoFromPath(string? path)
    {
        path = FileTools.RootPath(path);

        if (File.Exists(path))
        {
            return FileTools.IsZip(path) ? new ZipFolder(path) : new FileSystemFile(path);
        }

        if (Directory.Exists(path))
        {
            return new FileSystemFolder(path);
        }

        IFolder? entryPoint = null;
        var items = new List<string>();

        while (!string.IsNullOrEmpty(path))
        {
            entryPoint = TryToResolveEntryPoint(path);
            if (entryPoint != null)
            {
                break;
            }

            items.Insert(0, Path.GetFileName(path));
            path = Path.GetDirectoryName(path);
        }

        if (entryPoint == null)
        {
            throw new IOException($"Directory {path} not found.");
        }

        for (var i = 0; i < items.Count - 1; i++)
        {
            var name = items[i];
            path = Path.Combine(path!, name);

            entryPoint = entryPoint.GetFolders().FirstOrDefault(f => name.Equals(f.Name, StringComparison.OrdinalIgnoreCase));
            if (entryPoint == null)
            {
                throw new IOException($"Directory {path} not found.");
            }
        }

        var resultName = items.Last();
        path = Path.Combine(path!, resultName);

        var file = entryPoint.GetFiles().FirstOrDefault(f => resultName.Equals(f.Name, StringComparison.OrdinalIgnoreCase));
        if (file != null)
        {
            return file;
        }

        var folder = entryPoint.GetFolders().FirstOrDefault(f => resultName.Equals(f.Name, StringComparison.OrdinalIgnoreCase));
        if (folder == null)
        {
            throw new IOException($"File or folder {path} not found.");
        }

        return folder;
    }

    IFileSystemInfo IFileSystemFactory.FileSystemInfoFromPath(string? path) => FileSystemInfoFromPath(path);

    public IFileSystemInfo FromContent(string name, string content)
    {
        return new InLineScriptFile(name, content);
    }

    private static IFolder? TryToResolveEntryPoint(string path)
    {
        if (Directory.Exists(path))
        {
            return new FileSystemFolder(path);
        }

        if (File.Exists(path))
        {
            if (!FileTools.IsZip(path))
            {
                throw new NotSupportedException($"File format [{Path.GetExtension(path)}] is not supported as .zip container.");
            }

            return new ZipFolder(path);
        }

        return null;
    }
}