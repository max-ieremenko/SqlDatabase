using NUnit.Framework;
using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell.Internal;

[TestFixture]
public class PowerShellCommandBaseTest
{
    [Test]
    public void AppendDefaultConfiguration()
    {
        var command = new GenericCommandLine();

        PowerShellCommandBase.AppendDefaultConfiguration(command);

        Assert.That(command.ConfigurationFile, Does.Exist.IgnoreDirectories);
    }
}