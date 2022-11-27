using System.Configuration;
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

        configuration.ShouldBeNull();
    }

    [Test]
    public void LoadDefault()
    {
        var configuration = LoadFromResource("AppConfiguration.default.xml");

        configuration.ShouldNotBeNull();

        configuration.GetCurrentVersionScript.ShouldBeNullOrEmpty();
        configuration.SetCurrentVersionScript.ShouldBeNullOrEmpty();

        configuration.AssemblyScript.ClassName.ShouldBe("SqlDatabaseScript");
        configuration.AssemblyScript.MethodName.ShouldBe("Execute");

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

        configuration.Variables.AllKeys.ShouldBe(new[] { "x", "y" });
        configuration.Variables["x"].Value.ShouldBe("1");
        configuration.Variables["y"].Value.ShouldBe("2");

        configuration.MsSql.GetCurrentVersionScript.ShouldBe("get-mssql-version");
        configuration.MsSql.SetCurrentVersionScript.ShouldBe("set-mssql-version");
        configuration.MsSql.Variables.AllKeys.ShouldBe(new[] { "mssql1" });
        configuration.MsSql.Variables["mssql1"].Value.ShouldBe("10");

        configuration.PgSql.GetCurrentVersionScript.ShouldBe("get-pgsql-version");
        configuration.PgSql.SetCurrentVersionScript.ShouldBe("set-pgsql-version");
        configuration.PgSql.Variables.AllKeys.ShouldBe(new[] { "pgsql1" });
        configuration.PgSql.Variables["pgsql1"].Value.ShouldBe("20");
    }

    private static AppConfiguration LoadFromResource(string resourceName)
    {
        using (var temp = new TempDirectory())
        {
            var fileName = temp.CopyFileFromResources(resourceName);
            var configuration = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap { ExeConfigFilename = fileName }, ConfigurationUserLevel.None);
            return (AppConfiguration)configuration.GetSection(AppConfiguration.SectionName);
        }
    }
}