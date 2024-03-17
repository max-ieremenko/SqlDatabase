using SqlDatabase.Adapter;
using SqlDatabase.Commands;
using SqlDatabase.FileSystem;

namespace SqlDatabase.Configuration;

internal abstract class CommandLineBase : ICommandLine
{
    public string? ConnectionString { get; set; }

    public IList<IFileSystemInfo> Scripts { get; } = new List<IFileSystemInfo>();

    public IDictionary<string, string> Variables { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public string? ConfigurationFile { get; set; }

    public IFileSystemFactory FileSystemFactory { get; set; } = new FileSystemFactory();

    public void Parse(CommandLine args)
    {
        foreach (var arg in args.Args)
        {
            ApplyArg(arg);
        }

        if (string.IsNullOrWhiteSpace(ConnectionString))
        {
            throw new InvalidCommandLineException($"Options {Arg.Database} is not specified.");
        }

        if (Scripts.Count == 0)
        {
            throw new InvalidCommandLineException($"Options {Arg.Scripts} is not specified.");
        }

        Validate();
    }

    public abstract ICommand CreateCommand(ILogger logger);

    protected internal virtual void Validate()
    {
    }

    protected static bool TryParseSwitchParameter(Arg arg, string parameterName, out bool value)
    {
        if (parameterName.Equals(arg.Key, StringComparison.OrdinalIgnoreCase))
        {
            value = string.IsNullOrEmpty(arg.Value) || bool.Parse(arg.Value);
            return true;
        }

        if (!arg.IsPair && parameterName.Equals(arg.Value, StringComparison.OrdinalIgnoreCase))
        {
            value = true;
            return true;
        }

        value = false;
        return false;
    }

    protected static bool TryParseWhatIf(Arg arg, out bool whatIf) => TryParseSwitchParameter(arg, Arg.WhatIf, out whatIf);

    protected virtual bool ParseArg(Arg arg)
    {
        return false;
    }

    protected void SetInLineScript(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        var index = Scripts.Count + 1;
        var script = FileSystemFactory.FromContent($"from{index}.sql", value!);

        Scripts.Add(script);
    }

    private void ApplyArg(Arg arg)
    {
        bool isParsed;
        try
        {
            isParsed = (arg.IsPair && TryParseKnownPair(arg)) || ParseArg(arg);
        }
        catch (InvalidCommandLineException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidCommandLineException($"Fail to parse option [{arg}].", ex);
        }

        if (!isParsed)
        {
            throw new InvalidCommandLineException($"Unknown option [{arg}].");
        }
    }

    private bool TryParseKnownPair(Arg arg)
    {
        if (Arg.Database.Equals(arg.Key, StringComparison.OrdinalIgnoreCase))
        {
            ConnectionString = arg.Value;
            return true;
        }

        if (Arg.Scripts.Equals(arg.Key, StringComparison.OrdinalIgnoreCase))
        {
            SetScripts(arg.Value);
            return true;
        }

        if (arg.Key != null && arg.Key.StartsWith(Arg.Variable, StringComparison.OrdinalIgnoreCase))
        {
            SetVariable(arg.Key.Substring(Arg.Variable.Length), arg.Value);
            return true;
        }

        if (Arg.Configuration.Equals(arg.Key, StringComparison.OrdinalIgnoreCase))
        {
            SetConfigurationFile(arg.Value);
            return true;
        }

        return false;
    }

    private void SetScripts(string? value)
    {
        Scripts.Add(FileSystemFactory.FileSystemInfoFromPath(value));
    }

    private void SetVariable(string? name, string? value)
    {
        name = name?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            throw new InvalidCommandLineException(Arg.Variable, $"Invalid variable name [{name}].");
        }

        if (Variables.ContainsKey(name!))
        {
            throw new InvalidCommandLineException(Arg.Variable, $"Variable with name [{name}] is duplicated.");
        }

        Variables.Add(name!, value ?? string.Empty);
    }

    private void SetConfigurationFile(string? configurationFile)
    {
        ConfigurationFile = configurationFile;
    }
}