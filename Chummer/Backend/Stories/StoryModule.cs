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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace Chummer
{
    [DebuggerDisplay("{DisplayName(GlobalSettings.DefaultLanguage)}")]
    public class StoryModule : IHasName, IHasInternalId, IHasXmlDataNode
    {
        private readonly Dictionary<string, string> _dicEnglishTexts = new Dictionary<string, string>();
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
        }

        public void Create(XmlNode xmlStoryModuleDataNode)
        {
            xmlStoryModuleDataNode.TryGetField("id", Guid.TryParse, out _guiSourceID);
            xmlStoryModuleDataNode.TryGetStringFieldQuickly("name", ref _strName);

            XmlNode xmlTextsNode = xmlStoryModuleDataNode.SelectSingleNode("texts");
            if (xmlTextsNode != null)
            {
                using (XmlNodeList xmlChildrenList = xmlStoryModuleDataNode.SelectNodes("*"))
                {
                    if (xmlChildrenList != null)
                    {
                        foreach (XmlNode xmlText in xmlChildrenList)
                        {
                            _dicEnglishTexts.Add(xmlText.Name, xmlText.Value);
                            if (xmlText.SelectSingleNode("@default")?.Value == bool.TrueString)
                                _strDefaultTextKey = xmlText.Name;
                        }

                        if (string.IsNullOrEmpty(_strDefaultTextKey))
                            _strDefaultTextKey = _dicEnglishTexts.Keys.FirstOrDefault();
                    }
                }
            }
        }

        public void Create(XPathNavigator xmlStoryModuleDataNode)
        {
            xmlStoryModuleDataNode.TryGetField("id", Guid.TryParse, out _guiSourceID);
            xmlStoryModuleDataNode.TryGetStringFieldQuickly("name", ref _strName);

            XPathNavigator xmlTextsNode = xmlStoryModuleDataNode.SelectSingleNodeAndCacheExpression("texts");
            if (xmlTextsNode != null)
            {
                foreach (XPathNavigator xmlText in xmlStoryModuleDataNode.SelectChildren(XPathNodeType.Element))
                {
                    _dicEnglishTexts.Add(xmlText.Name, xmlText.Value);
                    if (xmlText.SelectSingleNode("@default")?.Value == bool.TrueString)
                        _strDefaultTextKey = xmlText.Name;
                }

                if (string.IsNullOrEmpty(_strDefaultTextKey))
                    _strDefaultTextKey = _dicEnglishTexts.Keys.FirstOrDefault();
            }
        }

        public Story ParentStory { get; set; }

        /// <summary>
        /// Was this story module generated randomly, likely due to a request for a random persistent module?
        /// </summary>
        public bool IsRandomlyGenerated { get; set; }

        public string Name
        {
            get => _strName;
            set => _strName = value;
        }

        /// <summary>
        /// Identifier of the object within data files.
        /// </summary>
        public Guid SourceID
        {
            get => _guiSourceID;
            set
            {
                if (_guiSourceID == value)
                    return;
                _guiSourceID = value;
                _objCachedMyXmlNode = null;
                _objCachedMyXPathNode = null;
            }
        }

        /// <summary>
        /// String-formatted identifier of the <inheritdoc cref="SourceID"/> from the data files.
        /// </summary>
        public string SourceIDString => _guiSourceID.ToString("D", GlobalSettings.InvariantCultureInfo);

        public string CurrentDisplayName => DisplayName(GlobalSettings.Language);

        public string DisplayName(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            return GetNodeXPath(strLanguage)?.SelectSingleNodeAndCacheExpression("translate")?.Value ?? Name;
        }

        public string DefaultKey
        {
            get => _strDefaultTextKey;
            set => _strDefaultTextKey = value;
        }

        public string DisplayText(string strKey, string strLanguage)
        {
            string strReturn;
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                return _dicEnglishTexts.TryGetValue(strKey, out strReturn) ? strReturn : '<' + strKey + '>';
            }

            return GetNodeXPath(strLanguage)?.SelectSingleNode("alttexts/" + strKey)?.Value ??
                   (_dicEnglishTexts.TryGetValue(strKey, out strReturn) ? strReturn : '<' + strKey + '>');
        }

        public Task TestRunToGeneratePersistents(CultureInfo objCulture, string strLanguage)
        {
            return ResolveMacros(DisplayText(DefaultKey, strLanguage), objCulture, strLanguage, true);
        }

        public async Task<string> PrintModule(CultureInfo objCulture, string strLanguage)
        {
            return (await ResolveMacros(DisplayText(DefaultKey, strLanguage), objCulture, strLanguage)).NormalizeWhiteSpace();
        }

        public async Task<string> ResolveMacros(string strInput, CultureInfo objCulture, string strLanguage, bool blnGeneratePersistents = false)
        {
            string strReturn = strInput;
            // Boolean in tuple is set to true if substring is a macro in need of processing, otherwise it's set to false
            List<Tuple<string, bool>> lstSubstrings = new List<Tuple<string, bool>>(1);
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
                            lstSubstrings.Add(new Tuple<string, bool>(strReturn.Substring(0, intOpeningBracketIndex), false));
                            lstSubstrings.Add(new Tuple<string, bool>(strReturn.Substring(intOpeningBracketIndex + 1, intClosingBracketIndex - intOpeningBracketIndex - 1), true));
                            strReturn = intClosingBracketIndex + 1 < strReturn.Length ? strReturn.Substring(intClosingBracketIndex + 1) : string.Empty;
                        }
                        else
                        {
                            strReturn = strReturn.Substring(0, intOpeningBracketIndex) + strReturn.Substring(intOpeningBracketIndex + 1);
                        }
                    }
                    else
                    {
                        lstSubstrings.Add(new Tuple<string, bool>(strReturn.Substring(0, strReturn.Length - 1), false));
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
                        blnGeneratePersistents);
                }
                else
                {
                    lstOutputStrings[i] = strContent;
                }
            }
            return string.Concat(lstOutputStrings);
        }

        public async Task<string> ProcessSingleMacro(string strInput, CultureInfo objCulture, string strLanguage, bool blnGeneratePersistents)
        {
            // Process Macros nested inside of single macro
            strInput = await ResolveMacros(strInput, objCulture, strLanguage, blnGeneratePersistents);

            int intPipeIndex = strInput.IndexOf('|');
            string strFunction = intPipeIndex == -1 ? strInput : strInput.Substring(0, intPipeIndex);
            string strArguments = intPipeIndex + 1 >= strInput.Length ? string.Empty : strInput.Substring(intPipeIndex + 1);

            switch (strFunction)
            {
                case "$ReverseTranslateExtra":
                    {
                        return await _objCharacter.ReverseTranslateExtraAsync(strArguments);
                    }
                case "$XmlNameFriendly":
                    {
                        return strArguments.FastEscape(' ', '$', '/', '?', ',', '\'', '\"', '¥', ';', ':', '(', ')', '[', ']', '|', '\\', '+', '=', '`', '~', '!', '@', '#', '%', '^', '&', '*').ToLower(objCulture);
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
                        return !string.IsNullOrEmpty(_objCharacter.Alias) ? _objCharacter.Alias : await LanguageManager.GetStringAsync("String_Unknown", strLanguage);
                    }
                case "$Name":
                    {
                        if (!string.IsNullOrWhiteSpace(_objCharacter.Name))
                        {
                            if (!string.IsNullOrEmpty(strArguments) && int.TryParse(strArguments, out int intNameIndex))
                            {
                                string[] lstNames = _objCharacter.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                                return lstNames[Math.Max(Math.Min(intNameIndex, lstNames.Length - 1), 0)];
                            }

                            return _objCharacter.Name;
                        }
                        return await LanguageManager.GetStringAsync("String_Unknown", strLanguage);
                    }
                case "$Year":
                    {
                        if (int.TryParse(_objCharacter.Age, out int intCurrentAge))
                        {
                            int intBirthYear = DateTime.UtcNow.Year + 62 - intCurrentAge;
                            if (!string.IsNullOrEmpty(strArguments) && int.TryParse(strArguments, out int intYearAtTime))
                            {
                                return (intBirthYear + intYearAtTime).ToString(objCulture);
                            }

                            return intBirthYear.ToString(objCulture);
                        }

                        return await LanguageManager.GetStringAsync("String_Unknown", strLanguage);
                    }
                case "$GetString":
                    {
                        return await LanguageManager.GetStringAsync(strArguments, strLanguage);
                    }
                case "$XPath":
                    {
                        object objProcess = CommonFunctions.EvaluateInvariantXPath(strArguments, out bool blnIsSuccess);
                        return blnIsSuccess ? objProcess.ToString() : await LanguageManager.GetStringAsync("String_Unknown", strLanguage);
                    }
                case "$Index":
                    {
                        string[] strArgumentsSplit = strArguments.Split('|', StringSplitOptions.RemoveEmptyEntries);
                        int intArgumentsCount = strArgumentsSplit.Length;
                        if (intArgumentsCount > 2 && int.TryParse(strArgumentsSplit[0], out int intIndex))
                        {
                            return strArgumentsSplit[Math.Max(0, Math.Min(intArgumentsCount - 1, intIndex + 1))];
                        }

                        return await LanguageManager.GetStringAsync("String_Unknown", strLanguage);
                    }
                case "$LookupExtra":
                    {
                        string strExtra = _objCharacter.AIPrograms.FirstOrDefault(x => x.Name == strArguments && !string.IsNullOrEmpty(x.Extra))?.Extra ??
                            _objCharacter.Armor.FirstOrDefault(x => x.Name == strArguments && !string.IsNullOrEmpty(x.Extra))?.Extra ??
                            _objCharacter.ComplexForms.FirstOrDefault(x => x.Name == strArguments && !string.IsNullOrEmpty(x.Extra))?.Extra ??
                            _objCharacter.CritterPowers.FirstOrDefault(x => x.Name == strArguments && !string.IsNullOrEmpty(x.Extra))?.Extra ??
                            _objCharacter.Cyberware.DeepFirstOrDefault(x => x.Children, x => x.Name == strArguments && !string.IsNullOrEmpty(x.Extra))?.Extra ??
                            _objCharacter.Gear.DeepFirstOrDefault(x => x.Children, x => x.Name == strArguments && !string.IsNullOrEmpty(x.Extra))?.Extra ??
                            _objCharacter.Powers.FirstOrDefault(x => x.Name == strArguments && !string.IsNullOrEmpty(x.Extra))?.Extra ??
                            _objCharacter.Qualities.FirstOrDefault(x => x.Name == strArguments && !string.IsNullOrEmpty(x.Extra))?.Extra ??
                            _objCharacter.Spells.FirstOrDefault(x => x.Name == strArguments && !string.IsNullOrEmpty(x.Extra))?.Extra;
                        if (!string.IsNullOrEmpty(strExtra))
                        {
                            return await _objCharacter.TranslateExtraAsync(strExtra, strLanguage);
                        }

                        return string.Empty;
                    }
                case "$Fallback":
                    {
                        int intArgumentPipeIndex = strArguments.IndexOf('|');
                        if (intArgumentPipeIndex != -1)
                        {
                            string strMainOutput = strArguments.Substring(0, intArgumentPipeIndex);
                            if (!string.IsNullOrEmpty(strMainOutput) && strMainOutput != await LanguageManager.GetStringAsync("String_Error", strLanguage) && strMainOutput != await LanguageManager.GetStringAsync("String_Unknown", strLanguage))
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
                    return await ResolveMacros(objInnerModule.DisplayText(strArguments, strLanguage), objCulture, strLanguage);
                StoryModule objPersistentStoryModule = ParentStory.GeneratePersistentModule(strFunction);
                if (objPersistentStoryModule != null)
                    return await ResolveMacros(objPersistentStoryModule.DisplayText(strArguments, strLanguage), objCulture, strLanguage);
            }
            else if (ParentStory.PersistentModules.TryGetValue(strFunction, out StoryModule objInnerModule))
                return await ResolveMacros(objInnerModule.DisplayText(strArguments, strLanguage), objCulture, strLanguage);

            return await LanguageManager.GetStringAsync("String_Error", strLanguage);
        }

        public string InternalId => _guiInternalId == Guid.Empty ? string.Empty : _guiInternalId.ToString("D", GlobalSettings.InvariantCultureInfo);

        public XmlNode GetNode(string strLanguage)
        {
            if (_objCachedMyXmlNode != null && strLanguage == _strCachedXmlNodeLanguage
                                            && !GlobalSettings.LiveCustomData)
                return _objCachedMyXmlNode;
            _objCachedMyXmlNode = _objCharacter.LoadData("stories.xml", strLanguage)
                                               .SelectSingleNode(
                                                   "/chummer/stories/story[id = " + SourceIDString.CleanXPath()
                                                   + " or id = " + SourceIDString.ToUpperInvariant().CleanXPath()
                                                   + ']');
            _strCachedXmlNodeLanguage = strLanguage;
            return _objCachedMyXmlNode;
        }

        private XPathNavigator _objCachedMyXPathNode;
        private string _strCachedXPathNodeLanguage = string.Empty;

        public XPathNavigator GetNodeXPath(string strLanguage)
        {
            if (_objCachedMyXPathNode != null && strLanguage == _strCachedXPathNodeLanguage
                                              && !GlobalSettings.LiveCustomData)
                return _objCachedMyXPathNode;
            _objCachedMyXPathNode = _objCharacter.LoadDataXPath("stories.xml", strLanguage)
                                                 .SelectSingleNode(
                                                     "/chummer/stories/story[id = " + SourceIDString.CleanXPath()
                                                     + " or id = " + SourceIDString.ToUpperInvariant().CleanXPath()
                                                     + ']');
            _strCachedXPathNodeLanguage = strLanguage;
            return _objCachedMyXPathNode;
        }
    }
}
