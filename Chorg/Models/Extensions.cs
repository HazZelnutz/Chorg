using System.Collections.Generic;
using Chorg.ViewModels;
using Chorg.Models;
using Chorg.Views;

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

        public static void Remove<T>(this ICollection<T> collection, Chart model) where T : ChartViewModel
        {
            foreach (var viewModel in collection)
            {
                if(viewModel.GetModel().Id == model.Id)
                {
                    collection.Remove(viewModel);
                    return;
                }
            }
        }

        public static ChartViewModel GetByModel<T>(this ICollection<T> collection, Chart model) where T : ChartViewModel
        {
            foreach (var item in collection)
            {
                if (item.GetModel() == model)
                    return item;
            }

            return null;
        }
    }
}
