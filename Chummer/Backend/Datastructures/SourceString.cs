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
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    public class SourceString : IComparable
    {
        private readonly string _strCode;
        private readonly int _intPage;
        #region Cached values for LanguageBookTooltip
        private static string _cachedLanguage = GlobalOptions.Language;
        private static string _cachedSpace = LanguageManager.GetString("String_Space", GlobalOptions.Language);
        private static string _cachedPage = LanguageManager.GetString("String_Page", GlobalOptions.Language);
        private string _cachedTooltip = string.Empty;
        #endregion

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
            return DisplayCode(strLanguage) + LanguageManager.GetString("String_Space", strLanguage) + Page;
        }

        /// <summary>
        /// Provides the long-form name of the object's sourcebook and page reference. 
        /// </summary>
        public string LanguageBookTooltip
        {
            get
            {
                //Nothing's changed, so return the cached string. 
                if (_cachedLanguage == GlobalOptions.Language && !string.IsNullOrWhiteSpace(_cachedTooltip))
                    return _cachedTooltip;
                //Cached language change, so refresh the properties
                if (_cachedLanguage != GlobalOptions.Language)
                {
                    _cachedLanguage = GlobalOptions.Language;
                    _cachedSpace = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                    _cachedPage = LanguageManager.GetString("String_Page", GlobalOptions.Language);
                }
                _cachedTooltip = CommonFunctions.LanguageBookLong(_strCode, GlobalOptions.Language) +
                                 _cachedSpace + _cachedPage + _cachedSpace + _intPage;
                return _cachedTooltip;
            }
        }

        public string DisplayCode(string strLanguage)
        {
            if (string.IsNullOrWhiteSpace(Code)) return Code;
            XmlNode objXmlBook = XmlManager.Load("books.xml", strLanguage).SelectSingleNode("/chummer/books/book[code = \"" + Code + "\"]/altcode");
            return objXmlBook?.InnerText ?? Code;
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

        /// <summary>
        /// Set the Text and ToolTips for the selected control. 
        /// </summary>
        /// <param name="source"></param>
        public void SetControl(Control source)
        {
            source.Text = ToString();
            source.SetToolTip(LanguageBookTooltip);
        }
    }
}
