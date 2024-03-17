namespace SqlDatabase.Configuration;

public sealed class GenericCommandLine
{
    public string? Command { get; set; }

    public string? Connection { get; set; }

    public TransactionMode Transaction { get; set; }

    public IList<string> Scripts { get; } = new List<string>();

    public IList<string> InLineScript { get; } = new List<string>();

    public IDictionary<string, string> Variables { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public string? ConfigurationFile { get; set; }

    public string? ExportToTable { get; set; }

    public string? ExportToFile { get; set; }

    public bool WhatIf { get; set; }

    public bool FolderAsModuleName { get; set; }

    public string? LogFileName { get; set; }
}