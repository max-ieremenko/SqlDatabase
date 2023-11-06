using System.Data;
using SqlDatabase.Adapter;

namespace SqlDatabase.Export;

internal interface IDataExporter
{
    SqlWriterBase Output { get; set; }

    ILogger Log { get; set; }

    void Export(IDataReader source, string tableName);
}