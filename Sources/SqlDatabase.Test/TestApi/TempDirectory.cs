using System;
using System.IO;
using NUnit.Framework;

namespace SqlDatabase.TestApi
{
    internal sealed class TempDirectory : IDisposable
    {
        public TempDirectory(string name = null)
        {
            Location = Path.Combine(Path.GetTempPath(), name ?? Guid.NewGuid().ToString());
            Directory.CreateDirectory(Location);
        }

        public string Location { get; }

        public string CopyFileFromResources(string resourceName)
        {
            var source = GetType().Assembly.GetManifestResourceStream("SqlDatabase.Resources." + resourceName);
            Assert.IsNotNull(source, resourceName);

            var fileName = Path.Combine(Location, resourceName);

            using (source)
            using (var dest = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite))
            {
                source.CopyTo(dest);
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
}
