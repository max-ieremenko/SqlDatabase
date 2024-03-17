using NUnit.Framework;
using Shouldly;

namespace SqlDatabase.Sequence;

[TestFixture]
public class UpgradeScriptCollectionTest
{
    [Test]
    [TestCase("1.0_1.1.sql", "", "1.0", "1.1")]
    [TestCase("module_1.0_1.1.sql", "module", "1.0", "1.1")]
    [TestCase("1.0.1.2_1.2.1.sql", "", "1.0.1.2", "1.2.1")]
    [TestCase("module_1.0.1.2_1.2.1.sql", "module", "1.0.1.2", "1.2.1")]
    [TestCase("1_2.sql", "", "1.0", "2.0")]
    [TestCase("module_1_2.sql", "module", "1.0", "2.0")]
    [TestCase("1.0.sql", null, null, null)]
    [TestCase("xxx.sql", null, null, null)]
    [TestCase("xxx_1.0.sql", null, null, null)]
    [TestCase("1.0_xxx.sql", null, null, null)]
    [TestCase("2.0_1.0.sql", null, null, null)]
    [TestCase("_1.0_1.1.sql", null, null, null)]
    public void TryParseFileName(string name, string? moduleName, string? from, string? to)
    {
        var actual = UpgradeScriptCollection.TryParseFileName(name, out var actualModuleName, out var actualFrom, out var actualTo);

        if (from == null)
        {
            actual.ShouldBeFalse();
        }
        else
        {
            actual.ShouldBeTrue();
            actualModuleName.ShouldBe(moduleName);
            actualFrom.ShouldBe(new Version(from));
            actualTo.ShouldBe(new Version(to!));
        }
    }
}