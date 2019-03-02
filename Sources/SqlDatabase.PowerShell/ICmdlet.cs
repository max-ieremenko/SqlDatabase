namespace SqlDatabase.PowerShell
{
    internal interface ICmdlet
    {
        void WriteErrorLine(string value);

        void WriteLine(string value);
    }
}
