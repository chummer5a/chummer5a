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

        private static string s_StrLanguage = GlobalOptions.DefaultLanguage;
        private static readonly Dictionary<string, string> s_DictionaryTranslatedStrings = new Dictionary<string, string>();
        private static readonly XmlDocument s_XmlDataDocument = new XmlDocument();

        #region Constructor
        static LanguageManager()
        {
            if (!Utils.IsRunningInVisualStudio)
            {
                XmlDocument objEnglishDocument = new XmlDocument();
                string strFilePath = Path.Combine(Application.StartupPath, "lang", "en-us.xml");
                if (File.Exists(strFilePath))
                {
                    objEnglishDocument.Load(strFilePath);
                    foreach (XmlNode objNode in objEnglishDocument.SelectNodes("/chummer/strings/string"))
                    {
                        string strKey = objNode["key"]?.InnerText;
                        string strText = objNode["text"]?.InnerText;
                        if (!string.IsNullOrEmpty(strKey) && !string.IsNullOrEmpty(strText))
                        {
                            if (s_DictionaryTranslatedStrings.ContainsKey(strKey))
                                Utils.BreakIfDebug();
                            else
                                s_DictionaryTranslatedStrings.Add(strKey, strText.Replace("\\n", "\n"));
                        }
                    }
                }
                else
                    MessageBox.Show("Language strings for the default language (en-us) could not be loaded.", "Cannot Load Language", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// XmlDocument that holds item name translations.
        /// </summary>
        public static XmlDocument DataDoc
        {
            get
            {
                return s_XmlDataDocument;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Load the selected XML file and its associated custom file.
        /// </summary>
        /// <param name="strLanguage">Language to Load.</param>
        /// <param name="objObject">Object to translate after loading the data.</param>
        public static void Load(string strLanguage, object objObject)
        {
            // _strLanguage is populated when the language is read for the first time, meaning this is only triggered once (and language is only read in once since it shouldn't change).
            if (string.IsNullOrEmpty(s_StrLanguage) && strLanguage != GlobalOptions.DefaultLanguage)
            {
                s_StrLanguage = strLanguage;
                XmlDocument objLanguageDocument = new XmlDocument();
                string strFilePath = Path.Combine(Application.StartupPath, "lang", strLanguage + ".xml");
                if (File.Exists(strFilePath))
                {
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
                                if (s_DictionaryTranslatedStrings.ContainsKey(strKey))
                                    s_DictionaryTranslatedStrings[strKey] = strText.Replace("\\n", "\n");
                                else
                                    s_DictionaryTranslatedStrings.Add(strKey, strText.Replace("\\n", "\n"));
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Language file " + strFilePath + " could not be loaded.", "Cannot Load Language", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("Language file " + strFilePath + " could not be loaded.", "Cannot Load Language", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Check to see if the data translation file for the selected language exists.
                string strDataPath = Path.Combine(Application.StartupPath, "lang", strLanguage + "_data.xml");
                if (File.Exists(strDataPath))
                {
                    s_XmlDataDocument.Load(strDataPath);
                }
            }

            // If the object is a Form, call the UpdateForm method to provide its translations.
            if (objObject is Form objForm)
                UpdateForm(objForm);

            // If the object is a UserControl, call the UpdateUserControl method to provide its translations.
            if (objObject is UserControl objUserControl)
                UpdateUserControl(objUserControl);
        }

        /// <summary>
        /// Recursive method to translate all of the controls in a Form or UserControl.
        /// </summary>
        /// <param name="objParent">Control container to translate.</param>
        private static void UpdateControls(Control objParent)
        {
            if (objParent == null)
                return;
            // Translatable items are identified by having a value in their Tag attribute. The contents of Tag is the string to lookup in the language list.

            foreach (Label lblLabel in objParent.Controls.OfType<Label>())
            {
                if (!string.IsNullOrEmpty(lblLabel.Tag?.ToString()))
                {
                    lblLabel.Text = GetString(lblLabel.Tag.ToString());
                }
            }
            foreach (Button cmdButton in objParent.Controls.OfType<Button>())
            {
                if (!string.IsNullOrEmpty(cmdButton.Tag?.ToString()))
                {
                    cmdButton.Text = GetString(cmdButton.Tag.ToString());
                }
            }
            foreach (CheckBox chkCheckbox in objParent.Controls.OfType<CheckBox>())
            {
                if (!string.IsNullOrEmpty(chkCheckbox.Tag?.ToString()))
                {
                    chkCheckbox.Text = GetString(chkCheckbox.Tag.ToString());
                }
            }
            foreach (ListView lstList in objParent.Controls.OfType<ListView>())
            {
                foreach (ColumnHeader objHeader in lstList.Columns)
                {
                    if (!string.IsNullOrEmpty(objHeader.Tag?.ToString()))
                    {
                        objHeader.Text = GetString(objHeader.Tag.ToString());
                    }
                }
            }

            // Run through any Panels on the container.
            foreach (Panel objPanel in objParent.Controls.OfType<Panel>())
            {
                UpdateControls(objPanel);
            }

            // Run through any Tabs on the container.
            foreach (TabControl objTabControl in objParent.Controls.OfType<TabControl>())
            {
                foreach (TabPage tabPage in objTabControl.TabPages)
                {
                    if (!string.IsNullOrEmpty(tabPage.Tag?.ToString()))
                    {
                        tabPage.Text = GetString(tabPage.Tag.ToString());
                    }

                    UpdateControls(tabPage);
                }
            }

            // Run through everything in any SplitContainers.
            foreach (SplitContainer objSplitControl in objParent.Controls.OfType<SplitContainer>())
            {
                for (int i = 1; i <= 2; i++)
                {
                    SplitterPanel objSplitPanel;
                    if (i == 1)
                        objSplitPanel = objSplitControl.Panel1;
                    else
                        objSplitPanel = objSplitControl.Panel2;

                    UpdateControls(objSplitPanel);
                }
            }

            // Run through any FlowLayoutPanels on the container.
            foreach (FlowLayoutPanel objContainer in objParent.Controls.OfType<FlowLayoutPanel>())
            {
                UpdateControls(objContainer);
            }

            foreach (TreeView treTree in objParent.Controls.OfType<TreeView>())
            {
                foreach (TreeNode objNode in treTree.Nodes)
                {
                    if (objNode.Level == 0)
                    {
                        if (objNode.Tag != null)
                        {
                            if (objNode.Tag.ToString().StartsWith("Node_"))
                            {
                                objNode.Text = GetString(objNode.Tag.ToString());
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Translate the contents of a UserControl.
        /// </summary>
        /// <param name="objControl">UserControl to translate.</param>
        private static void UpdateUserControl(UserControl objControl)
        {
            UpdateControls(objControl);
        }

        /// <summary>
        /// Translate the contents of a Form.
        /// </summary>
        /// <param name="objForm">Form to translate.</param>
        private static void UpdateForm(Form objForm)
        {
            // Translatable items are identified by having a value in their Tag attribute. The contents of Tag is the string to lookup in the language list.
            // Update the Form itself.
            if (objForm.Tag != null)
            {
                objForm.Text = GetString(objForm.Tag.ToString());
            }

            // update any menu strip items that have tags
            if (objForm.MainMenuStrip != null)
            {
                foreach (ToolStripMenuItem objItem in objForm.MainMenuStrip.Items)
                    SetMenuItemsRecursively(objItem);
            }

            // Run through any StatusStrips.
            foreach (ToolStrip objStrip in objForm.Controls.OfType<ToolStrip>())
            {
                foreach (ToolStripStatusLabel tssLabel in objStrip.Items.OfType<ToolStripStatusLabel>())
                {
                    if (tssLabel.Tag != null)
                        tssLabel.Text = GetString(tssLabel.Tag.ToString());
                }
            }

            // Handle control over to the method that handles translating all of the other Controls.
            UpdateControls(objForm);
        }

        /// <summary>
        /// Loads the proper language from the language file for every menu item recursively
        /// </summary>
        /// <param name="objItem"></param>
        private static void SetMenuItemsRecursively(ToolStripDropDownItem objItem)
        {
            if (objItem.DropDownItems.Count == 0)
                return; // we have no more drop down items to pull
            if (objItem.Tag != null)
                objItem.Text = GetString(objItem.Tag.ToString());

            foreach (ToolStripDropDownItem objRecursiveItem in objItem.DropDownItems.OfType<ToolStripDropDownItem>())
            {
                SetMenuItemsRecursively(objRecursiveItem);
                if (objItem.Tag != null)
                    objItem.Text = GetString(objItem.Tag.ToString());
            }
        }

        /// <summary>
        /// Retrieve a string from the language file.
        /// </summary>
        /// <param name="strKey">Key to retrieve.</param>
        /// <param name="blnReturnError">Should an error string be returned if the key isn't found?</param>
        public static string GetString(string strKey, bool blnReturnError = true)
        {
            if (s_DictionaryTranslatedStrings.TryGetValue(strKey, out string strReturn))
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
                    string strFilePath = Path.Combine(Application.StartupPath, "lang", "en-us.xml");
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
        public static string TranslateExtra(string strExtra)
        {
            string strReturn = string.Empty;

            // Only attempt to translate if we're not using English. Don't attempt to translate an empty string either.
            if (s_StrLanguage != GlobalOptions.DefaultLanguage && !string.IsNullOrWhiteSpace(strExtra))
            {
                // Attempt to translate CharacterAttribute names.
                switch (strExtra)
                {
                    case "BOD":
                        strReturn = GetString("String_AttributeBODShort");
                        break;
                    case "AGI":
                        strReturn = GetString("String_AttributeAGIShort");
                        break;
                    case "REA":
                        strReturn = GetString("String_AttributeREAShort");
                        break;
                    case "STR":
                        strReturn = GetString("String_AttributeSTRShort");
                        break;
                    case "CHA":
                        strReturn = GetString("String_AttributeCHAShort");
                        break;
                    case "INT":
                        strReturn = GetString("String_AttributeINTShort");
                        break;
                    case "LOG":
                        strReturn = GetString("String_AttributeLOGShort");
                        break;
                    case "WIL":
                        strReturn = GetString("String_AttributeWILShort");
                        break;
                    case "EDG":
                        strReturn = GetString("String_AttributeEDGShort");
                        break;
                    case "MAG":
                        strReturn = GetString("String_AttributeMAGShort");
                        break;
                    case "MAGAdept":
                        strReturn = GetString("String_AttributeMAGShort") + " (" + GetString("String_DescAdept") + ")";
                        break;
                    case "RES":
                        strReturn = GetString("String_AttributeRESShort");
                        break;
                    case "DEP":
                        strReturn = GetString("String_AttributeDEPShort");
                        break;
                    case "Physical":
                        strReturn = GetString("Node_Physical");
                        break;
                    case "Mental":
                        strReturn = GetString("Node_Mental");
                        break;
                    case "Social":
                        strReturn = GetString("Node_Social");
                        break;
                    case "Left":
                        strReturn = GetString("String_Improvement_SideLeft");
                        break;
                    case "Right":
                        strReturn = GetString("String_Improvement_SideRight");
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
