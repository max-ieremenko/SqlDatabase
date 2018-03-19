using System;

namespace SqlDatabase
{
    internal sealed class ConsoleLogger : ILogger
    {
        public void Error(string message)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine(message);

            Console.ForegroundColor = color;
        }

        public void Info(string message)
        {
            Console.WriteLine(message);
        }
    }
}