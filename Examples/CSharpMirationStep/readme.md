.NET assembly with a script implementation
==========================================

Any assembly script is

- .exe or .dll for target framework is 4.5.2+
- .dll for .net 7.0, 6.0, 5.0 or .net core 3.1
- has exactly one class with script implementation

This project is an example of script implementation.
The build output is 2.1_2.2.dll with target framework 4.5.2.
Due to the current dependencies, 2.1_2.2.dll works well on .net 7.0 - .net core 3.1.

## Script source

Method [SqlDatabaseScript.Execute](SqlDatabaseScript.cs) implements a logic of script

```C#
namespace SqlDatabaseCustomScript
{
    public /*sealed*/ class SqlDatabaseScript /*: IDisposable*/
    {
        public void Execute(IDbCommand command, IReadOnlyDictionary<string, string> variables)
        {
            Console.WriteLine("start execution");

            command.CommandText = string.Format("print 'current database name is {0}'", variables["DatabaseName"]);
            command.ExecuteNonQuery();

            command.CommandText = string.Format("print 'version from {0}'", variables["CurrentVersion"]);
            command.ExecuteNonQuery();

            command.CommandText = string.Format("print 'version to {0}'", variables["TargetVersion"]);
            command.ExecuteNonQuery();

            command.CommandText = "create table dbo.DemoTable (Id INT)";
            command.ExecuteNonQuery();

            command.CommandText = "print 'drop table DemoTable'";
            command.ExecuteNonQuery();

            command.CommandText = "drop table dbo.DemoTable";
            command.ExecuteNonQuery();

            Console.WriteLine("finish execution");
        }
    }
}
```

Use

- method`s parameter "IDbCommand command" to affect database
- Console.WriteLine() to write something into output/log

## Runtime .NET desktop

At runtime the assembly will be loaded into A private application domain with

- ApplicationBase: temporary directory
- Location of assembly: ApplicationBase, temporary directory

```C#
    public class SqlDatabaseScript
    {
        public void Execute(IDbCommand command, IReadOnlyDictionary<string, string> variables)
        {
            var assemblyLocation = GetType().Assembly.Location;

            // temporary directory
            Console.WriteLine(assemblyLocation);
        }
    }
```

Instance of migration step will be resolved via reflection: `Activator.CreateInstance(typeof(SqlDatabaseScript))`

After the migration step is finished or failed

- instance of SqlDatabaseScript will be disposed (if `IDisposable`)
- the domain will be unloaded
- temporary directory will be deleted

## Runtime .NET Core

At runtime the assembly will be loaded into the current application domain (`AssemblyLoadContext.Default`).

- ApplicationBase: is a directory of SqlDatabase
- Script assembly has no location:

```C#
    public class SqlDatabaseScript
    {
        public void Execute(IDbCommand command, IReadOnlyDictionary<string, string> variables)
        {
            var assemblyLocation = GetType().Assembly.Location;

            // output is empty
            Console.WriteLine(assemblyLocation);
        }
    }
```

Instance of migration step will be resolved via reflection: `Activator.CreateInstance(typeof(SqlDatabaseScript))`

After the migration step is finished or failed

- instance of SqlDatabaseScript will be disposed (if `IDisposable`)

## Resolving SqlDatabaseScript.Execute

The assembly must contain exactly one `public class SqlDatabaseScript`, namespace doesn't matter.
Class `SqlDatabaseScript` must contain instance method `public void Execute(...)`.

Supported signatures of Execute method

- void Execute(IDbCommand command, IReadOnlyDictionary<string, string> variables)
- void Execute(IReadOnlyDictionary<string, string> variables, IDbCommand command)
- void Execute(IDbCommand command)
- void Execute(IDbConnection connection)

Names *SqlDatabaseScript* and *Execute* are configurable.

## Configuration

name of class `SqlDatabaseScript` and method `Execute` can be changed in the [configuration file](../ConfigurationFile):

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