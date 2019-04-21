using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SqlDatabase.IO
{
    internal sealed class FileSystemFactory : IFileSystemFactory
    {
        public static IFileSystemInfo FileSystemInfoFromPath(string path)
        {
            path = FileTools.RootPath(path);

            if (File.Exists(path))
            {
                return FileTools.IsZip(path) ? (IFileSystemInfo)new ZipFolder(path) : new FileSystemFile(path);
            }

            if (Directory.Exists(path))
            {
                return new FileSystemFolder(path);
            }

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
                throw new IOException("Directory {0} not found.".FormatWith(path));
            }

            for (var i = 0; i < items.Count - 1; i++)
            {
                var name = items[i];
                path = Path.Combine(path, name);

                entryPoint = entryPoint.GetFolders().FirstOrDefault(f => name.Equals(f.Name, StringComparison.OrdinalIgnoreCase));
                if (entryPoint == null)
                {
                    throw new IOException("Directory {0} not found.".FormatWith(path));
                }
            }

            var resultName = items.Last();
            path = Path.Combine(path, resultName);

            var file = entryPoint.GetFiles().FirstOrDefault(f => resultName.Equals(f.Name, StringComparison.OrdinalIgnoreCase));
            if (file != null)
            {
                return file;
            }

            var folder = entryPoint.GetFolders().FirstOrDefault(f => resultName.Equals(f.Name, StringComparison.OrdinalIgnoreCase));
            if (folder == null)
            {
                throw new IOException("File or folder {0} not found.".FormatWith(path));
            }

            return folder;
        }

        IFileSystemInfo IFileSystemFactory.FileSystemInfoFromPath(string path) => FileSystemInfoFromPath(path);

        private static IFolder TryToResolveEntryPoint(string path)
        {
            if (Directory.Exists(path))
            {
                return new FileSystemFolder(path);
            }

            if (File.Exists(path))
            {
                if (!FileTools.IsZip(path))
                {
                    throw new NotSupportedException("File format [{0}] is not supported as .zip container.".FormatWith(Path.GetExtension(path)));
                }

                return new ZipFolder(path);
            }

            return null;
        }
    }
}