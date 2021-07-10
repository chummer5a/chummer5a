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
using System.Collections.Concurrent;
using System.Globalization;
using System.Windows.Forms;

namespace Chummer
{
    public readonly struct SourceString : IComparable, IEquatable<SourceString>, IComparable<SourceString>
    {
        private static readonly ConcurrentDictionary<string, Tuple<string, string>> _dicCachedStrings = new ConcurrentDictionary<string, Tuple<string, string>>();
        private readonly int _intHashCode;

        public SourceString(string strSourceString, string strLanguage = "", CultureInfo objCultureInfo = null, Character objCharacter = null)
        {
            Language = !string.IsNullOrEmpty(strLanguage) ? strLanguage : GlobalOptions.Language;
            CultureInfo = objCultureInfo ?? GlobalOptions.CultureInfo;
            string strCode = strSourceString ?? string.Empty;
            Page = 0;
            int intWhitespaceIndex = strCode.IndexOf(' ');
            if (intWhitespaceIndex != -1)
            {
                strCode = strCode.Substring(0, intWhitespaceIndex);
                if (intWhitespaceIndex + 1 < strCode.Length)
                {
                    int.TryParse(strCode.Substring(intWhitespaceIndex + 1), NumberStyles.Integer, GlobalOptions.InvariantCultureInfo, out int intPage);
                    Page = intPage;
                }
            }

            Code = CommonFunctions.LanguageBookShort(strCode, Language, objCharacter);
            _intHashCode = (Language, CultureInfo, Code, Page).GetHashCode();
            if (!_dicCachedStrings.ContainsKey(Language))
                _dicCachedStrings.TryAdd(Language, new Tuple<string, string>(
                        LanguageManager.GetString("String_Space", Language),
                        LanguageManager.GetString("String_Page", Language)));
            string strSpace = _dicCachedStrings[Language].Item1;
            LanguageBookTooltip = CommonFunctions.LanguageBookLong(strCode, Language, objCharacter) + strSpace + _dicCachedStrings[Language].Item2 + strSpace + Page.ToString(CultureInfo);
        }

        public SourceString(string strSource, string strPage, string strLanguage, CultureInfo objCultureInfo = null, Character objCharacter = null)
        {
            Language = !string.IsNullOrEmpty(strLanguage) ? strLanguage : GlobalOptions.Language;
            CultureInfo = objCultureInfo ?? GlobalOptions.CultureInfo;
            int.TryParse(strPage, NumberStyles.Integer, GlobalOptions.InvariantCultureInfo, out int intPage);
            Page = intPage;

            Code = CommonFunctions.LanguageBookShort(strSource, Language, objCharacter);
            _intHashCode = (Language, CultureInfo, Code, Page).GetHashCode();
            if (!_dicCachedStrings.ContainsKey(Language))
                _dicCachedStrings.TryAdd(Language, new Tuple<string, string>(
                    LanguageManager.GetString("String_Space", Language),
                    LanguageManager.GetString("String_Page", Language)));
            string strSpace = _dicCachedStrings[Language].Item1;
            LanguageBookTooltip = CommonFunctions.LanguageBookLong(strSource, Language, objCharacter) + strSpace + _dicCachedStrings[Language].Item2 + strSpace + Page.ToString(CultureInfo);
        }

        public SourceString(string strSource, int intPage, string strLanguage = "", CultureInfo objCultureInfo = null, Character objCharacter = null)
        {
            Language = !string.IsNullOrEmpty(strLanguage) ? strLanguage : GlobalOptions.Language;
            CultureInfo = objCultureInfo ?? GlobalOptions.CultureInfo;
            Page = intPage;

            Code = CommonFunctions.LanguageBookShort(strSource, Language, objCharacter);
            _intHashCode = (Language, CultureInfo, Code, Page).GetHashCode();
            if (!_dicCachedStrings.ContainsKey(Language))
                _dicCachedStrings.TryAdd(Language, new Tuple<string, string>(
                    LanguageManager.GetString("String_Space", Language),
                    LanguageManager.GetString("String_Page", Language)));
            string strSpace = _dicCachedStrings[Language].Item1;
            LanguageBookTooltip = CommonFunctions.LanguageBookLong(strSource, Language, objCharacter) + strSpace + _dicCachedStrings[Language].Item2 + strSpace + Page.ToString(CultureInfo);
        }

        public override string ToString()
        {
            return Code + _dicCachedStrings[Language].Item1 + Page.ToString(CultureInfo);
        }

        /// <summary>
        /// Language code originally used to construct the source info (alters book code, possibly alters page numbers)
        /// </summary>
        public string Language { get; }

        /// <summary>
        /// Culture info originally used to construct the source info (alters book code, possibly alters page numbers)
        /// </summary>
        public CultureInfo CultureInfo { get; }

        /// <summary>
        /// Book code of the source info, possibly modified from English by the language of the source info
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Page of the source info, possibly modified from English by the language of the source info
        /// </summary>
        public int Page { get; }

        /// <summary>
        /// Provides the long-form name of the object's sourcebook and page reference.
        /// </summary>
        public string LanguageBookTooltip { get; }

        public int CompareTo(object obj)
        {
            return CompareTo((SourceString)obj);
        }

        public int CompareTo(SourceString other)
        {
            int intCompareResult = string.Compare(Language, other.Language, false, GlobalOptions.CultureInfo);
            if (intCompareResult == 0)
            {
                intCompareResult = string.Compare(Code, other.Code, false, GlobalOptions.CultureInfo);
                if (intCompareResult == 0)
                {
                    intCompareResult = Page.CompareTo(other.Page);
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

        public bool Equals(SourceString other)
        {
            return Language == other.Language && Code == other.Code && Page == other.Page;
        }

        public override bool Equals(object obj)
        {
            if (obj is SourceString objOther)
                return Equals(objOther);
            return false;
        }

        public override int GetHashCode()
        {
            return _intHashCode;
        }

        public static bool operator ==(SourceString left, SourceString right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SourceString left, SourceString right)
        {
            return !(left == right);
        }

        public static bool operator <(SourceString left, SourceString right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(SourceString left, SourceString right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(SourceString left, SourceString right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(SourceString left, SourceString right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}