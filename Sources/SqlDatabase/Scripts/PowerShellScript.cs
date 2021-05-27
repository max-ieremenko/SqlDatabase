using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using SqlDatabase.Scripts.PowerShellInternal;

namespace SqlDatabase.Scripts
{
    internal sealed class PowerShellScript : IScript
    {
        public const string ParameterCommand = "Command";
        public const string ParameterVariables = "Variables";
        public const string ParameterWhatIf = "WhatIf";

        public string DisplayName { get; set; }

        public Func<Stream> ReadScriptContent { get; set; }

        public Func<Stream> ReadDescriptionContent { get; set; }

        public IPowerShellFactory PowerShellFactory { get; set; }

        public void Execute(IDbCommand command, IVariables variables, ILogger logger)
        {
            string script;
            using (var stream = ReadScriptContent())
            using (var reader = new StreamReader(stream))
            {
                script = reader.ReadToEnd();
            }

            var powerShell = PowerShellFactory.Create();
            if (command == null)
            {
                InvokeWhatIf(powerShell, script, variables, logger);
            }
            else
            {
                Invoke(powerShell, script, command, variables, logger, false);
            }
        }

        public IEnumerable<IDataReader> ExecuteReader(IDbCommand command, IVariables variables, ILogger logger)
        {
            throw new NotSupportedException("PowerShell script does not support readers.");
        }

        public IList<ScriptDependency> GetDependencies()
        {
            using (var description = ReadDescriptionContent())
            {
                if (description == null)
                {
                    return new ScriptDependency[0];
                }

                using (var reader = new StreamReader(description))
                {
                    return DependencyParser.ExtractDependencies(reader, DisplayName).ToArray();
                }
            }
        }

        private static void Invoke(IPowerShell powerShell, string script, IDbCommand command, IVariables variables, ILogger logger, bool whatIf)
        {
            var parameters = new KeyValuePair<string, object>[2 + (whatIf ? 1 : 0)];
            parameters[0] = new KeyValuePair<string, object>(ParameterCommand, command);
            parameters[1] = new KeyValuePair<string, object>(ParameterVariables, new VariablesProxy(variables));
            if (whatIf)
            {
                parameters[2] = new KeyValuePair<string, object>(ParameterWhatIf, null);
            }

            powerShell.Invoke(script, logger, parameters);
        }

        private static void InvokeWhatIf(IPowerShell powerShell, string script, IVariables variables, ILogger logger)
        {
            if (powerShell.SupportsShouldProcess(script))
            {
                Invoke(powerShell, script, null, variables, logger, true);
            }
            else
            {
                logger.Info("script does not support -WhatIf.");
            }
        }
    }
}
