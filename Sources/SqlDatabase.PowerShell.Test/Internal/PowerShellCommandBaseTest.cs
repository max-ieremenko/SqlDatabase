using NUnit.Framework;
using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell.Internal
{
    [TestFixture]
    public class PowerShellCommandBaseTest
    {
        [Test]
        public void AppendDefaultConfiguration()
        {
            var command = new GenericCommandLine();

            PowerShellCommandBase.AppendDefaultConfiguration(command);

            FileAssert.Exists(command.ConfigurationFile);
        }
    }
}
