using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SqlDatabase.Scripts.SqlTestCases;

internal static class ResourceReader
{
    public static IEnumerable<(string Name, string Input, string[] Expected)> Read(string folder)
    {
        var anchor = typeof(ResourceReader);
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