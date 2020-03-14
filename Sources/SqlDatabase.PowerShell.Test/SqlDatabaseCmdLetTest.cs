using NUnit.Framework;
using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell
{
    [TestFixture]
    public class SqlDatabaseCmdLetTest
    {
        [Test]
        public void AppendDefaultConfiguration()
        {
            var command = new GenericCommandLine();

            SqlDatabaseCmdLet.AppendDefaultConfiguration(command);

            FileAssert.Exists(command.ConfigurationFile);
        }
    }
}
