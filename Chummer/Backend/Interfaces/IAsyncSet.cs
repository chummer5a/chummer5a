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
using System.Threading;
using System.Threading.Tasks;

namespace Chummer
{
    public interface IAsyncSet<T> : IAsyncCollection<T>, ISet<T>
    {
        new Task<bool> AddAsync(T item, CancellationToken token = default);

        Task UnionWithAsync(IEnumerable<T> other, CancellationToken token = default);

        Task IntersectWithAsync(IEnumerable<T> other, CancellationToken token = default);

        Task ExceptWithAsync(IEnumerable<T> other, CancellationToken token = default);

        Task SymmetricExceptWithAsync(IEnumerable<T> other, CancellationToken token = default);

        Task<bool> IsSubsetOfAsync(IEnumerable<T> other, CancellationToken token = default);

        Task<bool> IsSupersetOfAsync(IEnumerable<T> other, CancellationToken token = default);

        Task<bool> IsProperSupersetOfAsync(IEnumerable<T> other, CancellationToken token = default);

        Task<bool> IsProperSubsetOfAsync(IEnumerable<T> other, CancellationToken token = default);

        Task<bool> OverlapsAsync(IEnumerable<T> other, CancellationToken token = default);

        Task<bool> SetEqualsAsync(IEnumerable<T> other, CancellationToken token = default);
    }
}
