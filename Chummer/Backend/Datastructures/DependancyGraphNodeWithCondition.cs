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

namespace Chummer
{
    public struct DependancyGraphNodeWithCondition<T>
    {
        public DependancyGraphNodeWithCondition(DependancyGraphNode<T> objNode, Func<bool> funcDependancyCondition)
        {
            Node = objNode;
            DependancyCondition = funcDependancyCondition;
        }

        public DependancyGraphNode<T> Node { get; }
        public Func<bool> DependancyCondition { get; }

        public static bool operator ==(DependancyGraphNodeWithCondition<T> objFirstEdge, DependancyGraphNodeWithCondition<T> objSecondEdge)
        {
            return ((objFirstEdge.Node == null && objSecondEdge.Node == null) || objFirstEdge.Node?.Equals(objSecondEdge.Node) == true);
        }

        public static bool operator !=(DependancyGraphNodeWithCondition<T> objFirstEdge, DependancyGraphNodeWithCondition<T> objSecondEdge)
        {
            return !((objFirstEdge.Node == null && objSecondEdge.Node == null) || objFirstEdge.Node?.Equals(objSecondEdge.Node) == true);
        }

        public override bool Equals(object obj)
        {
            return obj is DependancyGraphNodeWithCondition<T> objOtherEdge && ((Node == null && objOtherEdge.Node == null) || Node?.Equals(objOtherEdge.Node) == true);
        }

        public override int GetHashCode()
        {
            return Node.GetHashCode();
        }

        public override string ToString()
        {
            return Node?.ToString() ?? string.Empty;
        }
    }
}
