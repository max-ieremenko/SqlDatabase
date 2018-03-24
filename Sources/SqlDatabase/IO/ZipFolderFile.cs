using System.Diagnostics;
using System.IO;

namespace SqlDatabase.IO
{
    [DebuggerDisplay(@"zip\{EntryName}")]
    internal sealed partial class ZipFolderFile : IFile
    {
        private readonly ZipFolder _parent;

        public ZipFolderFile(ZipFolder parent, string entryName)
        {
            _parent = parent;

            EntryName = entryName;
            Name = Path.GetFileName(entryName);
        }

        public string Name { get; }

        public string EntryName { get; }

        public Stream OpenRead()
        {
            var content = _parent.OpenRead();
            var entry = content.GetEntry(EntryName);

            return new EntryStream(content, entry.Open());
        }
    }
}
