using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;


namespace CustomFloorPlugin.Helpers
{
    internal static class CollectionHelper
    {
        internal static void AddSorted<T>(this ObservableCollection<T> observableCollection, int index, int count, T item) where T : IComparable<T>
        {
            if (observableCollection.Count == 0 || observableCollection[observableCollection.Count - 1].CompareTo(item) <= 0)
            {
                observableCollection.Add(item);
                return;
            }

            int sortedIndex = observableCollection.ToList().BinarySearch(index, count, item, null);
            if (sortedIndex < 0) sortedIndex = ~sortedIndex;
            observableCollection.Insert(sortedIndex, item);
        }

        internal static void AddSorted<T>(this List<T> list, int index, int count, T item, IComparer<T> comparer)
        {
            if (list.Count == 0 || comparer.Compare(list[list.Count - 1], item) <= 0)
            {
                list.Add(item);
                return;
            }

            int sortedIndex = list.BinarySearch(index, count, item, comparer);
            if (sortedIndex < 0) sortedIndex = ~sortedIndex;
            list.Insert(sortedIndex, item);
        }
    }
}