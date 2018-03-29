using System;
using System.Collections.Generic;

namespace SqlDatabase.Scripts
{
    internal sealed class Variables : IVariables
    {
        private readonly IDictionary<string, string> _valueByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public string DatabaseName
        {
            get => GetValue(nameof(DatabaseName));
            set
            {
                SetValue(nameof(DatabaseName), value);
                SetValue("DbName", value);
            }
        }

        public string CurrentVersion
        {
            get => GetValue(nameof(CurrentVersion));
            set => SetValue(nameof(CurrentVersion), value);
        }

        public string TargetVersion
        {
            get => GetValue(nameof(TargetVersion));
            set => SetValue(nameof(TargetVersion), value);
        }

        public string GetValue(string name)
        {
            _valueByName.TryGetValue(name, out var value);
            if (value != null)
            {
                return value;
            }

            value = Environment.GetEnvironmentVariable(name);
            return string.IsNullOrEmpty(value) ? null : value;
        }

        internal void SetValue(string name, string value)
        {
            if (value == null)
            {
                _valueByName.Remove(name);
            }
            else
            {
                _valueByName[name] = value;
            }
        }
    }
}
