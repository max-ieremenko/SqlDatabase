using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Adapter;

namespace SqlDatabase.Log;

[TestFixture]
public class CombinedLoggerTest
{
    private CombinedLogger _sut = null!;
    private Mock<IDisposableLogger> _logger1 = null!;
    private Mock<IDisposableLogger> _logger2 = null!;

    public interface IDisposableLogger : ILogger, IDisposable
    {
    }

    [SetUp]
    public void BeforeEachTest()
    {
        _logger1 = new Mock<IDisposableLogger>(MockBehavior.Strict);
        _logger2 = new Mock<IDisposableLogger>(MockBehavior.Strict);
        _sut = new CombinedLogger(_logger1.Object, true, _logger2.Object, true);
    }

    [Test]
    public void Info()
    {
        _logger1.Setup(l => l.Info("some message"));
        _logger2.Setup(l => l.Info("some message"));

        _sut.Info("some message");

        _logger1.VerifyAll();
        _logger2.VerifyAll();
    }

    [Test]
    public void Error()
    {
        _logger1.Setup(l => l.Error("some message"));
        _logger2.Setup(l => l.Error("some message"));

        _sut.Error("some message");

        _logger1.VerifyAll();
        _logger2.VerifyAll();
    }

    [Test]
    public void Indent()
    {
        var indent1 = new Mock<IDisposable>(MockBehavior.Strict);
        var indent2 = new Mock<IDisposable>(MockBehavior.Strict);

        _logger1
            .Setup(l => l.Indent())
            .Returns(indent1.Object);
        _logger2
            .Setup(l => l.Indent())
            .Returns(indent2.Object);

        var actual = _sut.Indent();

        actual.ShouldNotBeNull();
        _logger1.VerifyAll();
        _logger2.VerifyAll();

        indent1.Setup(i => i.Dispose());
        indent2.Setup(i => i.Dispose());

        actual.Dispose();

        indent1.VerifyAll();
        indent2.VerifyAll();
    }

    [Test]
    public void DisposeOwned()
    {
        _logger1.Setup(l => l.Dispose());
        _logger2.Setup(l => l.Dispose());

        _sut.Dispose();

        _logger1.VerifyAll();
        _logger2.VerifyAll();
    }

    [Test]
    public void DisposeNotOwned()
    {
        _sut.OwnLogger1 = false;
        _sut.OwnLogger2 = false;

        _sut.Dispose();
    }
}