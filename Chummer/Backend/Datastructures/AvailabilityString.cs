using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chummer
{
    public struct AvailabilityString : IComparable
    {
        private readonly string _strPrefix;
        private readonly char _chrSuffix;

        public string Prefix => _strPrefix;
        public char Suffix => _chrSuffix;

        public AvailabilityString(string strAvailabilityString)
        {
            int intStringLength = strAvailabilityString.Length;
            if (intStringLength > 0)
            {
                _chrSuffix = strAvailabilityString[intStringLength - 1];
                if (_chrSuffix == 'F' || _chrSuffix == 'R')
                    _strPrefix = strAvailabilityString.Substring(0, intStringLength - 1);
                else
                {
                    _strPrefix = strAvailabilityString;
                    _chrSuffix = 'Z';
                }
            }
            else
            {
                _strPrefix = string.Empty;
                _chrSuffix = 'Z';
            }
        }

        override public string ToString()
        {
            return ToString(GlobalOptions.Language);
        }

        public string ToString(string strLanguage)
        {
            if (_chrSuffix == 'F')
                return _strPrefix + LanguageManager.GetString("String_AvailForbidden", strLanguage);
            if (_chrSuffix == 'R')
                return _strPrefix + LanguageManager.GetString("String_AvailRestricted", strLanguage);
            return _strPrefix;
        }

        public int CompareTo(object obj)
        {
            return CompareTo((AvailabilityString)obj);
        }

        public int CompareTo(AvailabilityString strOther)
        {
            int intCompareResult = 0;
            if (int.TryParse(_strPrefix, out int intXPrefix) && int.TryParse(strOther.Prefix, out int intYPrefix))
            {
                intCompareResult = intXPrefix.CompareTo(intYPrefix);
            }
            else
            {
                intCompareResult = string.Compare(_strPrefix, strOther.Prefix, false, GlobalOptions.CultureInfo);
            }
            if (intCompareResult == 0)
            {
                intCompareResult = -_chrSuffix.CompareTo(strOther.Suffix);
            }
            return intCompareResult;
        }
    }
}
