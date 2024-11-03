using System.Runtime.InteropServices;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Adapter;
using SqlDatabase.TestApi;

namespace SqlDatabase.Configuration;

[TestFixture]
public class HostedRuntimeResolverTest
{
    [Test]
    public void GetRuntime()
    {
        TestOutput.WriteLine($"Version: {Environment.Version}");
        TestOutput.WriteLine($"FrameworkDescription: {RuntimeInformation.FrameworkDescription}");

#if NET472
        var expected = FrameworkVersion.Net472;
#elif NET6_0
        var expected = FrameworkVersion.Net6;
#else
        var expected = FrameworkVersion.Net8;
#endif

        var actual = HostedRuntimeResolver.GetRuntime(false);

        actual.Version.ShouldBe(expected);
    }

    [Test]
    [TestCase(".NET Framework 4.8.9181.0", "4.0.30319.42000", FrameworkVersion.Net472, TestName = ".net 4.8")]
    [TestCase(".NET Core 4.6.26725.06", "4.0.30319.42000", FrameworkVersion.Net6, TestName = "6.1.0-ubuntu-18.04")]
    [TestCase(".NET Core 4.6.27317.03", "4.0.30319.42000", FrameworkVersion.Net6, TestName = "6.1.3-ubuntu-18.04")]
    [TestCase(".NET Core 4.6.27817.01", "4.0.30319.42000", FrameworkVersion.Net6, TestName = "6.2.2-ubuntu-18.04")]
    [TestCase(".NET Core 4.6.28008.01", "4.0.30319.42000", FrameworkVersion.Net6, TestName = "6.2.4-ubuntu-18.04")]
    [TestCase(".NET Core 3.1.2", "3.1.2", FrameworkVersion.Net6, TestName = "7.0.0-ubuntu-18.04")]
    [TestCase(".NET Core 3.1.6", "3.1.6", FrameworkVersion.Net6, TestName = "7.0.0-ubuntu-18.04")]
    [TestCase(".NET 5.0.0", "5.0.0", FrameworkVersion.Net6, TestName = "7.1.0-ubuntu-18.04")]
    public void ResolveVersion(string description, string version, FrameworkVersion expected)
    {
        HostedRuntimeResolver.ResolveVersion(description, new Version(version)).ShouldBe(expected);
    }
}