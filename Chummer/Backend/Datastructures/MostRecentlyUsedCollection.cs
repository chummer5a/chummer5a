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
    public class MostRecentlyUsedCollection<T> : ThreadSafeObservableCollectionWithMaxSize<T>
    {
        public MostRecentlyUsedCollection(int intMaxSize) : base(intMaxSize)
        {
        }

        public MostRecentlyUsedCollection(List<T> list, int intMaxSize) : base(list, intMaxSize)
        {
        }

        public MostRecentlyUsedCollection(IEnumerable<T> collection, int intMaxSize) : base(collection, intMaxSize)
        {
        }

        /// <inheritdoc cref="List{T}.Insert" />
        public override void Insert(int index, T item)
        {
            using (LockObject.EnterWriteLock())
            {
                int intExistingIndex = IndexOf(item);
                if (intExistingIndex == -1)
                    base.Insert(index, item);
                else
                    Move(intExistingIndex, Math.Min(index, Count - 1));
            }
        }

        /// <inheritdoc cref="List{T}.Insert" />
        public override async ValueTask InsertAsync(int index, T item, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token);
            try
            {
                int intExistingIndex = await IndexOfAsync(item, token);
                if (intExistingIndex == -1)
                    await base.InsertAsync(index, item);
                else
                    await MoveAsync(intExistingIndex, Math.Min(index, await GetCountAsync(token) - 1), token);
            }
            finally
            {
                await objLocker.DisposeAsync();
            }
        }

        public override int Add(object value)
        {
            using (LockObject.EnterWriteLock())
            {
                int intExistingIndex = IndexOf(value);
                if (intExistingIndex == -1)
                    return base.Add(value);
                int intNewIndex = Count - 1;
                Move(intExistingIndex, intNewIndex);
                return intNewIndex;
            }
        }

        /// <inheritdoc />
        public override void Add(T item)
        {
            using (LockObject.EnterWriteLock())
            {
                int intExistingIndex = IndexOf(item);
                if (intExistingIndex == -1)
                    base.Add(item);
                else
                    Move(intExistingIndex, Count - 1);
            }
        }

        public override async ValueTask AddAsync(T item, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token);
            try
            {
                int intExistingIndex = await IndexOfAsync(item, token);
                if (intExistingIndex == -1)
                    await base.AddAsync(item, token);
                else
                    await MoveAsync(intExistingIndex, await GetCountAsync(token) - 1, token);
            }
            finally
            {
                await objLocker.DisposeAsync();
            }
        }

        /// <inheritdoc />
        public override bool TryAdd(T item)
        {
            using (LockObject.EnterWriteLock())
            {
                int intExistingIndex = IndexOf(item);
                if (intExistingIndex == -1)
                    return base.TryAdd(item);
                Move(intExistingIndex, Count - 1);
                return true;
            }
        }
    }
}
