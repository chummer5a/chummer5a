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
        private int _intValue;
        private readonly char _chrSuffix;

        public bool AddToParent => _blnAddToParent;
        public int Value
        {
            get => _intValue;
            set => _intValue = value;
        }

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

        public AvailabilityValue(int intRating, string strInput, int intBonus = 0)
        {
            string strAvailExpr = strInput;
            if (strAvailExpr.StartsWith("FixedValues("))
            {
                string[] strValues = strAvailExpr.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                strAvailExpr = strValues[(int)Math.Max(Math.Min(intRating, strValues.Length) - 1, 0)];
            }

            _chrSuffix = strAvailExpr[strAvailExpr.Length - 1];
            _blnAddToParent = strAvailExpr.StartsWith('+') || strAvailExpr.StartsWith('-');
            if (_chrSuffix == 'F' || _chrSuffix == 'R') strAvailExpr = strAvailExpr.Substring(0, strAvailExpr.Length - 1);
            object objProcess = CommonFunctions.EvaluateInvariantXPath(strAvailExpr.Replace("Rating", intRating.ToString(GlobalOptions.InvariantCultureInfo)), out bool blnIsSuccess);
            _intValue = blnIsSuccess ? Convert.ToInt32(objProcess) : 0;
            _intValue += intBonus;
            if (_intValue < 0)
                _intValue = 0;
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
