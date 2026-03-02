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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Chummer
{
    /// <summary>
    /// A version of <see cref="Dictionary{TKey, TValue}"/> that is paired with a <see cref="AsyncFriendlyReaderWriterLock"/>.
    /// In theory, this dictionary can be faster in serial contexts than <see cref="System.Collections.Concurrent.ConcurrentDictionary{TKey, TValue}"/>.
    /// Because <see cref="AsyncFriendlyReaderWriterLock"/> allows parallel reads and only locks out writes, it's also faster than using a simple setup with the lock keyword.
    /// However, for mass parallel writes, use <see cref="System.Collections.Concurrent.ConcurrentDictionary{TKey, TValue}"/> instead because locking the entire dictionary when accessing keys is not good for performance.
    /// </summary>
    /// <typeparam name="TKey">Key to use for the dictionary.</typeparam>
    /// <typeparam name="TValue">Values to use for the dictionary.</typeparam>
    [Serializable]
    public class LockingDictionary<TKey, TValue> : IAsyncDictionary<TKey, TValue>, IAsyncReadOnlyDictionary<TKey, TValue>, IAsyncProducerConsumerCollection<KeyValuePair<TKey, TValue>>, IAsyncEnumerableWithSideEffects<KeyValuePair<TKey, TValue>>, IHasLockObject, ISerializable, IDeserializationCallback
    {
        private readonly Dictionary<TKey, TValue> _dicData;
        public AsyncFriendlyReaderWriterLock LockObject { get; }

        public LockingDictionary(AsyncFriendlyReaderWriterLock objParentLock = null, bool blnLockReadOnlyForParent = false)
        {
            LockObject = new AsyncFriendlyReaderWriterLock(objParentLock, blnLockReadOnlyForParent);
            _dicData = new Dictionary<TKey, TValue>();
        }

        public LockingDictionary(int capacity, AsyncFriendlyReaderWriterLock objParentLock = null, bool blnLockReadOnlyForParent = false)
        {
            LockObject = new AsyncFriendlyReaderWriterLock(objParentLock, blnLockReadOnlyForParent);
            _dicData = new Dictionary<TKey, TValue>(capacity);
        }

        public LockingDictionary(IDictionary<TKey, TValue> dictionary, AsyncFriendlyReaderWriterLock objParentLock = null, bool blnLockReadOnlyForParent = false)
        {
            LockObject = new AsyncFriendlyReaderWriterLock(objParentLock, blnLockReadOnlyForParent);
            _dicData = new Dictionary<TKey, TValue>(dictionary);
        }

        public LockingDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer, AsyncFriendlyReaderWriterLock objParentLock = null, bool blnLockReadOnlyForParent = false)
        {
            LockObject = new AsyncFriendlyReaderWriterLock(objParentLock, blnLockReadOnlyForParent);
            _dicData = new Dictionary<TKey, TValue>(dictionary, comparer);
        }

        public LockingDictionary(IEqualityComparer<TKey> comparer, AsyncFriendlyReaderWriterLock objParentLock = null, bool blnLockReadOnlyForParent = false)
        {
            LockObject = new AsyncFriendlyReaderWriterLock(objParentLock, blnLockReadOnlyForParent);
            _dicData = new Dictionary<TKey, TValue>(comparer);
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            LockingEnumerator<KeyValuePair<TKey, TValue>> objReturn = LockingEnumerator<KeyValuePair<TKey, TValue>>.Get(this);
            objReturn.SetEnumerator(_dicData.GetEnumerator());
            return objReturn;
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            LockingDictionaryEnumerator objReturn = LockingDictionaryEnumerator.Get(this);
            objReturn.SetEnumerator(_dicData.GetEnumerator());
            return objReturn;
        }

        public Task<IEnumerator<KeyValuePair<TKey, TValue>>> GetEnumeratorAsync(CancellationToken token = default)
        {
            // Needs to be like this (using async inner function) to make sure AsyncLocals for parents are set in proper location
            Task<LockingEnumerator<KeyValuePair<TKey, TValue>>> tskReturn = LockingEnumerator<KeyValuePair<TKey, TValue>>.GetAsync(this, token);
            return Inner(tskReturn);
            async Task<IEnumerator<KeyValuePair<TKey, TValue>>> Inner(Task<LockingEnumerator<KeyValuePair<TKey, TValue>>> tskInner)
            {
                LockingEnumerator<KeyValuePair<TKey, TValue>> objResult = await tskInner.ConfigureAwait(false);
                objResult.SetEnumerator(_dicData.GetEnumerator());
                return objResult;
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> EnumerateWithSideEffects()
        {
            LockingEnumerator<KeyValuePair<TKey, TValue>> objReturn = LockingEnumerator<KeyValuePair<TKey, TValue>>.GetWithSideEffects(this);
            objReturn.SetEnumerator(_dicData.GetEnumerator());
            return objReturn;
        }

        public Task<IEnumerator<KeyValuePair<TKey, TValue>>> EnumerateWithSideEffectsAsync(CancellationToken token = default)
        {
            // Needs to be like this (using async inner function) to make sure AsyncLocals for parents are set in proper location
            Task<LockingEnumerator<KeyValuePair<TKey, TValue>>> tskReturn = LockingEnumerator<KeyValuePair<TKey, TValue>>.GetWithSideEffectsAsync(this, token);
            return Inner(tskReturn);
            async Task<IEnumerator<KeyValuePair<TKey, TValue>>> Inner(Task<LockingEnumerator<KeyValuePair<TKey, TValue>>> tskInner)
            {
                LockingEnumerator<KeyValuePair<TKey, TValue>> objResult = await tskInner.ConfigureAwait(false);
                objResult.SetEnumerator(_dicData.GetEnumerator());
                return objResult;
            }
        }

        /// <inheritdoc />
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            using (LockObject.EnterWriteLock())
                _dicData.Add(item.Key, item.Value);
        }

        public async Task AddAsync(KeyValuePair<TKey, TValue> item, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _dicData.Add(item.Key, item.Value);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public Task AddAsync(object key, object value, CancellationToken token = default)
        {
            return AddAsync((TKey)key, (TValue)value, token);
        }

        /// <inheritdoc cref="IDictionary{TKey, TValue}.Clear" />
        public void Clear()
        {
            using (LockObject.EnterWriteLock())
                _dicData.Clear();
        }

        /// <inheritdoc cref="IDictionary{TKey, TValue}.Clear" />
        public async Task ClearAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _dicData.Clear();
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            using (LockObject.EnterReadLock())
                return _dicData.TryGetValue(item.Key, out TValue objValue) && objValue.Equals(item.Value);
        }

        public async Task<bool> ContainsAsync(KeyValuePair<TKey, TValue> item, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _dicData.TryGetValue(item.Key, out TValue objValue) && objValue.Equals(item.Value);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="ICollection.CopyTo" />
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            using (LockObject.EnterReadLock())
            {
                foreach (KeyValuePair<TKey, TValue> kvpItem in _dicData)
                {
                    array[arrayIndex] = kvpItem;
                    ++arrayIndex;
                }
            }
        }

        /// <inheritdoc />
        public void CopyTo(Array array, int index)
        {
            using (LockObject.EnterReadLock())
            {
                foreach (KeyValuePair<TKey, TValue> kvpItem in _dicData)
                {
                    array.SetValue(kvpItem, index);
                    ++index;
                }
            }
        }

        /// <inheritdoc cref="ICollection.CopyTo" />
        public async Task CopyToAsync(KeyValuePair<TKey, TValue>[] array, int index, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                foreach (KeyValuePair<TKey, TValue> kvpItem in _dicData)
                {
                    array[index] = kvpItem;
                    ++index;
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="ICollection.CopyTo" />
        public async Task CopyToAsync(Array array, int index, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                foreach (KeyValuePair<TKey, TValue> kvpItem in _dicData)
                {
                    array.SetValue(kvpItem, index);
                    ++index;
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public KeyValuePair<TKey, TValue>[] ToArray()
        {
            using (LockObject.EnterReadLock())
            {
                KeyValuePair<TKey, TValue>[] akvpReturn = new KeyValuePair<TKey, TValue>[_dicData.Count];
                int i = 0;
                foreach (KeyValuePair<TKey, TValue> kvpLoop in _dicData)
                {
                    akvpReturn[i] = kvpLoop;
                    ++i;
                }
                return akvpReturn;
            }
        }

        /// <inheritdoc />
        public async Task<ValueTuple<bool, KeyValuePair<TKey, TValue>>> TryTakeAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_dicData.Count == 0)
                    return new ValueTuple<bool, KeyValuePair<TKey, TValue>>(false, default);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            bool blnTakeSuccessful = false;
            TKey objKeyToTake = default;
            TValue objValue = default;
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_dicData.Count > 0)
                {
                    // FIFO to be compliant with how the default for BlockingCollection<T> is ConcurrentQueue
                    objKeyToTake = _dicData.Keys.First();
                    if (_dicData.TryGetValue(objKeyToTake, out objValue))
                    {
                        IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                        try
                        {
                            token.ThrowIfCancellationRequested();
                            blnTakeSuccessful = _dicData.Remove(objKeyToTake);
                        }
                        finally
                        {
                            await objLocker2.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            return blnTakeSuccessful
                ? new ValueTuple<bool, KeyValuePair<TKey, TValue>>(
                    true, new KeyValuePair<TKey, TValue>(objKeyToTake, objValue))
                : new ValueTuple<bool, KeyValuePair<TKey, TValue>>(false, default);
        }

        public async Task<KeyValuePair<TKey, TValue>[]> ToArrayAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                KeyValuePair<TKey, TValue>[] akvpReturn = new KeyValuePair<TKey, TValue>[_dicData.Count];
                int i = 0;
                foreach (KeyValuePair<TKey, TValue> kvpLoop in _dicData)
                {
                    akvpReturn[i] = kvpLoop;
                    ++i;
                }
                return akvpReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            using (LockObject.EnterReadLock())
            {
                if (!_dicData.TryGetValue(item.Key, out TValue _))
                    return false;
            }
            using (LockObject.EnterWriteLock())
            {
                if (!_dicData.TryGetValue(item.Key, out TValue objValue))
                    return false;
                if (Equals(objValue, default(TValue)))
                    return Equals(item.Value, default(TValue)) && _dicData.Remove(item.Key);
                return objValue.Equals(item.Value) && _dicData.Remove(item.Key);
            }
        }

        /// <inheritdoc />
        public bool Remove(TKey key)
        {
            using (LockObject.EnterWriteLock())
                return _dicData.Remove(key);
        }

        public async Task<bool> RemoveAsync(KeyValuePair<TKey, TValue> item, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!_dicData.TryGetValue(item.Key, out TValue _))
                    return false;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!_dicData.TryGetValue(item.Key, out TValue objValue))
                    return false;
                if (Equals(objValue, default(TValue)))
                    return Equals(item.Value, default(TValue)) && _dicData.Remove(item.Key);
                return objValue.Equals(item.Value) && _dicData.Remove(item.Key);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task<bool> RemoveAsync(TKey key, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _dicData.Remove(key);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public Task<bool> RemoveAsync(object key, CancellationToken token = default)
        {
            return RemoveAsync((TKey)key, token);
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            using (LockObject.EnterReadLock())
            {
                if (!_dicData.TryGetValue(key, out value))
                    return false;
            }
            using (LockObject.EnterWriteLock())
                return _dicData.TryGetValue(key, out value) && _dicData.Remove(key);
        }

        public async Task<ValueTuple<bool, TValue>> TryRemoveAsync(TKey key, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!_dicData.TryGetValue(key, out TValue value))
                    return new ValueTuple<bool, TValue>(false, value);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                bool blnSuccess = _dicData.TryGetValue(key, out TValue value) && _dicData.Remove(key);
                return new ValueTuple<bool, TValue>(blnSuccess, value);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public bool TryTake(out KeyValuePair<TKey, TValue> item)
        {
            using (LockObject.EnterReadLock())
            {
                if (_dicData.Count == 0)
                {
                    item = default;
                    return false;
                }
            }
            bool blnTakeSuccessful = false;
            TKey objKeyToTake = default;
            TValue objValue = default;
            using (LockObject.EnterUpgradeableReadLock())
            {
                if (_dicData.Count > 0)
                {
                    // FIFO to be compliant with how the default for BlockingCollection<T> is ConcurrentQueue
                    objKeyToTake = _dicData.Keys.First();
                    if (_dicData.TryGetValue(objKeyToTake, out objValue))
                    {
                        using (LockObject.EnterWriteLock())
                        {
                            blnTakeSuccessful = _dicData.Remove(objKeyToTake);
                        }
                    }
                }
            }

            if (blnTakeSuccessful)
            {
                item = new KeyValuePair<TKey, TValue>(objKeyToTake, objValue);
                return true;
            }
            item = default;
            return false;
        }

        /// <inheritdoc cref="IDictionary{TKey, TValue}.Count" />
        public int Count
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _dicData.Count;
            }
        }

        public async Task<int> GetCountAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _dicData.Count;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public object SyncRoot => LockObject;

        /// <inheritdoc />
        public bool IsSynchronized => true;

        /// <inheritdoc cref="IDictionary{TKey, TValue}.IsReadOnly" />
        public bool IsReadOnly => false;

        /// <inheritdoc cref="IDictionary{TKey, TValue}.ContainsKey" />
        public bool ContainsKey(TKey key)
        {
            using (LockObject.EnterReadLock())
                return _dicData.ContainsKey(key);
        }

        /// <inheritdoc cref="IDictionary{TKey, TValue}.ContainsKey" />
        public async Task<bool> ContainsKeyAsync(TKey key, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _dicData.ContainsKey(key);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="Dictionary{TKey, TValue}.ContainsValue" />
        public bool ContainsValue(TValue value)
        {
            using (LockObject.EnterReadLock())
                return _dicData.ContainsValue(value);
        }

        /// <inheritdoc cref="Dictionary{TKey, TValue}.ContainsValue" />
        public async Task<bool> ContainsValueAsync(TValue value, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _dicData.ContainsValue(value);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public void Add(TKey key, TValue value)
        {
            using (LockObject.EnterWriteLock())
                _dicData.Add(key, value);
        }

        public async Task AddAsync(TKey key, TValue value, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _dicData.Add(key, value);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public bool TryAdd(TKey key, TValue value)
        {
            using (LockObject.EnterReadLock())
            {
                if (_dicData.ContainsKey(key))
                    return false;
            }
            using (LockObject.EnterWriteLock())
            {
                if (_dicData.ContainsKey(key))
                    return false;
                _dicData.Add(key, value);
            }
            return true;
        }

        public async Task<bool> TryAddAsync(TKey key, TValue value, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_dicData.ContainsKey(key))
                    return false;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_dicData.ContainsKey(key))
                    return false;
                _dicData.Add(key, value);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            return true;
        }

        /// <inheritdoc />
        public bool TryAdd(KeyValuePair<TKey, TValue> item)
        {
            return TryAdd(item.Key, item.Value);
        }

        public Task<bool> TryAddAsync(KeyValuePair<TKey, TValue> item, CancellationToken token = default)
        {
            return TryAddAsync(item.Key, item.Value, token);
        }

        public bool TryUpdate(TKey key, TValue value)
        {
            using (LockObject.EnterReadLock())
            {
                if (!_dicData.ContainsKey(key))
                    return false;
            }
            using (LockObject.EnterWriteLock())
            {
                if (!_dicData.ContainsKey(key))
                    return false;
                _dicData[key] = value;
            }
            return true;
        }

        public async Task<bool> TryUpdateAsync(TKey key, TValue value, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!_dicData.ContainsKey(key))
                    return false;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!_dicData.ContainsKey(key))
                    return false;
                _dicData[key] = value;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            return true;
        }

        public bool TryUpdate(KeyValuePair<TKey, TValue> item)
        {
            return TryUpdate(item.Key, item.Value);
        }

        public Task<bool> TryUpdateAsync(KeyValuePair<TKey, TValue> item, CancellationToken token = default)
        {
            return TryUpdateAsync(item.Key, item.Value, token);
        }

        /// <summary>
        /// Uses the specified functions to add a key/value pair to the dictionary if the key does not already exist (and return it) or return the original value in dictionary if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be retrieved.</param>
        /// <param name="addValueFactory">The function used to generate a value for an absent key</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The new value for the key. This will be either be the result of addValueFactory (if the key was absent) or the existing value in the dictionary (if the key was present).</returns>
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> addValueFactory, CancellationToken token = default)
        {
            using (LockObject.EnterReadLock(token))
            {
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                    return objExistingValue;
            }
            TValue objReturn = addValueFactory(key);
            using (LockObject.EnterUpgradeableReadLock(token))
            {
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                    return objExistingValue;
                using (LockObject.EnterWriteLock(token))
                    _dicData.Add(key, objReturn);
            }
            return objReturn;
        }

        /// <summary>
        /// Adds a key/value pair to the dictionary if the key does not already exist, or return the original value in dictionary if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be retrieved.</param>
        /// <param name="addValue">The value to be added for an absent key</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The new value for the key. This will be either be addValue (if the key was absent) or the existing value in the dictionary (if the key was present).</returns>
        public TValue GetOrAdd(TKey key, TValue addValue, CancellationToken token = default)
        {
            using (LockObject.EnterReadLock(token))
            {
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                    return objExistingValue;
            }
            using (LockObject.EnterUpgradeableReadLock(token))
            {
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                    return objExistingValue;
                using (LockObject.EnterWriteLock(token))
                    _dicData.Add(key, addValue);
            }
            return addValue;
        }

        /// <summary>
        /// Uses the specified functions to add a key/value pair to the dictionary if the key does not already exist (and return it) or return the original value in dictionary if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be retrieved.</param>
        /// <param name="addValueFactory">The function used to generate a value for an absent key</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The new value for the key. This will be either be the result of addValueFactory (if the key was absent) or the existing value in the dictionary (if the key was present).</returns>
        public async Task<TValue> GetOrAddAsync(TKey key, Func<TKey, TValue> addValueFactory, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                    return objExistingValue;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            TValue objReturn = addValueFactory(key);
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                    return objExistingValue;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _dicData.Add(key, objReturn);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            return objReturn;
        }

        /// <summary>
        /// Adds a key/value pair to the dictionary if the key does not already exist, or return the original value in dictionary if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be retrieved.</param>
        /// <param name="addValue">The value to be added for an absent key</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The new value for the key. This will be either be addValue (if the key was absent) or the existing value in the dictionary (if the key was present).</returns>
        public async Task<TValue> GetOrAddAsync(TKey key, TValue addValue, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                    return objExistingValue;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                    return objExistingValue;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _dicData.Add(key, addValue);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            return addValue;
        }

        /// <summary>
        /// Uses the specified functions to add a key/value pair to the dictionary if the key does not already exist (and return it) or return the original value in dictionary if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be retrieved.</param>
        /// <param name="addValueFactory">The function used to generate a value for an absent key</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The new value for the key. This will be either be the result of addValueFactory (if the key was absent) or the existing value in the dictionary (if the key was present).</returns>
        public async Task<TValue> GetOrAddAsync(TKey key, Func<TKey, Task<TValue>> addValueFactory, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                    return objExistingValue;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            TValue objReturn = await addValueFactory(key).ConfigureAwait(false);
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                    return objExistingValue;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _dicData.Add(key, objReturn);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            return objReturn;
        }

        /// <summary>
        /// Adds a key/value pair to the dictionary if the key does not already exist, or return the original value in dictionary if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be retrieved.</param>
        /// <param name="addValue">The value to be added for an absent key</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The new value for the key. This will be either be addValue (if the key was absent) or the existing value in the dictionary (if the key was present).</returns>
        public async Task<TValue> GetOrAddAsync(TKey key, Task<TValue> addValue, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                    return objExistingValue;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            TValue objReturn;
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                objReturn = await addValue.ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                    return objExistingValue;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _dicData.Add(key, objReturn);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            return objReturn;
        }

        /// <summary>
        /// Uses the specified functions to add a key/value pair to the dictionary if the key does not already exist (and return it) or return the original value in dictionary if the key already exists.
        /// This version requests a write lock before potentially calling the function to generate the value to add. This makes it better than GetOrAdd when that function is expensive, but worse when that function is cheap.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be retrieved.</param>
        /// <param name="addValueFactory">The function used to generate a value for an absent key. Should be an expensive function. If it isn't, use GetOrAdd instead.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The new value for the key. This will be either be the result of addValueFactory (if the key was absent) or the existing value in the dictionary (if the key was present).</returns>
        public TValue GetOrCheapAdd(TKey key, Func<TKey, TValue> addValueFactory, CancellationToken token = default)
        {
            using (LockObject.EnterReadLock(token))
            {
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                    return objExistingValue;
            }

            TValue objReturn;
            using (LockObject.EnterUpgradeableReadLock(token))
            {
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                    return objExistingValue;
                using (LockObject.EnterWriteLock(token))
                {
                    objReturn = addValueFactory(key);
                    _dicData.Add(key, objReturn);
                }
            }
            return objReturn;
        }

        /// <summary>
        /// Uses the specified functions to add a key/value pair to the dictionary if the key does not already exist (and return it) or return the original value in dictionary if the key already exists.
        /// This version requests a write lock before potentially calling the function to generate the value to add. This makes it better than GetOrAdd when that function is expensive, but worse when that function is cheap.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be retrieved.</param>
        /// <param name="addValueFactory">The function used to generate a value for an absent key. Should be an expensive function. If it isn't, use GetOrAdd instead.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The new value for the key. This will be either be the result of addValueFactory (if the key was absent) or the existing value in the dictionary (if the key was present).</returns>
        public async Task<TValue> GetOrCheapAddAsync(TKey key, Func<TKey, TValue> addValueFactory, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                    return objExistingValue;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            TValue objReturn;
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                    return objExistingValue;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    objReturn = addValueFactory(key);
                    token.ThrowIfCancellationRequested();
                    _dicData.Add(key, objReturn);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            return objReturn;
        }

        /// <summary>
        /// Uses the specified functions to add a key/value pair to the dictionary if the key does not already exist (and return it) or return the original value in dictionary if the key already exists.
        /// This version requests a write lock before potentially calling the function to generate the value to add. This makes it better than GetOrAdd when that function is expensive, but worse when that function is cheap.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be retrieved.</param>
        /// <param name="addValueFactory">The function used to generate a value for an absent key. Should be an expensive function. If it isn't, use GetOrAdd instead.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The new value for the key. This will be either be the result of addValueFactory (if the key was absent) or the existing value in the dictionary (if the key was present).</returns>
        public async Task<TValue> GetOrCheapAddAsync(TKey key, Func<TKey, Task<TValue>> addValueFactory, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                    return objExistingValue;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            TValue objReturn;
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                    return objExistingValue;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    objReturn = await addValueFactory(key).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    _dicData.Add(key, objReturn);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            return objReturn;
        }

        /// <summary>
        /// Uses the specified functions to add a key/value pair to the dictionary if the key does not already exist, or to update a key/value pair in the dictionary if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="addValueFactory">The function used to generate a value for an absent key</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an existing key based on the key's existing value</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The new value for the key. This will be either be the result of addValueFactory (if the key was absent) or the result of updateValueFactory (if the key was present).</returns>
        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory,
                                  Func<TKey, TValue, TValue> updateValueFactory, CancellationToken token = default)
        {
            TValue objReturn;
            using (LockObject.EnterUpgradeableReadLock(token))
            {
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                {
                    objReturn = updateValueFactory(key, objExistingValue);
                    using (LockObject.EnterWriteLock(token))
                        _dicData[key] = objReturn;
                    return objReturn;
                }
            }
            objReturn = addValueFactory(key);
            using (LockObject.EnterWriteLock(token))
            {
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                {
                    objReturn = updateValueFactory(key, objExistingValue);
                    _dicData[key] = objReturn;
                }
                else
                    _dicData.Add(key, objReturn);
            }
            return objReturn;
        }

        /// <summary>
        /// Adds a key/value pair to the dictionary if the key does not already exist, or to update a key/value pair in the dictionary if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="addValue">The value to be added for an absent key</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an existing key based on the key's existing value</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The new value for the key. This will be either be addValue (if the key was absent) or the result of updateValueFactory (if the key was present).</returns>
        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory, CancellationToken token = default)
        {
            using (LockObject.EnterUpgradeableReadLock(token))
            {
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                {
                    TValue objNewValue = updateValueFactory(key, objExistingValue);
                    using (LockObject.EnterWriteLock(token))
                        _dicData[key] = objNewValue;
                    return objNewValue;
                }
            }
            using (LockObject.EnterWriteLock(token))
            {
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                {
                    TValue objNewValue = updateValueFactory(key, objExistingValue);
                    _dicData[key] = objNewValue;
                    return objNewValue;
                }
                _dicData.Add(key, addValue);
            }
            return addValue;
        }

        /// <summary>
        /// Uses the specified functions to add a key/value pair to the dictionary if the key does not already exist, or to update a key/value pair in the dictionary if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="addValueFactory">The function used to generate a value for an absent key</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an existing key based on the key's existing value</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The new value for the key. This will be either be the result of addValueFactory (if the key was absent) or the result of updateValueFactory (if the key was present).</returns>
        public async Task<TValue> AddOrUpdateAsync(TKey key, Func<TKey, TValue> addValueFactory,
                                                        Func<TKey, TValue, TValue> updateValueFactory, CancellationToken token = default)
        {
            TValue objReturn;
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                {
                    objReturn = updateValueFactory(key, objExistingValue);
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        _dicData[key] = objReturn;
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }

                    return objReturn;
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            objReturn = addValueFactory(key);
            objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                {
                    objReturn = updateValueFactory(key, objExistingValue);
                    token.ThrowIfCancellationRequested();
                    _dicData[key] = objReturn;
                    return objReturn;
                }
                _dicData.Add(key, objReturn);
                return objReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Adds a key/value pair to the dictionary if the key does not already exist, or to update a key/value pair in the dictionary if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="addValue">The value to be added for an absent key</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an existing key based on the key's existing value</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The new value for the key. This will be either be addValue (if the key was absent) or the result of updateValueFactory (if the key was present).</returns>
        public async Task<TValue> AddOrUpdateAsync(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                {
                    TValue objNewValue = updateValueFactory(key, objExistingValue);
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        _dicData[key] = objNewValue;
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }

                    return objNewValue;
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                {
                    TValue objNewValue = updateValueFactory(key, objExistingValue);
                    token.ThrowIfCancellationRequested();
                    _dicData[key] = objNewValue;
                    return objNewValue;
                }
                _dicData.Add(key, addValue);
                return addValue;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Uses the specified functions to add a key/value pair to the dictionary if the key does not already exist, or to update a key/value pair in the dictionary if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="addValueFactory">The function used to generate a value for an absent key</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an existing key based on the key's existing value</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The new value for the key. This will be either be the result of addValueFactory (if the key was absent) or the result of updateValueFactory (if the key was present).</returns>
        public async Task<TValue> AddOrUpdateAsync(TKey key, Func<TKey, Task<TValue>> addValueFactory,
                                                        Func<TKey, TValue, TValue> updateValueFactory, CancellationToken token = default)
        {
            TValue objReturn;
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                {
                    objReturn = updateValueFactory(key, objExistingValue);
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        _dicData[key] = objReturn;
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }

                    return objReturn;
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            objReturn = await addValueFactory(key).ConfigureAwait(false);
            objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                {
                    objReturn = updateValueFactory(key, objExistingValue);
                    token.ThrowIfCancellationRequested();
                    _dicData[key] = objReturn;
                    return objReturn;
                }
                _dicData.Add(key, objReturn);
                return objReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Adds a key/value pair to the dictionary if the key does not already exist, or to update a key/value pair in the dictionary if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="addValue">The value to be added for an absent key</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an existing key based on the key's existing value</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The new value for the key. This will be either be addValue (if the key was absent) or the result of updateValueFactory (if the key was present).</returns>
        public async Task<TValue> AddOrUpdateAsync(TKey key, Task<TValue> addValue, Func<TKey, TValue, TValue> updateValueFactory, CancellationToken token = default)
        {
            TValue objReturn;
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                {
                    objReturn = updateValueFactory(key, objExistingValue);
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        _dicData[key] = objReturn;
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }

                    return objReturn;
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                objReturn = await addValue.ConfigureAwait(false);
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                {
                    objReturn = updateValueFactory(key, objExistingValue);
                    token.ThrowIfCancellationRequested();
                    _dicData[key] = objReturn;
                    return objReturn;
                }
                _dicData.Add(key, objReturn);
                return objReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Uses the specified functions to add a key/value pair to the dictionary if the key does not already exist, or to update a key/value pair in the dictionary if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="addValueFactory">The function used to generate a value for an absent key</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an existing key based on the key's existing value</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The new value for the key. This will be either be the result of addValueFactory (if the key was absent) or the result of updateValueFactory (if the key was present).</returns>
        public async Task<TValue> AddOrUpdateAsync(TKey key, Func<TKey, TValue> addValueFactory,
                                                        Func<TKey, TValue, Task<TValue>> updateValueFactory, CancellationToken token = default)
        {
            TValue objReturn;
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                {
                    objReturn = await updateValueFactory(key, objExistingValue).ConfigureAwait(false);
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        _dicData[key] = objReturn;
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }

                    return objReturn;
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            objReturn = addValueFactory(key);
            objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                {
                    objReturn = await updateValueFactory(key, objExistingValue).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    _dicData[key] = objReturn;
                    return objReturn;
                }
                _dicData.Add(key, objReturn);
                return objReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Adds a key/value pair to the dictionary if the key does not already exist, or to update a key/value pair in the dictionary if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="addValue">The value to be added for an absent key</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an existing key based on the key's existing value</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The new value for the key. This will be either be addValue (if the key was absent) or the result of updateValueFactory (if the key was present).</returns>
        public async Task<TValue> AddOrUpdateAsync(TKey key, TValue addValue, Func<TKey, TValue, Task<TValue>> updateValueFactory, CancellationToken token = default)
        {
            TValue objReturn;
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                {
                    objReturn = await updateValueFactory(key, objExistingValue).ConfigureAwait(false);
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        _dicData[key] = objReturn;
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }

                    return objReturn;
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                {
                    objReturn = await updateValueFactory(key, objExistingValue).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    _dicData[key] = objReturn;
                    return objReturn;
                }
                _dicData.Add(key, addValue);
                return addValue;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Uses the specified functions to add a key/value pair to the dictionary if the key does not already exist, or to update a key/value pair in the dictionary if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="addValueFactory">The function used to generate a value for an absent key</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an existing key based on the key's existing value</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The new value for the key. This will be either be the result of addValueFactory (if the key was absent) or the result of updateValueFactory (if the key was present).</returns>
        public async Task<TValue> AddOrUpdateAsync(TKey key, Func<TKey, Task<TValue>> addValueFactory,
                                                        Func<TKey, TValue, Task<TValue>> updateValueFactory, CancellationToken token = default)
        {
            TValue objReturn;
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                {
                    objReturn = await updateValueFactory(key, objExistingValue).ConfigureAwait(false);
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        _dicData[key] = objReturn;
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }

                    return objReturn;
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            objReturn = await addValueFactory(key).ConfigureAwait(false);
            objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                {
                    objReturn = await updateValueFactory(key, objExistingValue).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    _dicData[key] = objReturn;
                    return objReturn;
                }
                _dicData.Add(key, objReturn);
                return objReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Adds a key/value pair to the dictionary if the key does not already exist, or to update a key/value pair in the dictionary if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="addValue">The value to be added for an absent key</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an existing key based on the key's existing value</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>The new value for the key. This will be either be addValue (if the key was absent) or the result of updateValueFactory (if the key was present).</returns>
        public async Task<TValue> AddOrUpdateAsync(TKey key, Task<TValue> addValue, Func<TKey, TValue, Task<TValue>> updateValueFactory, CancellationToken token = default)
        {
            TValue objReturn;
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                {
                    objReturn = await updateValueFactory(key, objExistingValue).ConfigureAwait(false);
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        _dicData[key] = objReturn;
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }

                    return objReturn;
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                objReturn = await addValue.ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                {
                    objReturn = await updateValueFactory(key, objExistingValue).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    _dicData[key] = objReturn;
                    return objReturn;
                }
                _dicData.Add(key, objReturn);
                return objReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc cref="IDictionary{TKey, TValue}.TryGetValue" />
        public bool TryGetValue(TKey key, out TValue value) => TryGetValue(key, out value, default);

        /// <inheritdoc cref="IDictionary{TKey, TValue}.TryGetValue" />
        public bool TryGetValue(TKey key, out TValue value, CancellationToken token)
        {
            using (LockObject.EnterReadLock(token))
                return _dicData.TryGetValue(key, out value);
        }

        /// <inheritdoc cref="IDictionary{TKey, TValue}.TryGetValue" />
        public async Task<ValueTuple<bool, TValue>> TryGetValueAsync(TKey key, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                bool blnSuccess = _dicData.TryGetValue(key, out TValue value);
                return new ValueTuple<bool, TValue>(blnSuccess, value);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        // ReSharper disable once InheritdocInvalidUsage
        /// <inheritdoc />
        public TValue this[TKey key]
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _dicData[key];
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_dicData.TryGetValue(key, out TValue objValue))
                    {
                        if (Equals(objValue, default(TKey)))
                        {
                            if (Equals(value, default(TKey)))
                                return;
                        }
                        else if (objValue.Equals(value))
                            return;
                    }
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_dicData.TryGetValue(key, out TValue objValue))
                    {
                        if (Equals(objValue, default(TKey)))
                        {
                            if (Equals(value, default(TKey)))
                                return;
                        }
                        else if (objValue.Equals(value))
                            return;
                    }
                    using (LockObject.EnterWriteLock())
                        _dicData[key] = value;
                }
            }
        }

        public async Task<TValue> GetValueAtAsync(TKey key, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _dicData[key];
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task SetValueAtAsync(TKey key, TValue value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_dicData.TryGetValue(key, out TValue objValue))
                {
                    if (Equals(objValue, default(TKey)))
                    {
                        if (Equals(value, default(TKey)))
                            return;
                    }
                    else if (objValue.Equals(value))
                        return;
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_dicData.TryGetValue(key, out TValue objValue))
                {
                    if (Equals(objValue, default(TKey)))
                    {
                        if (Equals(value, default(TKey)))
                            return;
                    }
                    else if (objValue.Equals(value))
                        return;
                }

                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _dicData[key] = value;
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    // This construction makes sure we hold onto the lock until enumeration is done
                    foreach (TKey objKey in _dicData.Keys)
                        yield return objKey;
                }
            }
        }

        /// <inheritdoc />
        public ICollection<TKey> Keys
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _dicData.Keys;
            }
        }

        public async Task<ICollection<TKey>> GetKeysAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _dicData.Keys;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task<IReadOnlyCollection<TKey>> GetReadOnlyKeysAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _dicData.Keys;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    // This construction makes sure we hold onto the lock until enumeration is done
                    foreach (TValue objValue in _dicData.Values)
                        yield return objValue;
                }
            }
        }

        /// <inheritdoc />
        public ICollection<TValue> Values
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _dicData.Values;
            }
        }

        public async Task<ICollection<TValue>> GetValuesAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _dicData.Values;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task<IReadOnlyCollection<TValue>> GetReadOnlyValuesAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _dicData.Values;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private int _intIsDisposed;

        public bool IsDisposed => _intIsDisposed > 0;

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Interlocked.CompareExchange(ref _intIsDisposed, 1, 0) > 0)
                    return;
                LockObject.Dispose();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual async ValueTask DisposeAsync(bool disposing)
        {
            if (disposing)
            {
                if (Interlocked.CompareExchange(ref _intIsDisposed, 1, 0) > 0)
                    return;
                await LockObject.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await DisposeAsync(true).ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            using (LockObject.EnterReadLock())
                _dicData.GetObjectData(info, context);
        }

        /// <inheritdoc />
        public void OnDeserialization(object sender)
        {
            using (LockObject.EnterReadLock())
                _dicData.OnDeserialization(sender);
        }
    }
}
