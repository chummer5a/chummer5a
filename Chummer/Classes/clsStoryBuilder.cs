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
        private readonly ConcurrentDictionary<string, string> persistenceDictionary = new ConcurrentDictionary<string, string>();
        private readonly Character _objCharacter;

        public StoryBuilder(Character objCharacter)
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            persistenceDictionary.TryAdd("metatype", _objCharacter.Metatype.ToLowerInvariant());
            persistenceDictionary.TryAdd("metavariant", _objCharacter.Metavariant.ToLowerInvariant());
        }

        public string GetStory(string strLanguage)
        {
            //Little bit of data required for following steps
            XmlDocument xdoc = XmlManager.Load("lifemodules.xml", strLanguage);

            //Generate list of all life modules (xml, we don't save required data to quality) this character has
            List<XmlNode> modules = new List<XmlNode>(10);

            foreach (Quality quality in _objCharacter.Qualities)
            {
                if (quality.Type == QualityType.LifeModule)
                {
                    modules.Add(Quality.GetNodeOverrideable(quality.SourceIDString, xdoc));
                }
            }

            //Sort the list (Crude way, but have to do)
            for (int i = 0; i < modules.Count; i++)
            {
                string stageName = xdoc.SelectSingleNode(i <= 4 ? "chummer/stages/stage[@order = \"" + (i + 1).ToString(GlobalOptions.InvariantCultureInfo) + "\"]" : "chummer/stages/stage[@order = \"5\"]")?.InnerText;
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
            object storyLock = new object();
            XPathNavigator xmlBaseMacrosNode = xdoc.GetFastNavigator().SelectSingleNode("/chummer/storybuilder/macros");
            //Actually "write" the story
            Parallel.For(0, modules.Count, i =>
            {
                XmlNode objStoryModule = modules[i];
                StringBuilder objModuleString = new StringBuilder();
                Write(objModuleString, objStoryModule["story"]?.InnerText ?? string.Empty, 5, xmlBaseMacrosNode);
                lock (storyLock)
                    story[i] = objModuleString.ToString();
            });

            return string.Join(Environment.NewLine + Environment.NewLine, story);
        }

        private void Write(StringBuilder story, string innerText, int levels, XPathNavigator xmlBaseMacrosNode)
        {
            if (levels <= 0) return;

            int startingLength = story.Length;

            string[] words;
            if (innerText.StartsWith('$') && innerText.IndexOf(' ') < 0)
            {
                words = Macro(innerText, xmlBaseMacrosNode).Split(' ', '\n', '\r', '\t');
            }
            else
            {
                words = innerText.Split(' ', '\n', '\r', '\t');
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

            //$DOLLAR is defined elsewhere to prevent recursive calling
            if (macroName == "street")
            {
                if (!string.IsNullOrEmpty(_objCharacter.Alias))
                {
                    return _objCharacter.Alias;
                }
                return "Alias ";
            }
            if(macroName == "real")
            {
                if (!string.IsNullOrEmpty(_objCharacter.Name))
                {
                    return _objCharacter.Name;
                }
                return "Unnamed John Doe ";
            }
            if (macroName == "year")
            {
                if (int.TryParse(_objCharacter.Age, out int year))
                {
                    if (int.TryParse(macroPool, out int age))
                    {
                        return (DateTime.UtcNow.Year + 62 + age - year).ToString(GlobalOptions.CultureInfo);
                    }
                    return (DateTime.UtcNow.Year + 62 - year).ToString(GlobalOptions.CultureInfo);
                }
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
                    if (!persistenceDictionary.TryGetValue(macroPool, out string strSelectedNodeName))
                    {
                        if (xmlUserMacroFirstChild.Name == "random")
                        {
                            XPathNodeIterator xmlPossibleNodeList = xmlUserMacroFirstChild.Select("./*[not(self::default)]");
                            if (xmlPossibleNodeList.Count > 0)
                            {
                                string[] strNames = new string[xmlPossibleNodeList.Count];
                                int i = 0;
                                foreach (XPathNavigator xmlLoopNode in xmlPossibleNodeList)
                                {
                                    strNames[i] = xmlLoopNode.Name;
                                    ++i;
                                }

                                strSelectedNodeName = strNames[strNames.Length > 1 ? GlobalOptions.RandomGenerator.NextModuloBiasRemoved(strNames.Length) : 0];
                            }
                        }
                        else if (xmlUserMacroFirstChild.Name == "persistent")
                        {
                            //Any node not named
                            XPathNodeIterator xmlPossibleNodeList = xmlUserMacroFirstChild.Select("./*[not(self::default)]");
                            if (xmlPossibleNodeList.Count > 0)
                            {
                                string[] strNames = new string[xmlPossibleNodeList.Count];
                                int i = 0;
                                foreach (XPathNavigator xmlLoopNode in xmlPossibleNodeList)
                                {
                                    strNames[i] = xmlLoopNode.Name;
                                    ++i;
                                }

                                strSelectedNodeName = strNames[strNames.Length > 1 ? GlobalOptions.RandomGenerator.NextModuloBiasRemoved(strNames.Length) : 0];
                                if (!persistenceDictionary.TryAdd(macroPool, strSelectedNodeName))
                                    persistenceDictionary.TryGetValue(macroPool, out strSelectedNodeName);
                            }
                        }
                        else
                        {
                            return "(Formating error in  $DOLLAR" + macroName + " )";
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

                    return "(Unknown key " + macroPool + " in  $DOLLAR" + macroName + " )";
                }

                return xmlUserMacroNode.Value;
            }
            return "(Unknown Macro  $DOLLAR" + innerText.Substring(1) + " )";
        }
    }
}
