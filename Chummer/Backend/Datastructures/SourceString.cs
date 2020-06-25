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
using System.Windows.Forms;

namespace Chummer
{
    public class SourceString : IComparable, IEquatable<SourceString>
    {
        private readonly int _intPage;
        private readonly string _strCachedSpace;

        public SourceString(string strSourceString, string strLanguage = "", Character objCharacter = null)
        {
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalOptions.Language;
            Language = strLanguage;
            string strCode = strSourceString ?? string.Empty;
            int intWhitespaceIndex = strCode.IndexOf(' ');
            if (intWhitespaceIndex != -1)
            {
                strCode = strCode.Substring(0, intWhitespaceIndex);
                if (intWhitespaceIndex + 1 < strCode.Length)
                    int.TryParse(strCode.Substring(intWhitespaceIndex + 1), NumberStyles.Integer, GlobalOptions.InvariantCultureInfo, out _intPage);
            }

            Code = CommonFunctions.LanguageBookShort(strCode, Language, objCharacter);
            _strCachedSpace = LanguageManager.GetString("String_Space", strLanguage);
            LanguageBookTooltip = CommonFunctions.LanguageBookLong(strCode, Language, objCharacter) +
                                _strCachedSpace + LanguageManager.GetString("String_Page", strLanguage) + _strCachedSpace + _intPage;
        }

        // Cannot have strLanguage default to empty string here, otherwise we would have ambiguous constructors
        public SourceString(string strSource, string strPage, string strLanguage, Character objCharacter = null)
        {
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalOptions.Language;
            Language = strLanguage;
            int.TryParse(strPage, NumberStyles.Integer, GlobalOptions.InvariantCultureInfo, out _intPage);

            Code = CommonFunctions.LanguageBookShort(strSource, Language, objCharacter);
            _strCachedSpace = LanguageManager.GetString("String_Space", strLanguage);
            LanguageBookTooltip = CommonFunctions.LanguageBookLong(strSource, Language, objCharacter) +
                                _strCachedSpace + LanguageManager.GetString("String_Page", strLanguage) + _strCachedSpace + _intPage;
        }

        public SourceString(string strSource, int intPage, string strLanguage = "", Character objCharacter = null)
        {
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalOptions.Language;
            Language = strLanguage;
            _intPage = intPage;

            Code = CommonFunctions.LanguageBookShort(strSource, Language, objCharacter);
            _strCachedSpace = LanguageManager.GetString("String_Space", strLanguage);
            LanguageBookTooltip = CommonFunctions.LanguageBookLong(strSource, Language, objCharacter) +
                                _strCachedSpace + LanguageManager.GetString("String_Page", strLanguage) + _strCachedSpace + _intPage;
        }

        public override string ToString()
        {
            return Code + _strCachedSpace + Page;
        }

        /// <summary>
        /// Language code originally used to construct the source info (alters book code, possibly alters page numbers)
        /// </summary>
        public string Language { get; }

        /// <summary>
        /// Book code of the source info, possibly modified from English by the language of the source info
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Page of the source info, possibly modified from English by the language of the source info
        /// </summary>
        public int Page => _intPage;

        /// <summary>
        /// Provides the long-form name of the object's sourcebook and page reference.
        /// </summary>
        public string LanguageBookTooltip { get; }

        public int CompareTo(object obj)
        {
            return CompareTo((SourceString)obj);
        }

        public int CompareTo(SourceString strOther)
        {
            if (strOther == null)
                return 1;
            int intCompareResult = string.Compare(Language, strOther.Language, false, GlobalOptions.CultureInfo);
            if (intCompareResult == 0)
            {
                intCompareResult = string.Compare(Code, strOther.Code, false, GlobalOptions.CultureInfo);
                if (intCompareResult == 0)
                {
                    intCompareResult = _intPage.CompareTo(strOther.Page);
                }
            }
            return intCompareResult;
        }

        /// <summary>
        /// Set the Text and ToolTips for the selected control.
        /// </summary>
        /// <param name="source"></param>
        public void SetControl(Control source)
        {
            if (source == null)
                return;
            source.Text = ToString();
            source.SetToolTip(LanguageBookTooltip);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            if (obj is SourceString objOther)
                return Equals(objOther);
            return false;
        }

        public override int GetHashCode()
        {
            return new {Language, Code, Page}.GetHashCode();
        }

        public bool Equals(SourceString other)
        {
            return other != null && Language == other.Language && Code == other.Code && Page == other.Page;
        }

        public static bool operator ==(SourceString left, SourceString right)
        {
            if (left is null)
            {
                return right is null;
            }

            return left.Equals(right);
        }

        public static bool operator !=(SourceString left, SourceString right)
        {
            return !(left == right);
        }

        public static bool operator <(SourceString left, SourceString right)
        {
            return left is null ? !(right is null) : left.CompareTo(right) < 0;
        }

        public static bool operator <=(SourceString left, SourceString right)
        {
            return left is null || left.CompareTo(right) <= 0;
        }

        public static bool operator >(SourceString left, SourceString right)
        {
            return !(left is null) && left.CompareTo(right) > 0;
        }

        public static bool operator >=(SourceString left, SourceString right)
        {
            return left is null ? right is null : left.CompareTo(right) >= 0;
        }
    }
}
