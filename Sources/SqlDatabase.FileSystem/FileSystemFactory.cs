namespace SqlDatabase.FileSystem;

[DebuggerDisplay("{CurrentDirectory}")]
public sealed class FileSystemFactory : IFileSystemFactory
{
    public FileSystemFactory(string currentDirectory)
    {
        CurrentDirectory = currentDirectory;
    }

    public string CurrentDirectory { get; }

    public IFileSystemInfo FileSystemInfoFromPath(string? path)
    {
        path = FileTools.RootPath(path, CurrentDirectory);

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
            if (TryToResolveEntryPoint(path, out entryPoint))
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

    public IFileSystemInfo FromContent(string name, string content) => new InLineScriptFile(name, content);

    private static bool TryToResolveEntryPoint(string path, [NotNullWhen(true)] out IFolder? entryPoint)
    {
        if (Directory.Exists(path))
        {
            entryPoint = new FileSystemFolder(path);
            return true;
        }

        if (File.Exists(path))
        {
            if (!FileTools.IsZip(path))
            {
                throw new NotSupportedException($"File format [{Path.GetExtension(path)}] is not supported as .zip container.");
            }

            entryPoint = new ZipFolder(path);
            return true;
        }

        entryPoint = null;
        return false;
    }
}