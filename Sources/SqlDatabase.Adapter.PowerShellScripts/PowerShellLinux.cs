using System.Globalization;

namespace SqlDatabase.Adapter.PowerShellScripts;

internal static class PowerShellLinux
{
    public static string GetInstallationPath() => "/opt/microsoft/powershell";

    public static bool IsExecutable(string fileName) => "pwsh".Equals(fileName, StringComparison.OrdinalIgnoreCase);

    public static int? GetParentProcessId(int processId)
    {
        // /proc/[pid]/stat https://man7.org/linux/man-pages/man5/procfs.5.html
        var fileName = "/proc/" + processId.ToString(CultureInfo.InvariantCulture) + "/stat";

        return ParseParentProcessId(fileName);
    }

    internal static int? ParseParentProcessId(string fileName)
    {
        string? line = null;

        try
        {
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true))
            {
                line = reader.ReadLine();
            }
        }
        catch
        {
        }

        if (string.IsNullOrWhiteSpace(line))
        {
            return null;
        }

        // (2) comm  %s: The filename of the executable, in parentheses.
        var startIndex = line!.LastIndexOf(')');
        if (startIndex <= 0 || startIndex >= line.Length)
        {
            return null;
        }

        // (3) state  %c
        startIndex = line.IndexOf(' ', startIndex + 1);
        if (startIndex <= 0 || startIndex >= line.Length)
        {
            return null;
        }

        // (4) ppid  %d: The PID of the parent of this process.
        startIndex = line.IndexOf(' ', startIndex + 1);
        if (startIndex <= 0 || startIndex >= line.Length)
        {
            return null;
        }

        var endIndex = line.IndexOf(' ', startIndex + 1);
        if (endIndex <= startIndex)
        {
            return null;
        }

        var ppid = line.Substring(startIndex + 1, endIndex - startIndex - 1);
        if (int.TryParse(ppid, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        return null;
    }
}