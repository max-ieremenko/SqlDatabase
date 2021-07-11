using System.Configuration;

namespace SqlDatabase.Configuration
{
    public sealed class AppConfiguration : ConfigurationSection
    {
        public const string SectionName = "sqlDatabase";

        private const string PropertyGetCurrentVersionScript = "getCurrentVersion";
        private const string PropertySetCurrentVersionScript = "setCurrentVersion";
        private const string PropertyAssemblyScript = "assemblyScript";
        private const string PropertyVariables = "variables";
        private const string PropertyMsSql = "mssql";
        private const string PropertyPgSql = "pgsql";
        private const string PropertyMySql = "mysql";

        [ConfigurationProperty(PropertyGetCurrentVersionScript)]
        public string GetCurrentVersionScript
        {
            get => (string)this[PropertyGetCurrentVersionScript];
            set => this[PropertyGetCurrentVersionScript] = value;
        }

        [ConfigurationProperty(PropertySetCurrentVersionScript)]
        public string SetCurrentVersionScript
        {
            get => (string)this[PropertySetCurrentVersionScript];
            set => this[PropertySetCurrentVersionScript] = value;
        }

        [ConfigurationProperty(PropertyAssemblyScript)]
        public AssemblyScriptConfiguration AssemblyScript => (AssemblyScriptConfiguration)this[PropertyAssemblyScript];

        [ConfigurationProperty(PropertyVariables)]
        public NameValueConfigurationCollection Variables => (NameValueConfigurationCollection)this[PropertyVariables];

        [ConfigurationProperty(PropertyMsSql)]
        public DatabaseConfiguration MsSql => (DatabaseConfiguration)this[PropertyMsSql];

        [ConfigurationProperty(PropertyPgSql)]
        public DatabaseConfiguration PgSql => (DatabaseConfiguration)this[PropertyPgSql];

        [ConfigurationProperty(PropertyMySql)]
        public DatabaseConfiguration MySql => (DatabaseConfiguration)this[PropertyMySql];
    }
}