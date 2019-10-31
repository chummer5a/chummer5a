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
        public bool AddToParent { get; }
        public bool IncludedInParent { get; }
        public int Value { get; set; }

        public char Suffix { get; }

        public AvailabilityValue(int intValue, char chrSuffix, bool blnAddToParent, bool blnIncludedInParent = false)
        {
            Value = intValue;
            AddToParent = blnAddToParent;
            IncludedInParent = blnIncludedInParent;
            switch (chrSuffix)
            {
                case 'F':
                case 'R':
                    Suffix = chrSuffix;
                    break;
                default:
                    Suffix = 'Z';
                    break;
            }
        }

        public AvailabilityValue(int intRating, string strInput, int intBonus = 0, bool blnIncludedInParent = false)
        {
            string strAvailExpr = strInput;
            if (strAvailExpr.StartsWith("FixedValues("))
            {
                string[] strValues = strAvailExpr.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                strAvailExpr = strValues[(int)Math.Max(Math.Min(intRating, strValues.Length) - 1, 0)];
            }

            Suffix = strAvailExpr[strAvailExpr.Length - 1];
            AddToParent = strAvailExpr.StartsWith('+') || strAvailExpr.StartsWith('-');
            IncludedInParent = blnIncludedInParent;
            if (Suffix == 'F' || Suffix == 'R') strAvailExpr = strAvailExpr.Substring(0, strAvailExpr.Length - 1);
            object objProcess = CommonFunctions.EvaluateInvariantXPath(strAvailExpr.Replace("Rating", intRating.ToString(GlobalOptions.InvariantCultureInfo)), out bool blnIsSuccess);
            Value = blnIsSuccess ? Convert.ToInt32(objProcess) : 0;
            Value += intBonus;
            if (Value < 0)
                Value = 0;
        }

        public override string ToString()
        {
            return ToString(GlobalOptions.CultureInfo, GlobalOptions.Language);
        }

        public string ToString(CultureInfo objCulture, string strLanguage)
        {
            string strBaseAvail = Value.ToString(objCulture);
            if (AddToParent && Value >= 0)
                strBaseAvail = '+' + strBaseAvail;
            switch (Suffix)
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
            int intCompareResult = Value.CompareTo(objOther.Value);
            if (intCompareResult == 0)
            {
                intCompareResult = Suffix.CompareTo(objOther.Suffix);
                if (intCompareResult == 0)
                {
                    intCompareResult = AddToParent.CompareTo(objOther.AddToParent);
                }
            }
            return intCompareResult;
        }
    }
}
