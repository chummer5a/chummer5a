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

        public override string ToString()
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
