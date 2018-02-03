using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chummer
{
    static class IListExtensions
    {
        public static void AddWithSort<T>(this IList<T> lstCollection, T objNewItem, bool blnReverse = false) where T : IComparable
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(IList<T>));
            int intTargetIndex = 0;
            for (; intTargetIndex < lstCollection.Count; ++intTargetIndex)
            {
                int intCompareResult = lstCollection[intTargetIndex].CompareTo(objNewItem);
                if (intCompareResult == 0 || (intCompareResult > 0) != blnReverse)
                {
                    break;
                }
            }
            lstCollection.Insert(intTargetIndex, objNewItem);
        }

        public static void AddWithSort<T>(this IList<T> lstCollection, T objNewItem, IComparer<T> comparer, bool blnReverse = false)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(IList<T>));
            if (comparer == null)
                throw new ArgumentNullException(nameof(IComparer<T>));
            int intTargetIndex = 0;
            for (; intTargetIndex < lstCollection.Count; ++intTargetIndex)
            {
                int intCompareResult = comparer.Compare(lstCollection[intTargetIndex], objNewItem);
                if (intCompareResult == 0 || (intCompareResult > 0) != blnReverse)
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
