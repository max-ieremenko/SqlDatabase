using System;
using System.IO;
using SqlDatabase.IO;

namespace SqlDatabase.Scripts
{
    internal sealed class ScriptFactory : IScriptFactory
    {
        public bool IsSupported(string fileName)
        {
            var ext = Path.GetExtension(fileName);

            return ".sql".Equals(ext, StringComparison.OrdinalIgnoreCase)
                   || ".exe".Equals(ext, StringComparison.OrdinalIgnoreCase)
                   || ".dll".Equals(ext, StringComparison.OrdinalIgnoreCase);
        }

        public IScript FromFile(IFile file)
        {
            var ext = Path.GetExtension(file.Name);

            if (".sql".Equals(ext, StringComparison.OrdinalIgnoreCase))
            {
                using (var stream = file.OpenRead())
                using (var reader = new StreamReader(stream))
                {
                    return new TextScript
                    {
                        DisplayName = file.Name,
                        Sql = reader.ReadToEnd()
                    };
                }
            }

            if (".exe".Equals(ext, StringComparison.OrdinalIgnoreCase)
                || ".dll".Equals(ext, StringComparison.OrdinalIgnoreCase))
            {
                using (var source = file.OpenRead())
                using (var dest = new MemoryStream())
                {
                    source.CopyTo(dest);

                    return new AssemblyScript
                    {
                        DisplayName = file.Name,
                        Assembly = dest.ToArray()
                    };
                }
            }

            throw new NotSupportedException("File [{0}] cannot be used as script.".FormatWith(file.Name));
        }
    }
}