using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace SqlDatabase.Scripts
{
    [TestFixture]
    public class SqlBatchParserTest
    {
        [Test]
        [TestCaseSource(typeof(SqlBatchParserTest), nameof(GetSplitByGoTestCases))]
        public void SplitByGo(Stream input, string[] expected)
        {
            var actual = SqlBatchParser.SplitByGo(input);
            CollectionAssert.AreEqual(expected, actual);
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
            Assert.AreEqual(expected, SqlBatchParser.IsGo(line));
        }

        private static IEnumerable<object> GetSplitByGoTestCases()
        {
            var anchor = typeof(SqlBatchParserTest);
            var prefix = anchor.Namespace + "." + anchor.Name + ".";

            var sources = anchor
                .Assembly
                .GetManifestResourceNames()
                .Where(i => i.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .OrderBy(i => i);

            foreach (var sourceName in sources)
            {
                var testCase = BuildSplitByGoTestCase(sourceName);
                testCase.TestName = Path.GetFileNameWithoutExtension(sourceName.Substring(prefix.Length));

                yield return testCase;
            }
        }

        private static TestCaseData BuildSplitByGoTestCase(string sourceName)
        {
            const string Separator = "--------------";

            var input = new StringBuilder();
            var expected = new List<string>();
            var currentExpected = new StringBuilder();

            using (var stream = typeof(SqlBatchParserTest).Assembly.GetManifestResourceStream(sourceName))
            using (var reader = new StreamReader(stream))
            {
                var isInput = true;

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line == Separator)
                    {
                        isInput = false;
                        if (currentExpected.Length > 0)
                        {
                            expected.Add(currentExpected.ToString());
                            currentExpected.Clear();
                        }
                    }
                    else if (isInput)
                    {
                        if (input.Length > 0)
                        {
                            input.AppendLine();
                        }

                        input.Append(line);
                    }
                    else
                    {
                        if (currentExpected.Length > 0)
                        {
                            currentExpected.AppendLine();
                        }

                        currentExpected.Append(line);
                    }
                }
            }

            if (currentExpected.Length > 0)
            {
                expected.Add(currentExpected.ToString());
            }

            return new TestCaseData(
                new MemoryStream(Encoding.Default.GetBytes(input.ToString())),
                expected.ToArray());
        }
    }
}
