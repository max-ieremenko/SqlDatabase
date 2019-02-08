using System;

namespace SqlDatabase.Log
{
    internal static class LoggerFactory
    {
        public static ILogger CreateDefault()
        {
            if (Console.IsOutputRedirected)
            {
                return new RedirectedConsoleLogger();
            }

            return new ConsoleLogger();
        }
    }
}
