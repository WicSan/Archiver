namespace Archiver.Util
{
    public class EntityEntry
    {
        public virtual object Entity { get; }

        public EntityEntry(object entity)
        {
            Entity = entity;
        }
    }

    public class EntityEntry<TEntity> : EntityEntry
        where TEntity : class
    {
        public EntityEntry(TEntity entity) : base(entity)
        {
        }

        public override TEntity Entity => (TEntity)base.Entity;
    }
}
