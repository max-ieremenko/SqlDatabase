namespace SqlDatabase
{
    public interface ILogger
    {
        void Error(string message);

        void Info(string message);
    }
}