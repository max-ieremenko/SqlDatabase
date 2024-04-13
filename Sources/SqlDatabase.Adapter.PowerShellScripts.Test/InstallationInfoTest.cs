using NUnit.Framework;
using Shouldly;

namespace SqlDatabase.Adapter.PowerShellScripts;

[TestFixture]
public class InstallationInfoTest
{
    [Test]
    [TestCaseSource(nameof(GetSortCases))]
    public void Sort(object item1, object item2)
    {
        var info1 = (InstallationInfo)item1;
        var info2 = (InstallationInfo)item2;

        List<InstallationInfo> list = [info1, info2];
        list.Sort();
        list.ShouldBe([info1, info2]);

        list = [info2, info1];
        list.Sort();
        list.ShouldBe([info1, info2]);
    }

    private static IEnumerable<TestCaseData> GetSortCases()
    {
        yield return new TestCaseData(
            new InstallationInfo("path", new Version(1, 0), "1.0"),
            new InstallationInfo("path", new Version(2, 0), "2.0"))
        {
            TestName = "1.0 vs 2.0"
        };

        yield return new TestCaseData(
            new InstallationInfo("path", new Version(1, 0), "1.0-preview"),
            new InstallationInfo("path", new Version(1, 0), "1.0"))
        {
            TestName = "1.0-preview vs 1.0"
        };

        yield return new TestCaseData(
            new InstallationInfo("path", new Version(1, 0), "1.0-preview.1"),
            new InstallationInfo("path", new Version(1, 0), "1.0-preview.1"))
        {
            TestName = "1.0-preview.1 vs 1.0-preview.2"
        };

        yield return new TestCaseData(
            new InstallationInfo("path", new Version(1, 0), "1.0-preview.1"),
            new InstallationInfo("path", new Version(1, 0), "1.0-preview.2"))
        {
            TestName = "1.0-preview.1 vs 1.0-preview.2"
        };

        yield return new TestCaseData(
            new InstallationInfo("path 1", new Version(1, 0), "1.0-preview.1"),
            new InstallationInfo("path 2", new Version(1, 0), "1.0-preview.1"))
        {
            TestName = "1.0-preview.1 vs 1.0-preview.1"
        };
    }
}