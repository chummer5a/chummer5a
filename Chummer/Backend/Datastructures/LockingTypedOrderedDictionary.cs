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
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Chummer
{
    /// <summary>
    /// Like OrderedDictionary, but with key and value types explicitly defined, therefore allowing it to be used in extension/interface methods for dictionaries and lists
    /// </summary>
    /// <typeparam name="TKey">Type used for unique keys in the internal dictionary</typeparam>
    /// <typeparam name="TValue">Type used for values in the internal dictionary</typeparam>
    public sealed class LockingTypedOrderedDictionary<TKey, TValue> :
        IDictionary<TKey, TValue>,
        IList<KeyValuePair<TKey, TValue>>,
        IDictionary,
        IReadOnlyList<KeyValuePair<TKey, TValue>>,
        ISerializable,
        IDeserializationCallback,
        IHasLockObject,
        IAsyncReadOnlyDictionary<TKey, TValue>,
        IProducerConsumerCollection<KeyValuePair<TKey, TValue>>
    {
        private readonly Dictionary<TKey, TValue> _dicUnorderedData;
        private readonly List<TKey> _lstIndexes;

        public LockingTypedOrderedDictionary()
        {
            _dicUnorderedData = new Dictionary<TKey, TValue>();
            _lstIndexes = new List<TKey>();
        }

        public LockingTypedOrderedDictionary(IDictionary<TKey, TValue> dictionary)
        {
            _dicUnorderedData = new Dictionary<TKey, TValue>(dictionary);
            _lstIndexes = new List<TKey>(dictionary.Keys);
        }

        public LockingTypedOrderedDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
        {
            _dicUnorderedData = new Dictionary<TKey, TValue>(dictionary, comparer);
            _lstIndexes = new List<TKey>(dictionary.Keys);
        }

        public LockingTypedOrderedDictionary(IEqualityComparer<TKey> comparer)
        {
            _dicUnorderedData = new Dictionary<TKey, TValue>(comparer);
            _lstIndexes = new List<TKey>();
        }

        public LockingTypedOrderedDictionary(int capacity)
        {
            _dicUnorderedData = new Dictionary<TKey, TValue>(capacity);
            _lstIndexes = new List<TKey>(capacity);
        }

        public LockingTypedOrderedDictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            _dicUnorderedData = new Dictionary<TKey, TValue>(capacity, comparer);
            _lstIndexes = new List<TKey>(capacity);
        }

        /// <inheritdoc cref="Dictionary{TKey, TValue}.Clear" />
        public void Clear()
        {
            using (LockObject.EnterWriteLock())
            {
                _dicUnorderedData.Clear();
                _lstIndexes.Clear();
            }
        }

        /// <inheritdoc cref="IEnumerable.GetEnumerator()" />
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            LockingEnumerator<KeyValuePair<TKey, TValue>> objReturn = LockingEnumerator<KeyValuePair<TKey, TValue>>.Get(this);
            objReturn.SetEnumerator(new LockingTypedOrderedDictionaryEnumerator(this));
            return objReturn;
        }

        public async ValueTask<IEnumerator<KeyValuePair<TKey, TValue>>> GetEnumeratorAsync()
        {
            LockingEnumerator<KeyValuePair<TKey, TValue>> objReturn = await LockingEnumerator<KeyValuePair<TKey, TValue>>.GetAsync(this);
            objReturn.SetEnumerator(new LockingTypedOrderedDictionaryEnumerator(this));
            return objReturn;
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            LockingDictionaryEnumerator objReturn = LockingDictionaryEnumerator.Get(this);
            objReturn.SetEnumerator(new LockingTypedOrderedDictionaryEnumerator(this));
            return objReturn;
        }

        /// <inheritdoc />
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            LockingDictionaryEnumerator objReturn = LockingDictionaryEnumerator.Get(this);
            objReturn.SetEnumerator(new LockingTypedOrderedDictionaryEnumerator(this));
            return objReturn;
        }

        /// <inheritdoc />
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            for (int i = 0; i < Count - 1; ++i)
                yield return this[i];
        }

        /// <inheritdoc />
        public bool Contains(object key)
        {
            switch (key)
            {
                case TKey objKey:
                    return ContainsKey(objKey);

                case KeyValuePair<TKey, TValue> objKeyValuePair:
                    return Contains(objKeyValuePair);

                case Tuple<TKey, TValue> objTuple:
                    return Contains(objTuple);

                default:
                    return false;
            }
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            using (EnterReadLock.Enter(LockObject))
                return _dicUnorderedData.TryGetValue(item.Key, out TValue objValue) && item.Value.Equals(objValue);
        }

        public bool Contains(Tuple<TKey, TValue> item)
        {
            (TKey objKey, TValue objValue) = item;
            using (EnterReadLock.Enter(LockObject))
                return _dicUnorderedData.TryGetValue(objKey, out TValue objExistingValue) && objValue.Equals(objExistingValue);
        }

        /// <inheritdoc cref="Dictionary{TKey, TValue}.ContainsKey" />
        public bool ContainsKey(TKey key)
        {
            using (EnterReadLock.Enter(LockObject))
                return _dicUnorderedData.ContainsKey(key);
        }

        /// <inheritdoc cref="Dictionary{TKey, TValue}.ContainsValue" />
        public bool ContainsValue(TValue value)
        {
            using (EnterReadLock.Enter(LockObject))
                return _dicUnorderedData.ContainsValue(value);
        }

        /// <inheritdoc />
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        /// <inheritdoc cref="Dictionary{TKey, TValue}.Add" />
        public void Add(Tuple<TKey, TValue> item)
        {
            (TKey objKey, TValue objValue) = item;
            Add(objKey, objValue);
        }

        /// <inheritdoc />
        public void Add(object key, object value)
        {
            if (!(key is TKey objKey))
                throw new ArgumentException(nameof(objKey));
            if (!(value is TValue objValue))
                throw new ArgumentException(nameof(objValue));
            Add(objKey, objValue);
        }

        /// <inheritdoc />
        public void Add(TKey key, TValue value)
        {
            using (LockObject.EnterWriteLock())
            {
                _dicUnorderedData.Add(key, value);
                _lstIndexes.Add(key);
            }
        }
        
        public Task AddAsync(KeyValuePair<TKey, TValue> item)
        {
            return AddAsync(item.Key, item.Value);
        }

        /// <inheritdoc cref="Dictionary{TKey, TValue}.Add" />
        public Task AddAsync(Tuple<TKey, TValue> item)
        {
            (TKey objKey, TValue objValue) = item;
            return AddAsync(objKey, objValue);
        }
        
        public Task AddAsync(object key, object value)
        {
            if (!(key is TKey objKey))
                throw new ArgumentException(nameof(objKey));
            if (!(value is TValue objValue))
                throw new ArgumentException(nameof(objValue));
            return AddAsync(objKey, objValue);
        }
        
        public async Task AddAsync(TKey key, TValue value)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync();
            try
            {
                _dicUnorderedData.Add(key, value);
                _lstIndexes.Add(key);
            }
            finally
            {
                await objLocker.DisposeAsync();
            }
        }

        /// <inheritdoc cref="ICollection{T}.CopyTo" />
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                if (arrayIndex + _dicUnorderedData.Count >= array.Length)
                    throw new ArgumentOutOfRangeException(nameof(arrayIndex));
                for (int i = 0; i < _dicUnorderedData.Count; ++i)
                    array[i + arrayIndex] = this[i];
            }
        }

        public void CopyTo(Tuple<TKey, TValue>[] array, int arrayIndex)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                if (arrayIndex + _dicUnorderedData.Count >= array.Length)
                    throw new ArgumentOutOfRangeException(nameof(arrayIndex));
                for (int i = 0; i < _dicUnorderedData.Count; ++i)
                {
                    array[i + arrayIndex] =
                        new Tuple<TKey, TValue>(_lstIndexes[i], _dicUnorderedData[_lstIndexes[i]]);
                }
            }
        }

        /// <inheritdoc />
        public void CopyTo(Array array, int index)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                if (index + _dicUnorderedData.Count >= array.Length)
                    throw new ArgumentOutOfRangeException(nameof(index));
                for (int i = 0; i < _dicUnorderedData.Count; ++i)
                    array.SetValue(this[i], i + index);
            }
        }

        public bool TryAdd(TKey key, TValue value)
        {
            // Immediately enter a write lock to prevent attempted reads until we have either added the item we want to add or failed to do so
            using (LockObject.EnterWriteLock())
            {
                if (_dicUnorderedData.ContainsKey(key))
                    return false;
                _dicUnorderedData.Add(key, value);
                _lstIndexes.Add(key);
            }
            return true;
        }

        public async ValueTask<bool> TryAddAsync(TKey key, TValue value)
        {
            // Immediately enter a write lock to prevent attempted reads until we have either added the item we want to add or failed to do so
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                if (_dicUnorderedData.ContainsKey(key))
                    return false;
                _dicUnorderedData.Add(key, value);
                _lstIndexes.Add(key);
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

        public ValueTask<bool> TryAddAsync(KeyValuePair<TKey, TValue> item)
        {
            return TryAddAsync(item.Key, item.Value);
        }

        /// <inheritdoc />
        public bool TryTake(out KeyValuePair<TKey, TValue> item)
        {
            bool blnTakeSuccessful = false;
            TKey objKeyToTake = default;
            TValue objValue = default;
            // Immediately enter a write lock to prevent attempted reads until we have either taken the item we want to take or failed to do so
            using (LockObject.EnterWriteLock())
            {
                if (_lstIndexes.Count > 0)
                {
                    // FIFO to be compliant with how the default for BlockingCollection<T> is ConcurrentQueue
                    objKeyToTake = _lstIndexes[0];
                    if (_dicUnorderedData.TryGetValue(objKeyToTake, out objValue))
                    {
                        blnTakeSuccessful = _dicUnorderedData.Remove(objKeyToTake);
                        if (blnTakeSuccessful)
                            _lstIndexes.RemoveAt(0);
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

        /// <inheritdoc />
        public KeyValuePair<TKey, TValue>[] ToArray()
        {
            using (EnterReadLock.Enter(LockObject))
            {
                KeyValuePair<TKey, TValue>[] akvpReturn = new KeyValuePair<TKey, TValue>[_dicUnorderedData.Count];
                for (int i = 0; i < _dicUnorderedData.Count; ++i)
                {
                    TKey objLoopKey = _lstIndexes[i];
                    akvpReturn[i] = new KeyValuePair<TKey, TValue>(objLoopKey, _dicUnorderedData[objLoopKey]);
                }
                return akvpReturn;
            }
        }

        /// <inheritdoc cref="Dictionary{TKey, TValue}.Count" />
        public int Count
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _dicUnorderedData.Count;
            }
        }

        /// <inheritdoc />
        public object SyncRoot
        {
            get
            {
                if (_objSyncRoot == null)
                {
                    Interlocked.CompareExchange<object>(ref _objSyncRoot, new object(), null);
                }

                return _objSyncRoot;
            }
        }

        private object _objSyncRoot;

        /// <inheritdoc />
        public bool IsSynchronized => false;

        /// <inheritdoc cref="Dictionary{TKey, TValue}.Comparer" />
        public IEqualityComparer<TKey> Comparer
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _dicUnorderedData.Comparer;
            }
        }

        /// <inheritdoc />
        public ICollection<TKey> Keys
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _lstIndexes;
            }
        }

        /// <inheritdoc />
        public ICollection<TValue> Values
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    // Needed to make sure ordering is retained
                    List<TValue> lstReturn = new List<TValue>(_dicUnorderedData.Count);
                    for (int i = 0; i < _dicUnorderedData.Count; ++i)
                        lstReturn.Add(_dicUnorderedData[_lstIndexes[i]]);
                    return lstReturn;
                }
            }
        }

        public ICollection<TKey> KeysUnsorted
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _dicUnorderedData.Keys;
            }
        }

        public ICollection<TValue> ValuesUnsorted
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _dicUnorderedData.Values;
            }
        }

        public IReadOnlyList<TKey> ReadOnlyKeys
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _lstIndexes;
            }
        }

        public IReadOnlyList<TValue> ReadOnlyValues
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    // Needed to make sure ordering is retained
                    List<TValue> lstReturn = new List<TValue>(_dicUnorderedData.Count);
                    for (int i = 0; i < _dicUnorderedData.Count; ++i)
                        lstReturn.Add(_dicUnorderedData[_lstIndexes[i]]);
                    return lstReturn;
                }
            }
        }

        public IReadOnlyCollection<TKey> ReadOnlyKeysUnsorted
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _dicUnorderedData.Keys;
            }
        }

        public IReadOnlyCollection<TValue> ReadOnlyValuesUnsorted
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _dicUnorderedData.Values;
            }
        }

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _lstIndexes;
            }
        }

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    // Needed to make sure ordering is retained
                    for (int i = 0; i < _dicUnorderedData.Count; ++i)
                        yield return _dicUnorderedData[_lstIndexes[i]];
                }
            }
        }

        ICollection IDictionary.Keys
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _lstIndexes;
            }
        }

        ICollection IDictionary.Values
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    // Needed to make sure ordering is retained
                    List<TValue> lstReturn = new List<TValue>(_dicUnorderedData.Count);
                    for (int i = 0; i < _dicUnorderedData.Count; ++i)
                        lstReturn.Add(_dicUnorderedData[_lstIndexes[i]]);
                    return lstReturn;
                }
            }
        }

        public bool IsReadOnly => false;

        public bool IsFixedSize => false;

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Contains(item) && Remove(item.Key);
        }

        public bool Remove(Tuple<TKey, TValue> item)
        {
            return item != null && Contains(item) && Remove(item.Item1);
        }

        public void Remove(object key)
        {
            switch (key)
            {
                case TKey objKey:
                    Remove(objKey);
                    break;

                case int intIndex:
                    RemoveAt(intIndex);
                    break;
            }
        }

        public bool Remove(TKey key)
        {
            using (LockObject.EnterWriteLock())
            {
                if (!_dicUnorderedData.Remove(key))
                    return false;
                _lstIndexes.Remove(key);
                return true;
            }
        }

        /// <inheritdoc cref="Dictionary{TKey, TValue}.TryGetValue" />
        public bool TryGetValue(TKey key, out TValue value)
        {
            using (EnterReadLock.Enter(LockObject))
                return _dicUnorderedData.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return _dicUnorderedData[key];
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    TValue objOldValue = _dicUnorderedData[key];
                    if (objOldValue.Equals(value))
                        return;
                    using (LockObject.EnterWriteLock())
                        _dicUnorderedData[key] = value;
                }
            }
        }

        public object this[object key]
        {
            get
            {
                switch (key)
                {
                    case TKey objKey:
                        return this[objKey];

                    case int intKey:
                        return this[intKey];

                    default:
                        throw new ArgumentException(null, nameof(key));
                }
            }
            set
            {
                switch (value)
                {
                    case TValue objValue when key is TKey objKey:
                        this[objKey] = objValue;
                        break;

                    case KeyValuePair<TKey, TValue> objKeyValuePair when key is int intKey:
                        this[intKey] = objKeyValuePair;
                        break;

                    case Tuple<TKey, TValue> objTuple when key is int intKey:
                        this[intKey] = new KeyValuePair<TKey, TValue>(objTuple.Item1, objTuple.Item2);
                        break;

                    default:
                        throw new InvalidOperationException(nameof(value));
                }
            }
        }

        public KeyValuePair<TKey, TValue> this[int index]
        {
            get
            {
                using (EnterReadLock.Enter(LockObject))
                    return new KeyValuePair<TKey, TValue>(_lstIndexes[index], _dicUnorderedData[_lstIndexes[index]]);
            }
            set
            {
                using (EnterReadLock.Enter(LockObject))
                {
                    if (_dicUnorderedData.TryGetValue(value.Key, out TValue objOldValue))
                    {
                        int intOriginalIndex = _lstIndexes.IndexOf(value.Key);
                        if (index == intOriginalIndex)
                            return;
                        TKey objKeyToRemove = _lstIndexes[index];
                        using (LockObject.EnterWriteLock())
                        {
                            _lstIndexes[index] = value.Key;
                            for (int i = intOriginalIndex; i < _lstIndexes.Count - 2; ++i)
                            {
                                if (i != index)
                                    _lstIndexes[i] = _lstIndexes[i + 1];
                            }

                            _lstIndexes.RemoveAt(_lstIndexes.Count - 1);
                            if (objKeyToRemove != null)
                                _dicUnorderedData.Remove(objKeyToRemove);
                            if (!objOldValue.Equals(value.Value))
                                _dicUnorderedData[value.Key] = value.Value;
                        }
                    }
                    else
                    {
                        using (LockObject.EnterWriteLock())
                        {
                            TKey objKeyToRemove = _lstIndexes[index];
                            _dicUnorderedData.Remove(objKeyToRemove);
                            _dicUnorderedData.Add(value.Key, value.Value);
                            _lstIndexes[index] = value.Key;
                        }
                    }
                }
            }
        }

        public int IndexOf(TKey key)
        {
            using (EnterReadLock.Enter(LockObject))
                return _dicUnorderedData.ContainsKey(key)
                    ? _lstIndexes.IndexOf(key)
                    : -1;
        }

        public int IndexOf(KeyValuePair<TKey, TValue> item)
        {
            return _dicUnorderedData.ContainsKey(item.Key) && _dicUnorderedData[item.Key].Equals(item.Value)
                ? _lstIndexes.IndexOf(item.Key)
                : -1;
        }

        public int IndexOf(Tuple<TKey, TValue> item)
        {
            using (EnterReadLock.Enter(LockObject))
                return item != null && _dicUnorderedData.ContainsKey(item.Item1)
                                    && _dicUnorderedData[item.Item1].Equals(item.Item2)
                    ? _lstIndexes.IndexOf(item.Item1)
                    : -1;
        }

        public int LastIndexOf(TKey key)
        {
            using (EnterReadLock.Enter(LockObject))
                return _dicUnorderedData.ContainsKey(key)
                    ? _lstIndexes.LastIndexOf(key)
                    : -1;
        }

        public int LastIndexOf(KeyValuePair<TKey, TValue> item)
        {
            using (EnterReadLock.Enter(LockObject))
                return _dicUnorderedData.ContainsKey(item.Key) && _dicUnorderedData[item.Key].Equals(item.Value)
                    ? _lstIndexes.LastIndexOf(item.Key)
                    : -1;
        }

        public int LastIndexOf(Tuple<TKey, TValue> item)
        {
            using (EnterReadLock.Enter(LockObject))
                return item != null && _dicUnorderedData.ContainsKey(item.Item1)
                                    && _dicUnorderedData[item.Item1].Equals(item.Item2)
                    ? _lstIndexes.LastIndexOf(item.Item1)
                    : -1;
        }

        public List<TKey> FindAll(Predicate<TKey> predicate)
        {
            using (EnterReadLock.Enter(LockObject))
                return _lstIndexes.FindAll(predicate);
        }

        public TypedOrderedDictionary<TKey, TValue> FindAll(Predicate<KeyValuePair<TKey, TValue>> predicate)
        {
            TypedOrderedDictionary<TKey, TValue> dicReturn = new TypedOrderedDictionary<TKey, TValue>(_dicUnorderedData.Count);
            for (int i = 0; i < _dicUnorderedData.Count; ++i)
            {
                KeyValuePair<TKey, TValue> kvpLoop = this[i];
                if (predicate(kvpLoop))
                    dicReturn.Add(kvpLoop);
            }
            return dicReturn;
        }

        public List<Tuple<TKey, TValue>> FindAll(Predicate<Tuple<TKey, TValue>> predicate)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                List<Tuple<TKey, TValue>> lstReturn = new List<Tuple<TKey, TValue>>(_dicUnorderedData.Count);
                for (int i = 0; i < _dicUnorderedData.Count; ++i)
                {
                    Tuple<TKey, TValue> tupLoop
                        = new Tuple<TKey, TValue>(_lstIndexes[i], _dicUnorderedData[_lstIndexes[i]]);
                    if (predicate(tupLoop))
                        lstReturn.Add(tupLoop);
                }

                return lstReturn;
            }
        }

        public void Insert(int index, KeyValuePair<TKey, TValue> item)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                if (_dicUnorderedData.ContainsKey(item.Key))
                    throw new ArgumentException(null, nameof(item));
                using (LockObject.EnterWriteLock())
                {
                    _dicUnorderedData.Add(item.Key, item.Value);
                    _lstIndexes.Insert(index, item.Key);
                }
            }
        }

        public void Insert(int index, Tuple<TKey, TValue> item)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                if (item == null)
                    throw new ArgumentNullException(nameof(item));
                if (_dicUnorderedData.ContainsKey(item.Item1))
                    throw new ArgumentException(null, nameof(item));
                using (LockObject.EnterWriteLock())
                {
                    _dicUnorderedData.Add(item.Item1, item.Item2);
                    _lstIndexes.Insert(index, item.Item1);
                }
            }
        }

        public void RemoveAt(int index)
        {
            using (EnterReadLock.Enter(LockObject))
            {
                TKey objKeyToRemove = _lstIndexes[index];
                if (objKeyToRemove.Equals(default))
                    return;
                using (LockObject.EnterWriteLock())
                {
                    _dicUnorderedData.Remove(objKeyToRemove);
                    _lstIndexes.RemoveAt(index);
                }
            }
        }

        public void Reverse(int index, int count)
        {
            using (LockObject.EnterWriteLock())
                _lstIndexes.Reverse(index, count);
        }

        public void Sort()
        {
            using (LockObject.EnterWriteLock())
                _lstIndexes.Sort();
        }

        public void Sort(Comparison<TKey> comparison)
        {
            using (LockObject.EnterWriteLock())
                _lstIndexes.Sort(comparison);
        }

        public void Sort(Comparison<KeyValuePair<TKey, TValue>> comparison)
        {
            using (LockObject.EnterWriteLock())
                _lstIndexes.Sort((x, y) => comparison(new KeyValuePair<TKey, TValue>(x, _dicUnorderedData[x]),
                                                      new KeyValuePair<TKey, TValue>(y, _dicUnorderedData[y])));
        }

        public void Sort(Comparison<Tuple<TKey, TValue>> comparison)
        {
            using (LockObject.EnterWriteLock())
                _lstIndexes.Sort((x, y) => comparison(new Tuple<TKey, TValue>(x, _dicUnorderedData[x]),
                                                      new Tuple<TKey, TValue>(y, _dicUnorderedData[y])));
        }

        public void Sort(IComparer<TKey> comparer)
        {
            using (LockObject.EnterWriteLock())
                _lstIndexes.Sort(comparer);
        }

        public void Sort(IComparer<KeyValuePair<TKey, TValue>> comparer)
        {
            using (LockObject.EnterWriteLock())
                _lstIndexes.Sort((x, y) => comparer.Compare(new KeyValuePair<TKey, TValue>(x, _dicUnorderedData[x]),
                                                            new KeyValuePair<TKey, TValue>(y, _dicUnorderedData[y])));
        }

        public void Sort(IComparer<Tuple<TKey, TValue>> comparer)
        {
            using (LockObject.EnterWriteLock())
                _lstIndexes.Sort((x, y) => comparer.Compare(new Tuple<TKey, TValue>(x, _dicUnorderedData[x]),
                                                            new Tuple<TKey, TValue>(y, _dicUnorderedData[y])));
        }

        public void Sort(int index, int count, IComparer<TKey> comparer)
        {
            using (LockObject.EnterWriteLock())
                _lstIndexes.Sort(index, count, comparer);
        }

        public void Sort(int index, int count, IComparer<KeyValuePair<TKey, TValue>> comparer)
        {
            using (LockObject.EnterWriteLock())
                _lstIndexes.Sort(index, count, new KeyValueToKeyComparer(this, comparer));
        }

        public void Sort(int index, int count, IComparer<Tuple<TKey, TValue>> comparer)
        {
            using (LockObject.EnterWriteLock())
                _lstIndexes.Sort(index, count, new KeyValueToKeyComparer(this, comparer));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            using (EnterReadLock.Enter(LockObject))
                _dicUnorderedData.GetObjectData(info, context);
        }

        public void OnDeserialization(object sender)
        {
            using (EnterReadLock.Enter(LockObject))
                _dicUnorderedData.OnDeserialization(sender);
        }

        private sealed class KeyValueToKeyComparer : IComparer<TKey>
        {
            private readonly LockingTypedOrderedDictionary<TKey, TValue> _dicMyDictionary;
            private readonly IComparer<KeyValuePair<TKey, TValue>> _objMyComparer;
            private readonly IComparer<Tuple<TKey, TValue>> _objMyTupleComparer;

            public KeyValueToKeyComparer(LockingTypedOrderedDictionary<TKey, TValue> dictionary, IComparer<KeyValuePair<TKey, TValue>> comparer)
            {
                _dicMyDictionary = dictionary;
                _objMyComparer = comparer;
                _objMyTupleComparer = null;
            }

            public KeyValueToKeyComparer(LockingTypedOrderedDictionary<TKey, TValue> dictionary, IComparer<Tuple<TKey, TValue>> comparer)
            {
                _dicMyDictionary = dictionary;
                _objMyComparer = null;
                _objMyTupleComparer = comparer;
            }

            public int Compare(TKey x, TKey y)
            {
                if (x == null)
                {
                    if (y == null)
                        return 0;
                    return -1;
                }

                if (y == null)
                    return 1;

                return _objMyComparer?.Compare(new KeyValuePair<TKey, TValue>(x, _dicMyDictionary._dicUnorderedData[x]),
                    new KeyValuePair<TKey, TValue>(y, _dicMyDictionary._dicUnorderedData[y]))
                       ?? _objMyTupleComparer.Compare(new Tuple<TKey, TValue>(x, _dicMyDictionary._dicUnorderedData[x]),
                           new Tuple<TKey, TValue>(y, _dicMyDictionary._dicUnorderedData[y]));
            }
        }

        private sealed class LockingTypedOrderedDictionaryEnumerator : IDictionaryEnumerator, IEnumerator<KeyValuePair<TKey, TValue>>
        {
            // A copy of the SimpleDictionary object's key/value pairs.
            private readonly LockingTypedOrderedDictionary<TKey, TValue> _dicMyDictionary;

            private int _intIndex = -1;

            public LockingTypedOrderedDictionaryEnumerator(LockingTypedOrderedDictionary<TKey, TValue> dictionary)
            {
                _dicMyDictionary = dictionary;
            }

            // Return the current item.
            public object Current
            {
                get
                {
                    ValidateIndex();
                    TKey objKey = _dicMyDictionary._lstIndexes[_intIndex];
                    return new KeyValuePair<TKey, TValue>(objKey, _dicMyDictionary._dicUnorderedData[objKey]);
                }
            }

            // Return the current dictionary entry.
            public DictionaryEntry Entry => (DictionaryEntry?)Current ?? default;

            // Return the key of the current item.
            public object Key
            {
                get
                {
                    ValidateIndex();
                    return _dicMyDictionary._lstIndexes[_intIndex];
                }
            }

            // Return the value of the current item.
            public object Value
            {
                get
                {
                    ValidateIndex();
                    return _dicMyDictionary._dicUnorderedData[_dicMyDictionary._lstIndexes[_intIndex]];
                }
            }

            // Advance to the next item.
            public bool MoveNext()
            {
                ++_intIndex;
                return _intIndex < _dicMyDictionary._lstIndexes.Count;
            }

            // Validate the enumeration index and throw an exception if the index is out of range.
            private void ValidateIndex()
            {
                if (_intIndex < 0 || _intIndex >= _dicMyDictionary._lstIndexes.Count)
                    throw new InvalidOperationException("Enumerator is before or after the collection.");
            }

            // Reset the index to restart the enumeration.
            public void Reset()
            {
                _intIndex = -1;
            }

            /// <inheritdoc />
            KeyValuePair<TKey, TValue> IEnumerator<KeyValuePair<TKey, TValue>>.Current
            {
                get
                {
                    ValidateIndex();
                    TKey objKey = _dicMyDictionary._lstIndexes[_intIndex];
                    return new KeyValuePair<TKey, TValue>(objKey, _dicMyDictionary._dicUnorderedData[objKey]);
                }
            }

            /// <inheritdoc />
            public void Dispose()
            {
                _intIndex = -1;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            LockObject.Dispose();
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            return LockObject.DisposeAsync();
        }

        /// <inheritdoc />
        public AsyncFriendlyReaderWriterLock LockObject { get; } = new AsyncFriendlyReaderWriterLock();

        /// <inheritdoc />
        public ValueTask<int> CountAsync => GetCountAsync();

        private async ValueTask<int> GetCountAsync()
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _dicUnorderedData.Count;
        }

        /// <inheritdoc />
        public async ValueTask<bool> ContainsKeyAsync(TKey key)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
                return _dicUnorderedData.ContainsKey(key);
        }

        /// <inheritdoc />
        public async ValueTask<Tuple<bool, TValue>> TryGetValueAsync(TKey key)
        {
            using (await EnterReadLock.EnterAsync(LockObject))
            {
                bool blnSuccess = _dicUnorderedData.TryGetValue(key, out TValue value);
                return new Tuple<bool, TValue>(blnSuccess, value);
            }
        }
    }
}
