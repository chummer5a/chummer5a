/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Chummer
{
    public static class ObservableCollectionExtensions
    {
        public static void Sort<T>(this ObservableCollection<T> lstCollection, int index, int count, IComparer<T> comparer)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            if (index > lstCollection.Count)
                return;
            if (lstCollection.Count < index + count)
                count = lstCollection.Count - index;
            List<T> lstSorted = new List<T>(lstCollection.Count);
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
                throw new ArgumentNullException(nameof(lstCollection));
            T[] lstSorted = lstCollection.ToArray();
            Array.Sort(lstSorted, comparison);
            for (int i = 0; i < lstSorted.Length; ++i)
                lstCollection.Move(lstCollection.IndexOf(lstSorted[i]), i);
        }

        public static void Sort<T>(this ObservableCollection<T> lstCollection)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            T[] lstSorted = lstCollection.ToArray();
            Array.Sort(lstSorted);
            for (int i = 0; i < lstSorted.Length; ++i)
                lstCollection.Move(lstCollection.IndexOf(lstSorted[i]), i);
        }

        public static void Sort<T>(this ObservableCollection<T> lstCollection, IComparer<T> comparer)
        {
            if (lstCollection == null)
                throw new ArgumentNullException(nameof(lstCollection));
            T[] lstSorted = lstCollection.ToArray();
            Array.Sort(lstSorted, comparer);
            for (int i = 0; i < lstSorted.Length; ++i)
                lstCollection.Move(lstCollection.IndexOf(lstSorted[i]), i);
        }
    }
}
