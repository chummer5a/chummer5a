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
using System.Collections.ObjectModel;
using System.Linq;

namespace Chummer
{
    public readonly struct MultiplePropertiesChangedEventArgs : IEquatable<MultiplePropertiesChangedEventArgs>
    {
        public MultiplePropertiesChangedEventArgs(params string[] astrPropertyNames) =>
            PropertyNames = astrPropertyNames.AsReadOnly();

        /// <summary>
        /// NOTE: This will regularly only create a wrapper around the provided arguments, so be careful when using disposable collections for the argument!
        /// </summary>
        public MultiplePropertiesChangedEventArgs(IEnumerable<string> astrPropertyNames) =>
            PropertyNames = astrPropertyNames.AsReadOnly();

        public ReadOnlyCollection<string> PropertyNames { get; }

        public bool Equals(MultiplePropertiesChangedEventArgs other)
        {
            return PropertyNames.CollectionEqual(other.PropertyNames);
        }

        public override bool Equals(object obj)
        {
            return obj is MultiplePropertiesChangedEventArgs objCasted && Equals(objCasted);
        }
        public static bool operator ==(MultiplePropertiesChangedEventArgs left, MultiplePropertiesChangedEventArgs right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MultiplePropertiesChangedEventArgs left, MultiplePropertiesChangedEventArgs right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return PropertyNames.GetEnsembleHashCode();
        }
    }
}
