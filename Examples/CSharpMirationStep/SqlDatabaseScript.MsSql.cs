using System;
using System.Collections.Generic;
using System.Data;

namespace SqlDatabaseCustomScript.MsSql
{
    public /*sealed*/ class SqlDatabaseScript /*: IDisposable*/
    {
        /// <summary>
        /// MSSQL server demo
        /// </summary>
        public void Execute(IDbCommand command, IReadOnlyDictionary<string, string> variables)
        {
            Console.WriteLine("start execution");

            command.CommandText = string.Format(
                "print 'upgrade database {0} from version {1} to {2}'",
                variables["DatabaseName"],
                variables["CurrentVersion"],
                variables["TargetVersion"]);
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
