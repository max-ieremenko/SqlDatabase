using System.Data;

namespace SqlDatabase.Export
{
    internal interface IDataExporter
    {
        SqlWriter Output { get; set; }

        ILogger Log { get; set; }

        void Export(IDataReader source, string tableName);
    }
}
