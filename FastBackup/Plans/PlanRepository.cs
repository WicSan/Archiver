using LiteDB;

namespace FastBackup.Plans
{
    public class PlanRepository
    {
        private static readonly LiteDatabase _db = new LiteDatabase("test.db");

        public PlanRepository()
        {
        }

        public ILiteCollection<T> GetCollection<T>()
        {
            return _db.GetCollection<T>();
        }
    }
}
