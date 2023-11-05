using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.RegularExpressions;

namespace SqlDatabase.Scripts;

internal static class DependencyParser
{
    private const string ModuleDependencyPattern = "^(-|\\*)+.*module dependency:\\s?(?'name'[\\w\\-]+)(\\s+|\\s*-\\s*)(?'version'[\\.\\w]+)";

    public static IEnumerable<ScriptDependency> ExtractDependencies(TextReader reader, string scriptName)
    {
        string? line;
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

    private static bool TryParseDependencyLine(
        string line,
        [NotNullWhen(true)] out string? moduleName,
        [NotNullWhen(true)] out string? version)
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