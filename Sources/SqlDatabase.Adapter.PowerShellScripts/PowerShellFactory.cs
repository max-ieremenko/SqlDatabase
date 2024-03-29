using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace SqlDatabase.Adapter.PowerShellScripts;

// https://github.com/PowerShell/PowerShell/tree/master/docs/host-powershell
internal sealed class PowerShellFactory : IPowerShellFactory
{
    private static string? _initializedInstallationPath;
    private bool _initialized;

    public PowerShellFactory(HostedRuntime runtime, string? installationPath)
    {
        Runtime = runtime;
        InstallationPath = installationPath;
    }

    public HostedRuntime Runtime { get; }

    public string? InstallationPath { get; private set; }

    public void Initialize(ILogger logger)
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        if (!Runtime.IsPowershell && Runtime.Version != FrameworkVersion.Net472)
        {
            InitializePowerShellAssemblyLoadContext(logger);
        }
    }

    public IPowerShell Create()
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("PowerShell host is not initialized.");
        }

        return new PowerShell(Runtime.Version);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void InitializePowerShellAssemblyLoadContext(ILogger logger)
    {
        if (string.IsNullOrEmpty(InstallationPath))
        {
            if (InstallationSeeker.TryFindByParentProcess(Runtime, out var test))
            {
                InstallationPath = test;
            }
            else if (InstallationSeeker.TryFindOnDisk(Runtime, out test))
            {
                InstallationPath = test;
            }
        }

        if (string.IsNullOrEmpty(InstallationPath))
        {
            throw new InvalidOperationException("PowerShell Core installation not found, please provide installation path via command line options -usePowerShell.");
        }

        if (!InstallationSeeker.TryGetInfo(InstallationPath!, out var info))
        {
            throw new InvalidOperationException($"PowerShell Core installation not found in {InstallationPath}.");
        }

        logger.Info($"host PowerShell from {InstallationPath}, version {info.ProductVersion}");

        Func<AssemblyLoadContext, AssemblyName, Assembly?> assemblyResolver = AssemblyResolving;
        AssemblyLoadContext.Default.Resolving += assemblyResolver;
        try
        {
            Test(logger);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"PowerShell host {InstallationPath} initialization failed. Try to use another PowerShell Core installation.", ex);
        }
        finally
        {
            AssemblyLoadContext.Default.Resolving -= assemblyResolver;
        }
    }

    private void Test(ILogger logger)
    {
        SetPowerShellAssemblyLoadContext();

        using (logger.Indent())
        {
            const string Script = @"
Write-Host ""PSVersion:"" $PSVersionTable.PSVersion
Write-Host ""PSEdition:"" $PSVersionTable.PSEdition
Write-Host ""OS:"" $PSVersionTable.OS";

            Create().Invoke(Script, logger);
        }
    }

    private Assembly? AssemblyResolving(AssemblyLoadContext context, AssemblyName assemblyName)
    {
        if (InstallationSeeker.RootAssemblyName.Equals(assemblyName.Name, StringComparison.OrdinalIgnoreCase))
        {
            var fileName = Path.Combine(InstallationPath!, InstallationSeeker.RootAssemblyFileName);
            return context.LoadFromAssemblyPath(fileName);
        }

        // https://github.com/PowerShell/PowerShell/releases/download/v7.0.5/powershell_7.0.5-1.debian.10_amd64.deb
        // Could not load file or assembly 'Microsoft.Management.Infrastructure, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'. The system cannot find the file specified.
        // package contains Microsoft.Management.Infrastructure, Version=2.0.0.0, Culture=neutral, PublicKeyToken=null
        if ("Microsoft.Management.Infrastructure".Equals(assemblyName.Name, StringComparison.OrdinalIgnoreCase))
        {
            var fileName = Path.Combine(InstallationPath!, assemblyName.Name + ".dll");
            if (File.Exists(fileName))
            {
                return context.LoadFromAssemblyPath(fileName);
            }
        }

        return null;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void SetPowerShellAssemblyLoadContext()
    {
        // The singleton of PowerShellAssemblyLoadContext has already been initialized
        if (_initializedInstallationPath == null || !_initializedInstallationPath.Equals(InstallationPath, StringComparison.OrdinalIgnoreCase))
        {
            PowerShellAssemblyLoadContextInitializerAdapter.SetPowerShellAssemblyLoadContext(InstallationPath!);
            _initializedInstallationPath = InstallationPath;
        }
    }
}