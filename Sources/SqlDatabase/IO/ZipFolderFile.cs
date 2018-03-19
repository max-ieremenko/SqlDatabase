using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace SqlDatabase.IO
{
    [DebuggerDisplay(@"zip\{EntryName}")]
    internal sealed partial class ZipFolderFile : IFile
    {
        public ZipFolderFile(string containerFileName, string entryName)
        {
            ContainerFileName = containerFileName;
            EntryName = entryName;
            Name = Path.GetFileName(entryName);
        }

        public string Name { get; }

        public string ContainerFileName { get; }

        public string EntryName { get; }

        public Stream OpenRead()
        {
            var zip = ZipFile.OpenRead(ContainerFileName);
            var entry = zip.GetEntry(EntryName);

            return new EntryStream(zip, entry.Open());
        }
    }
}
