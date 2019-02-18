namespace SqlDatabase.Log
{
    internal static class LoggerFactory
    {
        public static ILogger CreateDefault()
        {
            return new ConsoleLogger();
        }

        public static ILogger CreatePreFormatted()
        {
            return new RedirectedConsoleLogger();
        }
    }
}
