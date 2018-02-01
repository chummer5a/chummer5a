using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chummer
{
    static class IListExtensions
    {
        public static void AddWithSort<T>(this IList<T> lstCollection, T objNewItem) where T : IComparable
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(IList<T>));
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

        public static void AddWithSort<T>(this IList<T> lstCollection, T objNewItem, IComparer<T> comparer)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(IList<T>));
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

        public static void AddRange<T>(this IList<T> lstCollection, IEnumerable<T> lstToAdd)
        {
            foreach (T objItem in lstToAdd)
                lstCollection.Add(objItem);
        }
    }
}
