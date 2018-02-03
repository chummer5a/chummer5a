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
using System.Collections;
using System.Collections.Generic;

namespace Chummer
{
    /// <summary>
    /// This class is for multiple objects that depend on one or more child objects
    /// When changing the child, this will allow easily fetching every impacted parrent
    /// </summary>
    public sealed class ReverseTree<T> : IEnumerable<T>
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
            if (!_seachDictionary.TryGetValue(key, out ReverseTree<T> objRet))
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
