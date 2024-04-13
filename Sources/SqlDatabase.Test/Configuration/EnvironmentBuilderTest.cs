using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Adapter;
using SqlDatabase.FileSystem;
using SqlDatabase.Scripts;

namespace SqlDatabase.Configuration;

[TestFixture]
public class EnvironmentBuilderTest
{
    private Mock<ILogger> _log = null!;
    private AppConfiguration _configuration = null!;
    private EnvironmentBuilder _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _log = new Mock<ILogger>(MockBehavior.Strict);
        _configuration = new AppConfiguration();

        _sut = new EnvironmentBuilder(HostedRuntimeResolver.GetRuntime(false), new Mock<IFileSystemFactory>(MockBehavior.Strict).Object);

        _sut
            .WithConfiguration(_configuration)
            .WithLogger(_log.Object);
    }

    [Test]
    public void CreateDatabase()
    {
        _sut
            .WithDataBase("Data Source=.;Initial Catalog=SqlDatabaseTest", TransactionMode.PerStep, true)
            .WithVariables(new Dictionary<string, string>());

        var actual = _sut.BuildDatabase().ShouldBeOfType<Database>();

        actual.Log.ShouldBe(_log.Object);
        actual.Adapter.ShouldNotBeNull();
        actual.Transaction.ShouldBe(TransactionMode.PerStep);
        actual.WhatIf.ShouldBeTrue();
    }

    [Test]
    public void CreateDatabaseApplyVariables()
    {
        var variables = new Dictionary<string, string>
        {
            { "a", "1" },
            { "b", "2" }
        };

        _sut
            .WithDataBase("Data Source=.;Initial Catalog=SqlDatabaseTest", TransactionMode.None, false)
            .WithVariables(variables);

        _configuration.Variables.Add("b", "2.2");
        _configuration.Variables.Add("c", "3");

        var actual = _sut.BuildDatabase().ShouldBeOfType<Database>();

        actual.Variables.GetValue("a").ShouldBe("1");
        actual.Variables.GetValue("b").ShouldBe("2");
        actual.Variables.GetValue("c").ShouldBe("3");
    }

    [Test]
    public void CreateDatabaseValidateVariables()
    {
        var variables = new Dictionary<string, string>
        {
            { "a b", "1" }
        };

        _sut
            .WithDataBase("Data Source=.;Initial Catalog=SqlDatabaseTest", TransactionMode.None, false)
            .WithVariables(variables);

        _configuration.Variables.Add("c d", "1");

        var ex = Assert.Throws<InvalidOperationException>(() => _sut.BuildDatabase());

        ex!.Message.ShouldContain("a b");
        ex.Message.ShouldContain("c d");
    }
}