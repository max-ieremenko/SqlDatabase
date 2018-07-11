The project contains an example of database migration step implementation.

Build output is 2.1_2.2.dll with taget framework 4.5.2

Assembly must contain only one "public class SqlDatabaseScript", namespace name doesn't matter.

SqlDatabaseScript must contain "public void Execute(IDbCommand command, IReadOnlyDictionary<string, string> variables)".

Method Execute implements the logic of your migration step. See [code example](https://github.com/max-ieremenko/SqlDatabase/blob/master/Examples/CSharpMirationStep/SqlDatabaseScript.cs).

Use parameter "IDbCommand command" to affect database.
Use Console.WriteLine() to write something into migration log.

At runtime the assembly will be loaded into private application domain with
* ApplicationBase: location of SqlDatabase.exe
* ConfigurationFile: SqlDatabase.exe.config

After the migration step is finished or failed
- instance of SqlDatabaseScript will be disposed (id IDisposable)
- the domain will be unloaded
