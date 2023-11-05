using System;
using System.Collections;

namespace SqlDatabase.PowerShell.Internal;

internal readonly ref struct PSVersionTable
{
    public PSVersionTable(object value)
    {
        PSEdition = null;
        PSVersion = null;

        // https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_powershell_editions?view=powershell-7
        var source = (IEnumerable)value;
        foreach (DictionaryEntry entry in source)
        {
            var key = (string)entry.Key;
            if ("PSEdition".Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                PSEdition = Convert.ToString(entry.Value);
            }
            else if ("PSVersion".Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                PSVersion = Convert.ToString(entry.Value);
            }
        }
    }

    public string? PSEdition { get; }

    public string? PSVersion { get; }
}