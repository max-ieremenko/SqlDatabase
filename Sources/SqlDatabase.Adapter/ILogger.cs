namespace SqlDatabase.Adapter;

public interface ILogger
{
    void Error(string message);

    void Info(string message);

    IDisposable Indent();
}