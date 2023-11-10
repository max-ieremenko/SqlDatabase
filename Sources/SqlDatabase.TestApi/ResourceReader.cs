using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Shouldly;

namespace SqlDatabase.TestApi;

public static class ResourceReader
{
    public static Stream GetManifestResourceStream(string resourceName, Type? resourceAnchor = null)
    {
        if (resourceAnchor == null)
        {
            resourceAnchor = new StackTrace().GetFrame(1)!.GetMethod()!.DeclaringType;
        }

        var result = resourceAnchor!.Assembly.GetManifestResourceStream(resourceAnchor.Namespace + "." + resourceName);
        result.ShouldNotBeNull(resourceName);

        return result;
    }

    public static IEnumerable<(string Name, string Input, string[] Expected)> Read(Assembly assembly, string filter)
    {
        var sources = assembly
            .GetManifestResourceNames()
            .Where(i => i.IndexOf(filter, StringComparison.Ordinal) >= 0)
            .OrderBy(i => i)
            .ToArray();

        sources.ShouldNotBeEmpty();

        foreach (var sourceName in sources)
        {
            using (var stream = assembly.GetManifestResourceStream(sourceName))
            using (var reader = new StreamReader(stream!))
            {
                var (input, expected) = ParseResource(reader);

                yield return (GetShortName(sourceName), input, expected);
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

        string? line;
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

    private static string GetShortName(string fullName)
    {
        var result = fullName;

        var index = fullName.LastIndexOf('.');
        if (index > 0)
        {
            index = fullName.LastIndexOf('.', index - 1);
        }

        if (index > 0)
        {
            result = fullName.Substring(index + 1);
        }

        return result;
    }
}