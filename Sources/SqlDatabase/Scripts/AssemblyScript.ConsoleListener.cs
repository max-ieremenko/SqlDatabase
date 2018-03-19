using System.IO;
using System.Text;

namespace SqlDatabase.Scripts
{
    internal partial class AssemblyScript
    {
        private sealed class ConsoleListener : TextWriter
        {
            private readonly ILogger _logger;

            public ConsoleListener(ILogger logger)
            {
                _logger = logger;
            }

            public override Encoding Encoding => Encoding.Unicode;

            public override void Write(string value)
            {
                _logger.Info(value);
            }

            public override void WriteLine(string value)
            {
                _logger.Info(value);
            }
        }
    }
}