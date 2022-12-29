using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ArchivePlanner.Util
{
    public interface IRepository<T>
        where T : class
    {
        IObservable<T> ChangeStream { get; }

        IAsyncEnumerable<T> GetAllAsync(CancellationToken token = default);

        Task<T> UpsertAsync(T entity, CancellationToken token = default);
    }
}
