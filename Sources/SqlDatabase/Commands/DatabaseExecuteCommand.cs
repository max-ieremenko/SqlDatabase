using SqlDatabase.Scripts;

namespace SqlDatabase.Commands
{
    internal sealed class DatabaseExecuteCommand : DatabaseCommandBase
    {
        public IScript Script { get; set; }

        protected override void Greet(string databaseLocation)
        {
            Log.Info("Execute script [{0}] on {1}".FormatWith(Script.DisplayName, databaseLocation));
        }

        protected override void ExecuteCore()
        {
            Database.Execute(Script);
        }
    }
}