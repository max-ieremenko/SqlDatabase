using System.Collections;
using System.Management.Automation;

namespace SqlDatabase.Adapter.PowerShellScripts;

internal sealed class PowerShellStreamsListener : IDisposable
{
    private readonly PSDataStreams _streams;
    private readonly ILogger _logger;
    private readonly IList _information;

    public PowerShellStreamsListener(PSDataStreams streams, FrameworkVersion version, ILogger logger)
    {
        _streams = streams;
        _logger = logger;

        _information = version == FrameworkVersion.Net472 ? ReflectionGetInformation(streams) : streams.Information;

        InvokeDataAdded(_information, OnInformation, true);
        streams.Verbose.DataAdded += OnVerbose;
        streams.Error.DataAdded += OnError;
        streams.Warning.DataAdded += OnWarning;
    }

    public bool HasErrors { get; private set; }

    public void Dispose()
    {
        _streams.Verbose.DataAdded -= OnVerbose;
        _streams.Error.DataAdded -= OnError;
        _streams.Warning.DataAdded -= OnWarning;
        InvokeDataAdded(_information, OnInformation, false);
    }

    private static IList ReflectionGetInformation(PSDataStreams streams)
    {
        return (IList)streams
            .GetType()
            .FindProperty("Information")
            .GetValue(streams, null)!;
    }

    private static void InvokeDataAdded(object dataCollection, EventHandler<DataAddedEventArgs> handler, bool subscribe)
    {
        var evt = dataCollection
            .GetType()
            .FindEvent("DataAdded");

        if (subscribe)
        {
            evt.AddMethod!.Invoke(dataCollection, [handler]);
        }
        else
        {
            evt.RemoveMethod!.Invoke(dataCollection, [handler]);
        }
    }

    private void OnWarning(object? sender, DataAddedEventArgs e)
    {
        _logger.Info(_streams.Warning[e.Index]?.ToString() ?? string.Empty);
    }

    private void OnError(object? sender, DataAddedEventArgs e)
    {
        HasErrors = true;
        _logger.Error(_streams.Error[e.Index]?.ToString() ?? string.Empty);
    }

    private void OnVerbose(object? sender, DataAddedEventArgs e)
    {
        _logger.Info(_streams.Verbose[e.Index]?.ToString() ?? string.Empty);
    }

    private void OnInformation(object? sender, DataAddedEventArgs e)
    {
        _logger.Info(_information[e.Index]?.ToString() ?? string.Empty);
    }
}