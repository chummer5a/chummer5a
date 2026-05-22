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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Chummer
{
    /// <summary>
    /// Like <see cref="HashCode" />, but whose hash is handled in a way that is invariant to the order in which members are added.
    /// Mainly useful for getting hash codes of ensembles of order-invariant collections, like sets or dictionaries, where we still want collections that contain the same members just in a different order to still return identical hashes.
    /// </summary>
    public struct OrderInvariantHashCode
    {
        // Same constants that are used in the HashCode struct.
        private const uint Prime1 = 2654435761U;
        private const uint Prime2 = 2246822519U;
        private const uint Prime3 = 3266489917U;
        private const uint Prime4 = 668265263U;
        private const uint Prime5 = 374761393U;

        private const uint ModInvPrime2 = 3066638151U; // modular multiplicative inverse of Prime2 = 2246822519U against mod 2^32
        private const uint ModInvPrime3 = 2828982549U; // modular multiplicative inverse of Prime3 = 3266489917U against mod 2^32

        private static readonly uint s_seed = ReverseEngineerHashCodeGlobalSeed();

        private uint _length;
        private uint _innerHash;

        // In order to make sure we have the same seed as what's used in the HashCode struct, we need to generate an empty HashCode and then reverse the hashing operations
        // This would be a lot easier if you could just access the seed directly, but data encapsulation is what it is.
        private static uint ReverseEngineerHashCodeGlobalSeed()
        {
            unchecked
            {
                uint uintEmptyHash = (uint)new HashCode().ToHashCode();
                // Reversing HashCode.MixFinal()
                uintEmptyHash ^= uintEmptyHash >> 16; // Reverse hash ^= hash >> 16
                uintEmptyHash *= ModInvPrime3; // Reverse hash *= Prime3
                // Reverse hash ^= hash >> 13, which needs to be done in 2 steps because uint has 32 bits
                uint uintHelper = uintEmptyHash ^ ((uintEmptyHash >> 13) & 0xFFFFFFC0);
                uintEmptyHash = uintHelper ^ ((uintHelper >> 13) & 0x3F);
                uintEmptyHash *= ModInvPrime2; // Reverse hash *= Prime2
                // Reverse hash ^= hash >> 15, which needs to be done in 2 steps because uint has 32 bits
                uintHelper = uintEmptyHash ^ ((uintEmptyHash >> 15) & 0xFFFFFFFC);
                uintEmptyHash = uintHelper ^ ((uintHelper >> 15) & 0x3);
                // Done reversing HashCode.MixFinal(), now reverse HashCode.MixEmptyState() to get seed
                return uintEmptyHash - Prime5;
            }
        }

        /// <inheritdoc cref="HashCode.Combine{T1}(T1)"/>
        public static int Combine<T1>(T1 value1)
        {
            unchecked
            {
                uint hc1 = (uint)(value1?.GetHashCode() ?? 0);

                uint innerHash = hc1;

                uint hash = MixEmptyState();
                hash += 4;

                hash = QueueRound(hash, innerHash);

                hash = MixFinal(hash);
                return (int)hash;
            }
        }

        /// <summary>
        /// Equivalent to <see cref="HashCode.Combine{T1, T2}(T1, T2)"/>, but producing a hash code that is invariant to the order of what is being combined.
        /// </summary>
        public static int Combine<T1, T2>(T1 value1, T2 value2)
        {
            unchecked
            {
                uint hc1 = (uint)(value1?.GetHashCode() ?? 0);
                uint hc2 = (uint)(value2?.GetHashCode() ?? 0);

                uint innerHash = hc1 + hc2;

                uint hash = MixEmptyState();
                hash += 8;

                hash = QueueRound(hash, innerHash);

                hash = MixFinal(hash);
                return (int)hash;
            }
        }

        /// <summary>
        /// Equivalent to <see cref="HashCode.Combine{T1, T2, T3}(T1, T2, T3)"/>, but producing a hash code that is invariant to the order of what is being combined.
        /// </summary>
        public static int Combine<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
        {
            unchecked
            {
                uint hc1 = (uint)(value1?.GetHashCode() ?? 0);
                uint hc2 = (uint)(value2?.GetHashCode() ?? 0);
                uint hc3 = (uint)(value3?.GetHashCode() ?? 0);

                uint innerHash = hc1 + hc2 + hc3;

                uint hash = MixEmptyState();
                hash += 12;

                hash = QueueRound(hash, innerHash);

                hash = MixFinal(hash);
                return (int)hash;
            }
        }

        /// <summary>
        /// Equivalent to <see cref="HashCode.Combine{T1, T2, T3, T4}(T1, T2, T3, T4)"/>, but producing a hash code that is invariant to the order of what is being combined.
        /// </summary>
        public static int Combine<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4)
        {
            unchecked
            {
                uint hc1 = (uint)(value1?.GetHashCode() ?? 0);
                uint hc2 = (uint)(value2?.GetHashCode() ?? 0);
                uint hc3 = (uint)(value3?.GetHashCode() ?? 0);
                uint hc4 = (uint)(value4?.GetHashCode() ?? 0);

                uint innerHash = hc1 + hc2 + hc3 + hc4;

                uint hash = MixEmptyState();
                hash += 16;

                hash = QueueRound(hash, innerHash);

                hash = MixFinal(hash);
                return (int)hash;
            }
        }

        /// <summary>
        /// Equivalent to <see cref="HashCode.Combine{T1, T2, T3, T4, T5}(T1, T2, T3, T4, T5)"/>, but producing a hash code that is invariant to the order of what is being combined.
        /// </summary>
        public static int Combine<T1, T2, T3, T4, T5>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5)
        {
            unchecked
            {
                uint hc1 = (uint)(value1?.GetHashCode() ?? 0);
                uint hc2 = (uint)(value2?.GetHashCode() ?? 0);
                uint hc3 = (uint)(value3?.GetHashCode() ?? 0);
                uint hc4 = (uint)(value4?.GetHashCode() ?? 0);
                uint hc5 = (uint)(value5?.GetHashCode() ?? 0);

                uint innerHash = hc1 + hc2 + hc3 + hc4 + hc5;

                uint hash = MixEmptyState();
                hash += 20;

                hash = QueueRound(hash, innerHash);

                hash = MixFinal(hash);
                return (int)hash;
            }
        }

        /// <summary>
        /// Equivalent to <see cref="HashCode.Combine{T1, T2, T3, T4, T5, T6}(T1, T2, T3, T4, T5, T6)"/>, but producing a hash code that is invariant to the order of what is being combined.
        /// </summary>
        public static int Combine<T1, T2, T3, T4, T5, T6>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6)
        {
            unchecked
            {
                uint hc1 = (uint)(value1?.GetHashCode() ?? 0);
                uint hc2 = (uint)(value2?.GetHashCode() ?? 0);
                uint hc3 = (uint)(value3?.GetHashCode() ?? 0);
                uint hc4 = (uint)(value4?.GetHashCode() ?? 0);
                uint hc5 = (uint)(value5?.GetHashCode() ?? 0);
                uint hc6 = (uint)(value6?.GetHashCode() ?? 0);

                uint innerHash = hc1 + hc2 + hc3 + hc4 + hc5 + hc6;

                uint hash = MixEmptyState();
                hash += 24;

                hash = QueueRound(hash, innerHash);

                hash = MixFinal(hash);
                return (int)hash;
            }
        }

        /// <summary>
        /// Equivalent to <see cref="HashCode.Combine{T1, T2, T3, T4, T5, T6, T7}(T1, T2, T3, T4, T5, T6, T7)"/>, but producing a hash code that is invariant to the order of what is being combined.
        /// </summary>
        public static int Combine<T1, T2, T3, T4, T5, T6, T7>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7)
        {
            unchecked
            {
                uint hc1 = (uint)(value1?.GetHashCode() ?? 0);
                uint hc2 = (uint)(value2?.GetHashCode() ?? 0);
                uint hc3 = (uint)(value3?.GetHashCode() ?? 0);
                uint hc4 = (uint)(value4?.GetHashCode() ?? 0);
                uint hc5 = (uint)(value5?.GetHashCode() ?? 0);
                uint hc6 = (uint)(value6?.GetHashCode() ?? 0);
                uint hc7 = (uint)(value7?.GetHashCode() ?? 0);

                uint innerHash = hc1 + hc2 + hc3 + hc4 + hc5 + hc6 + hc7;

                uint hash = MixEmptyState();
                hash += 28;

                hash = QueueRound(hash, innerHash);

                hash = MixFinal(hash);
                return (int)hash;
            }
        }

        /// <summary>
        /// Equivalent to <see cref="HashCode.Combine{T1, T2, T3, T4, T5, T6, T7, T8}(T1, T2, T3, T4, T5, T6, T7, T8)"/>, but producing a hash code that is invariant to the order of what is being combined.
        /// </summary>
        public static int Combine<T1, T2, T3, T4, T5, T6, T7, T8>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8)
        {
            unchecked
            {
                uint hc1 = (uint)(value1?.GetHashCode() ?? 0);
                uint hc2 = (uint)(value2?.GetHashCode() ?? 0);
                uint hc3 = (uint)(value3?.GetHashCode() ?? 0);
                uint hc4 = (uint)(value4?.GetHashCode() ?? 0);
                uint hc5 = (uint)(value5?.GetHashCode() ?? 0);
                uint hc6 = (uint)(value6?.GetHashCode() ?? 0);
                uint hc7 = (uint)(value7?.GetHashCode() ?? 0);
                uint hc8 = (uint)(value8?.GetHashCode() ?? 0);

                uint innerHash = hc1 + hc2 + hc3 + hc4 + hc5 + hc6 + hc7 + hc8;

                uint hash = MixEmptyState();
                hash += 32;

                hash = QueueRound(hash, innerHash);

                hash = MixFinal(hash);
                return (int)hash;
            }
        }

        /// <summary>
        /// Instantiates the initial state of a hash code.
        /// Identical to method of the same name in <see cref="HashCode" />, though it is only used there if there are less than 4 members whose hashes are being combined.
        /// </summary>
        private static uint MixEmptyState()
        {
            unchecked
            {
                return s_seed + Prime5;
            }
        }

        /// <summary>
        /// Applies a final mix to a hash before it is returned.
        /// Identical to a private method of the same name in <see cref="HashCode" />.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint MixFinal(uint hash)
        {
            unchecked
            {
                hash ^= hash >> 15;
                hash *= Prime2;
                hash ^= hash >> 13;
                hash *= Prime3;
                hash ^= hash >> 16;
                return hash;
            }
        }

        /// <summary>
        /// Identical to the method of the same name in <see cref="HashCode"/>, but without access to BitOperations.RotateLeft
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Round(uint hash, uint input)
        {
            unchecked
            {
                uint temp = hash + input * Prime2;
                return ((temp << 13) | (temp >> 19)) * Prime1;
            }
        }

        /// <summary>
        /// Identical to the method of the same name in <see cref="HashCode"/>, but without access to BitOperations.RotateLeft
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint QueueRound(uint hash, uint queuedValue)
        {
            unchecked
            {
                uint temp = hash + queuedValue * Prime3;
                return ((temp << 17) | (temp >> 15)) * Prime4;
            }
        }

        // NOTE: Because we are order-invariant, we can (and should) use interlocked operations to accumulate hash codes of members.
        // Being order-invariant means the nondeterministic execution of parallelized code should not cause us problems.

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(T value)
        {
            unchecked
            {
                Interlocked.Add(ref _innerHash, (uint)(value?.GetHashCode() ?? 0));
                Interlocked.Increment(ref _length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(T value, IEqualityComparer<T> comparer)
        {
            unchecked
            {
                Interlocked.Add(ref _innerHash, (uint)(comparer != null ? comparer.GetHashCode(value) : (value?.GetHashCode() ?? 0)));
                Interlocked.Increment(ref _length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task AddAsync<T>(Task<T> tskValue, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            T value = await tskValue.ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            unchecked
            {
                Interlocked.Add(ref _innerHash, (uint)(value?.GetHashCode() ?? 0));
                Interlocked.Increment(ref _length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task AddAsync<T>(Task<T> tskValue, IEqualityComparer<T> comparer, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            T value = await tskValue.ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            unchecked
            {
                Interlocked.Add(ref _innerHash, (uint)(comparer != null ? comparer.GetHashCode(value) : (value?.GetHashCode() ?? 0)));
                Interlocked.Increment(ref _length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task AddAsync<T>(T value, Task<IEqualityComparer<T>> tskComparer, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IEqualityComparer<T> comparer = await tskComparer.ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            unchecked
            {
                Interlocked.Add(ref _innerHash, (uint)(comparer != null ? comparer.GetHashCode(value) : (value?.GetHashCode() ?? 0)));
                Interlocked.Increment(ref _length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task AddAsync<T>(Task<T> task, Task<IEqualityComparer<T>> tskComparer, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            T value = await task.ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            IEqualityComparer<T> comparer = await tskComparer.ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            unchecked
            {
                Interlocked.Add(ref _innerHash, (uint)(comparer != null ? comparer.GetHashCode(value) : (value?.GetHashCode() ?? 0)));
                Interlocked.Increment(ref _length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange<T>(IEnumerable<T> values)
        {
            if (values is ICollection<T> valuesCollection)
            {
                // Set up like this so that if this is a locking collection where the read lock is released when the enumerator is disposed, we make sure we fetch its length before that read lock is released.
                using (IEnumerator<T> objEnumerator = values.GetEnumerator())
                {
                    unchecked
                    {
                        for (bool blnMoveNext = objEnumerator.MoveNext(); blnMoveNext; blnMoveNext = objEnumerator.MoveNext())
                            Interlocked.Add(ref _innerHash, (uint)(objEnumerator.Current?.GetHashCode() ?? 0));
                    }
                    Interlocked.Add(ref _length, (uint)valuesCollection.Count);
                }
            }
            else
            {
                unchecked
                {
                    foreach (T value in values)
                    {
                        Interlocked.Add(ref _innerHash, (uint)(value?.GetHashCode() ?? 0));
                        Interlocked.Increment(ref _length);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange<T>(IEnumerable<T> values, IEqualityComparer<T> comparer)
        {
            if (values == null)
                return;
            if (comparer == null)
            {
                AddRange(values);
                return;
            }

            if (values is ICollection<T> valuesCollection)
            {
                // Set up like this so that if this is a locking collection where the read lock is released when the enumerator is disposed, we make sure we fetch its length before that read lock is released.
                using (IEnumerator<T> objEnumerator = values.GetEnumerator())
                {
                    unchecked
                    {
                        for (bool blnMoveNext = objEnumerator.MoveNext(); blnMoveNext; blnMoveNext = objEnumerator.MoveNext())
                            Interlocked.Add(ref _innerHash, (uint)comparer.GetHashCode(objEnumerator.Current));
                    }
                    Interlocked.Add(ref _length, (uint)valuesCollection.Count);
                }
            }
            else
            {
                unchecked
                {
                    foreach (T value in values)
                    {
                        Interlocked.Add(ref _innerHash, (uint)comparer.GetHashCode(value));
                        Interlocked.Increment(ref _length);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange<T>(IEnumerable<T> values, int count)
        {
            if (count <= 0)
                return;
            int i = 0;
            if (values is ICollection<T> valuesCollection)
            {
                // Set up like this so that if this is a locking collection where the read lock is released when the enumerator is disposed, we make sure we fetch its length before that read lock is released.
                using (IEnumerator<T> objEnumerator = values.GetEnumerator())
                {
                    unchecked
                    {
                        for (bool blnMoveNext = objEnumerator.MoveNext(); blnMoveNext; blnMoveNext = objEnumerator.MoveNext())
                        {
                            if (++i > count)
                                break;
                            Interlocked.Add(ref _innerHash, (uint)(objEnumerator.Current?.GetHashCode() ?? 0));
                        }
                    }
                    Interlocked.Add(ref _length, (uint)Math.Min(count, valuesCollection.Count));
                }
            }
            else
            {
                unchecked
                {
                    foreach (T value in values)
                    {
                        if (++i > count)
                            break;
                        Interlocked.Add(ref _innerHash, (uint)(value?.GetHashCode() ?? 0));
                        Interlocked.Increment(ref _length);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange<T>(IEnumerable<T> values, int count, IEqualityComparer<T> comparer)
        {
            if (values == null)
                return;
            if (count <= 0)
                return;
            if (comparer == null)
            {
                AddRange(values, count);
                return;
            }
            int i = 0;
            if (values is ICollection<T> valuesCollection)
            {
                // Set up like this so that if this is a locking collection where the read lock is released when the enumerator is disposed, we make sure we fetch its length before that read lock is released.
                using (IEnumerator<T> objEnumerator = values.GetEnumerator())
                {
                    unchecked
                    {
                        for (bool blnMoveNext = objEnumerator.MoveNext(); blnMoveNext; blnMoveNext = objEnumerator.MoveNext())
                            Interlocked.Add(ref _innerHash, (uint)comparer.GetHashCode(objEnumerator.Current));
                    }
                    Interlocked.Add(ref _length, (uint)Math.Min(count, valuesCollection.Count));
                }
            }
            else
            {
                unchecked
                {
                    foreach (T value in values)
                    {
                        if (++i > count)
                            break;
                        Interlocked.Add(ref _innerHash, (uint)comparer.GetHashCode(value));
                        Interlocked.Increment(ref _length);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange<T>(IReadOnlyCollection<T> values)
        {
            if (values == null)
                return;
            // Set up like this so that if this is a locking collection where the read lock is released when the enumerator is disposed, we make sure we fetch its length before that read lock is released.
            using (IEnumerator<T> objEnumerator = values.GetEnumerator())
            {
                unchecked
                {
                    for (bool blnMoveNext = objEnumerator.MoveNext(); blnMoveNext; blnMoveNext = objEnumerator.MoveNext())
                        Interlocked.Add(ref _innerHash, (uint)(objEnumerator.Current?.GetHashCode() ?? 0));
                }
                Interlocked.Add(ref _length, (uint)values.Count);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange<T>(IReadOnlyCollection<T> values, IEqualityComparer<T> comparer)
        {
            if (values == null)
                return;
            if (comparer == null)
            {
                AddRange(values);
                return;
            }
            // Set up like this so that if this is a locking collection where the read lock is released when the enumerator is disposed, we make sure we fetch its length before that read lock is released.
            using (IEnumerator<T> objEnumerator = values.GetEnumerator())
            {
                unchecked
                {
                    for (bool blnMoveNext = objEnumerator.MoveNext(); blnMoveNext; blnMoveNext = objEnumerator.MoveNext())
                        Interlocked.Add(ref _innerHash, (uint)comparer.GetHashCode(objEnumerator.Current));
                }
                Interlocked.Add(ref _length, (uint)values.Count);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRangeParallel<T>(IEnumerable<T> values, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (values == null)
                return;

            uint result = 0;
            uint count = 0;
            Parallel.ForEach(values, () => new ValueTuple<uint, uint>(0, 0), (i, state, local) =>
            {
                if (token.IsCancellationRequested)
                    state.Stop();
                unchecked
                {
                    return state.IsStopped ? local : new ValueTuple<uint, uint>(local.Item1 + (uint)(i?.GetHashCode() ?? 0), local.Item2 + 1u);
                }
            },
            localResult =>
            {
                Interlocked.Add(ref result, localResult.Item1);
                Interlocked.Add(ref count, localResult.Item2);
            });
            token.ThrowIfCancellationRequested();
            Interlocked.Add(ref _innerHash, result);
            Interlocked.Add(ref _length, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRangeParallel<T>(IEnumerable<T> values, IEqualityComparer<T> comparer, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (values == null)
                return;
            if (comparer == null)
            {
                AddRangeParallel(values, token);
                return;
            }

            uint result = 0;
            uint count = 0;
            Parallel.ForEach(values, () => new ValueTuple<uint, uint>(0, 0), (i, state, local) =>
            {
                if (token.IsCancellationRequested)
                    state.Stop();
                unchecked
                {
                    return state.IsStopped ? local : new ValueTuple<uint, uint>(local.Item1 + (uint)comparer.GetHashCode(i), local.Item2 + 1u);
                }
            },
            localResult =>
            {
                Interlocked.Add(ref result, localResult.Item1);
                Interlocked.Add(ref count, localResult.Item2);
            });
            token.ThrowIfCancellationRequested();
            Interlocked.Add(ref _innerHash, result);
            Interlocked.Add(ref _length, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRangeParallel<T>(IReadOnlyCollection<T> values, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (values == null)
                return;

            uint result = 0;
            int count = values.Count;
            Parallel.For(0, count, () => 0u, (i, state, local) =>
            {
                if (token.IsCancellationRequested)
                    state.Stop();
                unchecked
                {
                    return state.IsStopped ? local : local + (uint)(values.ElementAtBetter(i)?.GetHashCode() ?? 0);
                }
            },
            localResult => Interlocked.Add(ref result, localResult));
            token.ThrowIfCancellationRequested();
            Interlocked.Add(ref _innerHash, result);
            Interlocked.Add(ref _length, (uint)count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRangeParallel<T>(IReadOnlyCollection<T> values, IEqualityComparer<T> comparer, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (values == null)
                return;
            if (comparer == null)
            {
                AddRangeParallel(values, token);
                return;
            }

            uint result = 0;
            int count = values.Count;
            Parallel.For(0, count, () => 0u, (i, state, local) =>
            {
                if (token.IsCancellationRequested)
                    state.Stop();
                unchecked
                {
                    return state.IsStopped ? local : local + (uint)comparer.GetHashCode(values.ElementAtBetter(i));
                }
            },
            localResult => Interlocked.Add(ref result, localResult));
            token.ThrowIfCancellationRequested();
            Interlocked.Add(ref _innerHash, result);
            Interlocked.Add(ref _length, (uint)count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task AddRangeParallelAsync<T>(IEnumerableWithAsync<T> values, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (values == null)
                return;
            uint result = 0;
            int count = 0;
            IEnumerator<T> objEnumerator = await values.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                bool blnDoCount;
                List<Task<int>> lstTasks;
                if (values is IAsyncReadOnlyCollection<T> objTemp)
                {
                    count = await objTemp.GetCountAsync(token).ConfigureAwait(false);
                    lstTasks = new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, count));
                    blnDoCount = false;
                }
                else
                {
                    lstTasks = new List<Task<int>>(Utils.MaxParallelBatchSize);
                    blnDoCount = true;
                }
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    token.ThrowIfCancellationRequested();
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => objCurrent?.GetHashCode() ?? 0, token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        unchecked
                        {
                            foreach (Task<int> tskLoop in lstTasks)
                                result += (uint)await tskLoop.ConfigureAwait(false);
                            if (blnDoCount)
                                count += Utils.MaxParallelBatchSize;
                        }
                        lstTasks.Clear();
                    }
                }
                token.ThrowIfCancellationRequested();
                await Task.WhenAll(lstTasks).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                unchecked
                {
                    foreach (Task<int> tskLoop in lstTasks)
                    {
                        result += (uint)await tskLoop.ConfigureAwait(false);
                        if (blnDoCount)
                            ++count;
                    }
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            token.ThrowIfCancellationRequested();
            Interlocked.Add(ref _innerHash, result);
            Interlocked.Add(ref _length, (uint)count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task AddRangeParallelAsync<T>(IEnumerableWithAsync<T> values, IEqualityComparer<T> comparer, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (values == null)
                return;
            if (comparer == null)
            {
                await AddRangeParallelAsync(values, token);
                return;
            }
            uint result = 0;
            int count = 0;
            IEnumerator<T> objEnumerator = await values.GetEnumeratorAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                bool blnDoCount;
                List<Task<int>> lstTasks;
                if (values is IAsyncReadOnlyCollection<T> objTemp)
                {
                    count = await objTemp.GetCountAsync(token).ConfigureAwait(false);
                    lstTasks = new List<Task<int>>(Math.Min(Utils.MaxParallelBatchSize, count));
                    blnDoCount = false;
                }
                else
                {
                    lstTasks = new List<Task<int>>(Utils.MaxParallelBatchSize);
                    blnDoCount = true;
                }
                bool blnMoveNext = objEnumerator.MoveNext();
                while (blnMoveNext)
                {
                    token.ThrowIfCancellationRequested();
                    for (int i = 0; i < Utils.MaxParallelBatchSize && blnMoveNext; ++i)
                    {
                        token.ThrowIfCancellationRequested();
                        T objCurrent = objEnumerator.Current;
                        lstTasks.Add(Task.Run(() => comparer.GetHashCode(objCurrent), token));
                        blnMoveNext = objEnumerator.MoveNext();
                    }

                    if (blnMoveNext)
                    {
                        token.ThrowIfCancellationRequested();
                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        unchecked
                        {
                            foreach (Task<int> tskLoop in lstTasks)
                                result += (uint)await tskLoop.ConfigureAwait(false);
                            if (blnDoCount)
                                count += Utils.MaxParallelBatchSize;
                        }
                        lstTasks.Clear();
                    }
                }
                token.ThrowIfCancellationRequested();
                await Task.WhenAll(lstTasks).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                unchecked
                {
                    foreach (Task<int> tskLoop in lstTasks)
                    {
                        result += (uint)await tskLoop.ConfigureAwait(false);
                        if (blnDoCount)
                            ++count;
                    }
                }
            }
            finally
            {
                if (objEnumerator is IAsyncDisposable objAsyncDisposable)
                    await objAsyncDisposable.DisposeAsync().ConfigureAwait(false);
                else
                    objEnumerator.Dispose();
            }
            token.ThrowIfCancellationRequested();
            Interlocked.Add(ref _innerHash, result);
            Interlocked.Add(ref _length, (uint)count);
        }

        /// <summary>
        /// Somewhat similar to <see cref="HashCode.ToHashCode()"/> and <see cref="HashCode.Combine{T1}(T1)"/>, but using a set of operations that is order-invariant if we have more than 1 member.
        /// The method is designed such that if we are processing no members or one member, the output will be identical to what we would get with <see cref="HashCode.ToHashCode()"/> or <see cref="HashCode.Combine{T1}(T1)"/>. 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ToHashCode()
        {
            unchecked
            {
                uint hash = MixEmptyState();
                hash += 4 * _length;

                if (_length > 0)
                    hash = QueueRound(hash, _innerHash);

                hash = MixFinal(hash);
                return (int)hash;
            }
        }
    }
}
