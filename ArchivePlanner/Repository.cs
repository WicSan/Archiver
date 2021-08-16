using LiteDB;
using Microsoft.Extensions.Options;

namespace FastBackup
{
    public class Repository : IRepository
    {
        private readonly LiteDatabase _db;

        public Repository(IOptions<LiteDbOptions> options)
        {
            _db = new LiteDatabase(options.Value.DbName);
        }

        public ILiteCollection<T> GetCollection<T>()
        {
            return _db.GetCollection<T>();
        }
    }
}
