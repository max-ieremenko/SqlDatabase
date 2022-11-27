using System.Globalization;

namespace SqlDatabase;

internal static class StringExtensions
{
    public static string FormatWith(this string format, params object[] args)
    {
        return string.Format(CultureInfo.InvariantCulture, format, args);
    }
}