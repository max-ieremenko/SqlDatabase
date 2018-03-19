using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SqlDatabase.IO
{
    internal sealed class FileSystemFolder : IFolder
    {
        public FileSystemFolder(string location)
        {
            Location = location;
            Name = Path.GetFileName(location);
        }

        public string Name { get; }

        public string Location { get; }

        public IEnumerable<IFolder> GetFolders()
        {
            var folders = Directory
                .GetDirectories(Location)
                .Select(i => new FileSystemFolder(i))
                .Cast<IFolder>();

            var files = Directory.GetFiles(Location, "*" + ZipFolder.Extension).Select(i => new ZipFolder(i));

            return folders.Concat(files);
        }

        public IEnumerable<IFile> GetFiles()
        {
            return Directory
                .GetFiles(Location)
                .Where(i => !ZipFolder.Extension.Equals(Path.GetExtension(i), StringComparison.OrdinalIgnoreCase))
                .Select(i => new FileSystemFile(i));
        }
    }
}