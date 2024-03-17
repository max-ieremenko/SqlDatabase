namespace SqlDatabase.Log;

internal sealed class DisposableAction : IDisposable
{
    private Action? _action;

    public DisposableAction(Action action)
    {
        _action = action;
    }

    public void Dispose()
    {
        var a = _action;
        _action = null;
        a?.Invoke();
    }
}