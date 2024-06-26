﻿using System.Reflection;
using Moq;
using NUnit.Framework;
using Shouldly;

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
        method.ShouldNotBeNull();
        _sut.IsMatch(method).ShouldBeTrue();
    }

    [Test]
    public void CreateDelegate()
    {
        var method = GetType().GetMethod(nameof(Execute), BindingFlags.Instance | BindingFlags.NonPublic);
        method.ShouldNotBeNull();

        var actual = _sut.CreateDelegate(this, method);
        actual.ShouldNotBeNull();

        var command = new Mock<IDbCommand>(MockBehavior.Strict);
        var variables = new Mock<IReadOnlyDictionary<string, string?>>(MockBehavior.Strict);

        actual(command.Object, variables.Object);

        command.Object.ShouldBe(_executeCommand);
        variables.Object.ShouldBe(_executeVariables);
    }

    private void Execute(IReadOnlyDictionary<string, string?> variables, IDbCommand command)
    {
        _executeCommand.ShouldBeNull();
        _executeCommand = command;
        _executeVariables = variables;
    }
}