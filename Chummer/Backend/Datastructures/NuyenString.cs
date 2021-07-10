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
    public readonly struct NuyenString : IComparable, IEquatable<NuyenString>, IComparable<NuyenString>
    {
        public string BaseString { get; }
        public decimal Value { get; }
        public bool UseDecimal { get; }

        public NuyenString(string strNuyenString)
        {
            BaseString = strNuyenString.FastEscape('¥');
            UseDecimal = decimal.TryParse(BaseString, out decimal decValue);
            Value = decValue;
        }

        public override string ToString()
        {
            return BaseString + '¥';
        }

        public string ToString(string format)
        {
            if (UseDecimal)
                return Value.ToString(format, GlobalOptions.InvariantCultureInfo) + '¥';
            return BaseString + '¥';
        }

        public string ToString(IFormatProvider formatProvider)
        {
            if (UseDecimal)
                return Value.ToString(formatProvider) + '¥';
            return BaseString + '¥';
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (UseDecimal)
                return Value.ToString(format, formatProvider) + '¥';
            return BaseString + '¥';
        }

        public bool Equals(NuyenString other)
        {
            if (UseDecimal != other.UseDecimal)
                return false;
            return UseDecimal ? Value == other.Value : BaseString.Equals(other.BaseString, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is NuyenString other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (BaseString, Value, UseDecimal).GetHashCode();
        }

        public int CompareTo(object obj)
        {
            return CompareTo((NuyenString)obj);
        }

        public int CompareTo(NuyenString other)
        {
            if (!UseDecimal)
                return other.UseDecimal
                    ? 1
                    : string.Compare(BaseString, other.BaseString, false, GlobalOptions.CultureInfo);
            if (other.UseDecimal)
                return Value.CompareTo(other.Value);
            return -1;
        }

        public static bool operator ==(NuyenString left, NuyenString right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(NuyenString left, NuyenString right)
        {
            return !(left == right);
        }

        public static bool operator <(NuyenString left, NuyenString right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(NuyenString left, NuyenString right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(NuyenString left, NuyenString right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(NuyenString left, NuyenString right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}