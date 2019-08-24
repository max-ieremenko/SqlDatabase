using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Shouldly;

namespace SqlDatabase.Scripts
{
    [TestFixture]
    public class SqlBatchParserTest
    {
        [Test]
        [TestCaseSource(nameof(GetSplitByGoTestCases))]
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

        [Test]
        [TestCaseSource(nameof(GetExtractDependenciesTestCases))]
        public void ExtractDependencies(string sql, ScriptDependency[] expected)
        {
            var actual = SqlBatchParser.ExtractDependencies(sql, "file name").ToArray();
            actual.ShouldBe(expected);
        }

        [Test]
        [TestCase("1.a")]
        [TestCase("10")]
        public void ExtractDependenciesInvalidVersion(string versionText)
        {
            var input = "-- module dependency: moduleName " + versionText;
            var ex = Assert.Throws<InvalidOperationException>(() => SqlBatchParser.ExtractDependencies(input, "file name").ToArray());

            ex.Message.ShouldContain("moduleName");
            ex.Message.ShouldContain(versionText);
        }

        private static IEnumerable<TestCaseData> GetSplitByGoTestCases()
        {
            foreach (var testCase in ReadResources("Go"))
            {
                yield return new TestCaseData(
                    new MemoryStream(Encoding.Default.GetBytes(testCase.Input)),
                    testCase.Expected)
                {
                    TestName = testCase.Name
                };
            }
        }

        private static IEnumerable<TestCaseData> GetExtractDependenciesTestCases()
        {
            foreach (var testCase in ReadResources("Dependencies"))
            {
                var expected = new List<ScriptDependency>();
                foreach (var line in testCase.Expected)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        parts.Length.ShouldBe(2);
                        expected.Add(new ScriptDependency(parts[0], new Version(parts[1])));
                    }
                }

                yield return new TestCaseData(testCase.Input, expected.ToArray())
                {
                    TestName = testCase.Name
                };
            }
        }

        private static IEnumerable<(string Name, string Input, string[] Expected)> ReadResources(string folder)
        {
            var anchor = typeof(SqlBatchParserTest);
            var prefix = anchor.Namespace + "." + anchor.Name + "." + folder + ".";

            var sources = anchor
                .Assembly
                .GetManifestResourceNames()
                .Where(i => i.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .OrderBy(i => i);

            foreach (var sourceName in sources)
            {
                using (var stream = anchor.Assembly.GetManifestResourceStream(sourceName))
                using (var reader = new StreamReader(stream))
                {
                    var name = Path.GetFileNameWithoutExtension(sourceName.Substring(prefix.Length));
                    var (input, expected) = ParseResource(reader);

                    yield return (name, input, expected);
                }
            }
        }

        private static (string Input, string[] Expected) ParseResource(TextReader reader)
        {
            const string Separator = "--------------";

            var input = new StringBuilder();
            var expected = new List<string>();
            var currentExpected = new StringBuilder();

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

            if (currentExpected.Length > 0)
            {
                expected.Add(currentExpected.ToString());
            }

            return (input.ToString(), expected.ToArray());
        }
    }
}
