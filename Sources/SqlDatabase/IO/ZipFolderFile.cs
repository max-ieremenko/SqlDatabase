using System.Diagnostics;
using System.IO;

namespace SqlDatabase.IO
{
    [DebuggerDisplay(@"zip\{EntryName}")]
    internal sealed partial class ZipFolderFile : IFile
    {
        private readonly ZipFolder _parent;

        public ZipFolderFile(ZipFolder parent, string entryFullName)
        {
            _parent = parent;

            EntryFullName = entryFullName;
            Name = Path.GetFileName(entryFullName);
        }

        public string Name { get; }

        public string EntryFullName { get; }

        public Stream OpenRead()
        {
            var content = _parent.OpenRead();
            var entry = content.GetEntry(EntryFullName);

            return new EntryStream(content, entry.Open());
        }
    }
}
