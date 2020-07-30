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

namespace Chummer
{
    /// <summary>
    /// This class is for managing directed graphs where each node is an object and each directed edge points from a given object to another object on which the first depends.
    /// When changing an object, this allows for any and all objects that depend on the first in some way to be fetched.
    /// </summary>
    public sealed class DependencyGraph<T, T2>
    {
        /// <summary>
        /// Initializes a directed graph of dependent items based on a blueprint specified in the constructor.
        /// </summary>
        /// <param name="lstGraphNodes">Blueprints of nodes that should be followed when constructing the DependencyGraph. Make sure you use the correct DependencyGraphNode constructor!</param>
        public DependencyGraph(params DependencyGraphNode<T, T2>[] lstGraphNodes)
        {
            foreach (DependencyGraphNode<T, T2> objGraphNode in lstGraphNodes)
            {
                TryAddCopyToDictionary(objGraphNode);
            }
        }

        /// <summary>
        /// Returns a collection containing the current key's object and all objects that depend on the current key.
        /// Slower but idiot-proof compared to GetWithAllDependentsUnsafe().
        /// </summary>
        /// <param name="objParentInstance">Instance of the object whose dependencies are being processed, used for conditions.</param>
        /// <param name="objKey">Fetch the node associated with this object.</param>
        public HashSet<T> GetWithAllDependents(T2 objParentInstance, T objKey)
        {
            HashSet<T> objReturn = new HashSet<T>();
            if (NodeDictionary.TryGetValue(objKey, out DependencyGraphNode<T, T2> objLoopNode))
            {
                if (objReturn.Add(objLoopNode.MyObject))
                {
                    foreach (DependencyGraphNodeWithCondition<T, T2> objNode in objLoopNode.UpStreamNodes)
                    {
                        if (objNode.DependencyCondition?.Invoke(objParentInstance) != false)
                        {
                            CollectDependents(objParentInstance, objNode.Node.MyObject, objReturn);
                        }
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
        /// <param name="objParentInstance">Instance of the object whose dependencies are being processed, used for conditions.</param>
        /// <param name="objKey">Fetch the node associated with this object.</param>
        /// <param name="objReturn">HashSet containing all keys that depend on <paramref name="objKey"/> in some way. It's a HashSet to prevent infinite loops in case of cycles</param>
        private void CollectDependents(T2 objParentInstance, T objKey, HashSet<T> objReturn)
        {
            if (NodeDictionary.TryGetValue(objKey, out DependencyGraphNode<T, T2> objLoopNode))
            {
                if (objReturn.Add(objLoopNode.MyObject))
                {
                    foreach (DependencyGraphNodeWithCondition<T, T2> objNode in objLoopNode.UpStreamNodes)
                    {
                        if (objNode.DependencyCondition?.Invoke(objParentInstance) != false)
                        {
                            CollectDependents(objParentInstance, objNode.Node.MyObject, objReturn);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns an enumerable containing the current key's object and all objects that depend on the current key.
        /// Warning: DependencyGraphs with any cycles will cause this method to never terminate!
        /// </summary>
        /// <param name="objParentInstance">Instance of the object whose dependencies are being processed, used for conditions.</param>
        /// <param name="objKey">Fetch the node associated with this object.</param>
        public IEnumerable<T> GetWithAllDependentsUnsafe(T2 objParentInstance, T objKey)
        {
            if (NodeDictionary.TryGetValue(objKey, out DependencyGraphNode<T, T2> objLoopNode))
            {
                yield return objLoopNode.MyObject;
                foreach (DependencyGraphNodeWithCondition<T, T2> objNode in objLoopNode.UpStreamNodes)
                {
                    if (objNode.DependencyCondition?.Invoke(objParentInstance) != false)
                    {
                        foreach (T objDependantObject in GetWithAllDependentsUnsafe(objParentInstance, objNode.Node.MyObject))
                        {
                            yield return objDependantObject;
                        }
                    }
                }
            }
        }

        private readonly Dictionary<T, DependencyGraphNode<T, T2>> _dicNodeDictionary = new Dictionary<T, DependencyGraphNode<T, T2>>();
        /// <summary>
        /// Dictionary of nodes in the graph. This is where the graph is actually stored, and doubles as a fast way to get any node in the graph.
        /// It's an IReadOnlyDictionary because dependency graphs are intended to be set up once based on a blueprint called as part of the constructor.
        /// </summary>
        public IReadOnlyDictionary<T, DependencyGraphNode<T, T2>> NodeDictionary => _dicNodeDictionary;

        /// <summary>
        /// Attempts to add a copy of a DependencyGraphNode to the internal dictionary.
        /// </summary>
        /// <param name="objDependencyGraphNode"></param>
        /// <returns></returns>
        public DependencyGraphNode<T, T2> TryAddCopyToDictionary(DependencyGraphNode<T, T2> objDependencyGraphNode)
        {
            if (objDependencyGraphNode == null)
                throw new ArgumentNullException(nameof(objDependencyGraphNode));
            T objLoopKey = objDependencyGraphNode.MyObject;
            if (!NodeDictionary.TryGetValue(objLoopKey, out DependencyGraphNode<T, T2> objExistingValue))
            {
                objExistingValue = new DependencyGraphNode<T, T2>(objLoopKey, this);
                // This is the first time the DependencyGraphNode object was attempted to be added to the dictionary, so don't do anything extra
                _dicNodeDictionary.Add(objLoopKey, objExistingValue);
            }

            // Attempt to add all descendants of the current DependencyGraphNode to the SearchDictionary
            foreach (DependencyGraphNodeWithCondition<T, T2> objDownStreamNode in objDependencyGraphNode.DownStreamNodes)
            {
                if (!NodeDictionary.TryGetValue(objDownStreamNode.Node.MyObject, out DependencyGraphNode<T, T2> objLoopValue) || !objLoopValue.Initializing)
                {
                    bool blnTempLoopValueInitializing = objLoopValue?.Initializing == false;
                    if (blnTempLoopValueInitializing)
                        objLoopValue.Initializing = true;
                    DependencyGraphNode<T, T2> objDownStreamNodeCopy = TryAddCopyToDictionary(objDownStreamNode.Node);
                    objExistingValue.DownStreamNodes.Add(new DependencyGraphNodeWithCondition<T, T2>(objDownStreamNodeCopy, objDownStreamNode.DependencyCondition));
                    objDownStreamNodeCopy.UpStreamNodes.Add(new DependencyGraphNodeWithCondition<T, T2>(objExistingValue, objDownStreamNode.DependencyCondition));
                    if (blnTempLoopValueInitializing)
                        objLoopValue.Initializing = false;
                }
            }

            // Attempt to add all dependents of the current DependencyGraphNode to the SearchDictionary
            foreach (DependencyGraphNodeWithCondition<T, T2> objUpStreamNode in objDependencyGraphNode.UpStreamNodes)
            {
                if (!NodeDictionary.TryGetValue(objUpStreamNode.Node.MyObject, out DependencyGraphNode<T, T2> objLoopValue) || !objLoopValue.Initializing)
                {
                    bool blnTempLoopValueInitializing = objLoopValue?.Initializing == false;
                    if (blnTempLoopValueInitializing)
                        objLoopValue.Initializing = true;
                    DependencyGraphNode<T, T2> objUpStreamNodeCopy = TryAddCopyToDictionary(objUpStreamNode.Node);
                    objExistingValue.UpStreamNodes.Add(new DependencyGraphNodeWithCondition<T, T2>(objUpStreamNodeCopy, objUpStreamNode.DependencyCondition));
                    objUpStreamNodeCopy.DownStreamNodes.Add(new DependencyGraphNodeWithCondition<T, T2>(objExistingValue, objUpStreamNode.DependencyCondition));
                    if (blnTempLoopValueInitializing)
                        objLoopValue.Initializing = false;
                }
            }

            objExistingValue.Initializing = false;
            return objExistingValue;
        }
    }
}
