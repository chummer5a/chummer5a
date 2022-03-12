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
using System.Runtime.Serialization;
using System.Threading;

namespace Chummer
{
    /// <summary>
    /// Like OrderedDictionary, but with key and value types explicitly defined, therefore allowing it to be used in extension/interface methods for dictionaries and lists
    /// </summary>
    /// <typeparam name="TKey">Type used for unique keys in the internal dictionary</typeparam>
    /// <typeparam name="TValue">Type used for values in the internal dictionary</typeparam>
    public class TypedOrderedDictionary<TKey, TValue> :
        IDictionary<TKey, TValue>,
        IList<KeyValuePair<TKey, TValue>>,
        IDictionary,
        IReadOnlyDictionary<TKey, TValue>,
        IReadOnlyList<KeyValuePair<TKey, TValue>>,
        ISerializable,
        IDeserializationCallback
    {
        private readonly Dictionary<TKey, TValue> _dicUnorderedData;
        private readonly List<TKey> _lstIndexes;

        public TypedOrderedDictionary()
        {
            _dicUnorderedData = new Dictionary<TKey, TValue>();
            _lstIndexes = new List<TKey>();
        }

        public TypedOrderedDictionary(IDictionary<TKey, TValue> dictionary)
        {
            _dicUnorderedData = new Dictionary<TKey, TValue>(dictionary);
            _lstIndexes = new List<TKey>(dictionary.Keys);
        }

        public TypedOrderedDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
        {
            _dicUnorderedData = new Dictionary<TKey, TValue>(dictionary, comparer);
            _lstIndexes = new List<TKey>(dictionary.Keys);
        }

        public TypedOrderedDictionary(IEqualityComparer<TKey> comparer)
        {
            _dicUnorderedData = new Dictionary<TKey, TValue>(comparer);
            _lstIndexes = new List<TKey>();
        }

        public TypedOrderedDictionary(int capacity)
        {
            _dicUnorderedData = new Dictionary<TKey, TValue>(capacity);
            _lstIndexes = new List<TKey>(capacity);
        }

        public TypedOrderedDictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            _dicUnorderedData = new Dictionary<TKey, TValue>(capacity, comparer);
            _lstIndexes = new List<TKey>(capacity);
        }

        /// <inheritdoc cref="Dictionary{TKey, TValue}.Clear" />
        public void Clear()
        {
            _dicUnorderedData.Clear();
            _lstIndexes.Clear();
        }

        /// <inheritdoc />
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            for (int i = 0; i < Count - 1; ++i)
                yield return this[i];
        }

        /// <inheritdoc />
        public IDictionaryEnumerator GetEnumerator()
        {
            return new TypedOrderedDictionaryEnumerator(this);
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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
            return _dicUnorderedData.TryGetValue(item.Key, out TValue objValue) && item.Value.Equals(objValue);
        }
        
        public bool Contains(Tuple<TKey, TValue> item)
        {
            (TKey objKey, TValue objValue) = item;
            return _dicUnorderedData.TryGetValue(objKey, out TValue objExistingValue) && objValue.Equals(objExistingValue);
        }

        /// <inheritdoc cref="Dictionary{TKey, TValue}.ContainsKey" />
        public bool ContainsKey(TKey key)
        {
            return _dicUnorderedData.ContainsKey(key);
        }

        /// <inheritdoc cref="Dictionary{TKey, TValue}.ContainsValue" />
        public bool ContainsValue(TValue value)
        {
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
            _dicUnorderedData.Add(key, value);
            _lstIndexes.Add(key);
        }

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (arrayIndex + Count >= array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            for (int i = 0; i < Count; ++i)
                array[i + arrayIndex] = this[i];
        }
        
        public void CopyTo(Tuple<TKey, TValue>[] array, int arrayIndex)
        {
            if (arrayIndex + Count >= array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            for (int i = 0; i < Count; ++i)
            {
                array[i + arrayIndex] =
                    new Tuple<TKey, TValue>(_lstIndexes[i], _dicUnorderedData[_lstIndexes[i]]);
            }
        }

        /// <inheritdoc />
        public void CopyTo(Array array, int index)
        {
            if (index + Count >= array.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            for (int i = 0; i < Count; ++i)
                array.SetValue(this[i], i + index);
        }

        /// <inheritdoc cref="Dictionary{TKey, TValue}.Count" />
        public int Count => _dicUnorderedData.Count;

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
        public IEqualityComparer<TKey> Comparer => _dicUnorderedData.Comparer;

        /// <inheritdoc />
        public ICollection<TKey> Keys => _lstIndexes;

        /// <inheritdoc />
        public ICollection<TValue> Values
        {
            get
            {
                // Needed to make sure ordering is retained
                List<TValue> lstReturn = new List<TValue>(Count);
                for (int i = 0; i < Count; ++i)
                    lstReturn.Add(_dicUnorderedData[_lstIndexes[i]]);
                return lstReturn;
            }
        }

        public ICollection<TKey> KeysUnsorted => _dicUnorderedData.Keys;

        public ICollection<TValue> ValuesUnsorted => _dicUnorderedData.Values;

        public IReadOnlyList<TKey> ReadOnlyKeys => _lstIndexes;

        public IReadOnlyList<TValue> ReadOnlyValues
        {
            get
            {
                // Needed to make sure ordering is retained
                List<TValue> lstReturn = new List<TValue>(Count);
                for (int i = 0; i < Count; ++i)
                    lstReturn.Add(_dicUnorderedData[_lstIndexes[i]]);
                return lstReturn;
            }
        }

        public IReadOnlyCollection<TKey> ReadOnlyKeysUnsorted => _dicUnorderedData.Keys;

        public IReadOnlyCollection<TValue> ReadOnlyValuesUnsorted => _dicUnorderedData.Values;

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => _lstIndexes;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
        {
            get
            {
                // Needed to make sure ordering is retained
                for (int i = 0; i < Count; ++i)
                    yield return _dicUnorderedData[_lstIndexes[i]];
            }
        }

        ICollection IDictionary.Keys => _lstIndexes;

        ICollection IDictionary.Values
        {
            get
            {
                // Needed to make sure ordering is retained
                List<TValue> lstReturn = new List<TValue>(Count);
                for (int i = 0; i < Count; ++i)
                    lstReturn.Add(_dicUnorderedData[_lstIndexes[i]]);
                return lstReturn;
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
            if (!_dicUnorderedData.Remove(key))
                return false;
            _lstIndexes.Remove(key);
            return true;
        }

        /// <inheritdoc cref="Dictionary{TKey, TValue}.TryGetValue" />
        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dicUnorderedData.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get => _dicUnorderedData[key];
            set => _dicUnorderedData[key] = value;
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
            get => new KeyValuePair<TKey, TValue>(_lstIndexes[index], _dicUnorderedData[_lstIndexes[index]]);
            set
            {
                if (_dicUnorderedData.TryGetValue(value.Key, out TValue objOldValue))
                {
                    int intOriginalIndex = _lstIndexes.IndexOf(value.Key);
                    if (index == intOriginalIndex)
                        return;
                    TKey objKeyToRemove = _lstIndexes[index];
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
                else
                {
                    TKey objKeyToRemove = _lstIndexes[index];
                    _dicUnorderedData.Remove(objKeyToRemove);
                    _dicUnorderedData.Add(value.Key, value.Value);
                    _lstIndexes[index] = value.Key;
                }
            }
        }

        public int IndexOf(TKey key)
        {
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
            return item != null && _dicUnorderedData.ContainsKey(item.Item1) && _dicUnorderedData[item.Item1].Equals(item.Item2)
                ? _lstIndexes.IndexOf(item.Item1)
                : -1;
        }

        public int LastIndexOf(TKey key)
        {
            return _dicUnorderedData.ContainsKey(key)
                ? _lstIndexes.LastIndexOf(key)
                : -1;
        }

        public int LastIndexOf(KeyValuePair<TKey, TValue> item)
        {
            return _dicUnorderedData.ContainsKey(item.Key) && _dicUnorderedData[item.Key].Equals(item.Value)
                ? _lstIndexes.LastIndexOf(item.Key)
                : -1;
        }

        public int LastIndexOf(Tuple<TKey, TValue> item)
        {
            return item != null && _dicUnorderedData.ContainsKey(item.Item1) && _dicUnorderedData[item.Item1].Equals(item.Item2)
                ? _lstIndexes.LastIndexOf(item.Item1)
                : -1;
        }

        public List<TKey> FindAll(Predicate<TKey> predicate)
        {
            return _lstIndexes.FindAll(predicate);
        }

        public TypedOrderedDictionary<TKey, TValue> FindAll(Predicate<KeyValuePair<TKey, TValue>> predicate)
        {
            TypedOrderedDictionary<TKey, TValue> dicReturn = new TypedOrderedDictionary<TKey, TValue>(Count);
            for (int i = 0; i < Count; ++i)
            {
                KeyValuePair<TKey, TValue> kvpLoop = this[i];
                if (predicate(kvpLoop))
                    dicReturn.Add(kvpLoop);
            }
            return dicReturn;
        }

        public List<Tuple<TKey, TValue>> FindAll(Predicate<Tuple<TKey, TValue>> predicate)
        {
            List<Tuple<TKey, TValue>> lstReturn = new List<Tuple<TKey, TValue>>(Count);
            for (int i = 0; i < Count; ++i)
            {
                Tuple<TKey, TValue> tupLoop = new Tuple<TKey, TValue>(_lstIndexes[i], _dicUnorderedData[_lstIndexes[i]]);
                if (predicate(tupLoop))
                    lstReturn.Add(tupLoop);
            }
            return lstReturn;
        }

        public void Insert(int index, KeyValuePair<TKey, TValue> item)
        {
            if (_dicUnorderedData.ContainsKey(item.Key))
                throw new ArgumentException(null, nameof(item));
            _dicUnorderedData.Add(item.Key, item.Value);
            _lstIndexes.Insert(index, item.Key);
        }

        public void Insert(int index, Tuple<TKey, TValue> item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (_dicUnorderedData.ContainsKey(item.Item1))
                throw new ArgumentException(null, nameof(item));
            _dicUnorderedData.Add(item.Item1, item.Item2);
            _lstIndexes.Insert(index, item.Item1);
        }

        public void RemoveAt(int index)
        {
            TKey objKeyToRemove = _lstIndexes[index];
            if (objKeyToRemove.Equals(default))
                return;
            _dicUnorderedData.Remove(objKeyToRemove);
            _lstIndexes.RemoveAt(index);
        }

        public void Reverse(int index, int count)
        {
            _lstIndexes.Reverse(index, count);
        }

        public void Sort()
        {
            _lstIndexes.Sort();
        }

        public void Sort(Comparison<TKey> comparison)
        {
            _lstIndexes.Sort(comparison);
        }

        public void Sort(Comparison<KeyValuePair<TKey, TValue>> comparison)
        {
            _lstIndexes.Sort((x, y) => comparison(new KeyValuePair<TKey, TValue>(x, _dicUnorderedData[x]),
                new KeyValuePair<TKey, TValue>(y, _dicUnorderedData[y])));
        }

        public void Sort(Comparison<Tuple<TKey, TValue>> comparison)
        {
            _lstIndexes.Sort((x, y) => comparison(new Tuple<TKey, TValue>(x, _dicUnorderedData[x]),
                new Tuple<TKey, TValue>(y, _dicUnorderedData[y])));
        }

        public void Sort(IComparer<TKey> comparer)
        {
            _lstIndexes.Sort(comparer);
        }

        public void Sort(IComparer<KeyValuePair<TKey, TValue>> comparer)
        {
            _lstIndexes.Sort((x, y) => comparer.Compare(new KeyValuePair<TKey, TValue>(x, _dicUnorderedData[x]),
                new KeyValuePair<TKey, TValue>(y, _dicUnorderedData[y])));
        }

        public void Sort(IComparer<Tuple<TKey, TValue>> comparer)
        {
            _lstIndexes.Sort((x, y) => comparer.Compare(new Tuple<TKey, TValue>(x, _dicUnorderedData[x]),
                new Tuple<TKey, TValue>(y, _dicUnorderedData[y])));
        }

        public void Sort(int index, int count, IComparer<TKey> comparer)
        {
            _lstIndexes.Sort(index, count, comparer);
        }

        public void Sort(int index, int count, IComparer<KeyValuePair<TKey, TValue>> comparer)
        {
            _lstIndexes.Sort(index, count, new KeyValueToKeyComparer(this, comparer));
        }

        public void Sort(int index, int count, IComparer<Tuple<TKey, TValue>> comparer)
        {
            _lstIndexes.Sort(index, count, new KeyValueToKeyComparer(this, comparer));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            _dicUnorderedData.GetObjectData(info, context);
        }

        public void OnDeserialization(object sender)
        {
            _dicUnorderedData.OnDeserialization(sender);
        }

        private sealed class KeyValueToKeyComparer : IComparer<TKey>
        {
            private readonly TypedOrderedDictionary<TKey, TValue> _dicMyDictionary;
            private readonly IComparer<KeyValuePair<TKey, TValue>> _objMyComparer;
            private readonly IComparer<Tuple<TKey, TValue>> _objMyTupleComparer;

            public KeyValueToKeyComparer(TypedOrderedDictionary<TKey, TValue> dictionary, IComparer<KeyValuePair<TKey, TValue>> comparer)
            {
                _dicMyDictionary = dictionary;
                _objMyComparer = comparer;
                _objMyTupleComparer = null;
            }

            public KeyValueToKeyComparer(TypedOrderedDictionary<TKey, TValue> dictionary, IComparer<Tuple<TKey, TValue>> comparer)
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

        private sealed class TypedOrderedDictionaryEnumerator : IDictionaryEnumerator
        {
            // A copy of the SimpleDictionary object's key/value pairs.
            private readonly TypedOrderedDictionary<TKey, TValue> _dicMyDictionary;

            private int _intIndex = -1;

            public TypedOrderedDictionaryEnumerator(TypedOrderedDictionary<TKey, TValue> dictionary)
            {
                _dicMyDictionary = dictionary;
            }

            // Return the current item.
            public object Current
            {
                get
                {
                    ValidateIndex();
                    return _dicMyDictionary[_intIndex];
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
                    return _dicMyDictionary[_intIndex].Key;
                }
            }

            // Return the value of the current item.
            public object Value
            {
                get
                {
                    ValidateIndex();
                    return _dicMyDictionary[_intIndex].Value;
                }
            }

            // Advance to the next item.
            public bool MoveNext()
            {
                ++_intIndex;
                return _intIndex < _dicMyDictionary.Count;
            }

            // Validate the enumeration index and throw an exception if the index is out of range.
            private void ValidateIndex()
            {
                if (_intIndex < 0 || _intIndex >= _dicMyDictionary.Count)
                    throw new InvalidOperationException("Enumerator is before or after the collection.");
            }

            // Reset the index to restart the enumeration.
            public void Reset()
            {
                _intIndex = -1;
            }
        }
    }
}
