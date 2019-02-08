using System;

namespace SqlDatabase.Log
{
    internal sealed class RedirectedConsoleLogger : LoggerBase
    {
        // http://pueblo.sourceforge.net/doc/manual/ansi_color_codes.html
        internal const string SetForegroundColorToDefault = "\x001B[39m";
        internal const string SetForegroundColorToRed = "\x001B[31m";

        protected override void WriteError(string message)
        {
            Console.WriteLine(SetForegroundColorToRed + message + SetForegroundColorToDefault);
        }

        protected override void WriteInfo(string message)
        {
            Console.WriteLine(message);
        }
    }
}
