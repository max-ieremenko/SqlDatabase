using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace SqlDatabase.Scripts.AssemblyInternal;

[TestFixture]
public partial class EntryPointResolverTest
{
    private EntryPointResolver _sut = null!;
    private IList<string> _logErrorOutput = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _logErrorOutput = new List<string>();

        var log = new Mock<ILogger>(MockBehavior.Strict);
        log
            .Setup(l => l.Info(It.IsAny<string>()))
            .Callback<string>(m =>
            {
                Console.WriteLine("Info: {0}", m);
            });
        log
            .Setup(l => l.Error(It.IsAny<string>()))
            .Callback<string>(m =>
            {
                Console.WriteLine("Error: {0}", m);
                _logErrorOutput.Add(m);
            });

        _sut = new EntryPointResolver(log.Object, null!, null!);
    }

    [Test]
    [TestCase(nameof(ExampleSqlDatabaseScript))]
    [TestCase(nameof(EntryPointResolverTest) + "+" + nameof(ExampleSqlDatabaseScript))]
    [TestCase("AssemblyInternal." + nameof(EntryPointResolverTest) + "+" + nameof(ExampleSqlDatabaseScript))]
    [TestCase("Scripts.AssemblyInternal." + nameof(EntryPointResolverTest) + "+" + nameof(ExampleSqlDatabaseScript))]
    [TestCase("SqlDatabase.Scripts.AssemblyInternal." + nameof(EntryPointResolverTest) + "+" + nameof(ExampleSqlDatabaseScript))]
    public void ResolveFromExample(string className)
    {
        _sut.ExecutorClassName = className;
        _sut.ExecutorMethodName = nameof(ExampleSqlDatabaseScript.Execute);

        var actual = _sut.Resolve(GetType().Assembly);
        CollectionAssert.IsEmpty(_logErrorOutput);
        Assert.IsInstanceOf<DefaultEntryPoint>(actual);

        var entryPoint = (DefaultEntryPoint)actual!;
        entryPoint.Log.ShouldNotBeNull();
        entryPoint.ScriptInstance.ShouldBeOfType<ExampleSqlDatabaseScript>();
        entryPoint.Method.Method.Name.ShouldBe(nameof(ExampleSqlDatabaseScript.Execute));
    }

    [Test]
    public void FailToCreateInstance()
    {
        _sut.ExecutorClassName = nameof(DatabaseScriptWithInvalidConstructor);
        _sut.ExecutorMethodName = nameof(DatabaseScriptWithInvalidConstructor.Execute);

        Assert.IsNull(_sut.Resolve(GetType().Assembly));
        CollectionAssert.IsNotEmpty(_logErrorOutput);
    }

    [Test]
    public void ResolveExecuteWithCommandOnly()
    {
        _sut.ExecutorClassName = nameof(DatabaseScriptWithOneParameter);
        _sut.ExecutorMethodName = nameof(DatabaseScriptWithOneParameter.ExecuteCommand);

        var actual = _sut.Resolve(GetType().Assembly);
        CollectionAssert.IsEmpty(_logErrorOutput);
        Assert.IsInstanceOf<DefaultEntryPoint>(actual);

        var entryPoint = (DefaultEntryPoint)actual!;
        Assert.IsNotNull(entryPoint.Log);
        Assert.IsInstanceOf<DatabaseScriptWithOneParameter>(entryPoint.ScriptInstance);
        Assert.IsNotNull(entryPoint.Method);
    }

    [Test]
    public void ResolveExecuteWithConnection()
    {
        _sut.ExecutorClassName = nameof(DatabaseScriptWithConnection);
        _sut.ExecutorMethodName = nameof(DatabaseScriptWithConnection.Run);

        var actual = _sut.Resolve(GetType().Assembly);
        CollectionAssert.IsEmpty(_logErrorOutput);
        Assert.IsInstanceOf<DefaultEntryPoint>(actual);

        var entryPoint = (DefaultEntryPoint)actual!;
        Assert.IsNotNull(entryPoint.Log);
        Assert.IsInstanceOf<DatabaseScriptWithConnection>(entryPoint.ScriptInstance);
        Assert.IsNotNull(entryPoint.Method);
    }
}