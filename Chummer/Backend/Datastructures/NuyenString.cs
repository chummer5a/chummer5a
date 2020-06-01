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
    class NuyenString : IComparable
    {
        private readonly string _strBaseString;
        private readonly decimal _decValue;
        private readonly bool _blnUseDecimal;

        public string BaseString => _strBaseString;
        public decimal Value => _decValue;
        public bool UseDecimal => _blnUseDecimal;

        public NuyenString(string strNuyenString)
        {
            _strBaseString = strNuyenString.FastEscape('¥');
            _blnUseDecimal = decimal.TryParse(_strBaseString, out _decValue);
        }

        public override string ToString()
        {
            return _strBaseString + '¥';
        }

        public string ToString(string format)
        {
            if (_blnUseDecimal)
                return _decValue.ToString(format, GlobalOptions.InvariantCultureInfo) + '¥';
            return _strBaseString + '¥';
        }

        public string ToString(IFormatProvider formatProvider)
        {
            if (_blnUseDecimal)
                return _decValue.ToString(formatProvider) + '¥';
            return _strBaseString + '¥';
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (_blnUseDecimal)
                return _decValue.ToString(format, formatProvider) + '¥';
            return _strBaseString + '¥';
        }

        public int CompareTo(object obj)
        {
            return CompareTo((NuyenString)obj);
        }

        public int CompareTo(NuyenString strOther)
        {
            if (_blnUseDecimal)
            {
                if (strOther.UseDecimal)
                    return _decValue.CompareTo(strOther.Value);
                return -1;
            }

            if (strOther.UseDecimal)
                return 1;
            return string.Compare(_strBaseString, strOther.BaseString, false, GlobalOptions.CultureInfo);
        }
    }
}
