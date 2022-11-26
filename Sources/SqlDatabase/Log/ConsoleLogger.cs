using System;

namespace SqlDatabase.Log;

internal sealed class ConsoleLogger : LoggerBase
{
    protected override void WriteError(string message)
    {
        var color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;

        Console.WriteLine(message);

        Console.ForegroundColor = color;
    }

    protected override void WriteInfo(string message)
    {
        Console.WriteLine(message);
    }
}