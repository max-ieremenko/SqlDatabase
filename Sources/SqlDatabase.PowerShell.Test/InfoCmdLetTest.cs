using System;
using System.Runtime.InteropServices;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.PowerShell.TestApi;
using SqlDatabase.TestApi;

namespace SqlDatabase.PowerShell;

[TestFixture]
public class InfoCmdLetTest : SqlDatabaseCmdLetTest<InfoCmdLet>
{
    [Test]
    public void ProcessRecord()
    {
        var actual = InvokeCommand("Show-SqlDatabaseInfo");

        actual.Count.ShouldBe(1);
        TestOutput.WriteLine(actual[0]);

        actual[0].Properties["PSEdition"].Value.ShouldBeOfType<string>().ShouldNotBeNullOrWhiteSpace();
        actual[0].Properties["PSVersion"].Value.ShouldBeOfType<string>().ShouldNotBeNullOrWhiteSpace();
        actual[0].Properties["Version"].Value.ShouldBeOfType<Version>().ShouldNotBeNull();
        actual[0].Properties["FrameworkDescription"].Value.ShouldBeOfType<string>().ShouldNotBeNullOrWhiteSpace();
        actual[0].Properties["OSDescription"].Value.ShouldBeOfType<string>().ShouldNotBeNullOrWhiteSpace();
        actual[0].Properties["OSArchitecture"].Value.ShouldBeOfType<Architecture>();
        actual[0].Properties["ProcessArchitecture"].Value.ShouldBeOfType<Architecture>();
        actual[0].Properties["Location"].Value.ShouldBeOfType<string>().ShouldNotBeNullOrWhiteSpace();
        actual[0].Properties["WorkingDirectory"].Value.ShouldBeOfType<string>().ShouldNotBeNullOrWhiteSpace();
        actual[0].Properties["DefaultConfigurationFile"].Value.ShouldBeOfType<string>().ShouldNotBeNullOrWhiteSpace();
    }
}