using System;
using System.Configuration;
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
  <configSections>
    <section name='sqlDatabase'
            type = 'SqlDatabase.Configuration.AppConfiguration, SqlDatabase' />
  </configSections>

  <sqlDatabase getCurrentVersion='expected' />

</configuration>
";

    private ConfigurationManager _sut;

    [SetUp]
    public void BeforeEachTest()
    {
        _sut = new ConfigurationManager();
    }

    [Test]
    public void LoadFromCurrentConfiguration()
    {
        _sut.LoadFrom((string)null);

        _sut.SqlDatabase.ShouldNotBeNull();
        _sut.SqlDatabase.Variables.AllKeys.ShouldBe(new[] { nameof(ConfigurationManagerTest) });
        _sut.SqlDatabase.Variables[nameof(ConfigurationManagerTest)].Value.ShouldBe(nameof(LoadFromCurrentConfiguration));
    }

    [Test]
    public void LoadFromEmptyFile()
    {
        var file = FileFactory.File("app.config", "<configuration />");

        _sut.LoadFrom(file);

        Assert.IsNotNull(_sut.SqlDatabase);
        Assert.AreEqual(new AppConfiguration().GetCurrentVersionScript, _sut.SqlDatabase.GetCurrentVersionScript);
    }

    [Test]
    public void LoadFromFile()
    {
        var file = FileFactory.File("app.config", SomeConfiguration);

        _sut.LoadFrom(file);

        Assert.IsNotNull(_sut.SqlDatabase);
        Assert.AreEqual("expected", _sut.SqlDatabase.GetCurrentVersionScript);
    }

    [Test]
    public void LoadFromDirectory()
    {
        // SqlDatabase.exe.config or SqlDatabase.dll.config
        var fileName = Path.GetFileName(_sut.GetType().Assembly.Location) + ".config";
        var file = FileFactory.File(fileName, SomeConfiguration);
        var folder = FileFactory.Folder("some folder", file);

        _sut.LoadFrom(folder);

        Assert.IsNotNull(_sut.SqlDatabase);
        Assert.AreEqual("expected", _sut.SqlDatabase.GetCurrentVersionScript);
    }

    [Test]
    public void NotFoundInDirectory()
    {
        var folder = FileFactory.Folder("some folder");

        Assert.Throws<FileNotFoundException>(() => _sut.LoadFrom(folder));
    }

    [Test]
    public void FileNotFound()
    {
        var file = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        Assert.Throws<IOException>(() => _sut.LoadFrom(file));
    }

    [Test]
    public void LoadInvalidConfiguration()
    {
        var file = FileFactory.File("app.config", "<configuration>");

        Assert.Throws<ConfigurationErrorsException>(() => _sut.LoadFrom(file));
    }
}