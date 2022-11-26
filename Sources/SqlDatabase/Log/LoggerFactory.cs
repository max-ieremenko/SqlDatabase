namespace SqlDatabase.Log;

internal static class LoggerFactory
{
    public static ILogger CreateDefault()
    {
        return new ConsoleLogger();
    }
}