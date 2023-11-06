using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.TestApi;

namespace SqlDatabase.Scripts.MsSql;

[TestFixture]
public class MsSqlTextReaderTest
{
    private MsSqlTextReader _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _sut = new MsSqlTextReader();
    }

    [Test]
    [TestCase("go", true)]
    [TestCase("go go", true)]
    [TestCase(" go ", true)]
    [TestCase(" \tGO \t", true)]
    [TestCase("go\tgo go", true)]
    [TestCase("o", false)]
    [TestCase("fo", false)]
    [TestCase("go pro", false)]
    public void IsGo(string line, bool expected)
    {
        _sut.IsGo(line).ShouldBe(expected);
    }

    [Test]
    [TestCaseSource(nameof(GetSplitByGoTestCases))]
    public void SplitByGo(byte[] input, string[] expected)
    {
        var batches = _sut.ReadBatches(new MemoryStream(input));
        batches.ShouldBe(expected);

        var first = _sut.ReadFirstBatch(new MemoryStream(input));
        first.ShouldBe(expected[0]);
    }

    private static IEnumerable<TestCaseData> GetSplitByGoTestCases()
    {
        foreach (var testCase in ResourceReader.Read(typeof(MsSqlTextReaderTest).Assembly, "SqlTestCases.Go"))
        {
            yield return new TestCaseData(
                Encoding.Default.GetBytes(testCase.Input),
                testCase.Expected)
            {
                TestName = testCase.Name
            };
        }
    }
}