using SqlDatabase.Scripts;

namespace SqlDatabase
{
    public interface ICreateDatabase
    {
        void BeforeCreate();

        void Execute(IScript script);
    }
}