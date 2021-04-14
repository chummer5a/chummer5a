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
using System.Globalization;
using System.IO;
using System.Linq;
 using System.Runtime.CompilerServices;
 using System.Text;
using System.Threading.Tasks;
 using System.Windows.Forms;
using System.Xml;
 using System.Xml.XPath;

namespace Chummer
{
    public static class LanguageManager
    {
        private static readonly Dictionary<string, LanguageData> s_DictionaryLanguages = new Dictionary<string, LanguageData>();
        public static IReadOnlyDictionary<string, LanguageData> DictionaryLanguages => s_DictionaryLanguages;
        private static readonly Dictionary<string, string> s_DictionaryEnglishStrings = new Dictionary<string, string>();
        public static StringBuilder ManagerErrorMessage { get; } = new StringBuilder();

        #region Constructor
        static LanguageManager()
        {
            if (!Utils.IsDesignerMode)
            {
                string strFilePath = Path.Combine(Utils.GetStartupPath, "lang", GlobalOptions.DefaultLanguage + ".xml");
                if (File.Exists(strFilePath))
                {
                    try
                    {
                        XPathDocument xmlEnglishDocument;
                        using (StreamReader objStreamReader = new StreamReader(strFilePath, Encoding.UTF8, true))
                        using (XmlReader objXmlReader = XmlReader.Create(objStreamReader, GlobalOptions.SafeXmlReaderSettings))
                            xmlEnglishDocument = new XPathDocument(objXmlReader);
                        XPathNodeIterator xmlStringList =
                            xmlEnglishDocument.CreateNavigator().Select("/chummer/strings/string");
                        if (xmlStringList.Count > 0)
                        {
                            foreach (XPathNavigator objNode in xmlStringList)
                            {
                                string strKey = objNode.SelectSingleNode("key")?.Value;
                                string strText = objNode.SelectSingleNode("text")?.Value;
                                if (!string.IsNullOrEmpty(strKey) && !string.IsNullOrEmpty(strText))
                                {
                                    if (s_DictionaryEnglishStrings.ContainsKey(strKey))
                                        Utils.BreakIfDebug();
                                    else
                                        s_DictionaryEnglishStrings.Add(strKey, strText.NormalizeLineEndings(true));
                                }
                            }
                        }
                        else
                        {
                            ManagerErrorMessage.Append("Language strings for the default language (")
                                .Append(GlobalOptions.DefaultLanguage).AppendLine(") could not be loaded:")
                                .AppendLine().Append("No strings found in file.");
                        }
                    }
                    catch (IOException ex)
                    {
                        ManagerErrorMessage.Append("Language strings for the default language (")
                            .Append(GlobalOptions.DefaultLanguage).AppendLine(") could not be loaded:").AppendLine().Append(ex);
                    }
                    catch (XmlException ex)
                    {
                        ManagerErrorMessage.Append("Language strings for the default language (")
                            .Append(GlobalOptions.DefaultLanguage).AppendLine(") could not be loaded:").AppendLine().Append(ex);
                    }
                }
                else
                    ManagerErrorMessage.Append("Language strings for the default language (")
                        .Append(GlobalOptions.DefaultLanguage).AppendLine(") could not be loaded:")
                        .AppendLine().Append("File ").Append(strFilePath).Append(" does not exist or cannot be found.");
            }
        }
        #endregion

        #region Methods
       
        /// <summary>
        /// Translate an object int a specified language.
        /// </summary>
        /// <param name="strIntoLanguage">Language to which to translate the object.</param>
        /// <param name="objObject">Object to translate.</param>
        public static void TranslateWinForm(this Control objObject, string strIntoLanguage = "")
        {
            if (!Utils.IsDesignerMode)
            {
                objObject.SuspendLayout();
                if (string.IsNullOrEmpty(strIntoLanguage))
                    strIntoLanguage = GlobalOptions.Language;
                if (LoadLanguage(strIntoLanguage))
                {
                    RightToLeft eIntoRightToLeft = RightToLeft.No;
                    if (DictionaryLanguages.TryGetValue(strIntoLanguage, out LanguageData objLanguageData))
                    {
                        eIntoRightToLeft = objLanguageData.IsRightToLeftScript ? RightToLeft.Yes : RightToLeft.No;
                    }

                    UpdateControls(objObject, strIntoLanguage, eIntoRightToLeft);
                }
                else if (strIntoLanguage != GlobalOptions.DefaultLanguage)
                    UpdateControls(objObject, GlobalOptions.DefaultLanguage, RightToLeft.No);
                objObject.ResumeLayout();
            }
        }

        public static bool LoadLanguage(string strLanguage)
        {
            if (strLanguage != GlobalOptions.DefaultLanguage)
            {
                if (!s_DictionaryLanguages.TryGetValue(strLanguage, out LanguageData objNewLanguage))
                {
                    objNewLanguage = new LanguageData(strLanguage);
                    s_DictionaryLanguages.Add(strLanguage, objNewLanguage);
                }
                if (objNewLanguage.ErrorMessage.Length > 0)
                {
                    if (!objNewLanguage.ErrorAlreadyShown)
                    {
                        Program.MainForm.ShowMessageBox(
                            "Language with code " + strLanguage + " could not be loaded for the following reasons:" +
                            Environment.NewLine + Environment.NewLine + objNewLanguage.ErrorMessage, "Cannot Load Language",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        /// <param name="eIntoRightToLeft">Whether <paramref name="strIntoLanguage" /> is a right-to-left language</param>
        private static void UpdateControls(Control objParent, string strIntoLanguage, RightToLeft eIntoRightToLeft)
        {
            if (objParent == null)
                return;

            objParent.RightToLeft = eIntoRightToLeft;

            if (objParent is Form frmForm)
            {
                // Translatable items are identified by having a value in their Tag attribute. The contents of Tag is the string to lookup in the language list.
                // Update the Form itself.
                string strControlTag = frmForm.Tag?.ToString();
                if (!string.IsNullOrEmpty(strControlTag) && !int.TryParse(strControlTag, out int _) && !strControlTag.IsGuid() && !File.Exists(strControlTag))
                    frmForm.Text = GetString(strControlTag, strIntoLanguage);
                else if (frmForm.Text.StartsWith('['))
                    frmForm.Text = string.Empty;

                // update any menu strip items that have tags
                if (frmForm.MainMenuStrip != null)
                    foreach (ToolStripMenuItem tssItem in frmForm.MainMenuStrip.Items)
                        TranslateToolStripItemsRecursively(tssItem, strIntoLanguage, eIntoRightToLeft);
            }

            // Translatable items are identified by having a value in their Tag attribute. The contents of Tag is the string to lookup in the language list.
            foreach (Control objChild in objParent.Controls)
            {
                try
                {
                    objChild.RightToLeft = eIntoRightToLeft;
                }
                catch (NotSupportedException)
                {
                    if (objChild.GetType() == typeof(WebBrowser)) continue;
                    Utils.BreakIfDebug();
                }

                if (objChild is Label || objChild is Button || objChild is CheckBox)
                {
                    string strControlTag = objChild.Tag?.ToString();
                    if (!string.IsNullOrEmpty(strControlTag) && !int.TryParse(strControlTag, out int _) && !strControlTag.IsGuid() && !File.Exists(strControlTag))
                        objChild.Text = GetString(strControlTag, strIntoLanguage);
                    else if (objChild.Text.StartsWith('['))
                        objChild.Text = string.Empty;
                }
                else if (objChild is ToolStrip tssStrip)
                {
                    foreach (ToolStripItem tssItem in tssStrip.Items)
                    {
                        TranslateToolStripItemsRecursively(tssItem, strIntoLanguage, eIntoRightToLeft);
                    }
                }
                else if (objChild is ListView lstList)
                {
                    foreach (ColumnHeader objHeader in lstList.Columns)
                    {
                        string strControlTag = objHeader.Tag?.ToString();
                        if (!string.IsNullOrEmpty(strControlTag) && !int.TryParse(strControlTag, out int _) && !strControlTag.IsGuid() && !File.Exists(strControlTag))
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
                        if (!string.IsNullOrEmpty(strControlTag) && !int.TryParse(strControlTag, out int _) && !strControlTag.IsGuid() && !File.Exists(strControlTag))
                            tabPage.Text = GetString(strControlTag, strIntoLanguage);
                        else if (tabPage.Text.StartsWith('['))
                            tabPage.Text = string.Empty;

                        UpdateControls(tabPage, strIntoLanguage, eIntoRightToLeft);
                    }
                }
                else if (objChild is SplitContainer objSplitControl)
                {
                    UpdateControls(objSplitControl.Panel1, strIntoLanguage, eIntoRightToLeft);
                    UpdateControls(objSplitControl.Panel2, strIntoLanguage, eIntoRightToLeft);
                }
                else if (objChild is GroupBox)
                {
                    string strControlTag = objChild.Tag?.ToString();
                    if (!string.IsNullOrEmpty(strControlTag) && !int.TryParse(strControlTag, out int _) && !strControlTag.IsGuid() && !File.Exists(strControlTag))
                        objChild.Text = GetString(strControlTag, strIntoLanguage);
                    else if (objChild.Text.StartsWith('['))
                        objChild.Text = string.Empty;
                    UpdateControls(objChild, strIntoLanguage, eIntoRightToLeft);
                }
                else if (objChild is Panel)
                {
                    UpdateControls(objChild, strIntoLanguage, eIntoRightToLeft);
                }
                else if (objChild is TreeView treTree)
                {
                    foreach (TreeNode objNode in treTree.Nodes)
                    {
                        if (objNode.Level == 0)
                        {
                            string strControlTag = objNode.Tag?.ToString();
                            if (!string.IsNullOrEmpty(strControlTag) && strControlTag.StartsWith("Node_", StringComparison.Ordinal))
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
                else if (objChild is DataGridView objDataGridView)
                {
                    foreach (DataGridViewTextBoxColumn objColumn in objDataGridView.Columns)
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
        /// <param name="eIntoRightToLeft">Whether <paramref name="strIntoLanguage"/> uses right-to-left script or left-to-right. If left at Inherit, then a loading function will be used to set the value.</param>
        public static void TranslateToolStripItemsRecursively(this ToolStripItem tssItem, string strIntoLanguage = "", RightToLeft eIntoRightToLeft = RightToLeft.Inherit)
        {
            if (tssItem == null)
                return;
            if (string.IsNullOrEmpty(strIntoLanguage))
                strIntoLanguage = GlobalOptions.Language;
            if (eIntoRightToLeft == RightToLeft.Inherit)
            {
                if (LoadLanguage(strIntoLanguage) && DictionaryLanguages.TryGetValue(strIntoLanguage, out LanguageData objLanguageData))
                {
                    eIntoRightToLeft = objLanguageData.IsRightToLeftScript ? RightToLeft.Yes : RightToLeft.No;
                }
            }
            tssItem.RightToLeft = eIntoRightToLeft;

            string strControlTag = tssItem.Tag?.ToString();
            if (!string.IsNullOrEmpty(strControlTag) && !int.TryParse(strControlTag, out int _) && !strControlTag.IsGuid() && !File.Exists(strControlTag))
                tssItem.Text = GetString(strControlTag, strIntoLanguage);
            else if (tssItem.Text.StartsWith('['))
                tssItem.Text = string.Empty;

            if (tssItem is ToolStripDropDownItem tssDropDownItem)
                foreach (ToolStripItem tssDropDownChild in tssDropDownItem.DropDownItems)
                    TranslateToolStripItemsRecursively(tssDropDownChild, strIntoLanguage, eIntoRightToLeft);
        }

        /// <summary>
        /// Overload for standard GetString method, using GlobalOptions.Language as default string, but explicitly defining if an error is returned or not.
        /// </summary>
        /// <param name="strKey">Key to retrieve.</param>
        /// <param name="blnReturnError">Should an error string be returned if the key isn't found?</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetString(string strKey, bool blnReturnError)
        {
            return GetString(strKey, GlobalOptions.Language, blnReturnError);
        }
        /// <summary>
        /// Retrieve a string from the language file.
        /// </summary>
        /// <param name="strKey">Key to retrieve.</param>
        /// <param name="strLanguage">Language from which the string should be retrieved.</param>
        /// <param name="blnReturnError">Should an error string be returned if the key isn't found?</param>
        public static string GetString(string strKey, string strLanguage = "", bool blnReturnError = true)
        {
            if (Utils.IsDesignerMode)
                return strKey;
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalOptions.Language;
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
            return !blnReturnError ? string.Empty : strKey + " not found; check language file for string";
        }

        /// <summary>
        /// Processes a compound string that contains both plaintext and references to localized strings
        /// </summary>
        /// <param name="strInput">Input string to process.</param>
        /// <param name="objCharacter">Character whose custom data to use. If null, will not use any custom data.</param>
        /// <param name="strLanguage">Language into which to translate the compound string.</param>
        /// <param name="blnUseTranslateExtra">Whether to use TranslateExtra() or GetString() for translating localized strings.</param>
        /// <returns></returns>
        public static string ProcessCompoundString(string strInput, string strLanguage = "", Character objCharacter = null, bool blnUseTranslateExtra = false)
        {
            if (Utils.IsDesignerMode || string.IsNullOrEmpty(strInput))
                return strInput;
            // Exit out early if we don't have a pair of curly brackets, which is what would signify localized strings
            int intStartPosition = strInput.IndexOf('{');
            if (intStartPosition < 0)
                return strInput;
            int intEndPosition = strInput.LastIndexOf('}');
            if (intEndPosition < 0)
                return strInput;

            // strInput will get split up based on curly brackets and put into this list as a string-bool Tuple.
            // String value in Tuple will be a section of strInput either enclosed in curly brackets or between sets of enclosed curly brackets
            // Bool value in Tuple is a flag for whether the item was enclosed in curly brackets (True) or between sets of enclosed curly brackets (False)
            List<Tuple<string, bool>> lstStringWithCompoundsSplit = new List<Tuple<string, bool>>(5)
            {
                // Start out with part between start of string and the first set of enclosed curly brackets already added to the list
                new Tuple<string, bool>(strInput.Substring(0, intStartPosition), false)
            };

            char[] achrCurlyBrackets = {'{', '}'};
            // Current bracket level. This needs to be tracked so that this method can be performed recursively on curly bracket sets inside of curly bracket sets
            int intBracketLevel = 1;
            // Loop will be jumping to instances of '{' or '}' within strInput until it reaches the last closing curly bracket (at intEndPosition)
            for (int i = strInput.IndexOfAny(achrCurlyBrackets, intStartPosition + 1); i <= intEndPosition; i = strInput.IndexOfAny(achrCurlyBrackets, i + 1))
            {
                char chrLoop = strInput[i];
                switch (chrLoop)
                {
                    case '{':
                    {
                        if (intBracketLevel == 0)
                        {
                            // End of area between sets of curly brackets, push it to lstStringWithCompoundsSplit with Item2 set to False
                            lstStringWithCompoundsSplit.Add(new Tuple<string, bool>(strInput.Substring(intStartPosition + 1, i - 1), false));
                            // Tracks the start of the top-level curly bracket opening to know where to start the substring when this item will be closed by a closing curly bracket
                            intStartPosition = i;
                        }
                        intBracketLevel += 1;
                        break;
                    }
                    case '}':
                    {
                        // Makes sure the function doesn't mess up when there's a closing curly bracket without a matching opening curly bracket
                        if (intBracketLevel > 0)
                        {
                            intBracketLevel -= 1;
                            if (intBracketLevel == 0)
                            {
                                // End of area enclosed by curly brackets, push it to lstStringWithCompoundsSplit with Item2 set to True
                                lstStringWithCompoundsSplit.Add(new Tuple<string, bool>(strInput.Substring(intStartPosition + 1, i - 1), true));
                                // Tracks the start of the area between curly bracket sets to know where to start the substring when the next set of curly brackets is encountered
                                intStartPosition = i;
                            }
                        }

                        break;
                    }
                }
            }

            // End with part between the last set of enclosed curly brackets and the end of the string. This will also catch cases where there are opening curly brackets without matching closing brackets
            lstStringWithCompoundsSplit.Add(new Tuple<string, bool>(strInput.Substring(intEndPosition + 1), false));

            // Start building the return value.
            StringBuilder sbdReturn = new StringBuilder(strInput.Length);
            foreach (Tuple<string, bool> objLoop in lstStringWithCompoundsSplit)
            {
                string strLoop = objLoop.Item1;
                if (!string.IsNullOrEmpty(strLoop))
                {
                    // Items inside curly brackets need of processing, so do processing on them and append the result to the return value
                    if (objLoop.Item2)
                    {
                        // Inner string is a compound string in and of itself, so recurse this method
                        if (strLoop.IndexOfAny('{', '}') != -1)
                        {
                            strLoop = ProcessCompoundString(strLoop, strLanguage, objCharacter, blnUseTranslateExtra);
                        }
                        // Use more expensive TranslateExtra if flag is set to use that
                        sbdReturn.Append(blnUseTranslateExtra
                            ? TranslateExtra(strLoop, strLanguage, objCharacter)
                            : GetString(strLoop, strLanguage, false));
                    }
                    // Items between curly bracket sets do not need processing, so just append them to the return value wholesale
                    else
                    {
                        sbdReturn.Append(strLoop);
                    }
                }
            }

            return sbdReturn.ToString();
        }

        /// <summary>
        /// Retrieve a string from the language file.
        /// </summary>
        /// <param name="strLanguage">Language whose document should be retrieved.</param>
        public static XPathDocument GetDataDocument(string strLanguage)
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
                    string strFilePath = Path.Combine(Utils.GetStartupPath, "lang", GlobalOptions.DefaultLanguage + ".xml");
                    try
                    {
                        XPathDocument objEnglishDocument;
                        using (StreamReader objStreamReader = new StreamReader(strFilePath, Encoding.UTF8, true))
                            using (XmlReader objXmlReader = XmlReader.Create(objStreamReader, GlobalOptions.SafeXmlReaderSettings))
                                objEnglishDocument = new XPathDocument(objXmlReader);
                        foreach (XPathNavigator objNode in objEnglishDocument.CreateNavigator().Select("/chummer/strings/string"))
                        {
                            string strKey = objNode.SelectSingleNode("key")?.Value;
                            if (!string.IsNullOrEmpty(strKey))
                                lstEnglish.Add(strKey);
                        }
                    }
                    catch (IOException)
                    {
                    }
                    catch (XmlException)
                    {
                    }
                },
                () =>
                {
                    // Load the selected language version.
                    string strLangPath = Path.Combine(Utils.GetStartupPath, "lang", strLanguage + ".xml");
                    try
                    {
                        XPathDocument objLanguageDocument;
                        using (StreamReader objStreamReader = new StreamReader(strLangPath, Encoding.UTF8, true))
                            using (XmlReader objXmlReader = XmlReader.Create(objStreamReader, GlobalOptions.SafeXmlReaderSettings))
                                objLanguageDocument = new XPathDocument(objXmlReader);
                        foreach (XPathNavigator objNode in objLanguageDocument.CreateNavigator().Select("/chummer/strings/string"))
                        {
                            string strKey = objNode.SelectSingleNode("key")?.Value;
                            if (!string.IsNullOrEmpty(strKey))
                                lstLanguage.Add(strKey);
                        }
                    }
                    catch (IOException)
                    {
                    }
                    catch (XmlException)
                    {
                    }
                }
            );

            StringBuilder sbdMissingMessage = new StringBuilder();
            StringBuilder sbdUnusedMessage = new StringBuilder();
            Parallel.Invoke(
                () =>
                {
                    // Check for strings that are in the English file but not in the selected language file.
                    foreach (string strKey in lstEnglish)
                    {
                        if (!lstLanguage.Contains(strKey))
                            sbdMissingMessage.AppendLine("Missing String: " + strKey);
                    }
                },
                () =>
                {
                    // Check for strings that are not in the English file but are in the selected language file (someone has put in Keys that they shouldn't have which are ignored).
                    foreach (string strKey in lstLanguage)
                    {
                        if (!lstEnglish.Contains(strKey))
                            sbdUnusedMessage.AppendLine("Unused String: " + strKey);
                    }
                }
            );

            string strMessage = (sbdMissingMessage + sbdUnusedMessage.ToString()).TrimEndOnce(Environment.NewLine);
            // Display the message.
            Program.MainForm.ShowMessageBox(!string.IsNullOrEmpty(strMessage) ? strMessage : "Language file is OK.", "Language File Contents", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // List of XPaths to search for extras. Item1 is Document, Item2 is XPath, Item3 is the Name getter, Item4 is the Translate getter
        private static readonly Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>[] s_LstXPathsToSearch =
        {
            new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("weapons.xml", "/chummer/categories/category",
                x => x.Value, x => x.SelectSingleNode("@translate")?.Value),
            new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("spells.xml", "/chummer/categories/category",
                x => x.Value, x => x.SelectSingleNode("@translate")?.Value),
            new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("programs.xml", "/chummer/categories/category",
                x => x.Value, x => x.SelectSingleNode("@translate")?.Value),
            new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("skills.xml", "/chummer/skills/skill/specs/spec",
                x => x.Value, x => x.SelectSingleNode("@translate")?.Value),
            new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("skills.xml", "/chummer/knowledgeskills/skill/specs/spec",
                x => x.Value, x => x.SelectSingleNode("@translate")?.Value),
            new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("skills.xml", "/chummer/skillgroups/name",
                x => x.Value, x => x.SelectSingleNode("@translate")?.Value),
            new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("skills.xml", "/chummer/categories/category",
                x => x.Value, x => x.SelectSingleNode("@translate")?.Value),
            new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("licenses.xml", "/chummer/licenses/license",
                x => x.Value, x => x.SelectSingleNode("@translate")?.Value),
            new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("contacts.xml", "/chummer/contacts/contact",
                x => x.Value, x => x.SelectSingleNode("@translate")?.Value),
            new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("contacts.xml", "/chummer/genders/gender",
                x => x.Value, x => x.SelectSingleNode("@translate")?.Value),
            new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("contacts.xml", "/chummer/ages/age",
                x => x.Value, x => x.SelectSingleNode("@translate")?.Value),
            new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("contacts.xml", "/chummer/personallives/personallife",
                x => x.Value, x => x.SelectSingleNode("@translate")?.Value),
            new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("contacts.xml", "/chummer/types/type",
                x => x.Value, x => x.SelectSingleNode("@translate")?.Value),
            new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("contacts.xml", "/chummer/preferredpayments/preferredpayment",
                x => x.Value, x => x.SelectSingleNode("@translate")?.Value),
            new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("contacts.xml", "/chummer/hobbiesvices/hobbyvice",
                x => x.Value, x => x.SelectSingleNode("@translate")?.Value),
            new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("weapons.xml", "/chummer/weapons/weapon",
                x => x.SelectSingleNode("name")?.Value, x => x.SelectSingleNode("translate")?.Value),
            new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("skills.xml", "/chummer/skills/skill",
                x => x.SelectSingleNode("name")?.Value, x => x.SelectSingleNode("translate")?.Value),
            new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("mentors.xml", "/chummer/mentors/mentor",
                x => x.SelectSingleNode("name")?.Value, x => x.SelectSingleNode("translate")?.Value),
            new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("mentors.xml", "/chummer/mentors/mentor/choices/choice",
                x => x.SelectSingleNode("name")?.Value, x => x.SelectSingleNode("translate")?.Value),
            new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("armor.xml", "/chummer/armors/armor",
                x => x.SelectSingleNode("name")?.Value, x => x.SelectSingleNode("translate")?.Value),
            new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("armor.xml", "/chummer/mods/mod",
                x => x.SelectSingleNode("name")?.Value, x => x.SelectSingleNode("translate")?.Value),
            new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("spells.xml", "/chummer/spells/spell",
                x => x.SelectSingleNode("name")?.Value, x => x.SelectSingleNode("translate")?.Value),
            new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("programs.xml", "/chummer/programs/program",
                x => x.SelectSingleNode("name")?.Value, x => x.SelectSingleNode("translate")?.Value),
            new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("powers.xml", "/chummer/powers/power",
                x => x.SelectSingleNode("name")?.Value, x => x.SelectSingleNode("translate")?.Value),
            new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("metamagic.xml", "/chummer/metamagics/metamagic",
                x => x.SelectSingleNode("name")?.Value, x => x.SelectSingleNode("translate")?.Value),
            new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("echoes.xml", "/chummer/echoes/echo",
                x => x.SelectSingleNode("name")?.Value, x => x.SelectSingleNode("translate")?.Value),
            new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("metatypes.xml", "/chummer/metatypes/metatype",
                x => x.SelectSingleNode("name")?.Value, x => x.SelectSingleNode("translate")?.Value),
            new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("metatypes.xml", "/chummer/metatypes/metatype/metavariants/metavariant",
                x => x.SelectSingleNode("name")?.Value, x => x.SelectSingleNode("translate")?.Value),
            new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("cyberware.xml", "/chummer/cyberwares/cyberware",
                x => x.SelectSingleNode("name")?.Value, x => x.SelectSingleNode("translate")?.Value),
            new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("critterpowers.xml", "/chummer/powers/power",
                x => x.SelectSingleNode("name")?.Value, x => x.SelectSingleNode("translate")?.Value),
            new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("qualities.xml", "/chummer/qualities/quality",
                x => x.SelectSingleNode("name")?.Value, x => x.SelectSingleNode("translate")?.Value),
            new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("ranges.xml", "/chummer/ranges/range",
                x => x.SelectSingleNode("name")?.Value, x => x.SelectSingleNode("translate")?.Value),
            new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("paragons.xml", "/chummer/mentors/mentor",
                x => x.SelectSingleNode("name")?.Value, x => x.SelectSingleNode("translate")?.Value),
            new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("paragons.xml", "/chummer/mentors/mentor/choices/choice",
                x => x.SelectSingleNode("name")?.Value, x => x.SelectSingleNode("translate")?.Value),
            new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("actions.xml", "/chummer/actions/action",
                x => x.SelectSingleNode("name")?.Value, x => x.SelectSingleNode("translate")?.Value),
        };

        public static string MAGAdeptString(string strLanguage = "", bool blnLong = false)
        {
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalOptions.Language;
            return new StringBuilder(GetString(blnLong ? "String_AttributeMAGLong" : "String_AttributeMAGShort", strLanguage))
                .Append(GetString("String_Space", strLanguage))
                .Append('(').Append(GetString("String_DescAdept", strLanguage)).Append(')').ToString();
        }

        /// <summary>
        /// Attempt to translate any Extra text for an item.
        /// </summary>
        /// <param name="strExtra">Extra string to translate.</param>
        /// <param name="objCharacter">Character whose custom data to use. If null, will not use any custom data.</param>
        /// <param name="strIntoLanguage">Language into which the string should be translated</param>
        public static string TranslateExtra(string strExtra, string strIntoLanguage = "", Character objCharacter = null)
        {
            if (string.IsNullOrEmpty(strExtra))
                return string.Empty;
            if (string.IsNullOrEmpty(strIntoLanguage))
                strIntoLanguage = GlobalOptions.Language;
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
                        strReturn = MAGAdeptString(strIntoLanguage);
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
                    case "All":
                        strReturn = GetString("String_All", strIntoLanguage);
                        break;
                    case "None":
                        strReturn = GetString("String_None", strIntoLanguage);
                        break;
                    default:
                        string strExtraNoQuotes = strExtra.FastEscape('\"');

                        object strReturnLock = new object();
                        Parallel.For((long) 0, s_LstXPathsToSearch.Length, (i, state) =>
                        {
                            Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>> objXPathPair = s_LstXPathsToSearch[i];
                            foreach (XPathNavigator objNode in XmlManager.LoadXPath(objXPathPair.Item1, objCharacter?.Options.EnabledCustomDataDirectoryPaths, strIntoLanguage).Select(objXPathPair.Item2))
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
        /// <param name="objCharacter">Character whose custom data to use. If null, will not use any custom data.</param>
        /// <param name="strFromLanguage">Language from which the string should be translated</param>
        public static string ReverseTranslateExtra(string strExtra, string strFromLanguage = "", Character objCharacter = null)
        {
            if (string.IsNullOrEmpty(strFromLanguage))
                strFromLanguage = GlobalOptions.Language;
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

                if (strExtra == MAGAdeptString(strFromLanguage))
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

                if (strExtra == GetString("String_All", strFromLanguage))
                {
                    return "All";
                }

                if (strExtra == GetString("String_None", strFromLanguage))
                {
                    return "None";
                }

                string strExtraNoQuotes = strExtra.FastEscape('\"');

                object strReturnLock = new object();
                Parallel.For((long) 0, s_LstXPathsToSearch.Length, (i, state) =>
                {
                    Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>> objXPathPair = s_LstXPathsToSearch[i];
                    foreach (XPathNavigator xmlNode in XmlManager.LoadXPath(objXPathPair.Item1, objCharacter?.Options.EnabledCustomDataDirectoryPaths, strFromLanguage).Select(objXPathPair.Item2))
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

        public static void PopulateSheetLanguageList(ElasticComboBox cboLanguage, string strSelectedSheet, IEnumerable<Character> lstCharacters = null, CultureInfo defaultCulture = null)
        {
            if (cboLanguage == null)
                throw new ArgumentNullException(nameof(cboLanguage));
            string strDefaultSheetLanguage = GlobalOptions.Language;
            if (defaultCulture != null)
                strDefaultSheetLanguage = defaultCulture.Name.ToLowerInvariant();
            int? intLastIndexDirectorySeparator = strSelectedSheet?.LastIndexOf(Path.DirectorySeparatorChar);
            if (intLastIndexDirectorySeparator.HasValue && intLastIndexDirectorySeparator != -1)
            {
                string strSheetLanguage = strSelectedSheet.Substring(0, intLastIndexDirectorySeparator.Value);
                if (strSheetLanguage.Length == 5)
                    strDefaultSheetLanguage = strSheetLanguage;
            }

            cboLanguage.BeginUpdate();
            cboLanguage.ValueMember = "Value";
            cboLanguage.DisplayMember = "Name";
            cboLanguage.DataSource = GetSheetLanguageList(lstCharacters);
            cboLanguage.SelectedValue = strDefaultSheetLanguage;
            if (cboLanguage.SelectedIndex == -1)
            {
                if (defaultCulture != null)
                    cboLanguage.SelectedValue = defaultCulture.Name.ToLowerInvariant();
                else
                    cboLanguage.SelectedValue = GlobalOptions.DefaultLanguage;
            }
            cboLanguage.EndUpdate();
        }

        public static IList<ListItem> GetSheetLanguageList(IEnumerable<Character> lstCharacters = null)
        {
            List<ListItem> lstLanguages = new List<ListItem>();
            string languageDirectoryPath = Path.Combine(Utils.GetStartupPath, "lang");
            string[] languageFilePaths = Directory.GetFiles(languageDirectoryPath, "*.xml");
            List<Character> lstCharacterToUse = lstCharacters?.ToList();
            foreach (string filePath in languageFilePaths)
            {
                XPathDocument xmlDocument;
                try
                {
                    using (StreamReader objStreamReader = new StreamReader(filePath, Encoding.UTF8, true))
                        using (XmlReader objXmlReader = XmlReader.Create(objStreamReader, GlobalOptions.SafeXmlReaderSettings))
                            xmlDocument = new XPathDocument(objXmlReader);
                }
                catch (IOException)
                {
                    continue;
                }
                catch (XmlException)
                {
                    continue;
                }

                XPathNavigator node = xmlDocument.CreateNavigator().SelectSingleNode("/chummer/name");

                if (node == null)
                    continue;

                string strLanguageCode = Path.GetFileNameWithoutExtension(filePath);
                if (XmlManager.GetXslFilesFromLocalDirectory(strLanguageCode, lstCharacterToUse, true).Count > 0)
                {
                    lstLanguages.Add(new ListItem(strLanguageCode, node.Value));
                }
            }
            lstLanguages.Sort(CompareListItems.CompareNames);
            return lstLanguages;
        }
        #endregion
    }

    public class LanguageData
    {
        public bool IsRightToLeftScript { get; }
        public Dictionary<string, string> TranslatedStrings { get; } = new Dictionary<string, string>();
        public XPathDocument DataDocument { get; }
        public StringBuilder ErrorMessage { get; } = new StringBuilder();
        public bool ErrorAlreadyShown { get; set; }

        public LanguageData(string strLanguage)
        {
            string strFilePath = Path.Combine(Utils.GetStartupPath, "lang", strLanguage + ".xml");
            if (File.Exists(strFilePath))
            {
                try
                {
                    XPathDocument objLanguageDocument;
                    string strExtraMessage = string.Empty;
                    try
                    {
                        using (StreamReader objStreamReader = new StreamReader(strFilePath, Encoding.UTF8, true))
                            using (XmlReader objXmlReader = XmlReader.Create(objStreamReader, GlobalOptions.SafeXmlReaderSettings))
                                objLanguageDocument = new XPathDocument(objXmlReader);
                    }
                    catch (IOException ex)
                    {
                        objLanguageDocument = null;
                        strExtraMessage = ex.ToString();
                    }
                    catch (XmlException ex)
                    {
                        objLanguageDocument = null;
                        strExtraMessage = ex.ToString();
                    }

                    if (objLanguageDocument != null)
                    {
                        XPathNavigator objLanguageDocumentNavigator = objLanguageDocument.CreateNavigator();
                        IsRightToLeftScript = objLanguageDocumentNavigator.SelectSingleNode("/chummer/righttoleft")?.Value == bool.TrueString;
                        XPathNodeIterator xmlStringList =
                            objLanguageDocumentNavigator.Select("/chummer/strings/string");
                        if (xmlStringList.Count > 0)
                        {
                            foreach (XPathNavigator objNode in xmlStringList)
                            {
                                // Look for the English version of the found string. If it has been found, replace the English contents with the contents from this file.
                                // If the string was not found, then someone has inserted a Key that should not exist and is ignored.
                                string strKey = objNode.SelectSingleNode("key")?.Value;
                                string strText = objNode.SelectSingleNode("text")?.Value;
                                if (!string.IsNullOrEmpty(strKey) && !string.IsNullOrEmpty(strText))
                                {
                                    if (TranslatedStrings.ContainsKey(strKey))
                                        TranslatedStrings[strKey] = strText.NormalizeLineEndings(true);
                                    else
                                        TranslatedStrings.Add(strKey, strText.NormalizeLineEndings(true));
                                }
                            }
                        }
                        else
                        {
                            ErrorMessage.Append("Failed to load the strings file ").Append(strLanguage)
                                .Append(".xml into an XmlDocument: ").Append(strExtraMessage).AppendLine(".");
                        }
                    }
                    else
                    {
                        ErrorMessage.Append("Failed to load the strings file ").Append(strLanguage)
                            .Append(".xml into an XmlDocument: ").Append(strExtraMessage).AppendLine(".");
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage.Append("Encountered the following the exception while loading ").Append(strLanguage)
                        .Append(".xml into an XmlDocument: ").Append(ex).AppendLine(".");
                }
            }
            else
            {
                ErrorMessage.Append("Could not find the strings file ").Append(strLanguage).AppendLine(".xml.");
            }

            // Check to see if the data translation file for the selected language exists.
            string strDataPath = Path.Combine(Utils.GetStartupPath, "lang", strLanguage + "_data.xml");
            if (File.Exists(strDataPath))
            {
                try
                {
                    try
                    {
                        using (StreamReader objStreamReader = new StreamReader(strDataPath, Encoding.UTF8, true))
                            using (XmlReader objXmlReader = XmlReader.Create(objStreamReader, GlobalOptions.SafeXmlReaderSettings))
                                DataDocument = new XPathDocument(objXmlReader);
                    }
                    catch (IOException ex)
                    {
                        DataDocument = null;
                        ErrorMessage.Append("Failed to load the data file ").Append(strLanguage)
                            .Append("_data.xml into an XmlDocument: ").Append(ex).AppendLine(".");
                    }
                    catch (XmlException ex)
                    {
                        DataDocument = null;
                        ErrorMessage.Append("Failed to load the data file ").Append(strLanguage)
                            .Append("_data.xml into an XmlDocument: ").Append(ex).AppendLine(".");
                    }
                }
                catch (Exception ex)
                {
                    DataDocument = null;
                    ErrorMessage.Append("Encountered the following the exception while loading ").Append(strLanguage)
                        .Append("_data.xml into an XmlDocument: ").Append(ex).AppendLine(".");
                }
            }
            else
            {
                ErrorMessage.Append("Could not find the data file ").Append(strLanguage).AppendLine("_data.xml.");
            }
        }
    }
}
