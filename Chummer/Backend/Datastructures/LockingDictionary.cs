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
    /// However, for mass parallelism, use ConcurrentDictionary instead.
    /// </summary>
    /// <typeparam name="TKey">Key to use for the dictionary.</typeparam>
    /// <typeparam name="TValue">Values to use for the dictionary.</typeparam>
    public class LockingDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDisposable
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public bool Remove(TKey key)
        {
            using (new EnterWriteLock(_rwlThis))
                return _dicData.Remove(key);
        }

        /// <inheritdoc />
        public bool TryGetValue(TKey key, out TValue value)
        {
            using (new EnterReadLock(_rwlThis))
                return _dicData.TryGetValue(key, out value);
        }

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
                    if (_dicData[key].Equals(value))
                        return;
                    using (new EnterWriteLock(_rwlThis))
                        _dicData[key] = value;
                }
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
