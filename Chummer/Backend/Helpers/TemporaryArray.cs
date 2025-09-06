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
using System.Buffers;
using System.Collections;
using System.Collections.Generic;

namespace Chummer
{
    /// <summary>
    /// Helping wrapper around an array taken from the shared array pool that will also respect the desired input size of the array when being read or enumerated.
    /// Should only be used as a way to prevent excessive heap allocations for small arrays that are only meant to be used a handful of times (but cannot be stackalloc'ed because they are for reference types).
    /// </summary>
    public readonly struct TemporaryArray<T> : IReadOnlyList<T>, IDisposable
    {
        private readonly int _intSize;
        private readonly T[] _aobjInternal;

        // Constructors do not use params keyword because that will result in (undesired) array allocation on the heap

        public TemporaryArray(T item1)
        {
            _intSize = 1;
            _aobjInternal = ArrayPool<T>.Shared.Rent(1);
            _aobjInternal[0] = item1;
        }

        public TemporaryArray(T item1, T item2)
        {
            _intSize = 2;
            _aobjInternal = ArrayPool<T>.Shared.Rent(2);
            _aobjInternal[0] = item1;
            _aobjInternal[1] = item2;
        }

        public TemporaryArray(T item1, T item2, T item3)
        {
            _intSize = 3;
            _aobjInternal = ArrayPool<T>.Shared.Rent(3);
            _aobjInternal[0] = item1;
            _aobjInternal[1] = item2;
            _aobjInternal[2] = item3;
        }

        public TemporaryArray(T item1, T item2, T item3, T item4)
        {
            _intSize = 4;
            _aobjInternal = ArrayPool<T>.Shared.Rent(4);
            _aobjInternal[0] = item1;
            _aobjInternal[1] = item2;
            _aobjInternal[2] = item3;
            _aobjInternal[3] = item4;
        }

        public T this[int index]
        {
            get
            {
                if (index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return _aobjInternal[index];
            }
            set
            {
                if (index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                _aobjInternal[index] = value;
            }
        }

        public int Count => _intSize;

        // Make sure the method you are using will 100% for sure not exceed Count!
        public T[] RawArray => _aobjInternal;

        public void Dispose()
        {
            ArrayPool<T>.Shared.Return(_aobjInternal);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new SZArrayEnumerator(_aobjInternal, Count);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new SZArrayEnumerator(_aobjInternal, Count);
        }

        [Serializable]
        private sealed class SZArrayEnumerator : IEnumerator<T>
        {
            private T[] _array;

            private int _index;

            private int _endIndex;

            public T Current => _array[_index];

            object IEnumerator.Current => Current;

            internal SZArrayEnumerator(T[] array, int intCount)
            {
                _array = array;
                _index = -1;
                _endIndex = Math.Min(intCount, array.Length);
            }

            public bool MoveNext()
            {
                if (_index < _endIndex)
                {
                    _index++;
                    return _index < _endIndex;
                }

                return false;
            }

            public void Reset()
            {
                _index = -1;
            }

            public void Dispose()
            {
            }
        }
    }
}
