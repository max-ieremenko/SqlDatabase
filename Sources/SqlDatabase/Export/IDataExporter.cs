using System.Data;

namespace SqlDatabase.Export
{
    internal interface IDataExporter
    {
        SqlWriterBase Output { get; set; }

        ILogger Log { get; set; }

        void Export(IDataReader source, string tableName);
    }
}
