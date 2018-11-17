using System;
using System.IO;

namespace SqlDatabase.IO
{
    internal static class FileTools
    {
        public const string ZipExtension = ".zip";

        public static string RootPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return AppDomain.CurrentDomain.BaseDirectory;
            }

            if (!Path.IsPathRooted(path))
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            }

            return path;
        }

        public static bool IsZip(string path)
        {
            var ext = Path.GetExtension(path);
            return ZipExtension.Equals(ext, StringComparison.OrdinalIgnoreCase);
        }
    }
}
