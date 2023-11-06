using System;
using System.IO;
using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Configuration;
using SqlDatabase.TestApi;

namespace SqlDatabase.Scripts;

[TestFixture]
public class ScriptFactoryTest
{
    private ScriptFactory _sut = null!;
    private Mock<IPowerShellFactory> _powerShellFactory = null!;
    private AssemblyScriptConfiguration _configuration = null!;
    private Mock<ISqlTextReader> _textReader = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _configuration = new AssemblyScriptConfiguration();
        _powerShellFactory = new Mock<IPowerShellFactory>(MockBehavior.Strict);
        _textReader = new Mock<ISqlTextReader>(MockBehavior.Strict);

        _sut = new ScriptFactory(_configuration, _powerShellFactory.Object, _textReader.Object);
    }

    [Test]
    public void FromSqlFile()
    {
        var file = FileFactory.File("11.sql", "some script");

        _sut.IsSupported(file).ShouldBeTrue();

        var script = _sut.FromFile(file).ShouldBeOfType<TextScript>();

        script.DisplayName.ShouldBe("11.sql");
        script.TextReader.ShouldBe(_textReader.Object);
        new StreamReader(script.ReadSqlContent()).ReadToEnd().ShouldBe("some script");
    }

    [Test]
    public void FromPs1File()
    {
        var file = FileFactory.File(
            "11.ps1",
            "some script",
            FileFactory.Folder("name", FileFactory.File("11.txt", "3, 2, 1")));

        _powerShellFactory
            .Setup(f => f.Request());

        _sut.IsSupported(file).ShouldBeTrue();

        var script = _sut.FromFile(file).ShouldBeOfType<PowerShellScript>();

        script.PowerShellFactory.ShouldBe(_powerShellFactory.Object);
        script.DisplayName.ShouldBe("11.ps1");
        new StreamReader(script.ReadScriptContent()).ReadToEnd().ShouldBe("some script");
        new StreamReader(script.ReadDescriptionContent()!).ReadToEnd().ShouldBe("3, 2, 1");
        _powerShellFactory.VerifyAll();
    }

    [Test]
    public void FromPs1FileNotSupported()
    {
        var file = FileFactory.File("11.ps1", "some script");
        _sut.PowerShellFactory = null;

        _sut.IsSupported(file).ShouldBeFalse();

        Assert.Throws<NotSupportedException>(() => _sut.FromFile(file));
    }

    [Test]
    public void FromFileNotSupported()
    {
        var file = FileFactory.File("11.txt");

        _sut.IsSupported(file).ShouldBeFalse();
        Assert.Throws<NotSupportedException>(() => _sut.FromFile(file));
    }
}