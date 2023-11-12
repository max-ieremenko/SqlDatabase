using NUnit.Framework;
using Shouldly;

namespace SqlDatabase.Adapter.MsSql;

[TestFixture]
public class MsSqlDatabaseAdapterFactoryTest
{
    [Test]
    public void CanBe()
    {
        MsSqlDatabaseAdapterFactory.CanBe(MsSqlQuery.GetConnectionString()).ShouldBeTrue();
    }
}