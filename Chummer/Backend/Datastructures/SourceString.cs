using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Chummer
{
    class SourceString : IComparable
    {
        private readonly string _strCode;
        private readonly int _intPage;

        public string Code => _strCode;
        public int Page => _intPage;

        public SourceString(string strSourceString)
        {
            int intWhitespaceIndex = strSourceString.IndexOf(' ');
            if (intWhitespaceIndex != -1)
            {
                _strCode = strSourceString.Substring(0, intWhitespaceIndex);
                if (intWhitespaceIndex + 1 < strSourceString.Length)
                    int.TryParse(strSourceString.Substring(intWhitespaceIndex + 1), out _intPage);
            }
            else
                _strCode = strSourceString;
        }

        public SourceString(string strSource, string strPage)
        {
            _strCode = strSource;
            int.TryParse(strPage, out _intPage);
        }

        public SourceString(string strSource, int intPage)
        {
            _strCode = strSource;
            _intPage = intPage;
        }

        override public string ToString()
        {
            return ToString(GlobalOptions.Language);
        }

        public string ToString(string strLanguage)
        {
            return DisplayCode(strLanguage) + ' ' + _intPage.ToString();
        }

        public string DisplayCode(string strLanguage)
        {
            if (!string.IsNullOrWhiteSpace(_strCode))
            {
                XmlNode objXmlBook = XmlManager.Load("books.xml", strLanguage).SelectSingleNode("/chummer/books/book[code = \"" + _strCode + "\"]/altcode");
                return objXmlBook?.InnerText ?? _strCode;
            }
            return _strCode;
        }

        public int CompareTo(object obj)
        {
            return CompareTo((SourceString)obj);
        }

        public int CompareTo(SourceString strOther)
        {
            int intCompareResult = string.Compare(DisplayCode(GlobalOptions.Language), strOther.DisplayCode(GlobalOptions.Language), false, GlobalOptions.CultureInfo);
            if (intCompareResult == 0)
            {
                intCompareResult = _intPage.CompareTo(strOther.Page);
            }
            return intCompareResult;
        }
    }
}
