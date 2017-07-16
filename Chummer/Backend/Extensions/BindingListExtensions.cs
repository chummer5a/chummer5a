using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Chummer.Backend
{ 
    static class BindingListExtensions
    {
        internal static void MergeInto<T>(this BindingList<T> list, IEnumerable<T> items, Comparison<T> comparison)
        {
            if (list == null) throw new NullReferenceException(nameof(list));
            if (items == null) throw new NullReferenceException(nameof(items));
            if (comparison == null) throw new NullReferenceException(nameof(comparison));

            foreach (T item in items)
            {
                list.MergeInto(item, comparison);
            }
        }

        internal static void MergeInto<T>(this BindingList<T> list, T item, Comparison<T> comparison)
        {
            if (list == null) throw new NullReferenceException(nameof(list));
            if (item == null) throw new NullReferenceException(nameof(item));
            if (comparison == null) throw new NullReferenceException(nameof(comparison));

            //if (list.Count == 0)
            //{
            //    list.Add(item);
            //    return;
            //}

            int mergeIndex = 0;

            while (list.Count > mergeIndex && comparison(list[mergeIndex], item) < 0)
                mergeIndex++;

            list.Insert(mergeIndex, item);

        }

        internal static void RemoveAll<T>(this BindingList<T> list, Predicate<T> predicate)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (predicate(list[i]))
                {
                    list.RemoveAt(i);
                }
            }
        }

        internal static void AddRange<T>(this BindingList<T> list, IEnumerable<T> range)
        {
            foreach (T item in range)
            {
                list.Add(item);
            }
        }

    }
}
