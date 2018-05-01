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
using System.Diagnostics;

namespace Chummer
{
    /// <summary>
    /// A node for use with DependancyGraph. Essentially just a directed graph node that is doubly-linked with all graph nodes to which it has edges.
    /// </summary>
    [DebuggerDisplay("{" + nameof(MyObject) + "}")]
    public sealed class DependancyGraphNode<T>
    {
        /// <summary>
        /// Constructor used for to make a blueprint of a DependancyGraph.
        /// Use this constructor when specifying arguments for a DependancyGraph constructor.
        /// </summary>
        /// <param name="objMyObject">Object associated with the current node</param>
        /// <param name="lstDownStreamNodes">Any objects that depend on the object associated with the current node</param>
        public DependancyGraphNode(T objMyObject, params DependancyGraphNode<T>[] lstDownStreamNodes)
        {
            MyObject = objMyObject;

            foreach (DependancyGraphNode<T> objDownStreamNode in lstDownStreamNodes)
            {
                objDownStreamNode.UpStreamNodes.Add(new DependancyGraphNodeWithCondition<T>(this, null));
                DownStreamNodes.Add(new DependancyGraphNodeWithCondition<T>(objDownStreamNode, null));
            }
        }

        /// <summary>
        /// Constructor used for to make a blueprint of a DependancyGraph.
        /// Use this constructor when specifying arguments for a DependancyGraph constructor.
        /// </summary>
        /// <param name="objMyObject">Object associated with the current node</param>
        /// <param name="funcDependancyCondition">Function that must return true at the time of collecting dependancies in order for the dependancy to register.</param>
        /// <param name="lstDownStreamNodes">Any objects that depend on the object associated with the current node</param>
        public DependancyGraphNode(T objMyObject, Func<bool> funcDependancyCondition, params DependancyGraphNode<T>[] lstDownStreamNodes)
        {
            MyObject = objMyObject;

            foreach (DependancyGraphNode<T> objDownStreamNode in lstDownStreamNodes)
            {
                objDownStreamNode.UpStreamNodes.Add(new DependancyGraphNodeWithCondition<T>(this, funcDependancyCondition));
                DownStreamNodes.Add(new DependancyGraphNodeWithCondition<T>(objDownStreamNode, funcDependancyCondition));
            }
        }

        /// <summary>
        /// Constructor used for actually constructing a DependancyGraph (to make sure any and all parents and children of a node can always be found in that same graph).
        /// Use only inside of DependancyGraph methods!
        /// </summary>
        /// <param name="objMyObject">Object associated with the current node</param>
        /// <param name="objRoot">DependancyGraph to which this node is to belong</param>
        public DependancyGraphNode(T objMyObject, DependancyGraph<T> objRoot)
        {
            MyObject = objMyObject;
            Root = objRoot;
            Initializing = true;
        }

        /// <summary>
        /// True if the node is part of a graph and its links are still being processed, false otherwise.
        /// </summary>
        public bool Initializing { get; set; }

        /// <summary>
        /// Object tied to this node in the DependancyGraph
        /// </summary>
        public T MyObject { get; }

        /// <summary>
        /// Root DependancyGraph object to which this DependancyGraphNode is attached.
        /// </summary>
        public DependancyGraph<T> Root { get; }

        /// <summary>
        /// Collection of all items that depend on the object tied to this node.
        /// </summary>
        public ICollection<DependancyGraphNodeWithCondition<T>> UpStreamNodes { get; } = new HashSet<DependancyGraphNodeWithCondition<T>>();

        /// <summary>
        /// Collection of all items on which the object tied to this node depends.
        /// </summary>
        public ICollection<DependancyGraphNodeWithCondition<T>> DownStreamNodes { get; } = new HashSet<DependancyGraphNodeWithCondition<T>>();

        public override bool Equals(object obj)
        {
            if (Root != null)
            {
                if (obj is DependancyGraphNode<T> objOtherNode)
                {
                    return Root == objOtherNode.Root && (MyObject == null && objOtherNode.MyObject == null || MyObject?.Equals(objOtherNode.MyObject) == true);
                }
            }
            else if (obj is DependancyGraphNode<T> objOtherNode && objOtherNode.Root != null)
                return false;

            // ReSharper disable once BaseObjectEqualsIsObjectEquals
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            if (Root != null)
                return MyObject?.GetHashCode() ?? 0;

            // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return MyObject?.ToString() ?? string.Empty;
        }
    }
}
