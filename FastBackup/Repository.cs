using LiteDB;

namespace FastBackup.Plans
{
    public class Repository
    {
        private static readonly LiteDatabase _db = new LiteDatabase("test.db");

        public Repository()
        {
        }

        public ILiteCollection<T> GetCollection<T>()
        {
            return _db.GetCollection<T>();
        }
    }
}
