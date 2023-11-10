using System.IO;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.TestApi;

namespace SqlDatabase.Configuration;

[TestFixture]
public class ConfigurationManagerTest
{
    public const string SomeConfiguration = @"
<configuration>
  <sqlDatabase getCurrentVersion='expected' />
</configuration>
";

    private TempDirectory _temp = null!;
    private ConfigurationManager _sut = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        _temp = new TempDirectory();
        _sut = new ConfigurationManager();
    }

    [TearDown]
    public void AfterEachTest()
    {
        _temp.Dispose();
    }

    [Test]
    public void LoadFromCurrentConfiguration()
    {
        _sut.LoadFrom((string?)null);

        _sut.SqlDatabase.ShouldNotBeNull();
        _sut.SqlDatabase.Variables.Keys.ShouldBe(new[] { nameof(ConfigurationManagerTest) });
        _sut.SqlDatabase.Variables[nameof(ConfigurationManagerTest)].ShouldBe(nameof(LoadFromCurrentConfiguration));
    }

    [Test]
    public void LoadFromEmptyFile()
    {
        var fileName = Path.Combine(_temp.Location, "app.config");
        File.WriteAllText(fileName, "<configuration />");

        _sut.LoadFrom(fileName);

        _sut.SqlDatabase.ShouldNotBeNull();
    }

    [Test]
    public void LoadFromFile()
    {
        var fileName = Path.Combine(_temp.Location, "app.config");
        File.WriteAllText(fileName, SomeConfiguration);

        _sut.LoadFrom(fileName);

        _sut.SqlDatabase.ShouldNotBeNull();
        _sut.SqlDatabase.GetCurrentVersionScript.ShouldBe("expected");
    }

    [Test]
    [TestCase(ConfigurationManager.Name1)]
    [TestCase(ConfigurationManager.Name2)]
    public void LoadFromDirectory(string fileName)
    {
        File.WriteAllText(Path.Combine(_temp.Location, fileName), SomeConfiguration);

        _sut.LoadFrom(_temp.Location);

        _sut.SqlDatabase.ShouldNotBeNull();
        _sut.SqlDatabase.GetCurrentVersionScript.ShouldBe("expected");
    }

    [Test]
    public void NotFoundInDirectory()
    {
        Assert.Throws<FileNotFoundException>(() => _sut.LoadFrom(_temp.Location));
    }

    [Test]
    public void FileNotFound()
    {
        var fileName = Path.Combine(_temp.Location, "app.config");

        Assert.Throws<IOException>(() => _sut.LoadFrom(fileName));
    }

    [Test]
    public void LoadInvalidConfiguration()
    {
        var fileName = Path.Combine(_temp.Location, "app.config");
        File.WriteAllText(fileName, "<configuration>");

        Assert.Throws<ConfigurationErrorsException>(() => _sut.LoadFrom(fileName));
    }
}