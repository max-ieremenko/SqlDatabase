namespace SqlDatabase.TestApi;

public static class TestOutput
{
    [Conditional("DEBUG")]
    public static void WriteLine() => Console.WriteLine();

    [Conditional("DEBUG")]
    public static void WriteLine(string? value) => Console.WriteLine(value);

    [Conditional("DEBUG")]
    public static void WriteLine(object? value) => Console.WriteLine(value);

    [Conditional("DEBUG")]
    public static void WriteLine(string format, object? arg0) => Console.WriteLine(format, arg0);
}