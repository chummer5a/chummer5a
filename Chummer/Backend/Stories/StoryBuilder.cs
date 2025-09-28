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
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace Chummer
{
    public sealed class StoryBuilder : IHasLockObject, IHasCharacterObject
    {
        private readonly ConcurrentDictionary<string, string> _dicPersistence = new ConcurrentDictionary<string, string>();
        private readonly Character _objCharacter;

        public Character CharacterObject => _objCharacter; // readonly member, no locking needed

        public StoryBuilder(Character objCharacter)
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            LockObject = objCharacter.LockObject;
        }

        public async Task<string> GetStory(string strLanguage = "", CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(strLanguage))
                strLanguage = GlobalSettings.Language;

            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                //Little bit of data required for following steps
                XmlDocument xmlDoc = await _objCharacter.LoadDataAsync("lifemodules.xml", strLanguage, token: token)
                    .ConfigureAwait(false);
                XPathNavigator xdoc = await _objCharacter
                    .LoadDataXPathAsync("lifemodules.xml", strLanguage, token: token)
                    .ConfigureAwait(false);

                //Generate list of all life modules (xml, we don't save required data to quality) this character has
                List<XmlNode> modules = new List<XmlNode>(10);

                await _objCharacter.Qualities.ForEachAsync(quality =>
                {
                    if (quality.Type == QualityType.LifeModule)
                    {
                        modules.Add(Quality.GetNodeOverrideable(quality.SourceIDString, xmlDoc));
                    }
                }, token).ConfigureAwait(false);

                //Sort the list (Crude way, but have to do)
                for (int i = 0; i < modules.Count; i++)
                {
                    string stageName = xdoc
                        .SelectSingleNodeAndCacheExpression("chummer/stages/stage[@order = "
                                                            + (i <= 4
                                                                ? (i + 1).ToString(GlobalSettings
                                                                    .InvariantCultureInfo)
                                                                .CleanXPath()
                                                                : "\"5\"") + "]", token: token)?.Value;
                    int j;
                    for (j = i; j < modules.Count; j++)
                    {
                        if (modules[j]["stage"]?.InnerTextViaPool(token) == stageName)
                            break;
                    }

                    if (j != i && j < modules.Count)
                    {
                        (modules[i], modules[j]) = (modules[j], modules[i]);
                    }
                }

                string[] story = ArrayPool<string>.Shared.Rent(modules.Count);
                try
                {
                    XPathNavigator xmlBaseMacrosNode = xdoc
                            .SelectSingleNodeAndCacheExpression(
                                "/chummer/storybuilder/macros", token: token);
                    await ParallelExtensions.ForAsync(0, modules.Count, async i =>
                    {
                        using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                           out StringBuilder sbdTemp))
                        {
                            story[i] = (await Write(sbdTemp, modules[i]["story"]?.InnerTextViaPool(token) ?? string.Empty, 5,
                                xmlBaseMacrosNode, token).ConfigureAwait(false)).ToTrimmedString();
                        }
                    }, token).ConfigureAwait(false);

                    return StringExtensions.JoinFast(Environment.NewLine + Environment.NewLine, story, 0, modules.Count);
                }
                finally
                {
                    ArrayPool<string>.Shared.Return(story);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async Task<StringBuilder> Write(StringBuilder story, string innerText, int levels, XPathNavigator xmlBaseMacrosNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (levels <= 0)
                return story;
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int startingLength = story.Length;

                IEnumerable<string> words;
                if (innerText.StartsWith('$') && innerText.IndexOf(' ') < 0)
                {
                    words = (await Macro(innerText, xmlBaseMacrosNode, token).ConfigureAwait(false)).SplitNoAlloc(
                        StringSplitOptions.RemoveEmptyEntries, ' ', '\n', '\r', '\t');
                }
                else
                {
                    words = innerText.SplitNoAlloc(StringSplitOptions.RemoveEmptyEntries, ' ', '\n', '\r', '\t');
                }

                bool mfix = false;
                foreach (string word in words)
                {
                    if (string.IsNullOrWhiteSpace(word))
                        continue;
                    token.ThrowIfCancellationRequested();
                    string trim = word.Trim();

                    if (trim.StartsWith('$'))
                    {
                        if (trim.StartsWith("$DOLLAR", StringComparison.Ordinal))
                        {
                            story.Append('$');
                        }
                        else
                        {
                            //if (story.Length > 0 && story[story.Length - 1] == ' ') story.Length--;
                            await Write(story, trim, --levels, xmlBaseMacrosNode, token).ConfigureAwait(false);
                        }

                        mfix = true;
                    }
                    else
                    {
                        if (story.Length != startingLength && !mfix)
                        {
                            story.Append(' ');
                        }
                        else
                        {
                            mfix = false;
                        }

                        story.Append(trim);
                    }
                }

                return story;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task<string> Macro(string innerText, XPathNavigator xmlBaseMacrosNode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(innerText))
                return string.Empty;
            string endString = innerText.ToLowerInvariant().Substring(1).TrimEnd(',', '.');
            string macroName, macroPool;
            if (endString.Contains('_'))
            {
                string[] split = endString.SplitFixedSizePooledArray('_', 2);
                try
                {
                    macroName = split[0];
                    macroPool = split[1];
                }
                finally
                {
                    ArrayPool<string>.Shared.Return(split);
                }
            }
            else
            {
                macroName = macroPool = endString;
            }

            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                switch (macroName.ToUpperInvariant())
                {
                    //$DOLLAR is defined elsewhere to prevent recursive calling
                    case "METATYPE":
                        string strMetatype = await _objCharacter.GetMetatypeAsync(token).ConfigureAwait(false);
                        return strMetatype.ToLowerInvariant();

                    case "METAVARIANT":
                        string strMetavariant = await _objCharacter.GetMetavariantAsync(token).ConfigureAwait(false);
                        return strMetavariant.ToLowerInvariant();

                    case "STREET":
                        string strAlias = await _objCharacter.GetAliasAsync(token).ConfigureAwait(false);
                        return !string.IsNullOrEmpty(strAlias) ? strAlias : "Alias ";

                    case "REAL":
                        string strName = await _objCharacter.GetNameAsync(token).ConfigureAwait(false);
                        return !string.IsNullOrEmpty(strName) ? strName : "Unnamed John Doe ";

                    case "YEAR":
                        string strAge = await _objCharacter.GetAgeAsync(token).ConfigureAwait(false);
                        if (int.TryParse(strAge, out int year))
                        {
                            return int.TryParse(macroPool, out int age)
                                ? (DateTime.UtcNow.Year + 62 + age - year).ToString(GlobalSettings.CultureInfo)
                                : (DateTime.UtcNow.Year + 62 - year).ToString(GlobalSettings.CultureInfo);
                        }
                        return "(ERROR PARSING \"" + _objCharacter.Age + "\")";
                }

                //Did not meet predefined macros, check user defined

                XPathNavigator xmlUserMacroNode = xmlBaseMacrosNode?.SelectSingleNode(macroName);

                if (xmlUserMacroNode != null)
                {
                    XPathNavigator xmlUserMacroFirstChild
                        = xmlUserMacroNode.SelectChildren(XPathNodeType.Element).Current;
                    if (xmlUserMacroFirstChild != null)
                    {
                        //Already defined, no need to do anything fancy
                        if (!_dicPersistence.TryGetValue(macroPool, out string strSelectedNodeName))
                        {
                            switch (xmlUserMacroFirstChild.Name.ToUpperInvariant())
                            {
                                case "RANDOM":
                                {
                                    XPathNodeIterator xmlPossibleNodeList = xmlUserMacroFirstChild
                                        .SelectAndCacheExpression("./*[not(self::default)]", token: token);
                                    if (xmlPossibleNodeList.Count > 0)
                                    {
                                        int intUseIndex = xmlPossibleNodeList.Count > 1
                                            ? await Utils.GlobalRandom
                                                .NextModuloBiasRemovedAsync(
                                                    xmlPossibleNodeList.Count, token: token)
                                                .ConfigureAwait(false)
                                            : 0;
                                        int i = 0;
                                        foreach (XPathNavigator xmlLoopNode in xmlPossibleNodeList)
                                        {
                                            token.ThrowIfCancellationRequested();
                                            if (i == intUseIndex)
                                            {
                                                strSelectedNodeName = xmlLoopNode.Name;
                                                break;
                                            }

                                            ++i;
                                        }
                                    }

                                    break;
                                }
                                case "PERSISTENT":
                                {
                                    //Any node not named
                                    XPathNodeIterator xmlPossibleNodeList = xmlUserMacroFirstChild
                                        .SelectAndCacheExpression("./*[not(self::default)]", token: token);
                                    if (xmlPossibleNodeList.Count > 0)
                                    {
                                        int intUseIndex = xmlPossibleNodeList.Count > 1
                                            ? await Utils.GlobalRandom
                                                .NextModuloBiasRemovedAsync(
                                                    xmlPossibleNodeList.Count, token: token)
                                                .ConfigureAwait(false)
                                            : 0;
                                        int i = 0;
                                        foreach (XPathNavigator xmlLoopNode in xmlPossibleNodeList)
                                        {
                                            token.ThrowIfCancellationRequested();
                                            if (i == intUseIndex)
                                            {
                                                strSelectedNodeName = xmlLoopNode.Name;
                                                break;
                                            }

                                            ++i;
                                        }

                                        string strToAdd = strSelectedNodeName;
                                        strSelectedNodeName = _dicPersistence.GetOrAdd(macroPool, x => strToAdd);
                                    }

                                    break;
                                }
                                default:
                                    return "(Formating error in $DOLLAR" + macroName + ")";
                            }
                        }

                        if (!string.IsNullOrEmpty(strSelectedNodeName))
                        {
                            string strSelected = xmlUserMacroFirstChild.SelectSingleNode(strSelectedNodeName)?.Value;
                            if (!string.IsNullOrEmpty(strSelected))
                                return strSelected;
                        }

                        string strDefault = xmlUserMacroFirstChild
                            .SelectSingleNodeAndCacheExpression("default", token: token)?.Value;
                        if (!string.IsNullOrEmpty(strDefault))
                        {
                            return strDefault;
                        }

                        return "(Unknown key " + macroPool + " in $DOLLAR" + macroName + ")";
                    }

                    return xmlUserMacroNode.Value;
                }

                return "(Unknown Macro $DOLLAR" + innerText.Substring(1) + ")";
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // No disposal necessary because our LockObject is our character owner's LockObject
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            // No disposal necessary because our LockObject is our character owner's LockObject
            return default;
        }

        /// <inheritdoc />
        public AsyncFriendlyReaderWriterLock LockObject { get; }
    }
}
