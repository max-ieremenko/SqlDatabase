using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using Moq;
using NUnit.Framework;

namespace SqlDatabase.Scripts.AssemblyInternal;

[TestFixture]
public class ExecuteMethodResolverSqlConnectionTest
{
    private ExecuteMethodResolverSqlConnection _sut = null!;
    private SqlConnection? _executeConnection;

    [SetUp]
    public void BeforeEachTest()
    {
        _sut = new ExecuteMethodResolverSqlConnection();
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

        var connection = new SqlConnection();

        var command = new Mock<IDbCommand>(MockBehavior.Strict);
        command
            .SetupGet(c => c.Connection)
            .Returns(connection);

        actual(command.Object, null!);
        Assert.AreEqual(_executeConnection, connection);
    }

    private void Execute(SqlConnection connection)
    {
        Assert.IsNull(_executeConnection);
        _executeConnection = connection;
    }
}