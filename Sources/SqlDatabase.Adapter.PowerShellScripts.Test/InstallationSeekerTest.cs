using System.Reflection;
using System.Runtime.InteropServices;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.TestApi;

namespace SqlDatabase.Adapter.PowerShellScripts;

[TestFixture]
public class InstallationSeekerTest
{
    private HostedRuntime _runtime;

    [OneTimeSetUp]
    public void BeforeAllTests()
    {
#if NET472
        var version = FrameworkVersion.Net472;
#elif NET8_0
        var version = FrameworkVersion.Net8;
#else
        var version = FrameworkVersion.Net9;
#endif

        _runtime = new HostedRuntime(false, RuntimeInformation.IsOSPlatform(OSPlatform.Windows), version);
    }

    [Test]
    public void TryFindByParentProcess()
    {
        var actual = InstallationSeeker.TryFindByParentProcess(_runtime, out var path);
        if (actual)
        {
            TestOutput.WriteLine(path);
        }
    }

    [Test]
    public void TryFindOnDisk()
    {
        InstallationSeeker.TryFindOnDisk(_runtime, out var path).ShouldBe(_runtime.Version != FrameworkVersion.Net472);

        TestOutput.WriteLine(path);
    }

    [Test]
    public void TryGetInfo()
    {
        using (var dir = new TempDirectory())
        {
            var root = Path.Combine(dir.Location, InstallationSeeker.RootAssemblyFileName);

            InstallationSeeker.TryGetInfo(dir.Location, out _).ShouldBeFalse();

            File.WriteAllText(Path.Combine(dir.Location, InstallationSeeker.PowershellFileName), "dummy");
            File.WriteAllText(root, "dummy");

            InstallationSeeker.TryGetInfo(dir.Location, out _).ShouldBeFalse();

            File.Delete(root);
            File.Copy(GetType().Assembly.Location, root);

            InstallationSeeker.TryGetInfo(dir.Location, out var actual).ShouldBeTrue();

            actual.Location.ShouldBe(dir.Location);
            actual.Version.ShouldBe(GetType().Assembly.GetName().Version);
            actual.ProductVersion.ShouldBe(GetType().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion);
        }
    }
}