using SqlDatabase.Adapter;

namespace SqlDatabase.CommandLine;

public sealed class ExecuteCommandLine : ICommandLine
{
    public string Database { get; set; } = string.Empty;

    public List<ScriptSource> From { get; } = new();

    public TransactionMode Transaction { get; set; }

    public Dictionary<string, string> Variables { get; } = new(StringComparer.OrdinalIgnoreCase);

    public string? Configuration { get; set; }

    public string? Log { get; set; }

    public string? UsePowerShell { get; set; }

    public bool WhatIf { get; set; }
}