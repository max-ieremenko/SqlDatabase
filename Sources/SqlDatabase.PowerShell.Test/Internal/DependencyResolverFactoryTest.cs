using System;
using System.Collections;
using NUnit.Framework;
using Shouldly;

namespace SqlDatabase.PowerShell.Internal;

[TestFixture]
public class DependencyResolverFactoryTest
{
    [Test]
    [TestCase("Desktop", typeof(PowerShellDesktopDependencyResolver))]
    [TestCase(null, typeof(PowerShellDesktopDependencyResolver))]
    [TestCase("Core", typeof(PowerShellCoreDependencyResolver))]
    public void CreateProgram(string? psEdition, Type expected)
    {
        var psVersionTable = new Hashtable();
        if (psEdition != null)
        {
            psVersionTable.Add("PSEdition", psEdition);
        }

        var actual = DependencyResolverFactory.Create(new PSVersionTable(psVersionTable));

        actual.ShouldBeOfType(expected);
    }
}