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
using System.Threading;
using System.Threading.Tasks;

namespace Chummer
{
    /// <summary>
    /// Pairs Random with a lock object and a cache of ints that it generates in a thread-safe manner.
    /// Access to the cache is guaranteed to be thread-safe and also overall faster than just using locking alone.
    /// </summary>
    public class ThreadSafeCachedRandom : ThreadSafeRandom
    {
        private int _intCurrentCacheIndex = -1;
        private const int DefaultCacheSize = 1024;
        private readonly int[] _aintCache;

        public ThreadSafeCachedRandom(bool blnImmediatelyRegenerateCache = false)
        {
            _aintCache = ArrayPool<int>.Shared.Rent(DefaultCacheSize);
            if (blnImmediatelyRegenerateCache)
                RegenerateCache();
            else
                _intCurrentCacheIndex = DefaultCacheSize; // Guarantees that the first call will rebuild the cache
        }

        public ThreadSafeCachedRandom(int seed, bool blnImmediatelyRegenerateCache = false) : base(seed)
        {
            _aintCache = ArrayPool<int>.Shared.Rent(DefaultCacheSize);
            if (blnImmediatelyRegenerateCache)
                RegenerateCache();
            else
                _intCurrentCacheIndex = DefaultCacheSize; // Guarantees that the first call will rebuild the cache
        }

        public ThreadSafeCachedRandom(Random objRandom, bool blnImmediatelyRegenerateCache = false) : base(objRandom)
        {
            _aintCache = ArrayPool<int>.Shared.Rent(DefaultCacheSize);
            if (blnImmediatelyRegenerateCache)
                RegenerateCache();
            else
                _intCurrentCacheIndex = DefaultCacheSize; // Guarantees that the first call will rebuild the cache
        }

        public ThreadSafeCachedRandom(int seed, int cacheSize, bool blnImmediatelyRegenerateCache = false) : base(seed)
        {
            if (cacheSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(cacheSize));
            _aintCache = ArrayPool<int>.Shared.Rent(cacheSize);
            if (blnImmediatelyRegenerateCache)
                RegenerateCache();
            else
                _intCurrentCacheIndex = cacheSize; // Guarantees that the first call will rebuild the cache
        }

        public ThreadSafeCachedRandom(Random objRandom, int cacheSize, bool blnImmediatelyRegenerateCache = false) : base(objRandom)
        {
            if (cacheSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(cacheSize));
            _aintCache = ArrayPool<int>.Shared.Rent(cacheSize);
            if (blnImmediatelyRegenerateCache)
                RegenerateCache();
            else
                _intCurrentCacheIndex = cacheSize; // Guarantees that the first call will rebuild the cache
        }

        private void RegenerateCache()
        {
            for (int i = 0; i < _aintCache.Length; ++i)
            {
                _aintCache[i] = _objRandom.Next();
            }
            _intCurrentCacheIndex = -1; // Set index last to make sure that when it's -1, the list has been fully regenerated
        }

        /// <inheritdoc />
        public override int Next()
        {
            int intIndex = Interlocked.Increment(ref _intCurrentCacheIndex);
            while (intIndex >= _aintCache.Length)
            {
                _objLock.SafeWait();
                try
                {
                    intIndex = Interlocked.Increment(ref _intCurrentCacheIndex);
                    // Just in case we request a regenerate while we're already regenerating
                    if (intIndex >= _aintCache.Length)
                    {
                        RegenerateCache();
                        intIndex = Interlocked.Increment(ref _intCurrentCacheIndex);
                    }
                }
                finally
                {
                    _objLock.Release();
                }
            }
            return _aintCache[intIndex];
        }

        /// <inheritdoc />
        public override int Next(int maxValue)
        {
            if (maxValue < 0)
                throw new ArgumentOutOfRangeException(nameof(maxValue));
            if (maxValue <= 1)
                return 0;
            return Next() % maxValue;
        }

        /// <inheritdoc />
        public override int Next(int minValue, int maxValue)
        {
            return minValue + Next(maxValue - minValue);
        }

        public override async Task<int> NextAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intIndex = Interlocked.Increment(ref _intCurrentCacheIndex);
            while (intIndex >= _aintCache.Length)
            {
                await _objLock.WaitAsync(token).ConfigureAwait(false);
                try
                {
                    intIndex = Interlocked.Increment(ref _intCurrentCacheIndex);
                    // Just in case we request a regenerate while we're already regenerating
                    if (intIndex >= _aintCache.Length)
                    {
                        RegenerateCache();
                        intIndex = Interlocked.Increment(ref _intCurrentCacheIndex);
                    }
                }
                finally
                {
                    _objLock.Release();
                }
            }
            return _aintCache[intIndex];
        }

        public override Task<int> NextAsync(int maxValue, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<int>(token);
            if (maxValue < 0)
                return Task.FromException<int>(new ArgumentOutOfRangeException(nameof(maxValue)));
            return maxValue <= 1 ? Task.FromResult(0) : DoInner();

            async Task<int> DoInner()
            {
                return await NextAsync(token).ConfigureAwait(false) % maxValue;
            }
        }

        public override async Task<int> NextAsync(int minValue, int maxValue, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return minValue + await NextAsync(maxValue, token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public override void NextBytes(byte[] buffer)
        {
            int intIterationsNeeded = buffer.Length.DivAwayFromZero(sizeof(int));
            for (int i = 0; i < intIterationsNeeded - 1; ++i)
            {
                BitConverter.GetBytes(Next()).CopyTo(buffer, i);
            }

            if (intIterationsNeeded * sizeof(int) == buffer.Length)
                return;

            unsafe
            {
                fixed (byte* pchrLastBytes = BitConverter.GetBytes(Next()))
                {
                    int intLeadingI = (intIterationsNeeded - 1) * sizeof(int);
                    int intFinalI = buffer.Length - intLeadingI;
                    for (int i = 0; i < intFinalI; ++i)
                    {
                        buffer[intLeadingI + i] = *(pchrLastBytes + i);
                    }
                }
            }
        }

        public override async Task NextBytesAsync(byte[] buffer, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intIterationsNeeded = buffer.Length.DivAwayFromZero(sizeof(int));
            for (int i = 0; i < intIterationsNeeded - 1; ++i)
            {
                BitConverter.GetBytes(await NextAsync(token).ConfigureAwait(false)).CopyTo(buffer, i);
            }

            if (intIterationsNeeded * sizeof(int) == buffer.Length)
                return;

            int intNext = await NextAsync(token).ConfigureAwait(false);
            unsafe
            {
                fixed (byte* pchrLastBytes = BitConverter.GetBytes(intNext))
                {
                    int intLeadingI = (intIterationsNeeded - 1) * sizeof(int);
                    int intFinalI = buffer.Length - intLeadingI;
                    for (int i = 0; i < intFinalI; ++i)
                    {
                        buffer[intLeadingI + i] = *(pchrLastBytes + i);
                    }
                }
            }
        }

        /// <inheritdoc />
        public override double NextDouble()
        {
            long lngReturn = (long)(((ulong)Next() << 32) + (ulong)Next());
            return BitConverter.Int64BitsToDouble(lngReturn);
        }

        public override async Task<double> NextDoubleAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            long lngReturn = (long)(((ulong)await NextAsync(token).ConfigureAwait(false) << 32) +
                                    (ulong)await NextAsync(token).ConfigureAwait(false));
            return BitConverter.Int64BitsToDouble(lngReturn);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ArrayPool<int>.Shared.Return(_aintCache);
            }

            base.Dispose(disposing);
        }

        protected override ValueTask DisposeAsync(bool disposing)
        {
            if (disposing)
            {
                ArrayPool<int>.Shared.Return(_aintCache);
            }

            return base.DisposeAsync(disposing);
        }
    }
}
