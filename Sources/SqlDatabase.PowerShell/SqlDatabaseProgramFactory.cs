using System;
using System.Collections;
using System.Linq;

namespace SqlDatabase.PowerShell
{
    internal static class SqlDatabaseProgramFactory
    {
        public static ISqlDatabaseProgram CreateProgram(IEnumerable psVersionTable, ICmdlet owner)
        {
            // https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_powershell_editions?view=powershell-7
            var psEdition = (string)psVersionTable
                .Cast<DictionaryEntry>()
                .FirstOrDefault(i => "PSEdition".Equals((string)i.Key, StringComparison.OrdinalIgnoreCase))
                .Value;

            // In PowerShell 4 and below, this variable does not exist
            if (string.IsNullOrEmpty(psEdition) || "Desktop".Equals(psEdition, StringComparison.OrdinalIgnoreCase))
            {
                return new SqlDatabaseProgramNet452(owner);
            }

            return new SqlDatabaseProgramNetCore(owner);
        }
    }
}
