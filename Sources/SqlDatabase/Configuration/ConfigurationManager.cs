using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using SqlDatabase.IO;
using Manager = System.Configuration.ConfigurationManager;

namespace SqlDatabase.Configuration
{
    internal sealed class ConfigurationManager : IConfigurationManager
    {
        public AppConfiguration SqlDatabase { get; private set; }

        public void LoadFrom(string configurationFile)
        {
            try
            {
                var info = string.IsNullOrEmpty(configurationFile) ? null : FileSystemFactory.FileSystemInfoFromPath(configurationFile);
                LoadFrom(info);
            }
            catch (Exception ex) when ((ex as IOException) == null)
            {
                throw new ConfigurationErrorsException("Fail to load configuration from [{0}].".FormatWith(configurationFile), ex);
            }
        }

        internal void LoadFrom(IFileSystemInfo info)
        {
            AppConfiguration section;
            if (info == null)
            {
                section = LoadCurrent();
            }
            else
            {
                // in a PowerShell context resolving SqlDatabase does not work
                AppDomain.CurrentDomain.AssemblyResolve += ResolveSqlDatabaseAssembly;
                try
                {
                    section = Load(info);
                }
                finally
                {
                    AppDomain.CurrentDomain.AssemblyResolve -= ResolveSqlDatabaseAssembly;
                }
            }

            SqlDatabase = section ?? new AppConfiguration();
        }

        private static AppConfiguration LoadCurrent()
        {
            return (AppConfiguration)Manager.GetSection(AppConfiguration.SectionName);
        }

        private static AppConfiguration Load(IFileSystemInfo info)
        {
            var file = ResolveFile(info);

            var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                using (var destination = new FileStream(tempFile, FileMode.Create, FileAccess.ReadWrite))
                using (var source = file.OpenRead())
                {
                    source.CopyTo(destination);
                }

                var configuration = Manager.OpenMappedExeConfiguration(new ExeConfigurationFileMap { ExeConfigFilename = tempFile }, ConfigurationUserLevel.None);
                return (AppConfiguration)configuration.GetSection(AppConfiguration.SectionName);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        private static IFile ResolveFile(IFileSystemInfo info)
        {
            IFile file;
            if (info is IFolder folder)
            {
                var fileName = Path.GetFileName(typeof(ConfigurationManager).Assembly.Location) + ".config";
                file = folder
                    .GetFiles()
                    .FirstOrDefault(i => fileName.Equals(i.Name, StringComparison.OrdinalIgnoreCase));

                if (file == null)
                {
                    throw new FileNotFoundException("Configuration file {0} not found in {1}.".FormatWith(fileName, info.Name));
                }
            }
            else
            {
                file = (IFile)info;
            }

            return file;
        }

        private static Assembly ResolveSqlDatabaseAssembly(object sender, ResolveEventArgs args)
        {
            var assembly = typeof(ConfigurationManager).Assembly;
            if (string.Equals(assembly.GetName().Name, args.Name, StringComparison.OrdinalIgnoreCase))
            {
                return assembly;
            }

            return null;
        }
    }
}