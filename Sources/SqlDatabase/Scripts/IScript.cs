using System.Data;

namespace SqlDatabase.Scripts
{
    public interface IScript
    {
        string DisplayName { get; set; }

        void Execute(IDbCommand command, IVariables variables, ILogger logger);
    }
}