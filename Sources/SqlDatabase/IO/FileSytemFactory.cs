using System;
using System.IO;

namespace SqlDatabase.IO
{
    internal static class FileSytemFactory
    {
        public static IFolder FolderFromPath(string path)
        {
            if (ZipFolder.Extension.Equals(Path.GetExtension(path), StringComparison.OrdinalIgnoreCase)
                && !Directory.Exists(path))
            {
                return new ZipFolder(path);
            }

            return new FileSystemFolder(path);
        }
    }
}