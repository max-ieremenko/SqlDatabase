using System;
using NUnit.Framework;

namespace SqlDatabase.Scripts;

[TestFixture]
public class VariablesTest
{
    private Variables _sut;

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

        Assert.IsNotNull(_sut.GetValue(Key));
        Assert.AreEqual(Environment.GetEnvironmentVariable(Key), _sut.GetValue(Key));

        _sut.SetValue(source, Key, "new value");

        var expected = isOverridden ? "new value" : Environment.GetEnvironmentVariable(Key);
        Assert.AreEqual(expected, _sut.GetValue(Key));
    }

    [Test]
    public void NullValue()
    {
        const string Key = "some name";

        Assert.IsNull(_sut.GetValue(Key));

        _sut.SetValue(VariableSource.CommandLine, Key, "1");
        Assert.AreEqual("1", _sut.GetValue(Key));

        _sut.SetValue(VariableSource.CommandLine, Key, string.Empty);
        Assert.AreEqual(string.Empty, _sut.GetValue(Key));

        _sut.SetValue(VariableSource.CommandLine, Key, null);
        Assert.IsNull(_sut.GetValue(Key));
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
        Assert.IsNull(_sut.GetValue(Key));

        // competitor, expected
        _sut.SetValue(competitor, Key, CompetitorValue);
        _sut.SetValue(expected, Key, ExpectedValue);

        Assert.AreEqual(ExpectedValue, _sut.GetValue(Key));

        // try to remove by competitor
        _sut.SetValue(competitor, Key, null);

        Assert.AreEqual(ExpectedValue, _sut.GetValue(Key));

        // try to remove by expected
        _sut.SetValue(expected, Key, null);

        Assert.IsNull(_sut.GetValue(Key));

        // expected, competitor
        _sut.SetValue(expected, Key, ExpectedValue);
        _sut.SetValue(competitor, Key, CompetitorValue);

        Assert.AreEqual(ExpectedValue, _sut.GetValue(Key));
    }
}