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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Chummer
{
    /// <summary>
    /// A version of Dictionary that is paired with a ReaderWriterLock.
    /// In theory, this dictionary can be faster in serial contexts than ConcurrentDictionary.
    /// Because ReadWriterLock allows parallel reads and only locks out writes, it's also faster than using a simple setup with the lock keyword.
    /// However, for mass parallel writes, use ConcurrentDictionary instead because locking the entire dictionary when accessing keys is not good for performance.
    /// </summary>
    /// <typeparam name="TKey">Key to use for the dictionary.</typeparam>
    /// <typeparam name="TValue">Values to use for the dictionary.</typeparam>
    public sealed class LockingDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IDisposable, IProducerConsumerCollection<KeyValuePair<TKey, TValue>>, IHasLockingEnumerators<KeyValuePair<TKey, TValue>>, IHasLockingDictionaryEnumerators
    {
        private readonly Dictionary<TKey, TValue> _dicData;
        private readonly ReaderWriterLockSlim
            _rwlThis = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public LockingDictionary()
        {
            _dicData = new Dictionary<TKey, TValue>();
        }

        public LockingDictionary(int capacity)
        {
            _dicData = new Dictionary<TKey, TValue>(capacity);
        }

        public LockingDictionary(IDictionary<TKey, TValue> dictionary)
        {
            _dicData = new Dictionary<TKey, TValue>(dictionary);
        }

        public LockingDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
        {
            _dicData = new Dictionary<TKey, TValue>(dictionary, comparer);
        }

        public LockingDictionary(IEqualityComparer<TKey> comparer)
        {
            _dicData = new Dictionary<TKey, TValue>(comparer);
        }

        private readonly object _objActiveEnumeratorsLock = new object();
        private readonly List<LockingEnumerator<KeyValuePair<TKey, TValue>>> _lstActiveEnumerators = new List<LockingEnumerator<KeyValuePair<TKey, TValue>>>();
        private readonly List<LockingDictionaryEnumerator> _lstActiveDictionaryEnumerators = new List<LockingDictionaryEnumerator>();

        public LockingEnumerator<KeyValuePair<TKey, TValue>> CreateLockingEnumerator()
        {
            lock (_objActiveEnumeratorsLock)
            {
                bool blnDoLock = _lstActiveEnumerators.Count == 0 && _lstActiveDictionaryEnumerators.Count == 0;
                LockingEnumerator<KeyValuePair<TKey, TValue>>  objReturn = new LockingEnumerator<KeyValuePair<TKey, TValue>>(_dicData.GetEnumerator(), this);
                _lstActiveEnumerators.Add(objReturn);
                if (blnDoLock)
                    _rwlThis.EnterReadLock();
                return objReturn;
            }
        }

        public void FreeLockingEnumerator(LockingEnumerator<KeyValuePair<TKey, TValue>> objToFree)
        {
            lock (_objActiveEnumeratorsLock)
            {
                _lstActiveEnumerators.Remove(objToFree);
                if (_rwlThis.IsReadLockHeld && _lstActiveEnumerators.Count == 0 && _lstActiveDictionaryEnumerators.Count == 0)
                    _rwlThis.ExitReadLock();
            }
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return CreateLockingEnumerator();
        }

        public LockingDictionaryEnumerator CreateLockingDictionaryEnumerator()
        {
            lock (_objActiveEnumeratorsLock)
            {
                bool blnDoLock = _lstActiveEnumerators.Count == 0 && _lstActiveDictionaryEnumerators.Count == 0;
                LockingDictionaryEnumerator objReturn = new LockingDictionaryEnumerator(((IDictionary)_dicData).GetEnumerator(), this);
                _lstActiveDictionaryEnumerators.Add(objReturn);
                if (blnDoLock)
                    _rwlThis.EnterReadLock();
                return objReturn;
            }
        }

        public void FreeLockingDictionaryEnumerator(LockingDictionaryEnumerator objToFree)
        {
            lock (_objActiveEnumeratorsLock)
            {
                _lstActiveDictionaryEnumerators.Remove(objToFree);
                if (_rwlThis.IsReadLockHeld && _lstActiveEnumerators.Count == 0 && _lstActiveDictionaryEnumerators.Count == 0)
                    _rwlThis.ExitReadLock();
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return CreateLockingDictionaryEnumerator();
        }

        /// <inheritdoc />
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return CreateLockingDictionaryEnumerator();
        }

        /// <inheritdoc />
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            using (new EnterWriteLock(_rwlThis))
                _dicData.Add(item.Key, item.Value);
        }

        /// <inheritdoc />
        public bool Contains(object key)
        {
            return ContainsKey((TKey)key);
        }

        /// <inheritdoc />
        public void Add(object key, object value)
        {
            Add((TKey)key, (TValue)value);
        }

        /// <inheritdoc cref="IDictionary{TKey, TValue}.Clear" />
        public void Clear()
        {
            using (new EnterWriteLock(_rwlThis))
                _dicData.Clear();
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            using (new EnterReadLock(_rwlThis))
                return _dicData.Contains(item);
        }

        /// <inheritdoc cref="ICollection.CopyTo" />
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            using (new EnterReadLock(_rwlThis))
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
            using (new EnterReadLock(_rwlThis))
            {
                foreach (KeyValuePair<TKey, TValue> kvpItem in _dicData)
                {
                    array.SetValue(kvpItem, index);
                    ++index;
                }
            }
        }

        /// <inheritdoc />
        public KeyValuePair<TKey, TValue>[] ToArray()
        {
            using (new EnterReadLock(_rwlThis))
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
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            // Immediately enter a write lock to prevent attempted reads until we have either removed the item we want to remove or failed to do so
            using (new EnterWriteLock(_rwlThis))
            {
                return _dicData.TryGetValue(item.Key, out TValue objValue) && objValue.Equals(item.Value)
                                                                           && _dicData.Remove(item.Key);
            }
        }

        /// <inheritdoc />
        public bool Remove(TKey key)
        {
            using (new EnterWriteLock(_rwlThis))
                return _dicData.Remove(key);
        }

        /// <inheritdoc />
        public void Remove(object key)
        {
            Remove((TKey)key);
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            // Immediately enter a write lock to prevent attempted reads until we have either removed the item we want to remove or failed to do so
            using (new EnterWriteLock(_rwlThis))
            {
                return _dicData.TryGetValue(key, out value) && _dicData.Remove(key);
            }
        }

        /// <inheritdoc />
        public bool TryTake(out KeyValuePair<TKey, TValue> item)
        {
            bool blnTakeSuccessful = false;
            TKey objKeyToTake = default;
            TValue objValue = default;
            // Immediately enter a write lock to prevent attempted reads until we have either taken the item we want to take or failed to do so
            using (new EnterWriteLock(_rwlThis))
            {
                if (_dicData.Count > 0)
                {
                    // FIFO to be compliant with how the default for BlockingCollection<T> is ConcurrentQueue
                    objKeyToTake = _dicData.Keys.First();
                    if (_dicData.TryGetValue(objKeyToTake, out objValue))
                    {
                        blnTakeSuccessful = _dicData.Remove(objKeyToTake);
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
                using (new EnterReadLock(_rwlThis))
                    return _dicData.Count;
            }
        }

        /// <inheritdoc />
        public object SyncRoot => _rwlThis;

        /// <inheritdoc />
        public bool IsSynchronized => true;

        /// <inheritdoc cref="IDictionary{TKey, TValue}.IsReadOnly" />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public bool IsFixedSize => false;

        /// <inheritdoc cref="IDictionary{TKey, TValue}.ContainsKey" />
        public bool ContainsKey(TKey key)
        {
            using (new EnterReadLock(_rwlThis))
                return _dicData.ContainsKey(key);
        }

        /// <inheritdoc />
        public void Add(TKey key, TValue value)
        {
            using (new EnterWriteLock(_rwlThis))
                _dicData.Add(key, value);
        }

        public bool TryAdd(TKey key, TValue value)
        {
            // Immediately enter a write lock to prevent attempted reads until we have either added the item we want to add or failed to do so
            using (new EnterWriteLock(_rwlThis))
            {
                if (_dicData.ContainsKey(key))
                    return false;
                _dicData.Add(key, value);
            }
            return true;
        }

        /// <inheritdoc />
        public bool TryAdd(KeyValuePair<TKey, TValue> item)
        {
            return TryAdd(item.Key, item.Value);
        }

        /// <summary>
        /// Uses the specified functions to add a key/value pair to the dictionary if the key does not already exist, or to update a key/value pair in the dictionary if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="addValueFactory">The function used to generate a value for an absent key</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an existing key based on the key's existing value</param>
        /// <returns>The new value for the key. This will be either be the result of addValueFactory (if the key was absent) or the result of updateValueFactory (if the key was present).</returns>
        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory,
                                  Func<TKey, TValue, TValue> updateValueFactory)
        {
            TValue objReturn;
            using (new EnterWriteLock(_rwlThis))
            {
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                {
                    objReturn = updateValueFactory(key, objExistingValue);
                    _dicData[key] = objReturn;
                }
                else
                {
                    objReturn = addValueFactory(key);
                    _dicData.Add(key, objReturn);
                }
            }
            return objReturn;
        }

        /// <summary>
        /// Adds a key/value pair to the dictionary if the key does not already exist, or to update a key/value pair in the dictionary if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="addValue">The value to be added for an absent key</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an existing key based on the key's existing value</param>
        /// <returns>The new value for the key. This will be either be addValue (if the key was absent) or the result of updateValueFactory (if the key was present).</returns>
        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            using (new EnterWriteLock(_rwlThis))
            {
                if (_dicData.TryGetValue(key, out TValue objExistingValue))
                {
                    TValue objNewValue = updateValueFactory(key, objExistingValue);
                    _dicData[key] = objNewValue;
                    return objNewValue;
                }

                _dicData.Add(key, addValue);
                return addValue;
            }
        }

        /// <inheritdoc cref="IDictionary{TKey, TValue}.TryGetValue" />
        public bool TryGetValue(TKey key, out TValue value)
        {
            using (new EnterReadLock(_rwlThis))
                return _dicData.TryGetValue(key, out value);
        }

        // ReSharper disable once InheritdocInvalidUsage
        /// <inheritdoc />
        public TValue this[TKey key]
        {
            get
            {
                using (new EnterReadLock(_rwlThis))
                    return _dicData[key];
            }
            set
            {
                using (new EnterUpgradeableReadLock(_rwlThis))
                {
                    if (_dicData.TryGetValue(key, out TValue objValue) && objValue.Equals(value))
                        return;
                    using (new EnterWriteLock(_rwlThis))
                        _dicData[key] = value;
                }
            }
        }

        /// <inheritdoc />
        public object this[object key]
        {
            get => this[(TKey)key];
            set => this[(TKey)key] = (TValue)value;
        }

        /// <inheritdoc />
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
        {
            get
            {
                using (new EnterReadLock(_rwlThis))
                {
                    // This construction makes sure we hold onto the lock until enumeration is done
                    foreach (TKey objKey in _dicData.Keys)
                        yield return objKey;
                }
            }
        }

        /// <inheritdoc />
        ICollection IDictionary.Keys
        {
            get
            {
                using (new EnterReadLock(_rwlThis))
                    return _dicData.Keys;
            }
        }

        /// <inheritdoc />
        public ICollection<TKey> Keys
        {
            get
            {
                using (new EnterReadLock(_rwlThis))
                    return _dicData.Keys;
            }
        }

        /// <inheritdoc />
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
        {
            get
            {
                using (new EnterReadLock(_rwlThis))
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
                using (new EnterReadLock(_rwlThis))
                    return _dicData.Values;
            }
        }

        /// <inheritdoc />
        ICollection IDictionary.Values
        {
            get
            {
                using (new EnterReadLock(_rwlThis))
                    return _dicData.Values;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            IsDisposed = true;
            while (_rwlThis.IsReadLockHeld || _rwlThis.IsUpgradeableReadLockHeld || _rwlThis.IsUpgradeableReadLockHeld)
                Utils.SafeSleep();
            _rwlThis.Dispose();
        }

        public bool IsDisposed { get; private set; }
    }
}
