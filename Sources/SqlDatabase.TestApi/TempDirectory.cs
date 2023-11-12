using System;
using System.Diagnostics;
using System.IO;
using Shouldly;

namespace SqlDatabase.TestApi;

public sealed class TempDirectory : IDisposable
{
    public TempDirectory(string? name = null)
    {
        Location = Path.Combine(Path.GetTempPath(), name ?? Guid.NewGuid().ToString());
        Directory.CreateDirectory(Location);
    }

    public string Location { get; }

    public string CopyFileFromResources(string resourceName, Type? resourceAnchor = null)
    {
        if (resourceAnchor == null)
        {
            resourceAnchor = new StackTrace().GetFrame(1)!.GetMethod()!.DeclaringType;
        }

        var source = resourceAnchor!.Assembly.GetManifestResourceStream(resourceAnchor.Namespace + "." + resourceName);
        source.ShouldNotBeNull(resourceName);

        var fileName = Path.Combine(Location, resourceName);

        using (source)
        using (var dest = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite))
        {
            source!.CopyTo(dest);
        }

        return fileName;
    }

    public void Dispose()
    {
        if (Directory.Exists(Location))
        {
            Directory.Delete(Location, true);
        }
    }
}