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
using System.Threading;
using System.Threading.Tasks;

namespace Chummer
{
    public class ThreadSafeObservableCollectionWithMaxSize<T> : ThreadSafeObservableCollection<T>
    {
        private readonly int _intMaxSize;

        public ThreadSafeObservableCollectionWithMaxSize(int intMaxSize)
        {
            _intMaxSize = intMaxSize;
        }

        public ThreadSafeObservableCollectionWithMaxSize(List<T> list, int intMaxSize) : base(list)
        {
            _intMaxSize = intMaxSize;
            for (int intCount = Count; intCount >= _intMaxSize; --intCount)
            {
                RemoveAt(intCount - 1);
            }
        }

        public ThreadSafeObservableCollectionWithMaxSize(IEnumerable<T> collection, int intMaxSize) : base(collection)
        {
            _intMaxSize = intMaxSize;
            for (int intCount = Count; intCount >= _intMaxSize; --intCount)
            {
                RemoveAt(intCount - 1);
            }
        }

        /// <inheritdoc cref="List{T}.Insert" />
        public override void Insert(int index, T item)
        {
            using (LockObject.EnterWriteLock())
            {
                if (index >= _intMaxSize)
                    return;
                for (int intCount = Count; intCount >= _intMaxSize; --intCount)
                {
                    RemoveAt(intCount - 1);
                }
                base.Insert(index, item);
            }
        }

        /// <inheritdoc cref="List{T}.Insert" />
        public override async ValueTask InsertAsync(int index, T item, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (index >= _intMaxSize)
                    return;
                for (int intCount = await GetCountAsync(token).ConfigureAwait(false); intCount >= _intMaxSize; --intCount)
                {
                    await RemoveAtAsync(intCount - 1, token).ConfigureAwait(false);
                }
                await base.InsertAsync(index, item, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public override int Add(object value)
        {
            using (LockObject.EnterReadLock())
            {
                if (Count >= _intMaxSize)
                    return -1;
                return base.Add(value);
            }
        }

        /// <inheritdoc />
        public override void Add(T item)
        {
            using (LockObject.EnterReadLock())
            {
                if (Count >= _intMaxSize)
                    return;
                base.Add(item);
            }
        }

        public override async ValueTask AddAsync(T item, CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                if (await GetCountAsync(token).ConfigureAwait(false) >= _intMaxSize)
                    return;
                await base.AddAsync(item, token).ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public override bool TryAdd(T item)
        {
            using (LockObject.EnterReadLock())
            {
                if (Count >= _intMaxSize)
                    return false;
                base.Add(item);
                return true;
            }
        }

        /// <inheritdoc />
        public override async ValueTask<bool> TryAddAsync(T item, CancellationToken token = default)
        {
            using (await LockObject.EnterReadLockAsync(token).ConfigureAwait(false))
            {
                if (await GetCountAsync(token).ConfigureAwait(false) >= _intMaxSize)
                    return false;
                await base.AddAsync(item, token).ConfigureAwait(false);
                return true;
            }
        }
    }
}
