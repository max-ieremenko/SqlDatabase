using System.IO;

namespace SqlDatabase.IO
{
    internal sealed class FileSystemFile : IFile
    {
        public FileSystemFile(string location)
        {
            Location = location;
            Name = Path.GetFileName(location);
        }

        public string Name { get; }

        public string Location { get; }

        public Stream OpenRead()
        {
            return File.OpenRead(Location);
        }
    }
}