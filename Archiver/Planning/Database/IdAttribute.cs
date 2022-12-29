using System;

namespace Archiver.Planning.Database
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IdAttribute: Attribute
    {
        public IdAttribute()
        {
        }
    }
}
