using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SqlDatabase.IO
{
    internal static class FileSytemFactory
    {
        public static IFolder FolderFromPath(string path)
        {
            path = RootPath(path);

            IFolder entryPoint = null;
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
                throw new DirectoryNotFoundException("Directory {0} not found.".FormatWith(path));
            }

            for (var i = 0; i < items.Count; i++)
            {
                var name = items[i];
                path = Path.Combine(path, name);

                entryPoint = entryPoint.GetFolders().FirstOrDefault(f => name.Equals(f.Name, StringComparison.OrdinalIgnoreCase));
                if (entryPoint == null)
                {
                    throw new DirectoryNotFoundException("Directory {0} not found.".FormatWith(path));
                }
            }

            return entryPoint;
        }

        public static IFile FileFromPath(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            IFolder folder;
            try
            {
                folder = FolderFromPath(Path.GetDirectoryName(fileName));
            }
            catch (DirectoryNotFoundException ex)
            {
                throw new FileNotFoundException("File {0} not found.".FormatWith(fileName), fileName, ex);
            }

            var name = Path.GetFileName(fileName);

            var file = folder.GetFiles().FirstOrDefault(i => name.Equals(i.Name, StringComparison.OrdinalIgnoreCase));
            if (file == null)
            {
                throw new FileNotFoundException("File {0} not found.".FormatWith(fileName), fileName);
            }

            return file;
        }

        private static IFolder TryToResolveEntryPoint(string path)
        {
            if (Directory.Exists(path))
            {
                return new FileSystemFolder(path);
            }

            if (File.Exists(path))
            {
                var ext = Path.GetExtension(path);
                if (!ZipFolder.Extension.Equals(ext, StringComparison.OrdinalIgnoreCase))
                {
                    throw new NotSupportedException("File format [{0}] is not supported as .zip container.".FormatWith(ext));
                }

                return new ZipFolder(path);
            }

            return null;
        }

        private static string RootPath(string path)
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
    }
}