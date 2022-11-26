using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SqlDatabase.Scripts;

internal sealed class SqlScriptVariableParser
{
    internal const string ValueIsHidden = "[value is hidden]";

    // {{var}}
    private const string Pattern1 = @"\{\{(?'name'\w+)\}\}";

    // $(var)
    private const string Pattern2 = @"\$\((?'name'\w+)\)";

    public SqlScriptVariableParser(IVariables variables)
    {
        Variables = variables;
        ValueByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    public IVariables Variables { get; }

    public IDictionary<string, string> ValueByName { get; }

    public static bool IsValidVariableName(string name)
    {
        var match = Regex.Match("{{" + name + "}}", Pattern1, RegexOptions.Compiled);
        if (!match.Success && !name.Equals(match.Groups["name"].Value, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        match = Regex.Match("$(" + name + ")", Pattern2, RegexOptions.Compiled);
        return match.Success && name.Equals(match.Groups["name"].Value, StringComparison.OrdinalIgnoreCase);
    }

    public string ApplyVariables(string script)
    {
        if (string.IsNullOrWhiteSpace(script))
        {
            return script;
        }

        var result = Regex.Replace(script, Pattern1, Evaluator, RegexOptions.Compiled);
        return Regex.Replace(result, Pattern2, Evaluator, RegexOptions.Compiled);
    }

    private static bool HideValueFromOutput(string name)
    {
        return name[0] == '_';
    }

    private string Evaluator(Match match)
    {
        var name = match.Groups["name"].Value;

        var value = Variables.GetValue(name);
        if (value == null)
        {
            throw new InvalidOperationException("Variable [{0}] not defined.".FormatWith(name));
        }

        OnVariablesReplace(name, value);
        return value;
    }

    private void OnVariablesReplace(string name, string value)
    {
        if (!ValueByName.ContainsKey(name))
        {
            ValueByName.Add(name, HideValueFromOutput(name) ? ValueIsHidden : value);
        }
    }
}