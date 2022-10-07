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
    public sealed class ThreadSafeRandom : Random, IDisposable
    {
        private readonly Random _objRandom;
        private DebuggableSemaphoreSlim _objLock = Utils.SemaphorePool.Get();

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

        public async Task<int> NextAsync(CancellationToken token = default)
        {
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

        public async Task<int> NextAsync(int minValue, int maxValue, CancellationToken token = default)
        {
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

        public async Task<int> NextAsync(int maxValue, CancellationToken token = default)
        {
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

        public async Task NextBytesAsync(byte[] buffer, CancellationToken token = default)
        {
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

        public async Task<double> NextDoubleAsync(CancellationToken token = default)
        {
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

        /// <inheritdoc />
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _intIsDisposed, 1, 0) > 0)
                return;
            Utils.SemaphorePool.Return(ref _objLock);
        }
    }
}
