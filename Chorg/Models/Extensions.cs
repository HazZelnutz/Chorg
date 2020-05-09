using System.Collections.Generic;

namespace Chorg.Models
{
    public static class Extensions
    {
        /// <summary>
        /// Clears the collection and adds the items
        /// </summary>
        /// <param name="collection">The Collection.</param>
        /// <param name="items">The Items.</param>
        public static void Replace<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            collection.Clear();

            if (items == null)
                return;

            foreach (T current in items)
                collection.Add(current);
        }
    }
}
