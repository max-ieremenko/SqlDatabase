using System.IO;

namespace SqlDatabase.IO
{
    public interface IFile
    {
        string Name { get; }

        Stream OpenRead();
    }
}