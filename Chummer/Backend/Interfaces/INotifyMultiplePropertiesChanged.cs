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
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Chummer
{
    public interface INotifyMultiplePropertiesChanged : INotifyPropertyChanged
    {
        void OnMultiplePropertiesChanged(IReadOnlyCollection<string> lstPropertyNames);

        /// <summary>Occurs when one or more property values change.</summary>
        event MultiplePropertiesChangedEventHandler MultiplePropertiesChanged;
    }

    public delegate void MultiplePropertiesChangedEventHandler(object sender, MultiplePropertiesChangedEventArgs e);

    public static class NotifyMultiplePropertiesChangedExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnMultiplePropertyChanged(this INotifyMultiplePropertiesChanged objSubject,
                                                     IEnumerable<string> lstPropertyNames)
        {
            objSubject.OnMultiplePropertiesChanged(lstPropertyNames.ToList());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnMultiplePropertyChanged(this INotifyMultiplePropertiesChanged objSubject,
                                                     params string[] lstPropertyNames)
        {
            objSubject.OnMultiplePropertiesChanged(Array.AsReadOnly(lstPropertyNames));
        }
    }
}
