using System;
using NUnit.Framework;
using Shouldly;

namespace SqlDatabase.Scripts;

[TestFixture]
public class VariablesTest
{
    private Variables _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _sut = new Variables();
    }

    [Test]
    [TestCase(VariableSource.Runtime, true)]
    [TestCase(VariableSource.CommandLine, true)]
    [TestCase(VariableSource.Environment, true)]
    [TestCase(VariableSource.ConfigurationFile, false)]
    public void GetValueEnvironmentVariable(VariableSource source, bool isOverridden)
    {
        const string Key = "TEMP";

        _sut.GetValue(Key).ShouldNotBeNull();
        _sut.GetValue(Key).ShouldBe(Environment.GetEnvironmentVariable(Key));

        _sut.SetValue(source, Key, "new value");

        var expected = isOverridden ? "new value" : Environment.GetEnvironmentVariable(Key);
        _sut.GetValue(Key).ShouldBe(expected);
    }

    [Test]
    public void NullValue()
    {
        const string Key = "some name";

        _sut.GetValue(Key).ShouldBeNull();

        _sut.SetValue(VariableSource.CommandLine, Key, "1");
        _sut.GetValue(Key).ShouldBe("1");

        _sut.SetValue(VariableSource.CommandLine, Key, string.Empty);
        _sut.GetValue(Key).ShouldBe(string.Empty);

        _sut.SetValue(VariableSource.CommandLine, Key, null);
        _sut.GetValue(Key).ShouldBeNull();
    }

    [Test]
    [TestCase(VariableSource.Runtime, VariableSource.ConfigurationFile)]
    [TestCase(VariableSource.Runtime, VariableSource.CommandLine)]
    [TestCase(VariableSource.Runtime, VariableSource.Environment)]
    [TestCase(VariableSource.CommandLine, VariableSource.ConfigurationFile)]
    [TestCase(VariableSource.CommandLine, VariableSource.Environment)]
    [TestCase(VariableSource.Environment, VariableSource.ConfigurationFile)]
    public void ResolvePriority(VariableSource expected, VariableSource competitor)
    {
        const string Key = "{839BA97E-9271-4D25-9453-434E269F8BDB}";
        const string ExpectedValue = "expected";
        const string CompetitorValue = "competitor";

        // default
        _sut.GetValue(Key).ShouldBeNull();

        // competitor, expected
        _sut.SetValue(competitor, Key, CompetitorValue);
        _sut.SetValue(expected, Key, ExpectedValue);

        _sut.GetValue(Key).ShouldBe(ExpectedValue);

        // try to remove by competitor
        _sut.SetValue(competitor, Key, null);

        _sut.GetValue(Key).ShouldBe(ExpectedValue);

        // try to remove by expected
        _sut.SetValue(expected, Key, null);

        _sut.GetValue(Key).ShouldBeNull();

        // expected, competitor
        _sut.SetValue(expected, Key, ExpectedValue);
        _sut.SetValue(competitor, Key, CompetitorValue);

        _sut.GetValue(Key).ShouldBe(ExpectedValue);
    }
}