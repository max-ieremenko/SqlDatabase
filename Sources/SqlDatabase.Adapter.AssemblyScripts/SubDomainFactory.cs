using System.Runtime.CompilerServices;

namespace SqlDatabase.Adapter.AssemblyScripts;

internal static class SubDomainFactory
{
    public static void Test(FrameworkVersion version)
    {
        if (version == FrameworkVersion.Net472)
        {
            Test472SubDomain();
        }
        else
        {
            Test472CoreSubDomain();
        }
    }

    public static ISubDomain Create(FrameworkVersion version, ILogger logger, string assemblyFileName, Func<byte[]> readAssemblyContent)
    {
        if (version == FrameworkVersion.Net472)
        {
            return Create472SubDomain(logger, assemblyFileName, readAssemblyContent);
        }

        return CreateCoreSubDomain(logger, assemblyFileName, readAssemblyContent);
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