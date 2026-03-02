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
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Chummer
{
    public readonly struct NuyenString : IComparable, IEquatable<NuyenString>, IComparable<NuyenString>
    {
        private static readonly ConcurrentDictionary<string, string> s_DicCachedStrings = new ConcurrentDictionary<string, string>();

        private static string GetNuyenSymbol(string strLanguage)
        {
            return s_DicCachedStrings.GetOrAdd(strLanguage, x => LanguageManager.GetString("String_NuyenSymbol", x));
        }

        private static Task<string> GetNuyenSymbolAsync(string strLanguage, CancellationToken token = default)
        {
            return s_DicCachedStrings.GetOrAddAsync(strLanguage, x => LanguageManager.GetStringAsync("String_NuyenSymbol", x, token: token), token);
        }

        public string BaseString { get; }
        public decimal Value { get; }
        public bool UseDecimal { get; }
        private readonly string _strToString;
        private readonly string _strNuyenSymbol;
        private readonly CultureInfo _objMyCulture;

        public NuyenString(string strNuyenString, CultureInfo objCulture = null, string strLanguage = "")
        {
            _objMyCulture = objCulture ?? GlobalSettings.CultureInfo;
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalSettings.Language;
            _strNuyenSymbol = GetNuyenSymbol(strLanguage);
            BaseString = strNuyenString.FastEscape(_strNuyenSymbol);
            _strToString = BaseString + _strNuyenSymbol;
            UseDecimal = decimal.TryParse(BaseString, NumberStyles.Any, _objMyCulture, out decimal decValue);
            Value = decValue;
        }

        public static async Task<NuyenString> GetNuyenStringAsync(string strNuyenString, CultureInfo objCulture = null, string strLanguage = "", CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalSettings.Language;
            string strNuyenSymbol = await GetNuyenSymbolAsync(strLanguage, token).ConfigureAwait(false);
            return new NuyenString(strNuyenString, strNuyenSymbol, objCulture);
        }

        private NuyenString(string strNuyenString, string strNuyenSymbol, CultureInfo objCulture = null)
        {
            _objMyCulture = objCulture ?? GlobalSettings.CultureInfo;
            _strNuyenSymbol = strNuyenSymbol;
            BaseString = strNuyenString.FastEscape(_strNuyenSymbol);
            _strToString = BaseString + _strNuyenSymbol;
            UseDecimal = decimal.TryParse(BaseString, NumberStyles.Any, _objMyCulture, out decimal decValue);
            Value = decValue;
        }

        public override string ToString()
        {
            return _strToString;
        }

        public string ToString(string format)
        {
            if (UseDecimal)
                return Value.ToString(format, _objMyCulture) + _strNuyenSymbol;
            return ToString();
        }

        public string ToString(IFormatProvider formatProvider)
        {
            if (UseDecimal)
                return Value.ToString(formatProvider) + _strNuyenSymbol;
            return ToString();
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (UseDecimal)
                return Value.ToString(format, formatProvider) + _strNuyenSymbol;
            return ToString();
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
                    : string.Compare(BaseString, other.BaseString, false, _objMyCulture);
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
