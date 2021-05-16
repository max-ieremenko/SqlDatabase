using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Scripts.SqlTestCases;

namespace SqlDatabase.Scripts.MsSql
{
    [TestFixture]
    public class MsSqlTextReaderTest
    {
        private MsSqlTextReader _sut;

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
        public void SplitByGo(Stream input, string[] expected)
        {
            var actual = _sut.Read(input);
            actual.ShouldBe(expected);
        }

        private static IEnumerable<TestCaseData> GetSplitByGoTestCases()
        {
            foreach (var testCase in ResourceReader.Read("Go"))
            {
                yield return new TestCaseData(
                    new MemoryStream(Encoding.Default.GetBytes(testCase.Input)),
                    testCase.Expected)
                {
                    TestName = testCase.Name
                };
            }
        }
    }
}
