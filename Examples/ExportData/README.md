Export data from a database to sql script (file)
=================

```bash
$ SqlDatabase export ^
      "-database=Data Source=server;Initial Catalog=database;Integrated Security=True" ^
      "-fromSql=SELECT * FROM sys.databases" ^
      -toTable=#databases ^
      -toFile=c:\databases.sql

PS> Export-SqlDatabase `
      -database "Data Source=server;Initial Catalog=database;Integrated Security=True" `
      -fromSql "SELECT * FROM sys.databases" `
      -toFile c:\databases.sql `
      -toTable #databases `
      -InformationAction Continue
```

export data from sys.databases view into "c:\databases.sql" from "MyDatabase" on "server"

#### c:\databases.sql

```sql
CREATE TABLE #databases
(
    [name] NVARCHAR(128) NOT NULL
    ,[database_id] INT NOT NULL
    ,[source_database_id] INT NULL
    ,[owner_sid] VARBINARY(85) NULL
    ,[create_date] DATETIME NOT NULL
    ,[compatibility_level] TINYINT NOT NULL
    ,[collation_name] NVARCHAR(128) NULL
    ,[user_access] TINYINT NULL
    ,[user_access_desc] NVARCHAR(60) NULL
    ,[is_read_only] BIT NULL
    ,[is_auto_close_on] BIT NOT NULL
    ,[is_auto_shrink_on] BIT NULL
    ,[state] TINYINT NULL
    -- ...
)
GO

INSERT INTO #databases([name], [database_id], [source_database_id], [owner_sid], [create_date], [compatibility_level], [collation_name], [user_access], [user_access_desc], [is_read_only], [is_auto_close_on], [is_auto_shrink_on], [state] /* ... */)
VALUES (N'master', 1, NULL, 0x01, '2003-04-08 09:13:36:390', 140, N'SQL_Latin1_General_CP1_CI_AS', 0, N'MULTI_USER', 0, 0, 0, 0 /* ... */)
      ,(N'tempdb', 2, NULL, 0x01, '2019-04-25 16:11:04:140', 140, N'SQL_Latin1_General_CP1_CI_AS', 0, N'MULTI_USER', 0, 0, 0, 0 /* ... */)
      -- ...
GO
```

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
