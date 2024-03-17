using SqlDatabase.Adapter;
using SqlDatabase.FileSystem;
using SqlDatabase.Scripts;
using SqlDatabase.Sequence;

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

    IUpgradeScriptSequence BuildUpgradeSequence(IList<IFileSystemInfo> scripts, bool folderAsModuleName);

    ICreateScriptSequence BuildCreateSequence(IList<IFileSystemInfo> scripts);
}