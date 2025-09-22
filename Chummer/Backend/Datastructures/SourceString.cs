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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.Threading;

namespace Chummer
{
    public readonly struct SourceString : IComparable, IEquatable<SourceString>, IComparable<SourceString>
    {
        private static readonly ConcurrentDictionary<string, ValueTuple<string, string>> s_DicCachedStrings = new ConcurrentDictionary<string, ValueTuple<string, string>>();

        private static ValueTuple<string, string> GetSpaceAndPageStrings(string strLanguage)
        {
            return s_DicCachedStrings.GetOrAdd(
                strLanguage,
                x => new ValueTuple<string, string>(LanguageManager.GetString("String_Space", x),
                                               LanguageManager.GetString("String_Page", x)));
        }

        private static Task<ValueTuple<string, string>> GetSpaceAndPageStringsAsync(string strLanguage, CancellationToken token = default)
        {
            return s_DicCachedStrings.GetOrAddAsync(
                strLanguage,
                async x => new ValueTuple<string, string>(await LanguageManager.GetStringAsync("String_Space", x, token: token).ConfigureAwait(false),
                    await LanguageManager.GetStringAsync("String_Page", x, token: token).ConfigureAwait(false)), token);
        }

        private readonly int _intHashCode;

        public static readonly SourceString Blank = GetSourceString(string.Empty, 0, GlobalSettings.DefaultLanguage, GlobalSettings.InvariantCultureInfo);

        public static SourceString GetSourceString(string strSourceString, string strLanguage, CultureInfo objCultureInfo, Character objCharacter)
        {
            return GetSourceString(strSourceString, strLanguage, objCultureInfo, objCharacter.Settings);
        }

        public static SourceString GetSourceString(string strSourceString, string strLanguage = "",
                                                   CultureInfo objCultureInfo = null, CharacterSettings objSettings = null)
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
            return GetSourceString(strCode, intPage, strLanguage, objCultureInfo, objSettings);
        }

        public static async Task<SourceString> GetSourceStringAsync(string strSourceString, string strLanguage, CultureInfo objCultureInfo, Character objCharacter, CancellationToken token = default)
        {
            return await GetSourceStringAsync(strSourceString, strLanguage, objCultureInfo, await objCharacter.GetSettingsAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
        }

        public static Task<SourceString> GetSourceStringAsync(string strSourceString, string strLanguage = "",
                                                                         CultureInfo objCultureInfo = null, CharacterSettings objSettings = null, CancellationToken token = default)
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
            return GetSourceStringAsync(strCode, intPage, strLanguage, objCultureInfo, objSettings, token);
        }

        public static SourceString GetSourceString(string strSource, string strPage, string strLanguage, CultureInfo objCultureInfo, Character objCharacter)
        {
            return GetSourceString(strSource, strPage, strLanguage, objCultureInfo, objCharacter.Settings);
        }

        public static SourceString GetSourceString(string strSource, string strPage, string strLanguage = "",
                                                   CultureInfo objCultureInfo = null, CharacterSettings objSettings = null)
        {
            int.TryParse(strPage, NumberStyles.Integer, GlobalSettings.InvariantCultureInfo, out int intPage);
            return GetSourceString(strSource, intPage, strLanguage, objCultureInfo, objSettings);
        }

        public static async Task<SourceString> GetSourceStringAsync(string strSource, string strPage, string strLanguage, CultureInfo objCultureInfo, Character objCharacter, CancellationToken token = default)
        {
            return await GetSourceStringAsync(strSource, strPage, strLanguage, objCultureInfo, await objCharacter.GetSettingsAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
        }

        public static Task<SourceString> GetSourceStringAsync(string strSource, string strPage, string strLanguage = "",
                                                                   CultureInfo objCultureInfo = null, CharacterSettings objSettings = null, CancellationToken token = default)
        {
            int.TryParse(strPage, NumberStyles.Integer, GlobalSettings.InvariantCultureInfo, out int intPage);
            return GetSourceStringAsync(strSource, intPage, strLanguage, objCultureInfo, objSettings, token);
        }

        public static SourceString GetSourceString(string strSource, int intPage, string strLanguage, CultureInfo objCultureInfo, Character objCharacter)
        {
            return GetSourceString(strSource, intPage, strLanguage, objCultureInfo, objCharacter.Settings);
        }

        public static SourceString GetSourceString(string strSource, int intPage, string strLanguage = "",
                                                   CultureInfo objCultureInfo = null, CharacterSettings objSettings = null)
        {
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalSettings.Language;
            return new SourceString(CommonFunctions.LanguageBookShort(strSource, strLanguage, objSettings),
                                    CommonFunctions.LanguageBookLong(strSource, strLanguage, objSettings), intPage,
                                    strLanguage, objCultureInfo);
        }

        public static async Task<SourceString> GetSourceStringAsync(string strSource, int intPage, string strLanguage, CultureInfo objCultureInfo, Character objCharacter, CancellationToken token = default)
        {
            return await GetSourceStringAsync(strSource, intPage, strLanguage, objCultureInfo, await objCharacter.GetSettingsAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
        }

        public static async Task<SourceString> GetSourceStringAsync(string strSource, int intPage, string strLanguage = "",
                                                                         CultureInfo objCultureInfo = null, CharacterSettings objSettings = null, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalSettings.Language;
            return new SourceString(await CommonFunctions.LanguageBookShortAsync(strSource, strLanguage, objSettings, token).ConfigureAwait(false),
                                    await CommonFunctions.LanguageBookLongAsync(strSource, strLanguage, objSettings, token).ConfigureAwait(false), intPage,
                                    strLanguage, objCultureInfo);
        }

        private SourceString(string strBookCodeShort, string strBookCodeLong, int intPage, string strLanguage, CultureInfo objCultureInfo)
        {
            strLanguage = !string.IsNullOrEmpty(strLanguage) ? strLanguage : GlobalSettings.Language;
            if (objCultureInfo == null)
                objCultureInfo = GlobalSettings.CultureInfo;
            Language = strLanguage;
            CultureInfo = objCultureInfo;
            Page = intPage;
            Code = strBookCodeShort;
            _intHashCode = (Language, CultureInfo, Code, Page).GetHashCode();
            _objTooltipInitializer = new Lazy<string>(() =>
            {
                (string strSpace, string strPage) = GetSpaceAndPageStrings(strLanguage);
                return strBookCodeLong + strSpace + strPage + strSpace + intPage.ToString(objCultureInfo);
            });
            _objAsyncTooltipInitializer = new AsyncLazy<string>(async () =>
            {
                (string strSpace, string strPage) = await GetSpaceAndPageStringsAsync(strLanguage).ConfigureAwait(false);
                return strBookCodeLong + strSpace + strPage + strSpace + intPage.ToString(objCultureInfo);
            }, Utils.JoinableTaskFactory);
        }

        public override string ToString()
        {
            string strCode = Code;
            if (string.IsNullOrEmpty(strCode))
                return string.Empty;
            string strSpace = GetSpaceAndPageStrings(Language).Item1;
            return strCode + strSpace + Page.ToString(CultureInfo);
        }

        public async Task<string> ToStringAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strCode = Code;
            if (string.IsNullOrEmpty(strCode))
                return string.Empty;
            string strSpace = (await GetSpaceAndPageStringsAsync(Language, token).ConfigureAwait(false)).Item1;
            return strCode + strSpace + Page.ToString(CultureInfo);
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
        public string LanguageBookTooltip => _objTooltipInitializer.Value;

        /// <summary>
        /// Provides the long-form name of the object's sourcebook and page reference.
        /// </summary>
        public Task<string> GetLanguageBookTooltipAsync(CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<string>(token);
            return _objTooltipInitializer.IsValueCreated
                ? Task.FromResult(_objTooltipInitializer.Value)
                : _objAsyncTooltipInitializer.GetValueAsync(token);
        }

        private readonly Lazy<string> _objTooltipInitializer;
        private readonly AsyncLazy<string> _objAsyncTooltipInitializer;

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
                    return Page.CompareTo(other.Page);
                }
            }
            return intCompareResult;
        }

        /// <summary>
        /// Set the Text and ToolTips for the selected control.
        /// </summary>
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
        public void SetControl(Control source, Form frmParent)
        {
            if (source == null)
                return;
            string strText = ToString();
            source.DoThreadSafe(x => x.Text = strText);
            string strToolTip = LanguageBookTooltip;
            if (source is IControlWithToolTip objSourceWithToolTip)
                objSourceWithToolTip.ToolTipText = strToolTip;
            else
                source.SetToolTip(frmParent, strToolTip);
        }

        /// <summary>
        /// Set the Text and ToolTips for the selected control.
        /// </summary>
        public async Task SetControlAsync(Control source, CancellationToken token = default)
        {
            if (source == null)
                return;
            string strText = await ToStringAsync(token).ConfigureAwait(false);
            await source.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
            await source.SetToolTipAsync(await GetLanguageBookTooltipAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
        }

        /// <summary>
        /// Set the Text and ToolTips for the selected control.
        /// </summary>
        public async Task SetControlAsync(Control source, Form frmParent, CancellationToken token = default)
        {
            if (source == null)
                return;
            string strText = await ToStringAsync(token).ConfigureAwait(false);
            await source.DoThreadSafeAsync(x => x.Text = strText, token).ConfigureAwait(false);
            string strToolTip = await GetLanguageBookTooltipAsync(token).ConfigureAwait(false);
            if (source is IControlWithToolTip objSourceWithToolTip)
                await objSourceWithToolTip.SetToolTipTextAsync(strToolTip, token).ConfigureAwait(false);
            else
                await source.SetToolTipAsync(frmParent, strToolTip, token).ConfigureAwait(false);
        }

        public bool Equals(SourceString other)
        {
            if (GetHashCode() != other.GetHashCode())
                return false;
            return string.Equals(Language, other.Language, StringComparison.OrdinalIgnoreCase) && string.Equals(Code, other.Code, StringComparison.Ordinal) && Page == other.Page;
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
