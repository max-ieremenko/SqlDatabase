using System;
using System.Collections.Generic;
using System.Data;

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
