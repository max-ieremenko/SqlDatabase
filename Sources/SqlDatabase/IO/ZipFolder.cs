using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace SqlDatabase.IO
{
    [DebuggerDisplay("{Name}")]
    internal sealed class ZipFolder : IFolder
    {
        public const string Extension = ".zip";

        private readonly ZipFolder _parent;

        public ZipFolder(string fileName)
            : this(null, fileName, Path.GetFileName(fileName))
        {
        }

        public ZipFolder(ZipFolder parent, string entryName, string name)
        {
            _parent = parent;

            Name = name;
            FileName = entryName;
        }

        public string Name { get; }

        public string FileName { get; }

        public IEnumerable<IFolder> GetFolders()
        {
            var myLocalName = GetMyLocalName();
            using (var zip = OpenRead())
            {
                foreach (var entry in zip.Entries)
                {
                    var name = GetMyEntryName(entry.FullName, myLocalName);
                    if (name != null &&
                        (IsFolderName(entry.FullName) || IsFolderName(name)))
                    {
                        yield return new ZipFolder(this, entry.FullName, name);
                    }
                }
            }
        }

        public IEnumerable<IFile> GetFiles()
        {
            var myLocalName = GetMyLocalName();
            using (var zip = OpenRead())
            {
                foreach (var entry in zip.Entries)
                {
                    var name = GetMyEntryName(entry.FullName, myLocalName);
                    if (name != null
                        && !IsFolderName(entry.FullName)
                        && !IsFolderName(name))
                    {
                        yield return new ZipFolderFile(this, entry.FullName);
                    }
                }
            }
        }

        internal static string GetMyEntryName(string entryFullName, string myLocalName)
        {
            // 11/
            // 11/11.txt
            // 11/22/
            // 11/22/22.zip
            // 11/22/22.zip/33.txt
            if (myLocalName != null)
            {
                if (!entryFullName.StartsWith(myLocalName, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                // 11/11.txt => 11.txt
                entryFullName = entryFullName.Substring(myLocalName.Length);
            }

            if (entryFullName.Length == 0)
            {
                return null;
            }

            var index = entryFullName.IndexOf("/", StringComparison.CurrentCulture);
            if (index == entryFullName.Length - 1)
            {
                return entryFullName.Substring(0, entryFullName.Length - 1);
            }

            if (index < 0)
            {
                return entryFullName;
            }

            return null;
        }

        internal ZipArchive OpenRead()
        {
            if (_parent == null)
            {
                return ZipFile.OpenRead(FileName);
            }

            if (!Name.EndsWith(Extension, StringComparison.OrdinalIgnoreCase))
            {
                return _parent.OpenRead();
            }

            var file = new ZipFolderFile(_parent, FileName);
            return new ZipArchive(file.OpenRead(), ZipArchiveMode.Read, false);
        }

        private static bool IsFolderName(string name)
        {
            return name.EndsWith("/", StringComparison.OrdinalIgnoreCase)
                   || name.EndsWith(Extension, StringComparison.OrdinalIgnoreCase);
        }

        private string GetMyLocalName()
        {
            return _parent == null || FileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase) ? null : FileName;
        }
    }
}
