using System.Collections.Generic;
using System.Text;

namespace SqlDatabase.Configuration;

internal sealed class GenericCommandLineBuilder
{
    public GenericCommandLineBuilder()
        : this(new GenericCommandLine())
    {
    }

    public GenericCommandLineBuilder(GenericCommandLine line)
    {
        Line = line;
    }

    internal GenericCommandLine Line { get; }

    public GenericCommandLineBuilder SetCommand(string command)
    {
        Line.Command = command;
        return this;
    }

    public GenericCommandLineBuilder SetConnection(string connectionString)
    {
        Line.Connection = connectionString;
        return this;
    }

    public GenericCommandLineBuilder SetScripts(string value)
    {
        Line.Scripts.Add(value);

        return this;
    }

    public GenericCommandLineBuilder SetInLineScript(string value)
    {
        Line.InLineScript.Add(value);

        return this;
    }

    public GenericCommandLineBuilder SetTransaction(TransactionMode mode)
    {
        Line.Transaction = mode;
        return this;
    }

    public GenericCommandLineBuilder SetVariable(string name, string value)
    {
        name = name?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            throw new InvalidCommandLineException(Arg.Variable, "Invalid variable name [{0}].".FormatWith(name));
        }

        if (Line.Variables.ContainsKey(name))
        {
            throw new InvalidCommandLineException(Arg.Variable, "Variable with name [{0}] is duplicated.".FormatWith(name));
        }

        Line.Variables.Add(name, value);

        return this;
    }

    public GenericCommandLineBuilder SetVariable(string nameValue)
    {
        if (!CommandLineParser.ParseArg(Arg.Sign + nameValue, out var arg)
            || !arg.IsPair)
        {
            throw new InvalidCommandLineException(Arg.Variable, "Invalid variable value definition [{0}].".FormatWith(nameValue));
        }

        return SetVariable(arg.Key, arg.Value);
    }

    public GenericCommandLineBuilder SetConfigurationFile(string configurationFile)
    {
        Line.ConfigurationFile = configurationFile;
        return this;
    }

    public GenericCommandLineBuilder SetExportToTable(string name)
    {
        Line.ExportToTable = name;
        return this;
    }

    public GenericCommandLineBuilder SetExportToFile(string fileName)
    {
        Line.ExportToFile = fileName;
        return this;
    }

    public GenericCommandLineBuilder SetWhatIf(bool value)
    {
        Line.WhatIf = value;
        return this;
    }

    public GenericCommandLineBuilder SetFolderAsModuleName(bool value)
    {
        Line.FolderAsModuleName = value;
        return this;
    }

    public GenericCommandLineBuilder SetLogFileName(string fileName)
    {
        Line.LogFileName = fileName;
        return this;
    }

    public GenericCommandLine Build()
    {
        return Line;
    }

    public string[] BuildArray()
    {
        var cmd = Build();

        var result = new List<string>
        {
            cmd.Command,
            CombineArg(Arg.Database, cmd.Connection)
        };

        foreach (var script in cmd.Scripts)
        {
            result.Add(CombineArg(Arg.Scripts, script));
        }

        foreach (var script in cmd.InLineScript)
        {
            result.Add(CombineArg(Arg.InLineScript, script));
        }

        if (cmd.Transaction != default(TransactionMode))
        {
            result.Add(CombineArg(Arg.Transaction, cmd.Transaction.ToString()));
        }

        if (!string.IsNullOrEmpty(cmd.ConfigurationFile))
        {
            result.Add(CombineArg(Arg.Configuration, cmd.ConfigurationFile));
        }

        if (!string.IsNullOrEmpty(cmd.ExportToTable))
        {
            result.Add(CombineArg(Arg.ExportToTable, cmd.ExportToTable));
        }

        if (!string.IsNullOrEmpty(cmd.ExportToFile))
        {
            result.Add(CombineArg(Arg.ExportToFile, cmd.ExportToFile));
        }

        foreach (var entry in cmd.Variables)
        {
            result.Add(CombineArg(Arg.Variable + entry.Key, entry.Value));
        }

        if (cmd.WhatIf)
        {
            result.Add(CombineArg(Arg.WhatIf, cmd.WhatIf.ToString()));
        }

        if (cmd.FolderAsModuleName)
        {
            result.Add(CombineArg(Arg.FolderAsModuleName, cmd.FolderAsModuleName.ToString()));
        }

        if (!string.IsNullOrEmpty(cmd.LogFileName))
        {
            result.Add(CombineArg(Arg.Log, cmd.LogFileName));
        }

        return result.ToArray();
    }

    private static string CombineArg(string key, string value)
    {
        var result = new StringBuilder()
            .Append(Arg.Sign)
            .Append(key)
            .Append("=")
            .Append(value);

        return result.ToString();
    }
}