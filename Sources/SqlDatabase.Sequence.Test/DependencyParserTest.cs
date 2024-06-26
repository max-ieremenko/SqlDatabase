﻿using NUnit.Framework;
using Shouldly;
using SqlDatabase.TestApi;

namespace SqlDatabase.Sequence;

[TestFixture]
public class DependencyParserTest
{
    [Test]
    [TestCaseSource(nameof(GetExtractDependenciesTestCases))]
    public void ExtractDependencies(string sql, Array expected)
    {
        var actual = DependencyParser.ExtractDependencies(new StringReader(sql), "file name").ToArray();
        actual.ShouldBe((ScriptDependency[])expected);
    }

    [Test]
    [TestCase("1.a")]
    [TestCase("10")]
    public void ExtractDependenciesInvalidVersion(string versionText)
    {
        var input = "-- module dependency: moduleName " + versionText;
        var ex = Assert.Throws<InvalidOperationException>(() => DependencyParser.ExtractDependencies(new StringReader(input), "file name").ToArray());

        ex!.Message.ShouldContain("moduleName");
        ex.Message.ShouldContain(versionText);
    }

    private static IEnumerable<TestCaseData> GetExtractDependenciesTestCases()
    {
        foreach (var testCase in ResourceReader.Read(typeof(DependencyParserTest).Assembly, nameof(DependencyParserTest)))
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
}