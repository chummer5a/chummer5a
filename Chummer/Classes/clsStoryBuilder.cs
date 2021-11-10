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
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace Chummer
{
    public sealed class StoryBuilder
    {
        private readonly ConcurrentDictionary<string, string> _dicPersistence = new ConcurrentDictionary<string, string>();
        private readonly Character _objCharacter;

        public StoryBuilder(Character objCharacter)
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            _dicPersistence.TryAdd("metatype", _objCharacter.Metatype.ToLowerInvariant());
            _dicPersistence.TryAdd("metavariant", _objCharacter.Metavariant.ToLowerInvariant());
        }

        public async Task<string> GetStory(string strLanguage)
        {
            //Little bit of data required for following steps
            XmlDocument xmlDoc = await _objCharacter.LoadDataAsync("lifemodules.xml", strLanguage);
            XPathNavigator xdoc = await _objCharacter.LoadDataXPathAsync("lifemodules.xml", strLanguage);

            //Generate list of all life modules (xml, we don't save required data to quality) this character has
            List<XmlNode> modules = new List<XmlNode>(10);

            foreach (Quality quality in _objCharacter.Qualities)
            {
                if (quality.Type == QualityType.LifeModule)
                {
                    modules.Add(Quality.GetNodeOverrideable(quality.SourceIDString, xmlDoc));
                }
            }

            //Sort the list (Crude way, but have to do)
            for (int i = 0; i < modules.Count; i++)
            {
                string stageName = xdoc.SelectSingleNode("chummer/stages/stage[@order = " + (i <= 4 ? (i + 1).ToString(GlobalSettings.InvariantCultureInfo).CleanXPath() : "\"5\"") + "]")?.Value;
                int j;
                for (j = i; j < modules.Count; j++)
                {
                    if (modules[j]["stage"]?.InnerText == stageName)
                        break;
                }
                if (j != i && j < modules.Count)
                {
                    XmlNode tmp = modules[i];
                    modules[i] = modules[j];
                    modules[j] = tmp;
                }
            }

            string[] story = new string[modules.Count];
            XPathNavigator xmlBaseMacrosNode = xdoc.SelectSingleNode("/chummer/storybuilder/macros");
            //Actually "write" the story
            await Task.Run(() => Parallel.For(0, modules.Count,
                i => story[i] = Write(new StringBuilder(), modules[i]["story"]?.InnerText ?? string.Empty, 5,
                    xmlBaseMacrosNode).ToString()));
            return string.Join(Environment.NewLine + Environment.NewLine, story);
        }

        private StringBuilder Write(StringBuilder story, string innerText, int levels, XPathNavigator xmlBaseMacrosNode)
        {
            if (levels <= 0)
                return story;
            int startingLength = story.Length;

            IEnumerable<string> words;
            if (innerText.StartsWith('$') && innerText.IndexOf(' ') < 0)
            {
                words = Macro(innerText, xmlBaseMacrosNode).SplitNoAlloc(' ', '\n', '\r', '\t');
            }
            else
            {
                words = innerText.SplitNoAlloc(' ', '\n', '\r', '\t');
            }
            bool mfix = false;
            foreach (string word in words)
            {
                string trim = word.Trim();
                if (string.IsNullOrEmpty(trim))
                    continue;

                if (trim.StartsWith('$'))
                {
                    if (trim.StartsWith("$DOLLAR", StringComparison.Ordinal))
                    {
                        story.Append('$');
                    }
                    else
                    {
                        //if (story.Length > 0 && story[story.Length - 1] == ' ') story.Length--;
                        Write(story, trim, --levels, xmlBaseMacrosNode);
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

        public string Macro(string innerText, XPathNavigator xmlBaseMacrosNode)
        {
            if (string.IsNullOrEmpty(innerText))
                return string.Empty;
            string endString = innerText.ToLowerInvariant().Substring(1).TrimEnd(',', '.');
            string macroName, macroPool;
            if (endString.Contains('_'))
            {
                string[] split = endString.Split('_');
                macroName = split[0];
                macroPool = split[1];
            }
            else
            {
                macroName = macroPool = endString;
            }

            switch (macroName)
            {
                //$DOLLAR is defined elsewhere to prevent recursive calling
                case "street":
                    return !string.IsNullOrEmpty(_objCharacter.Alias) ? _objCharacter.Alias : "Alias ";

                case "real":
                    return !string.IsNullOrEmpty(_objCharacter.Name) ? _objCharacter.Name : "Unnamed John Doe ";

                case "year" when int.TryParse(_objCharacter.Age, out int year):
                    return int.TryParse(macroPool, out int age)
                        ? (DateTime.UtcNow.Year + 62 + age - year).ToString(GlobalSettings.CultureInfo)
                        : (DateTime.UtcNow.Year + 62 - year).ToString(GlobalSettings.CultureInfo);

                case "year":
                    return "(ERROR PARSING \"" + _objCharacter.Age + "\")";
            }

            //Did not meet predefined macros, check user defined

            XPathNavigator xmlUserMacroNode = xmlBaseMacrosNode?.SelectSingleNode(macroName);

            if (xmlUserMacroNode != null)
            {
                XPathNavigator xmlUserMacroFirstChild = xmlUserMacroNode.SelectChildren(XPathNodeType.Element).Current;
                if (xmlUserMacroFirstChild != null)
                {
                    //Already defined, no need to do anything fancy
                    if (!_dicPersistence.TryGetValue(macroPool, out string strSelectedNodeName))
                    {
                        switch (xmlUserMacroFirstChild.Name)
                        {
                            case "random":
                                {
                                    XPathNodeIterator xmlPossibleNodeList = xmlUserMacroFirstChild.SelectAndCacheExpression("./*[not(self::default)]");
                                    if (xmlPossibleNodeList.Count > 0)
                                    {
                                        int intUseIndex = xmlPossibleNodeList.Count > 1
                                            ? GlobalSettings.RandomGenerator.NextModuloBiasRemoved(xmlPossibleNodeList.Count)
                                            : 0;
                                        int i = 0;
                                        foreach (XPathNavigator xmlLoopNode in xmlPossibleNodeList)
                                        {
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
                            case "persistent":
                                {
                                    //Any node not named
                                    XPathNodeIterator xmlPossibleNodeList = xmlUserMacroFirstChild.SelectAndCacheExpression("./*[not(self::default)]");
                                    if (xmlPossibleNodeList.Count > 0)
                                    {
                                        int intUseIndex = xmlPossibleNodeList.Count > 1
                                            ? GlobalSettings.RandomGenerator.NextModuloBiasRemoved(xmlPossibleNodeList.Count)
                                            : 0;
                                        int i = 0;
                                        foreach (XPathNavigator xmlLoopNode in xmlPossibleNodeList)
                                        {
                                            if (i == intUseIndex)
                                            {
                                                strSelectedNodeName = xmlLoopNode.Name;
                                                break;
                                            }
                                            ++i;
                                        }

                                        if (!_dicPersistence.TryAdd(macroPool, strSelectedNodeName))
                                            _dicPersistence.TryGetValue(macroPool, out strSelectedNodeName);
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

                    string strDefault = xmlUserMacroFirstChild.SelectSingleNode("default")?.Value;
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
    }
}
