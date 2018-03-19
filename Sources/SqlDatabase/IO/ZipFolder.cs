using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace SqlDatabase.IO
{
    [DebuggerDisplay("{Name}")]
    internal sealed class ZipFolder : IFolder
    {
        public const string Extension = ".zip";

        public ZipFolder(string fileName)
        {
            Name = Path.GetFileName(fileName);
            FileName = fileName;
        }

        public string Name { get; }

        public string FileName { get; }

        public IEnumerable<IFolder> GetFolders()
        {
            return Enumerable.Empty<IFolder>();
        }

        public IEnumerable<IFile> GetFiles()
        {
            using (var zip = ZipFile.OpenRead(FileName))
            {
                return zip
                    .Entries
                    .Where(i => !i.FullName.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                    .Select(i => new ZipFolderFile(FileName, i.FullName))
                    .ToArray();
            }
        }
    }
}
