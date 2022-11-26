using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using SqlDatabase.Configuration;

namespace SqlDatabase.Scripts;

internal sealed class Database : IDatabase
{
    public Database()
    {
        Variables = new Variables();
    }

    public IDatabaseAdapter Adapter { get; set; }

    public ILogger Log { get; set; }

    public TransactionMode Transaction { get; set; }

    public bool WhatIf { get; set; }

    internal Variables Variables { get; }

    public Version GetCurrentVersion(string moduleName)
    {
        Variables.ModuleName = moduleName;

        using (var connection = Adapter.CreateConnection(false))
        using (var command = connection.CreateCommand())
        {
            command.CommandTimeout = 0;
            connection.Open();

            return ReadCurrentVersion(command);
        }
    }

    public string GetServerVersion()
    {
        using (var connection = Adapter.CreateConnection(true))
        using (var command = connection.CreateCommand())
        {
            command.CommandText = Adapter.GetServerVersionSelectScript();

            connection.Open();
            return Convert.ToString(command.ExecuteScalar());
        }
    }

    public void Execute(IScript script, string moduleName, Version currentVersion, Version targetVersion)
    {
        Variables.ModuleName = moduleName;
        Variables.CurrentVersion = currentVersion.ToString();
        Variables.TargetVersion = targetVersion.ToString();
        Variables.DatabaseName = Adapter.DatabaseName;

        if (WhatIf)
        {
            ExecuteWhatIf(script);
        }
        else
        {
            InvokeExecuteUpgrade(script, targetVersion);
        }
    }

    public void Execute(IScript script)
    {
        Variables.DatabaseName = Adapter.DatabaseName;

        if (WhatIf)
        {
            ExecuteWhatIf(script);
        }
        else
        {
            InvokeExecute(script);
        }
    }

    public IEnumerable<IDataReader> ExecuteReader(IScript script)
    {
        Variables.DatabaseName = Adapter.DatabaseName;

        using (var connection = Adapter.CreateConnection(false))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandTimeout = 0;

                foreach (var reader in script.ExecuteReader(command, Variables, Log))
                {
                    yield return reader;
                }
            }
        }
    }

    private void WriteCurrentVersion(IDbCommand command, Version targetVersion)
    {
        var script = new SqlScriptVariableParser(Variables).ApplyVariables(Adapter.GetVersionUpdateScript());
        command.CommandText = script;

        try
        {
            command.ExecuteNonQuery();
        }
        catch (DbException ex)
        {
            throw new InvalidOperationException("Fail to update the version, script: {0}".FormatWith(script), ex);
        }

        var checkVersion = ReadCurrentVersion(command);
        if (checkVersion != targetVersion)
        {
            throw new InvalidOperationException("Set version script works incorrectly: expected version is {0}, but actual is {1}. Script: {2}".FormatWith(
                targetVersion,
                checkVersion,
                script));
        }
    }

    private Version ReadCurrentVersion(IDbCommand command)
    {
        var script = new SqlScriptVariableParser(Variables).ApplyVariables(Adapter.GetVersionSelectScript());
        command.CommandText = script;

        string version;
        try
        {
            version = Convert.ToString(command.ExecuteScalar());
        }
        catch (DbException ex)
        {
            throw new InvalidOperationException("Fail to read the version, script: {0}".FormatWith(script), ex);
        }

        if (!Version.TryParse(version, out var result))
        {
            if (string.IsNullOrEmpty(Variables.ModuleName))
            {
                throw new InvalidOperationException("The version [{0}] of database is invalid.".FormatWith(version));
            }

            throw new InvalidOperationException("The version [{0}] of module [{1}] is invalid.".FormatWith(version, Variables.ModuleName));
        }

        return result;
    }

    private void InvokeExecuteUpgrade(IScript script, Version targetVersion)
    {
        using (var connection = Adapter.CreateConnection(false))
        using (var command = connection.CreateCommand())
        {
            command.CommandTimeout = 0;
            connection.Open();

            using (var transaction = Transaction == TransactionMode.PerStep ? connection.BeginTransaction(IsolationLevel.ReadCommitted) : null)
            {
                command.Transaction = transaction;

                script.Execute(command, Variables, Log);

                WriteCurrentVersion(command, targetVersion);

                transaction?.Commit();
            }
        }
    }

    private void InvokeExecute(IScript script)
    {
        bool useMaster;

        using (var connection = Adapter.CreateConnection(true))
        using (var command = connection.CreateCommand())
        {
            command.CommandTimeout = 0;
            connection.Open();

            command.CommandText = Adapter.GetDatabaseExistsScript(Variables.DatabaseName);
            var value = command.ExecuteScalar();

            useMaster = value == null || Convert.IsDBNull(value);
        }

        using (var connection = Adapter.CreateConnection(useMaster))
        {
            connection.Open();

            using (var transaction = Transaction == TransactionMode.PerStep ? connection.BeginTransaction(IsolationLevel.ReadCommitted) : null)
            {
                using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandTimeout = 0;
                    script.Execute(command, Variables, Log);
                }

                transaction?.Commit();
            }
        }
    }

    private void ExecuteWhatIf(IScript script)
    {
        Log.Info("what-if mode");
        script.Execute(null, Variables, Log);
    }
}