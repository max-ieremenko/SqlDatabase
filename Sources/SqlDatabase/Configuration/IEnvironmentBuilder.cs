using SqlDatabase.Adapter;
using SqlDatabase.Scripts;

namespace SqlDatabase.Configuration;

internal interface IEnvironmentBuilder
{
    IEnvironmentBuilder WithConfiguration(string? configurationFile);

    IEnvironmentBuilder WithLogger(ILogger logger);

    IEnvironmentBuilder WithPowerShellScripts(string? installationPath);

    IEnvironmentBuilder WithAssemblyScripts();

    IEnvironmentBuilder WithVariables(IDictionary<string, string> variables);

    IEnvironmentBuilder WithDataBase(string connectionString, TransactionMode transaction, bool whatIf);

    IDatabase BuildDatabase();

    IScriptResolver BuildScriptResolver();

    IScriptFactory BuildScriptFactory();
}