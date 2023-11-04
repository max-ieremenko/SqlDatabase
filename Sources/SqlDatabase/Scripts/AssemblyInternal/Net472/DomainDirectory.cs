using System;
using System.IO;

namespace SqlDatabase.Scripts.AssemblyInternal.Net472;

internal sealed class DomainDirectory : IDisposable
{
    private readonly ILogger _logger;

    public DomainDirectory(ILogger logger)
    {
        _logger = logger;

        Location = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(Location);
    }

    public string Location { get; }

    public string SaveFile(byte[] content, string fileName)
    {
        var location = Path.Combine(Location, fileName);
        try
        {
            File.WriteAllBytes(location, content);
        }
        catch (Exception ex)
        {
            _logger.Error("Fail to copy content of [{0}]: {1}".FormatWith(fileName, ex.Message));
            File.Delete(location);
            throw;
        }

        return location;
    }

    public void Dispose()
    {
        if (Directory.Exists(Location))
        {
            try
            {
                Directory.Delete(Location, true);
            }
            catch (Exception ex)
            {
                _logger.Info("Fail to delete assembly content from {0}: {1}".FormatWith(Location, ex.Message));
            }
        }
    }
}