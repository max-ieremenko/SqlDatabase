namespace SqlDatabase.Adapter.AssemblyScripts;

internal sealed class ConsoleListener : TextWriter
{
    private readonly TextWriter _original;
    private readonly ILogger _logger;

    public ConsoleListener(ILogger logger)
    {
        _logger = logger;

        _original = Console.Out;
        Console.SetOut(this);
    }

    public override Encoding Encoding => _original.Encoding;

    public override IFormatProvider FormatProvider => _original.FormatProvider;

    [AllowNull]
    public override string NewLine
    {
        get => _original.NewLine;
        set => _original.NewLine = value;
    }

    public override void Write(string? value)
    {
        _logger.Info(value ?? string.Empty);
    }

    public override void WriteLine(string? value)
    {
        _logger.Info(value ?? string.Empty);
    }

    protected override void Dispose(bool disposing)
    {
        Console.SetOut(_original);

        base.Dispose(disposing);
    }
}