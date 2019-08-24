Configuration file
==================

By default the current configuration file is "SqlDatabase.exe.config". It can be changed in a CLI
```bash
$ SqlDatabase ... -configuration=path\to\sql-database.config
```


<!-- toc -->
- [getCurrentVersion](#getCurrentVersion)
- [setCurrentVersion](#setCurrentVersion)
- [Example (get/set)CurrentVersion](#Example-(get/set)CurrentVersion)
- [assemblyScript](#assemblyScript)
- [variables](#variables)

<!-- tocstop -->


```xml
<configuration>
  <configSections>
    <section name="sqlDatabase"
             type="SqlDatabase.Configuration.AppConfiguration, SqlDatabase"/>
  </configSections>

  <sqlDatabase getCurrentVersion="SELECT value from sys.fn_listextendedproperty('version', default, default, default, default, default, default)"
               setCurrentVersion="EXEC sys.sp_updateextendedproperty @name=N'version', @value=N'{{TargetVersion}}'">
               
    <assemblyScript className="SqlDatabaseScript"
                    methodName="Execute" />
                    
    <variables>
      <add name="Variable1"
           value="value1" />
      <add name="Variable2"
           value="value 2" />
    </variables>
  </sqlDatabase>
</configuration>
```

## getCurrentVersion
An sql script to determine the current version of database, see [database upgrade](https://github.com/max-ieremenko/SqlDatabase/tree/master/Examples/MigrationStepsFolder).
Default value:
```sql
SELECT value from sys.fn_listextendedproperty('version', default, default, default, default, default, default)
```

## setCurrentVersion
An sql script to update the current version of database, see [database upgrade](https://github.com/max-ieremenko/SqlDatabase/tree/master/Examples/MigrationStepsFolder).
Default value:
```sql
EXEC sys.sp_updateextendedproperty @name=N'version', @value=N'{{TargetVersion}}'
```

## Example (get/set)CurrentVersion
The example shows an alternative how to store database version:
```sql
CREATE TABLE dbo.Version
(
    Name NVARCHAR(30) NOT NULL PRIMARY KEY
    ,Version NVARCHAR(30) NOT NULL
)
GO
```

```xml
<sqlDatabase getCurrentVersion="SELECT Version FROM dbo.Version WHERE Name = 'database'"
              setCurrentVersion="UPDATE dbo.Version SET Version = N'{{TargetVersion}}'' WHERE Name = 'database'" />
```

## assemblyScript
A configuration of [.NET Assembly scripts](https://github.com/max-ieremenko/SqlDatabase/tree/master/Examples/CSharpMirationStep).

* className - a script class name, default value is *SqlDatabaseScript*
* methodName - a method, entry point of *SqlDatabaseScript*

example
```C#
namespace <any namespace>
{
    public sealed class MyScript
    {
        public void MyEntryPoint(IDbCommand command, IReadOnlyDictionary<string, string> variables)
        {
            // write a message to a log
            Console.WriteLine("hello from my assembly script");
        }
    }
}
```
```xml
<sqlDatabase>
  <assemblyScript className="MyScript"
                  methodName="MyEntryPoint" />
</sqlDatabase>
```

## variables
A list of variables in format
```xml
<add name="a name of variable" value="a value of variable" />
<add name="a name of variable" value="a value of variable" />
<add name="a name of variable" value="a value of variable" />
```
by default the list is empty.

example
```xml
<sqlDatabase>
  <variables>
    <add name="SchemaName"
          value="demo" />
    <add name="TableName"
          value="Table1" />
  </variables>
</sqlDatabase>
```
```sql
-- script file *.sql
PRINT 'drop table {{SchemaName}}.{{TableName}}'
DROP TABLE [{{SchemaName}}].[{{TableName}}]

-- script at runtime
PRINT 'drop table demo.Table1'
DROP TABLE [demo].[Table1]
```
