using System.Management.Automation;
using System.Runtime.InteropServices;
using SqlDatabase.Configuration;
using SqlDatabase.PowerShell.Internal;

namespace SqlDatabase.PowerShell;

[Cmdlet(VerbsCommon.Show, "SqlDatabaseInfo")]
public sealed class InfoCmdLet : PSCmdlet
{
    protected override void ProcessRecord()
    {
        using (var resolver = DependencyResolverFactory.Create(this))
        {
            resolver.Initialize();
            WriteInfo();
        }
    }

    private void WriteInfo()
    {
        var assembly = GetType().Assembly;
        var location = Path.GetDirectoryName(assembly.Location);

        this.TryGetPSVersionTable(out var psVersionTable);

        WriteObject(new
        {
            psVersionTable.PSEdition,
            psVersionTable.PSVersion,
            Version = assembly.GetName().Version,
            RuntimeInformation.FrameworkDescription,
            RuntimeInformation.OSDescription,
            RuntimeInformation.OSArchitecture,
            RuntimeInformation.ProcessArchitecture,
            Location = location,
            WorkingDirectory = this.GetWorkingDirectory(),
            DefaultConfigurationFile = ConfigurationManager.ResolveDefaultConfigurationFile(location)
        });
    }
}