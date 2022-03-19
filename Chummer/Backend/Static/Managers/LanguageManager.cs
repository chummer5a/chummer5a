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
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer
{
    public static class LanguageManager
    {
        private static readonly LockingDictionary<string, SemaphoreSlim> s_DicLanguageDataLockers = new LockingDictionary<string, SemaphoreSlim>();
        private static readonly LockingDictionary<string, LanguageData> s_DicLanguageData = new LockingDictionary<string, LanguageData>();
        private static readonly LockingDictionary<string, string> s_DicEnglishStrings = new LockingDictionary<string, string>();
        public static IAsyncReadOnlyDictionary<string, LanguageData> LoadedLanguageData => s_DicLanguageData;
        public static string ManagerErrorMessage { get; }

        #region Constructor

        static LanguageManager()
        {
            if (Utils.IsDesignerMode)
                return;
            string strFilePath = Path.Combine(Utils.GetStartupPath, "lang", GlobalSettings.DefaultLanguage + ".xml");
            if (File.Exists(strFilePath))
            {
                try
                {
                    XPathDocument xmlEnglishDocument;
                    using (StreamReader objStreamReader = new StreamReader(strFilePath, Encoding.UTF8, true))
                    using (XmlReader objXmlReader = XmlReader.Create(objStreamReader, GlobalSettings.SafeXmlReaderSettings))
                        xmlEnglishDocument = new XPathDocument(objXmlReader);
                    XPathNodeIterator xmlStringList =
                        xmlEnglishDocument.CreateNavigator().SelectAndCacheExpression("/chummer/strings/string");
                    if (xmlStringList.Count > 0)
                    {
                        foreach (XPathNavigator objNode in xmlStringList)
                        {
                            string strKey = objNode.SelectSingleNodeAndCacheExpression("key")?.Value;
                            if (string.IsNullOrEmpty(strKey))
                                continue;
                            string strText = objNode.SelectSingleNodeAndCacheExpression("text")?.Value;
                            if (string.IsNullOrEmpty(strText))
                                continue;
                            if (s_DicEnglishStrings.ContainsKey(strKey))
                                Utils.BreakIfDebug();
                            else
                                s_DicEnglishStrings.Add(strKey, strText.NormalizeLineEndings(true));
                        }
                    }
                    else
                    {
                        ManagerErrorMessage = "Language strings for the default language (" + GlobalSettings.DefaultLanguage + ") could not be loaded:"
                                              + Environment.NewLine + Environment.NewLine + "No strings found in file.";
                    }
                }
                catch (IOException ex)
                {
                    ManagerErrorMessage = "Language strings for the default language (" + GlobalSettings.DefaultLanguage + ") could not be loaded:"
                                          + Environment.NewLine + Environment.NewLine + ex;
                }
                catch (XmlException ex)
                {
                    ManagerErrorMessage = "Language strings for the default language (" + GlobalSettings.DefaultLanguage + ") could not be loaded:"
                                          + Environment.NewLine + Environment.NewLine + ex;
                }
            }
            else
                ManagerErrorMessage = "Language strings for the default language (" + GlobalSettings.DefaultLanguage + ") could not be loaded:"
                                      + Environment.NewLine + Environment.NewLine + "File " + strFilePath + " does not exist or cannot be found.";
        }

        #endregion Constructor

        #region Methods

        /// <summary>
        /// Translate an object int a specified language.
        /// </summary>
        /// <param name="strIntoLanguage">Language to which to translate the object.</param>
        /// <param name="objObject">Object to translate.</param>
        /// <param name="blnDoResumeLayout">Whether to suspend and then resume the control being translated.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TranslateWinForm(this Control objObject, string strIntoLanguage = "", bool blnDoResumeLayout = true)
        {
            TranslateWinFormCoreAsync(true, objObject, strIntoLanguage, blnDoResumeLayout).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Translate an object int a specified language.
        /// </summary>
        /// <param name="strIntoLanguage">Language to which to translate the object.</param>
        /// <param name="objObject">Object to translate.</param>
        /// <param name="blnDoResumeLayout">Whether to suspend and then resume the control being translated.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task TranslateWinFormAsync(this Control objObject, string strIntoLanguage = "", bool blnDoResumeLayout = true)
        {
            return TranslateWinFormCoreAsync(false, objObject, strIntoLanguage, blnDoResumeLayout);
        }

        /// <summary>
        /// Translate an object int a specified language.
        /// Uses flag hack method design outlined here to avoid locking:
        /// https://docs.microsoft.com/en-us/archive/msdn-magazine/2015/july/async-programming-brownfield-async-development
        /// </summary>
        /// <param name="blnSync">Flag for whether method should always use synchronous code or not.</param>
        /// <param name="strIntoLanguage">Language to which to translate the object.</param>
        /// <param name="objObject">Object to translate.</param>
        /// <param name="blnDoResumeLayout">Whether to suspend and then resume the control being translated.</param>
        private static async Task TranslateWinFormCoreAsync(bool blnSync, Control objObject, string strIntoLanguage, bool blnDoResumeLayout)
        {
            if (Utils.IsDesignerMode)
                return;
            if (blnDoResumeLayout)
                objObject.SuspendLayout();
            if (string.IsNullOrEmpty(strIntoLanguage))
                strIntoLanguage = GlobalSettings.Language;
            bool blnLanguageLoaded = blnSync
                // ReSharper disable once MethodHasAsyncOverload
                ? LoadLanguage(strIntoLanguage)
                : await LoadLanguageAsync(strIntoLanguage);
            if (blnLanguageLoaded)
            {
                RightToLeft eIntoRightToLeft = RightToLeft.No;
                string strKey = strIntoLanguage.ToUpperInvariant();
                if (blnSync)
                {
                    if (LoadedLanguageData.TryGetValue(strKey, out LanguageData objLanguageData))
                        eIntoRightToLeft = objLanguageData.IsRightToLeftScript ? RightToLeft.Yes : RightToLeft.No;
                }
                else
                {
                    (bool blnSuccess, LanguageData objLanguageData) = await LoadedLanguageData.TryGetValueAsync(strKey);
                    if (blnSuccess)
                        eIntoRightToLeft = objLanguageData.IsRightToLeftScript ? RightToLeft.Yes : RightToLeft.No;
                }

                UpdateControls(objObject, strIntoLanguage, eIntoRightToLeft);
            }
            else if (!strIntoLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                UpdateControls(objObject, GlobalSettings.DefaultLanguage, RightToLeft.No);
            if (blnDoResumeLayout)
                objObject.ResumeLayout();
        }

        /// <summary>
        /// Load a language's string translations.
        /// </summary>
        /// <param name="strLanguage">Language whose data should be loaded.</param>
        /// <returns>True if loading is successful, false if not.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool LoadLanguage(string strLanguage)
        {
            return LoadLanguageCoreAsync(true, strLanguage).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Load a language's string translations.
        /// </summary>
        /// <param name="strLanguage">Language whose data should be loaded.</param>
        /// <returns>True if loading is successful, false if not.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<bool> LoadLanguageAsync(string strLanguage)
        {
            return LoadLanguageCoreAsync(false, strLanguage);
        }

        /// <summary>
        /// Load a language's string translations.
        /// Uses flag hack method design outlined here to avoid locking:
        /// https://docs.microsoft.com/en-us/archive/msdn-magazine/2015/july/async-programming-brownfield-async-development
        /// </summary>
        /// <param name="blnSync">Flag for whether method should always use synchronous code or not.</param>
        /// <param name="strLanguage">Language whose data should be loaded.</param>
        /// <returns>True if loading is successful, false if not.</returns>
        private static async Task<bool> LoadLanguageCoreAsync(bool blnSync, string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return true;
            string strKey = strLanguage.ToUpperInvariant();
            SemaphoreSlim objLockerObject;
            if (blnSync)
            {
                // ReSharper disable once MethodHasAsyncOverload
                while (!s_DicLanguageDataLockers.TryGetValue(strKey, out objLockerObject))
                {
                    objLockerObject = Utils.SemaphorePool.Get();
                    // ReSharper disable once MethodHasAsyncOverload
                    if (s_DicLanguageDataLockers.TryAdd(strKey, objLockerObject))
                        break;
                    objLockerObject.Dispose();
                    Utils.SafeSleep();
                }
            }
            else
            {
                bool blnSuccess;
                (blnSuccess, objLockerObject) = await s_DicLanguageDataLockers.TryGetValueAsync(strKey);
                while (!blnSuccess)
                {
                    objLockerObject = Utils.SemaphorePool.Get();
                    if (await s_DicLanguageDataLockers.TryAddAsync(strKey, objLockerObject))
                        break;
                    objLockerObject.Dispose();
                    await Utils.SafeSleepAsync();
                    (blnSuccess, objLockerObject) = await s_DicLanguageDataLockers.TryGetValueAsync(strKey);
                }
            }

            LanguageData objNewLanguage;
            if (blnSync)
                // ReSharper disable once MethodHasAsyncOverload
                objLockerObject.SafeWait();
            else
                await objLockerObject.WaitAsync();
            try
            {
                if (blnSync)
                {
                    // ReSharper disable MethodHasAsyncOverload
                    if (!s_DicLanguageData.TryGetValue(strKey, out objNewLanguage) || objNewLanguage == null)
                    {
                        if (s_DicLanguageData.ContainsKey(strKey))
                            s_DicLanguageData.Remove(strKey);
                        objNewLanguage = new LanguageData(strLanguage);
                        s_DicLanguageData.Add(strKey, objNewLanguage);
                    }
                    // ReSharper restore MethodHasAsyncOverload
                }
                else
                {
                    bool blnSuccess;
                    (blnSuccess, objNewLanguage) = await s_DicLanguageData.TryGetValueAsync(strKey);
                    if (!blnSuccess || objNewLanguage == null)
                    {
                        if (await s_DicLanguageData.ContainsKeyAsync(strKey))
                            await s_DicLanguageData.RemoveAsync(strKey);
                        objNewLanguage = await Task.Run(() => new LanguageData(strLanguage));
                        await s_DicLanguageData.AddAsync(strKey, objNewLanguage);
                    }
                }
            }
            finally
            {
                objLockerObject.Release();
            }

            if (!string.IsNullOrEmpty(objNewLanguage.ErrorMessage))
            {
                if (!objNewLanguage.ErrorAlreadyShown)
                {
                    Program.ShowMessageBox(
                        "Language with code " + strLanguage + " could not be loaded for the following reasons:" +
                        Environment.NewLine + Environment.NewLine + objNewLanguage.ErrorMessage, "Cannot Load Language",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    objNewLanguage.ErrorAlreadyShown = true;
                }
                return false;
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

                switch (objChild)
                {
                    case Label _:
                    case Button _:
                    case CheckBox _:
                        {
                            string strControlTag = objChild.Tag?.ToString();
                            if (!string.IsNullOrEmpty(strControlTag) && !int.TryParse(strControlTag, out int _) && !strControlTag.IsGuid() && !File.Exists(strControlTag))
                                objChild.Text = GetString(strControlTag, strIntoLanguage);
                            else if (objChild.Text.StartsWith('['))
                                objChild.Text = string.Empty;
                            break;
                        }
                    case ToolStrip tssStrip:
                        {
                            foreach (ToolStripItem tssItem in tssStrip.Items)
                            {
                                TranslateToolStripItemsRecursively(tssItem, strIntoLanguage, eIntoRightToLeft);
                            }

                            break;
                        }
                    case ListView lstList:
                        {
                            foreach (ColumnHeader objHeader in lstList.Columns)
                            {
                                string strControlTag = objHeader.Tag?.ToString();
                                if (!string.IsNullOrEmpty(strControlTag) && !int.TryParse(strControlTag, out int _) && !strControlTag.IsGuid() && !File.Exists(strControlTag))
                                    objHeader.Text = GetString(strControlTag, strIntoLanguage);
                                else if (objHeader.Text.StartsWith('['))
                                    objHeader.Text = string.Empty;
                            }

                            break;
                        }
                    case TabControl objTabControl:
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

                            break;
                        }
                    case SplitContainer objSplitControl:
                        UpdateControls(objSplitControl.Panel1, strIntoLanguage, eIntoRightToLeft);
                        UpdateControls(objSplitControl.Panel2, strIntoLanguage, eIntoRightToLeft);
                        break;

                    case GroupBox _:
                        {
                            string strControlTag = objChild.Tag?.ToString();
                            if (!string.IsNullOrEmpty(strControlTag) && !int.TryParse(strControlTag, out int _) && !strControlTag.IsGuid() && !File.Exists(strControlTag))
                                objChild.Text = GetString(strControlTag, strIntoLanguage);
                            else if (objChild.Text.StartsWith('['))
                                objChild.Text = string.Empty;
                            UpdateControls(objChild, strIntoLanguage, eIntoRightToLeft);
                            break;
                        }
                    case Panel _:
                        UpdateControls(objChild, strIntoLanguage, eIntoRightToLeft);
                        break;

                    case TreeView treTree:
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

                            break;
                        }
                    case DataGridView objDataGridView:
                        {
                            foreach (DataGridViewTextBoxColumn objColumn in objDataGridView.Columns)
                            {
                                if (objColumn is DataGridViewTextBoxColumnTranslated objTranslatedColumn && !string.IsNullOrWhiteSpace(objTranslatedColumn.TranslationTag))
                                {
                                    objColumn.HeaderText = GetString(objTranslatedColumn.TranslationTag, strIntoLanguage);
                                }
                            }

                            break;
                        }
                    case ITranslatable translatable:
                        // let custom nodes determine how they want to be translated
                        translatable.Translate();
                        break;
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
                strIntoLanguage = GlobalSettings.Language;
            if (eIntoRightToLeft == RightToLeft.Inherit && LoadLanguage(strIntoLanguage))
            {
                string strKey = strIntoLanguage.ToUpperInvariant();
                if (LoadedLanguageData.TryGetValue(strKey, out LanguageData objLanguageData))
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
        /// Overload for standard GetString method, using GlobalSettings.Language as default string, but explicitly defining if an error is returned or not.
        /// </summary>
        /// <param name="strKey">Key to retrieve.</param>
        /// <param name="blnReturnError">Should an error string be returned if the key isn't found?</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetString(string strKey, bool blnReturnError)
        {
            return GetString(strKey, GlobalSettings.Language, blnReturnError);
        }

        /// <summary>
        /// Overload for standard GetString method, using GlobalSettings.Language as default string, but explicitly defining if an error is returned or not.
        /// </summary>
        /// <param name="strKey">Key to retrieve.</param>
        /// <param name="blnReturnError">Should an error string be returned if the key isn't found?</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<string> GetStringAsync(string strKey, bool blnReturnError)
        {
            return GetStringAsync(strKey, GlobalSettings.Language, blnReturnError);
        }

        /// <summary>
        /// Retrieve a string from the language file.
        /// </summary>
        /// <param name="strKey">Key to retrieve.</param>
        /// <param name="strLanguage">Language from which the string should be retrieved.</param>
        /// <param name="blnReturnError">Should an error string be returned if the key isn't found?</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetString(string strKey, string strLanguage = "", bool blnReturnError = true)
        {
            return GetStringCoreAsync(true, strKey, strLanguage, blnReturnError).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Retrieve a string from the language file.
        /// </summary>
        /// <param name="strKey">Key to retrieve.</param>
        /// <param name="strLanguage">Language from which the string should be retrieved.</param>
        /// <param name="blnReturnError">Should an error string be returned if the key isn't found?</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<string> GetStringAsync(string strKey, string strLanguage = "", bool blnReturnError = true)
        {
            return GetStringCoreAsync(false, strKey, strLanguage, blnReturnError);
        }

        /// <summary>
        /// Retrieve a string from the language file.
        /// Uses flag hack method design outlined here to avoid locking:
        /// https://docs.microsoft.com/en-us/archive/msdn-magazine/2015/july/async-programming-brownfield-async-development
        /// </summary>
        /// <param name="blnSync">Flag for whether method should always use synchronous code or not.</param>
        /// <param name="strKey">Key to retrieve.</param>
        /// <param name="strLanguage">Language from which the string should be retrieved.</param>
        /// <param name="blnReturnError">Should an error string be returned if the key isn't found?</param>
        private static async Task<string> GetStringCoreAsync(bool blnSync, string strKey, string strLanguage, bool blnReturnError)
        {
            if (Utils.IsDesignerMode)
                return strKey;
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalSettings.Language;
            string strReturn;
            bool blnLanguageLoaded = blnSync
                // ReSharper disable once MethodHasAsyncOverload
                ? LoadLanguage(strLanguage)
                : await LoadLanguageAsync(strLanguage);
            if (blnLanguageLoaded)
            {
                string strLanguageKey = strLanguage.ToUpperInvariant();
                if (blnSync)
                {
                    if (LoadedLanguageData.TryGetValue(strLanguageKey, out LanguageData objLanguageData)
                        && objLanguageData.TranslatedStrings.TryGetValue(strKey, out strReturn))
                    {
                        return strReturn;
                    }
                }
                else
                {
                    (bool blnSuccess, LanguageData objLanguageData)
                        = await LoadedLanguageData.TryGetValueAsync(strLanguageKey);
                    if (blnSuccess && objLanguageData.TranslatedStrings.TryGetValue(strKey, out strReturn))
                    {
                        return strReturn;
                    }
                }
            }

            if (blnSync)
            {
                // ReSharper disable once MethodHasAsyncOverload
                if (s_DicEnglishStrings.TryGetValue(strKey, out strReturn))
                    return strReturn;
            }
            else
            {
                bool blnSuccess;
                (blnSuccess, strReturn)
                    = await s_DicEnglishStrings.TryGetValueAsync(strKey);
                if (blnSuccess)
                    return strReturn;
            }

            return !blnReturnError ? string.Empty : strKey + " not found; check language file for string";
        }

        private static readonly char[] s_CurlyBrackets = "{}".ToCharArray();

        /// <summary>
        /// Processes a compound string that contains both plaintext and references to localized strings
        /// </summary>
        /// <param name="strInput">Input string to process.</param>
        /// <param name="objCharacter">Character whose custom data to use. If null, will not use any custom data.</param>
        /// <param name="strLanguage">Language into which to translate the compound string.</param>
        /// <param name="blnUseTranslateExtra">Whether to use TranslateExtra() instead of GetString() for translating localized strings.</param>
        /// <returns></returns>
        public static async ValueTask<string> ProcessCompoundString(string strInput, string strLanguage = "", Character objCharacter = null, bool blnUseTranslateExtra = false)
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

            // Current bracket level. This needs to be tracked so that this method can be performed recursively on curly bracket sets inside of curly bracket sets
            int intBracketLevel = 1;
            // Loop will be jumping to instances of '{' or '}' within strInput until it reaches the last closing curly bracket (at intEndPosition)
            for (int i = strInput.IndexOfAny(s_CurlyBrackets, intStartPosition + 1); i <= intEndPosition; i = strInput.IndexOfAny(s_CurlyBrackets, i + 1))
            {
                switch (strInput[i])
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
                            ++intBracketLevel;
                            break;
                        }
                    case '}':
                        {
                            // Makes sure the function doesn't mess up when there's a closing curly bracket without a matching opening curly bracket
                            if (intBracketLevel > 0)
                            {
                                --intBracketLevel;
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
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdReturn))
            {
                int intNewCapacity = strInput.Length;
                if (sbdReturn.Capacity < intNewCapacity)
                    sbdReturn.Capacity = intNewCapacity;
                foreach (Tuple<string, bool> objLoop in lstStringWithCompoundsSplit)
                {
                    string strLoop = objLoop.Item1;
                    if (string.IsNullOrEmpty(strLoop))
                        continue;
                    // Items inside curly brackets need of processing, so do processing on them and append the result to the return value
                    if (objLoop.Item2)
                    {
                        // Inner string is a compound string in and of itself, so recurse this method
                        if (strLoop.IndexOfAny('{', '}') != -1)
                        {
                            strLoop = await ProcessCompoundString(strLoop, strLanguage, objCharacter,
                                                                  blnUseTranslateExtra);
                        }

                        // Use more expensive TranslateExtra if flag is set to use that
                        sbdReturn.Append(blnUseTranslateExtra
                                             ? await TranslateExtraAsync(strLoop, strLanguage, objCharacter)
                                             : await GetStringAsync(strLoop, strLanguage, false));
                    }
                    // Items between curly bracket sets do not need processing, so just append them to the return value wholesale
                    else
                    {
                        sbdReturn.Append(strLoop);
                    }
                }

                return sbdReturn.ToString();
            }
        }

        /// <summary>
        /// Retrieve a string from the language file.
        /// </summary>
        /// <param name="strLanguage">Language whose document should be retrieved.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XPathDocument GetDataDocument(string strLanguage)
        {
            return GetDataDocumentCoreAsync(true, strLanguage).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Retrieve a string from the language file.
        /// </summary>
        /// <param name="strLanguage">Language whose document should be retrieved.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<XPathDocument> GetDataDocumentAsync(string strLanguage)
        {
            return GetDataDocumentCoreAsync(false, strLanguage);
        }

        /// <summary>
        /// Retrieve a string from the language file.
        /// Uses flag hack method design outlined here to avoid locking:
        /// https://docs.microsoft.com/en-us/archive/msdn-magazine/2015/july/async-programming-brownfield-async-development
        /// </summary>
        /// <param name="blnSync">Flag for whether method should always use synchronous code or not.</param>
        /// <param name="strLanguage">Language whose document should be retrieved.</param>
        private static async Task<XPathDocument> GetDataDocumentCoreAsync(bool blnSync, string strLanguage)
        {
            bool blnLanguageLoaded = blnSync
                // ReSharper disable once MethodHasAsyncOverload
                ? LoadLanguage(strLanguage)
                : await LoadLanguageAsync(strLanguage);
            if (blnLanguageLoaded)
            {
                string strKey = strLanguage.ToUpperInvariant();
                if (blnSync)
                {
                    if (LoadedLanguageData.TryGetValue(strKey, out LanguageData objLanguageData))
                    {
                        return objLanguageData.DataDocument;
                    }
                }
                else
                {
                    (bool blnSuccess, LanguageData objLanguageData) = await LoadedLanguageData.TryGetValueAsync(strKey);
                    if (blnSuccess)
                    {
                        return objLanguageData.DataDocument;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Check the Keys in the selected language file against the English version.
        /// </summary>
        /// <param name="strLanguage">Language to check.</param>
        public static async ValueTask VerifyStrings(string strLanguage)
        {
            string strMessage;
            using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                            out HashSet<string> setEnglishKeys))
            using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                            out HashSet<string> setLanguageKeys))
            {
                // Potentially expensive checks that can (and therefore should) be parallelized.
                await Task.WhenAll(
                    Task.Run(() =>
                    {
                        // Load the English version.
                        string strFilePath
                            = Path.Combine(Utils.GetStartupPath, "lang", GlobalSettings.DefaultLanguage + ".xml");
                        try
                        {
                            XPathDocument objEnglishDocument;
                            using (StreamReader objStreamReader = new StreamReader(strFilePath, Encoding.UTF8, true))
                            using (XmlReader objXmlReader
                                   = XmlReader.Create(objStreamReader, GlobalSettings.SafeXmlReaderSettings))
                                objEnglishDocument = new XPathDocument(objXmlReader);
                            foreach (XPathNavigator objNode in objEnglishDocument.CreateNavigator()
                                         .SelectAndCacheExpression("/chummer/strings/string"))
                            {
                                string strKey = objNode.SelectSingleNodeAndCacheExpression("key")?.Value;
                                if (!string.IsNullOrEmpty(strKey))
                                    setEnglishKeys.Add(strKey);
                            }
                        }
                        catch (IOException)
                        {
                            //swallow this
                        }
                        catch (XmlException)
                        {
                            //swallow this
                        }
                    }),
                    Task.Run(() =>
                    {
                        // Load the selected language version.
                        string strLangPath = Path.Combine(Utils.GetStartupPath, "lang", strLanguage + ".xml");
                        try
                        {
                            XPathDocument objLanguageDocument;
                            using (StreamReader objStreamReader = new StreamReader(strLangPath, Encoding.UTF8, true))
                            using (XmlReader objXmlReader
                                   = XmlReader.Create(objStreamReader, GlobalSettings.SafeXmlReaderSettings))
                                objLanguageDocument = new XPathDocument(objXmlReader);
                            foreach (XPathNavigator objNode in objLanguageDocument.CreateNavigator()
                                         .SelectAndCacheExpression("/chummer/strings/string"))
                            {
                                string strKey = objNode.SelectSingleNodeAndCacheExpression("key")?.Value;
                                if (!string.IsNullOrEmpty(strKey))
                                    setLanguageKeys.Add(strKey);
                            }
                        }
                        catch (IOException)
                        {
                            //swallow this
                        }
                        catch (XmlException)
                        {
                            //swallow this
                        }
                    }));

                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sbdMissingMessage))
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sbdUnusedMessage))
                {
                    // Potentially expensive checks that can (and therefore should) be parallelized. Normally, this would just be a Parallel.Invoke,
                    // but we want to allow UI messages to happen, just in case this is called on the Main Thread and another thread wants to show a message box.
                    await Task.WhenAll(
                        Task.Run(() =>
                        {
                            // Check for strings that are in the English file but not in the selected language file.
                            foreach (string strKey in setEnglishKeys)
                            {
                                if (!setLanguageKeys.Contains(strKey))
                                    sbdMissingMessage.Append("Missing String: ").AppendLine(strKey);
                            }
                        }),
                        Task.Run(() =>
                        {
                            // Check for strings that are not in the English file but are in the selected language file (someone has put in Keys that they shouldn't have which are ignored).
                            foreach (string strKey in setLanguageKeys)
                            {
                                if (!setEnglishKeys.Contains(strKey))
                                    sbdUnusedMessage.Append("Unused String: ").AppendLine(strKey);
                            }
                        }));

                    strMessage = (sbdMissingMessage + sbdUnusedMessage.ToString()).TrimEndOnce(Environment.NewLine);
                }
            }

            // Display the message.
            Program.ShowMessageBox(!string.IsNullOrEmpty(strMessage) ? strMessage : "Language file is OK.",
                "Language File Contents", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // List of arrays for XPaths to search for extras. Item1 is Document, Item2 is XPath, Item3 is the Name getter, Item4 is the Translate getter.
        // List index indicates priority. Priority tries to avoid issues where an English word has multiple translations, and an unofficial one might get preference over an official one
        private static readonly IReadOnlyList<IReadOnlyList<Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>>> s_LstAXPathsToSearch =
            new[]
            {
            new []
            {
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("programs.xml", "/chummer/categories/category",
                    x => x.Value, x => x.SelectSingleNodeAndCacheExpression("@translate")?.Value),
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("skills.xml", "/chummer/skillgroups/name",
                    x => x.Value, x => x.SelectSingleNodeAndCacheExpression("@translate")?.Value),
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("skills.xml", "/chummer/categories/category",
                    x => x.Value, x => x.SelectSingleNodeAndCacheExpression("@translate")?.Value),
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("spells.xml", "/chummer/categories/category",
                    x => x.Value, x => x.SelectSingleNodeAndCacheExpression("@translate")?.Value),
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("weapons.xml", "/chummer/categories/category",
                    x => x.Value, x => x.SelectSingleNodeAndCacheExpression("@translate")?.Value),
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("armor.xml", "/chummer/armors/armor",
                    x => x.SelectSingleNodeAndCacheExpression("name")?.Value, x => x.SelectSingleNodeAndCacheExpression("translate")?.Value),
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("armor.xml", "/chummer/mods/mod",
                    x => x.SelectSingleNodeAndCacheExpression("name")?.Value, x => x.SelectSingleNodeAndCacheExpression("translate")?.Value),
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("cyberware.xml", "/chummer/cyberwares/cyberware",
                    x => x.SelectSingleNodeAndCacheExpression("name")?.Value, x => x.SelectSingleNodeAndCacheExpression("translate")?.Value),
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("critterpowers.xml", "/chummer/powers/power",
                    x => x.SelectSingleNodeAndCacheExpression("name")?.Value, x => x.SelectSingleNodeAndCacheExpression("translate")?.Value),
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("echoes.xml", "/chummer/echoes/echo",
                    x => x.SelectSingleNodeAndCacheExpression("name")?.Value, x => x.SelectSingleNodeAndCacheExpression("translate")?.Value),
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("mentors.xml", "/chummer/mentors/mentor",
                    x => x.SelectSingleNodeAndCacheExpression("name")?.Value, x => x.SelectSingleNodeAndCacheExpression("translate")?.Value),
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("metamagic.xml", "/chummer/metamagics/metamagic",
                    x => x.SelectSingleNodeAndCacheExpression("name")?.Value, x => x.SelectSingleNodeAndCacheExpression("translate")?.Value),
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("metatypes.xml", "/chummer/metatypes/metatype",
                    x => x.SelectSingleNodeAndCacheExpression("name")?.Value, x => x.SelectSingleNodeAndCacheExpression("translate")?.Value),
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("metatypes.xml", "/chummer/metatypes/metatype/metavariants/metavariant",
                    x => x.SelectSingleNodeAndCacheExpression("name")?.Value, x => x.SelectSingleNodeAndCacheExpression("translate")?.Value),
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("paragons.xml", "/chummer/mentors/mentor",
                    x => x.SelectSingleNodeAndCacheExpression("name")?.Value, x => x.SelectSingleNodeAndCacheExpression("translate")?.Value),
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("powers.xml", "/chummer/powers/power",
                    x => x.SelectSingleNodeAndCacheExpression("name")?.Value, x => x.SelectSingleNodeAndCacheExpression("translate")?.Value),
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("programs.xml", "/chummer/programs/program",
                    x => x.SelectSingleNodeAndCacheExpression("name")?.Value, x => x.SelectSingleNodeAndCacheExpression("translate")?.Value),
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("qualities.xml", "/chummer/qualities/quality",
                    x => x.SelectSingleNodeAndCacheExpression("name")?.Value, x => x.SelectSingleNodeAndCacheExpression("translate")?.Value),
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("ranges.xml", "/chummer/ranges/range",
                    x => x.SelectSingleNodeAndCacheExpression("name")?.Value, x => x.SelectSingleNodeAndCacheExpression("translate")?.Value),
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("skills.xml", "/chummer/skills/skill",
                    x => x.SelectSingleNodeAndCacheExpression("name")?.Value, x => x.SelectSingleNodeAndCacheExpression("translate")?.Value),
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("spells.xml", "/chummer/spells/spell",
                    x => x.SelectSingleNodeAndCacheExpression("name")?.Value, x => x.SelectSingleNodeAndCacheExpression("translate")?.Value),
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("streams.xml", "/chummer/traditions/tradition[name != 'Default']",
                    x => x.SelectSingleNodeAndCacheExpression("name")?.Value, x => x.SelectSingleNodeAndCacheExpression("translate")?.Value),
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("traditions.xml", "/chummer/traditions/tradition[name != 'Custom']",
                    x => x.SelectSingleNodeAndCacheExpression("name")?.Value, x => x.SelectSingleNodeAndCacheExpression("translate")?.Value),
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("weapons.xml", "/chummer/weapons/weapon",
                    x => x.SelectSingleNodeAndCacheExpression("name")?.Value, x => x.SelectSingleNodeAndCacheExpression("translate")?.Value)
            },
            new []
            {
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("contacts.xml", "/chummer/contacts/contact",
                    x => x.Value, x => x.SelectSingleNodeAndCacheExpression("@translate")?.Value),
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("contacts.xml", "/chummer/genders/gender",
                    x => x.Value, x => x.SelectSingleNodeAndCacheExpression("@translate")?.Value),
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("contacts.xml", "/chummer/ages/age",
                    x => x.Value, x => x.SelectSingleNodeAndCacheExpression("@translate")?.Value),
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("contacts.xml", "/chummer/personallives/personallife",
                    x => x.Value, x => x.SelectSingleNodeAndCacheExpression("@translate")?.Value),
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("contacts.xml", "/chummer/types/type",
                    x => x.Value, x => x.SelectSingleNodeAndCacheExpression("@translate")?.Value),
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("contacts.xml", "/chummer/preferredpayments/preferredpayment",
                    x => x.Value, x => x.SelectSingleNodeAndCacheExpression("@translate")?.Value),
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("contacts.xml", "/chummer/hobbiesvices/hobbyvice",
                    x => x.Value, x => x.SelectSingleNodeAndCacheExpression("@translate")?.Value),
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("licenses.xml", "/chummer/licenses/license",
                    x => x.Value, x => x.SelectSingleNodeAndCacheExpression("@translate")?.Value),
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("skills.xml", "/chummer/skills/skill/specs/spec",
                    x => x.Value, x => x.SelectSingleNodeAndCacheExpression("@translate")?.Value),
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("skills.xml", "/chummer/knowledgeskills/skill/specs/spec",
                    x => x.Value, x => x.SelectSingleNodeAndCacheExpression("@translate")?.Value),
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("actions.xml", "/chummer/actions/action",
                    x => x.SelectSingleNodeAndCacheExpression("name")?.Value, x => x.SelectSingleNodeAndCacheExpression("translate")?.Value),
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("mentors.xml", "/chummer/mentors/mentor/choices/choice",
                    x => x.SelectSingleNodeAndCacheExpression("name")?.Value, x => x.SelectSingleNodeAndCacheExpression("translate")?.Value),
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("paragons.xml", "/chummer/mentors/mentor/choices/choice",
                    x => x.SelectSingleNodeAndCacheExpression("name")?.Value, x => x.SelectSingleNodeAndCacheExpression("translate")?.Value)
            },
            new []
            {
                new Tuple<string, string, Func<XPathNavigator, string>, Func<XPathNavigator, string>>("references.xml", "/chummer/rules/rule",
                    x => x.SelectSingleNodeAndCacheExpression("name")?.Value, x => x.SelectSingleNodeAndCacheExpression("translate")?.Value)
            }
        };

        private static readonly Regex s_RgxExtraFileSpecifierExpression = new Regex(@"^(\[([a-z])+\.xml\])",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static string MAGAdeptString(string strLanguage = "", bool blnLong = false)
        {
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalSettings.Language;
            return GetString(blnLong ? "String_AttributeMAGLong" : "String_AttributeMAGShort", strLanguage) + GetString("String_Space", strLanguage)
                + '(' + GetString("String_DescAdept", strLanguage) + ')';
        }

        public static async ValueTask<string> MAGAdeptStringAsync(string strLanguage = "", bool blnLong = false)
        {
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalSettings.Language;
            return await GetStringAsync(blnLong ? "String_AttributeMAGLong" : "String_AttributeMAGShort", strLanguage) + await GetStringAsync("String_Space", strLanguage)
                + '(' + await GetStringAsync("String_DescAdept", strLanguage) + ')';
        }

        /// <summary>
        /// Attempt to translate any Extra text for an item.
        /// </summary>
        /// <param name="strExtra">Extra string to translate.</param>
        /// <param name="objCharacter">Character whose custom data to use. If null, will not use any custom data.</param>
        /// <param name="strIntoLanguage">Language into which the string should be translated</param>
        /// <param name="strPreferFile">Name of a file to prefer for extras before all others.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string TranslateExtra(string strExtra, string strIntoLanguage = "", Character objCharacter = null,
            string strPreferFile = "")
        {
            // This task can normally end up locking up the UI thread because of the Parallel.Foreach call, so we manually schedule it and intermittently do events while waiting for it
            // Because of how ubiquitous this method is, setting it to async so that we can await this instead would require a massive overhaul.
            // TODO: Do this overhaul.
            return Utils.RunWithoutThreadLock(() => TranslateExtraCoreAsync(true, strExtra, strIntoLanguage, objCharacter, strPreferFile));
        }

        /// <summary>
        /// Attempt to translate any Extra text for an item.
        /// </summary>
        /// <param name="strExtra">Extra string to translate.</param>
        /// <param name="objCharacter">Character whose custom data to use. If null, will not use any custom data.</param>
        /// <param name="strIntoLanguage">Language into which the string should be translated</param>
        /// <param name="strPreferFile">Name of a file to prefer for extras before all others.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<string> TranslateExtraAsync(string strExtra, string strIntoLanguage = "",
                                                             Character objCharacter = null,
                                                             string strPreferFile = "")
        {
            return TranslateExtraCoreAsync(false, strExtra, strIntoLanguage, objCharacter, strPreferFile);
        }

        /// <summary>
        /// Attempt to translate any Extra text for an item.
        /// Uses flag hack method design outlined here to avoid locking:
        /// https://docs.microsoft.com/en-us/archive/msdn-magazine/2015/july/async-programming-brownfield-async-development
        /// </summary>
        /// <param name="blnSync">Flag for whether method should always use synchronous code or not.</param>
        /// <param name="strExtra">Extra string to translate.</param>
        /// <param name="objCharacter">Character whose custom data to use. If null, will not use any custom data.</param>
        /// <param name="strIntoLanguage">Language into which the string should be translated</param>
        /// <param name="strPreferFile">Name of a file to prefer for extras before all others.</param>
        private static async Task<string> TranslateExtraCoreAsync(bool blnSync, string strExtra, string strIntoLanguage, Character objCharacter, string strPreferFile)
        {
            if (string.IsNullOrEmpty(strExtra))
                return string.Empty;
            if (string.IsNullOrEmpty(strIntoLanguage))
                strIntoLanguage = GlobalSettings.Language;
            string strReturn = string.Empty;

            // Only attempt to translate if we're not using English. Don't attempt to translate an empty string either.
            if (!strIntoLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(strExtra))
            {
                // Attempt to translate CharacterAttribute names.
                switch (strExtra)
                {
                    case "BOD":
                        strReturn = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? GetString("String_AttributeBODShort", strIntoLanguage)
                            : await GetStringAsync("String_AttributeBODShort", strIntoLanguage);
                        break;

                    case "AGI":
                        strReturn = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? GetString("String_AttributeAGIShort", strIntoLanguage)
                            : await GetStringAsync("String_AttributeAGIShort", strIntoLanguage);
                        break;

                    case "REA":
                        strReturn = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? GetString("String_AttributeREAShort", strIntoLanguage)
                            : await GetStringAsync("String_AttributeREAShort", strIntoLanguage);
                        break;

                    case "STR":
                        strReturn = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? GetString("String_AttributeSTRShort", strIntoLanguage)
                            : await GetStringAsync("String_AttributeSTRShort", strIntoLanguage);
                        break;

                    case "CHA":
                        strReturn = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? GetString("String_AttributeCHAShort", strIntoLanguage)
                            : await GetStringAsync("String_AttributeCHAShort", strIntoLanguage);
                        break;

                    case "INT":
                        strReturn = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? GetString("String_AttributeINTShort", strIntoLanguage)
                            : await GetStringAsync("String_AttributeINTShort", strIntoLanguage);
                        break;

                    case "LOG":
                        strReturn = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? GetString("String_AttributeLOGShort", strIntoLanguage)
                            : await GetStringAsync("String_AttributeLOGShort", strIntoLanguage);
                        break;

                    case "WIL":
                        strReturn = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? GetString("String_AttributeWILShort", strIntoLanguage)
                            : await GetStringAsync("String_AttributeWILShort", strIntoLanguage);
                        break;

                    case "EDG":
                        strReturn = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? GetString("String_AttributeEDGShort", strIntoLanguage)
                            : await GetStringAsync("String_AttributeEDGShort", strIntoLanguage);
                        break;

                    case "MAG":
                        strReturn = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? GetString("String_AttributeMAGShort", strIntoLanguage)
                            : await GetStringAsync("String_AttributeMAGShort", strIntoLanguage);
                        break;

                    case "MAGAdept":
                        strReturn = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? MAGAdeptString(strIntoLanguage)
                            : await MAGAdeptStringAsync(strIntoLanguage);
                        break;

                    case "RES":
                        strReturn = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? GetString("String_AttributeRESShort", strIntoLanguage)
                            : await GetStringAsync("String_AttributeRESShort", strIntoLanguage);
                        break;

                    case "DEP":
                        strReturn = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? GetString("String_AttributeDEPShort", strIntoLanguage)
                            : await GetStringAsync("String_AttributeDEPShort", strIntoLanguage);
                        break;

                    case "Physical":
                        strReturn = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? GetString("Node_Physical", strIntoLanguage)
                            : await GetStringAsync("Node_Physical", strIntoLanguage);
                        break;

                    case "Mental":
                        strReturn = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? GetString("Node_Mental", strIntoLanguage)
                            : await GetStringAsync("Node_Mental", strIntoLanguage);
                        break;

                    case "Social":
                        strReturn = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? GetString("Node_Social", strIntoLanguage)
                            : await GetStringAsync("Node_Social", strIntoLanguage);
                        break;

                    case "Left":
                        strReturn = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? GetString("String_Improvement_SideLeft", strIntoLanguage)
                            : await GetStringAsync("String_Improvement_SideLeft", strIntoLanguage);
                        break;

                    case "Right":
                        strReturn = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? GetString("String_Improvement_SideRight", strIntoLanguage)
                            : await GetStringAsync("String_Improvement_SideRight", strIntoLanguage);
                        break;

                    case "All":
                        strReturn = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? GetString("String_All", strIntoLanguage)
                            : await GetStringAsync("String_All", strIntoLanguage);
                        break;

                    case "None":
                        strReturn = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? GetString("String_None", strIntoLanguage)
                            : await GetStringAsync("String_None", strIntoLanguage);
                        break;

                    default:
                        strReturn = strExtra;
                        Match objFileSpecifier = s_RgxExtraFileSpecifierExpression.Match(strReturn);
                        if (objFileSpecifier.Success)
                        {
                            strReturn = strReturn.TrimStartOnce(objFileSpecifier.Value).Trim();
                            if (string.IsNullOrEmpty(strPreferFile))
                                strPreferFile = objFileSpecifier.Value.Trim('[', ']');
                        }
                        string strTemp = string.Empty;
                        string strExtraNoQuotes = strReturn.FastEscape('\"');
                        CancellationTokenSource objCancellationTokenSource = new CancellationTokenSource();
                        CancellationToken objCancellationToken = objCancellationTokenSource.Token;
                        if (!string.IsNullOrEmpty(strPreferFile))
                        {
                            if (blnSync)
                            {
                                strTemp = FindString(strPreferFile);
                            }
                            else
                            {
                                try
                                {
                                    strTemp = await Task.Run(() => FindString(strPreferFile, objCancellationToken),
                                                             objCancellationToken);
                                }
                                catch (OperationCanceledException)
                                {
                                    //swallow this
                                }
                            }

                            if (!string.IsNullOrEmpty(strTemp))
                                strReturn = strTemp;
                        }

                        if (objCancellationToken.IsCancellationRequested)
                            break;

                        if (blnSync)
                        {
                            strTemp = FindString();
                        }
                        else
                        {
                            try
                            {
                                strTemp = await Task.Run(() => FindString(innerToken: objCancellationToken), objCancellationToken);
                            }
                            catch (OperationCanceledException)
                            {
                                //swallow this
                            }
                        }

                        string FindString(string strPreferredFileName = "", CancellationToken innerToken = default)
                        {
                            string strInnerReturn = string.Empty;
                            foreach (IReadOnlyList<Tuple<string, string, Func<XPathNavigator, string>,
                                         Func<XPathNavigator, string>>> aobjPaths
                                     in s_LstAXPathsToSearch)
                            {
                                IEnumerable<Tuple<string, string, Func<XPathNavigator, string>,
                                    Func<XPathNavigator, string>>> lstToSearch
                                    = !string.IsNullOrEmpty(strPreferredFileName)
                                        ? aobjPaths.Where(x => x.Item1 == strPreferredFileName)
                                        : aobjPaths;
                                Parallel.ForEach(lstToSearch, () => string.Empty, (objXPathPair, objState, x) =>
                                {
                                    if (objState.ShouldExitCurrentIteration)
                                        return string.Empty;
                                    if (innerToken.IsCancellationRequested)
                                    {
                                        objState.Stop();
                                        return string.Empty;
                                    }

                                    XPathNavigator xmlDocument = XmlManager.LoadXPath(
                                        objXPathPair.Item1,
                                        objCharacter?.Settings.EnabledCustomDataDirectoryPaths,
                                        strIntoLanguage);
                                    if (objState.ShouldExitCurrentIteration)
                                        return string.Empty;
                                    if (innerToken.IsCancellationRequested)
                                    {
                                        objState.Stop();
                                        return string.Empty;
                                    }

                                    foreach (XPathNavigator objNode in xmlDocument.SelectAndCacheExpression(
                                                 objXPathPair.Item2))
                                    {
                                        if (objState.ShouldExitCurrentIteration)
                                            return string.Empty;
                                        if (innerToken.IsCancellationRequested)
                                        {
                                            objState.Stop();
                                            return string.Empty;
                                        }

                                        if (objXPathPair.Item3(objNode) != strExtraNoQuotes)
                                            continue;
                                        string strTranslate = objXPathPair.Item4(objNode);
                                        if (string.IsNullOrEmpty(strTranslate))
                                            continue;
                                        return strTranslate;
                                    }

                                    return string.Empty;
                                }, strFound =>
                                {
                                    if (innerToken.IsCancellationRequested)
                                        return;
                                    if (string.IsNullOrEmpty(strFound))
                                        return;
                                    strInnerReturn = strFound;
                                    objCancellationTokenSource.Cancel(false);
                                });
                                if (!string.IsNullOrEmpty(strInnerReturn))
                                    return strInnerReturn;
                            }

                            return strInnerReturn;
                        }

                        if (!string.IsNullOrEmpty(strTemp))
                            strReturn = strTemp;

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
        /// <param name="strPreferFile">Name of a file to prefer for extras before all others.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReverseTranslateExtra(string strExtra, string strFromLanguage = "",
            Character objCharacter = null, string strPreferFile = "")
        {
            // This task can normally end up locking up the UI thread because of the Parallel.Foreach call, so we manually schedule it and intermittently do events while waiting for it
            // Because of how ubiquitous this method is, setting it to async so that we can await this instead would require a massive overhaul.
            // TODO: Do this overhaul.
            return Utils.RunWithoutThreadLock(() => ReverseTranslateExtraCoreAsync(true, strExtra, strFromLanguage, objCharacter, strPreferFile));
        }

        /// <summary>
        /// Attempt to translate any Extra text for an item from a foreign language to the default one.
        /// </summary>
        /// <param name="strExtra">Extra string to translate.</param>
        /// <param name="objCharacter">Character whose custom data to use. If null, will not use any custom data.</param>
        /// <param name="strFromLanguage">Language from which the string should be translated</param>
        /// <param name="strPreferFile">Name of a file to prefer for extras before all others.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<string> ReverseTranslateExtraAsync(string strExtra, string strFromLanguage = "",
                                                                    Character objCharacter = null,
                                                                    string strPreferFile = "")
        {
            return ReverseTranslateExtraCoreAsync(false, strExtra, strFromLanguage, objCharacter, strPreferFile);
        }

        /// <summary>
        /// Attempt to translate any Extra text for an item from a foreign language to the default one.
        /// Uses flag hack method design outlined here to avoid locking:
        /// https://docs.microsoft.com/en-us/archive/msdn-magazine/2015/july/async-programming-brownfield-async-development
        /// </summary>
        /// <param name="blnSync">Flag for whether method should always use synchronous code or not.</param>
        /// <param name="strExtra">Extra string to translate.</param>
        /// <param name="objCharacter">Character whose custom data to use. If null, will not use any custom data.</param>
        /// <param name="strFromLanguage">Language from which the string should be translated</param>
        /// <param name="strPreferFile">Name of a file to prefer for extras before all others.</param>
        public static async Task<string> ReverseTranslateExtraCoreAsync(bool blnSync, string strExtra, string strFromLanguage,
                                                                        Character objCharacter, string strPreferFile)
        {
            if (string.IsNullOrEmpty(strFromLanguage))
                strFromLanguage = GlobalSettings.Language;
            // Only attempt to translate if we're not using English. Don't attempt to translate an empty string either.
            if (strFromLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(strExtra))
                return strExtra;
            // Attempt to translate CharacterAttribute names.
            // ReSharper disable once MethodHasAsyncOverload
            if (strExtra == (blnSync ? GetString("String_AttributeBODShort", strFromLanguage) : await GetStringAsync("String_AttributeBODShort", strFromLanguage)))
                return "BOD";
            // ReSharper disable once MethodHasAsyncOverload
            if (strExtra == (blnSync ? GetString("String_AttributeAGIShort", strFromLanguage) : await GetStringAsync("String_AttributeAGIShort", strFromLanguage)))
                return "AGI";
            // ReSharper disable once MethodHasAsyncOverload
            if (strExtra == (blnSync ? GetString("String_AttributeREAShort", strFromLanguage) : await GetStringAsync("String_AttributeREAShort", strFromLanguage)))
                return "REA";
            // ReSharper disable once MethodHasAsyncOverload
            if (strExtra == (blnSync ? GetString("String_AttributeSTRShort", strFromLanguage) : await GetStringAsync("String_AttributeSTRShort", strFromLanguage)))
                return "STR";
            // ReSharper disable once MethodHasAsyncOverload
            if (strExtra == (blnSync ? GetString("String_AttributeCHAShort", strFromLanguage) : await GetStringAsync("String_AttributeCHAShort", strFromLanguage)))
                return "CHA";
            // ReSharper disable once MethodHasAsyncOverload
            if (strExtra == (blnSync ? GetString("String_AttributeINTShort", strFromLanguage) : await GetStringAsync("String_AttributeINTShort", strFromLanguage)))
                return "INT";
            // ReSharper disable once MethodHasAsyncOverload
            if (strExtra == (blnSync ? GetString("String_AttributeLOGShort", strFromLanguage) : await GetStringAsync("String_AttributeLOGShort", strFromLanguage)))
                return "LOG";
            // ReSharper disable once MethodHasAsyncOverload
            if (strExtra == (blnSync ? GetString("String_AttributeWILShort", strFromLanguage) : await GetStringAsync("String_AttributeWILShort", strFromLanguage)))
                return "WIL";
            // ReSharper disable once MethodHasAsyncOverload
            if (strExtra == (blnSync ? GetString("String_AttributeEDGShort", strFromLanguage) : await GetStringAsync("String_AttributeEDGShort", strFromLanguage)))
                return "EDG";
            // ReSharper disable once MethodHasAsyncOverload
            if (strExtra == (blnSync ? GetString("String_AttributeMAGShort", strFromLanguage) : await GetStringAsync("String_AttributeMAGShort", strFromLanguage)))
                return "MAG";
            // ReSharper disable once MethodHasAsyncOverload
            if (strExtra == (blnSync ? MAGAdeptString(strFromLanguage) : await MAGAdeptStringAsync(strFromLanguage)))
                return "MAGAdept";
            // ReSharper disable once MethodHasAsyncOverload
            if (strExtra == (blnSync ? GetString("String_AttributeRESShort", strFromLanguage) : await GetStringAsync("String_AttributeRESShort", strFromLanguage)))
                return "RES";
            // ReSharper disable once MethodHasAsyncOverload
            if (strExtra == (blnSync ? GetString("String_AttributeDEPShort", strFromLanguage) : await GetStringAsync("String_AttributeDEPShort", strFromLanguage)))
                return "DEP";
            // ReSharper disable once MethodHasAsyncOverload
            if (strExtra == (blnSync ? GetString("Node_Physical", strFromLanguage) : await GetStringAsync("Node_Physical", strFromLanguage)))
                return "Physical";
            // ReSharper disable once MethodHasAsyncOverload
            if (strExtra == (blnSync ? GetString("Node_Mental", strFromLanguage) : await GetStringAsync("Node_Mental", strFromLanguage)))
                return "Mental";
            // ReSharper disable once MethodHasAsyncOverload
            if (strExtra == (blnSync ? GetString("Node_Social", strFromLanguage) : await GetStringAsync("Node_Social", strFromLanguage)))
                return "Social";
            // ReSharper disable once MethodHasAsyncOverload
            if (strExtra == (blnSync ? GetString("String_Improvement_SideLeft", strFromLanguage) : await GetStringAsync("String_Improvement_SideLeft", strFromLanguage)))
                return "Left";
            // ReSharper disable once MethodHasAsyncOverload
            if (strExtra == (blnSync ? GetString("String_Improvement_SideRight", strFromLanguage) : await GetStringAsync("String_Improvement_SideRight", strFromLanguage)))
                return "Right";
            // ReSharper disable once MethodHasAsyncOverload
            if (strExtra == (blnSync ? GetString("String_All", strFromLanguage) : await GetStringAsync("String_All", strFromLanguage)))
                return "All";
            // ReSharper disable once MethodHasAsyncOverload
            if (strExtra == (blnSync ? GetString("String_None", strFromLanguage) : await GetStringAsync("String_None", strFromLanguage)))
                return "None";
            // If no original could be found, just use whatever we were passed.
            string strReturn = strExtra;
            Match objFileSpecifier = s_RgxExtraFileSpecifierExpression.Match(strReturn);
            if (objFileSpecifier.Success)
            {
                strReturn = strReturn.TrimStartOnce(objFileSpecifier.Value).Trim();
                if (string.IsNullOrEmpty(strPreferFile))
                    strPreferFile = objFileSpecifier.Value.Trim('[', ']');
            }
            string strTemp = string.Empty;
            string strExtraNoQuotes = strReturn.FastEscape('\"');
            CancellationTokenSource objCancellationTokenSource = new CancellationTokenSource();
            CancellationToken objCancellationToken = objCancellationTokenSource.Token;
            if (!string.IsNullOrEmpty(strPreferFile))
            {
                if (blnSync)
                {
                    strTemp = FindString(strPreferFile);
                }
                else
                {
                    try
                    {
                        strTemp = await Task.Run(() => FindString(strPreferFile, objCancellationToken), objCancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        //swallow this
                    }
                }

                if (!string.IsNullOrEmpty(strTemp))
                    strReturn = strTemp;
            }

            if (objCancellationToken.IsCancellationRequested)
                return strReturn;

            if (blnSync)
            {
                strTemp = FindString();
            }
            else
            {
                try
                {
                    strTemp = await Task.Run(() => FindString(innerToken: objCancellationToken), objCancellationToken);
                }
                catch (OperationCanceledException)
                {
                    //swallow this
                }
            }

            string FindString(string strPreferredFileName = "", CancellationToken innerToken = default)
            {
                string strInnerReturn = string.Empty;
                foreach (IReadOnlyList<Tuple<string, string, Func<XPathNavigator, string>,
                                 Func<XPathNavigator, string>>>
                             aobjPaths
                         in s_LstAXPathsToSearch)
                {
                    IEnumerable<Tuple<string, string, Func<XPathNavigator, string>,
                        Func<XPathNavigator, string>>> lstToSearch
                        = !string.IsNullOrEmpty(strPreferredFileName)
                            ? aobjPaths.Where(x => x.Item1 == strPreferredFileName)
                            : aobjPaths;
                    Parallel.ForEach(lstToSearch, () => string.Empty, (objXPathPair, objState, x) =>
                    {
                        if (objState.ShouldExitCurrentIteration)
                            return string.Empty;
                        if (innerToken.IsCancellationRequested)
                        {
                            objState.Stop();
                            return string.Empty;
                        }
                        XPathNavigator xmlDocument = XmlManager.LoadXPath(objXPathPair.Item1,
                                                                          objCharacter?.Settings
                                                                              .EnabledCustomDataDirectoryPaths,
                                                                          strFromLanguage);
                        if (objState.ShouldExitCurrentIteration)
                            return string.Empty;
                        if (innerToken.IsCancellationRequested)
                        {
                            objState.Stop();
                            return string.Empty;
                        }
                        foreach (XPathNavigator objNode in xmlDocument.SelectAndCacheExpression(
                                     objXPathPair.Item2))
                        {
                            if (objState.ShouldExitCurrentIteration)
                                return string.Empty;
                            if (innerToken.IsCancellationRequested)
                            {
                                objState.Stop();
                                return string.Empty;
                            }
                            if (objXPathPair.Item4(objNode) != strExtraNoQuotes)
                                continue;
                            string strOriginal = objXPathPair.Item3(objNode);
                            if (string.IsNullOrEmpty(strOriginal))
                                continue;
                            return strOriginal;
                        }
                        return string.Empty;
                    }, strFound =>
                    {
                        if (innerToken.IsCancellationRequested)
                            return;
                        if (string.IsNullOrEmpty(strFound))
                            return;
                        strInnerReturn = strFound;
                        objCancellationTokenSource.Cancel(false);
                    });
                    if (!string.IsNullOrEmpty(strInnerReturn))
                        return strInnerReturn;
                }
                return strInnerReturn;
            }

            if (!string.IsNullOrEmpty(strTemp))
                strReturn = strTemp;
            return strReturn;
        }

        public static void PopulateSheetLanguageList(ElasticComboBox cboLanguage, string strSelectedSheet, IEnumerable<Character> lstCharacters = null, CultureInfo defaultCulture = null)
        {
            if (cboLanguage == null)
                throw new ArgumentNullException(nameof(cboLanguage));
            string strDefaultSheetLanguage = defaultCulture?.Name.ToLowerInvariant() ?? GlobalSettings.Language;
            int? intLastIndexDirectorySeparator = strSelectedSheet?.LastIndexOf(Path.DirectorySeparatorChar);
            if (intLastIndexDirectorySeparator.HasValue && intLastIndexDirectorySeparator != -1)
            {
                string strSheetLanguage = strSelectedSheet.Substring(0, intLastIndexDirectorySeparator.Value);
                if (strSheetLanguage.Length == 5)
                    strDefaultSheetLanguage = strSheetLanguage;
            }

            List<ListItem> lstSheetLanguageList = GetSheetLanguageList(lstCharacters, true);
            try
            {
                cboLanguage.PopulateWithListItems(lstSheetLanguageList);
                cboLanguage.DoThreadSafe(x =>
                {
                    x.SelectedValue = strDefaultSheetLanguage;
                    if (x.SelectedIndex == -1)
                        x.SelectedValue
                            = defaultCulture?.Name.ToLowerInvariant() ?? GlobalSettings.DefaultLanguage;
                });
            }
            finally
            {
                Utils.ListItemListPool.Return(lstSheetLanguageList);
            }
        }

        public static Task PopulateSheetLanguageListAsync(ElasticComboBox cboLanguage, string strSelectedSheet, IEnumerable<Character> lstCharacters = null, CultureInfo defaultCulture = null)
        {
            return cboLanguage == null
                ? Task.FromException(new ArgumentNullException(nameof(cboLanguage)))
                : PopulateSheetLanguageListAsyncInner();
            async Task PopulateSheetLanguageListAsyncInner()
            {
                string strDefaultSheetLanguage = defaultCulture?.Name.ToLowerInvariant() ?? GlobalSettings.Language;
                int? intLastIndexDirectorySeparator = strSelectedSheet?.LastIndexOf(Path.DirectorySeparatorChar);
                if (intLastIndexDirectorySeparator.HasValue && intLastIndexDirectorySeparator != -1)
                {
                    string strSheetLanguage = strSelectedSheet.Substring(0, intLastIndexDirectorySeparator.Value);
                    if (strSheetLanguage.Length == 5)
                        strDefaultSheetLanguage = strSheetLanguage;
                }

                List<ListItem> lstSheetLanguageList = await GetSheetLanguageListAsync(lstCharacters, true);
                try
                {
                    await cboLanguage.PopulateWithListItemsAsync(lstSheetLanguageList);
                    await cboLanguage.DoThreadSafeAsync(x =>
                    {
                        x.SelectedValue = strDefaultSheetLanguage;
                        if (x.SelectedIndex == -1)
                            x.SelectedValue
                                = defaultCulture?.Name.ToLowerInvariant() ?? GlobalSettings.DefaultLanguage;
                    });
                }
                finally
                {
                    Utils.ListItemListPool.Return(lstSheetLanguageList);
                }
            }
        }

        public static List<ListItem> GetSheetLanguageList(IEnumerable<Character> lstCharacters = null, bool blnUsePool = false)
        {
            string languageDirectoryPath = Path.Combine(Utils.GetStartupPath, "lang");
            List<Character> lstCharacterToUse = lstCharacters?.ToList();
            string[] astrFilePaths = Directory.GetFiles(languageDirectoryPath, "*.xml");
            List<ListItem> lstLanguages = blnUsePool ? Utils.ListItemListPool.Get() : new List<ListItem>(astrFilePaths.Length);
            foreach (string filePath in astrFilePaths)
            {
                XPathDocument xmlDocument;
                try
                {
                    using (StreamReader objStreamReader = new StreamReader(filePath, Encoding.UTF8, true))
                    using (XmlReader objXmlReader = XmlReader.Create(objStreamReader, GlobalSettings.SafeXmlReaderSettings))
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

                XPathNavigator node = xmlDocument.CreateNavigator().SelectSingleNodeAndCacheExpression("/chummer/name");

                if (node == null)
                    continue;

                string strLanguageCode = Path.GetFileNameWithoutExtension(filePath);
                if (XmlManager.AnyXslFiles(strLanguageCode, lstCharacterToUse))
                {
                    lstLanguages.Add(new ListItem(strLanguageCode, node.Value));
                }
            }
            lstLanguages.Sort(CompareListItems.CompareNames);
            return lstLanguages;
        }

        public static async Task<List<ListItem>> GetSheetLanguageListAsync(IEnumerable<Character> lstCharacters = null, bool blnUsePool = false)
        {
            string languageDirectoryPath = Path.Combine(Utils.GetStartupPath, "lang");
            List<Character> lstCharacterToUse = lstCharacters?.ToList();
            string[] astrFilePaths = Directory.GetFiles(languageDirectoryPath, "*.xml");
            List<ListItem> lstLanguages = blnUsePool ? Utils.ListItemListPool.Get() : new List<ListItem>(astrFilePaths.Length);
            foreach (string filePath in astrFilePaths)
            {
                XPathDocument xmlDocument;
                try
                {
                    using (StreamReader objStreamReader = new StreamReader(filePath, Encoding.UTF8, true))
                    using (XmlReader objXmlReader = XmlReader.Create(objStreamReader, GlobalSettings.SafeXmlReaderSettings))
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

                XPathNavigator node = await xmlDocument.CreateNavigator().SelectSingleNodeAndCacheExpressionAsync("/chummer/name");

                if (node == null)
                    continue;

                string strLanguageCode = Path.GetFileNameWithoutExtension(filePath);
                if (await XmlManager.AnyXslFilesAsync(strLanguageCode, lstCharacterToUse))
                {
                    lstLanguages.Add(new ListItem(strLanguageCode, node.Value));
                }
            }
            lstLanguages.Sort(CompareListItems.CompareNames);
            return lstLanguages;
        }

        #endregion Methods
    }

    public class LanguageData
    {
        public bool IsRightToLeftScript { get; }
        public Dictionary<string, string> TranslatedStrings { get; } = new Dictionary<string, string>();
        public XPathDocument DataDocument { get; }
        public string ErrorMessage { get; } = string.Empty;
        public bool ErrorAlreadyShown { get; set; }

        public LanguageData(string strLanguage)
        {
            string strFilePath = Path.Combine(Utils.GetStartupPath, "lang", strLanguage + ".xml");
            if (!File.Exists(strFilePath))
                strFilePath = Path.Combine(Utils.GetStartupPath, "lang", strLanguage.ToLowerInvariant() + ".xml");
            if (File.Exists(strFilePath))
            {
                try
                {
                    XPathDocument objLanguageDocument;
                    string strExtraMessage = string.Empty;
                    try
                    {
                        using (StreamReader objStreamReader = new StreamReader(strFilePath, Encoding.UTF8, true))
                        using (XmlReader objXmlReader = XmlReader.Create(objStreamReader, GlobalSettings.SafeXmlReaderSettings))
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
                        IsRightToLeftScript = objLanguageDocumentNavigator.SelectSingleNodeAndCacheExpression("/chummer/righttoleft")?.Value == bool.TrueString;
                        XPathNodeIterator xmlStringList =
                            objLanguageDocumentNavigator.SelectAndCacheExpression("/chummer/strings/string");
                        if (xmlStringList.Count > 0)
                        {
                            foreach (XPathNavigator objNode in xmlStringList)
                            {
                                // Look for the English version of the found string. If it has been found, replace the English contents with the contents from this file.
                                // If the string was not found, then someone has inserted a Key that should not exist and is ignored.
                                string strKey = objNode.SelectSingleNodeAndCacheExpression("key")?.Value;
                                string strText = objNode.SelectSingleNodeAndCacheExpression("text")?.Value;
                                if (!string.IsNullOrEmpty(strKey) && !string.IsNullOrEmpty(strText))
                                    TranslatedStrings[strKey] = strText.NormalizeLineEndings(true);
                            }
                        }
                        else
                        {
                            ErrorMessage = "Failed to load the strings file " + strLanguage + ".xml into an XmlDocument: " + strExtraMessage + '.';
                        }
                    }
                    else
                    {
                        ErrorMessage = "Failed to load the strings file " + strLanguage + ".xml into an XmlDocument: " + strExtraMessage + '.';
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage = "Encountered the following the exception while loading " + strLanguage + ".xml into an XmlDocument: " + ex + '.';
                }
            }
            else
            {
                ErrorMessage = "Could not find the strings file " + strLanguage + ".xml.";
            }

            // Check to see if the data translation file for the selected language exists.
            string strDataPath = Path.Combine(Utils.GetStartupPath, "lang", strLanguage + "_data.xml");
            if (!File.Exists(strDataPath))
                strDataPath = Path.Combine(Utils.GetStartupPath, "lang", strLanguage.ToLowerInvariant() + "_data.xml");
            if (File.Exists(strDataPath))
            {
                try
                {
                    try
                    {
                        using (StreamReader objStreamReader = new StreamReader(strDataPath, Encoding.UTF8, true))
                        using (XmlReader objXmlReader = XmlReader.Create(objStreamReader, GlobalSettings.SafeXmlReaderSettings))
                            DataDocument = new XPathDocument(objXmlReader);
                    }
                    catch (IOException ex)
                    {
                        DataDocument = null;
                        if (!string.IsNullOrEmpty(ErrorMessage))
                            ErrorMessage += Environment.NewLine;
                        ErrorMessage += "Failed to load the data file " + strLanguage + "_data.xml into an XmlDocument: " + ex + '.';
                    }
                    catch (XmlException ex)
                    {
                        DataDocument = null;
                        if (!string.IsNullOrEmpty(ErrorMessage))
                            ErrorMessage += Environment.NewLine;
                        ErrorMessage += "Failed to load the data file " + strLanguage + "_data.xml into an XmlDocument: " + ex + '.';
                    }
                }
                catch (Exception ex)
                {
                    DataDocument = null;
                    if (!string.IsNullOrEmpty(ErrorMessage))
                        ErrorMessage += Environment.NewLine;
                    ErrorMessage += "Encountered the following the exception while loading " + strLanguage + "_data.xml into an XmlDocument: " + ex + '.';
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(ErrorMessage))
                    ErrorMessage += Environment.NewLine;
                ErrorMessage += "Could not find the data file " + strLanguage + "_data.xml.";
            }
        }
    }
}
