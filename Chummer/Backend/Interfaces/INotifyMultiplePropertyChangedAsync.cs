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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Chummer
{
    public interface INotifyMultiplePropertyChangedAsync : INotifyMultiplePropertyChanged, INotifyPropertyChangedAsync
    {
        Task OnMultiplePropertyChangedAsync(IReadOnlyCollection<string> lstPropertyNames, CancellationToken token = default);
    }

    public static class NotifyMultiplePropertyChangedAsyncExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task OnMultiplePropertyChangedAsync(this INotifyMultiplePropertyChangedAsync objSubject,
            IEnumerable<string> lstPropertyNames, CancellationToken token = default)
        {
            return objSubject.OnMultiplePropertyChangedAsync(lstPropertyNames.ToList(), token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task OnMultiplePropertyChangedAsync(this INotifyMultiplePropertyChangedAsync objSubject, CancellationToken token,
            params string[] lstPropertyNames)
        {
            return objSubject.OnMultiplePropertyChangedAsync(Array.AsReadOnly(lstPropertyNames), token);
        }
    }
}
