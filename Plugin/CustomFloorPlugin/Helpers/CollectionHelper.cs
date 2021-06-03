using System;
using System.Collections.Generic;
using System.Linq;


namespace CustomFloorPlugin.Helpers
{
    internal static class CollectionHelper
    {
        internal static void AddSorted<T>(this IList<T> list, int index, int count, T value, IComparer<T>? comparer = null)
        {
            comparer ??= Comparer<T>.Default;

            if (list.Count == 0 || comparer.Compare(list[list.Count - 1], value) <= 0)
            {
                list.Add(value);
                return;
            }

            int sortedIndex = list.BinarySearch(index, count, value, comparer);
            list.Insert(sortedIndex, value);
        }

        internal static bool TryGetFirst<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate, out T item)
        {
            item = enumerable.FirstOrDefault(predicate);
            return item is not null;
        }

        internal static void Replace<T>(this IList<T> list, T oldItem, T newItem)
        {
            int index = list.IndexOf(oldItem);
            if (index is -1) return;
            list[index] = newItem;
        }

        private static int BinarySearch<T>(this IList<T> list, int index, int count, T value, IComparer<T> comparer)
        {
            int lower = index;
            int upper = index + count - 1;

            while (lower <= upper)
            {
                int middle = lower + ((upper - lower) / 2);
                int comparisonResult = comparer.Compare(value, list[middle]);
                if (comparisonResult == 0) return middle;
                if (comparisonResult < 0) upper = middle - 1;
                else lower = middle + 1;
            }

            return lower;
        }
    }
}