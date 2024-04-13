using SqlDatabase.CommandLine;

namespace SqlDatabase.PowerShell.Internal;

internal static class CommandLineTools
{
    public static void AppendFrom(List<ScriptSource> target, bool isInline, string[]? from)
    {
        if (from == null)
        {
            return;
        }

        for (var i = 0; i < from.Length; i++)
        {
            var path = from[i];
            if (!string.IsNullOrEmpty(path))
            {
                target.Add(new ScriptSource(isInline, path));
            }
        }
    }

    public static void AppendVariables(Dictionary<string, string> target, string[]? from)
    {
        if (from == null)
        {
            return;
        }

        for (var i = 0; i < from.Length; i++)
        {
            CommandLineParser.AddVariable(target, from[i]);
        }
    }
}