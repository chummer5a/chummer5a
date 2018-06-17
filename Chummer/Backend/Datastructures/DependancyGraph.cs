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
using System.Collections.Generic;
using System.Linq;

namespace Chummer
{
    /// <summary>
    /// This class is for managing directed graphs where each node is an object and each directed edge points from a given object to another object on which the first depends.
    /// When changing an object, this allows for any and all objects that depend on the first in some way to be fetched.
    /// </summary>
    public sealed class DependancyGraph<T>
    {
        /// <summary>
        /// Initializes a directed graph of dependant items based on a blueprint specified in the constructor.
        /// </summary>
        /// <param name="lstGraphNodes">Blueprints of nodes that should be followed when constructing the DependancyGraph. Make sure you use the correct DependancyGraphNode constructor!</param>
        public DependancyGraph(params DependancyGraphNode<T>[] lstGraphNodes)
        {
            foreach (DependancyGraphNode<T> objGraphNode in lstGraphNodes)
            {
                TryAddCopyToDictionary(objGraphNode);
            }
        }

        /// <summary>
        /// Returns a collection containing the current key's object and all objects that depend on the current key.
        /// Slower but idiot-proof compared to GetWithAllDependantsUnsafe().
        /// </summary>
        /// <param name="objKey">Fetch the node associated with this object.</param>
        public ICollection<T> GetWithAllDependants(T objKey)
        {
            HashSet<T> objReturn = new HashSet<T>();
            if (NodeDictionary.TryGetValue(objKey, out DependancyGraphNode<T> objLoopNode))
            {
                if (objReturn.Add(objLoopNode.MyObject))
                {
                    foreach (DependancyGraphNode<T> objDependant in objLoopNode.UpStreamNodes.Where(x => x.DependancyCondition?.Invoke() != false).Select(x => x.Node))
                    {
                        CollectDependants(objDependant.MyObject, objReturn);
                    }
                }
            }
            else
                objReturn.Add(objKey);

            return objReturn;
        }

        /// <summary>
        /// Collects the current key's object and all objects that depend on the current key into an ever-growing HashSet.
        /// </summary>
        /// <param name="objKey">Fetch the node associated with this object.</param>
        /// <param name="objReturn">HashSet containing all keys that depend on <paramref name="objKey"/> in some way. It's a HashSet to prevent infinite loops in case of cycles</param>
        private void CollectDependants(T objKey, HashSet<T> objReturn)
        {
            if (NodeDictionary.TryGetValue(objKey, out DependancyGraphNode<T> objLoopNode))
            {
                if (objReturn.Add(objLoopNode.MyObject))
                {
                    foreach (DependancyGraphNode<T> objDependant in objLoopNode.UpStreamNodes.Where(x => x.DependancyCondition?.Invoke() != false).Select(x => x.Node))
                    {
                        CollectDependants(objDependant.MyObject, objReturn);
                    }
                }
            }
        }

        /// <summary>
        /// Returns an enumerable containing the current key's object and all objects that depend on the current key.
        /// Warning: DependancyGraphs with any cycles will cause this method to never terminate!
        /// </summary>
        /// <param name="objKey">Fetch the node associated with this object.</param>
        public IEnumerable<T> GetWithAllDependantsUnsafe(T objKey)
        {
            if (NodeDictionary.TryGetValue(objKey, out DependancyGraphNode<T> objLoopNode))
            {
                yield return objLoopNode.MyObject;
                foreach (DependancyGraphNode<T> objDependant in objLoopNode.UpStreamNodes.Where(x => x.DependancyCondition?.Invoke() != false).Select(x => x.Node))
                {
                    foreach (T objDependantObject in GetWithAllDependantsUnsafe(objDependant.MyObject))
                    {
                        yield return objDependantObject;
                    }
                }
            }
        }

        private readonly Dictionary<T, DependancyGraphNode<T>> _dicNodeDictionary = new Dictionary<T, DependancyGraphNode<T>>();
        /// <summary>
        /// Dictionary of nodes in the graph. This is where the graph is actually stored, and doubles as a fast way to get any node in the graph.
        /// It's an IReadOnlyDictionary because dependancy graphs are intended to be set up once based on a blueprint called as part of the constructor.
        /// </summary>
        public IReadOnlyDictionary<T, DependancyGraphNode<T>> NodeDictionary => _dicNodeDictionary;

        /// <summary>
        /// Attempts to add a copy of a DependancyGraphNode to the internal dictionary.
        /// </summary>
        /// <param name="objDependancyGraphNode"></param>
        /// <returns></returns>
        public DependancyGraphNode<T> TryAddCopyToDictionary(DependancyGraphNode<T> objDependancyGraphNode)
        {
            T objLoopKey = objDependancyGraphNode.MyObject;
            if (!NodeDictionary.TryGetValue(objLoopKey, out DependancyGraphNode<T> objExistingValue))
            {
                objExistingValue = new DependancyGraphNode<T>(objLoopKey, this);
                // This is the first time the DependancyGraphNode object was attempted to be added to the dictionary, so don't do anything extra
                _dicNodeDictionary.Add(objLoopKey, objExistingValue);
            }

            // Attempt to add all descendants of the current DependancyGraphNode to the SearchDictionary
            foreach (DependancyGraphNodeWithCondition<T> objDownStreamNode in objDependancyGraphNode.DownStreamNodes)
            {
                if (!NodeDictionary.TryGetValue(objDownStreamNode.Node.MyObject, out DependancyGraphNode<T> objLoopValue) || !objLoopValue.Initializing)
                {
                    bool blnTempLoopValueInitializing = objLoopValue?.Initializing == false;
                    if (blnTempLoopValueInitializing)
                        objLoopValue.Initializing = true;
                    DependancyGraphNode<T> objDownStreamNodeCopy = TryAddCopyToDictionary(objDownStreamNode.Node);
                    objExistingValue.DownStreamNodes.Add(new DependancyGraphNodeWithCondition<T>(objDownStreamNodeCopy, objDownStreamNode.DependancyCondition));
                    objDownStreamNodeCopy.UpStreamNodes.Add(new DependancyGraphNodeWithCondition<T>(objExistingValue, objDownStreamNode.DependancyCondition));
                    if (blnTempLoopValueInitializing)
                        objLoopValue.Initializing = false;
                }
            }

            // Attempt to add all dependants of the current DependancyGraphNode to the SearchDictionary
            foreach (DependancyGraphNodeWithCondition<T> objUpStreamNode in objDependancyGraphNode.UpStreamNodes)
            {
                if (!NodeDictionary.TryGetValue(objUpStreamNode.Node.MyObject, out DependancyGraphNode<T> objLoopValue) || !objLoopValue.Initializing)
                {
                    bool blnTempLoopValueInitializing = objLoopValue?.Initializing == false;
                    if (blnTempLoopValueInitializing)
                        objLoopValue.Initializing = true;
                    DependancyGraphNode<T> objUpStreamNodeCopy = TryAddCopyToDictionary(objUpStreamNode.Node);
                    objExistingValue.UpStreamNodes.Add(new DependancyGraphNodeWithCondition<T>(objUpStreamNodeCopy, objUpStreamNode.DependancyCondition));
                    objUpStreamNodeCopy.DownStreamNodes.Add(new DependancyGraphNodeWithCondition<T>(objExistingValue, objUpStreamNode.DependancyCondition));
                    if (blnTempLoopValueInitializing)
                        objLoopValue.Initializing = false;
                }
            }

            objExistingValue.Initializing = false;
            return objExistingValue;
        }
    }
}
