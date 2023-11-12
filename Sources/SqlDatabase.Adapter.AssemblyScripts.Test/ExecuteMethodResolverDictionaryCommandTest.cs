using System.Collections.Generic;
using System.Data;
using System.Reflection;
using Moq;
using NUnit.Framework;

namespace SqlDatabase.Adapter.AssemblyScripts;

[TestFixture]
public class ExecuteMethodResolverDictionaryCommandTest
{
    private ExecuteMethodResolverDictionaryCommand _sut = null!;
    private IDbCommand? _executeCommand;
    private IReadOnlyDictionary<string, string?>? _executeVariables;

    [SetUp]
    public void BeforeEachTest()
    {
        _sut = new ExecuteMethodResolverDictionaryCommand();
    }

    [Test]
    public void IsMatch()
    {
        var method = GetType().GetMethod(nameof(Execute), BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsTrue(_sut.IsMatch(method!));
    }

    [Test]
    public void CreateDelegate()
    {
        var method = GetType().GetMethod(nameof(Execute), BindingFlags.Instance | BindingFlags.NonPublic);
        var actual = _sut.CreateDelegate(this, method!);
        Assert.IsNotNull(actual);

        var command = new Mock<IDbCommand>(MockBehavior.Strict);
        var variables = new Mock<IReadOnlyDictionary<string, string?>>(MockBehavior.Strict);

        actual(command.Object, variables.Object);

        Assert.AreEqual(_executeCommand, command.Object);
        Assert.AreEqual(_executeVariables, variables.Object);
    }

    private void Execute(IReadOnlyDictionary<string, string?> variables, IDbCommand command)
    {
        Assert.IsNull(_executeCommand);
        _executeCommand = command;
        _executeVariables = variables;
    }
}