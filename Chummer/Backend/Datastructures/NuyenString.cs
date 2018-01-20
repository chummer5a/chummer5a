using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        override public string ToString()
        {
            return _strBaseString + '¥';
        }

        public string ToString(string format)
        {
            if (_blnUseDecimal)
                return _decValue.ToString(format) + '¥';
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
                else
                    return -1;
            }
            else if (strOther.UseDecimal)
                return 1;
            else
            {
                return string.Compare(_strBaseString, strOther.BaseString, false, GlobalOptions.CultureInfo);
            }
        }
    }
}
