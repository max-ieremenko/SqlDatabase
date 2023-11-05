using System;
using System.IO;

namespace SqlDatabase.TestApi;

public sealed class TempFile : IDisposable
{
    public TempFile(string extension)
    {
        Location = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + extension);
    }

    public string Location { get; }

    public void Dispose()
    {
        if (File.Exists(Location))
        {
            File.Delete(Location);
        }
    }
}