#### CLI
```bash
$ SqlDatabase.exe create
      "-database=Data Source=MyServer;Initial Catalog=MyDatabase;Integrated Security=True"
	  -from=Examples\MigrationStepsFolder
	  -varVariable1=value1
	  -varVariable2=value2
```
create new database *MyDatabase* on Sql Server *MyServer* based on scripts from *Examples\CreateDatabaseFolder* with "Variable1=value1" and "Variable2=value2"

|Switch|Description|
|:--|:----------|
|-database|set connection string to target database|
|-from|path to a folder or .zip file with scripts|
|[-var]|set a variable in format "=var[name of variable]=[value of variable]"|


#### Example of .sql script
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


#### Example of .dll or .exe script
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
see [example](../CSharpMirationStep/)

#### Script`s execution order
1. all script`s files in the root folder, sorted alphabetically
2. all script`s in each sub-folder, sorted alphabetically

|File|Execution order|
|:--|:----------|
|01_folder||
|├── 01_script.sql|3|
|├── 02_script.exe|4|
|└── 03_script.sql|5|
|02_folder||
|├── 02_folder||
|│   └── 01_script.sql|7|
|└── 01_script.sql|6|
|01_script.sql|2|
|02_script.sql|1|


#### Predefined variables
|Name|Description|
|:--|:----------|
|DatabaseName|the target database name|
