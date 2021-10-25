using System.Collections.Generic;

namespace ArchivePlanner
{
    public interface IRepository
    {
        IEnumerable<T> GetAll<T>();

        void Upsert<T>(T entity);
    }
}
