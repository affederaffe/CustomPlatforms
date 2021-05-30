using System;
using System.Collections.Generic;
using System.Linq;


namespace CustomFloorPlugin.Helpers
{
    internal static class CollectionHelper
    {
        internal static void AddSorted<T>(this IList<T> list, int index, int count, T item, IComparer<T>? comparer)
        {
            comparer ??= Comparer<T>.Default;

            if (list.Count == 0 || comparer.Compare(list[list.Count - 1], item) <= 0)
            {
                list.Add(item);
                return;
            }

            int sortedIndex = list.ToList().BinarySearch(index, count, item, comparer);
            if (sortedIndex < 0) sortedIndex = ~sortedIndex;
            list.Insert(sortedIndex, item);
        }

        internal static bool TryGetFirst<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate, out T item)
        {
            item = enumerable.FirstOrDefault(predicate);
            return item is not null;
        }
    }
}