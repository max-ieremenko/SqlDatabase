using System.Reflection;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace SqlDatabase.Adapter.AssemblyScripts;

[TestFixture]
public class ExecuteMethodResolverDbConnectionTest
{
    private ExecuteMethodResolverDbConnection _sut = null!;
    private IDbConnection? _executeConnection;

    [SetUp]
    public void BeforeEachTest()
    {
        _sut = new ExecuteMethodResolverDbConnection();
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

        var connection = new Mock<IDbConnection>(MockBehavior.Strict);

        var command = new Mock<IDbCommand>(MockBehavior.Strict);
        command
            .SetupGet(c => c.Connection)
            .Returns(connection.Object);

        actual(command.Object, null!);
        connection.Object.ShouldBe(_executeConnection);
    }

    private void Execute(IDbConnection connection)
    {
        _executeConnection.ShouldBeNull();
        _executeConnection = connection;
    }
}