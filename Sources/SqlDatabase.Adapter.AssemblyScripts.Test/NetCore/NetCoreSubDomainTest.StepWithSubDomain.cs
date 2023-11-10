using System;
using System.Data;

namespace SqlDatabase.Adapter.AssemblyScripts.NetCore;

public partial class NetCoreSubDomainTest
{
    public sealed class StepWithCoreSubDomain
    {
        public void ShowAppBase(IDbCommand command)
        {
            var assembly = GetType().Assembly;

            command.CommandText = assembly.Location;
            command.ExecuteNonQuery();

            command.CommandText = AppDomain.CurrentDomain.BaseDirectory;
            command.ExecuteNonQuery();
        }
    }
}