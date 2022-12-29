using System;

namespace ArchivePlanner.Planning.Database
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IdAttribute: Attribute
    {
        public IdAttribute()
        {
        }
    }
}
