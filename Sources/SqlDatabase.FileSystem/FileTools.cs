namespace SqlDatabase.FileSystem;

internal static class FileTools
{
    private const string ZipExtension = ".zip";
    private const string NuGetExtension = ".nupkg";

    public static string RootPath(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        if (!Path.IsPathRooted(path))
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
        }

        return path!;
    }

    public static IEnumerable<string> GetZipExtensions()
    {
        return new[]
        {
            ZipExtension,
            NuGetExtension
        };
    }

    public static bool IsZip(string path)
    {
        var ext = Path.GetExtension(path);
        return ZipExtension.Equals(ext, StringComparison.OrdinalIgnoreCase)
               || NuGetExtension.Equals(ext, StringComparison.OrdinalIgnoreCase);
    }
}