using System.IO;
using System.Management.Automation;
using SqlDatabase.Configuration;

namespace SqlDatabase.PowerShell
{
    internal static class CmdletExtensions
    {
        public static string GetWorkingDirectory(this PSCmdlet cmdlet)
        {
            var root = cmdlet.MyInvocation.PSScriptRoot;
            if (string.IsNullOrEmpty(root))
            {
                root = cmdlet.CurrentProviderLocation("FileSystem").ProviderPath;
            }

            return root;
        }

        public static string RootPath(this PSCmdlet cmdlet, string path)
        {
            if (string.IsNullOrEmpty(path) || Path.IsPathRooted(path))
            {
                return path;
            }

            return Path.Combine(GetWorkingDirectory(cmdlet), path);
        }

        public static bool TryGetPSVersionTable(this PSCmdlet cmdlet, out PSVersionTable value)
        {
            var psVersionTable = cmdlet.GetVariableValue("PSVersionTable");
            if (psVersionTable == null)
            {
                value = default;
                return false;
            }

            value = new PSVersionTable(psVersionTable);
            return true;
        }

        public static void AppendFrom(this PSCmdlet cmdlet, string[] from, GenericCommandLineBuilder target)
        {
            if (from != null)
            {
                for (var i = 0; i < from.Length; i++)
                {
                    target.SetScripts(cmdlet.RootPath(from[i]));
                }
            }
        }
    }
}
