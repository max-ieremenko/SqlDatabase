using System;
using System.Collections.Generic;
using SqlDatabase.Log;

namespace SqlDatabase.PowerShell
{
    internal sealed class OutputReader
    {
        internal const string SetForegroundColorToDefault = RedirectedConsoleLogger.SetForegroundColorToDefault;
        internal const string SetForegroundColorToRed = RedirectedConsoleLogger.SetForegroundColorToRed;

        private bool _errorFlag;

        public KeyValuePair<string, bool> NextLine(string text)
        {
            var startFromRed = text.StartsWith(SetForegroundColorToRed, StringComparison.Ordinal);
            var endWithDefault = text.EndsWith(SetForegroundColorToDefault, StringComparison.Ordinal);

            var message = text;
            if (startFromRed)
            {
                message = message.Substring(SetForegroundColorToRed.Length);
            }

            if (endWithDefault)
            {
                message = message.Substring(0, message.Length - SetForegroundColorToDefault.Length);
            }

            var line = new KeyValuePair<string, bool>(message, _errorFlag || startFromRed);

            if (endWithDefault)
            {
                _errorFlag = false;
            }
            else if (startFromRed)
            {
                _errorFlag = true;
            }

            return line;
        }
    }
}
