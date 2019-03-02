using System;
using System.Text;
using SqlDatabase.Log;

namespace SqlDatabase.PowerShell
{
    internal sealed class OutputReader
    {
        internal const string SetForegroundColorToDefault = RedirectedConsoleLogger.SetForegroundColorToDefault;
        internal const string SetForegroundColorToRed = RedirectedConsoleLogger.SetForegroundColorToRed;

        private readonly StringBuilder _errorBuffer = new StringBuilder();
        private bool _errorFlag;

        public Record? NextLine(string text)
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

            var isError = _errorFlag || startFromRed;

            if (endWithDefault)
            {
                _errorFlag = false;
            }
            else if (startFromRed)
            {
                _errorFlag = true;
            }

            if (isError)
            {
                if (_errorBuffer.Length > 0)
                {
                    _errorBuffer.AppendLine();
                }

                _errorBuffer.Append(message);

                if (_errorFlag)
                {
                    return null;
                }

                var record = new Record(_errorBuffer.ToString(), true);
                _errorBuffer.Clear();
                return record;
            }

            return new Record(message, false);
        }

        public Record? Flush()
        {
            if (_errorBuffer.Length == 0)
            {
                return null;
            }

            return new Record(_errorBuffer.ToString(), true);
        }

        public struct Record
        {
            public Record(string text, bool isError)
            {
                Text = text;
                IsError = isError;
            }

            public string Text { get; }

            public bool IsError { get; }
        }
    }
}
