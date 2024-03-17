using Moq;
using Shouldly;
using SqlDatabase.Adapter;
using SqlDatabase.FileSystem;
using SqlDatabase.Scripts;
using SqlDatabase.Sequence;

namespace SqlDatabase.Configuration;

internal sealed class EnvironmentBuilderMock
{
    private readonly Mock<IEnvironmentBuilder> _mock = new(MockBehavior.Strict);
    private readonly List<string> _mockSequence = new();

    public IDatabase Database { get; } = new Mock<IDatabase>(MockBehavior.Strict).Object;

    public IScriptResolver ScriptResolver { get; } = new Mock<IScriptResolver>(MockBehavior.Strict).Object;

    public IUpgradeScriptSequence UpgradeSequence { get; } = new Mock<IUpgradeScriptSequence>(MockBehavior.Strict).Object;

    public ICreateScriptSequence CreateSequence { get; } = new Mock<ICreateScriptSequence>(MockBehavior.Strict).Object;

    public EnvironmentBuilderMock WithLogger(ILogger logger)
    {
        _mock.Setup(b => b.WithLogger(logger)).Returns(_mock.Object);
        _mockSequence.Add(nameof(IEnvironmentBuilder.WithLogger));
        return this;
    }

    public EnvironmentBuilderMock WithConfiguration(string? configurationFile)
    {
        _mock.Setup(b => b.WithConfiguration(configurationFile)).Returns(_mock.Object);
        _mockSequence.Add(nameof(IEnvironmentBuilder.WithConfiguration));
        return this;
    }

    public EnvironmentBuilderMock WithPowerShellScripts(string? installationPath)
    {
        _mock.Setup(b => b.WithPowerShellScripts(installationPath)).Returns(_mock.Object);
        _mockSequence.Add(nameof(IEnvironmentBuilder.WithPowerShellScripts));
        return this;
    }

    public EnvironmentBuilderMock WithAssemblyScripts()
    {
        _mock.Setup(b => b.WithAssemblyScripts()).Returns(_mock.Object);
        _mockSequence.Add(nameof(IEnvironmentBuilder.WithAssemblyScripts));
        return this;
    }

    public EnvironmentBuilderMock WithVariables(IDictionary<string, string> variables)
    {
        _mock.Setup(b => b.WithVariables(variables)).Returns(_mock.Object);
        _mockSequence.Add(nameof(IEnvironmentBuilder.WithVariables));
        return this;
    }

    public EnvironmentBuilderMock WithDataBase(string connectionString, TransactionMode transaction, bool whatIf)
    {
        _mock.Setup(b => b.WithDataBase(connectionString, transaction, whatIf)).Returns(_mock.Object);
        _mockSequence.Add(nameof(IEnvironmentBuilder.WithDataBase));
        return this;
    }

    public EnvironmentBuilderMock WithUpgradeSequence(IList<IFileSystemInfo> scripts, bool folderAsModuleName)
    {
        _mock.Setup(b => b.BuildUpgradeSequence(scripts, folderAsModuleName)).Returns(UpgradeSequence);
        _mockSequence.Add(nameof(IEnvironmentBuilder.BuildUpgradeSequence));
        return this;
    }

    public EnvironmentBuilderMock WithCreateSequence(IList<IFileSystemInfo> scripts)
    {
        _mock.Setup(b => b.BuildCreateSequence(scripts)).Returns(CreateSequence);
        _mockSequence.Add(nameof(IEnvironmentBuilder.BuildCreateSequence));
        return this;
    }

    public IEnvironmentBuilder Build()
    {
        _mock.Setup(b => b.BuildDatabase()).Returns(Database);
        _mock.Setup(b => b.BuildScriptResolver()).Returns(ScriptResolver);

        return _mock.Object;
    }

    public void VerifyAll()
    {
        _mock.VerifyAll();

        var sequence = _mock
            .Invocations
            .Select(i => i.Method.Name)
            .Where(i => i != nameof(IEnvironmentBuilder.BuildDatabase) && i != nameof(IEnvironmentBuilder.BuildScriptResolver))
            .ToArray();
        sequence.ShouldBe(_mockSequence);
    }
}