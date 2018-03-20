# SqlDatabase
This is a code for a SQL Server database migration tool.

#### Requirements

Microsoft [.NET Framework 4.5.2](https://www.microsoft.com/en-us/download/details.aspx?id=42642) or higher.

#### Download

Download the [latest release](https://github.com/max-ieremenko/SqlDatabase/releases).

#### CLI
```bash
$ SqlDatabase.exe upgrade "-database=Data Source=server;Initial Catalog=MyDatabase;Integrated Security=True" -from=c:\MyDatabase\upgrade
```
|Switch|Description|
|:--:|:----------|
|-database|set connection string to target database|
|-from|path to a folder or .zip file with migration steps|
|-transaction|set transaction mode (none, perStep). Option [none] is default, means no trasactions. Option [perStep] means to use one transaction per each migration step|

Exit codes
* 0 - OK
* 1 - invalid command line
* 2 - there are errors during migration


#### Content example of a folder or .zip file with migration steps
|File|Description|
|:--:|:----------|
|1.0_1.3.zip|.zip file with archived migration steps from database version 1.0 to 1.3 (inclusive).|
|1.3_2.0.sql|the database migration step from version 1.3 to 2.0|
|2.0_2.1.sql|the database migration step from version 2.0 to 2.1|
|2.1_2.2.dll|the database migration step from version 2.1 to 2.2|
|2.2_2.3.exe|the database migration step from version 2.2 to 2.3|
|2.3_3.0.sql|the database migration step from version 2.3 to 3.0|

see [example](Examples/MigrationStepsFolder/)


#### Content example of .sql miration step, 1.3_2.0.sql
```sql
PRINT 'create schema demo'
CREATE SCHEMA [demo]
GO

PRINT 'create table demo.Table1'
CREATE TABLE [demo].[Table1]
(
  ID INT
)
GO

-- etc.
```


#### Content example of .dll or .exe miration step, 2.1_2.2.dll or 2.2_2.3.exe
The file must be an .NET assembly with following migration step implementation:
```C#
namespace <any namespace name>
{
    public /*sealed*/ class SqlDatabaseScript /*: IDisposable*/
    {
        public void Execute(IDbCommand command, IReadOnlyDictionary<string, string> variables)
        {
            // write a message to an migration log
            Console.WriteLine("start execution");

            // execute a query
            command.CommandText = string.Format("print 'current database name is {0}'", variables["DatabaseName"]);
            command.ExecuteNonQuery();

            // execute a query
            command.CommandText = 'CREATE SCHEMA [demo]';
            command.ExecuteNonQuery();

            // write a message to an migration log
            Console.WriteLine("finish execution");
        }
    }
}
```
see [example](Examples/CSharpMirationStep/)


#### Determining and updating the database version
Sql script to determinine current version
```sql
SELECT value from sys.fn_listextendedproperty('version', default, default, default, default, default, default)
```
Sql script to update current version
```sql
EXEC sys.sp_updateextendedproperty @name=N'version', @value=N'{{TargetVersion}}'
```
Both scripts are configurable in SqlDatabase.exe.config, see [example](Examples/ConfigurationFile/)


#### Variables in .sql migartion steps
Any entry like *{{VariableName}}* is interpreted as variable and has to be changed (text replacement) with active value before script execution. The name is case insensitive.
Non defined value of a variable leads to and error and stops migration execution.

The value is resolving in the following order:
1. TODO
2. check environment variable (Environment.GetEnvironmentVariable())

Predefined variables:

|Name|Description|
|:--:|:----------|
|DatabaseName|the target database name|
|CurrentVersion|the database version before execution of a migration step|
|TargetVersion|the database version after execution of a migration step|


#### License
This tool is distributed under the [MIT](LICENSE) license.
