using System.IO;
using System.Management.Automation;
using System.Runtime.InteropServices;
using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell
{
    [Cmdlet(VerbsCommon.Show, "SqlDatabaseInfo")]
    public sealed class InfoCmdLet : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            var assembly = GetType().Assembly;

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
                Location = Path.GetDirectoryName(assembly.Location),
                WorkingDirectory = this.GetWorkingDirectory(),
                DefaultConfigurationFile = ConfigurationManager.ResolveDefaultConfigurationFile()
            });
        }
    }
}
