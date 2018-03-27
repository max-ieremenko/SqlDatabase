#### CLI
```bash
$ SqlDatabase.exe upgrade "-database=Data Source=MyServer;Initial Catalog=MyDatabase;Integrated Security=True" -from=Examples\MigrationStepsFolder -varVariable1=value1 -varVariable2=value2
```
upgrade existing database *MyDatabase* on Sql Server *MyServer* based on scripts from *Examples\MigrationStepsFolder* with "Variable1=value1" and "Variable2=value2"

|Switch|Description|
|:--|:----------|
|-database|set connection string to target database|
|-from|path to a folder or .zip file with migration steps|
|-transaction|set transaction mode (none, perStep). Option [none] is default, means no trasactions. Option [perStep] means to use one transaction per each migration step|
|[-var]|set a variable in format "=var[name of variable]=[value of variable]"|


#### Example of .sql miration step, 1.3_2.0.sql
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


#### Example of .dll or .exe miration step, 2.1_2.2.dll or 2.2_2.3.exe
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

            // write a message to a log
            Console.WriteLine("finish execution");
        }
    }
}
```
see [example](CSharpMirationStep/)

#### Predefined variables
|Name|Description|
|:--|:----------|
|DatabaseName|the target database name|
|CurrentVersion|the database version before execution of a migration step|
|TargetVersion|the database version after execution of a migration step|
