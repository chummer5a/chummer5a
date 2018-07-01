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
        public class LanguageData
        {
            public IDictionary<string, string> TranslatedStrings { get; } = new Dictionary<string, string>();
            public XmlDocument DataDocument { get; } = new XmlDocument();
            public string ErrorMessage { get; } = string.Empty;
            public bool ErrorAlreadyShown { get; set; }

            public LanguageData(string strLanguage)
            {
                string strFilePath = Path.Combine(Application.StartupPath, "lang", strLanguage + ".xml");
                if (File.Exists(strFilePath))
                {
                    XmlDocument objLanguageDocument = new XmlDocument();
                    try
                    {
                        string strExtraMessage = string.Empty;
                        try
                        {
                            using (StreamReader objStreamReader = new StreamReader(strFilePath, Encoding.UTF8, true))
                            {
                                objLanguageDocument.Load(objStreamReader);
                            }
                        }
                        catch (IOException ex)
                        {
                            objLanguageDocument = null;
                            strExtraMessage += ex.ToString();
                        }
                        catch (XmlException ex)
                        {
                            objLanguageDocument = null;
                            strExtraMessage += ex.ToString();
                        }

                        if (objLanguageDocument != null)
                        {
                            using (XmlNodeList xmlStringList = objLanguageDocument.SelectNodes("/chummer/strings/string"))
                                if (xmlStringList != null)
                                {
                                    foreach (XmlNode objNode in xmlStringList)
                                    {
                                        // Look for the English version of the found string. If it has been found, replace the English contents with the contents from this file.
                                        // If the string was not found, then someone has inserted a Key that should not exist and is ignored.
                                        string strKey = objNode["key"]?.InnerText;
                                        string strText = objNode["text"]?.InnerText;
                                        if (!string.IsNullOrEmpty(strKey) && !string.IsNullOrEmpty(strText))
                                        {
                                            if (TranslatedStrings.ContainsKey(strKey))
                                                TranslatedStrings[strKey] = strText.Replace("\\n\\r", Environment.NewLine).Replace("\\n", Environment.NewLine);
                                            else
                                                TranslatedStrings.Add(strKey, strText.Replace("\\n\\r", Environment.NewLine).Replace("\\n", Environment.NewLine));
                                        }
                                    }
                                }
                                else
                                {
                                    ErrorMessage += "Failed to load the strings file " + strLanguage + ".xml into an XmlDocument: " + strExtraMessage + "." + Environment.NewLine;
                                }
                        }
                        else
                        {
                            ErrorMessage += "Failed to load the strings file " + strLanguage + ".xml into an XmlDocument: " + strExtraMessage + "." + Environment.NewLine;
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage += "Encountered the following the exception while loading " + strLanguage + ".xml into an XmlDocument: " + ex.ToString() + "." + Environment.NewLine;
                    }
                }
                else
                {
                    ErrorMessage += "Could not find the strings file " + strLanguage + ".xml." + Environment.NewLine;
                }

                // Check to see if the data translation file for the selected language exists.
                string strDataPath = Path.Combine(Application.StartupPath, "lang", strLanguage + "_data.xml");
                if (File.Exists(strDataPath))
                {
                    try
                    {
                        try
                        {
                            using (StreamReader objStreamReader = new StreamReader(strDataPath, Encoding.UTF8, true))
                            {
                                DataDocument.Load(objStreamReader);
                            }
                        }
                        catch (IOException ex)
                        {
                            DataDocument = null;
                            ErrorMessage += "Failed to load the data file " + strLanguage + "_data.xml into an XmlDocument: " + ex.ToString() + "." + Environment.NewLine;
                        }
                        catch (XmlException ex)
                        {
                            DataDocument = null;
                            ErrorMessage += "Failed to load the data file " + strLanguage + "_data.xml into an XmlDocument: " + ex.ToString() + "." + Environment.NewLine;
                        }
                    }
                    catch (Exception ex)
                    {
                        DataDocument = null;
                        ErrorMessage += "Encountered the following the exception while loading " + strLanguage + "_data.xml into an XmlDocument: " + ex.ToString() + "." + Environment.NewLine;
                    }
                }
                else
                {
                    ErrorMessage += "Could not find the data file " + strLanguage + "_data.xml." + Environment.NewLine;
                }
            }
        }

        private static readonly Dictionary<string, LanguageData> s_DictionaryLanguages = new Dictionary<string, LanguageData>();
        public static IReadOnlyDictionary<string, LanguageData> DictionaryLanguages => s_DictionaryLanguages;
        private static readonly Dictionary<string, string> s_DictionaryEnglishStrings = new Dictionary<string, string>();
        public static string ManagerErrorMessage { get; } = string.Empty;

        #region Constructor
        static LanguageManager()
        {
            if (!Utils.IsRunningInVisualStudio)
            {
                XmlDocument objEnglishDocument = new XmlDocument();
                string strFilePath = Path.Combine(Application.StartupPath, "lang", GlobalOptions.DefaultLanguage + ".xml");
                if (File.Exists(strFilePath))
                {
                    try
                    {
                        using (StreamReader objStreamReader = new StreamReader(strFilePath, Encoding.UTF8, true))
                        {
                            objEnglishDocument.Load(objStreamReader);
                        }
                    }
                    catch (IOException ex)
                    {
                        ManagerErrorMessage += "Language strings for the default language (" + GlobalOptions.DefaultLanguage + ") could not be loaded:" + Environment.NewLine + Environment.NewLine + ex.ToString();
                    }
                    catch (XmlException ex)
                    {
                        ManagerErrorMessage += "Language strings for the default language (" + GlobalOptions.DefaultLanguage + ") could not be loaded:" + Environment.NewLine + Environment.NewLine + ex.ToString();
                    }

                    if (string.IsNullOrEmpty(ManagerErrorMessage))
                    {
                        using (XmlNodeList xmlStringList = objEnglishDocument.SelectNodes("/chummer/strings/string"))
                            if (xmlStringList != null)
                            {
                                foreach (XmlNode objNode in xmlStringList)
                                {
                                    string strKey = objNode["key"]?.InnerText;
                                    string strText = objNode["text"]?.InnerText;
                                    if (!string.IsNullOrEmpty(strKey) && !string.IsNullOrEmpty(strText))
                                    {
                                        if (s_DictionaryEnglishStrings.ContainsKey(strKey))
                                            Utils.BreakIfDebug();
                                        else
                                            s_DictionaryEnglishStrings.Add(strKey, strText.Replace("\\n\\r", Environment.NewLine).Replace("\\n", Environment.NewLine));
                                    }
                                }
                            }
                            else
                            {
                                ManagerErrorMessage += "Language strings for the default language (" + GlobalOptions.DefaultLanguage + ") could not be loaded:" +
                                                       Environment.NewLine + Environment.NewLine + "No strings found in file.";
                            }
                    }
                }
                else
                    ManagerErrorMessage += "Language strings for the default language (" + GlobalOptions.DefaultLanguage + ") could not be loaded:" +
                                           Environment.NewLine + Environment.NewLine + "File " + strFilePath + " does not exist or cannot be found.";
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Translate an object int a specified language.
        /// </summary>
        /// <param name="strIntoLanguage">Language to which to translate the object.</param>
        /// <param name="objObject">Object to translate.</param>
        public static void TranslateWinForm(string strIntoLanguage, Control objObject)
        {
            if (LoadLanguage(strIntoLanguage))
                UpdateControls(objObject, strIntoLanguage);
            else if (strIntoLanguage != GlobalOptions.DefaultLanguage)
                UpdateControls(objObject, GlobalOptions.DefaultLanguage);
        }

        private static bool LoadLanguage(string strLanguage)
        {
            if (strLanguage != GlobalOptions.DefaultLanguage)
            {
                if (!s_DictionaryLanguages.TryGetValue(strLanguage, out LanguageData objNewLanguage))
                {
                    objNewLanguage = new LanguageData(strLanguage);
                    s_DictionaryLanguages.Add(strLanguage, objNewLanguage);
                }
                if (!string.IsNullOrEmpty(objNewLanguage.ErrorMessage))
                {
                    if (!objNewLanguage.ErrorAlreadyShown)
                    {
                        MessageBox.Show("Language with code " + strLanguage + " could not be loaded for the following reasons:" + Environment.NewLine + Environment.NewLine + objNewLanguage.ErrorMessage, "Cannot Load Language", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        objNewLanguage.ErrorAlreadyShown = true;
                    }
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Recursive method to translate all of the controls in a Form or UserControl.
        /// </summary>
        /// <param name="strIntoLanguage">Language into which the control should be translated</param>
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
                if (!string.IsNullOrEmpty(strControlTag) && !int.TryParse(strControlTag, out int _) && !strControlTag.IsGuid())
                    frmForm.Text = GetString(strControlTag, strIntoLanguage);
                else if (frmForm.Text.StartsWith('['))
                    frmForm.Text = string.Empty;

                // update any menu strip items that have tags
                if (frmForm.MainMenuStrip != null)
                    foreach (ToolStripMenuItem tssItem in frmForm.MainMenuStrip.Items)
                        TranslateToolStripItemsRecursively(tssItem, strIntoLanguage);
            }

            // Translatable items are identified by having a value in their Tag attribute. The contents of Tag is the string to lookup in the language list.
            foreach (Control objChild in objParent.Controls)
            {
                if (objChild is Label || objChild is Button || objChild is CheckBox)
                {
                    string strControlTag = objChild.Tag?.ToString();
                    if (!string.IsNullOrEmpty(strControlTag) && !int.TryParse(strControlTag, out int _) && !strControlTag.IsGuid())
                        objChild.Text = GetString(strControlTag, strIntoLanguage);
                    else if (objChild.Text.StartsWith('['))
                        objChild.Text = string.Empty;
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
                        if (!string.IsNullOrEmpty(strControlTag) && !int.TryParse(strControlTag, out int _) && !strControlTag.IsGuid())
                            objHeader.Text = GetString(strControlTag, strIntoLanguage);
                        else if (objHeader.Text.StartsWith('['))
                            objHeader.Text = string.Empty;
                    }
                }
                else if (objChild is TabControl objTabControl)
                {
                    foreach (TabPage tabPage in objTabControl.TabPages)
                    {
                        string strControlTag = tabPage.Tag?.ToString();
                        if (!string.IsNullOrEmpty(strControlTag) && !int.TryParse(strControlTag, out int _) && !strControlTag.IsGuid())
                            tabPage.Text = GetString(strControlTag, strIntoLanguage);
                        else if (tabPage.Text.StartsWith('['))
                            tabPage.Text = string.Empty;

                        UpdateControls(tabPage, strIntoLanguage);
                    }
                }
                else if (objChild is SplitContainer objSplitControl)
                {
                    UpdateControls(objSplitControl.Panel1, strIntoLanguage);
                    UpdateControls(objSplitControl.Panel2, strIntoLanguage);
                }
                else if (objChild is Panel)
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
                            if (!string.IsNullOrEmpty(strControlTag) && strControlTag.StartsWith("Node_"))
                            {
                                objNode.Text = GetString(strControlTag, strIntoLanguage);
                            }
                            else if (objNode.Text.StartsWith('['))
                                objNode.Text = string.Empty;
                        }
                        else if (objNode.Text.StartsWith('['))
                            objNode.Text = string.Empty;
                    }
                }
                else if (objChild is DataGridView)
                {
                    foreach (DataGridViewTextBoxColumn objColumn in ((DataGridView) objChild).Columns)
                    {
                        if (objColumn is DataGridViewTextBoxColumnTranslated objTranslatedColumn && !string.IsNullOrWhiteSpace(objTranslatedColumn.TranslationTag))
                        {
                            objColumn.HeaderText = GetString(objTranslatedColumn.TranslationTag, strIntoLanguage);
                        }
                    }
                }
                else if (objChild is ITranslatable translatable)
                {
                    // let custom nodes determine how they want to be translated
                    translatable.Translate();
                }
            }
        }

        /// <summary>
        /// Loads the proper language from the language file for every menu item recursively
        /// </summary>
        /// <param name="tssItem">Given ToolStripItem to translate.</param>
        /// <param name="strIntoLanguage">Language into which the ToolStripItem and all dropdown items should be translated.</param>
        public static void TranslateToolStripItemsRecursively(ToolStripItem tssItem, string strIntoLanguage)
        {
            string strControlTag = tssItem.Tag?.ToString();
            if (!string.IsNullOrEmpty(strControlTag) && !int.TryParse(strControlTag, out int _) && !strControlTag.IsGuid())
                tssItem.Text = GetString(strControlTag, strIntoLanguage);
            else if (tssItem.Text.StartsWith('['))
                tssItem.Text = string.Empty;

            if (tssItem is ToolStripDropDownItem tssDropDownItem)
                foreach (ToolStripItem tssDropDownChild in tssDropDownItem.DropDownItems)
                    TranslateToolStripItemsRecursively(tssDropDownChild, strIntoLanguage);
        }

        /// <summary>
        /// Retrieve a string from the language file.
        /// </summary>
        /// <param name="strKey">Key to retrieve.</param>
        /// <param name="strLanguage">Language in from which the string should be retrieved.</param>
        /// <param name="blnReturnError">Should an error string be returned if the key isn't found?</param>
        public static string GetString(string strKey, string strLanguage, bool blnReturnError = true)
        {
            string strReturn;
            if (LoadLanguage(strLanguage))
            {
                if (DictionaryLanguages.TryGetValue(strLanguage, out LanguageData objLanguageData))
                {
                    if (objLanguageData.TranslatedStrings.TryGetValue(strKey, out strReturn))
                    {
                        return strReturn;
                    }
                }
            }
            if (s_DictionaryEnglishStrings.TryGetValue(strKey, out strReturn))
            {
                return strReturn;
            }
            return !blnReturnError ? string.Empty : $"{strKey} not found; check language file for string";
        }

        /// <summary>
        /// Retrieve a string from the language file.
        /// </summary>
        /// <param name="strLanguage">Language whose document should be retrieved.</param>
        public static XmlDocument GetDataDocument(string strLanguage)
        {
            if (LoadLanguage(strLanguage) && DictionaryLanguages.TryGetValue(strLanguage, out LanguageData objLanguageData))
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
            ConcurrentBag<string> lstEnglish = new ConcurrentBag<string>();
            ConcurrentBag<string> lstLanguage = new ConcurrentBag<string>();
            Parallel.Invoke(
                () =>
                {
                    // Load the English version.
                    XmlDocument objEnglishDocument = new XmlDocument();
                    string strFilePath = Path.Combine(Application.StartupPath, "lang", GlobalOptions.DefaultLanguage + ".xml");

                    try
                    {
                        using (StreamReader objStreamReader = new StreamReader(strFilePath, Encoding.UTF8, true))
                        {
                            objEnglishDocument.Load(objStreamReader);
                        }
                    }
                    catch (IOException)
                    {
                        objEnglishDocument = null;
                    }
                    catch (XmlException)
                    {
                        objEnglishDocument = null;
                    }

                    if (objEnglishDocument != null)
                    {
                        using (XmlNodeList xmlEnglishStringList = objEnglishDocument.SelectNodes("/chummer/strings/string"))
                            if (xmlEnglishStringList != null)
                                foreach (XmlNode objNode in xmlEnglishStringList)
                                {
                                    string strKey = objNode["key"]?.InnerText;
                                    if (!string.IsNullOrEmpty(strKey))
                                        lstEnglish.Add(strKey);
                                }
                    }
                },
                () =>
                {
                    // Load the selected language version.
                    XmlDocument objLanguageDocument = new XmlDocument();
                    string strLangPath = Path.Combine(Application.StartupPath, "lang", strLanguage + ".xml");

                    try
                    {
                        using (StreamReader objStreamReader = new StreamReader(strLangPath, Encoding.UTF8, true))
                        {
                            objLanguageDocument.Load(objStreamReader);
                        }
                    }
                    catch (IOException)
                    {
                        objLanguageDocument = null;
                    }
                    catch (XmlException)
                    {
                        objLanguageDocument = null;
                    }

                    if (objLanguageDocument != null)
                    {
                        using (XmlNodeList xmlLanguageStringList = objLanguageDocument.SelectNodes("/chummer/strings/string"))
                            if (xmlLanguageStringList != null)
                                foreach (XmlNode objNode in xmlLanguageStringList)
                                {
                                    string strKey = objNode["key"]?.InnerText;
                                    if (!string.IsNullOrEmpty(strKey))
                                        lstLanguage.Add(strKey);
                                }
                    }
                }
            );

            StringBuilder objMissingMessage = new StringBuilder();
            StringBuilder objUnusedMessage = new StringBuilder();
            Parallel.Invoke(
                () =>
                {
                    // Check for strings that are in the English file but not in the selected language file.
                    foreach (string strKey in lstEnglish)
                    {
                        if (!lstLanguage.Contains(strKey))
                            objMissingMessage.AppendLine("Missing String: " + strKey);
                    }
                },
                () =>
                {
                    // Check for strings that are not in the English file but are in the selected language file (someone has put in Keys that they shouldn't have which are ignored).
                    foreach (string strKey in lstLanguage)
                    {
                        if (!lstEnglish.Contains(strKey))
                            objUnusedMessage.AppendLine("Unused String: " + strKey);
                    }
                }
            );

            string strMessage = (objMissingMessage.ToString() + objUnusedMessage.ToString()).TrimEndOnce(Environment.NewLine);
            // Display the message.
            MessageBox.Show(!string.IsNullOrEmpty(strMessage) ? strMessage : "Language file is OK.", "Language File Contents", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // List of XPaths to search for extras. Item1 is Document, Item2 is XPath, Item3 is the Name getter, Item4 is the Translate getter
        private static readonly Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>[] s_LstXPathsToSearch =
        {
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("weapons.xml", "/chummer/categories/category",
                x => x.InnerText, x => x.Attributes?["translate"]?.InnerText),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("spells.xml", "/chummer/categories/category",
                x => x.InnerText, x => x.Attributes?["translate"]?.InnerText),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("programs.xml", "/chummer/categories/category",
                x => x.InnerText, x => x.Attributes?["translate"]?.InnerText),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("skills.xml", "/chummer/skills/skill/specs/spec",
                x => x.InnerText, x => x.Attributes?["translate"]?.InnerText),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("skills.xml", "/chummer/knowledgeskills/skill/specs/spec",
                x => x.InnerText, x => x.Attributes?["translate"]?.InnerText),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("skills.xml", "/chummer/skillgroups/name",
                x => x.InnerText, x => x.Attributes?["translate"]?.InnerText),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("skills.xml", "/chummer/categories/category",
                x => x.InnerText, x => x.Attributes?["translate"]?.InnerText),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("licenses.xml", "/chummer/licenses/license",
                x => x.InnerText, x => x.Attributes?["translate"]?.InnerText),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("contacts.xml", "/chummer/contacts/contact",
                x => x.InnerText, x => x.Attributes?["translate"]?.InnerText),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("contacts.xml", "/chummer/sexes/sex",
                x => x.InnerText, x => x.Attributes?["translate"]?.InnerText),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("contacts.xml", "/chummer/ages/age",
                x => x.InnerText, x => x.Attributes?["translate"]?.InnerText),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("contacts.xml", "/chummer/personallives/personallife",
                x => x.InnerText, x => x.Attributes?["translate"]?.InnerText),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("contacts.xml", "/chummer/types/type",
                x => x.InnerText, x => x.Attributes?["translate"]?.InnerText),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("contacts.xml", "/chummer/preferredpayments/preferredpayment",
                x => x.InnerText, x => x.Attributes?["translate"]?.InnerText),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("contacts.xml", "/chummer/hobbiesvices/hobbyvice",
                x => x.InnerText, x => x.Attributes?["translate"]?.InnerText),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("weapons.xml", "/chummer/weapons/weapon",
                x => x["name"]?.InnerText, x => x["translate"]?.InnerText),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("skills.xml", "/chummer/skills/skill",
                x => x["name"]?.InnerText, x => x["translate"]?.InnerText),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("mentors.xml", "/chummer/mentors/mentor",
                x => x["name"]?.InnerText, x => x["translate"]?.InnerText),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("mentors.xml", "/chummer/mentors/mentor/choices/choice",
                x => x["name"]?.InnerText, x => x["translate"]?.InnerText),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("armor.xml", "/chummer/armors/armor",
                x => x["name"]?.InnerText, x => x["translate"]?.InnerText),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("armor.xml", "/chummer/mods/mod",
                x => x["name"]?.InnerText, x => x["translate"]?.InnerText),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("spells.xml", "/chummer/spells/spell",
                x => x["name"]?.InnerText, x => x["translate"]?.InnerText),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("programs.xml", "/chummer/programs/program",
                x => x["name"]?.InnerText, x => x["translate"]?.InnerText),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("powers.xml", "/chummer/powers/power",
                x => x["name"]?.InnerText, x => x["translate"]?.InnerText),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("metamagic.xml", "/chummer/metamagics/metamagic",
                x => x["name"]?.InnerText, x => x["translate"]?.InnerText),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("echoes.xml", "/chummer/echoes/echo",
                x => x["name"]?.InnerText, x => x["translate"]?.InnerText),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("metatypes.xml", "/chummer/metatypes/metatype",
                x => x["name"]?.InnerText, x => x["translate"]?.InnerText),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("metatypes.xml", "/chummer/metatypes/metatype/metavariants/metavariant",
                x => x["name"]?.InnerText, x => x["translate"]?.InnerText),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("cyberware.xml", "/chummer/cyberwares/cyberware",
                x => x["name"]?.InnerText, x => x["translate"]?.InnerText),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("critterpowers.xml", "/chummer/powers/power",
                x => x["name"]?.InnerText, x => x["translate"]?.InnerText),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("qualities.xml", "/chummer/qualities/quality",
                x => x["name"]?.InnerText, x => x["translate"]?.InnerText),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("ranges.xml", "/chummer/ranges/range",
                x => x["name"]?.InnerText, x => x["translate"]?.InnerText),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("paragons.xml", "/chummer/mentors/mentor",
                x => x["name"]?.InnerText, x => x["translate"]?.InnerText),
            new Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>>("paragons.xml", "/chummer/mentors/mentor/choices/choice",
                x => x["name"]?.InnerText, x => x["translate"]?.InnerText),
        };

        /// <summary>
        /// Attempt to translate any Extra text for an item.
        /// </summary>
        /// <param name="strExtra">Extra string to translate.</param>
        /// <param name="strIntoLanguage">Language into which the string should be translated</param>
        public static string TranslateExtra(string strExtra, string strIntoLanguage)
        {
            string strReturn = string.Empty;

            // Only attempt to translate if we're not using English. Don't attempt to translate an empty string either.
            if (strIntoLanguage != GlobalOptions.DefaultLanguage && !string.IsNullOrWhiteSpace(strExtra))
            {
                // Attempt to translate CharacterAttribute names.
                switch (strExtra)
                {
                    case "BOD":
                        strReturn = GetString("String_AttributeBODShort", strIntoLanguage);
                        break;
                    case "AGI":
                        strReturn = GetString("String_AttributeAGIShort", strIntoLanguage);
                        break;
                    case "REA":
                        strReturn = GetString("String_AttributeREAShort", strIntoLanguage);
                        break;
                    case "STR":
                        strReturn = GetString("String_AttributeSTRShort", strIntoLanguage);
                        break;
                    case "CHA":
                        strReturn = GetString("String_AttributeCHAShort", strIntoLanguage);
                        break;
                    case "INT":
                        strReturn = GetString("String_AttributeINTShort", strIntoLanguage);
                        break;
                    case "LOG":
                        strReturn = GetString("String_AttributeLOGShort", strIntoLanguage);
                        break;
                    case "WIL":
                        strReturn = GetString("String_AttributeWILShort", strIntoLanguage);
                        break;
                    case "EDG":
                        strReturn = GetString("String_AttributeEDGShort", strIntoLanguage);
                        break;
                    case "MAG":
                        strReturn = GetString("String_AttributeMAGShort", strIntoLanguage);
                        break;
                    case "MAGAdept":
                        strReturn = GetString("String_AttributeMAGShort", strIntoLanguage) + " (" + GetString("String_DescAdept", strIntoLanguage) + ')';
                        break;
                    case "RES":
                        strReturn = GetString("String_AttributeRESShort", strIntoLanguage);
                        break;
                    case "DEP":
                        strReturn = GetString("String_AttributeDEPShort", strIntoLanguage);
                        break;
                    case "Physical":
                        strReturn = GetString("Node_Physical", strIntoLanguage);
                        break;
                    case "Mental":
                        strReturn = GetString("Node_Mental", strIntoLanguage);
                        break;
                    case "Social":
                        strReturn = GetString("Node_Social", strIntoLanguage);
                        break;
                    case "Left":
                        strReturn = GetString("String_Improvement_SideLeft", strIntoLanguage);
                        break;
                    case "Right":
                        strReturn = GetString("String_Improvement_SideRight", strIntoLanguage);
                        break;
                    default:
                        string strExtraNoQuotes = strExtra.FastEscape('\"');

                        object strReturnLock = new object();
                        Parallel.For(0, s_LstXPathsToSearch.Length, (i, state) =>
                        {
                            Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>> objXPathPair = s_LstXPathsToSearch[i];
                            using (XmlNodeList xmlNodeList = XmlManager.Load(objXPathPair.Item1, strIntoLanguage).SelectNodes(objXPathPair.Item2))
                                if (xmlNodeList != null)
                                    foreach (XmlNode objNode in xmlNodeList)
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
            if (string.IsNullOrEmpty(strReturn) || strReturn.Contains("not found; check language file for string"))
                strReturn = strExtra;

            return strReturn;
        }

        /// <summary>
        /// Attempt to translate any Extra text for an item from a foreign language to the default one.
        /// </summary>
        /// <param name="strExtra">Extra string to translate.</param>
        /// <param name="strFromLanguage">Language from which the string should be translated</param>
        public static string ReverseTranslateExtra(string strExtra, string strFromLanguage)
        {
            // If no original could be found, just use whatever we were passed.
            string strReturn = strExtra;

            // Only attempt to translate if we're not using English. Don't attempt to translate an empty string either.
            if (strFromLanguage != GlobalOptions.DefaultLanguage && !string.IsNullOrWhiteSpace(strExtra))
            {
                // Attempt to translate CharacterAttribute names.
                if (strExtra == GetString("String_AttributeBODShort", strFromLanguage))
                {
                    return "BOD";
                }

                if (strExtra == GetString("String_AttributeAGIShort", strFromLanguage))
                {
                    return "AGI";
                }

                if (strExtra == GetString("String_AttributeREAShort", strFromLanguage))
                {
                    return "REA";
                }

                if (strExtra == GetString("String_AttributeSTRShort", strFromLanguage))
                {
                    return "STR";
                }

                if (strExtra == GetString("String_AttributeCHAShort", strFromLanguage))
                {
                    return "CHA";
                }

                if (strExtra == GetString("String_AttributeINTShort", strFromLanguage))
                {
                    return "INT";
                }

                if (strExtra == GetString("String_AttributeLOGShort", strFromLanguage))
                {
                    return "LOG";
                }

                if (strExtra == GetString("String_AttributeWILShort", strFromLanguage))
                {
                    return "WIL";
                }

                if (strExtra == GetString("String_AttributeEDGShort", strFromLanguage))
                {
                    return "EDG";
                }

                if(strExtra == GetString("String_AttributeMAGShort", strFromLanguage))
                {
                    return "MAG";
                }

                if (strExtra == GetString("String_AttributeMAGShort", strFromLanguage) + " (" + GetString("String_DescAdept", strFromLanguage) + ')')
                {
                    return "MAGAdept";
                }

                if (strExtra == GetString("String_AttributeRESShort", strFromLanguage))
                {
                    return "RES";
                }

                if (strExtra == GetString("String_AttributeDEPShort", strFromLanguage))
                {
                    return "DEP";
                }

                if (strExtra == GetString("Node_Physical", strFromLanguage))
                {
                    return "Physical";
                }

                if (strExtra == GetString("Node_Mental", strFromLanguage))
                {
                    return "Mental";
                }

                if (strExtra == GetString("Node_Social", strFromLanguage))
                {
                    return "Social";
                }

                if (strExtra == GetString("String_Improvement_SideLeft", strFromLanguage))
                {
                    return "Left";
                }

                if (strExtra == GetString("String_Improvement_SideRight", strFromLanguage))
                {
                    return "Right";
                }

                string strExtraNoQuotes = strExtra.FastEscape('\"');

                object strReturnLock = new object();
                Parallel.For(0, s_LstXPathsToSearch.Length, (i, state) =>
                {
                    Tuple<string, string, Func<XmlNode, string>, Func<XmlNode, string>> objXPathPair = s_LstXPathsToSearch[i];
                    using (XmlNodeList xmlNodeList = XmlManager.Load(objXPathPair.Item1, strFromLanguage).SelectNodes(objXPathPair.Item2))
                        if (xmlNodeList != null)
                            foreach (XmlNode xmlNode in xmlNodeList)
                            {
                                if (objXPathPair.Item4(xmlNode) == strExtraNoQuotes)
                                {
                                    string strOriginal = objXPathPair.Item3(xmlNode);
                                    if (!string.IsNullOrEmpty(strOriginal))
                                    {
                                        lock (strReturnLock)
                                            strReturn = strOriginal;
                                        state.Stop();
                                        break;
                                    }
                                }
                            }
                });
            }

            return strReturn;
        }
        #endregion
    }
}
