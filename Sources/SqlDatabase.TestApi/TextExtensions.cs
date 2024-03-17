using System.Text;

namespace SqlDatabase.TestApi;

public static class TextExtensions
{
    public static Func<Stream> AsFuncStream(this string text)
    {
        return () => new MemoryStream(Encoding.Default.GetBytes(text));
    }
}