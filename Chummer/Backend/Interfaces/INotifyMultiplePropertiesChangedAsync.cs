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
    public interface INotifyMultiplePropertiesChangedAsync : INotifyMultiplePropertiesChanged, INotifyPropertyChangedAsync
    {
        Task OnMultiplePropertiesChangedAsync(IReadOnlyCollection<string> lstPropertyNames, CancellationToken token = default);

        /// <summary>Occurs when one or more property values change.</summary>
        event MultiplePropertiesChangedAsyncEventHandler MultiplePropertiesChangedAsync;
    }

    public delegate Task MultiplePropertiesChangedAsyncEventHandler(object sender, MultiplePropertiesChangedEventArgs e, CancellationToken token = default);

    public static class NotifyMultiplePropertiesChangedAsyncExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task OnMultiplePropertyChangedAsync(this INotifyMultiplePropertiesChangedAsync objSubject,
            IEnumerable<string> lstPropertyNames, CancellationToken token = default)
        {
            return objSubject.OnMultiplePropertiesChangedAsync(lstPropertyNames.ToList(), token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task OnMultiplePropertyChangedAsync(this INotifyMultiplePropertiesChangedAsync objSubject, params string[] lstPropertyNames)
        {
            return objSubject.OnMultiplePropertiesChangedAsync(Array.AsReadOnly(lstPropertyNames));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task OnMultiplePropertyChangedAsync(this INotifyMultiplePropertiesChangedAsync objSubject, string strPropertyName)
        {
            using (TemporaryStringArray astrArg = new TemporaryStringArray(strPropertyName))
                await objSubject.OnMultiplePropertiesChangedAsync(astrArg).ConfigureAwait(false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task OnMultiplePropertyChangedAsync(this INotifyMultiplePropertiesChangedAsync objSubject, string strPropertyName1, string strPropertyName2)
        {
            using (TemporaryStringArray astrArg = new TemporaryStringArray(strPropertyName1, strPropertyName2))
                await objSubject.OnMultiplePropertiesChangedAsync(astrArg).ConfigureAwait(false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task OnMultiplePropertyChangedAsync(this INotifyMultiplePropertiesChangedAsync objSubject, string strPropertyName1, string strPropertyName2, string strPropertyName3)
        {
            using (TemporaryStringArray astrArg = new TemporaryStringArray(strPropertyName1, strPropertyName2, strPropertyName3))
                await objSubject.OnMultiplePropertiesChangedAsync(astrArg).ConfigureAwait(false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task OnMultiplePropertyChangedAsync(this INotifyMultiplePropertiesChangedAsync objSubject, string strPropertyName1, string strPropertyName2, string strPropertyName3, string strPropertyName4)
        {
            using (TemporaryStringArray astrArg = new TemporaryStringArray(strPropertyName1, strPropertyName2, strPropertyName3, strPropertyName4))
                await objSubject.OnMultiplePropertiesChangedAsync(astrArg).ConfigureAwait(false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task OnMultiplePropertyChangedAsync(this INotifyMultiplePropertiesChangedAsync objSubject,
            string strPropertyName1, string strPropertyName2, string strPropertyName3, string strPropertyName4, string strPropertyName5)
        {
            using (TemporaryStringArray astrArg = new TemporaryStringArray(strPropertyName1, strPropertyName2, strPropertyName3, strPropertyName4, strPropertyName5))
                await objSubject.OnMultiplePropertiesChangedAsync(astrArg).ConfigureAwait(false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task OnMultiplePropertyChangedAsync(this INotifyMultiplePropertiesChangedAsync objSubject,
            string strPropertyName1, string strPropertyName2, string strPropertyName3, string strPropertyName4, string strPropertyName5, string strPropertyName6)
        {
            using (TemporaryStringArray astrArg = new TemporaryStringArray(strPropertyName1, strPropertyName2, strPropertyName3, strPropertyName4, strPropertyName5, strPropertyName6))
                await objSubject.OnMultiplePropertiesChangedAsync(astrArg).ConfigureAwait(false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task OnMultiplePropertyChangedAsync(this INotifyMultiplePropertiesChangedAsync objSubject,
            string strPropertyName1, string strPropertyName2, string strPropertyName3, string strPropertyName4, string strPropertyName5, string strPropertyName6, string strPropertyName7)
        {
            using (TemporaryStringArray astrArg = new TemporaryStringArray(strPropertyName1, strPropertyName2, strPropertyName3, strPropertyName4, strPropertyName5, strPropertyName6, strPropertyName7))
                await objSubject.OnMultiplePropertiesChangedAsync(astrArg).ConfigureAwait(false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task OnMultiplePropertyChangedAsync(this INotifyMultiplePropertiesChangedAsync objSubject,
            string strPropertyName1, string strPropertyName2, string strPropertyName3, string strPropertyName4, string strPropertyName5, string strPropertyName6, string strPropertyName7, string strPropertyName8)
        {
            using (TemporaryStringArray astrArg = new TemporaryStringArray(strPropertyName1, strPropertyName2, strPropertyName3, strPropertyName4, strPropertyName5, strPropertyName6, strPropertyName7, strPropertyName8))
                await objSubject.OnMultiplePropertiesChangedAsync(astrArg).ConfigureAwait(false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task OnMultiplePropertyChangedAsync(this INotifyMultiplePropertiesChangedAsync objSubject, CancellationToken token,
            params string[] lstPropertyNames)
        {
            return objSubject.OnMultiplePropertiesChangedAsync(Array.AsReadOnly(lstPropertyNames), token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task OnMultiplePropertyChangedAsync(this INotifyMultiplePropertiesChangedAsync objSubject, CancellationToken token,
            string strPropertyName)
        {
            token.ThrowIfCancellationRequested();
            using (TemporaryStringArray astrArg = new TemporaryStringArray(strPropertyName))
                await objSubject.OnMultiplePropertiesChangedAsync(astrArg, token).ConfigureAwait(false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task OnMultiplePropertyChangedAsync(this INotifyMultiplePropertiesChangedAsync objSubject, CancellationToken token,
            string strPropertyName1, string strPropertyName2)
        {
            token.ThrowIfCancellationRequested();
            using (TemporaryStringArray astrArg = new TemporaryStringArray(strPropertyName1, strPropertyName2))
                await objSubject.OnMultiplePropertiesChangedAsync(astrArg, token).ConfigureAwait(false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task OnMultiplePropertyChangedAsync(this INotifyMultiplePropertiesChangedAsync objSubject, CancellationToken token,
            string strPropertyName1, string strPropertyName2, string strPropertyName3)
        {
            token.ThrowIfCancellationRequested();
            using (TemporaryStringArray astrArg = new TemporaryStringArray(strPropertyName1, strPropertyName2, strPropertyName3))
                await objSubject.OnMultiplePropertiesChangedAsync(astrArg, token).ConfigureAwait(false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task OnMultiplePropertyChangedAsync(this INotifyMultiplePropertiesChangedAsync objSubject, CancellationToken token,
            string strPropertyName1, string strPropertyName2, string strPropertyName3, string strPropertyName4)
        {
            token.ThrowIfCancellationRequested();
            using (TemporaryStringArray astrArg = new TemporaryStringArray(strPropertyName1, strPropertyName2, strPropertyName3, strPropertyName4))
                await objSubject.OnMultiplePropertiesChangedAsync(astrArg, token).ConfigureAwait(false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task OnMultiplePropertyChangedAsync(this INotifyMultiplePropertiesChangedAsync objSubject, CancellationToken token,
            string strPropertyName1, string strPropertyName2, string strPropertyName3, string strPropertyName4, string strPropertyName5)
        {
            token.ThrowIfCancellationRequested();
            using (TemporaryStringArray astrArg = new TemporaryStringArray(strPropertyName1, strPropertyName2, strPropertyName3, strPropertyName4, strPropertyName5))
                await objSubject.OnMultiplePropertiesChangedAsync(astrArg, token).ConfigureAwait(false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task OnMultiplePropertyChangedAsync(this INotifyMultiplePropertiesChangedAsync objSubject, CancellationToken token,
            string strPropertyName1, string strPropertyName2, string strPropertyName3, string strPropertyName4, string strPropertyName5, string strPropertyName6)
        {
            token.ThrowIfCancellationRequested();
            using (TemporaryStringArray astrArg = new TemporaryStringArray(strPropertyName1, strPropertyName2, strPropertyName3, strPropertyName4, strPropertyName5, strPropertyName6))
                await objSubject.OnMultiplePropertiesChangedAsync(astrArg, token).ConfigureAwait(false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task OnMultiplePropertyChangedAsync(this INotifyMultiplePropertiesChangedAsync objSubject, CancellationToken token,
            string strPropertyName1, string strPropertyName2, string strPropertyName3, string strPropertyName4, string strPropertyName5, string strPropertyName6, string strPropertyName7)
        {
            token.ThrowIfCancellationRequested();
            using (TemporaryStringArray astrArg = new TemporaryStringArray(strPropertyName1, strPropertyName2, strPropertyName3, strPropertyName4, strPropertyName5, strPropertyName6, strPropertyName7))
                await objSubject.OnMultiplePropertiesChangedAsync(astrArg, token).ConfigureAwait(false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task OnMultiplePropertyChangedAsync(this INotifyMultiplePropertiesChangedAsync objSubject, CancellationToken token,
            string strPropertyName1, string strPropertyName2, string strPropertyName3, string strPropertyName4, string strPropertyName5, string strPropertyName6, string strPropertyName7, string strPropertyName8)
        {
            token.ThrowIfCancellationRequested();
            using (TemporaryStringArray astrArg = new TemporaryStringArray(strPropertyName1, strPropertyName2, strPropertyName3, strPropertyName4, strPropertyName5, strPropertyName6, strPropertyName7, strPropertyName8))
                await objSubject.OnMultiplePropertiesChangedAsync(astrArg, token).ConfigureAwait(false);
        }
    }
}
