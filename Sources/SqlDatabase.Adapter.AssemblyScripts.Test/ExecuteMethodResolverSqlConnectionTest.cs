using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace SqlDatabase.Adapter.AssemblyScripts;

[TestFixture]
public class ExecuteMethodResolverSqlConnectionTest
{
    private ExecuteMethodResolverSqlConnection _sut = null!;
    private SqlConnection? _executeConnection;
    private MethodInfo _execute = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _execute = GetType()
            .GetMethod(nameof(Execute), BindingFlags.Instance | BindingFlags.NonPublic)
            .ShouldNotBeNull();

        _sut = new ExecuteMethodResolverSqlConnection();
    }

    [Test]
    public void IsMatch()
    {
        _sut.IsMatch(_execute).ShouldBeTrue();
    }

    [Test]
    public void CreateDelegate()
    {
        var actual = _sut.CreateDelegate(this, _execute);
        actual.ShouldNotBeNull();

        var connection = new SqlConnection();

        var command = new Mock<IDbCommand>(MockBehavior.Strict);
        command
            .SetupGet(c => c.Connection)
            .Returns(connection);

        actual(command.Object, null!);
        _executeConnection.ShouldBe(connection);
    }

    private void Execute(SqlConnection connection)
    {
        _executeConnection.ShouldBeNull();
        _executeConnection = connection;
    }
}