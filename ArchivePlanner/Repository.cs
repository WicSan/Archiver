using LiteDB;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace ArchivePlanner
{
    public abstract class Repository : IRepository, IDisposable
    {
        protected readonly LiteDatabase _db;

        public Repository(IOptions<LiteDbOptions> options)
        {
            _db = new LiteDatabase(options.Value.DbName);
        }

        private ILiteCollection<T> GetCollection<T>()
        {
            return _db.GetCollection<T>();
        }

        public virtual IEnumerable<T> GetAll<T>()
        {
            return _db.GetCollection<T>().FindAll();
        }

        public void Upsert<T>(T entity)
        {
            GetCollection<T>().Upsert(entity);
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}
