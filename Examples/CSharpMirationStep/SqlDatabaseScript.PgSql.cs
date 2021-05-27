using System;
using System.Collections.Generic;
using System.Data;

namespace SqlDatabaseCustomScript.PgSql
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
                @"
DO $$
BEGIN
RAISE NOTICE 'upgrade database {0} from version {1} to {2}';
END
$$;",
                variables["DatabaseName"],
                variables["CurrentVersion"],
                variables["TargetVersion"]);
            command.ExecuteNonQuery();

            command.CommandText = "create table public.demo_table (id integer)";
            command.ExecuteNonQuery();

            command.CommandText = @"
DO $$
BEGIN
RAISE NOTICE 'drop table demo_table';
END
$$;";
            command.ExecuteNonQuery();

            command.CommandText = "drop table public.demo_table";
            command.ExecuteNonQuery();

            Console.WriteLine("finish execution");
        }
    }
}
