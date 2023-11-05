using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using SqlDatabase.IO;

namespace SqlDatabase.Scripts.UpgradeInternal;

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
                throw new InvalidOperationException("The current version [{0}] is greater then latest upgrade [{1}].".FormatWith(moduleVersion, maxVersion));
            }

            throw new InvalidOperationException("Module [{0}], the current version [{1}] is greater then latest upgrade [{2}].".FormatWith(moduleName, moduleVersion, maxVersion));
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
                    throw new InvalidOperationException("Duplicated step found [{0}] and [{1}].".FormatWith(file.Script.DisplayName, files[0].Script.DisplayName));
                }

                throw new InvalidOperationException("Module [{0}], duplicated step found [{1}] and [{2}].".FormatWith(moduleName, file.Script.DisplayName, files[0].Script.DisplayName));
            }

            sequence.Add(file);
            version = file.To;
        }

        if (version != maxVersion)
        {
            if (string.IsNullOrEmpty(moduleName))
            {
                throw new InvalidOperationException("Upgrade step from [{0}] to a next not found.".FormatWith(version));
            }

            throw new InvalidOperationException("Module [{0}], upgrade step from [{1}] to a next not found.".FormatWith(moduleName, version));
        }

        _stepsByModule[moduleName] = sequence;
    }

    public void LoadDependencies()
    {
        foreach (var steps in _stepsByModule.Values)
        {
            foreach (var step in steps)
            {
                var dependencies = step.Script.GetDependencies();
                _dependencyByStep.Add(step.Script, dependencies);
            }
        }
    }

    public void ShowWithDependencies(ILogger logger)
    {
        foreach (var moduleName in _stepsByModule.Keys.OrderBy(i => i))
        {
            var steps = _stepsByModule[moduleName];
            logger.Info("module [{0}], {1} step{2}:".FormatWith(
                moduleName,
                steps.Count,
                steps.Count == 1 ? null : "s"));

            using (logger.Indent())
            {
                foreach (var step in steps)
                {
                    var dependencies = GetDependencies(step);
                    if (dependencies.Count == 0)
                    {
                        logger.Info("{0}, no dependencies".FormatWith(step.Script.DisplayName));
                    }
                    else
                    {
                        logger.Info("{0}, depends on {1}".FormatWith(
                            step.Script.DisplayName,
                            string.Join("; ", dependencies.OrderBy(i => i.ModuleName))));
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
                    throw new InvalidOperationException("Migration step [{0}] requires module [{1}] to be version [{2}], but current is [{3}].".FormatWith(
                        step.Script.DisplayName,
                        dependency.ModuleName,
                        dependency.Version,
                        currentVersion));
                }

                if (currentVersion != dependency.Version)
                {
                    var contains = _stepsByModule.ContainsKey(dependency.ModuleName)
                                   && _stepsByModule[dependency.ModuleName].Any(i => i.To == dependency.Version);

                    if (!contains)
                    {
                        throw new InvalidOperationException("Migration step [{0}] depends on module [{1}] version [{2}], but upgrade for this module not found.".FormatWith(
                            step.Script.DisplayName,
                            dependency.ModuleName,
                            dependency.Version));
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
                if (scriptFactory.IsSupported(file.Name) && TryParseFileName(file.Name, out var moduleName, out var from, out var to))
                {
                    if (FolderAsModuleName && string.IsNullOrEmpty(rootFolderName))
                    {
                        throw new InvalidOperationException("File [{0}] is not expected in the root folder.".FormatWith(file.Name));
                    }

                    if (FolderAsModuleName && !string.IsNullOrEmpty(moduleName) && !moduleName.Equals(rootFolderName, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException("File [{0}] with module name [{1}] is not expected in the folder [{2}].".FormatWith(file.Name, moduleName, rootFolderName));
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