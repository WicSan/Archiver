﻿using Archiver.Planning;
using Archiver.Planning.Database;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Archiver.Util
{
    public abstract class Repository<T> : IRepository<T>, IDisposable
        where T : class
    {
        protected JsonDatabase _db;
        private readonly IdentityMap _identityMap;
        private readonly Subject<T> _changeStream;

        public IObservable<T> ChangeStream => _changeStream;

        public Repository(JsonDatabase database)
        {
            _identityMap = new IdentityMap();
            _changeStream = new Subject<T>();
            _db = database;
        }

        private JsonDatabase Database
        {
            get
            {
                return _db;
            }
        }

        public virtual async IAsyncEnumerable<T> GetAllAsync([EnumeratorCancellation] CancellationToken token = default)
        {
            var all = await Task.Factory.StartNew(() => Database.FindAll<T>().ToList());
            foreach (var item in all)
            {
                if (token.IsCancellationRequested)
                {
                    yield break;
                }

                var entry = _identityMap.GetOrCreateEntity(item);
                yield return ((EntityEntry<T>)entry).Entity;
            }
        }

        public async Task<T> UpsertAsync(T entity, CancellationToken token = default)
        {
            await Task.Run(() => Database.Upsert(entity));
            _identityMap.GetOrCreateEntity(entity);

            _changeStream.OnNext(entity);

            return entity;
        }

        public void Dispose()
        {
            _db?.Dispose();
        }
    }
}
