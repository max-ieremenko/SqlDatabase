using System;

namespace SqlDatabase.PowerShell
{
    internal static class SqlDatabaseProgramFactory
    {
        public static ISqlDatabaseProgram CreateProgram(PSVersionTable psVersionTable, ICmdlet owner)
        {
            // In PowerShell 4 and below, this variable does not exist
            if (string.IsNullOrEmpty(psVersionTable.PSEdition) || "Desktop".Equals(psVersionTable.PSEdition, StringComparison.OrdinalIgnoreCase))
            {
                return new SqlDatabaseProgramNet452(owner);
            }

            return new SqlDatabaseProgramNetCore(owner);
        }
    }
}
