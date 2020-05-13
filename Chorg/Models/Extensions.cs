using System.Collections.Generic;
using Chorg.ViewModels;
using Chorg.Models;

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

        public static void Remove(this ICollection<ChartThumbnailViewModel> collection, Chart item)
        {
            foreach (var viewModel in collection)
            {
                if(viewModel.GetModel().Id == item.Id)
                {
                    collection.Remove(viewModel);
                    return;
                }
            }
        }
    }
}
