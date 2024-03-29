using NUnit.Framework;
using Shouldly;

namespace SqlDatabase.Adapter.PowerShellScripts;

[TestFixture]
public class PowerShellWindowsTest
{
    [Test]
    public void GetParentProcessId()
    {
        var processId = Process.GetCurrentProcess().Id;

        PowerShellWindows.GetParentProcessId(processId).ShouldNotBeNull();
    }
}