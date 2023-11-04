using System;
using System.Collections;
using System.Management.Automation;

namespace SqlDatabase.Scripts.PowerShellInternal;

internal sealed class PowerShellStreamsListener : IDisposable
{
    private readonly PSDataStreams _streams;
    private readonly ILogger _logger;
    private readonly IList _information;

    public PowerShellStreamsListener(PSDataStreams streams, ILogger logger)
    {
        _streams = streams;
        _logger = logger;

        _information = GetInformation(streams);

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

    private static IList GetInformation(PSDataStreams streams)
    {
#if !NET472
        return streams.Information;
#else
            return ReflectionGetInformation(streams);
#endif
    }

    private static IList ReflectionGetInformation(PSDataStreams streams)
    {
        return (IList)streams
            .GetType()
            .FindProperty("Information")
            .GetValue(streams, null);
    }

    private static void InvokeDataAdded(object dataCollection, EventHandler<DataAddedEventArgs> handler, bool subscribe)
    {
        var evt = dataCollection
            .GetType()
            .FindEvent("DataAdded");

        if (subscribe)
        {
            evt.AddMethod.Invoke(dataCollection, new object[] { handler });
        }
        else
        {
            evt.RemoveMethod.Invoke(dataCollection, new object[] { handler });
        }
    }

    private void OnWarning(object sender, DataAddedEventArgs e)
    {
        _logger.Info(_streams.Warning[e.Index]?.ToString());
    }

    private void OnError(object sender, DataAddedEventArgs e)
    {
        HasErrors = true;
        _logger.Error(_streams.Error[e.Index]?.ToString());
    }

    private void OnVerbose(object sender, DataAddedEventArgs e)
    {
        _logger.Info(_streams.Verbose[e.Index]?.ToString());
    }

    private void OnInformation(object sender, DataAddedEventArgs e)
    {
        _logger.Info(_information[e.Index]?.ToString());
    }
}