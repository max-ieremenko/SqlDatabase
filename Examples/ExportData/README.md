Export data from a database to sql script (file)
=================

```bash
$ SqlDatabase export ^
      "-database=Data Source=server;Initial Catalog=master;Integrated Security=True" ^
      "-fromSql=SELECT * FROM sys.tables" ^
      -toTable=#tables ^
      -toFile=c:\tables.sql

PS> Export-SqlDatabase `
      -database "Data Source=server;Initial Catalog=master;Integrated Security=True" `
      -fromSql "SELECT * FROM sys.tables" `
      -toFile c:\tables.sql `
      -toTable #tables `
      -InformationAction Continue
```

export data from sys.tables view into "c:\tables.sql" from "master" database on "server"

#### c:\tables.sql

```sql
CREATE TABLE #tables
(
    [name] NVARCHAR(128) NOT NULL
    ,[object_id] INT NOT NULL
    ,[principal_id] INT NULL
    ,[schema_id] INT NOT NULL
    ,[parent_object_id] INT NOT NULL
    ,[type] CHAR(2) NULL
    ,[type_desc] NVARCHAR(60) NULL
    ,[create_date] DATETIME NOT NULL
    ,[modify_date] DATETIME NOT NULL
    ,[is_ms_shipped] BIT NOT NULL
    ,[is_published] BIT NOT NULL
    ,[is_schema_published] BIT NOT NULL
    ,[lob_data_space_id] INT NOT NULL
    -- ...
)
GO

INSERT INTO #tables([name], [object_id], [principal_id], [schema_id], [parent_object_id], [type], [type_desc], [create_date], [modify_date], [is_ms_shipped], [is_published], [is_schema_published], [lob_data_space_id] /* ... */)
VALUES (N'spt_fallback_db', 117575457, NULL, 1, 0, N'U ', N'USER_TABLE', '2003-04-08 09:18:01:557', '2018-11-30 15:06:04:520', 1, 0, 0, 0 /* ... */)
      ,(N'spt_fallback_dev', 133575514, NULL, 1, 0, N'U ', N'USER_TABLE', '2003-04-08 09:18:02:870', '2018-11-30 15:06:04:523', 1, 0, 0, 0 /* ... */)
      ,(N'spt_fallback_usg', 149575571, NULL, 1, 0, N'U ', N'USER_TABLE', '2003-04-08 09:18:04:180', '2018-11-30 15:06:04:527', 1, 0, 0, 0 /* ... */)
      ,(N'spt_monitor', 1803153469, NULL, 1, 0, N'U ', N'USER_TABLE', '2018-11-30 15:04:02:047', '2018-11-30 15:06:04:533', 1, 0, 0, 0 /* ... */)
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
|-var|set a variable in format "=var[name of variable]=[value of variable]"|

#### -from

```bash
# execute data selections scripts from file Script.sql
-from=C:\MyDatabase\Script.sql

# execute scripts from file Script.sql in MyDatabase.zip archive
-from=C:\MyDatabase.zip\Script.sql
```

#### -fromSql

```bash
"-fromSql=SELECT * FROM sys.tables"

"-fromSql=SELECT * FROM [{{Schema}}].[{{Table}}]" -varSchema=sys -varTable=tables

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
-from=script.sql -varSchema=sys -varTable=tables

# output
script.sql ...
   variable Schema was replaced with sys
   variable Table was replaced with tables
```

```sql
-- script at runtime
SELECT * FROM sys.tables
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
