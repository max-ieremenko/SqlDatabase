using System.Data;
using System.Reflection;
using Moq;
using NUnit.Framework;

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
        Assert.IsTrue(_sut.IsMatch(method!));
    }

    [Test]
    public void CreateDelegate()
    {
        var method = GetType().GetMethod(nameof(Execute), BindingFlags.Instance | BindingFlags.NonPublic);
        var actual = _sut.CreateDelegate(this, method!);
        Assert.IsNotNull(actual);

        var command = new Mock<IDbCommand>(MockBehavior.Strict);
        actual(command.Object, null!);
        Assert.AreEqual(_executeCommand, command.Object);
    }

    private void Execute(IDbCommand command)
    {
        Assert.IsNull(_executeCommand);
        _executeCommand = command;
    }
}