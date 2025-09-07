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
using System.Threading;
using System.Threading.Tasks;

namespace Chummer
{
    /// <summary>
    /// Helping wrapper around an array taken from the shared array pool that will also respect the desired input size of the array when being read or enumerated.
    /// Should only be used as a way to prevent excessive heap allocations for small arrays that are only meant to be used a handful of times (but cannot be stackalloc'ed because they are for reference types).
    /// </summary>
    public readonly struct TemporaryArray<T> : IReadOnlyList<T>, IDisposable, IEquatable<TemporaryArray<T>> // Note: array is *not* read-only, only its size is unchangeable, it's just that there is no good interface for typed arrays
    {
        private readonly int _intSize;
        private readonly T[] _aobjInternal;

        // Constructors do not use params keyword because that will result in (undesired) array allocation on the heap

        public TemporaryArray(T[] items)
        {
            _intSize = items.Length;
            if (_intSize == 0)
                _aobjInternal = Array.Empty<T>();
            else
            {
                _aobjInternal = ArrayPool<T>.Shared.Rent(_intSize);
                Array.Copy(items, _aobjInternal, _intSize);
            }
        }

        public TemporaryArray(IReadOnlyCollection<T> items)
        {
            _intSize = items.Count;
            if (_intSize == 0)
                _aobjInternal = Array.Empty<T>();
            else
            {
                _aobjInternal = ArrayPool<T>.Shared.Rent(_intSize);
                int i = 0;
                foreach (T item in items)
                    _aobjInternal[i++] = item;
            }
        }

        public TemporaryArray(IReadOnlyList<T> items)
        {
            _intSize = items.Count;
            if (_intSize == 0)
                _aobjInternal = Array.Empty<T>();
            else
            {
                _aobjInternal = ArrayPool<T>.Shared.Rent(_intSize);
                for (int i = 0; i < _intSize; ++i)
                    _aobjInternal[i] = items[i];
            }
        }

        public TemporaryArray(IEnumerable<T> items)
        {
            using (IEnumerator<T> enumerator = items.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    T item1 = enumerator.Current;
                    if (enumerator.MoveNext())
                    {
                        T item2 = enumerator.Current;
                        if (enumerator.MoveNext())
                        {
                            T item3 = enumerator.Current;
                            if (enumerator.MoveNext())
                            {
                                T item4 = enumerator.Current;
                                if (enumerator.MoveNext())
                                {
                                    T item5 = enumerator.Current;
                                    if (enumerator.MoveNext())
                                    {
                                        T item6 = enumerator.Current;
                                        if (enumerator.MoveNext())
                                        {
                                            T item7 = enumerator.Current;
                                            if (enumerator.MoveNext())
                                            {
                                                T item8 = enumerator.Current;
                                                if (enumerator.MoveNext())
                                                {
                                                    throw new ArgumentOutOfRangeException(nameof(items));
                                                }
                                                else
                                                {
                                                    _intSize = 8;
                                                    _aobjInternal = ArrayPool<T>.Shared.Rent(8);
                                                }
                                                _aobjInternal[7] = item8;
                                            }
                                            else
                                            {
                                                _intSize = 7;
                                                _aobjInternal = ArrayPool<T>.Shared.Rent(7);
                                            }
                                            _aobjInternal[6] = item7;
                                        }
                                        else
                                        {
                                            _intSize = 6;
                                            _aobjInternal = ArrayPool<T>.Shared.Rent(6);
                                        }
                                        _aobjInternal[5] = item6;
                                    }
                                    else
                                    {
                                        _intSize = 5;
                                        _aobjInternal = ArrayPool<T>.Shared.Rent(5);
                                    }
                                    _aobjInternal[4] = item5;
                                }
                                else
                                {
                                    _intSize = 4;
                                    _aobjInternal = ArrayPool<T>.Shared.Rent(4);
                                }
                                _aobjInternal[3] = item4;
                            }
                            else
                            {
                                _intSize = 3;
                                _aobjInternal = ArrayPool<T>.Shared.Rent(3);
                            }
                            _aobjInternal[2] = item3;
                        }
                        else
                        {
                            _intSize = 2;
                            _aobjInternal = ArrayPool<T>.Shared.Rent(2);
                        }
                        _aobjInternal[1] = item2;
                    }
                    else
                    {
                        _intSize = 1;
                        _aobjInternal = ArrayPool<T>.Shared.Rent(1);
                    }
                    _aobjInternal[0] = item1;
                }
                else
                {
                    _intSize = 0;
                    _aobjInternal = Array.Empty<T>();
                }
            }
        }

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

        public TemporaryArray(T item1, T item2, T item3, T item4, T item5)
        {
            _intSize = 5;
            _aobjInternal = ArrayPool<T>.Shared.Rent(5);
            _aobjInternal[0] = item1;
            _aobjInternal[1] = item2;
            _aobjInternal[2] = item3;
            _aobjInternal[3] = item4;
            _aobjInternal[4] = item5;
        }

        public TemporaryArray(T item1, T item2, T item3, T item4, T item5, T item6)
        {
            _intSize = 6;
            _aobjInternal = ArrayPool<T>.Shared.Rent(6);
            _aobjInternal[0] = item1;
            _aobjInternal[1] = item2;
            _aobjInternal[2] = item3;
            _aobjInternal[3] = item4;
            _aobjInternal[4] = item5;
            _aobjInternal[5] = item6;
        }

        public TemporaryArray(T item1, T item2, T item3, T item4, T item5, T item6, T item7)
        {
            _intSize = 7;
            _aobjInternal = ArrayPool<T>.Shared.Rent(7);
            _aobjInternal[0] = item1;
            _aobjInternal[1] = item2;
            _aobjInternal[2] = item3;
            _aobjInternal[3] = item4;
            _aobjInternal[4] = item5;
            _aobjInternal[5] = item6;
            _aobjInternal[6] = item7;
        }

        public TemporaryArray(T item1, T item2, T item3, T item4, T item5, T item6, T item7, T item8)
        {
            _intSize = 8;
            _aobjInternal = ArrayPool<T>.Shared.Rent(8);
            _aobjInternal[0] = item1;
            _aobjInternal[1] = item2;
            _aobjInternal[2] = item3;
            _aobjInternal[3] = item4;
            _aobjInternal[4] = item5;
            _aobjInternal[5] = item6;
            _aobjInternal[6] = item7;
            _aobjInternal[7] = item8;
        }

        public static async Task<TemporaryArray<T>> NewAsync(IAsyncEnumerable<T> items, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (IEnumerator<T> enumerator = await items.GetEnumeratorAsync(token).ConfigureAwait(false))
            {
                if (enumerator.MoveNext())
                {
                    T item1 = enumerator.Current;
                    if (enumerator.MoveNext())
                    {
                        T item2 = enumerator.Current;
                        if (enumerator.MoveNext())
                        {
                            T item3 = enumerator.Current;
                            if (enumerator.MoveNext())
                            {
                                T item4 = enumerator.Current;
                                if (enumerator.MoveNext())
                                {
                                    T item5 = enumerator.Current;
                                    if (enumerator.MoveNext())
                                    {
                                        T item6 = enumerator.Current;
                                        if (enumerator.MoveNext())
                                        {
                                            T item7 = enumerator.Current;
                                            if (enumerator.MoveNext())
                                            {
                                                T item8 = enumerator.Current;
                                                if (enumerator.MoveNext())
                                                {
                                                    throw new ArgumentOutOfRangeException(nameof(items));
                                                }
                                                else
                                                {
                                                    return new TemporaryArray<T>(item1, item2, item3, item4, item5, item6, item7, item8);
                                                }
                                            }
                                            else
                                            {
                                                return new TemporaryArray<T>(item1, item2, item3, item4, item5, item6, item7);
                                            }
                                        }
                                        else
                                        {
                                            return new TemporaryArray<T>(item1, item2, item3, item4, item5, item6);
                                        }
                                    }
                                    else
                                    {
                                        return new TemporaryArray<T>(item1, item2, item3, item4, item5);
                                    }
                                }
                                else
                                {
                                    return new TemporaryArray<T>(item1, item2, item3, item4);
                                }
                            }
                            else
                            {
                                return new TemporaryArray<T>(item1, item2, item3);
                            }
                        }
                        else
                        {
                            return new TemporaryArray<T>(item1, item2);
                        }
                    }
                    else
                    {
                        return new TemporaryArray<T>(item1);
                    }
                }
                else
                {
                    return new TemporaryArray<T>(Array.Empty<T>());
                }
            }
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
            if (Count > 0)
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

        public bool Equals(TemporaryArray<T> other)
        {
            return RawArray.Equals(other.RawArray);
        }

        public override bool Equals(object obj)
        {
            return obj is TemporaryArray<T> && Equals((TemporaryArray<T>)obj);
        }

        public static bool operator ==(TemporaryArray<T> left, TemporaryArray<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TemporaryArray<T> left, TemporaryArray<T> right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return RawArray.GetHashCode();
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
