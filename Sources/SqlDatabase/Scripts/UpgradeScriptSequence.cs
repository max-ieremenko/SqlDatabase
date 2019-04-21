using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SqlDatabase.IO;

namespace SqlDatabase.Scripts
{
    internal sealed class UpgradeScriptSequence : IUpgradeScriptSequence
    {
        public IList<IFileSystemInfo> Sources { get; set; } = new List<IFileSystemInfo>();

        public IScriptFactory ScriptFactory { get; set; }

        public IList<ScriptStep> BuildSequence(Version currentVersion)
        {
            var files = ExpandSources(Sources)
                .Where(i => ScriptFactory.IsSupported(i.Key.Name))
                .OrderBy(i => i.Value.From)
                .ThenByDescending(i => i.Value.To)
                .ToList();

            var result = new List<ScriptStep>();
            if (files.Count == 0)
            {
                return result;
            }

            var maxVersion = files.Select(i => i.Value.To).Max();
            if (maxVersion == currentVersion)
            {
                return result;
            }

            if (maxVersion < currentVersion)
            {
                throw new InvalidOperationException("Current version [{0}] is greater then latest upgrade [{1}].".FormatWith(currentVersion, maxVersion));
            }

            files = files
                .Where(i => i.Value.From >= currentVersion)
                .ToList();

            var version = currentVersion;
            while (files.Count > 0)
            {
                var file = files[0];
                files.RemoveAt(0);

                if (version != file.Value.From)
                {
                    continue;
                }

                if (files.Count > 0 && files[0].Value.From == version && files[0].Value.To == file.Value.To)
                {
                    throw new InvalidOperationException("Duplicated step found [{0}] and [{1}].".FormatWith(file.Key.Name, files[0].Key.Name));
                }

                result.Add(new ScriptStep
                {
                    From = version,
                    To = file.Value.To,
                    Script = ScriptFactory.FromFile(file.Key)
                });
                version = file.Value.To;
            }

            if (version != maxVersion)
            {
                throw new InvalidOperationException("Upgrade step from [{0}] to a next not found.".FormatWith(version));
            }

            return result;
        }

        internal static FileVersion ParseName(string name)
        {
            var index = name.LastIndexOf(".", StringComparison.OrdinalIgnoreCase);
            if (index > 0)
            {
                name = name.Substring(0, index);
            }

            index = name.IndexOf("_", StringComparison.InvariantCultureIgnoreCase);
            if (index <= 0 || index == name.Length)
            {
                return null;
            }

            var result = new FileVersion
            {
                From = ParseVersion(name.Substring(0, index)),
                To = ParseVersion(name.Substring(index + 1))
            };

            return result.From != null && result.To != null && result.From < result.To ? result : null;
        }

        private static Version ParseVersion(string value)
        {
            if (Version.TryParse(value, out var ver))
            {
                return ver;
            }

            if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var major) && major >= 0)
            {
                return new Version(major, 0);
            }

            return null;
        }

        private static IEnumerable<KeyValuePair<IFile, FileVersion>> ExpandSources(IEnumerable<IFileSystemInfo> sources)
        {
            var result = Enumerable.Empty<KeyValuePair<IFile, FileVersion>>();

            foreach (var source in sources)
            {
                if (source is IFolder folder)
                {
                    result = result.Concat(GetContent(folder));
                }
                else
                {
                    result = result.Concat(new[]
                    {
                        new KeyValuePair<IFile, FileVersion>((IFile)source, ParseName(source.Name))
                    });
                }
            }

            return result;
        }

        private static IEnumerable<KeyValuePair<IFile, FileVersion>> GetContent(IFolder folder)
        {
            var subFiles = folder
                .GetFolders()
                .Where(i => ParseName(i.Name) != null)
                .SelectMany(GetContent);

            var files = folder
                .GetFiles()
                .Select(i => new KeyValuePair<IFile, FileVersion>(i, ParseName(i.Name)))
                .Where(i => i.Value != null);

            return subFiles.Concat(files);
        }

        internal sealed class FileVersion
        {
            public Version From { get; set; }

            public Version To { get; set; }
        }
    }
}
