#if NETCOREAPP || NET5_0_OR_GREATER
using System;
using System.IO;
using System.Management.Automation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using SqlDatabase.Configuration;

namespace SqlDatabase.Scripts.PowerShellInternal
{
    // https://github.com/PowerShell/PowerShell/tree/master/docs/host-powershell
    internal partial class PowerShellFactory
    {
        partial void DoInitialize(ILogger logger)
        {
            if (string.IsNullOrEmpty(InstallationPath))
            {
                if (InstallationSeeker.TryFindByParentProcess(out var test))
                {
                    InstallationPath = test;
                }
                else if (InstallationSeeker.TryFindOnDisk(out test))
                {
                    InstallationPath = test;
                }
            }

            if (string.IsNullOrEmpty(InstallationPath))
            {
                throw new InvalidOperationException("PowerShell Core installation not found, please provide installation path via command line options {0}{1}.".FormatWith(Arg.Sign, Arg.UsePowerShell));
            }

            if (!InstallationSeeker.TryGetInfo(InstallationPath, out var info))
            {
                throw new InvalidOperationException("PowerShell Core installation not found in {0}.".FormatWith(InstallationPath));
            }

            logger.Info("host PowerShell from {0}, version {1}".FormatWith(InstallationPath, info.ProductVersion));

            AssemblyLoadContext.Default.Resolving += AssemblyResolving;
            try
            {
                Test(logger);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("PowerShell host initialization failed. Try to use another PowerShell Core installation.", ex);
            }
            finally
            {
                AssemblyLoadContext.Default.Resolving -= AssemblyResolving;
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

        private Assembly AssemblyResolving(AssemblyLoadContext context, AssemblyName assemblyName)
        {
            if (InstallationSeeker.RootAssemblyName.Equals(assemblyName.Name, StringComparison.OrdinalIgnoreCase))
            {
                var fileName = Path.Combine(InstallationPath, InstallationSeeker.RootAssemblyFileName);
                return context.LoadFromAssemblyPath(fileName);
            }

            // https://github.com/PowerShell/PowerShell/releases/download/v7.0.5/powershell_7.0.5-1.debian.10_amd64.deb
            // Could not load file or assembly 'Microsoft.Management.Infrastructure, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'. The system cannot find the file specified.
            // package contains Microsoft.Management.Infrastructure, Version=2.0.0.0, Culture=neutral, PublicKeyToken=null
            if ("Microsoft.Management.Infrastructure".Equals(assemblyName.Name, StringComparison.OrdinalIgnoreCase))
            {
                var fileName = Path.Combine(InstallationPath, assemblyName.Name + ".dll");
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
            PowerShellAssemblyLoadContextInitializer.SetPowerShellAssemblyLoadContext(InstallationPath);
        }
    }
}
#endif