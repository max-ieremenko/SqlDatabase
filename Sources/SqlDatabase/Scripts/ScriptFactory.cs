using System;
using System.IO;
using System.Linq;
using SqlDatabase.Adapter;
using SqlDatabase.Adapter.AssemblyScripts;
using SqlDatabase.Configuration;
using SqlDatabase.FileSystem;

namespace SqlDatabase.Scripts;

internal sealed class ScriptFactory : IScriptFactory
{
    private readonly IScriptFactory[] _concrete;

    public ScriptFactory(AssemblyScriptConfiguration assemblyScriptConfiguration, IPowerShellFactory? powerShellFactory, ISqlTextReader textReader)
    {
        _concrete = new IScriptFactory[]
        {
            new SqlScriptFactory(textReader),
            new PowerShellScriptFactory(powerShellFactory),
            new AssemblyScriptFactory(assemblyScriptConfiguration.ClassName, assemblyScriptConfiguration.MethodName)
        };
    }

    internal IPowerShellFactory? PowerShellFactory
    {
        get => ((PowerShellScriptFactory)_concrete[1]).PowerShellFactory;
        set => ((PowerShellScriptFactory)_concrete[1]).PowerShellFactory = value;
    }

    public bool IsSupported(IFile file) => FindSupported(file) != null;

    public IScript FromFile(IFile file)
    {
        var factory = FindSupported(file);
        if (factory == null)
        {
            throw new NotSupportedException("File [{0}] cannot be used as script.".FormatWith(file.Name));
        }

        return factory.FromFile(file);
    }

    private static Func<Stream?> CreateScriptDescriptionReader(IFile file)
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

    private IScriptFactory? FindSupported(IFile file)
    {
        for (var i = 0; i < _concrete.Length; i++)
        {
            var result = _concrete[i];
            if (result.IsSupported(file))
            {
                return result;
            }
        }

        return null;
    }

    private sealed class PowerShellScriptFactory : IScriptFactory
    {
        public PowerShellScriptFactory(IPowerShellFactory? powerShellFactory)
        {
            PowerShellFactory = powerShellFactory;
        }

        public IPowerShellFactory? PowerShellFactory { get; set; }

        public bool IsSupported(IFile file)
        {
            return PowerShellFactory != null && ".ps1".Equals(file.Extension, StringComparison.OrdinalIgnoreCase);
        }

        public IScript FromFile(IFile file)
        {
            if (PowerShellFactory == null)
            {
                throw new NotSupportedException(".ps1 scripts are not supported in this context.");
            }

            PowerShellFactory.Request();

            return new PowerShellScript(
                file.Name,
                file.OpenRead,
                CreateScriptDescriptionReader(file),
                PowerShellFactory);
        }
    }

    private sealed class SqlScriptFactory : IScriptFactory
    {
        private readonly ISqlTextReader _textReader;

        public SqlScriptFactory(ISqlTextReader textReader)
        {
            _textReader = textReader;
        }

        public bool IsSupported(IFile file)
        {
            return ".sql".Equals(file.Extension, StringComparison.OrdinalIgnoreCase);
        }

        public IScript FromFile(IFile file)
        {
            return new TextScript(file.Name, file.OpenRead, _textReader);
        }
    }
}