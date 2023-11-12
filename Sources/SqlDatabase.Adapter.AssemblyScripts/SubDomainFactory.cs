using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SqlDatabase.Adapter.AssemblyScripts;

internal static class SubDomainFactory
{
    public static void Test()
    {
        if (IsNetFrameworkRuntime())
        {
            Test472SubDomain();
        }
        else
        {
            Test472CoreSubDomain();
        }
    }

    public static ISubDomain Create(ILogger logger, string assemblyFileName, Func<byte[]> readAssemblyContent)
    {
        if (IsNetFrameworkRuntime())
        {
            return Create472SubDomain(logger, assemblyFileName, readAssemblyContent);
        }

        return CreateCoreSubDomain(logger, assemblyFileName, readAssemblyContent);
    }

    // https://learn.microsoft.com/en-us/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed
    private static bool IsNetFrameworkRuntime()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
               && RuntimeInformation.FrameworkDescription.IndexOf("Framework", StringComparison.OrdinalIgnoreCase) > 0;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ISubDomain Create472SubDomain(ILogger logger, string assemblyFileName, Func<byte[]> readAssemblyContent)
    {
        return new Net472.Net472SubDomain(logger, assemblyFileName, readAssemblyContent);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Test472SubDomain()
    {
        Net472.Net472SubDomain.Test();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ISubDomain CreateCoreSubDomain(ILogger logger, string assemblyFileName, Func<byte[]> readAssemblyContent)
    {
        return new NetCore.NetCoreSubDomain(logger, assemblyFileName, readAssemblyContent);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Test472CoreSubDomain()
    {
        NetCore.NetCoreSubDomain.Test();
    }
}