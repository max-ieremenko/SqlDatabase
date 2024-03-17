using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.FileSystem;
using SqlDatabase.TestApi;

namespace SqlDatabase.Adapter.PowerShellScripts;

[TestFixture]
public class PowerShellScriptFactoryTest
{
    private Mock<IPowerShellFactory> _powerShell = null!;
    private PowerShellScriptFactory _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _powerShell = new Mock<IPowerShellFactory>(MockBehavior.Strict);
        _sut = new PowerShellScriptFactory(_powerShell.Object);
    }

    [Test]
    [TestCase(".Ps1", true)]
    [TestCase(".sql", false)]
    [TestCase(null, false)]
    public void IsSupported(string? ext, bool expected)
    {
        var file = new Mock<IFile>(MockBehavior.Strict);
        file
            .SetupGet(f => f.Extension)
            .Returns(ext!);

        _sut.IsSupported(file.Object).ShouldBe(expected);
    }

    [Test]
    public void FromFile()
    {
        var file = FileFactory.File(
            "11.ps1",
            "some script",
            FileFactory.Folder("name", FileFactory.File("11.txt", "3, 2, 1")));

        var script = _sut.FromFile(file).ShouldBeOfType<PowerShellScript>();

        script.DisplayName.ShouldBe("11.ps1");
        script.PowerShellFactory.ShouldBe(_powerShell.Object);
        new StreamReader(script.ReadScriptContent()).ReadToEnd().ShouldBe("some script");
        new StreamReader(script.ReadDescriptionContent()!).ReadToEnd().ShouldBe("3, 2, 1");
    }
}