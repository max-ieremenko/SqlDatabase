using System.Runtime.Loader;

namespace SqlDatabase.Adapter.PowerShellScripts;

// PowerShellAssemblyLoadContextInitializer is defined in Microsoft.WSMan.Runtime
// in the PowerShellStandard.Library it is missing
internal static class PowerShellAssemblyLoadContextInitializerAdapter
{
    private const string TypeFullName = $"{InstallationSeeker.RootAssemblyName}.PowerShellAssemblyLoadContextInitializer";
    private const string MethodName = "SetPowerShellAssemblyLoadContext";

    public static void SetPowerShellAssemblyLoadContext(string installationPath)
    {
        var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(installationPath, InstallationSeeker.RootAssemblyFileName));

        var type = assembly.GetType(TypeFullName, throwOnError: true, ignoreCase: false);

        var method = type
            .FindStaticMethod(MethodName, typeof(string))
            .CreateDelegate<Action<string>>();

        method(installationPath);
    }
}