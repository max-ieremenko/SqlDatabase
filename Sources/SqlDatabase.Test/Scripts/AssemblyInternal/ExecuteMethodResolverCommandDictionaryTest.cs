using System.Collections.Generic;
using System.Data;
using System.Reflection;
using Moq;
using NUnit.Framework;

namespace SqlDatabase.Scripts.AssemblyInternal;

[TestFixture]
public class ExecuteMethodResolverCommandDictionaryTest
{
    private ExecuteMethodResolverCommandDictionary _sut = null!;
    private IDbCommand? _executeCommand;
    private IReadOnlyDictionary<string, string?>? _executeVariables;

    [SetUp]
    public void BeforeEachTest()
    {
        _sut = new ExecuteMethodResolverCommandDictionary();
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

    private void Execute(IDbCommand command, IReadOnlyDictionary<string, string?> variables)
    {
        Assert.IsNull(_executeCommand);
        _executeCommand = command;
        _executeVariables = variables;
    }
}