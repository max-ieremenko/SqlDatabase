using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using SqlDatabase.Adapter;
using SqlDatabase.FileSystem;

namespace SqlDatabase.Sequence;

internal sealed class UpgradeScriptCollection
{
    private readonly IDictionary<string, IList<ScriptStep>> _stepsByModule = new Dictionary<string, IList<ScriptStep>>(StringComparer.OrdinalIgnoreCase);
    private readonly IDictionary<IScript, IList<ScriptDependency>> _dependencyByStep = new Dictionary<IScript, IList<ScriptDependency>>();

    public UpgradeScriptCollection(bool folderAsModuleName)
    {
        FolderAsModuleName = folderAsModuleName;
    }

    public bool FolderAsModuleName { get; }

    public ICollection<string> ModuleNames => _stepsByModule.Keys;

    public IList<ScriptStep> GetSteps(string moduleName) => _stepsByModule[moduleName];

    public void LoadFrom(IEnumerable<IFileSystemInfo> sources, IScriptFactory scriptFactory)
    {
        LoadFrom(sources, scriptFactory, 0, null);
    }

    public void BuildModuleSequence(string moduleName, Version moduleVersion)
    {
        var files = _stepsByModule[moduleName];
        _stepsByModule.Remove(moduleName);

        var maxVersion = files.Select(i => i.To).Max();
        if (maxVersion == moduleVersion)
        {
            // up to date
            return;
        }

        if (maxVersion < moduleVersion)
        {
            if (string.IsNullOrEmpty(moduleName))
            {
                throw new InvalidOperationException($"The current version [{moduleVersion}] is greater then latest upgrade [{maxVersion}].");
            }

            throw new InvalidOperationException($"Module [{moduleName}], the current version [{moduleVersion}] is greater then latest upgrade [{maxVersion}].");
        }

        files = files
            .Where(i => i.From >= moduleVersion)
            .OrderBy(i => i.From)
            .ThenByDescending(i => i.To)
            .ToList();

        var sequence = new List<ScriptStep>();
        var version = moduleVersion;
        while (files.Count > 0)
        {
            var file = files[0];
            files.RemoveAt(0);

            if (version != file.From)
            {
                continue;
            }

            if (files.Count > 0 && files[0].From == version && files[0].To == file.To)
            {
                if (string.IsNullOrEmpty(moduleName))
                {
                    throw new InvalidOperationException($"Duplicated step found [{file.Script.DisplayName}] and [{files[0].Script.DisplayName}].");
                }

                throw new InvalidOperationException($"Module [{moduleName}], duplicated step found [{file.Script.DisplayName}] and [{files[0].Script.DisplayName}].");
            }

            sequence.Add(file);
            version = file.To;
        }

        if (version != maxVersion)
        {
            if (string.IsNullOrEmpty(moduleName))
            {
                throw new InvalidOperationException($"Upgrade step from [{version}] to a next not found.");
            }

            throw new InvalidOperationException($"Module [{moduleName}], upgrade step from [{version}] to a next not found.");
        }

        _stepsByModule[moduleName] = sequence;
    }

    public void LoadDependencies()
    {
        foreach (var steps in _stepsByModule.Values)
        {
            foreach (var step in steps)
            {
                var dependencies = Array.Empty<ScriptDependency>();
                using var reader = step.Script.GetDependencies();
                if (reader != null)
                {
                    dependencies = DependencyParser.ExtractDependencies(reader, step.Script.DisplayName).ToArray();
                }

                _dependencyByStep.Add(step.Script, dependencies);
            }
        }
    }

    public void ShowWithDependencies(ILogger logger)
    {
        foreach (var moduleName in _stepsByModule.Keys.OrderBy(i => i))
        {
            var steps = _stepsByModule[moduleName];

            var s = steps.Count == 1 ? null : "s";
            logger.Info($"module [{moduleName}], {steps.Count} step{s}:");

            using (logger.Indent())
            {
                foreach (var step in steps)
                {
                    var dependencies = GetDependencies(step);
                    if (dependencies.Count == 0)
                    {
                        logger.Info($"{step.Script.DisplayName}, no dependencies");
                    }
                    else
                    {
                        var dependsOn = string.Join("; ", dependencies.OrderBy(i => i.ModuleName));
                        logger.Info($"{step.Script.DisplayName}, depends on {dependsOn}");
                    }
                }
            }
        }
    }

    public void ValidateModuleDependencies(string moduleName, IModuleVersionResolver versionResolver)
    {
        foreach (var step in _stepsByModule[moduleName])
        {
            foreach (var dependency in GetDependencies(step))
            {
                var currentVersion = versionResolver.GetCurrentVersion(dependency.ModuleName);
                if (currentVersion > dependency.Version)
                {
                    throw new InvalidOperationException($"Migration step [{step.Script.DisplayName}] requires module [{dependency.ModuleName}] to be version [{dependency.Version}], but current is [{currentVersion}].");
                }

                if (currentVersion != dependency.Version)
                {
                    var contains = _stepsByModule.ContainsKey(dependency.ModuleName)
                                   && _stepsByModule[dependency.ModuleName].Any(i => i.To == dependency.Version);

                    if (!contains)
                    {
                        throw new InvalidOperationException($"Migration step [{step.Script.DisplayName}] depends on module [{dependency.ModuleName}] version [{dependency.Version}], but upgrade for this module not found.");
                    }
                }
            }
        }
    }

    public ScriptStep GetNextStep(string moduleName) => _stepsByModule[moduleName][0];

    public void RemoveNextStep(string moduleName)
    {
        var steps = _stepsByModule[moduleName];
        steps.RemoveAt(0);

        if (steps.Count == 0)
        {
            _stepsByModule.Remove(moduleName);
        }
    }

    public bool TestStep(IDictionary<string, Version> versionByModule, string stepModule, ScriptStep step)
    {
        // can be step executed now
        foreach (var dependency in GetDependencies(step))
        {
            // skip external module => already tested in ValidateModuleDependencies
            if (versionByModule.TryGetValue(dependency.ModuleName, out var version))
            {
                if (dependency.Version != version)
                {
                    return false;
                }
            }
        }

        var objections = _stepsByModule
            .Values
            .SelectMany(i => i)
            .Where(i => i.Script != step.Script) // skip this
            .SelectMany(GetDependencies)
            .Where(i => string.Equals(i.ModuleName, stepModule, StringComparison.OrdinalIgnoreCase))
            .Any(i => i.Version < step.To);

        return !objections;
    }

    internal static bool TryParseFileName(
        string name,
        [NotNullWhen(true)] out string? moduleName,
        [NotNullWhen(true)] out Version? from,
        [NotNullWhen(true)] out Version? to)
    {
        moduleName = null;
        from = null;
        to = null;

        var index = name.LastIndexOf(".", StringComparison.OrdinalIgnoreCase);
        if (index > 0)
        {
            name = name.Substring(0, index);
        }

        index = name.LastIndexOf("_", StringComparison.InvariantCultureIgnoreCase);
        if (index <= 0 || index == name.Length)
        {
            return false;
        }

        var versionTo = name.Substring(index + 1);
        var versionFrom = name = name.Substring(0, index);

        index = name.LastIndexOf("_", StringComparison.InvariantCultureIgnoreCase);
        if (index == 0)
        {
            return false;
        }

        if (index > 0)
        {
            versionFrom = name.Substring(index + 1);
            moduleName = name.Substring(0, index);
        }
        else
        {
            moduleName = string.Empty;
        }

        from = ParseVersion(versionFrom);
        to = ParseVersion(versionTo);
        return from != null && to != null && from < to;
    }

    private static Version? ParseVersion(string value)
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

    private void LoadFrom(IEnumerable<IFileSystemInfo> sources, IScriptFactory scriptFactory, int depth, string? rootFolderName)
    {
        foreach (var source in sources)
        {
            if (source is IFolder folder)
            {
                if (FolderAsModuleName && depth == 1)
                {
                    rootFolderName = folder.Name;
                }

                var children = folder.GetFolders().Cast<IFileSystemInfo>().Concat(folder.GetFiles());
                LoadFrom(children, scriptFactory, depth + 1, rootFolderName);
            }
            else
            {
                var file = (IFile)source;
                if (scriptFactory.IsSupported(file) && TryParseFileName(file.Name, out var moduleName, out var from, out var to))
                {
                    if (FolderAsModuleName && string.IsNullOrEmpty(rootFolderName))
                    {
                        throw new InvalidOperationException($"File [{file.Name}] is not expected in the root folder.");
                    }

                    if (FolderAsModuleName && !string.IsNullOrEmpty(moduleName) && !moduleName.Equals(rootFolderName, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException($"File [{file.Name}] with module name [{moduleName}] is not expected in the folder [{rootFolderName}].");
                    }

                    if (FolderAsModuleName)
                    {
                        moduleName = rootFolderName;
                    }

                    if (!_stepsByModule.TryGetValue(moduleName!, out var steps))
                    {
                        steps = new List<ScriptStep>();
                        _stepsByModule.Add(moduleName!, steps);
                    }

                    var script = scriptFactory.FromFile(file);
                    steps.Add(new ScriptStep(moduleName!, from, to, script));
                }
            }
        }
    }

    private IList<ScriptDependency> GetDependencies(ScriptStep step) => _dependencyByStep[step.Script];
}