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
using System.Collections.Generic;
using System.Linq;

namespace Chummer
{
    /// <summary>
    /// This class is for managing directed graphs where each node is an object and each directed edge points from a given object to another object on which the first depends.
    /// When changing an object, this allows for any and all objects that depend on the first in some way to be fetched.
    /// </summary>
    public sealed class DependencyGraph<T>
    {
        /// <summary>
        /// Initializes a directed graph of dependent items based on a blueprint specified in the constructor.
        /// </summary>
        /// <param name="lstGraphNodes">Blueprints of nodes that should be followed when constructing the DependencyGraph. Make sure you use the correct DependencyGraphNode constructor!</param>
        public DependencyGraph(params DependencyGraphNode<T>[] lstGraphNodes)
        {
            foreach (DependencyGraphNode<T> objGraphNode in lstGraphNodes)
            {
                TryAddCopyToDictionary(objGraphNode);
            }
        }

        /// <summary>
        /// Returns a collection containing the current key's object and all objects that depend on the current key.
        /// Slower but idiot-proof compared to GetWithAllDependentsUnsafe().
        /// </summary>
        /// <param name="objKey">Fetch the node associated with this object.</param>
        public ICollection<T> GetWithAllDependents(T objKey)
        {
            HashSet<T> objReturn = new HashSet<T>();
            if (NodeDictionary.TryGetValue(objKey, out DependencyGraphNode<T> objLoopNode))
            {
                if (objReturn.Add(objLoopNode.MyObject))
                {
                    foreach (DependencyGraphNode<T> objDependant in objLoopNode.UpStreamNodes.Where(x => x.DependencyCondition?.Invoke() != false).Select(x => x.Node))
                    {
                        CollectDependents(objDependant.MyObject, objReturn);
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
        private void CollectDependents(T objKey, HashSet<T> objReturn)
        {
            if (NodeDictionary.TryGetValue(objKey, out DependencyGraphNode<T> objLoopNode))
            {
                if (objReturn.Add(objLoopNode.MyObject))
                {
                    foreach (DependencyGraphNode<T> objDependant in objLoopNode.UpStreamNodes.Where(x => x.DependencyCondition?.Invoke() != false).Select(x => x.Node))
                    {
                        CollectDependents(objDependant.MyObject, objReturn);
                    }
                }
            }
        }

        /// <summary>
        /// Returns an enumerable containing the current key's object and all objects that depend on the current key.
        /// Warning: DependencyGraphs with any cycles will cause this method to never terminate!
        /// </summary>
        /// <param name="objKey">Fetch the node associated with this object.</param>
        public IEnumerable<T> GetWithAllDependentsUnsafe(T objKey)
        {
            if (NodeDictionary.TryGetValue(objKey, out DependencyGraphNode<T> objLoopNode))
            {
                yield return objLoopNode.MyObject;
                foreach (DependencyGraphNode<T> objDependant in objLoopNode.UpStreamNodes.Where(x => x.DependencyCondition?.Invoke() != false).Select(x => x.Node))
                {
                    foreach (T objDependantObject in GetWithAllDependentsUnsafe(objDependant.MyObject))
                    {
                        yield return objDependantObject;
                    }
                }
            }
        }

        private readonly Dictionary<T, DependencyGraphNode<T>> _dicNodeDictionary = new Dictionary<T, DependencyGraphNode<T>>();
        /// <summary>
        /// Dictionary of nodes in the graph. This is where the graph is actually stored, and doubles as a fast way to get any node in the graph.
        /// It's an IReadOnlyDictionary because dependency graphs are intended to be set up once based on a blueprint called as part of the constructor.
        /// </summary>
        public IReadOnlyDictionary<T, DependencyGraphNode<T>> NodeDictionary => _dicNodeDictionary;

        /// <summary>
        /// Attempts to add a copy of a DependencyGraphNode to the internal dictionary.
        /// </summary>
        /// <param name="objDependencyGraphNode"></param>
        /// <returns></returns>
        public DependencyGraphNode<T> TryAddCopyToDictionary(DependencyGraphNode<T> objDependencyGraphNode)
        {
            if (objDependencyGraphNode == null)
                throw new ArgumentNullException(nameof(objDependencyGraphNode));
            T objLoopKey = objDependencyGraphNode.MyObject;
            if (!NodeDictionary.TryGetValue(objLoopKey, out DependencyGraphNode<T> objExistingValue))
            {
                objExistingValue = new DependencyGraphNode<T>(objLoopKey, this);
                // This is the first time the DependencyGraphNode object was attempted to be added to the dictionary, so don't do anything extra
                _dicNodeDictionary.Add(objLoopKey, objExistingValue);
            }

            // Attempt to add all descendants of the current DependencyGraphNode to the SearchDictionary
            foreach (DependencyGraphNodeWithCondition<T> objDownStreamNode in objDependencyGraphNode.DownStreamNodes)
            {
                if (!NodeDictionary.TryGetValue(objDownStreamNode.Node.MyObject, out DependencyGraphNode<T> objLoopValue) || !objLoopValue.Initializing)
                {
                    bool blnTempLoopValueInitializing = objLoopValue?.Initializing == false;
                    if (blnTempLoopValueInitializing)
                        objLoopValue.Initializing = true;
                    DependencyGraphNode<T> objDownStreamNodeCopy = TryAddCopyToDictionary(objDownStreamNode.Node);
                    objExistingValue.DownStreamNodes.Add(new DependencyGraphNodeWithCondition<T>(objDownStreamNodeCopy, objDownStreamNode.DependencyCondition));
                    objDownStreamNodeCopy.UpStreamNodes.Add(new DependencyGraphNodeWithCondition<T>(objExistingValue, objDownStreamNode.DependencyCondition));
                    if (blnTempLoopValueInitializing)
                        objLoopValue.Initializing = false;
                }
            }

            // Attempt to add all dependents of the current DependencyGraphNode to the SearchDictionary
            foreach (DependencyGraphNodeWithCondition<T> objUpStreamNode in objDependencyGraphNode.UpStreamNodes)
            {
                if (!NodeDictionary.TryGetValue(objUpStreamNode.Node.MyObject, out DependencyGraphNode<T> objLoopValue) || !objLoopValue.Initializing)
                {
                    bool blnTempLoopValueInitializing = objLoopValue?.Initializing == false;
                    if (blnTempLoopValueInitializing)
                        objLoopValue.Initializing = true;
                    DependencyGraphNode<T> objUpStreamNodeCopy = TryAddCopyToDictionary(objUpStreamNode.Node);
                    objExistingValue.UpStreamNodes.Add(new DependencyGraphNodeWithCondition<T>(objUpStreamNodeCopy, objUpStreamNode.DependencyCondition));
                    objUpStreamNodeCopy.DownStreamNodes.Add(new DependencyGraphNodeWithCondition<T>(objExistingValue, objUpStreamNode.DependencyCondition));
                    if (blnTempLoopValueInitializing)
                        objLoopValue.Initializing = false;
                }
            }

            objExistingValue.Initializing = false;
            return objExistingValue;
        }
    }
}
