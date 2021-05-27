using System;
using System.IO;
using System.Linq;
using SqlDatabase.Configuration;
using SqlDatabase.IO;

namespace SqlDatabase.Scripts
{
    internal sealed class ScriptFactory : IScriptFactory
    {
        public AssemblyScriptConfiguration AssemblyScriptConfiguration { get; set; }

        public IPowerShellFactory PowerShellFactory { get; set; }

        public ISqlTextReader TextReader { get; set; }

        public bool IsSupported(string fileName)
        {
            var ext = Path.GetExtension(fileName);

            return ".sql".Equals(ext, StringComparison.OrdinalIgnoreCase)
                   || ".exe".Equals(ext, StringComparison.OrdinalIgnoreCase)
                   || ".dll".Equals(ext, StringComparison.OrdinalIgnoreCase)
                   || (PowerShellFactory != null && ".ps1".Equals(ext, StringComparison.OrdinalIgnoreCase));
        }

        public IScript FromFile(IFile file)
        {
            var ext = Path.GetExtension(file.Name);

            if (".sql".Equals(ext, StringComparison.OrdinalIgnoreCase))
            {
                return new TextScript
                {
                    DisplayName = file.Name,
                    ReadSqlContent = file.OpenRead,
                    TextReader = TextReader
                };
            }

            if (".exe".Equals(ext, StringComparison.OrdinalIgnoreCase)
                || ".dll".Equals(ext, StringComparison.OrdinalIgnoreCase))
            {
                return new AssemblyScript
                {
                    DisplayName = file.Name,
                    Configuration = AssemblyScriptConfiguration,
                    ReadAssemblyContent = CreateBinaryReader(file),
                    ReadDescriptionContent = CreateScriptDescriptionReader(file)
                };
            }

            if (".ps1".Equals(ext, StringComparison.OrdinalIgnoreCase))
            {
                if (PowerShellFactory == null)
                {
                    throw new NotSupportedException(".ps1 scripts are not supported in this context.");
                }

                PowerShellFactory.Request();

                return new PowerShellScript
                {
                    DisplayName = file.Name,
                    ReadScriptContent = file.OpenRead,
                    ReadDescriptionContent = CreateScriptDescriptionReader(file),
                    PowerShellFactory = PowerShellFactory
                };
            }

            throw new NotSupportedException("File [{0}] cannot be used as script.".FormatWith(file.Name));
        }

        private static Func<byte[]> CreateBinaryReader(IFile file)
        {
            return () => BinaryRead(file);
        }

        private static Func<Stream> CreateScriptDescriptionReader(IFile file)
        {
            return () =>
            {
                var parent = file.GetParent();
                if (parent == null)
                {
                    return null;
                }

                var descriptionName = Path.GetFileNameWithoutExtension(file.Name) + ".txt";
                var description = parent.GetFiles().FirstOrDefault(i => string.Equals(descriptionName, i.Name, StringComparison.OrdinalIgnoreCase));

                return description?.OpenRead();
            };
        }

        private static byte[] BinaryRead(IFile file)
        {
            using (var source = file.OpenRead())
            using (var dest = new MemoryStream())
            {
                source.CopyTo(dest);

                return dest.ToArray();
            }
        }
    }
}