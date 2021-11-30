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
    public sealed class LockingDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IDisposable
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

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            using (new EnterReadLock(_rwlThis))
                return _dicData.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            using (new EnterReadLock(_rwlThis))
                return GetEnumerator();
        }

        /// <inheritdoc />
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            using (new EnterWriteLock(_rwlThis))
                _dicData.Add(item.Key, item.Value);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            using (new EnterUpgradeableReadLock(_rwlThis))
            {
                if (!_dicData.ContainsKey(item.Key) || !_dicData[item.Key].Equals(item.Value))
                    return false;
                using (new EnterWriteLock(_rwlThis))
                    return _dicData.Remove(item.Key);
            }
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            using (new EnterUpgradeableReadLock(_rwlThis))
            {
                if (!_dicData.TryGetValue(key, out value))
                    return false;
                using (new EnterWriteLock(_rwlThis))
                    return _dicData.Remove(key);
            }
        }

        /// <inheritdoc cref="IDictionary" />
        public int Count
        {
            get
            {
                using (new EnterReadLock(_rwlThis))
                    return _dicData.Count;
            }
        }

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc cref="IDictionary" />
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
            using (new EnterWriteLock(_rwlThis))
            {
                if (_dicData.ContainsKey(key))
                    return false;
                _dicData.Add(key, value);
            }
            return true;
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
                if (_dicData.ContainsKey(key))
                {
                    objReturn = updateValueFactory(key, _dicData[key]);
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
                if (_dicData.ContainsKey(key))
                {
                    TValue objNewValue = updateValueFactory(key, _dicData[key]);
                    _dicData[key] = objNewValue;
                    return objNewValue;
                }

                _dicData.Add(key, addValue);
                return addValue;
            }
        }

        /// <inheritdoc />
        public bool Remove(TKey key)
        {
            using (new EnterWriteLock(_rwlThis))
                return _dicData.Remove(key);
        }

        /// <inheritdoc cref="IDictionary" />
        public bool TryGetValue(TKey key, out TValue value)
        {
            using (new EnterReadLock(_rwlThis))
                return _dicData.TryGetValue(key, out value);
        }

        /// <inheritdoc cref="IDictionary" />
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
                    if (_dicData[key].Equals(value))
                        return;
                    using (new EnterWriteLock(_rwlThis))
                        _dicData[key] = value;
                }
            }
        }

        /// <inheritdoc />
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
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
                    return _dicData.Values;
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
        public ICollection<TValue> Values
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
            _rwlThis.Dispose();
        }
    }
}
