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

using System.Collections;
using System.Collections.Generic;

namespace Chummer
{
    public class ThreadSafeStack<T> : Stack<T>, ICollection
    {
        private readonly object _objLock = new object();

        public new int Count
        {
            get
            {
                lock (_objLock)
                    return base.Count;
            }
        }

        bool ICollection.IsSynchronized => true;

        object ICollection.SyncRoot => _objLock;

        public new void Clear()
        {
            lock (_objLock)
                base.Clear();
        }

        public new bool Contains(T item)
        {
            lock (_objLock)
                return base.Contains(item);
        }

        public new void CopyTo(T[] array, int arrayIndex)
        {
            lock (_objLock)
                base.CopyTo(array, arrayIndex);
        }

        public new Enumerator GetEnumerator()
        {
            lock (_objLock)
                return base.GetEnumerator();
        }

        public new void TrimExcess()
        {
            lock (_objLock)
                base.TrimExcess();
        }

        public new T Peek()
        {
            lock (_objLock)
                return base.Count > 0 ? base.Peek() : default;
        }

        public new T Pop()
        {
            lock (_objLock)
                return base.Count > 0 ? base.Pop() : default;
        }

        public new void Push(T item)
        {
            lock (_objLock)
                base.Push(item);
        }

        public new T[] ToArray()
        {
            lock (_objLock)
                return base.ToArray();
        }
    }
}
