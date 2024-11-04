using System.Management.Automation;

namespace SqlDatabase.PowerShell.Internal;

internal static class PowerShellCommand
{
    private static CmdLetExecutorInvoker? _invoker;

    public static void Execute(PSCmdlet cmdlet, string methodName, IDictionary<string, object?> param)
    {
        var logger = new CmdLetLogger(cmdlet);
        try
        {
            GetInvoker(cmdlet).Invoke(logger, cmdlet.GetWorkingDirectory(), methodName, param);
        }
        catch (Exception ex)
        {
            logger.Error(ex.Message);
            throw;
        }
    }

    public static string GetDefaultConfigurationFile(PSCmdlet cmdlet) => GetInvoker(cmdlet).GetDefaultConfigurationFile();

    private static CmdLetExecutorInvoker GetInvoker(PSCmdlet cmdlet)
    {
        if (_invoker == null)
        {
            _invoker = new CmdLetExecutorInvoker(DependencyResolverFactory.Create(cmdlet));
        }

        return _invoker;
    }
}