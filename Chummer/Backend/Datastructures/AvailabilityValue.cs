using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        override public string ToString()
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
