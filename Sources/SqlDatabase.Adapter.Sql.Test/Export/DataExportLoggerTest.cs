using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace SqlDatabase.Adapter.Sql.Export;

[TestFixture]
public class DataExportLoggerTest
{
    private DataExportLogger _sut = null!;
    private IList<string> _output = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _output = new List<string>();

        var logger = new Mock<ILogger>(MockBehavior.Strict);
        logger
            .Setup(l => l.Info(It.IsAny<string>()))
            .Callback<string>(m => _output.Add(m));
        logger
            .Setup(l => l.Error(It.IsAny<string>()))
            .Callback<string>(m => _output.Add(m));

        _sut = new DataExportLogger(logger.Object);
    }

    [Test]
    public void IgnoreInfo()
    {
        _sut.Info("some text");

        _output.ShouldBeEmpty();
    }

    [Test]
    public void SqlEscapeError()
    {
        _sut.Error("some text");

        _output.Count.ShouldBe(1);
        _output[0].ShouldBe("-- some text");
    }
}