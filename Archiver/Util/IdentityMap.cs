using Archiver.Planning.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Archiver.Util
{
    public class IdentityMap
    {
        public IDictionary<object, EntityEntry> LoadedEntities { get; set; } = new Dictionary<object, EntityEntry>();

        public EntityEntry GetOrCreateEntity<TEntity>(TEntity entity)
            where TEntity : class
        {
            var key = entity.GetType()
                .GetProperties()
                .Single(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(IdAttribute)));
            if (LoadedEntities.TryGetValue(key.GetValue(entity)!, out var entityEntry))
            {
                return entityEntry;
            }
            else
            {
                var newEntry = new EntityEntry<TEntity>(entity);
                return newEntry;
            }
        }

        public void UpdateEntity<TEntity>(TEntity entity)
            where TEntity : class
        {
            var key = entity.GetType()
                .GetProperties()
                .Single(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(IdAttribute)));
            if (LoadedEntities.TryGetValue(key.GetValue(entity)!, out var entityEntry))
            {
                throw new InvalidOperationException();
            }

            var properties = typeof(TEntity).GetProperties();

            foreach (var property in properties)
            {
                property.SetValue(entityEntry, property.GetValue(entity));
            }
        }
    }
}
