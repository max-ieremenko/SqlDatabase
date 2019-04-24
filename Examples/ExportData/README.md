Export data from a database to sql script (file)
=================

```bash
$ SqlDatabase export ^
      "-database=Data Source=server;Initial Catalog=database;Integrated Security=True" ^
      "-fromSql=SELECT * FROM sys.databases" ^
      -toFile=c:\databases.sql

PS> Export-SqlDatabase `
      -database "Data Source=server;Initial Catalog=database;Integrated Security=True" `
      -fromSql "SELECT * FROM sys.databases" `
      -toFile c:\databases.sql `
      -InformationAction Continue
```

export data from sys.databases view into "c:\databases.sql" from "MyDatabase" on "server"

CLI
===

|Option|Description|
|:--|:----------|
|-database|set connection string to target database|
|-from|a path to a folder or zip archive with sql scripts or path to a sql script file. Repeat -from to setup several sources.|
|-fromSql|an sql script to select export data. Repeat -fromSql to setup several scripts.|
|-toTable|setup "INSERT INTO" table name. Default is dbo.SqlDatabaseExport.|
|-toFile|write sql scripts into a file. By default write into standard output (console/information stream).|
|-configuration|a path to application configuration file. Default is current [SqlDatabase.exe.config](https://github.com/max-ieremenko/SqlDatabase/tree/master/Examples/ConfigurationFile)|
|[-var]|set a variable in format "=var[name of variable]=[value of variable]"|

#### -from

```bash
# execute data selections scripts from file Script.sql
-from=C:\MyDatabase\Script.sql

# execute scripts from file Script.sql in MyDatabase.zip archive
-from=C:\MyDatabase.zip\Script.sql
```

#### -fromSql

```bash
"-fromSql=SELECT * FROM sys.databases"

"-fromSql=SELECT * FROM [{{Schema}}].[{{Table}}]" -varSchema=sys -varTable=databases

"-fromSql=SELECT 1"
```

#### -toTable

```bash
-toTable=#tmp
-toTable=schema.Table
```

#### -var

```sql
-- script.sql
SELECT * FROM [{{Schema}}].[{{Table}}]
```

```bash
# execute script.sql
-from=script.sql -varSchema=sys -varTable=databases

# output
script.sql ...
   variable Schema was replaced with dbo
   variable Table was replaced with Person
```

```sql
-- script at runtime
SELECT * FROM sys.databases
```

#### Exit codes
* 0 - OK
* 1 - invalid command line
* 2 - errors during execution

Predefined variables
========================

|Name|Description|
|:--|:----------|
|DatabaseName|the target database name|
