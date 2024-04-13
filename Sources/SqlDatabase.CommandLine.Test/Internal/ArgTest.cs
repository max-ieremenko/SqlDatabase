using NUnit.Framework;
using Shouldly;

namespace SqlDatabase.CommandLine.Internal;

[TestFixture]
public class ArgTest
{
    [Test]
    [TestCase("command", null, null)]
    [TestCase("-", null, null)]
    [TestCase("-=", null, null)]
    [TestCase("-arg", "arg", null)]
    [TestCase("-arg =", "arg", null)]
    [TestCase("-arg = ", "arg", null)]
    [TestCase("-arg= value", "arg", "value")]
    public void TryParse(string value, string? expectedKey, string? expectedValue)
    {
        Arg.TryParse(value, out var actual).ShouldBe(expectedKey != null);

        if (expectedKey != null)
        {
            actual.Key.ShouldBe(expectedKey);
            actual.Value.ShouldBe(expectedValue);
        }
    }
}