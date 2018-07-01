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
using System.Globalization;

namespace Chummer
{
    public struct AvailabilityValue : IComparable
    {
        private readonly bool _blnAddToParent;
        private readonly int _intValue;
        private readonly char _chrSuffix;

        public bool AddToParent => _blnAddToParent;
        public int Value => _intValue;
        public char Suffix => _chrSuffix;

        public AvailabilityValue(int intValue, char chrSuffix, bool blnAddToParent)
        {
            _intValue = intValue;
            _blnAddToParent = blnAddToParent;
            switch (chrSuffix)
            {
                case 'F':
                case 'R':
                    _chrSuffix = chrSuffix;
                    break;
                default:
                    _chrSuffix = 'Z';
                    break;
            }
        }

        public override string ToString()
        {
            return ToString(GlobalOptions.CultureInfo, GlobalOptions.Language);
        }

        public string ToString(CultureInfo objCulture, string strLanguage)
        {
            string strBaseAvail = _intValue.ToString(objCulture);
            if (_blnAddToParent && _intValue >= 0)
                strBaseAvail = '+' + strBaseAvail;
            switch (_chrSuffix)
            {
                case 'F':
                    return strBaseAvail + LanguageManager.GetString("String_AvailForbidden", strLanguage);
                case 'R':
                    return strBaseAvail + LanguageManager.GetString("String_AvailRestricted", strLanguage);
            }
            return strBaseAvail;
        }

        public int CompareTo(object obj)
        {
            return CompareTo((AvailabilityValue)obj);
        }

        public int CompareTo(AvailabilityValue objOther)
        {
            int intCompareResult = _intValue.CompareTo(objOther.Value);
            if (intCompareResult == 0)
            {
                intCompareResult = _chrSuffix.CompareTo(objOther.Suffix);
                if (intCompareResult == 0)
                {
                    intCompareResult = _blnAddToParent.CompareTo(objOther.AddToParent);
                }
            }
            return intCompareResult;
        }
    }
}
