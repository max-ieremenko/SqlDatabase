using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SqlDatabase.Scripts
{
    internal static class SqlBatchParser
    {
        private const string ModuleDependencyPattern = "^(-|\\*)+.*module dependency:\\s?(?'name'[\\w\\-]+)(\\s+|\\s*-\\s*)(?'version'[\\.\\w]+)";

        public static IEnumerable<string> SplitByGo(Stream sql)
        {
            var batch = new StringBuilder();

            foreach (var line in ReadLines(sql))
            {
                if (IsGo(line))
                {
                    if (batch.Length > 0)
                    {
                        yield return batch.ToString();
                        batch.Clear();
                    }
                }
                else if (batch.Length > 0 || line.Trim().Length > 0)
                {
                    if (batch.Length > 0)
                    {
                        batch.AppendLine();
                    }

                    batch.Append(line);
                }
            }

            if (batch.Length > 0)
            {
                yield return batch.ToString();
            }
        }

        public static IEnumerable<ScriptDependency> ExtractDependencies(TextReader reader, string scriptName)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (TryParseDependencyLine(line, out var moduleName, out var versionText))
                {
                    if (!Version.TryParse(versionText, out var version))
                    {
                        throw new InvalidOperationException("The current version value [{0}] of module [{1}] is invalid, script {2}.".FormatWith(versionText, moduleName, scriptName));
                    }

                    yield return new ScriptDependency(moduleName, version);
                }
            }
        }

        internal static bool IsGo(string text)
        {
            return Regex.IsMatch(text, "^(\\s*(go)+\\s*)+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        private static IEnumerable<string> ReadLines(Stream sql)
        {
            using (var reader = new StreamReader(sql))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        private static bool TryParseDependencyLine(string line, out string moduleName, out string version)
        {
            moduleName = null;
            version = null;

            var match = Regex.Match(line, ModuleDependencyPattern, RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                return false;
            }

            moduleName = match.Groups["name"].Value;
            version = match.Groups["version"].Value;
            return true;
        }
    }
}
