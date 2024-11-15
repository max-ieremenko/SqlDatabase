﻿using System.Management.Automation;
using System.Runtime.InteropServices;
using SqlDatabase.PowerShell.Internal;

namespace SqlDatabase.PowerShell;

[Cmdlet(VerbsCommon.Show, "SqlDatabaseInfo")]
public sealed class InfoCmdLet : PSCmdlet
{
    protected override void ProcessRecord()
    {
        var assembly = GetType().Assembly;

        this.TryGetPSVersionTable(out var psVersionTable);

        WriteObject(new
        {
            psVersionTable.PSEdition,
            psVersionTable.PSVersion,
            Version = assembly.GetName().Version,
            ClrVersion = Environment.Version,
            RuntimeInformation.FrameworkDescription,
            RuntimeInformation.OSDescription,
            RuntimeInformation.OSArchitecture,
            RuntimeInformation.ProcessArchitecture,
            Location = assembly.GetDirectoryLocation(),
            WorkingDirectory = this.GetWorkingDirectory(),
            DefaultConfigurationFile = PowerShellCommand.GetDefaultConfigurationFile(this)
        });
    }
}