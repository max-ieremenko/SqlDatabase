namespace SqlDatabase.Adapter.Sql.Export;

public interface IDataExporter
{
    SqlWriterBase Output { get; set; }

    ILogger Log { get; set; }

    void Export(IDataReader source, string tableName);
}