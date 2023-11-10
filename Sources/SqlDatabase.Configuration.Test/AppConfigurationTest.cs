using NUnit.Framework;
using Shouldly;
using SqlDatabase.TestApi;

namespace SqlDatabase.Configuration;

[TestFixture]
public class AppConfigurationTest
{
    [Test]
    public void LoadEmpty()
    {
        var configuration = LoadFromResource("AppConfiguration.empty.xml");

        configuration.ShouldNotBeNull();
    }

    [Test]
    public void LoadDefault()
    {
        var configuration = LoadFromResource("AppConfiguration.default.xml");

        configuration.ShouldNotBeNull();

        configuration.GetCurrentVersionScript.ShouldBeNullOrEmpty();
        configuration.SetCurrentVersionScript.ShouldBeNullOrEmpty();

        configuration.AssemblyScript.ClassName.ShouldBeNullOrEmpty();
        configuration.AssemblyScript.MethodName.ShouldBeNullOrEmpty();

        configuration.Variables.Count.ShouldBe(0);

        configuration.MsSql.GetCurrentVersionScript.ShouldBeNullOrEmpty();
        configuration.MsSql.GetCurrentVersionScript.ShouldBeNullOrEmpty();
        configuration.MsSql.Variables.Count.ShouldBe(0);

        configuration.PgSql.GetCurrentVersionScript.ShouldBeNullOrEmpty();
        configuration.PgSql.GetCurrentVersionScript.ShouldBeNullOrEmpty();
        configuration.PgSql.Variables.Count.ShouldBe(0);

        configuration.MySql.GetCurrentVersionScript.ShouldBeNullOrEmpty();
        configuration.MySql.GetCurrentVersionScript.ShouldBeNullOrEmpty();
        configuration.MySql.Variables.Count.ShouldBe(0);
    }

    [Test]
    public void LoadFull()
    {
        var configuration = LoadFromResource("AppConfiguration.full.xml");

        configuration.ShouldNotBeNull();

        configuration.GetCurrentVersionScript.ShouldBe("get-version");
        configuration.SetCurrentVersionScript.ShouldBe("set-version");

        configuration.AssemblyScript.ClassName.ShouldBe("class-name");
        configuration.AssemblyScript.MethodName.ShouldBe("method-name");

        configuration.Variables.Keys.ShouldBe(new[] { "x", "y" });
        configuration.Variables["x"].ShouldBe("1");
        configuration.Variables["y"].ShouldBe("2");

        configuration.MsSql.GetCurrentVersionScript.ShouldBe("get-mssql-version");
        configuration.MsSql.SetCurrentVersionScript.ShouldBe("set-mssql-version");
        configuration.MsSql.Variables.Keys.ShouldBe(new[] { "mssql1" });
        configuration.MsSql.Variables["mssql1"].ShouldBe("10");

        configuration.PgSql.GetCurrentVersionScript.ShouldBe("get-pgsql-version");
        configuration.PgSql.SetCurrentVersionScript.ShouldBe("set-pgsql-version");
        configuration.PgSql.Variables.Keys.ShouldBe(new[] { "pgsql1" });
        configuration.PgSql.Variables["pgsql1"].ShouldBe("20");
    }

    private static AppConfiguration LoadFromResource(string resourceName)
    {
        using var source = ResourceReader.GetManifestResourceStream(resourceName);
        return ConfigurationReader.Read(source);
    }
}