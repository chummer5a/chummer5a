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
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Chummer
{
    /// <summary>
    /// XPath-style placeholder replacements for character values that are not core attributes
    /// (see <see cref="Chummer.Backend.Attributes.AttributeSection.AttributeStrings"/>).
    /// Invoked from <see cref="Character.ExpandXPathPlaceholders(string, IReadOnlyDictionary{string, int}?, CancellationToken)"/>
    /// after <see cref="Chummer.Backend.Attributes.AttributeSection.ApplyAttributeXPathReplacements(string, IReadOnlyDictionary{string, int}?, CancellationToken)"/>.
    /// </summary>
    internal static class CharacterXPathSubstitution
    {
        /// <summary>
        /// Numeric tokens for XPath-style expressions (e.g. <c>{InitiateGrade}</c>). Add entries here to support new placeholders.
        /// </summary>
        private static readonly NumericScalarDefinition[] s_numericScalars =
        {
            new NumericScalarDefinition(
                "{InitiateGrade",
                nameof(Character.InitiateGrade),
                c => c.InitiateGrade,
                async (c, t) => await c.GetInitiateGradeAsync(t).ConfigureAwait(false)),
            new NumericScalarDefinition(
                "{SubmersionGrade",
                nameof(Character.SubmersionGrade),
                c => c.SubmersionGrade,
                async (c, t) => await c.GetSubmersionGradeAsync(t).ConfigureAwait(false)),
        };

        /// <summary>
        /// Tooltip-formatted tokens (localized label + optional value). Parallel to <see cref="s_numericScalars"/> where tooltip text is needed.
        /// </summary>
        private static readonly TooltipScalarDefinition[] s_tooltipScalars =
        {
            new TooltipScalarDefinition(
                "{InitiateGrade",
                nameof(Character.InitiateGrade),
                "String_InitiateGrade",
                c => c.InitiateGrade,
                async (c, t) => await c.GetInitiateGradeAsync(t).ConfigureAwait(false)),
            new TooltipScalarDefinition(
                "{SubmersionGrade",
                nameof(Character.SubmersionGrade),
                "String_SubmersionGrade",
                c => c.SubmersionGrade,
                async (c, t) => await c.GetSubmersionGradeAsync(t).ConfigureAwait(false)),
        };

        private readonly struct NumericScalarDefinition
        {
            public string TokenPrefix { get; }
            public string OverrideKey { get; }
            private readonly Func<Character, int> _valueSync;
            private readonly Func<Character, CancellationToken, Task<int>> _valueAsync;

            public NumericScalarDefinition(string tokenPrefix, string overrideKey,
                Func<Character, int> valueSync, Func<Character, CancellationToken, Task<int>> valueAsync)
            {
                TokenPrefix = tokenPrefix;
                OverrideKey = overrideKey;
                _valueSync = valueSync;
                _valueAsync = valueAsync;
            }

            private string FullToken => TokenPrefix + "}";

            /// <summary>
            /// True when this scalar's placeholder appears in <paramref name="xmlBonus"/> and the character property
            /// for this scalar is in <paramref name="changedCharacterProperties"/>.
            /// Used to re-apply bonuses after stored <see cref="Improvement.Value"/> became stale (same issue as Rating).
            /// Does not walk the skills list — only the supplied bonus XML.
            /// </summary>
            internal bool ShouldRefreshPowerBonus(HashSet<string> changedCharacterProperties, XmlNode xmlBonus,
                CancellationToken token)
            {
                token.ThrowIfCancellationRequested();
                if (xmlBonus == null || changedCharacterProperties == null || changedCharacterProperties.Count == 0)
                    return false;
                if (!changedCharacterProperties.Contains(OverrideKey))
                    return false;
                return xmlBonus.InnerXmlContentContains(FullToken, token);
            }

            private int Resolve(Character c, IReadOnlyDictionary<string, int> d)
            {
                return d != null && d.TryGetValue(OverrideKey, out int intOverride)
                    ? intOverride
                    : _valueSync(c);
            }

            private async Task<int> ResolveAsync(Character c, IReadOnlyDictionary<string, int> d, CancellationToken token)
            {
                return d != null && d.TryGetValue(OverrideKey, out int intOverride)
                    ? intOverride
                    : await _valueAsync(c, token).ConfigureAwait(false);
            }

            public string ApplyString(string str, Character c, IReadOnlyDictionary<string, int> d, CancellationToken token)
            {
                NumericScalarDefinition self = this;
                token.ThrowIfCancellationRequested();
                if (string.IsNullOrEmpty(str) || str.IndexOf(self.TokenPrefix, StringComparison.Ordinal) < 0)
                    return str;
                return str.CheapReplace(self.FullToken,
                    () => self.Resolve(c, d).ToString(GlobalSettings.InvariantCultureInfo));
            }

            public async Task<string> ApplyStringAsync(string str, Character c, IReadOnlyDictionary<string, int> d,
                CancellationToken token)
            {
                NumericScalarDefinition self = this;
                token.ThrowIfCancellationRequested();
                if (string.IsNullOrEmpty(str) || str.IndexOf(self.TokenPrefix, StringComparison.Ordinal) < 0)
                    return str;
                return await str.CheapReplaceAsync(self.FullToken,
                    async () => (await self.ResolveAsync(c, d, token).ConfigureAwait(false))
                        .ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
            }

            public void ApplyStringBuilder(StringBuilder sbdInput, string strOriginal, Character c,
                IReadOnlyDictionary<string, int> d, CancellationToken token)
            {
                NumericScalarDefinition self = this;
                token.ThrowIfCancellationRequested();
                if (sbdInput == null || sbdInput.Length <= 0 || string.IsNullOrEmpty(strOriginal)
                                       || strOriginal.IndexOf(self.TokenPrefix, StringComparison.Ordinal) < 0)
                    return;
                sbdInput.CheapReplace(strOriginal, self.FullToken,
                    () => self.Resolve(c, d).ToString(GlobalSettings.InvariantCultureInfo));
            }

            public async Task ApplyStringBuilderAsync(StringBuilder sbdInput, string strOriginal, Character c,
                IReadOnlyDictionary<string, int> d, CancellationToken token)
            {
                NumericScalarDefinition self = this;
                token.ThrowIfCancellationRequested();
                if (sbdInput == null || sbdInput.Length <= 0 || string.IsNullOrEmpty(strOriginal)
                                       || strOriginal.IndexOf(self.TokenPrefix, StringComparison.Ordinal) < 0)
                    return;
                await sbdInput.CheapReplaceAsync(strOriginal, self.FullToken,
                    async () => (await self.ResolveAsync(c, d, token).ConfigureAwait(false))
                        .ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
            }
        }

        private readonly struct TooltipScalarDefinition
        {
            public string TokenPrefix { get; }
            public string OverrideKey { get; }
            public string LanguageKey { get; }
            private readonly Func<Character, int> _valueSync;
            private readonly Func<Character, CancellationToken, Task<int>> _valueAsync;

            public TooltipScalarDefinition(string tokenPrefix, string overrideKey, string languageKey,
                Func<Character, int> valueSync, Func<Character, CancellationToken, Task<int>> valueAsync)
            {
                TokenPrefix = tokenPrefix;
                OverrideKey = overrideKey;
                LanguageKey = languageKey;
                _valueSync = valueSync;
                _valueAsync = valueAsync;
            }

            private string FullToken => TokenPrefix + "}";

            private int ResolveSync(Character c, IReadOnlyDictionary<string, int> d)
            {
                return d != null && d.TryGetValue(OverrideKey, out int intOverride)
                    ? intOverride
                    : _valueSync(c);
            }

            private async Task<int> ResolveAsync(Character c, IAsyncReadOnlyDictionary<string, int> d,
                CancellationToken token)
            {
                return d != null && d.TryGetValue(OverrideKey, out int intOverride)
                    ? intOverride
                    : await _valueAsync(c, token).ConfigureAwait(false);
            }

            public string ApplyString(string str, Character c, CultureInfo objCultureInfo, string strLanguage,
                bool blnShowValues, string strSpace, IReadOnlyDictionary<string, int> d, CancellationToken token)
            {
                TooltipScalarDefinition self = this;
                token.ThrowIfCancellationRequested();
                if (string.IsNullOrEmpty(str) || str.IndexOf(self.TokenPrefix, StringComparison.Ordinal) < 0)
                    return str;
                return str.CheapReplace(self.FullToken, () =>
                {
                    string strInnerReturn = LanguageManager.GetString(self.LanguageKey, strLanguage, token: token);
                    if (blnShowValues)
                    {
                        int intVal = self.ResolveSync(c, d);
                        strInnerReturn += strSpace + "(" + intVal.ToString(objCultureInfo) + ")";
                    }

                    return strInnerReturn;
                });
            }

            public void ApplyStringBuilder(StringBuilder sbdInput, string strOriginal, Character c,
                CultureInfo objCultureInfo, string strLanguage, bool blnShowValues, string strSpace,
                IReadOnlyDictionary<string, int> d, CancellationToken token)
            {
                TooltipScalarDefinition self = this;
                token.ThrowIfCancellationRequested();
                if (sbdInput == null || sbdInput.Length <= 0 || string.IsNullOrEmpty(strOriginal)
                                       || strOriginal.IndexOf(self.TokenPrefix, StringComparison.Ordinal) < 0)
                    return;
                sbdInput.CheapReplace(strOriginal, self.FullToken, () =>
                {
                    string strInnerReturn = LanguageManager.GetString(self.LanguageKey, strLanguage, token: token);
                    if (blnShowValues)
                    {
                        int intVal = self.ResolveSync(c, d);
                        strInnerReturn += strSpace + "(" + intVal.ToString(objCultureInfo) + ")";
                    }

                    return strInnerReturn;
                });
            }

            public async Task<string> ApplyStringAsync(string str, Character c, CultureInfo objCultureInfo,
                string strLanguage, bool blnShowValues, string strSpace, IAsyncReadOnlyDictionary<string, int> d,
                CancellationToken token)
            {
                TooltipScalarDefinition self = this;
                token.ThrowIfCancellationRequested();
                if (string.IsNullOrEmpty(str) || str.IndexOf(self.TokenPrefix, StringComparison.Ordinal) < 0)
                    return str;
                return await str.CheapReplaceAsync(self.FullToken, async () =>
                {
                    string strInnerReturn =
                        await LanguageManager.GetStringAsync(self.LanguageKey, strLanguage, token: token)
                            .ConfigureAwait(false);
                    if (blnShowValues)
                    {
                        int intVal = await self.ResolveAsync(c, d, token).ConfigureAwait(false);
                        strInnerReturn += strSpace + "(" + intVal.ToString(objCultureInfo) + ")";
                    }

                    return strInnerReturn;
                }, token: token).ConfigureAwait(false);
            }

            public async Task ApplyStringBuilderAsync(StringBuilder sbdInput, string strOriginal, Character c,
                CultureInfo objCultureInfo, string strLanguage, bool blnShowValues, string strSpace,
                IAsyncReadOnlyDictionary<string, int> d, CancellationToken token)
            {
                TooltipScalarDefinition self = this;
                token.ThrowIfCancellationRequested();
                if (sbdInput == null || sbdInput.Length <= 0 || string.IsNullOrEmpty(strOriginal)
                                       || strOriginal.IndexOf(self.TokenPrefix, StringComparison.Ordinal) < 0)
                    return;
                await sbdInput.CheapReplaceAsync(strOriginal, self.FullToken, async () =>
                {
                    string strInnerReturn =
                        await LanguageManager.GetStringAsync(self.LanguageKey, strLanguage, token: token)
                            .ConfigureAwait(false);
                    if (blnShowValues)
                    {
                        int intVal = await self.ResolveAsync(c, d, token).ConfigureAwait(false);
                        strInnerReturn += strSpace + "(" + intVal.ToString(objCultureInfo) + ")";
                    }

                    return strInnerReturn;
                }, token: token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Fast gate: true if <paramref name="changedCharacterProperties"/> includes any property that backs a numeric scalar
        /// (see <see cref="s_numericScalars"/>). Used before scanning inventory bonus XML for stale XPath scalar placeholders.
        /// </summary>
        internal static bool ChangedPropertiesOverlapNumericScalarDependencies(HashSet<string> changedCharacterProperties)
        {
            if (changedCharacterProperties == null || changedCharacterProperties.Count == 0)
                return false;
            foreach (NumericScalarDefinition spec in s_numericScalars)
            {
                if (changedCharacterProperties.Contains(spec.OverrideKey))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// True if a bonus XML node should be recreated because it references a numeric scalar whose backing property changed.
        /// </summary>
        internal static bool BonusXmlNeedsNumericScalarRefresh(XmlNode xmlBonus,
            HashSet<string> changedCharacterProperties, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (xmlBonus == null || changedCharacterProperties == null || changedCharacterProperties.Count == 0)
                return false;
            foreach (NumericScalarDefinition spec in s_numericScalars)
            {
                if (spec.ShouldRefreshPowerBonus(changedCharacterProperties, xmlBonus, token))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// True if any of the bonus XML fragments reference a stale numeric scalar for the given property changes.
        /// </summary>
        internal static bool AnyBonusXmlNeedsNumericScalarRefresh(HashSet<string> changedCharacterProperties,
            CancellationToken token, params XmlNode[] xmlBonuses)
        {
            token.ThrowIfCancellationRequested();
            if (xmlBonuses == null || xmlBonuses.Length == 0)
                return false;
            foreach (XmlNode xmlBonus in xmlBonuses)
            {
                if (xmlBonus == null)
                    continue;
                if (BonusXmlNeedsNumericScalarRefresh(xmlBonus, changedCharacterProperties, token))
                    return true;
            }

            return false;
        }

        internal static string ApplyScalarXPathReplacements(string strReturn, Character objCharacter,
            IReadOnlyDictionary<string, int> dicValueOverrides = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strReturn) || strReturn.IndexOf('{') < 0)
                return strReturn;
            foreach (NumericScalarDefinition spec in s_numericScalars)
                strReturn = spec.ApplyString(strReturn, objCharacter, dicValueOverrides, token);
            return strReturn;
        }

        internal static void ApplyScalarXPathReplacements(StringBuilder sbdInput, string strOriginal,
            Character objCharacter, IReadOnlyDictionary<string, int> dicValueOverrides = null,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (sbdInput == null || sbdInput.Length <= 0 || string.IsNullOrEmpty(strOriginal)
                                   || strOriginal.IndexOf('{') < 0)
                return;
            foreach (NumericScalarDefinition spec in s_numericScalars)
                spec.ApplyStringBuilder(sbdInput, strOriginal, objCharacter, dicValueOverrides, token);
        }

        internal static async Task<string> ApplyScalarXPathReplacementsAsync(string strReturn, Character objCharacter,
            IReadOnlyDictionary<string, int> dicValueOverrides = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strReturn) || strReturn.IndexOf('{') < 0)
                return strReturn;
            foreach (NumericScalarDefinition spec in s_numericScalars)
                strReturn = await spec.ApplyStringAsync(strReturn, objCharacter, dicValueOverrides, token)
                    .ConfigureAwait(false);
            return strReturn;
        }

        internal static async Task ApplyScalarXPathReplacementsAsync(StringBuilder sbdInput, string strOriginal,
            Character objCharacter, IReadOnlyDictionary<string, int> dicValueOverrides = null,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (sbdInput == null || sbdInput.Length <= 0 || string.IsNullOrEmpty(strOriginal)
                                   || strOriginal.IndexOf('{') < 0)
                return;
            foreach (NumericScalarDefinition spec in s_numericScalars)
                await spec.ApplyStringBuilderAsync(sbdInput, strOriginal, objCharacter, dicValueOverrides, token)
                    .ConfigureAwait(false);
        }

        internal static string ApplyScalarXPathReplacementsForTooltip(string strReturn, Character objCharacter,
            CultureInfo objCultureInfo, string strLanguage, bool blnShowValues, string strSpace,
            IReadOnlyDictionary<string, int> dicValueOverrides = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strReturn) || strReturn.IndexOf('{') < 0)
                return strReturn;
            foreach (TooltipScalarDefinition spec in s_tooltipScalars)
                strReturn = spec.ApplyString(strReturn, objCharacter, objCultureInfo, strLanguage, blnShowValues,
                    strSpace, dicValueOverrides, token);
            return strReturn;
        }

        internal static void ApplyScalarXPathReplacementsForTooltip(StringBuilder sbdInput, string strOriginal,
            Character objCharacter, CultureInfo objCultureInfo, string strLanguage, bool blnShowValues,
            string strSpace, IReadOnlyDictionary<string, int> dicValueOverrides = null,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (sbdInput == null || sbdInput.Length <= 0 || string.IsNullOrEmpty(strOriginal)
                                   || strOriginal.IndexOf('{') < 0)
                return;
            foreach (TooltipScalarDefinition spec in s_tooltipScalars)
                spec.ApplyStringBuilder(sbdInput, strOriginal, objCharacter, objCultureInfo, strLanguage,
                    blnShowValues, strSpace, dicValueOverrides, token);
        }

        internal static async Task<string> ApplyScalarXPathReplacementsForTooltipAsync(string strReturn,
            Character objCharacter, CultureInfo objCultureInfo, string strLanguage, bool blnShowValues, string strSpace,
            IAsyncReadOnlyDictionary<string, int> dicValueOverrides = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strReturn) || strReturn.IndexOf('{') < 0)
                return strReturn;
            foreach (TooltipScalarDefinition spec in s_tooltipScalars)
                strReturn = await spec.ApplyStringAsync(strReturn, objCharacter, objCultureInfo, strLanguage,
                    blnShowValues, strSpace, dicValueOverrides, token).ConfigureAwait(false);
            return strReturn;
        }

        internal static async Task ApplyScalarXPathReplacementsForTooltipAsync(StringBuilder sbdInput,
            string strOriginal, Character objCharacter, CultureInfo objCultureInfo, string strLanguage,
            bool blnShowValues, string strSpace, IAsyncReadOnlyDictionary<string, int> dicValueOverrides = null,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (sbdInput == null || sbdInput.Length <= 0 || string.IsNullOrEmpty(strOriginal)
                                   || strOriginal.IndexOf('{') < 0)
                return;
            foreach (TooltipScalarDefinition spec in s_tooltipScalars)
                await spec.ApplyStringBuilderAsync(sbdInput, strOriginal, objCharacter, objCultureInfo, strLanguage,
                    blnShowValues, strSpace, dicValueOverrides, token).ConfigureAwait(false);
        }
    }
}
