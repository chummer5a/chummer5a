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
﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    public static class LanguageManager
    {
        /// <summary>
        /// An individual language string.
        /// </summary>
        private struct LanguageString
        {
            /// <summary>
            /// String's unique Key.
            /// </summary>
            public string Key { get; }

            /// <summary>
            /// String's text.
            /// </summary>
            public string Text { get; }

            public LanguageString(string strKey, string strText)
            {
                Key = strKey ?? string.Empty;
                Text = strText ?? string.Empty;
            }

            public override bool Equals(object obj)
            {
                return Key.Equals(obj.ToString());
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override string ToString()
            {
                return Key;
            }
        }

        public class LanguageData
        {
            public IDictionary<string, string> TranslatedStrings { get; } = new Dictionary<string, string>();
            public XmlDocument DataDocument { get; } = new XmlDocument();
            public bool IsLoaded { get; private set; } = true;

            public LanguageData(string strLanguage)
            {
                string strFilePath = Path.Combine(Application.StartupPath, "lang", strLanguage + ".xml");
                if (File.Exists(strFilePath))
                {
                    XmlDocument objLanguageDocument = new XmlDocument();
                    objLanguageDocument.Load(strFilePath);
                    if (objLanguageDocument != null)
                    {
                        foreach (XmlNode objNode in objLanguageDocument.SelectNodes("/chummer/strings/string"))
                        {
                            // Look for the English version of the found string. If it has been found, replace the English contents with the contents from this file.
                            // If the string was not found, then someone has inserted a Key that should not exist and is ignored.
                            string strKey = objNode["key"]?.InnerText;
                            string strText = objNode["text"]?.InnerText;
                            if (!string.IsNullOrEmpty(strKey) && !string.IsNullOrEmpty(strText))
                            {
                                if (TranslatedStrings.ContainsKey(strKey))
                                    TranslatedStrings[strKey] = strText.Replace("\\n", "\n");
                                else
                                    TranslatedStrings.Add(strKey, strText.Replace("\\n", "\n"));
                            }
                        }
                    }
                    else
                        IsLoaded = false;
                }
                else
                    IsLoaded = false;

                // Check to see if the data translation file for the selected language exists.
                string strDataPath = Path.Combine(Application.StartupPath, "lang", strLanguage + "_data.xml");
                if (File.Exists(strDataPath))
                {
                    DataDocument.Load(strDataPath);
                }
                else
                    IsLoaded = false;
            }
        }

        private static string s_StrDefaultLanguage = GlobalOptions.DefaultLanguage;
        private static readonly Dictionary<string, LanguageData> s_DictionaryLanguages = new Dictionary<string, LanguageData>();
        public static IReadOnlyDictionary<string, LanguageData> DictionaryLanguages { get => s_DictionaryLanguages; }
        private static readonly Dictionary<string, string> s_DictionaryEnglishStrings = new Dictionary<string, string>();

        #region Constructor
        static LanguageManager()
        {
            if (!Utils.IsRunningInVisualStudio)
            {
                XmlDocument objEnglishDocument = new XmlDocument();
                string strFilePath = Path.Combine(Application.StartupPath, "lang", GlobalOptions.DefaultLanguage + ".xml");
                if (File.Exists(strFilePath))
                {
                    objEnglishDocument.Load(strFilePath);
                    foreach (XmlNode objNode in objEnglishDocument.SelectNodes("/chummer/strings/string"))
                    {
                        string strKey = objNode["key"]?.InnerText;
                        string strText = objNode["text"]?.InnerText;
                        if (!string.IsNullOrEmpty(strKey) && !string.IsNullOrEmpty(strText))
                        {
                            if (s_DictionaryEnglishStrings.ContainsKey(strKey))
                                Utils.BreakIfDebug();
                            else
                                s_DictionaryEnglishStrings.Add(strKey, strText.Replace("\\n", "\n"));
                        }
                    }
                }
                else
                    MessageBox.Show("Language strings for the default language (" + GlobalOptions.DefaultLanguage + ") could not be loaded.", "Cannot Load Language", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Translate an object int a specified language.
        /// </summary>
        /// <param name="strIntoLanguage">Language to which to translate the object.</param>
        /// <param name="objObject">Object to translate.</param>
        public static void Translate(string strIntoLanguage, object objObject)
        {
            // s_StrLanguage is populated when the language is read for the first time, meaning this is only triggered once (and language is only read in once since it shouldn't change).
            if (s_StrDefaultLanguage != strIntoLanguage)
            {
                s_StrDefaultLanguage = strIntoLanguage;
                
                if (strIntoLanguage != GlobalOptions.DefaultLanguage && !s_DictionaryLanguages.ContainsKey(strIntoLanguage))
                {
                    LanguageData objNewLanguage = new LanguageData(strIntoLanguage);
                    if (!objNewLanguage.IsLoaded)
                    {
                        MessageBox.Show("Language with code " + strIntoLanguage + " could not be loaded.", "Cannot Load Language", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    else
                        s_DictionaryLanguages.Add(strIntoLanguage, objNewLanguage);
                }
            }

            UpdateControls(objObject as Control, strIntoLanguage);
        }

        /// <summary>
        /// Recursive method to translate all of the controls in a Form or UserControl.
        /// </summary>
        /// <param name="objParent">Control container to translate.</param>
        private static void UpdateControls(Control objParent, string strIntoLanguage)
        {
            if (objParent == null)
                return;

            if (objParent is Form frmForm)
            {
                // Translatable items are identified by having a value in their Tag attribute. The contents of Tag is the string to lookup in the language list.
                // Update the Form itself.
                string strControlTag = frmForm.Tag?.ToString();
                if (!string.IsNullOrEmpty(strControlTag))
                    frmForm.Text = GetString(strControlTag, strIntoLanguage);

                // update any menu strip items that have tags
                if (frmForm.MainMenuStrip != null)
                    foreach (ToolStripMenuItem tssItem in frmForm.MainMenuStrip.Items)
                        TranslateToolStripItemsRecursively(tssItem, strIntoLanguage);
            }

            // Translatable items are identified by having a value in their Tag attribute. The contents of Tag is the string to lookup in the language list.
            foreach (Control objChild in objParent.Controls)
            {
                if (objChild as Label != null || objChild as Button != null || objChild as CheckBox != null)
                {
                    string strControlTag = objChild.Tag?.ToString();
                    if (!string.IsNullOrEmpty(strControlTag))
                        objChild.Text = GetString(strControlTag, strIntoLanguage);
                }
                else if (objChild is ToolStrip tssStrip)
                {
                    foreach (ToolStripItem tssItem in tssStrip.Items)
                    {
                        TranslateToolStripItemsRecursively(tssItem, strIntoLanguage);
                    }
                }
                else if (objChild is ListView lstList)
                {
                    foreach (ColumnHeader objHeader in lstList.Columns)
                    {
                        string strControlTag = objHeader.Tag?.ToString();
                        if (!string.IsNullOrEmpty(strControlTag))
                            objHeader.Text = GetString(strControlTag, strIntoLanguage);
                    }
                }
                else if (objChild is TabControl objTabControl)
                {
                    foreach (TabPage tabPage in objTabControl.TabPages)
                    {
                        string strControlTag = tabPage.Tag?.ToString();
                        if (!string.IsNullOrEmpty(strControlTag))
                            tabPage.Text = GetString(strControlTag, strIntoLanguage);

                        UpdateControls(tabPage, strIntoLanguage);
                    }
                }
                else if (objChild is SplitContainer objSplitControl)
                {
                    UpdateControls(objSplitControl.Panel1, strIntoLanguage);
                    UpdateControls(objSplitControl.Panel2, strIntoLanguage);
                }
                else if (objChild as Panel != null)
                {
                    UpdateControls(objChild, strIntoLanguage);
                }
                else if (objChild is TreeView treTree)
                {
                    foreach (TreeNode objNode in treTree.Nodes)
                    {
                        if (objNode.Level == 0)
                        {
                            string strControlTag = objNode.Tag?.ToString();
                            if (!string.IsNullOrEmpty(strControlTag))
                            {
                                if (strControlTag.StartsWith("Node_"))
                                {
                                    objNode.Text = GetString(strControlTag, strIntoLanguage);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads the proper language from the language file for every menu item recursively
        /// </summary>
        /// <param name="tssItem"></param>
        public static void TranslateToolStripItemsRecursively(ToolStripItem tssItem, string strIntoLanguage)
        {
            string strControlTag = tssItem.Tag?.ToString();
            if (!string.IsNullOrEmpty(strControlTag))
                tssItem.Text = GetString(strControlTag, strIntoLanguage);

            if (tssItem is ToolStripDropDownItem tssDropDownItem)
                foreach (ToolStripItem tssDropDownChild in tssDropDownItem.DropDownItems)
                    TranslateToolStripItemsRecursively(tssDropDownChild, strIntoLanguage);
        }

        /// <summary>
        /// Retrieve a string from the language file.
        /// </summary>
        /// <param name="strKey">Key to retrieve.</param>
        /// <param name="blnReturnError">Should an error string be returned if the key isn't found?</param>
        public static string GetString(string strKey, string strLanguage, bool blnReturnError = true)
        {
            string strReturn;
            if (s_DictionaryLanguages.TryGetValue(strLanguage, out LanguageData objLanguageData))
            {
                if (objLanguageData.TranslatedStrings.TryGetValue(strKey, out strReturn))
                {
                    return strReturn;
                }
            }
            if (s_DictionaryEnglishStrings.TryGetValue(strKey, out strReturn))
            {
                return strReturn;
            }
            if (!blnReturnError)
            {
                return string.Empty;
            }
            return "Error finding string for key - " + strKey;
        }

        /// <summary>
        /// Retrieve a string from the language file.
        /// </summary>
        /// <param name="strKey">Key to retrieve.</param>
        /// <param name="blnReturnError">Should an error string be returned if the key isn't found?</param>
        public static XmlDocument GetDataDocument(string strLanguage)
        {
            if (s_DictionaryLanguages.TryGetValue(strLanguage, out LanguageData objLanguageData))
            {
                return objLanguageData.DataDocument;
            }
            return null;
        }

        /// <summary>
        /// Check the Keys in the selected language file against the English version. 
        /// </summary>
        /// <param name="strLanguage">Language to check.</param>
        public static void VerifyStrings(string strLanguage)
        {
            ConcurrentBag<LanguageString> lstEnglish = new ConcurrentBag<LanguageString>();
            ConcurrentBag<LanguageString> lstLanguage = new ConcurrentBag<LanguageString>();
            Parallel.Invoke(
                () =>
                {
                    // Load the English version.
                    XmlDocument objEnglishDocument = new XmlDocument();
                    string strFilePath = Path.Combine(Application.StartupPath, "lang", GlobalOptions.DefaultLanguage + ".xml");
                    objEnglishDocument.Load(strFilePath);
                    foreach (XmlNode objNode in objEnglishDocument.SelectNodes("/chummer/strings/string"))
                    {
                        lstEnglish.Add(new LanguageString(objNode["key"]?.InnerText, objNode["text"]?.InnerText));
                    }
                },
                () =>
                {
                    // Load the selected language version.
                    XmlDocument objLanguageDocument = new XmlDocument();
                    string strLangPath = Path.Combine(Application.StartupPath, "lang", strLanguage + ".xml");
                    objLanguageDocument.Load(strLangPath);
                    foreach (XmlNode objNode in objLanguageDocument.SelectNodes("/chummer/strings/string"))
                    {
                        lstLanguage.Add(new LanguageString(objNode["key"]?.InnerText, objNode["text"]?.InnerText));
                    }
                }
            );

            StringBuilder objMissingMessage = new StringBuilder();
            StringBuilder objUnusedMessage = new StringBuilder();
            Parallel.Invoke(
                () =>
                {
                    // Check for strings that are in the English file but not in the selected language file.
                    foreach (LanguageString objString in lstEnglish)
                    {
                        if (!lstLanguage.Any(objItem => objItem.Key == objString.Key))
                            objMissingMessage.Append("\nMissing String: " + objString.Key);
                    }
                },
                () =>
                {
                    // Check for strings that are not in the English file but are in the selected language file (someone has put in Keys that they shouldn't have which are ignored).
                    foreach (LanguageString objString in lstLanguage)
                    {
                        if (!lstEnglish.Any(objItem => objItem.Key == objString.Key))
                            objUnusedMessage.Append("\nUnused String: " + objString.Key);
                    }
                }
            );

            string strMessage = objMissingMessage.ToString() + objUnusedMessage.ToString();
            // Display the message.
            if (!string.IsNullOrEmpty(strMessage))
                MessageBox.Show(strMessage, "Language File Contents", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("Language file is OK.", "Language File Contents", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // List of XPaths to search for extras. Item1 is Document, Item2 is XPath, Item3 is the Name getter, Item4 is the Translate getter
        private static readonly Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>[] s_LstXPathsToSearch =
        {
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("weapons.xml", "/chummer/categories/category",
                new Func<XmlNode, string>(x => x.InnerText), new Func<XmlNode, string>(x => x.Attributes?["translate"]?.InnerText)),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("spells.xml", "/chummer/categories/category",
                new Func<XmlNode, string>(x => x.InnerText), new Func<XmlNode, string>(x => x.Attributes?["translate"]?.InnerText)),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("programs.xml", "/chummer/categories/category",
                new Func<XmlNode, string>(x => x.InnerText), new Func<XmlNode, string>(x => x.Attributes?["translate"]?.InnerText)),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("skills.xml", "/chummer/skills/skill/specs/spec",
                new Func<XmlNode, string>(x => x.InnerText), new Func<XmlNode, string>(x => x.Attributes?["translate"]?.InnerText)),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("skills.xml", "/chummer/skillgroups/name",
                new Func<XmlNode, string>(x => x.InnerText), new Func<XmlNode, string>(x => x.Attributes?["translate"]?.InnerText)),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("skills.xml", "/chummer/categories/category",
                new Func<XmlNode, string>(x => x.InnerText), new Func<XmlNode, string>(x => x.Attributes?["translate"]?.InnerText)),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("licenses.xml", "/chummer/licenses/license",
                new Func<XmlNode, string>(x => x.InnerText), new Func<XmlNode, string>(x => x.Attributes?["translate"]?.InnerText)),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("weapons.xml", "/chummer/weapons/weapon",
                new Func<XmlNode, string>(x => x["name"]?.InnerText), new Func<XmlNode, string>(x => x["translate"]?.InnerText)),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("skills.xml", "/chummer/skills/skill",
                new Func<XmlNode, string>(x => x["name"]?.InnerText), new Func<XmlNode, string>(x => x["translate"]?.InnerText)),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("mentors.xml", "/chummer/mentors/mentor",
                new Func<XmlNode, string>(x => x["name"]?.InnerText), new Func<XmlNode, string>(x => x["translate"]?.InnerText)),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("mentors.xml", "/chummer/mentors/mentor/choices/choice",
                new Func<XmlNode, string>(x => x["name"]?.InnerText), new Func<XmlNode, string>(x => x["translate"]?.InnerText)),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("armor.xml", "/chummer/armors/armor",
                new Func<XmlNode, string>(x => x["name"]?.InnerText), new Func<XmlNode, string>(x => x["translate"]?.InnerText)),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("armor.xml", "/chummer/mods/mod",
                new Func<XmlNode, string>(x => x["name"]?.InnerText), new Func<XmlNode, string>(x => x["translate"]?.InnerText)),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("spells.xml", "/chummer/spells/spell",
                new Func<XmlNode, string>(x => x["name"]?.InnerText), new Func<XmlNode, string>(x => x["translate"]?.InnerText)),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("programs.xml", "/chummer/programs/program",
                new Func<XmlNode, string>(x => x["name"]?.InnerText), new Func<XmlNode, string>(x => x["translate"]?.InnerText)),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("powers.xml", "/chummer/powers/power",
                new Func<XmlNode, string>(x => x["name"]?.InnerText), new Func<XmlNode, string>(x => x["translate"]?.InnerText)),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("metamagic.xml", "/chummer/metamagics/metamagic",
                new Func<XmlNode, string>(x => x["name"]?.InnerText), new Func<XmlNode, string>(x => x["translate"]?.InnerText)),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("echoes.xml", "/chummer/echoes/echo",
                new Func<XmlNode, string>(x => x["name"]?.InnerText), new Func<XmlNode, string>(x => x["translate"]?.InnerText)),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("metatypes.xml", "/chummer/metatypes/metatype",
                new Func<XmlNode, string>(x => x["name"]?.InnerText), new Func<XmlNode, string>(x => x["translate"]?.InnerText)),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("cyberware.xml", "/chummer/cyberwares/cyberware",
                new Func<XmlNode, string>(x => x["name"]?.InnerText), new Func<XmlNode, string>(x => x["translate"]?.InnerText)),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("critterpowers.xml", "/chummer/powers/power",
                new Func<XmlNode, string>(x => x["name"]?.InnerText), new Func<XmlNode, string>(x => x["translate"]?.InnerText)),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("qualities.xml", "/chummer/qualities/quality",
                new Func<XmlNode, string>(x => x["name"]?.InnerText), new Func<XmlNode, string>(x => x["translate"]?.InnerText)),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("ranges.xml", "/chummer/ranges/range",
                new Func<XmlNode, string>(x => x["name"]?.InnerText), new Func<XmlNode, string>(x => x["translate"]?.InnerText)),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("paragons.xml", "/chummer/mentors/mentor",
                new Func<XmlNode, string>(x => x["name"]?.InnerText), new Func<XmlNode, string>(x => x["translate"]?.InnerText)),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("paragons.xml", "/chummer/mentors/mentor/choices/choice",
                new Func<XmlNode, string>(x => x["name"]?.InnerText), new Func<XmlNode, string>(x => x["translate"]?.InnerText)),
        };
        /// <summary>
        /// Attempt to translate any Extra text for an item.
        /// </summary>
        /// <param name="strExtra">Extra string to translate.</param>
        public static string TranslateExtra(string strExtra, string strLanguage)
        {
            string strReturn = string.Empty;

            // Only attempt to translate if we're not using English. Don't attempt to translate an empty string either.
            if (strLanguage != GlobalOptions.DefaultLanguage && !string.IsNullOrWhiteSpace(strExtra))
            {
                // Attempt to translate CharacterAttribute names.
                switch (strExtra)
                {
                    case "BOD":
                        strReturn = GetString("String_AttributeBODShort", strLanguage);
                        break;
                    case "AGI":
                        strReturn = GetString("String_AttributeAGIShort", strLanguage);
                        break;
                    case "REA":
                        strReturn = GetString("String_AttributeREAShort", strLanguage);
                        break;
                    case "STR":
                        strReturn = GetString("String_AttributeSTRShort", strLanguage);
                        break;
                    case "CHA":
                        strReturn = GetString("String_AttributeCHAShort", strLanguage);
                        break;
                    case "INT":
                        strReturn = GetString("String_AttributeINTShort", strLanguage);
                        break;
                    case "LOG":
                        strReturn = GetString("String_AttributeLOGShort", strLanguage);
                        break;
                    case "WIL":
                        strReturn = GetString("String_AttributeWILShort", strLanguage);
                        break;
                    case "EDG":
                        strReturn = GetString("String_AttributeEDGShort", strLanguage);
                        break;
                    case "MAG":
                        strReturn = GetString("String_AttributeMAGShort", strLanguage);
                        break;
                    case "MAGAdept":
                        strReturn = GetString("String_AttributeMAGShort", strLanguage) + " (" + GetString("String_DescAdept", strLanguage) + ")";
                        break;
                    case "RES":
                        strReturn = GetString("String_AttributeRESShort", strLanguage);
                        break;
                    case "DEP":
                        strReturn = GetString("String_AttributeDEPShort", strLanguage);
                        break;
                    case "Physical":
                        strReturn = GetString("Node_Physical", strLanguage);
                        break;
                    case "Mental":
                        strReturn = GetString("Node_Mental", strLanguage);
                        break;
                    case "Social":
                        strReturn = GetString("Node_Social", strLanguage);
                        break;
                    case "Left":
                        strReturn = GetString("String_Improvement_SideLeft", strLanguage);
                        break;
                    case "Right":
                        strReturn = GetString("String_Improvement_SideRight", strLanguage);
                        break;
                    default:
                        string strExtraNoQuotes = strExtra.FastEscape('\"');

                        object strReturnLock = new object();
                        Parallel.For(0, s_LstXPathsToSearch.Length, (i, state) =>
                        {
                            Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>> objXPathPair = s_LstXPathsToSearch[i];
                            foreach (XmlNode objNode in XmlManager.Load(objXPathPair.Item1).SelectNodes(objXPathPair.Item2))
                            {
                                if (objXPathPair.Item3(objNode) == strExtraNoQuotes)
                                {
                                    string strTranslate = objXPathPair.Item4(objNode);
                                    if (!string.IsNullOrEmpty(strTranslate))
                                    {
                                        lock (strReturnLock)
                                            strReturn = strTranslate;
                                        state.Stop();
                                        break;
                                    }
                                }
                            }
                        });
                        break;
                }
            }

            // If no translation could be found, just use whatever we were passed.
            if (string.IsNullOrEmpty(strReturn) || strReturn.Contains("Error finding string for key - "))
                strReturn = strExtra;

            return strReturn;
        }
        #endregion
    }
}
