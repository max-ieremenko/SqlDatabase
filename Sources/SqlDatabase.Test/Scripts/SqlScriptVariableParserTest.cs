using System;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace SqlDatabase.Scripts;

[TestFixture]
public class SqlScriptVariableParserTest
{
    private Mock<IVariables> _variables;
    private SqlScriptVariableParser _sut;

    [SetUp]
    public void BeforeEachTest()
    {
        _variables = new Mock<IVariables>(MockBehavior.Strict);

        _sut = new SqlScriptVariableParser(_variables.Object);
    }

    [Test]
    [TestCase("_'{{Value}}'_", "x", "_'x'_")]
    [TestCase("{{Value}}", "x", "x")]
    [TestCase("{{ValuE}}", "x", "x")]
    [TestCase("{{Value}}+{{Value}}", "x", "x+x")]
    [TestCase("{{Value}}\r\n{{Value}}", "x", "x\r\nx")]
    public void ApplyVariables(string sql, string value, string expected)
    {
        _variables
            .Setup(v => v.GetValue(It.IsAny<string>()))
            .Returns<string>(name =>
            {
                if ("value".Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return value;
                }

                return null;
            });

        var actual = _sut.ApplyVariables(sql);

        actual.ShouldBe(expected);
        _sut.ValueByName.Count.ShouldBe(1);
        _sut.ValueByName["value"].ShouldNotBeNull();
    }

    [Test]
    [TestCase("[$(value)]", "some name", "[some name]")]
    [TestCase("$(Value)+$(Value)", "x", "x+x")]
    public void ApplySqlCmdVariables(string sql, string value, string expected)
    {
        _variables
            .Setup(v => v.GetValue(It.IsAny<string>()))
            .Returns<string>(name =>
            {
                if ("value".Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return value;
                }

                return null;
            });

        var actual = _sut.ApplyVariables(sql);

        actual.ShouldBe(expected);
        _sut.ValueByName.Count.ShouldBe(1);
        _sut.ValueByName["value"].ShouldNotBeNull();
    }

    [Test]
    [TestCase("{{Value}}", "value", "123")]
    [TestCase("{{Value1}}", "value1", "123")]
    [TestCase("{{Value_1}}", "value_1", "123")]
    public void ApplyVariablesVariableNames(string sql, string name, string expected)
    {
        _variables
            .Setup(v => v.GetValue(It.IsAny<string>()))
            .Returns<string>(n =>
            {
                if (name.Equals(n, StringComparison.OrdinalIgnoreCase))
                {
                    return "123";
                }

                return null;
            });

        var actual = _sut.ApplyVariables(sql);

        actual.ShouldBe(expected);
        _sut.ValueByName[name].ShouldNotBeNull();
    }

    [Test]
    public void ApplyVariablesFailsOnUnknownVariable()
    {
        _variables
            .Setup(v => v.GetValue(It.IsAny<string>()))
            .Returns((string)null);

        var ex = Assert.Throws<InvalidOperationException>(() => _sut.ApplyVariables("{{Value_1}}"));

        ex.Message.ShouldContain("Value_1");
    }

    [Test]
    [TestCase("var_1", true)]
    [TestCase("Var_9", true)]
    [TestCase("var-1", false)]
    [TestCase("var 1", false)]
    public void IsValidVariableName(string variableName, bool isValid)
    {
        SqlScriptVariableParser.IsValidVariableName(variableName).ShouldBe(isValid);
    }

    [Test]
    [TestCase("var", "value", "value")]
    [TestCase("_var", "value", SqlScriptVariableParser.ValueIsHidden)]
    public void NoLogVariableName(string variableName, string value, string expectedLogValue)
    {
        var sql = "{{" + variableName + "}}";

        _variables
            .Setup(v => v.GetValue(variableName))
            .Returns(value);

        _sut.ApplyVariables(sql);

        _sut.ValueByName[variableName].ShouldBe(expectedLogValue);
    }
}