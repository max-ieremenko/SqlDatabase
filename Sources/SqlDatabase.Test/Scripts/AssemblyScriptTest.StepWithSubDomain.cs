using System;
using System.Data;
using System.IO;
using SqlDatabase.TestApi;

namespace SqlDatabase.Scripts
{
    public partial class AssemblyScriptTest
    {
        public sealed class StepWithSubDomain
        {
            public void ShowAppBase(IDbCommand command)
            {
                var assembly = GetType().Assembly;

                command.CommandText = assembly.Location;
                command.ExecuteNonQuery();

                command.CommandText = AppDomain.CurrentDomain.BaseDirectory;
                command.ExecuteNonQuery();
            }

            public void ShowConfiguration(IDbCommand command)
            {
                command.CommandText = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
                command.ExecuteNonQuery();

                command.CommandText = Query.ConnectionString;
                command.ExecuteNonQuery();
            }

            public void Execute(IDbCommand command)
            {
                var assembly = GetType().Assembly;

                var setup = new AppDomainSetup
                {
                    ApplicationBase = Path.GetDirectoryName(assembly.Location),
                    ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile
                };

                var domain = AppDomain.CreateDomain("StepWithSubDomain", null, setup);
                try
                {
                    var agent = (StepDomainAgent)domain.CreateInstanceFromAndUnwrap(assembly.Location, typeof(StepDomainAgent).FullName);

                    command.CommandText = agent.Hello();
                    command.ExecuteNonQuery();
                }
                finally
                {
                    AppDomain.Unload(domain);
                }
            }
        }

        public sealed class StepDomainAgent : MarshalByRefObject
        {
            public string Hello()
            {
                return "hello";
            }
        }
    }
}
