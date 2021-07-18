using System;
using System.Collections.Generic;
using System.Data;

namespace SqlDatabaseCustomScript.MySql
{
    public /*sealed*/ class SqlDatabaseScript /*: IDisposable*/
    {
        /// <summary>
        /// PostgreSQL server demo
        /// </summary>
        public void Execute(IDbCommand command, IReadOnlyDictionary<string, string> variables)
        {
            Console.WriteLine("start execution");

            command.CommandText = string.Format(
                @"SELECT 'upgrade database {0} from version {1} to {2}' info",
                variables["DatabaseName"],
                variables["CurrentVersion"],
                variables["TargetVersion"]);
            command.ExecuteNonQuery();

            command.CommandText = "create table demo_table (id INT)";
            command.ExecuteNonQuery();

            command.CommandText = "SELECT 'drop table demo_table' info;";
            command.ExecuteNonQuery();

            command.CommandText = "drop table demo_table";
            command.ExecuteNonQuery();

            Console.WriteLine("finish execution");
        }
    }
}
