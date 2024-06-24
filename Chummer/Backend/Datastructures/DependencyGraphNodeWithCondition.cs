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
using System.Threading;
using System.Threading.Tasks;

namespace Chummer
{
    public readonly struct DependencyGraphNodeWithCondition<T, T2> : IEquatable<DependencyGraphNodeWithCondition<T, T2>>
    {
        public DependencyGraphNodeWithCondition(DependencyGraphNode<T, T2> objNode, Func<T2, bool> funcDependencyCondition)
        {
            Node = objNode;
            DependencyCondition = funcDependencyCondition;
            if (funcDependencyCondition == null)
                DependencyConditionAsync = null;
            else
                DependencyConditionAsync = (arg1, token) => DefaultDependencyConditionAsync(funcDependencyCondition, arg1, token);
        }

        public DependencyGraphNodeWithCondition(DependencyGraphNode<T, T2> objNode, Func<T2, bool> funcDependencyCondition, Func<T2, CancellationToken, Task<bool>> funcDependencyConditionAsync)
        {
            Node = objNode;
            DependencyCondition = funcDependencyCondition;
            DependencyConditionAsync = funcDependencyConditionAsync;
        }

        public DependencyGraphNode<T, T2> Node { get; }
        public Func<T2, bool> DependencyCondition { get; }
        public Func<T2, CancellationToken, Task<bool>> DependencyConditionAsync { get; }

        private static Task<bool> DefaultDependencyConditionAsync(Func<T2, bool> myDependencyCondition, T2 obj, CancellationToken token) =>
            token.IsCancellationRequested ? Task.FromCanceled<bool>(token) : Task.FromResult(myDependencyCondition(obj));

        public static bool operator ==(DependencyGraphNodeWithCondition<T, T2> objFirstEdge, DependencyGraphNodeWithCondition<T, T2> objSecondEdge)
        {
            return (objFirstEdge.Node == null && objSecondEdge.Node == null) || objFirstEdge.Node?.Equals(objSecondEdge.Node) == true;
        }

        public static bool operator !=(DependencyGraphNodeWithCondition<T, T2> objFirstEdge, DependencyGraphNodeWithCondition<T, T2> objSecondEdge)
        {
            return !((objFirstEdge.Node == null && objSecondEdge.Node == null) || objFirstEdge.Node?.Equals(objSecondEdge.Node) == true);
        }

        public bool Equals(DependencyGraphNodeWithCondition<T, T2> other)
        {
            return (Node == null && other.Node == null) || Node?.Equals(other.Node) == true;
        }

        public override bool Equals(object obj)
        {
            return obj is DependencyGraphNodeWithCondition<T, T2> objOtherEdge && Equals(objOtherEdge);
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
