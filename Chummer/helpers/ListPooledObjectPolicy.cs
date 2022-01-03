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
using System.Runtime.CompilerServices;
using Microsoft.Extensions.ObjectPool;

namespace Chummer
{
    /// <summary>
    /// A version of DefaultPooledObjectPolicy that is set up for lists so that they are cleared whenever they are returned to the pool.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ListPooledObjectPolicy<T> : IPooledObjectPolicy<List<T>>
    {
        private readonly DefaultPooledObjectPolicy<List<T>> _objBasePolicy = new DefaultPooledObjectPolicy<List<T>>();

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<T> Create()
        {
            return _objBasePolicy.Create();
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Return(List<T> obj)
        {
            obj.Clear();
            return _objBasePolicy.Return(obj);
        }
    }
}
