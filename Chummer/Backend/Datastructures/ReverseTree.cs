using System.Collections;
using System.Collections.Generic;

namespace Chummer.Datastructures
{
    /// <summary>
    /// This class is for multiple objects that depend on one or more child objects
    /// When changing the child, this will allow easily fetching every impacted parrent
    /// </summary>
    internal class ReverseTree<T> : IEnumerable<T>
    {
        private readonly T _self;
        private ReverseTree<T> _parent;
        private ReverseTree<T> _root;
        //private ReverseTree<T>[] _children;
        private Dictionary<T, ReverseTree<T>> _seachDictionary = new Dictionary<T, ReverseTree<T>>();
        public ReverseTree(T self, params ReverseTree<T>[] children)
        {
            _self = self;
            _seachDictionary.Add(self, this);

            foreach (ReverseTree<T> child in children)
            {
                child._parent = this;
                child.setRoot(this);
            }
        }



        private void setRoot(ReverseTree<T> root)
        {
            /*
            if (_children != null)
            {
                foreach (ReverseTree<T> reverseTree in _children)
                {
                    reverseTree.setRoot(root);
                }
            }
            */
            _root = root;
            foreach (KeyValuePair<T, ReverseTree<T>> keyValuePair in _seachDictionary)
            {
                root._seachDictionary.Add(keyValuePair.Key, keyValuePair.Value);
            }
            _seachDictionary = root._seachDictionary;
        }

        public ReverseTree<T> Find(T key)
        {
            ReverseTree<T> objRet;
            if (!_seachDictionary.TryGetValue(key, out objRet))
            {
                objRet = new ReverseTree<T>(key); // single tree with only key
            }

            return objRet;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            ReverseTree<T> current = this;

            yield return current._self;
            while (current._parent != null)
            {
                current = current._parent;
                yield return current._self;
            }

        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
