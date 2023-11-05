using System.Collections.Generic;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace SqlDatabase.Log;

[TestFixture]
public class LoggerBaseTest
{
    private IList<string> _info = null!;
    private IList<string> _error = null!;
    private LoggerBase _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _info = new List<string>();
        _error = new List<string>();

        var logger = new Mock<LoggerBase>(MockBehavior.Strict);
        logger
            .Protected()
            .Setup("WriteError", ItExpr.IsAny<string>())
            .Callback<string>(_error.Add);
        logger
            .Protected()
            .Setup("WriteInfo", ItExpr.IsAny<string>())
            .Callback<string>(_info.Add);

        _sut = logger.Object;
    }

    [Test]
    public void Info()
    {
        _sut.Info("the text");

        Assert.AreEqual(1, _info.Count);
        Assert.AreEqual(0, _error.Count);
        Assert.AreEqual("the text", _info[0]);
    }

    [Test]
    public void Error()
    {
        _sut.Error("the text");

        Assert.AreEqual(0, _info.Count);
        Assert.AreEqual(1, _error.Count);
        Assert.AreEqual("the text", _error[0]);
    }

    [Test]
    public void NoIndentOnError()
    {
        _sut.Indent();
        _sut.Error("the text");

        Assert.AreEqual("the text", _error[0]);
    }

    [Test]
    public void IndentInfo()
    {
        _sut.Info("1-");

        using (_sut.Indent())
        {
            _sut.Info("2-");

            using (_sut.Indent())
            {
                _sut.Info("3");
            }

            _sut.Info("2+");
        }

        Assert.AreEqual(4, _info.Count);
        Assert.AreEqual("1-", _info[0]);
        Assert.AreEqual("   2-", _info[1]);
        Assert.AreEqual("      3", _info[2]);
        Assert.AreEqual("   2+", _info[3]);
    }
}