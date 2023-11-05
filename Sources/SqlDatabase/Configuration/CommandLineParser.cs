using System;
using System.Collections.Generic;

namespace SqlDatabase.Configuration;

internal sealed class CommandLineParser
{
    public static string? GetLogFileName(IList<string> args)
    {
        for (var i = 0; i < args.Count; i++)
        {
            if (ParseArg(args[i], out var value)
                && IsLog(value))
            {
                return value.Value;
            }
        }

        return null;
    }

    public CommandLine Parse(params string[] args)
    {
        var result = new List<Arg>(args.Length);

        foreach (var arg in args)
        {
            if (!ParseArg(arg, out var value))
            {
                throw new InvalidCommandLineException("Invalid option [{0}].".FormatWith(arg));
            }

            if (!IsLog(value))
            {
                result.Add(value);
            }
        }

        return new CommandLine(result, args);
    }

    internal static bool ParseArg(string input, out Arg arg)
    {
        arg = default;

        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        if (input.StartsWith(Arg.Sign, StringComparison.OrdinalIgnoreCase))
        {
            if (SplitKeyValue(input, 1, out var key, out var value))
            {
                arg = new Arg(key, value);
                return true;
            }

            return false;
        }

        arg = new Arg(input);
        return true;
    }

    private static bool SplitKeyValue(string keyValue, int offset, out string key, out string? value)
    {
        keyValue = keyValue.Substring(offset);
        key = keyValue;
        value = null;

        if (key.Length == 0)
        {
            return false;
        }

        var index = keyValue.IndexOf("=", StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return true;
        }

        if (index == 0)
        {
            return false;
        }

        key = keyValue.Substring(0, index).Trim();
        value = index == keyValue.Length - 1 ? null : keyValue.Substring(index + 1).Trim();

        return true;
    }

    private static bool IsLog(Arg arg)
    {
        return arg.IsPair
               && Arg.Log.Equals(arg.Key, StringComparison.OrdinalIgnoreCase)
               && !string.IsNullOrWhiteSpace(arg.Value);
    }
}