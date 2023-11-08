using System;
using System.Collections.Generic;
using System.Linq;
using SqlDatabase.Adapter;
using SqlDatabase.Adapter.AssemblyScripts;
using SqlDatabase.Adapter.PowerShellScripts;
using SqlDatabase.Adapter.Sql;
using SqlDatabase.FileSystem;
using SqlDatabase.Scripts;
using SqlDatabase.Sequence;

namespace SqlDatabase.Configuration;

internal sealed class EnvironmentBuilder : IEnvironmentBuilder
{
    private ILogger? _logger;
    private AppConfiguration? _configuration;
    private string? _connectionString;
    private TransactionMode _transactionMode;
    private bool _whatIf;
    private IDictionary<string, string>? _variables;
    private PowerShellScriptFactory? _powerShellScript;
    private AssemblyScriptFactory? _assemblyScript;
    private ScriptResolver? _scriptResolver;
    private IDatabase? _database;

    public IEnvironmentBuilder WithConfiguration(string? configurationFile)
    {
        var configuration = new ConfigurationManager();
        configuration.LoadFrom(configurationFile);

        return WithConfiguration(configuration.SqlDatabase);
    }

    public IEnvironmentBuilder WithLogger(ILogger logger)
    {
        _logger = logger;

        return this;
    }

    public IEnvironmentBuilder WithPowerShellScripts(string? installationPath)
    {
        _powerShellScript = new PowerShellScriptFactory(installationPath);
        return this;
    }

    public IEnvironmentBuilder WithAssemblyScripts()
    {
        var configuration = GetConfiguration().AssemblyScript;
        _assemblyScript = new AssemblyScriptFactory(configuration.ClassName, configuration.ClassName);
        return this;
    }

    public IEnvironmentBuilder WithVariables(IDictionary<string, string> variables)
    {
        _variables = variables;
        return this;
    }

    public IEnvironmentBuilder WithDataBase(string connectionString, TransactionMode transaction, bool whatIf)
    {
        _connectionString = connectionString;
        _transactionMode = transaction;
        _whatIf = whatIf;
        return this;
    }

    public IDatabase BuildDatabase()
    {
        if (_database == null)
        {
            _database = CreateDatabase();
        }

        return _database;
    }

    public IScriptResolver BuildScriptResolver()
    {
        if (_scriptResolver == null)
        {
            _scriptResolver = CreateScriptResolver();
        }

        return _scriptResolver;
    }

    public IUpgradeScriptSequence BuildUpgradeSequence(IList<IFileSystemInfo> scripts, bool folderAsModuleName)
    {
        var scriptResolver = (ScriptResolver)BuildScriptResolver();
        var database = BuildDatabase();

        return new UpgradeScriptSequence(
            scriptResolver,
            database.GetCurrentVersion,
            scripts.ToArray(),
            GetLogger(),
            folderAsModuleName,
            _whatIf);
    }

    public ICreateScriptSequence BuildCreateSequence(IList<IFileSystemInfo> scripts)
    {
        var scriptResolver = (ScriptResolver)BuildScriptResolver();
        return new CreateScriptSequence(scripts.ToArray(), scriptResolver);
    }

    internal IEnvironmentBuilder WithConfiguration(AppConfiguration configuration)
    {
        _configuration = configuration;
        return this;
    }

    private Database CreateDatabase()
    {
        if (_connectionString == null || _variables == null)
        {
            throw new InvalidOperationException();
        }

        IDatabaseAdapter adapter;
        try
        {
            adapter = DatabaseAdapterFactory.CreateAdapter(_connectionString, GetConfiguration(), GetLogger());
        }
        catch (Exception ex)
        {
            throw new InvalidCommandLineException(Arg.Database, "Invalid connection string value.", ex);
        }

        var database = new Database(adapter, GetLogger(), _transactionMode, _whatIf);

        var configurationVariables = GetConfiguration().Variables;
        foreach (var name in configurationVariables.AllKeys)
        {
            database.Variables.SetValue(VariableSource.ConfigurationFile, name, configurationVariables[name].Value);
        }

        foreach (var entry in _variables)
        {
            database.Variables.SetValue(VariableSource.CommandLine, entry.Key, entry.Value);
        }

        var invalidNames = database
            .Variables
            .GetNames()
            .OrderBy(i => i)
            .Where(i => !SqlScriptVariableParser.IsValidVariableName(i))
            .Select(i => $"[{i}]")
            .ToList();

        if (invalidNames.Count == 1)
        {
            throw new InvalidOperationException($"The variable name {invalidNames[0]} is invalid.");
        }

        if (invalidNames.Count > 1)
        {
            throw new InvalidOperationException($"The following variable names are invalid: {string.Join(", ", invalidNames)}.");
        }

        return database;
    }

    private ScriptResolver CreateScriptResolver()
    {
        var factories = new List<IScriptFactory>(3);

        factories.Add(new TextScriptFactory(BuildDatabase().Adapter.CreateSqlTextReader()));

        if (_powerShellScript != null)
        {
            factories.Add(_powerShellScript);
        }

        if (_assemblyScript != null)
        {
            factories.Add(_assemblyScript);
        }

        return new ScriptResolver(factories.ToArray());
    }

    private AppConfiguration GetConfiguration()
    {
        if (_configuration == null)
        {
            throw new InvalidOperationException();
        }

        return _configuration;
    }

    private ILogger GetLogger()
    {
        if (_logger == null)
        {
            throw new InvalidOperationException();
        }

        return _logger;
    }
}