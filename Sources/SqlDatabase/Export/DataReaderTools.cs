using System;

namespace SqlDatabase.Export
{
    internal static class DataReaderTools
    {
        public static object CleanValue(object value)
        {
            if (value == null || Convert.IsDBNull(value))
            {
                return null;
            }

            return value;
        }
    }
}
