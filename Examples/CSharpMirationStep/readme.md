# .NET assembly with a script implementation
The project contains an example of database migration step implementation.

Build output is 2.1_2.2.dll with target framework 4.5.2

Method [SqlDatabaseScript.Execute](https://github.com/max-ieremenko/SqlDatabase/blob/master/Examples/CSharpMirationStep/SqlDatabaseScript.cs) implements the logic of your migration step.

Use parameter "IDbCommand command" to affect database.
Use Console.WriteLine() to write something into migration log.

## Runtime
At runtime the assembly will be loaded into private application domain with
* ApplicationBase: temporary directory
* ConfigurationFile: SqlDatabase.exe.config
* Location of assembly: ApplicationBase, temporary directory

Instance of migration step will be resolved via reflection: Activator.CreateInstance(typeof(SqlDatabaseScript))

After the migration step is finished or failed
- instance of SqlDatabaseScript will be disposed (if IDisposable)
- the domain will be unloaded
- temporary directory will be deleted

## Reflection
The assembly must contain only one "public class SqlDatabaseScript", namespace doesn't matter.
Class SqlDatabaseScript must contain method "public void Execute(...)".

Supported signatures of Execute method
* void Execute(IDbCommand command, IReadOnlyDictionary<string, string> variables)
* void Execute(IReadOnlyDictionary<string, string> variables, IDbCommand command)
* void Execute(IDbCommand command)
* void Execute(IDbConnection connection)

## Configuration
name of class SqlDatabaseScript and method Execute can be changed in the [SqlDatabase.exe.config](https://github.com/max-ieremenko/SqlDatabase/tree/master/Examples/ConfigurationFile):
```xml
<configuration>
  <configSections>
    <section name="sqlDatabase"
             type="SqlDatabase.Configuration.AppConfiguration, SqlDatabase"/>
  </configSections>

  <sqlDatabase>
    <assemblyScript className="SqlDatabaseScript"
                    methodName="Execute" />
  </sqlDatabase>
</configuration>
```