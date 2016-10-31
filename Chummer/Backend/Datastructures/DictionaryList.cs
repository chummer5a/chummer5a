using System.Collections;
using System.Collections.Generic;

namespace Chummer.Datastructures
{
    public class DictionaryList<TKey, TValue> : IEnumerable<KeyValuePair<TKey, List<TValue>>>
    {
        private Dictionary<TKey, List<TValue>> inner = new Dictionary<TKey, List<TValue>>();

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Add(TKey key, TValue value)
        {
            List<TValue> list;
            if (inner.TryGetValue(key, out list))
            {
                list.Add(value);
                return false;
            }
            else
            {
                list = new List<TValue>{value};
                inner.Add(key, list);
                return true;
            }

        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Clear(TKey key)
        {
            return inner.Remove(key);
        }

        public IEnumerator<KeyValuePair<TKey, List<TValue>>> GetEnumerator()
        {
            return inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}