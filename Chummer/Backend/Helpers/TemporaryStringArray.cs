/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUstring ANY WARRANTY; without even the implied warranty of
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
    /// Helping wrapper around an objArray taken from the shared objArray pool that will also respect the desired input size of the objArray when being read or enumerated.
    /// Should only be used as a way to prevent excessive heap allocations for small arrays that are only meant to be used a handful of times (but cannot be stackalloc'ed because they are for reference types).
    /// </summary>
    public readonly struct TemporaryStringArray : IReadOnlyList<string>, IDisposable, IEquatable<TemporaryStringArray> // Note: objArray is *not* read-only, only its size is unchangeable, it's just that there is no good interface for typed arrays
    {
        private readonly int _intSize;
        private readonly string[] _aobjInternal;

        // Constructors do not use params keyword because that will result in (undesired) objArray allocation on the heap

        public TemporaryStringArray(string[] items)
        {
            _intSize = items.Length;
            if (_intSize == 0)
                _aobjInternal = Array.Empty<string>();
            else
            {
                _aobjInternal = ArrayPool<string>.Shared.Rent(_intSize);
                Array.Copy(items, _aobjInternal, _intSize);
            }
        }

        public TemporaryStringArray(IReadOnlyCollection<string> items)
        {
            _intSize = items.Count;
            if (_intSize == 0)
                _aobjInternal = Array.Empty<string>();
            else
            {
                _aobjInternal = ArrayPool<string>.Shared.Rent(_intSize);
                int i = 0;
                foreach (string item in items)
                    _aobjInternal[i++] = item;
            }
        }

        public TemporaryStringArray(IReadOnlyList<string> items)
        {
            _intSize = items.Count;
            if (_intSize == 0)
                _aobjInternal = Array.Empty<string>();
            else
            {
                _aobjInternal = ArrayPool<string>.Shared.Rent(_intSize);
                for (int i = 0; i < _intSize; ++i)
                    _aobjInternal[i] = items[i];
            }
        }

        public TemporaryStringArray(IEnumerable<string> items)
        {
            using (IEnumerator<string> enumerator = items.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    string item1 = enumerator.Current;
                    if (enumerator.MoveNext())
                    {
                        string item2 = enumerator.Current;
                        if (enumerator.MoveNext())
                        {
                            string item3 = enumerator.Current;
                            if (enumerator.MoveNext())
                            {
                                string item4 = enumerator.Current;
                                if (enumerator.MoveNext())
                                {
                                    string item5 = enumerator.Current;
                                    if (enumerator.MoveNext())
                                    {
                                        string item6 = enumerator.Current;
                                        if (enumerator.MoveNext())
                                        {
                                            string item7 = enumerator.Current;
                                            if (enumerator.MoveNext())
                                            {
                                                string item8 = enumerator.Current;
                                                if (enumerator.MoveNext())
                                                {
                                                    throw new ArgumentOutOfRangeException(nameof(items));
                                                }
                                                else
                                                {
                                                    _intSize = 8;
                                                    _aobjInternal = ArrayPool<string>.Shared.Rent(8);
                                                }
                                                _aobjInternal[7] = item8;
                                            }
                                            else
                                            {
                                                _intSize = 7;
                                                _aobjInternal = ArrayPool<string>.Shared.Rent(7);
                                            }
                                            _aobjInternal[6] = item7;
                                        }
                                        else
                                        {
                                            _intSize = 6;
                                            _aobjInternal = ArrayPool<string>.Shared.Rent(6);
                                        }
                                        _aobjInternal[5] = item6;
                                    }
                                    else
                                    {
                                        _intSize = 5;
                                        _aobjInternal = ArrayPool<string>.Shared.Rent(5);
                                    }
                                    _aobjInternal[4] = item5;
                                }
                                else
                                {
                                    _intSize = 4;
                                    _aobjInternal = ArrayPool<string>.Shared.Rent(4);
                                }
                                _aobjInternal[3] = item4;
                            }
                            else
                            {
                                _intSize = 3;
                                _aobjInternal = ArrayPool<string>.Shared.Rent(3);
                            }
                            _aobjInternal[2] = item3;
                        }
                        else
                        {
                            _intSize = 2;
                            _aobjInternal = ArrayPool<string>.Shared.Rent(2);
                        }
                        _aobjInternal[1] = item2;
                    }
                    else
                    {
                        _intSize = 1;
                        _aobjInternal = ArrayPool<string>.Shared.Rent(1);
                    }
                    _aobjInternal[0] = item1;
                }
                else
                {
                    _intSize = 0;
                    _aobjInternal = Array.Empty<string>();
                }
            }
        }

        public TemporaryStringArray(string item1)
        {
            _intSize = 1;
            _aobjInternal = ArrayPool<string>.Shared.Rent(1);
            _aobjInternal[0] = item1;
        }

        public TemporaryStringArray(string item1, string item2)
        {
            _intSize = 2;
            _aobjInternal = ArrayPool<string>.Shared.Rent(2);
            _aobjInternal[0] = item1;
            _aobjInternal[1] = item2;
        }

        public TemporaryStringArray(string item1, string item2, string item3)
        {
            _intSize = 3;
            _aobjInternal = ArrayPool<string>.Shared.Rent(3);
            _aobjInternal[0] = item1;
            _aobjInternal[1] = item2;
            _aobjInternal[2] = item3;
        }

        public TemporaryStringArray(string item1, string item2, string item3, string item4)
        {
            _intSize = 4;
            _aobjInternal = ArrayPool<string>.Shared.Rent(4);
            _aobjInternal[0] = item1;
            _aobjInternal[1] = item2;
            _aobjInternal[2] = item3;
            _aobjInternal[3] = item4;
        }

        public TemporaryStringArray(string item1, string item2, string item3, string item4, string item5)
        {
            _intSize = 5;
            _aobjInternal = ArrayPool<string>.Shared.Rent(5);
            _aobjInternal[0] = item1;
            _aobjInternal[1] = item2;
            _aobjInternal[2] = item3;
            _aobjInternal[3] = item4;
            _aobjInternal[4] = item5;
        }

        public TemporaryStringArray(string item1, string item2, string item3, string item4, string item5, string item6)
        {
            _intSize = 6;
            _aobjInternal = ArrayPool<string>.Shared.Rent(6);
            _aobjInternal[0] = item1;
            _aobjInternal[1] = item2;
            _aobjInternal[2] = item3;
            _aobjInternal[3] = item4;
            _aobjInternal[4] = item5;
            _aobjInternal[5] = item6;
        }

        public TemporaryStringArray(string item1, string item2, string item3, string item4, string item5, string item6, string item7)
        {
            _intSize = 7;
            _aobjInternal = ArrayPool<string>.Shared.Rent(7);
            _aobjInternal[0] = item1;
            _aobjInternal[1] = item2;
            _aobjInternal[2] = item3;
            _aobjInternal[3] = item4;
            _aobjInternal[4] = item5;
            _aobjInternal[5] = item6;
            _aobjInternal[6] = item7;
        }

        public TemporaryStringArray(string item1, string item2, string item3, string item4, string item5, string item6, string item7, string item8)
        {
            _intSize = 8;
            _aobjInternal = ArrayPool<string>.Shared.Rent(8);
            _aobjInternal[0] = item1;
            _aobjInternal[1] = item2;
            _aobjInternal[2] = item3;
            _aobjInternal[3] = item4;
            _aobjInternal[4] = item5;
            _aobjInternal[5] = item6;
            _aobjInternal[6] = item7;
            _aobjInternal[7] = item8;
        }

        public static async Task<TemporaryStringArray> NewAsync(IAsyncEnumerable<string> items, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (IEnumerator<string> enumerator = await items.GetEnumeratorAsync(token).ConfigureAwait(false))
            {
                if (enumerator.MoveNext())
                {
                    string item1 = enumerator.Current;
                    if (enumerator.MoveNext())
                    {
                        string item2 = enumerator.Current;
                        if (enumerator.MoveNext())
                        {
                            string item3 = enumerator.Current;
                            if (enumerator.MoveNext())
                            {
                                string item4 = enumerator.Current;
                                if (enumerator.MoveNext())
                                {
                                    string item5 = enumerator.Current;
                                    if (enumerator.MoveNext())
                                    {
                                        string item6 = enumerator.Current;
                                        if (enumerator.MoveNext())
                                        {
                                            string item7 = enumerator.Current;
                                            if (enumerator.MoveNext())
                                            {
                                                string item8 = enumerator.Current;
                                                if (enumerator.MoveNext())
                                                {
                                                    throw new ArgumentOutOfRangeException(nameof(items));
                                                }
                                                else
                                                {
                                                    return new TemporaryStringArray(item1, item2, item3, item4, item5, item6, item7, item8);
                                                }
                                            }
                                            else
                                            {
                                                return new TemporaryStringArray(item1, item2, item3, item4, item5, item6, item7);
                                            }
                                        }
                                        else
                                        {
                                            return new TemporaryStringArray(item1, item2, item3, item4, item5, item6);
                                        }
                                    }
                                    else
                                    {
                                        return new TemporaryStringArray(item1, item2, item3, item4, item5);
                                    }
                                }
                                else
                                {
                                    return new TemporaryStringArray(item1, item2, item3, item4);
                                }
                            }
                            else
                            {
                                return new TemporaryStringArray(item1, item2, item3);
                            }
                        }
                        else
                        {
                            return new TemporaryStringArray(item1, item2);
                        }
                    }
                    else
                    {
                        return new TemporaryStringArray(item1);
                    }
                }
                else
                {
                    return new TemporaryStringArray(Array.Empty<string>());
                }
            }
        }

        public string this[int index]
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
        public string[] RawArray => _aobjInternal;

        public void Dispose()
        {
            if (Count > 0)
                ArrayPool<string>.Shared.Return(_aobjInternal);
        }

        public IEnumerator<string> GetEnumerator()
        {
            return new SZArrayEnumerator(_aobjInternal, Count);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new SZArrayEnumerator(_aobjInternal, Count);
        }

        public bool Equals(TemporaryStringArray other)
        {
            if (Count != other._intSize)
                return false;
            for (int i = 0; i < _intSize; ++i)
            {
                if (!_aobjInternal[i].Equals(other._aobjInternal[i]))
                    return false;
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            return obj is TemporaryStringArray objArray && Equals(objArray);
        }

        public static bool operator ==(TemporaryStringArray left, TemporaryStringArray right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TemporaryStringArray left, TemporaryStringArray right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return _aobjInternal.GetEnsembleHashCode(_intSize);
        }

        [Serializable]
        private sealed class SZArrayEnumerator : IEnumerator<string>
        {
            private readonly string[] _array;

            private int _index;

            private readonly int _endIndex;

            public string Current => _array[_index];

            object IEnumerator.Current => Current;

            internal SZArrayEnumerator(string[] array, int intCount)
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
