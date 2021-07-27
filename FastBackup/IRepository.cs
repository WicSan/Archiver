using LiteDB;

namespace FastBackup
{
    public interface IRepository
    {
        ILiteCollection<T> GetCollection<T>();
    }
}
