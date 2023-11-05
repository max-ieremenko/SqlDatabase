using System;
using Moq;
using SqlDatabase.Scripts;
using SqlDatabase.Scripts.PowerShellInternal;

namespace SqlDatabase.TestApi;

internal static class TestPowerShellHost
{
    public static IPowerShellFactory GetOrCreateFactory()
    {
        if (PowerShellFactory.SharedTestFactory == null)
        {
            PowerShellFactory.SharedTestFactory = CreateFactory();
        }

        return PowerShellFactory.SharedTestFactory;
    }

    private static IPowerShellFactory CreateFactory()
    {
        var logger = new Mock<ILogger>(MockBehavior.Strict);
        logger
            .Setup(l => l.Info(It.IsNotNull<string>()))
            .Callback<string>(m => Console.WriteLine("info: " + m));
        logger
            .Setup(l => l.Error(It.IsNotNull<string>()))
            .Callback<string>(m => Console.WriteLine("error: " + m));
        logger
            .Setup(l => l.Indent())
            .Returns((IDisposable)null!);

        var factory = PowerShellFactory.Create(null);
        factory.Request();
        factory.InitializeIfRequested(logger.Object);
        return factory;
    }
}