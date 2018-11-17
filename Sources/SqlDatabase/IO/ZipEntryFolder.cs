using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SqlDatabase.IO
{
    [DebuggerDisplay("{Name}")]
    internal sealed class ZipEntryFolder : IFolder
    {
        public ZipEntryFolder(IEnumerable<string> names)
        {
            Name = string.Join("\\", names);

            FolderByName = new Dictionary<string, IFolder>(StringComparer.OrdinalIgnoreCase);
            Files = new List<ZipFolderFile>();
        }

        public string Name { get; }

        public IDictionary<string, IFolder> FolderByName { get; }

        public IList<ZipFolderFile> Files { get; }

        public IEnumerable<IFolder> GetFolders() => FolderByName.Values;

        public IEnumerable<IFile> GetFiles() => Files;
    }
}
