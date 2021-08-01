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
        IReadOnlyList<TValue>,
        IDictionary,
        IReadOnlyDictionary<TKey, TValue>,
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

        public void Clear()
        {
            _dicUnorderedData.Clear();
            _lstIndexes.Clear();
        }

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
        {
            for (int i = 0; i < Count - 1; ++i)
                yield return _dicUnorderedData[_lstIndexes[i]];
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            for (int i = 0; i < Count - 1; ++i)
                yield return new KeyValuePair<TKey, TValue>(_lstIndexes[i], _dicUnorderedData[_lstIndexes[i]]);
        }
        
        public IDictionaryEnumerator GetEnumerator()
        {
            return new TypedOrderedDictionaryEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

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

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _dicUnorderedData.TryGetValue(item.Key, out TValue objValue) && item.Value.Equals(objValue);
        }

        public bool Contains(Tuple<TKey, TValue> item)
        {
            return _dicUnorderedData.TryGetValue(item.Item1, out TValue objValue) && item.Item2.Equals(objValue);
        }

        public bool ContainsKey(TKey key)
        {
            return _dicUnorderedData.ContainsKey(key);
        }

        public bool ContainsValue(TValue value)
        {
            return _dicUnorderedData.ContainsValue(value);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Add(Tuple<TKey, TValue> item)
        {
            Add(item.Item1, item.Item2);
        }

        public void Add(object key, object value)
        {
            if (!(key is TKey objKey))
                throw new ArgumentException(nameof(objKey));
            if (!(value is TValue objValue))
                throw new ArgumentException(nameof(objValue));
            Add(objKey, objValue);
        }

        public void Add(TKey key, TValue value)
        {
            _dicUnorderedData.Add(key, value);
            _lstIndexes.Add(key);
        }

        public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> lstItems)
        {
            foreach (KeyValuePair<TKey, TValue> kvpItem in lstItems)
            {
                Add(kvpItem.Key, kvpItem.Value);
            }
        }

        public void AddRange(IEnumerable<Tuple<TKey, TValue>> lstItems)
        {
            foreach ((TKey objKey, TValue objValue) in lstItems)
            {
                Add(objKey, objValue);
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (arrayIndex + Count >= array.Length)
                throw new ArgumentOutOfRangeException();
            for (int i = 0; i < Count; ++i)
                array[i + arrayIndex] =
                    new KeyValuePair<TKey, TValue>(_lstIndexes[i], _dicUnorderedData[_lstIndexes[i]]);
        }

        public void CopyTo(Tuple<TKey, TValue>[] array, int arrayIndex)
        {
            if (arrayIndex + Count >= array.Length)
                throw new ArgumentOutOfRangeException();
            for (int i = 0; i < Count; ++i)
                array[i + arrayIndex] =
                    new Tuple<TKey, TValue>(_lstIndexes[i], _dicUnorderedData[_lstIndexes[i]]);
        }

        public void CopyTo(Array array, int index)
        {
            if (index + Count >= array.Length)
                throw new ArgumentOutOfRangeException();
            for (int i = 0; i < Count; ++i)
                array.SetValue(new KeyValuePair<TKey, TValue>(_lstIndexes[i], _dicUnorderedData[_lstIndexes[i]]), i + index);
        }

        public int Count => _dicUnorderedData.Count;

        public object SyncRoot { get; } = new object();

        public bool IsSynchronized => false;

        public IEqualityComparer<TKey> Comparer => _dicUnorderedData.Comparer;

        public ICollection<TKey> Keys => _dicUnorderedData.Keys;

        public ICollection<TValue> Values => _dicUnorderedData.Values;

        public IReadOnlyCollection<TKey> ReadOnlyKeys => _dicUnorderedData.Keys;

        public IReadOnlyCollection<TValue> ReadOnlyValues => _dicUnorderedData.Values;

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

        ICollection IDictionary.Values => _dicUnorderedData.Values;

        ICollection IDictionary.Keys => _dicUnorderedData.Keys;

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
                        throw new ArgumentException(nameof(key));
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
                        throw new ArgumentException(nameof(value));
                }
            }
        }

        public KeyValuePair<TKey, TValue> this[int index]
        {
            get => new KeyValuePair<TKey, TValue>(_lstIndexes[index], _dicUnorderedData[_lstIndexes[index]]);
            set
            {
                if (_dicUnorderedData.ContainsKey(value.Key))
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
                    if (!_dicUnorderedData[value.Key].Equals(value.Value))
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

        TValue IReadOnlyList<TValue>.this[int index] => _dicUnorderedData[_lstIndexes[index]];

        public int IndexOf(KeyValuePair<TKey, TValue> item)
        {
            return _dicUnorderedData.ContainsKey(item.Key)
                ? _lstIndexes.IndexOf(item.Key)
                : -1;
        }

        public int IndexOf(Tuple<TKey, TValue> item)
        {
            return item != null && _dicUnorderedData.ContainsKey(item.Item1)
                ? _lstIndexes.IndexOf(item.Item1)
                : -1;
        }

        public void Insert(int index, KeyValuePair<TKey, TValue> item)
        {
            if (_dicUnorderedData.ContainsKey(item.Key))
                throw new ArgumentException(nameof(item));
            _dicUnorderedData.Add(item.Key, item.Value);
            _lstIndexes.Insert(index, item.Key);
        }

        public void Insert(int index, Tuple<TKey, TValue> item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (_dicUnorderedData.ContainsKey(item.Item1))
                throw new ArgumentException(nameof(item));
            _dicUnorderedData.Add(item.Item1, item.Item2);
            _lstIndexes.Insert(index, item.Item1);
        }

        public void RemoveAt(int index)
        {
            TKey objKeyToRemove = _lstIndexes.ElementAt(index);
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

        public void Sort(IComparer<TKey> comparer)
        {
            _lstIndexes.Sort(comparer);
        }

        public void Sort(int index, int count, IComparer<TKey> comparer)
        {
            _lstIndexes.Sort(index, count, comparer);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            _dicUnorderedData.GetObjectData(info, context);
        }

        public void OnDeserialization(object sender)
        {
            _dicUnorderedData.OnDeserialization(sender);
        }

        private class TypedOrderedDictionaryEnumerator : IDictionaryEnumerator
        {
            // A copy of the SimpleDictionary object's key/value pairs.
            private readonly TypedOrderedDictionary<TKey, TValue> _dicMyDictionary;

            private int intIndex = -1;

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
                    return _dicMyDictionary[intIndex];
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
                    return _dicMyDictionary[intIndex].Key;
                }
            }

            // Return the value of the current item.
            public object Value
            {
                get
                {
                    ValidateIndex();
                    return _dicMyDictionary[intIndex].Value;
                }
            }

            // Advance to the next item.
            public bool MoveNext()
            {
                if (intIndex >= _dicMyDictionary.Count - 1)
                    return false;
                intIndex += 1;
                return true;
            }

            // Validate the enumeration index and throw an exception if the index is out of range.
            private void ValidateIndex()
            {
                if (intIndex < 0 || intIndex >= _dicMyDictionary.Count)
                    throw new InvalidOperationException("Enumerator is before or after the collection.");
            }

            // Reset the index to restart the enumeration.
            public void Reset()
            {
                intIndex = -1;
            }
        }
    }
}
