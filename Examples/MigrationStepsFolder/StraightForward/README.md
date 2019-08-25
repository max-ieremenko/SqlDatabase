Straight forward upgrade
===========================

Step file name <a name="file-name"></a>
===
A file name must be in the format "[version from]\_[version to].[extension]", for example

|File|Version from|Version to|Extension|
|:--|:----------|:----------|:----------|
|1.0_2.0.sql|1.0|2.0|.sql|
|2.1_2.2.exe|2.1|2.2|.exe|
|1.0_2.0.dll|1.0|2.0|.dll|

Select/update a database version <a name="module-version"></a>
===
Scripts for resolving and updating a database version are defined in the [configuration file](https://github.com/max-ieremenko/SqlDatabase/tree/master/Examples/ConfigurationFile).

Execution
===
In the current folder are migration steps
- 1.0_1.1.sql - creates a table "dbo.Person"
- 1.1_2.0.sql - creates a table "dbo.Book"
- 2.0_3.0.sql - creates a table "dbo.BookComment"

The folder structure does not matter, SqlDatabase analyzes all files and folders recursively.

#### runtime
1. Load all migartion steps
2. Resolve the current version of database: 1.0
4. build migration sequence: 1.0 => 1.1; 1.1 => 2.0; 2.0 => 3.0
5. Execute each step executed one by one:

```sql
/* 1.0 => 1.1 */
execute 1.0_1.1.sql
-- update database version
EXEC sys.sp_updateextendedproperty @name=N'version', @value=N'1.1'

/* 1.1 => 2.0 */
execute 1.1_2.0.sql
-- update database version
EXEC sys.sp_updateextendedproperty @name=N'version', @value=N'2.0'

/* 2.0 => 3.0 */
execute 2.0_3.0.sql
-- update database version
EXEC sys.sp_updateextendedproperty @name=N'version', @value=N'3.0'
```

-whatIf option <a name="whatIf"></a>
===
Use SqlDatabase -whatIf to list all steps and test upgrade sequence.
