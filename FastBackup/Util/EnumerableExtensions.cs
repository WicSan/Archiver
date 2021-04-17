using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastBackup.Util
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> AddRange<T>(this ICollection<T> enumerable, IEnumerable<T> itemsToAdd)
        {
            foreach (var item in itemsToAdd)
            {
                enumerable.Append(item);
            }

            return enumerable;
        }
    }
}
