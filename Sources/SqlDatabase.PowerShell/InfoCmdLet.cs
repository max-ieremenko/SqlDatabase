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

            WriteObject(new
            {
                Version = assembly.GetName().Version,
                RuntimeInformation.FrameworkDescription,
                RuntimeInformation.OSDescription,
                Location = Path.GetDirectoryName(assembly.Location),
                DefaultConfigurationFile = ConfigurationManager.ResolveDefaultConfigurationFile()
            });
        }
    }
}
