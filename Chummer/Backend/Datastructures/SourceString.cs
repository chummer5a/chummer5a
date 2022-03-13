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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chummer
{
    public readonly struct SourceString : IComparable, IEquatable<SourceString>, IComparable<SourceString>
    {
        public static readonly SourceString Blank = GetSourceString(string.Empty, 0, GlobalSettings.DefaultLanguage, GlobalSettings.InvariantCultureInfo);

        private static readonly LockingDictionary<string, Tuple<string, string>> s_DicCachedStrings = new LockingDictionary<string, Tuple<string, string>>();
        private readonly int _intHashCode;

        public static SourceString GetSourceString(string strSourceString, string strLanguage = "",
                                                   CultureInfo objCultureInfo = null, Character objCharacter = null)
        {
            string strCode = strSourceString ?? string.Empty;
            int intPage = 0;
            int intWhitespaceIndex = strCode.IndexOf(' ');
            if (intWhitespaceIndex != -1)
            {
                strCode = strCode.Substring(0, intWhitespaceIndex);
                if (intWhitespaceIndex + 1 < strCode.Length)
                {
                    int.TryParse(strCode.Substring(intWhitespaceIndex + 1), NumberStyles.Integer, GlobalSettings.InvariantCultureInfo, out intPage);
                }
            }
            return GetSourceString(strCode, intPage, strLanguage, objCultureInfo, objCharacter);
        }

        public static ValueTask<SourceString> GetSourceStringAsync(string strSourceString, string strLanguage = "",
                                                                         CultureInfo objCultureInfo = null, Character objCharacter = null)
        {
            string strCode = strSourceString ?? string.Empty;
            int intPage = 0;
            int intWhitespaceIndex = strCode.IndexOf(' ');
            if (intWhitespaceIndex != -1)
            {
                strCode = strCode.Substring(0, intWhitespaceIndex);
                if (intWhitespaceIndex + 1 < strCode.Length)
                {
                    int.TryParse(strCode.Substring(intWhitespaceIndex + 1), NumberStyles.Integer, GlobalSettings.InvariantCultureInfo, out intPage);
                }
            }
            return GetSourceStringAsync(strCode, intPage, strLanguage, objCultureInfo, objCharacter);
        }

        public static SourceString GetSourceString(string strSource, string strPage, string strLanguage = "",
                                                   CultureInfo objCultureInfo = null, Character objCharacter = null)
        {
            int.TryParse(strPage, NumberStyles.Integer, GlobalSettings.InvariantCultureInfo, out int intPage);
            return GetSourceString(strSource, intPage, strLanguage, objCultureInfo, objCharacter);
        }

        public static ValueTask<SourceString> GetSourceStringAsync(string strSource, string strPage, string strLanguage = "",
                                                                   CultureInfo objCultureInfo = null, Character objCharacter = null)
        {
            int.TryParse(strPage, NumberStyles.Integer, GlobalSettings.InvariantCultureInfo, out int intPage);
            return GetSourceStringAsync(strSource, intPage, strLanguage, objCultureInfo, objCharacter);
        }

        public static SourceString GetSourceString(string strSource, int intPage, string strLanguage = "",
                                                   CultureInfo objCultureInfo = null, Character objCharacter = null)
        {
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalSettings.Language;
            return new SourceString(CommonFunctions.LanguageBookShort(strSource, strLanguage, objCharacter),
                                    CommonFunctions.LanguageBookLong(strSource, strLanguage, objCharacter), intPage,
                                    strLanguage, objCultureInfo);
        }

        public static async ValueTask<SourceString> GetSourceStringAsync(string strSource, int intPage, string strLanguage = "",
                                                                         CultureInfo objCultureInfo = null, Character objCharacter = null)
        {
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalSettings.Language;
            return new SourceString(await CommonFunctions.LanguageBookShortAsync(strSource, strLanguage, objCharacter),
                                    await CommonFunctions.LanguageBookLongAsync(strSource, strLanguage, objCharacter), intPage,
                                    strLanguage, objCultureInfo);
        }

        private SourceString(string strBookCodeShort, string strBookCodeLong, int intPage, string strLanguage, CultureInfo objCultureInfo)
        {
            Language = !string.IsNullOrEmpty(strLanguage) ? strLanguage : GlobalSettings.Language;
            CultureInfo = objCultureInfo ?? GlobalSettings.CultureInfo;
            Page = intPage;

            Code = strBookCodeShort;
            _intHashCode = (Language, CultureInfo, Code, Page).GetHashCode();
            if (!s_DicCachedStrings.ContainsKey(Language))
                s_DicCachedStrings.TryAdd(Language, new Tuple<string, string>(
                    LanguageManager.GetString("String_Space", Language),
                    LanguageManager.GetString("String_Page", Language)));
            string strSpace = s_DicCachedStrings[Language].Item1;
            LanguageBookTooltip = strBookCodeLong + strSpace + s_DicCachedStrings[Language].Item2 + strSpace + Page.ToString(CultureInfo);
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Code)
                ? string.Empty
                : Code + s_DicCachedStrings[Language].Item1 + Page.ToString(CultureInfo);
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
            int intCompareResult = string.Compare(Language, other.Language, false, GlobalSettings.CultureInfo);
            if (intCompareResult == 0)
            {
                intCompareResult = string.Compare(Code, other.Code, false, GlobalSettings.CultureInfo);
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
            string strText = ToString();
            source.DoThreadSafe(x => x.Text = strText);
            source.SetToolTip(LanguageBookTooltip);
        }

        /// <summary>
        /// Set the Text and ToolTips for the selected control.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="token"></param>
        public Task SetControlAsync(Control source, CancellationToken token = default)
        {
            if (source == null)
                return Task.CompletedTask;
            string strText = ToString();
            return Task.WhenAll(source.DoThreadSafeAsync(x => x.Text = strText, token),
                                source.SetToolTipAsync(LanguageBookTooltip, token));
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
