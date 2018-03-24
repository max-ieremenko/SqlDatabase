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
                return new TextScript
                {
                    DisplayName = file.Name,
                    ReadSqlContent = CreateTextReader(file)
                };
            }

            if (".exe".Equals(ext, StringComparison.OrdinalIgnoreCase)
                || ".dll".Equals(ext, StringComparison.OrdinalIgnoreCase))
            {
                return new AssemblyScript
                {
                    DisplayName = file.Name,
                    ReadAssemblyContent = CreateBinaryReader(file)
                };
            }

            throw new NotSupportedException("File [{0}] cannot be used as script.".FormatWith(file.Name));
        }

        private static Func<string> CreateTextReader(IFile file)
        {
            return () =>
            {
                using (var stream = file.OpenRead())
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            };
        }

        private static Func<byte[]> CreateBinaryReader(IFile file)
        {
            return () =>
            {
                using (var source = file.OpenRead())
                using (var dest = new MemoryStream())
                {
                    source.CopyTo(dest);

                    return dest.ToArray();
                }
            };
        }
    }
}