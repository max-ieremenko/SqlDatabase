Modularity upgrade
===========================

Table of Contents
-----------------

<!-- toc -->

- [Step file name](#file-name)
- [Step dependencies](#dependencies)
- [Select/update a module version](#module-version)
- [Execution](#execution)
- [SqlDatabase.exe -whatIf](#whatIf)

<!-- tocstop -->

Step file name <a name="file-name"></a>
===
A file name must be in the format "[module name]\_[version from]\_[version to].[extension]", for example

|File|Module name|Version from|Version to|Extension|
|:--|:----------|:----------|:----------|:----------|
|moduleA_1.0_2.0.sql|moduleA|1.0|2.0|.sql|
|moduleA_2.1_2.2.exe|moduleA|2.1|2.2|.exe|
|moduleB_1.0_2.0.dll|moduleB|1.0|2.0|.dll|

[Back to ToC](#table-of-contents)

Step dependencies <a name="dependencies"></a>
===
Dependencies are optional. They are written as a special type of comment in the form "module dependency: [module name] [module version]". Module names are not case-sensitive.

#### .sql step
Once GO instruction has been processed, SqlDatabase no longer looks for dependencies. Therefore, all dependencies must be at the very top of a script.

The following step depends on module A version 2.0 and module B version 1.0.
```sql
/*
* module dependency: a 2.0
* module dependency: b 1.0
*/
GO
...
```

The following step depends on module A version 2.0.
```sql
-- module dependency: a 2.0
GO
...
```

The following example is invalid:
```sql
/*
module dependency: a 2.0
*/
GO
...
```

#### .dll, .exe or .ps1 step
Dependencies for this step can be defined in a separate text file with the same name. The text file must be the next to step file.

Step moduleA_2.1_2.2.exe, text file moduleA_2.1_2.2.txt
```txt
/*
* module dependency: b 2.0
* module dependency: c 1.0
*/
...
```

Step moduleB_1.0_2.0.dll, text file moduleB_1.0_2.0.txt
```txt
-- module dependency: b 2.0
-- module dependency: c 1.0
...
```

[Back to ToC](#table-of-contents)

Select/update a module version <a name="module-version"></a>
===
Scripts for resolving and updating a module version are defined in the [configuration file](../../ConfigurationFile).

Default scripts must be changed in order to support modularity. Here is one possible example
```sql
-- select current version
SELECT value from sys.fn_listextendedproperty('version-{{ModuleName}}', default, default, default, default, default, default)

-- update current version
EXEC sys.sp_updateextendedproperty @name=N'version-{{ModuleName}}', @value=N'{{TargetVersion}}'
```

[Back to ToC](#table-of-contents)

Execution
===
In the current folder are migration steps for three modules *person*, *book* and *reader*.
- [person_1.0_2.0.sql](person_1.0_2.0.sql) - creates a table "dbo.Person"
- [book_1.0_2.0.sql](book_1.0_2.0.sql) - creates a table "dbo.Book", depends on person 2.0
- [reader_1.0_2.0.sql](reader_1.0_2.0.sql) - creates a table "dbo.BookComment", depends on person 2.0 and book 2.0

The folder structure does not matter, SqlDatabase analyzes all files and folders recursively.

#### runtime
1. Load all migartion steps
2. Resolve the current version of modules: author 1.0, book 1.0 and reader 1.0
3. Resolve migration step dependencies
4. build migration sequence: person 1.0 => 2.0; book 1.0 => 2.0; reader 1.0 => 2.0;
5. Execute each step one by one:

```sql
/* person 1.0 => 2.0 */
execute person_1.0_2.0.sql
-- update author`s version
EXEC sys.sp_updateextendedproperty @name=N'version-person', @value=N'2.0'

/* book 1.0 => 2.0 */
execute book_1.0_2.0.sql
-- update book`s version
EXEC sys.sp_updateextendedproperty @name=N'version-book', @value=N'2.0'

/* reader 1.0 => 2.0 */
execute reader_1.0_2.0.sql
-- update reader`s version
EXEC sys.sp_updateextendedproperty @name=N'version-reader', @value=N'2.0'
```

-whatIf option <a name="whatIf"></a>
===
Use SqlDatabase -whatIf to list all steps with resolved dependencies and to test upgrade sequence.

[Back to ToC](#table-of-contents)
