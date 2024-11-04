using System.Collections;
using NUnit.Framework;
using Shouldly;

namespace SqlDatabase.PowerShell.Internal;

[TestFixture]
public class DependencyResolverFactoryTest
{
    [Test]
    [TestCase("Desktop")]
    [TestCase(null)]
    public void CreateDesktop(string? psEdition)
    {
        var psVersionTable = new Hashtable();
        if (psEdition != null)
        {
            psVersionTable.Add("PSEdition", psEdition);
        }

        var actual = DependencyResolverFactory.Create(new PSVersionTable(psVersionTable));

        actual.ShouldBeOfType<PowerShellDesktopDependencyResolver>();
    }

    [Test]
    public void CreateCore()
    {
        var psVersionTable = new Hashtable
        {
            { "PSEdition", "Core" }
        };

        var ex = Should.Throw<FileNotFoundException>(() => DependencyResolverFactory.Create(new PSVersionTable(psVersionTable)));

        ex.Message.ShouldContain("System.Runtime.Loader");
    }
}