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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace Chummer
{
    [DebuggerDisplay("{DisplayName(GlobalSettings.DefaultLanguage)}")]
    public sealed class StoryModule : IHasName, IHasInternalId, IHasXmlDataNode, IHasLockObject
    {
        private readonly ConcurrentDictionary<string, string> _dicEnglishTexts = new ConcurrentDictionary<string, string>();
        private readonly Guid _guiInternalId;
        private string _strName;
        private Guid _guiSourceID;
        private readonly Character _objCharacter;
        private string _strDefaultTextKey;

        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;

        public StoryModule(Character objCharacter)
        {
            _guiInternalId = Guid.NewGuid();
            _objCharacter = objCharacter;
            LockObject = new AsyncFriendlyReaderWriterLock(objCharacter?.LockObject);
        }

        public void Create(XmlNode xmlStoryModuleDataNode)
        {
            Create(xmlStoryModuleDataNode.CreateNavigator());
        }

        public void Create(XPathNavigator xmlStoryModuleDataNode)
        {
            using (LockObject.EnterWriteLock())
            {
                xmlStoryModuleDataNode.TryGetField("id", Guid.TryParse, out _guiSourceID);
                xmlStoryModuleDataNode.TryGetStringFieldQuickly("name", ref _strName);

                XPathNavigator xmlTextsNode = xmlStoryModuleDataNode.SelectSingleNodeAndCacheExpression("texts");
                if (xmlTextsNode != null)
                {
                    foreach (XPathNavigator xmlText in xmlStoryModuleDataNode.SelectChildren(XPathNodeType.Element))
                    {
                        _dicEnglishTexts.TryAdd(xmlText.Name, xmlText.Value);
                        if (xmlText.SelectSingleNodeAndCacheExpression("@default")?.Value == bool.TrueString)
                            _strDefaultTextKey = xmlText.Name;
                    }

                    if (string.IsNullOrEmpty(_strDefaultTextKey))
                        _strDefaultTextKey = _dicEnglishTexts.FirstOrDefault().Key;
                }
            }
        }

        public Task CreateAsync(XmlNode xmlStoryModuleDataNode, CancellationToken token = default)
        {
            return CreateAsync(xmlStoryModuleDataNode.CreateNavigator(), token);
        }

        public async Task CreateAsync(XPathNavigator xmlStoryModuleDataNode, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                xmlStoryModuleDataNode.TryGetField("id", Guid.TryParse, out _guiSourceID);
                xmlStoryModuleDataNode.TryGetStringFieldQuickly("name", ref _strName);

                XPathNavigator xmlTextsNode = xmlStoryModuleDataNode
                                                    .SelectSingleNodeAndCacheExpression("texts", token: token);
                if (xmlTextsNode != null)
                {
                    foreach (XPathNavigator xmlText in xmlStoryModuleDataNode.SelectChildren(XPathNodeType.Element))
                    {
                        token.ThrowIfCancellationRequested();
                        _dicEnglishTexts.TryAdd(xmlText.Name, xmlText.Value);
                        if (xmlText.SelectSingleNodeAndCacheExpression("@default", token: token)?.Value == bool.TrueString)
                            _strDefaultTextKey = xmlText.Name;
                    }

                    if (string.IsNullOrEmpty(_strDefaultTextKey))
                        _strDefaultTextKey = _dicEnglishTexts.FirstOrDefault().Key;
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public Story ParentStory
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _objParentStory;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _objParentStory = value;
            }
        }

        /// <summary>
        /// Was this story module generated randomly, likely due to a request for a random persistent module?
        /// </summary>
        public bool IsRandomlyGenerated
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnIsRandomlyGenerated;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _blnIsRandomlyGenerated = value;
            }
        }

        public string Name
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strName;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strName = value;
            }
        }

        /// <summary>
        /// Identifier of the object within data files.
        /// </summary>
        public Guid SourceID
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _guiSourceID;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_guiSourceID == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _guiSourceID = value;
                        _objCachedMyXmlNode = null;
                        _objCachedMyXPathNode = null;
                    }
                }
            }
        }

        /// <summary>
        /// String-formatted identifier of the <inheritdoc cref="SourceID"/> from the data files.
        /// </summary>
        public string SourceIDString => _guiSourceID.ToString("D", GlobalSettings.InvariantCultureInfo);

        public string DisplayNameShort(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            using (LockObject.EnterReadLock())
                return this.GetNodeXPath(strLanguage)?.SelectSingleNodeAndCacheExpression("translate")?.Value ?? Name;
        }

        public string DisplayName(string strLanguage)
        {
            return DisplayNameShort(strLanguage);
        }

        public string CurrentDisplayNameShort => DisplayNameShort(GlobalSettings.Language);

        public string CurrentDisplayName => DisplayName(GlobalSettings.Language);

        public async Task<string> DisplayNameShortAsync(string strLanguage, CancellationToken token = default)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                XPathNavigator objNode = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
                return objNode?.SelectSingleNodeAndCacheExpression("translate", token)?.Value ?? Name;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public Task<string> DisplayNameAsync(string strLanguage, CancellationToken token = default)
        {
            return DisplayNameShortAsync(strLanguage, token);
        }

        public Task<string> GetCurrentDisplayNameShortAsync(CancellationToken token = default) => DisplayNameShortAsync(GlobalSettings.Language, token);

        public Task<string> GetCurrentDisplayNameAsync(CancellationToken token = default) => DisplayNameAsync(GlobalSettings.Language, token);

        public string DefaultKey
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strDefaultTextKey;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strDefaultTextKey = value;
            }
        }

        public string DisplayText(string strKey, string strLanguage)
        {
            using (LockObject.EnterReadLock())
            {
                string strReturn;
                if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                {
                    return _dicEnglishTexts.TryGetValue(strKey, out strReturn) ? strReturn : '<' + strKey + '>';
                }

                return this.GetNodeXPath(strLanguage)?.SelectSingleNode("alttexts/" + strKey)?.Value ??
                       (_dicEnglishTexts.TryGetValue(strKey, out strReturn) ? strReturn : '<' + strKey + '>');
            }
        }

        public async Task<string> DisplayTextAsync(string strKey, string strLanguage, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                string strReturn;
                if (!strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                {
                    strReturn = (await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false))
                                ?.SelectSingleNode("alttexts/" + strKey)?.Value;
                    if (string.IsNullOrWhiteSpace(strReturn))
                        return strReturn;
                }

                return _dicEnglishTexts.TryGetValue(strKey, out strReturn) ? strReturn : '<' + strKey + '>';
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task<string> TestRunToGeneratePersistents(CultureInfo objCulture, string strLanguage, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await ResolveMacros(await DisplayTextAsync(DefaultKey, strLanguage, token).ConfigureAwait(false),
                                           objCulture, strLanguage, true, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task<string> PrintModule(CultureInfo objCulture, string strLanguage, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return (await ResolveMacros(
                    await DisplayTextAsync(DefaultKey, strLanguage, token).ConfigureAwait(false), objCulture,
                    strLanguage, token: token).ConfigureAwait(false)).NormalizeWhiteSpace();
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task<string> ResolveMacros(string strInput, CultureInfo objCulture, string strLanguage, bool blnGeneratePersistents = false, CancellationToken token = default)
        {
            string strReturn = strInput;
            // Boolean in tuple is set to true if substring is a macro in need of processing, otherwise it's set to false
            List<Tuple<string, bool>> lstSubstrings = new List<Tuple<string, bool>>(1);
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                while (!string.IsNullOrEmpty(strReturn))
                {
                    int intOpeningBracketIndex = strReturn.IndexOf('{');
                    if (intOpeningBracketIndex == -1)
                    {
                        lstSubstrings.Add(new Tuple<string, bool>(strReturn, false));
                        strReturn = string.Empty;
                    }
                    else
                    {
                        if (intOpeningBracketIndex + 1 < strReturn.Length)
                        {
                            int intClosingBracketIndex = -1;
                            int intBracketCount = 1;
                            for (int i = intOpeningBracketIndex + 1; i < strReturn.Length; ++i)
                            {
                                char chrLoopChar = strReturn[i];
                                if (chrLoopChar == '{')
                                    ++intBracketCount;
                                else if (chrLoopChar == '}')
                                {
                                    if (intBracketCount == 1)
                                    {
                                        intClosingBracketIndex = i;
                                        break;
                                    }

                                    --intBracketCount;
                                }
                            }

                            if (intClosingBracketIndex != -1)
                            {
                                lstSubstrings.Add(
                                    new Tuple<string, bool>(strReturn.Substring(0, intOpeningBracketIndex), false));
                                lstSubstrings.Add(new Tuple<string, bool>(
                                                      strReturn.Substring(
                                                          intOpeningBracketIndex + 1,
                                                          intClosingBracketIndex - intOpeningBracketIndex - 1), true));
                                strReturn = intClosingBracketIndex + 1 < strReturn.Length
                                    ? strReturn.Substring(intClosingBracketIndex + 1)
                                    : string.Empty;
                            }
                            else
                            {
                                strReturn = strReturn.Substring(0, intOpeningBracketIndex)
                                            + strReturn.Substring(intOpeningBracketIndex + 1);
                            }
                        }
                        else
                        {
                            lstSubstrings.Add(
                                new Tuple<string, bool>(strReturn.Substring(0, strReturn.Length - 1), false));
                            strReturn = string.Empty;
                        }
                    }
                }

                switch (lstSubstrings.Count)
                {
                    // Quit out early if we only have one item and it doesn't need processing
                    case 0:
                        return string.Empty;

                    case 1:
                    {
                        (string strContent, bool blnContainsMacros) = lstSubstrings[0];
                        if (!blnContainsMacros)
                            return strContent;
                        break;
                    }
                }

                string[] lstOutputStrings = new string[lstSubstrings.Count];
                for (int i = 0; i < lstSubstrings.Count; ++i)
                {
                    (string strContent, bool blnContainsMacros) = lstSubstrings[i];
                    if (blnContainsMacros)
                    {
                        lstOutputStrings[i] = await ProcessSingleMacro(strContent, objCulture, strLanguage,
                                                                       blnGeneratePersistents, token)
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        lstOutputStrings[i] = strContent;
                    }
                }

                return string.Concat(lstOutputStrings);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task<string> ProcessSingleMacro(string strInput, CultureInfo objCulture, string strLanguage, bool blnGeneratePersistents, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // Process Macros nested inside of single macro
                strInput = await ResolveMacros(strInput, objCulture, strLanguage, blnGeneratePersistents, token)
                    .ConfigureAwait(false);

                int intPipeIndex = strInput.IndexOf('|');
                string strFunction = intPipeIndex == -1 ? strInput : strInput.Substring(0, intPipeIndex);
                string strArguments = intPipeIndex + 1 >= strInput.Length
                    ? string.Empty
                    : strInput.Substring(intPipeIndex + 1);

                switch (strFunction)
                {
                    case "$ReverseTranslateExtra":
                    {
                        return await _objCharacter.ReverseTranslateExtraAsync(strArguments, token: token)
                                                  .ConfigureAwait(false);
                    }
                    case "$XmlNameFriendly":
                    {
                        return strArguments
                               .FastEscape(' ', '$', '/', '?', ',', '\'', '\"', ';', ':', '(', ')', '[', ']', '|', '\\',
                                           '+', '=', '`', '~', '!', '@', '#', '%', '^', '&', '*')
                               .FastEscape((await LanguageManager
                                                  .GetStringAsync("String_NuyenSymbol", strLanguage, token: token)
                                                  .ConfigureAwait(false))
                                           .ToCharArray()).ToLower(objCulture);
                    }
                    case "$CharacterName":
                    {
                        return _objCharacter.CharacterName;
                    }
                    case "$CharacterGrammaticalGender":
                    {
                        return _objCharacter.CharacterGrammaticGender;
                    }
                    case "$Metatype":
                    {
                        return _objCharacter.Metatype;
                    }
                    case "$Metavariant":
                    {
                        return _objCharacter.Metavariant;
                    }
                    case "$Eyes":
                    {
                        return _objCharacter.Eyes;
                    }
                    case "$Hair":
                    {
                        return _objCharacter.Hair;
                    }
                    case "$Skin":
                    {
                        return _objCharacter.Skin;
                    }
                    case "$Height":
                    {
                        return _objCharacter.Height;
                    }
                    case "$Weight":
                    {
                        return _objCharacter.Weight;
                    }
                    case "$Gender":
                    {
                        return _objCharacter.Gender;
                    }
                    case "$Alias":
                    {
                        return !string.IsNullOrEmpty(_objCharacter.Alias)
                            ? _objCharacter.Alias
                            : await LanguageManager.GetStringAsync("String_Unknown", strLanguage, token: token)
                                                   .ConfigureAwait(false);
                    }
                    case "$Name":
                    {
                        if (!string.IsNullOrWhiteSpace(_objCharacter.Name))
                        {
                            if (!string.IsNullOrEmpty(strArguments) && int.TryParse(strArguments, out int intNameIndex))
                            {
                                string[] lstNames
                                    = _objCharacter.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                                return lstNames[Math.Max(Math.Min(intNameIndex, lstNames.Length - 1), 0)];
                            }

                            return _objCharacter.Name;
                        }

                        return await LanguageManager.GetStringAsync("String_Unknown", strLanguage, token: token)
                                                    .ConfigureAwait(false);
                    }
                    case "$Year":
                    {
                        if (int.TryParse(_objCharacter.Age, out int intCurrentAge))
                        {
                            int intBirthYear = DateTime.UtcNow.Year + 62 - intCurrentAge;
                            if (!string.IsNullOrEmpty(strArguments)
                                && int.TryParse(strArguments, out int intYearAtTime))
                            {
                                return (intBirthYear + intYearAtTime).ToString(objCulture);
                            }

                            return intBirthYear.ToString(objCulture);
                        }

                        return await LanguageManager.GetStringAsync("String_Unknown", strLanguage, token: token)
                                                    .ConfigureAwait(false);
                    }
                    case "$GetString":
                    {
                        return await LanguageManager.GetStringAsync(strArguments, strLanguage, token: token)
                                                    .ConfigureAwait(false);
                    }
                    case "$XPath":
                    {
                        (bool blnSuccess, object objProcess) = await CommonFunctions
                                                                     .EvaluateInvariantXPathAsync(strArguments, token)
                                                                     .ConfigureAwait(false);
                        return blnSuccess
                            ? objProcess.ToString()
                            : await LanguageManager.GetStringAsync("String_Unknown", strLanguage, token: token)
                                                   .ConfigureAwait(false);
                    }
                    case "$Index":
                    {
                        string[] strArgumentsSplit = strArguments.Split('|', StringSplitOptions.RemoveEmptyEntries);
                        int intArgumentsCount = strArgumentsSplit.Length;
                        if (intArgumentsCount > 2 && int.TryParse(strArgumentsSplit[0], out int intIndex))
                        {
                            return strArgumentsSplit[Math.Max(0, Math.Min(intArgumentsCount - 1, intIndex + 1))];
                        }

                        return await LanguageManager.GetStringAsync("String_Unknown", strLanguage, token: token)
                                                    .ConfigureAwait(false);
                    }
                    case "$LookupExtra":
                    {
                        string strExtra = _objCharacter.AIPrograms
                                                       .FirstOrDefault(
                                                           x => x.Name == strArguments
                                                                && !string.IsNullOrEmpty(x.Extra))?.Extra ??
                                          _objCharacter.Armor
                                                       .FirstOrDefault(
                                                           x => x.Name == strArguments
                                                                && !string.IsNullOrEmpty(x.Extra))?.Extra ??
                                          _objCharacter.ComplexForms
                                                       .FirstOrDefault(
                                                           x => x.Name == strArguments
                                                                && !string.IsNullOrEmpty(x.Extra))?.Extra ??
                                          _objCharacter.CritterPowers
                                                       .FirstOrDefault(
                                                           x => x.Name == strArguments
                                                                && !string.IsNullOrEmpty(x.Extra))?.Extra ??
                                          _objCharacter.Cyberware
                                                       .DeepFirstOrDefault(
                                                           x => x.Children,
                                                           x => x.Name == strArguments
                                                                && !string.IsNullOrEmpty(x.Extra))?.Extra ??
                                          _objCharacter.Gear
                                                       .DeepFirstOrDefault(
                                                           x => x.Children,
                                                           x => x.Name == strArguments
                                                                && !string.IsNullOrEmpty(x.Extra))?.Extra ??
                                          _objCharacter.Powers
                                                       .FirstOrDefault(
                                                           x => x.Name == strArguments
                                                                && !string.IsNullOrEmpty(x.Extra))?.Extra ??
                                          _objCharacter.Qualities
                                                       .FirstOrDefault(
                                                           x => x.Name == strArguments
                                                                && !string.IsNullOrEmpty(x.Extra))?.Extra ??
                                          _objCharacter.Spells
                                                       .FirstOrDefault(
                                                           x => x.Name == strArguments
                                                                && !string.IsNullOrEmpty(x.Extra))?.Extra;
                        if (!string.IsNullOrEmpty(strExtra))
                        {
                            return await _objCharacter.TranslateExtraAsync(strExtra, strLanguage, token: token)
                                                      .ConfigureAwait(false);
                        }

                        return string.Empty;
                    }
                    case "$Fallback":
                    {
                        int intArgumentPipeIndex = strArguments.IndexOf('|');
                        if (intArgumentPipeIndex != -1)
                        {
                            string strMainOutput = strArguments.Substring(0, intArgumentPipeIndex);
                            if (!string.IsNullOrEmpty(strMainOutput)
                                && strMainOutput != await LanguageManager
                                                          .GetStringAsync("String_Error", strLanguage, token: token)
                                                          .ConfigureAwait(false) && strMainOutput
                                != await LanguageManager.GetStringAsync("String_Unknown", strLanguage, token: token)
                                                        .ConfigureAwait(false))
                                return strMainOutput;
                            if (intArgumentPipeIndex + 1 < strArguments.Length)
                                return strArguments.Substring(intArgumentPipeIndex + 1);
                        }

                        return string.Empty;
                    }
                }

                if (blnGeneratePersistents)
                {
                    if (ParentStory.PersistentModules.TryGetValue(strFunction, out StoryModule objInnerModule))
                        return await ResolveMacros(
                                await objInnerModule.DisplayTextAsync(strArguments, strLanguage, token)
                                                    .ConfigureAwait(false), objCulture, strLanguage, token: token)
                            .ConfigureAwait(false);
                    StoryModule objPersistentStoryModule
                        = await ParentStory.GeneratePersistentModule(strFunction, token).ConfigureAwait(false);
                    if (objPersistentStoryModule != null)
                        return await ResolveMacros(
                                await objPersistentStoryModule.DisplayTextAsync(strArguments, strLanguage, token)
                                                              .ConfigureAwait(false), objCulture, strLanguage,
                                token: token)
                            .ConfigureAwait(false);
                }
                else if (ParentStory.PersistentModules.TryGetValue(strFunction, out StoryModule objInnerModule))
                    return await ResolveMacros(
                            await objInnerModule.DisplayTextAsync(strArguments, strLanguage, token)
                                .ConfigureAwait(false), objCulture, strLanguage, token: token)
                        .ConfigureAwait(false);

                return await LanguageManager.GetStringAsync("String_Error", strLanguage, token: token)
                                            .ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string InternalId
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    return _guiInternalId == Guid.Empty
                        ? string.Empty
                        : _guiInternalId.ToString("D", GlobalSettings.InvariantCultureInfo);
                }
            }
        }

        public async Task<XmlNode> GetNodeCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IDisposable objLocker = null;
            IAsyncDisposable objLockerAsync = null;
            if (blnSync)
                // ReSharper disable once MethodHasAsyncOverload
                objLocker = LockObject.EnterReadLock(token);
            else
                objLockerAsync = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                XmlNode objReturn = _objCachedMyXmlNode;
                if (objReturn != null && strLanguage == _strCachedXmlNodeLanguage
                                      && !GlobalSettings.LiveCustomData)
                    return objReturn;
                XmlDocument objDoc = blnSync
                    // ReSharper disable once MethodHasAsyncOverload
                    ? _objCharacter.LoadData("stories.xml", strLanguage, token: token)
                    : await _objCharacter.LoadDataAsync("stories.xml", strLanguage, token: token).ConfigureAwait(false);
                objReturn = objDoc.TryGetNodeById("/chummer/stories/story", SourceID);
                if (objReturn == null && SourceID != Guid.Empty)
                {
                    objReturn = objDoc.TryGetNodeByNameOrId("/chummer/stories/story", Name);
                    objReturn?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
                }
                _objCachedMyXmlNode = objReturn;
                _strCachedXmlNodeLanguage = strLanguage;
                return objReturn;
            }
            finally
            {
                objLocker?.Dispose();
                if (objLockerAsync != null)
                    await objLockerAsync.DisposeAsync().ConfigureAwait(false);
            }
        }

        private XPathNavigator _objCachedMyXPathNode;
        private string _strCachedXPathNodeLanguage = string.Empty;
        private bool _blnIsRandomlyGenerated;
        private Story _objParentStory;

        public async Task<XPathNavigator> GetNodeXPathCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IDisposable objLocker = null;
            IAsyncDisposable objLockerAsync = null;
            if (blnSync)
                // ReSharper disable once MethodHasAsyncOverload
                objLocker = LockObject.EnterReadLock(token);
            else
                objLockerAsync = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                XPathNavigator objReturn = _objCachedMyXPathNode;
                if (objReturn != null && strLanguage == _strCachedXPathNodeLanguage
                                      && !GlobalSettings.LiveCustomData)
                    return objReturn;
                XPathNavigator objDoc = blnSync
                    // ReSharper disable once MethodHasAsyncOverload
                    ? _objCharacter.LoadDataXPath("stories.xml", strLanguage, token: token)
                    : await _objCharacter.LoadDataXPathAsync("stories.xml", strLanguage, token: token).ConfigureAwait(false);
                objReturn = objDoc.TryGetNodeById("/chummer/stories/story", SourceID);
                if (objReturn == null && SourceID != Guid.Empty)
                {
                    objReturn = objDoc.TryGetNodeByNameOrId("/chummer/stories/story", Name);
                    objReturn?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
                }
                _objCachedMyXPathNode = objReturn;
                _strCachedXPathNodeLanguage = strLanguage;
                return objReturn;
            }
            finally
            {
                objLocker?.Dispose();
                if (objLockerAsync != null)
                    await objLockerAsync.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            LockObject.Dispose();
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            return LockObject.DisposeAsync();
        }

        /// <inheritdoc />
        public AsyncFriendlyReaderWriterLock LockObject { get; }
    }
}
