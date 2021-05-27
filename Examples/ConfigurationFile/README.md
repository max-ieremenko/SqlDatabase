Configuration file
==================

By default the current configuration file is [SqlDatabase.dll.config](SqlDatabase.dll.config). The file is located in the installation folder. It can be changed in CLI

```bash
$ SqlDatabase ... -configuration=path\to\sql-database.config
```

## File example

```xml
<configuration>
  <configSections>
    <section name="sqlDatabase"
             type="SqlDatabase.Configuration.AppConfiguration, SqlDatabase"/>
  </configSections>

  <sqlDatabase>
               
    <assemblyScript className="SqlDatabaseScript"
                    methodName="Execute" />

    <!-- variables applicable for mssql and pgsql -->
    <variables>
      <add name="Variable1"
           value="value1" />
      <add name="Variable2"
           value="value 2" />
    </variables>

    <!-- mssql configuration:
    - default scripts to read and update database version
    - few predefined variables, applicable only for mssql  -->
    <mssql getCurrentVersion="SELECT value from sys.fn_listextendedproperty('version', default, default, default, default, default, default)"
           setCurrentVersion="EXEC sys.sp_updateextendedproperty @name=N'version', @value=N'{{TargetVersion}}'">
      <variables>
        <add name="MsSqlVariable1"
             value="value1" />
        <add name="MsSqllVariable2"
             value="value 2" />
      </variables>
    </mssql>

    <!-- pgsql configuration:
    - default scripts to read and update database version
    - few predefined variables, applicable only for pgsql  -->
    <pgsql getCurrentVersion="SELECT version FROM public.version WHERE module_name = 'database'"
           setCurrentVersion="UPDATE public.version SET version='{{TargetVersion}}' WHERE module_name = 'database'">
      <variables>
        <add name="PgSqlVariable1"
             value="value1" />
        <add name="PgSqllVariable2"
             value="value 2" />
      </variables>
    </pgsql>
  </sqlDatabase>
</configuration>
```

## getCurrentVersion

An sql script to determine the current version of database, see [database upgrade](../MigrationStepsFolder).

Default value for MSSQL Server (sqlDatabase/mssql/@getCurrentVersion):

```sql
SELECT value from sys.fn_listextendedproperty('version', default, default, default, default, default, default)
```

Default value for PostgreSQL (sqlDatabase/pgsql/@getCurrentVersion):

```sql
SELECT version FROM public.version WHERE module_name = 'database'
```

Warn: SqlDatabase does not validate the provided script, please make sure that script is working before running SqlDatabase.

## setCurrentVersion

An sql script to update the current version of database, see [database upgrade](../MigrationStepsFolder).

Default value for MSSQL Server (sqlDatabase/mssql/@setCurrentVersion):

```sql
EXEC sys.sp_updateextendedproperty @name=N'version', @value=N'{{TargetVersion}}'
```

Default value for PostgreSQL (sqlDatabase/pgsql/@setCurrentVersion):

```sql
UPDATE public.version SET version='{{TargetVersion}}' WHERE module_name = 'database'
```

Warn: SqlDatabase does not validate the provided script, please make sure that script is working before running SqlDatabase.

## assemblyScript

A configuration of [.NET Assembly scripts](../CSharpMirationStep).

* className - a script class name, default value is *SqlDatabaseScript*
* methodName - a method, entry point of *SqlDatabaseScript*, default value is *Execute*

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

sections

* sqlDatabase/variables
* sqlDatabase/mssql/variables
* sqlDatabase/pgsql/variables

A list of variables in format

```xml
<add name="a name of variable" value="a value of variable" />
<add name="a name of variable" value="a value of variable" />
<add name="a name of variable" value="a value of variable" />
```

by default all lists are empty.

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
