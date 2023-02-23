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
using System.Threading;
using System.Threading.Tasks;

namespace Chummer
{
    /// <summary>
    /// Pairs Random with a lock object and overrides all of Random's methods with versions that engage the lock while the internal Random object is in use.
    /// </summary>
    public class ThreadSafeRandom : Random, IDisposable, IAsyncDisposable
    {
        [CLSCompliant(false)]
        protected readonly Random _objRandom;
        [CLSCompliant(false)]
        protected DebuggableSemaphoreSlim _objLock = Utils.SemaphorePool.Get();

        public ThreadSafeRandom()
        {
            _objRandom = new Random();
        }

        public ThreadSafeRandom(int seed)
        {
            _objRandom = new Random(seed);
        }

        public ThreadSafeRandom(Random objRandom)
        {
            _objRandom = objRandom;
        }

        /// <inheritdoc />
        public override int Next()
        {
            int intReturn;
            _objLock.SafeWait();
            try
            {
                intReturn = _objRandom.Next();
            }
            finally
            {
                _objLock.Release();
            }
            return intReturn;
        }

        /// <inheritdoc />
        public override int Next(int minValue, int maxValue)
        {
            int intReturn;
            _objLock.SafeWait();
            try
            {
                intReturn = _objRandom.Next(minValue, maxValue);
            }
            finally
            {
                _objLock.Release();
            }
            return intReturn;
        }

        /// <inheritdoc />
        public override int Next(int maxValue)
        {
            int intReturn;
            _objLock.SafeWait();
            try
            {
                intReturn = _objRandom.Next(maxValue);
            }
            finally
            {
                _objLock.Release();
            }
            return intReturn;
        }

        /// <inheritdoc />
        public override void NextBytes(byte[] buffer)
        {
            _objLock.SafeWait();
            try
            {
                _objRandom.NextBytes(buffer);
            }
            finally
            {
                _objLock.Release();
            }
        }

        /// <inheritdoc />
        public override double NextDouble()
        {
            double dblReturn;
            _objLock.SafeWait();
            try
            {
                dblReturn = _objRandom.NextDouble();
            }
            finally
            {
                _objLock.Release();
            }
            return dblReturn;
        }

        public virtual async Task<int> NextAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intReturn;
            await _objLock.WaitAsync(token).ConfigureAwait(false);
            try
            {
                intReturn = _objRandom.Next();
            }
            finally
            {
                _objLock.Release();
            }
            return intReturn;
        }

        public virtual async Task<int> NextAsync(int minValue, int maxValue, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intReturn;
            await _objLock.WaitAsync(token).ConfigureAwait(false);
            try
            {
                intReturn = _objRandom.Next(minValue, maxValue);
            }
            finally
            {
                _objLock.Release();
            }
            return intReturn;
        }

        public virtual async Task<int> NextAsync(int maxValue, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intReturn;
            await _objLock.WaitAsync(token).ConfigureAwait(false);
            try
            {
                intReturn = _objRandom.Next(maxValue);
            }
            finally
            {
                _objLock.Release();
            }
            return intReturn;
        }

        public virtual async Task NextBytesAsync(byte[] buffer, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await _objLock.WaitAsync(token).ConfigureAwait(false);
            try
            {
                _objRandom.NextBytes(buffer);
            }
            finally
            {
                _objLock.Release();
            }
        }

        public virtual async Task<double> NextDoubleAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            double dblReturn;
            await _objLock.WaitAsync(token).ConfigureAwait(false);
            try
            {
                dblReturn = _objRandom.NextDouble();
            }
            finally
            {
                _objLock.Release();
            }
            return dblReturn;
        }

        /// <inheritdoc />
        protected override double Sample()
        {
            return NextDouble();
        }

        private int _intIsDisposed;

        public bool IsDisposed => _intIsDisposed > 0;

        protected virtual void Dispose(bool disposing)
        {
        }

#pragma warning disable CS1998
        protected virtual async ValueTask DisposeAsync(bool disposing)
#pragma warning restore CS1998
        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _intIsDisposed, 1, 0) > 0)
                return;
            _objLock.SafeWait();
            try
            {
                Dispose(true);
            }
            finally
            {
                _objLock.Release();
            }
            Utils.SemaphorePool.Return(ref _objLock);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await _objLock.WaitAsync().ConfigureAwait(false);
            try
            {
                await DisposeAsync(true).ConfigureAwait(false);
            }
            finally
            {
                _objLock.Release();
            }
            Utils.SemaphorePool.Return(ref _objLock);
            GC.SuppressFinalize(this);
        }
    }
}
