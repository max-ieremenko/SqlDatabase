using System.Reflection;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace SqlDatabase.Adapter.AssemblyScripts;

[TestFixture]
public class ExecuteMethodResolverCommandTest
{
    private ExecuteMethodResolverCommand _sut = null!;
    private IDbCommand? _executeCommand;

    [SetUp]
    public void BeforeEachTest()
    {
        _sut = new ExecuteMethodResolverCommand();
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
        actual(command.Object, null!);
        command.Object.ShouldBe(_executeCommand);
    }

    private void Execute(IDbCommand command)
    {
        _executeCommand.ShouldBeNull();
        _executeCommand = command;
    }
}