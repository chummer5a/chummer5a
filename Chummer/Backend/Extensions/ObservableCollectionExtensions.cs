using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chummer
{
    static class ObservableCollectionExtensions
    {
        public static void Sort<T>(this ObservableCollection<T> lstCollection, int index, int count, IComparer<T> comparer)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(ObservableCollection<T>));
            if (index > lstCollection.Count)
                return;
            if (lstCollection.Count < index + count)
                count = lstCollection.Count - index;
            List<T> lstSorted = new List<T>();
            for (int i = index; i < count; ++i)
            {
                lstSorted.Add(lstCollection[i]);
            }
            lstSorted.Sort(comparer);
            for (int i = 0; i < lstSorted.Count; ++i)
                lstCollection.Move(lstCollection.IndexOf(lstSorted[i]), index + i);
        }

        public static void Sort<T>(this ObservableCollection<T> lstCollection, Comparison<T> comparison)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(ObservableCollection<T>));
            List<T> lstSorted = lstCollection.ToList();
            lstSorted.Sort(comparison);
            for (int i = 0; i < lstSorted.Count; ++i)
                lstCollection.Move(lstCollection.IndexOf(lstSorted[i]), i);
        }

        public static void Sort<T>(this ObservableCollection<T> lstCollection)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(ObservableCollection<T>));
            List<T> lstSorted = lstCollection.ToList();
            lstSorted.Sort();
            for (int i = 0; i < lstSorted.Count; ++i)
                lstCollection.Move(lstCollection.IndexOf(lstSorted[i]), i);
        }

        public static void Sort<T>(this ObservableCollection<T> lstCollection, IComparer<T> comparer)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(ObservableCollection<T>));
            List<T> lstSorted = lstCollection.ToList();
            lstSorted.Sort(comparer);
            for (int i = 0; i < lstSorted.Count; ++i)
                lstCollection.Move(lstCollection.IndexOf(lstSorted[i]), i);
        }

        public static void AddWithSort<T>(this ObservableCollection<T> lstCollection, T objNewItem) where T : IComparable
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(ObservableCollection<T>));
            int intTargetIndex = 0;
            for (; intTargetIndex < lstCollection.Count; ++intTargetIndex)
            {
                if (lstCollection[intTargetIndex].CompareTo(objNewItem) >= 0)
                {
                    break;
                }
            }
            lstCollection.Insert(intTargetIndex, objNewItem);
        }

        public static void AddWithSort<T>(this ObservableCollection<T> lstCollection, T objNewItem, IComparer<T> comparer)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(ObservableCollection<T>));
            if (comparer == null)
                throw new ArgumentNullException(nameof(IComparer<T>));
            int intTargetIndex = 0;
            for (; intTargetIndex < lstCollection.Count; ++intTargetIndex)
            {
                if (comparer.Compare(lstCollection[intTargetIndex], objNewItem) >= 0)
                {
                    break;
                }
            }
            lstCollection.Insert(intTargetIndex, objNewItem);
        }
    }
}
