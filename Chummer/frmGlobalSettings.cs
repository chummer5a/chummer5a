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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using NLog;
using Application = System.Windows.Forms.Application;

namespace Chummer
{
    public partial class frmGlobalSettings : Form
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();

        // List of custom data directories possible to be added to a character
        private readonly HashSet<CustomDataDirectoryInfo> _setCustomDataDirectoryInfos;

        // List of sourcebook infos, needed to make sure we don't directly modify ones in the options unless we save our options
        private readonly Dictionary<string, SourcebookInfo> _dicSourcebookInfos;

        private bool _blnSkipRefresh;
        private bool _blnDirty;
        private bool _blnLoading = true;
        private string _strSelectedLanguage = GlobalSettings.Language;
        private CultureInfo _objSelectedCultureInfo = GlobalSettings.CultureInfo;
        private ColorMode _eSelectedColorModeSetting = GlobalSettings.ColorModeSetting;

        #region Form Events

        public frmGlobalSettings(string strActiveTab = "")
        {
            InitializeComponent();
#if !DEBUG
            // tabPage3 only contains cmdUploadPastebin, which is not used if DEBUG is not enabled
            // Remove this line if cmdUploadPastebin_Click has some functionality if DEBUG is not enabled or if tabPage3 gets some other control that can be used if DEBUG is not enabled
            tabOptions.TabPages.Remove(tabGitHubIssues);
#endif
            this.UpdateLightDarkMode();
            this.TranslateWinForm(_strSelectedLanguage);

            _setCustomDataDirectoryInfos = new HashSet<CustomDataDirectoryInfo>(GlobalSettings.CustomDataDirectoryInfos);
            _dicSourcebookInfos = new Dictionary<string, SourcebookInfo>(GlobalSettings.SourcebookInfos);
            if (!string.IsNullOrEmpty(strActiveTab))
            {
                int intActiveTabIndex = tabOptions.TabPages.IndexOfKey(strActiveTab);
                if (intActiveTabIndex > 0)
                    tabOptions.SelectedTab = tabOptions.TabPages[intActiveTabIndex];
            }
        }

        private void frmOptions_Load(object sender, EventArgs e)
        {
            PopulateDefaultCharacterSettingLists();
            PopulateMugshotCompressionOptions();
            SetToolTips();
            PopulateOptions();
            PopulateLanguageList();
            SetDefaultValueForLanguageList();
            PopulateSheetLanguageList();
            SetDefaultValueForSheetLanguageList();
            PopulateXsltList();
            SetDefaultValueForXsltList();
            PopulatePdfParameters();

            _blnLoading = false;

            if (_blnPromptPdfReaderOnLoad)
            {
                _blnPromptPdfReaderOnLoad = false;
                PromptPdfAppPath();
            }

            if (!string.IsNullOrEmpty(_strSelectCodeOnRefresh))
            {
                lstGlobalSourcebookInfos.SelectedValue = _strSelectCodeOnRefresh;
                if (lstGlobalSourcebookInfos.SelectedIndex >= 0)
                    PromptPdfLocation();
                _strSelectCodeOnRefresh = string.Empty;
            }
        }

        #endregion Form Events

        #region Control Events

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (_blnDirty)
            {
                string text = LanguageManager.GetString("Message_Options_SaveForms", _strSelectedLanguage);
                string caption = LanguageManager.GetString("MessageTitle_Options_CloseForms", _strSelectedLanguage);

                if (Program.MainForm.ShowMessageBox(this, text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
            }

            DialogResult = DialogResult.OK;
            SaveRegistrySettings();

            if (_blnDirty)
                Utils.RestartApplication(_strSelectedLanguage, "Message_Options_CloseForms");
        }

        private void cboLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            _strSelectedLanguage = cboLanguage.SelectedValue?.ToString() ?? GlobalSettings.DefaultLanguage;
            try
            {
                _objSelectedCultureInfo = CultureInfo.GetCultureInfo(_strSelectedLanguage);
            }
            catch (CultureNotFoundException)
            {
                _objSelectedCultureInfo = GlobalSettings.SystemCultureInfo;
            }

            imgLanguageFlag.Image = Math.Min(imgLanguageFlag.Width, imgLanguageFlag.Height) >= 32
                ? FlagImageGetter.GetFlagFromCountryCode192Dpi(_strSelectedLanguage.Substring(3, 2))
                : FlagImageGetter.GetFlagFromCountryCode(_strSelectedLanguage.Substring(3, 2));

            bool isEnabled = !string.IsNullOrEmpty(_strSelectedLanguage) && !_strSelectedLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase);
            cmdVerify.Enabled = isEnabled;
            cmdVerifyData.Enabled = isEnabled;

            if (!_blnLoading)
            {
                using (new CursorWait(this))
                {
                    _blnLoading = true;
                    TranslateForm();
                    _blnLoading = false;
                }
            }

            OptionsChanged(sender, e);
        }

        private void cboSheetLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            using (new CursorWait(this))
                PopulateXsltList();
        }

        private async void cmdVerify_Click(object sender, EventArgs e)
        {
            using (new CursorWait(this))
                await LanguageManager.VerifyStrings(_strSelectedLanguage);
        }

        private void cmdVerifyData_Click(object sender, EventArgs e)
        {
            using (new CursorWait(this))
            {
                // Build a list of Sourcebooks that will be passed to the Verify method.
                // This is done since not all of the books are available in every language or the user may only wish to verify the content of certain books.
                HashSet<string> setBooks = new HashSet<string>();
                foreach (ListItem objItem in lstGlobalSourcebookInfos.Items)
                {
                    string strItemValue = objItem.Value?.ToString();
                    setBooks.Add(strItemValue);
                }

                string strSelectedLanguage = _strSelectedLanguage;
                XmlManager.Verify(strSelectedLanguage, setBooks);

                string strFilePath = Path.Combine(Utils.GetStartupPath, "lang", "results_" + strSelectedLanguage + ".xml");
                Program.MainForm.ShowMessageBox(this, string.Format(_objSelectedCultureInfo, LanguageManager.GetString("Message_Options_ValidationResults", _strSelectedLanguage), strFilePath),
                    LanguageManager.GetString("MessageTitle_Options_ValidationResults", _strSelectedLanguage), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void cmdPDFAppPath_Click(object sender, EventArgs e)
        {
            PromptPdfAppPath();
        }

        private void cmdPDFLocation_Click(object sender, EventArgs e)
        {
            PromptPdfLocation();
        }

        private void lstGlobalSourcebookInfos_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strSelectedCode = lstGlobalSourcebookInfos.SelectedValue?.ToString() ?? string.Empty;

            // Find the selected item in the Sourcebook List.
            SourcebookInfo objSource = _dicSourcebookInfos.ContainsKey(strSelectedCode) ? _dicSourcebookInfos[strSelectedCode] : null;

            if (objSource != null)
            {
                grpSelectedSourcebook.Enabled = true;
                txtPDFLocation.Text = objSource.Path;
                nudPDFOffset.Value = objSource.Offset;
            }
            else
            {
                grpSelectedSourcebook.Enabled = false;
            }
        }

        private void nudPDFOffset_ValueChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh || _blnLoading)
                return;

            int intOffset = decimal.ToInt32(nudPDFOffset.Value);
            string strTag = lstGlobalSourcebookInfos.SelectedValue?.ToString() ?? string.Empty;
            SourcebookInfo objFoundSource = _dicSourcebookInfos.ContainsKey(strTag) ? _dicSourcebookInfos[strTag] : null;

            if (objFoundSource != null)
            {
                objFoundSource.Offset = intOffset;
            }
            else
            {
                // If the Sourcebook was not found in the options, add it.
                _dicSourcebookInfos.Add(strTag, new SourcebookInfo
                {
                    Code = strTag,
                    Offset = intOffset
                });
            }
        }

        private void cmdPDFTest_Click(object sender, EventArgs e)
        {
            using (new CursorWait(this))
                CommonFunctions.OpenPdf(lstGlobalSourcebookInfos.SelectedValue + " 5", null, cboPDFParameters.SelectedValue?.ToString() ?? string.Empty, txtPDFAppPath.Text);
        }

        private void cboUseLoggingApplicationInsights_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            UseAILogging useAI = (UseAILogging)((ListItem)cboUseLoggingApplicationInsights.SelectedItem).Value;
            GlobalSettings.UseLoggingResetCounter = 10;
            if (useAI > UseAILogging.Info
                && GlobalSettings.UseLoggingApplicationInsightsPreference <= UseAILogging.Info
                && DialogResult.Yes != Program.MainForm.ShowMessageBox(this,
                    LanguageManager.GetString("Message_Options_ConfirmTelemetry", _strSelectedLanguage).WordWrap(),
                    LanguageManager.GetString("MessageTitle_Options_ConfirmTelemetry", _strSelectedLanguage),
                    MessageBoxButtons.YesNo))
            {
                _blnLoading = true;
                cboUseLoggingApplicationInsights.SelectedItem = UseAILogging.Info;
                _blnLoading = false;
                return;
            }
            OptionsChanged(sender, e);
        }

        private void chkUseLogging_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            if (chkUseLogging.Checked && !GlobalSettings.UseLogging && DialogResult.Yes != Program.MainForm.ShowMessageBox(this,
                LanguageManager.GetString("Message_Options_ConfirmDetailedTelemetry", _strSelectedLanguage).WordWrap(),
                LanguageManager.GetString("MessageTitle_Options_ConfirmDetailedTelemetry", _strSelectedLanguage),
                MessageBoxButtons.YesNo))
            {
                _blnLoading = true;
                chkUseLogging.Checked = false;
                _blnLoading = false;
                return;
            }
            cboUseLoggingApplicationInsights.Enabled = chkUseLogging.Checked;
            OptionsChanged(sender, e);
        }

        private void cboUseLoggingHelp_Click(object sender, EventArgs e)
        {
            //open the telemetry document
            Process.Start("https://docs.google.com/document/d/1LThAg6U5qXzHAfIRrH0Kb7griHrPN0hy7ab8FSJDoFY/edit?usp=sharing");
        }

        private void cmdPluginsHelp_Click(object sender, EventArgs e)
        {
            Process.Start("https://docs.google.com/document/d/1WOPB7XJGgcmxg7REWxF6HdP3kQdtHpv6LJOXZtLggxM/edit?usp=sharing");
        }

        private void chkCustomDateTimeFormats_CheckedChanged(object sender, EventArgs e)
        {
            grpDateFormat.Enabled = chkCustomDateTimeFormats.Checked;
            grpTimeFormat.Enabled = chkCustomDateTimeFormats.Checked;
            if (!chkCustomDateTimeFormats.Checked)
            {
                txtDateFormat.Text = GlobalSettings.CultureInfo.DateTimeFormat.ShortDatePattern;
                txtTimeFormat.Text = GlobalSettings.CultureInfo.DateTimeFormat.ShortTimePattern;
            }
            OptionsChanged(sender, e);
        }

        private void txtDateFormat_TextChanged(object sender, EventArgs e)
        {
            try
            {
                txtDateFormatView.Text = DateTime.Now.ToString(txtDateFormat.Text, _objSelectedCultureInfo);
            }
            catch (Exception)
            {
                txtDateFormatView.Text = LanguageManager.GetString("String_Error", _strSelectedLanguage);
            }
            OptionsChanged(sender, e);
        }

        private void txtTimeFormat_TextChanged(object sender, EventArgs e)
        {
            try
            {
                txtTimeFormatView.Text = DateTime.Now.ToString(txtTimeFormat.Text, _objSelectedCultureInfo);
            }
            catch (Exception)
            {
                txtTimeFormatView.Text = LanguageManager.GetString("String_Error", _strSelectedLanguage);
            }
            OptionsChanged(sender, e);
        }

        private void cboMugshotCompression_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            bool blnShowQualitySelector = Equals(cboMugshotCompression.SelectedValue, "jpeg_manual");
            lblMugshotCompressionQuality.Visible = blnShowQualitySelector;
            nudMugshotCompressionQuality.Visible = blnShowQualitySelector;
            OptionsChanged(sender, e);
        }

        private void cboColorMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            using (new CursorWait(this))
            {
                if (Enum.TryParse(cboColorMode.SelectedValue.ToString(), true, out ColorMode eNewColorMode) && _eSelectedColorModeSetting != eNewColorMode)
                {
                    _eSelectedColorModeSetting = eNewColorMode;
                    switch (eNewColorMode)
                    {
                        case ColorMode.Automatic:
                            this.UpdateLightDarkMode(!ColorManager.DoesRegistrySayDarkMode());
                            break;

                        case ColorMode.Light:
                            this.UpdateLightDarkMode(true);
                            break;

                        case ColorMode.Dark:
                            this.UpdateLightDarkMode(false);
                            break;
                    }
                }
            }

            OptionsChanged(sender, e);
        }

        private void chkPrintExpenses_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            if (chkPrintExpenses.Checked)
            {
                chkPrintFreeExpenses.Enabled = true;
            }
            else
            {
                chkPrintFreeExpenses.Enabled = false;
                chkPrintFreeExpenses.Checked = false;
            }
            OptionsChanged(sender, e);
        }

        private void chkAutomaticUpdate_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            if (chkAutomaticUpdate.Checked)
            {
                chkPreferNightlyBuilds.Enabled = true;
            }
            else
            {
                chkPreferNightlyBuilds.Enabled = false;
                chkPreferNightlyBuilds.Checked = false;
            }
            OptionsChanged(sender, e);
        }

        private void cmdRemovePDFLocation_Click(object sender, EventArgs e)
        {
            UpdateSourcebookInfoPath(string.Empty);
            txtPDFLocation.Text = string.Empty;
        }

        private void txtPDFLocation_TextChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            cmdRemovePDFLocation.Enabled = txtPDFLocation.TextLength > 0;
            cmdPDFTest.Enabled = txtPDFAppPath.TextLength > 0 && txtPDFLocation.TextLength > 0;
            OptionsChanged(sender, e);
        }

        private void cmdRemoveCharacterRoster_Click(object sender, EventArgs e)
        {
            txtCharacterRosterPath.Text = string.Empty;
            cmdRemoveCharacterRoster.Enabled = false;
            OptionsChanged(sender, e);
        }

        private void cmdRemovePDFAppPath_Click(object sender, EventArgs e)
        {
            txtPDFAppPath.Text = string.Empty;
            cmdRemovePDFAppPath.Enabled = false;
            cmdPDFTest.Enabled = false;
            OptionsChanged(sender, e);
        }

        private void chkLifeModules_CheckedChanged(object sender, EventArgs e)
        {
            if (!chkLifeModule.Checked || _blnLoading) return;
            if (Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Tip_LifeModule_Warning", _strSelectedLanguage), Application.ProductName,
                   MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
                chkLifeModule.Checked = false;
            else
            {
                OptionsChanged(sender, e);
            }
        }

        private void cmdCharacterRoster_Click(object sender, EventArgs e)
        {
            // Prompt the user to select a save file to associate with this Contact.
            using (FolderBrowserDialog dlgSelectFolder = new FolderBrowserDialog())
            {
                if (dlgSelectFolder.ShowDialog(this) != DialogResult.OK)
                    return;
                txtCharacterRosterPath.Text = dlgSelectFolder.SelectedPath;
            }
            cmdRemoveCharacterRoster.Enabled = txtCharacterRosterPath.TextLength > 0;
            OptionsChanged(sender, e);
        }

        private void cmdAddCustomDirectory_Click(object sender, EventArgs e)
        {
            // Prompt the user to select a save file to associate with this Contact.
            using (FolderBrowserDialog dlgSelectFolder = new FolderBrowserDialog { SelectedPath = Utils.GetStartupPath })
            {
                if (dlgSelectFolder.ShowDialog(this) != DialogResult.OK)
                    return;
                using (frmSelectText frmSelectCustomDirectoryName = new frmSelectText
                {
                    Description = LanguageManager.GetString("String_CustomItem_SelectText", _strSelectedLanguage)
                })
                {
                    if (frmSelectCustomDirectoryName.ShowDialog(this) != DialogResult.OK)
                        return;
                    CustomDataDirectoryInfo objNewCustomDataDirectory = new CustomDataDirectoryInfo(frmSelectCustomDirectoryName.SelectedValue, dlgSelectFolder.SelectedPath);
                    if (objNewCustomDataDirectory.XmlException != default)
                    {
                        Program.MainForm.ShowMessageBox(this,
                            string.Format(_objSelectedCultureInfo, LanguageManager.GetString("Message_FailedLoad", _strSelectedLanguage),
                                objNewCustomDataDirectory.XmlException.Message),
                            string.Format(_objSelectedCultureInfo,
                                LanguageManager.GetString("MessageTitle_FailedLoad", _strSelectedLanguage) +
                                LanguageManager.GetString("String_Space", _strSelectedLanguage) + objNewCustomDataDirectory.Name + Path.DirectorySeparatorChar + "manifest.xml"),
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    if (_setCustomDataDirectoryInfos.Any(x => x.DirectoryPath == objNewCustomDataDirectory.DirectoryPath))
                    {
                        Program.MainForm.ShowMessageBox(this,
                            string.Format(
                                LanguageManager.GetString("Message_Duplicate_CustomDataDirectoryPath",
                                    _strSelectedLanguage), objNewCustomDataDirectory.Name),
                            LanguageManager.GetString("MessageTitle_Duplicate_CustomDataDirectoryPath",
                                _strSelectedLanguage), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    if (_setCustomDataDirectoryInfos.Contains(objNewCustomDataDirectory))
                    {
                        CustomDataDirectoryInfo objExistingInfo = _setCustomDataDirectoryInfos.FirstOrDefault(x => x.Equals(objNewCustomDataDirectory));
                        if (objExistingInfo != null)
                        {
                            if (objNewCustomDataDirectory.HasManifest)
                            {
                                if (objExistingInfo.HasManifest)
                                {
                                    Program.MainForm.ShowMessageBox(
                                        string.Format(
                                            LanguageManager.GetString(
                                                "Message_Duplicate_CustomDataDirectory"),
                                            objExistingInfo.Name, objNewCustomDataDirectory.Name),
                                        LanguageManager.GetString(
                                            "MessageTitle_Duplicate_CustomDataDirectory"),
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }

                                _setCustomDataDirectoryInfos.Remove(objExistingInfo);
                                do
                                {
                                    objExistingInfo.RandomizeGuid();
                                } while (objExistingInfo.Equals(objNewCustomDataDirectory) || _setCustomDataDirectoryInfos.Contains(objExistingInfo));
                                _setCustomDataDirectoryInfos.Add(objExistingInfo);
                            }
                            else
                            {
                                do
                                {
                                    objNewCustomDataDirectory.RandomizeGuid();
                                } while (_setCustomDataDirectoryInfos.Contains(objNewCustomDataDirectory));
                            }
                        }
                    }
                    if (_setCustomDataDirectoryInfos.Any(x =>
                        objNewCustomDataDirectory.CharacterSettingsSaveKey.Equals(x.CharacterSettingsSaveKey,
                            StringComparison.OrdinalIgnoreCase)) && Program.MainForm.ShowMessageBox(this,
                        string.Format(
                            LanguageManager.GetString("Message_Duplicate_CustomDataDirectoryName",
                                _strSelectedLanguage), objNewCustomDataDirectory.Name),
                        LanguageManager.GetString("MessageTitle_Duplicate_CustomDataDirectoryName",
                            _strSelectedLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                        return;
                    _setCustomDataDirectoryInfos.Add(objNewCustomDataDirectory);
                    PopulateCustomDataDirectoryListBox();
                }
            }
        }

        private void cmdRemoveCustomDirectory_Click(object sender, EventArgs e)
        {
            if (lsbCustomDataDirectories.SelectedIndex == -1)
                return;
            ListItem objSelected = (ListItem)lsbCustomDataDirectories.SelectedItem;
            CustomDataDirectoryInfo objInfoToRemove = (CustomDataDirectoryInfo)objSelected.Value;
            if (!_setCustomDataDirectoryInfos.Remove(objInfoToRemove))
                return;
            OptionsChanged(sender, e);
            PopulateCustomDataDirectoryListBox();
        }

        private void cmdRenameCustomDataDirectory_Click(object sender, EventArgs e)
        {
            if (lsbCustomDataDirectories.SelectedIndex == -1)
                return;
            ListItem objSelected = (ListItem)lsbCustomDataDirectories.SelectedItem;
            CustomDataDirectoryInfo objInfoToRename = (CustomDataDirectoryInfo)objSelected.Value;
            using (frmSelectText frmSelectCustomDirectoryName = new frmSelectText
            {
                Description = LanguageManager.GetString("String_CustomItem_SelectText", _strSelectedLanguage)
            })
            {
                if (frmSelectCustomDirectoryName.ShowDialog(this) != DialogResult.OK)
                    return;
                CustomDataDirectoryInfo objNewInfo = new CustomDataDirectoryInfo(frmSelectCustomDirectoryName.SelectedValue, objInfoToRename.DirectoryPath);
                if (!objNewInfo.HasManifest)
                    objNewInfo.CopyGuid(objInfoToRename);
                if (objNewInfo.XmlException != default)
                {
                    Program.MainForm.ShowMessageBox(this,
                        string.Format(_objSelectedCultureInfo, LanguageManager.GetString("Message_FailedLoad", _strSelectedLanguage),
                            objNewInfo.XmlException.Message),
                        string.Format(_objSelectedCultureInfo,
                            LanguageManager.GetString("MessageTitle_FailedLoad", _strSelectedLanguage) +
                            LanguageManager.GetString("String_Space", _strSelectedLanguage) + objNewInfo.Name + Path.DirectorySeparatorChar + "manifest.xml"),
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (_setCustomDataDirectoryInfos.Any(x => x != objInfoToRename &&
                                                          objNewInfo.CharacterSettingsSaveKey.Equals(
                                                              x.CharacterSettingsSaveKey,
                                                              StringComparison.OrdinalIgnoreCase)) &&
                    Program.MainForm.ShowMessageBox(this,
                        string.Format(
                            LanguageManager.GetString("Message_Duplicate_CustomDataDirectoryName",
                                _strSelectedLanguage), objNewInfo.Name),
                        LanguageManager.GetString("MessageTitle_Duplicate_CustomDataDirectoryName",
                            _strSelectedLanguage), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    return;
                _setCustomDataDirectoryInfos.Remove(objInfoToRename);
                _setCustomDataDirectoryInfos.Add(objNewInfo);
                PopulateCustomDataDirectoryListBox();
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void chkEnablePlugins_CheckedChanged(object sender, EventArgs e)
        {
            PluginsShowOrHide(chkEnablePlugins.Checked);
            OptionsChanged(sender, e);
        }

        private async void cmdUploadPastebin_Click(object sender, EventArgs e)
        {
#if DEBUG
            const string strFilePath = "Insert local file here";
            System.Collections.Specialized.NameValueCollection data = new System.Collections.Specialized.NameValueCollection();
            string line;
            using (StreamReader sr = new StreamReader(strFilePath, Encoding.UTF8, true))
            {
                line = await sr.ReadToEndAsync();
            }
            data["api_paste_name"] = "Chummer";
            data["api_paste_expire_date"] = "N";
            data["api_paste_format"] = "xml";
            data["api_paste_code"] = line;
            data["api_dev_key"] = "7845fd372a1050899f522f2d6bab9666";
            data["api_option"] = "paste";

            using (System.Net.WebClient wb = new System.Net.WebClient())
            {
                byte[] bytes;
                try
                {
                    bytes = await wb.UploadValuesTaskAsync("https://pastebin.com/api/api_post.php", data);
                }
                catch (System.Net.WebException)
                {
                    return;
                }

                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    using (StreamReader reader = new StreamReader(ms, Encoding.UTF8, true))
                    {
                        string response = await reader.ReadToEndAsync();
                        Clipboard.SetText(response);
                    }
                }
            }
#endif
        }

        private void clbPlugins_VisibleChanged(object sender, EventArgs e)
        {
            clbPlugins.Items.Clear();
            if (Program.PluginLoader.MyPlugins.Count == 0)
                return;
            using (new CursorWait(this))
            {
                foreach (var plugin in Program.PluginLoader.MyPlugins)
                {
                    try
                    {
                        plugin.CustomInitialize(Program.MainForm);
                        if (GlobalSettings.PluginsEnabledDic.TryGetValue(plugin.ToString(), out var check))
                        {
                            clbPlugins.Items.Add(plugin, check);
                        }
                        else
                        {
                            clbPlugins.Items.Add(plugin);
                        }
                    }
                    catch (ApplicationException ae)
                    {
                        Log.Debug(ae);
                    }
                }

                if (clbPlugins.Items.Count > 0)
                {
                    clbPlugins.SelectedIndex = 0;
                }
            }
        }

        private void clbPlugins_SelectedValueChanged(object sender, EventArgs e)
        {
            UserControl pluginControl = (clbPlugins.SelectedItem as Plugins.IPlugin)?.GetOptionsControl();
            if (pluginControl != null)
            {
                pnlPluginOption.Controls.Clear();
                pnlPluginOption.Controls.Add(pluginControl);
            }
        }

        private void clbPlugins_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            using (new CursorWait(this))
            {
                var plugin = clbPlugins.Items[e.Index];
                if (GlobalSettings.PluginsEnabledDic.ContainsKey(plugin.ToString()))
                    GlobalSettings.PluginsEnabledDic.Remove(plugin.ToString());
                GlobalSettings.PluginsEnabledDic.Add(plugin.ToString(), e.NewValue == CheckState.Checked);
                OptionsChanged(sender, e);
            }
        }

        private void txtPDFAppPath_TextChanged(object sender, EventArgs e)
        {
            cmdRemovePDFAppPath.Enabled = txtPDFAppPath.TextLength > 0;
            cmdPDFTest.Enabled = txtPDFAppPath.TextLength > 0 && txtPDFLocation.TextLength > 0;
            OptionsChanged(sender, e);
        }

        private void lsbCustomDataDirectories_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            ListItem objSelectedItem = (ListItem)lsbCustomDataDirectories.SelectedItem;
            CustomDataDirectoryInfo objSelected = (CustomDataDirectoryInfo)objSelectedItem.Value;
            if (objSelected == null)
            {
                gpbDirectoryInfo.Visible = false;
                return;
            }

            gpbDirectoryInfo.SuspendLayout();
            txtDirectoryDescription.Text = objSelected.GetDisplayDescription(_strSelectedLanguage);
            lblDirectoryVersion.Text = objSelected.MyVersion.ToString();
            lblDirectoryAuthors.Text = objSelected.GetDisplayAuthors(_strSelectedLanguage, _objSelectedCultureInfo);
            lblDirectoryName.Text = objSelected.Name;
            lblDirectoryPath.Text = objSelected.DirectoryPath.Replace(Utils.GetStartupPath, LanguageManager.GetString("String_Chummer5a", _strSelectedLanguage));

            if (objSelected.DependenciesList.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var dependency in objSelected.DependenciesList)
                    sb.AppendLine(dependency.DisplayName);
                lblDependencies.Text = sb.ToString();
            }
            else
            {
                //Make sure all old information is discarded
                lblDependencies.Text = string.Empty;
            }

            if (objSelected.IncompatibilitiesList.Count > 0)
            {
                //We only need a Stringbuilder if we got anything
                StringBuilder sb = new StringBuilder();
                foreach (var exclusivity in objSelected.IncompatibilitiesList)
                    sb.AppendLine(exclusivity.DisplayName);
                lblIncompatibilities.Text = sb.ToString();
            }
            else
            {
                lblIncompatibilities.Text = string.Empty;
            }
            gpbDirectoryInfo.Visible = true;
            gpbDirectoryInfo.ResumeLayout();
        }

        #endregion Control Events

        #region Methods

        private bool _blnPromptPdfReaderOnLoad;

        public void DoLinkPdfReader()
        {
            if (_blnLoading)
                _blnPromptPdfReaderOnLoad = true;
            else
                PromptPdfAppPath();
        }

        private string _strSelectCodeOnRefresh = string.Empty;

        public void DoLinkPdf(string strCode)
        {
            if (_blnLoading)
                _strSelectCodeOnRefresh = strCode;
            else
            {
                lstGlobalSourcebookInfos.SelectedValue = strCode;
                if (lstGlobalSourcebookInfos.SelectedIndex >= 0)
                    PromptPdfLocation();
            }
        }

        private void PromptPdfLocation()
        {
            if (!txtPDFLocation.Enabled)
                return;
            // Prompt the user to select a save file to associate with this Contact.
            using (new CursorWait(this))
            {
                string strNewFileName;
                using (OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = LanguageManager.GetString("DialogFilter_Pdf") + '|' +
                             LanguageManager.GetString("DialogFilter_All")
                })
                {
                    if (!string.IsNullOrEmpty(txtPDFLocation.Text) && File.Exists(txtPDFLocation.Text))
                    {
                        openFileDialog.InitialDirectory = Path.GetDirectoryName(txtPDFLocation.Text);
                        openFileDialog.FileName = Path.GetFileName(txtPDFLocation.Text);
                    }

                    if (openFileDialog.ShowDialog(this) != DialogResult.OK)
                        return;

                    strNewFileName = openFileDialog.FileName;
                }

                try
                {
                    PdfReader objPdfReader = new PdfReader(strNewFileName);
                    objPdfReader.Close();
                }
                catch (Exception)
                {
                    Program.MainForm.ShowMessageBox(this, string.Format(
                                                        LanguageManager.GetString(
                                                            "Message_Options_FileIsNotPDF",
                                                            _strSelectedLanguage), Path.GetFileName(strNewFileName)),
                                                    LanguageManager.GetString(
                                                        "MessageTitle_Options_FileIsNotPDF",
                                                        _strSelectedLanguage), MessageBoxButtons.OK,
                                                    MessageBoxIcon.Error);
                    return;
                }

                UpdateSourcebookInfoPath(strNewFileName);
                txtPDFLocation.Text = strNewFileName;
            }
        }

        private void PromptPdfAppPath()
        {
            // Prompt the user to select a save file to associate with this Contact.
            using (new CursorWait(this))
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = LanguageManager.GetString("DialogFilter_Exe") + '|' +
                             LanguageManager.GetString("DialogFilter_All")
                })
                {
                    if (!string.IsNullOrEmpty(txtPDFAppPath.Text) && File.Exists(txtPDFAppPath.Text))
                    {
                        openFileDialog.InitialDirectory = Path.GetDirectoryName(txtPDFAppPath.Text);
                        openFileDialog.FileName = Path.GetFileName(txtPDFAppPath.Text);
                    }

                    if (openFileDialog.ShowDialog(this) != DialogResult.OK)
                        return;
                    txtPDFAppPath.Text = openFileDialog.FileName;
                }
            }
        }

        private void TranslateForm()
        {
            this.TranslateWinForm(_strSelectedLanguage);
            PopulateDefaultCharacterSettingLists();
            PopulateMugshotCompressionOptions();
            SetToolTips();

            string strSheetLanguage = cboSheetLanguage.SelectedValue?.ToString();
            if (strSheetLanguage != _strSelectedLanguage
               && cboSheetLanguage.Items.Cast<ListItem>().Any(x => x.Value.ToString() == _strSelectedLanguage))
            {
                cboSheetLanguage.SelectedValue = _strSelectedLanguage;
            }

            PopulatePdfParameters();
            PopulateCustomDataDirectoryListBox();
            PopulateApplicationInsightsOptions();
            PopulateColorModes();
            PopulateDpiScalingMethods();
        }

        private void RefreshGlobalSourcebookInfosListView()
        {
            // Load the Sourcebook information.
            // Put the Sourcebooks into a List so they can first be sorted.
            List<ListItem> lstSourcebookInfos = new List<ListItem>();
            foreach (XPathNavigator objXmlBook in XmlManager.LoadXPath("books.xml", null, _strSelectedLanguage).SelectAndCacheExpression("/chummer/books/book"))
            {
                string strCode = objXmlBook.SelectSingleNodeAndCacheExpression("code")?.Value;
                if (!string.IsNullOrEmpty(strCode))
                {
                    ListItem objBookInfo = new ListItem(strCode, objXmlBook.SelectSingleNodeAndCacheExpression("translate")?.Value ?? objXmlBook.SelectSingleNodeAndCacheExpression("name")?.Value ?? strCode);
                    lstSourcebookInfos.Add(objBookInfo);
                }
            }

            lstSourcebookInfos.Sort(CompareListItems.CompareNames);
            bool blnOldSkipRefresh = _blnSkipRefresh;
            _blnSkipRefresh = true;
            lstGlobalSourcebookInfos.BeginUpdate();
            string strOldSelected = lstGlobalSourcebookInfos.SelectedValue?.ToString();
            lstGlobalSourcebookInfos.PopulateWithListItems(lstSourcebookInfos);
            _blnSkipRefresh = blnOldSkipRefresh;
            if (string.IsNullOrEmpty(strOldSelected))
                lstGlobalSourcebookInfos.SelectedIndex = -1;
            else
                lstGlobalSourcebookInfos.SelectedValue = strOldSelected;
            lstGlobalSourcebookInfos.EndUpdate();
        }

        private void PopulateCustomDataDirectoryListBox()
        {
            bool blnOldSkipRefresh = _blnSkipRefresh;
            _blnSkipRefresh = true;
            ListItem objOldSelected = lsbCustomDataDirectories.SelectedIndex != -1 ? (ListItem)lsbCustomDataDirectories.SelectedItem : ListItem.Blank;
            lsbCustomDataDirectories.BeginUpdate();
            if (_setCustomDataDirectoryInfos.Count != lsbCustomDataDirectories.Items.Count)
            {
                lsbCustomDataDirectories.Items.Clear();

                foreach (CustomDataDirectoryInfo objCustomDataDirectory in _setCustomDataDirectoryInfos)
                {
                    ListItem objItem = new ListItem(objCustomDataDirectory, objCustomDataDirectory.Name);
                    lsbCustomDataDirectories.Items.Add(objItem);
                }
            }
            else
            {
                HashSet<CustomDataDirectoryInfo> setListedInfos = new HashSet<CustomDataDirectoryInfo>();
                for (int iI = lsbCustomDataDirectories.Items.Count - 1; iI >= 0; --iI)
                {
                    ListItem objExistingItem = (ListItem)lsbCustomDataDirectories.Items[iI];
                    CustomDataDirectoryInfo objExistingInfo = (CustomDataDirectoryInfo)objExistingItem.Value;
                    if (!_setCustomDataDirectoryInfos.Contains(objExistingInfo))
                        lsbCustomDataDirectories.Items.RemoveAt(iI);
                    else
                        setListedInfos.Add(objExistingInfo);
                }
                foreach (CustomDataDirectoryInfo objCustomDataDirectory in _setCustomDataDirectoryInfos.Where(x => !setListedInfos.Contains(x)))
                {
                    ListItem objItem = new ListItem(objCustomDataDirectory, objCustomDataDirectory.Name);
                    lsbCustomDataDirectories.Items.Add(objItem);
                }
            }
            if (_blnLoading)
            {
                lsbCustomDataDirectories.DisplayMember = nameof(ListItem.Name);
                lsbCustomDataDirectories.ValueMember = nameof(ListItem.Value);
            }
            lsbCustomDataDirectories.EndUpdate();
            _blnSkipRefresh = blnOldSkipRefresh;
            lsbCustomDataDirectories.SelectedItem = objOldSelected;
        }

        /// <summary>
        /// Set the values for all of the controls based on the Options for the selected Setting.
        /// </summary>
        private void PopulateOptions()
        {
            RefreshGlobalSourcebookInfosListView();
            PopulateCustomDataDirectoryListBox();

            chkAutomaticUpdate.Checked = GlobalSettings.AutomaticUpdate;
            chkPreferNightlyBuilds.Enabled = GlobalSettings.AutomaticUpdate;
            chkPreferNightlyBuilds.Checked = chkPreferNightlyBuilds.Enabled && GlobalSettings.PreferNightlyBuilds;
            chkLiveCustomData.Checked = GlobalSettings.LiveCustomData;
            chkLiveUpdateCleanCharacterFiles.Checked = GlobalSettings.LiveUpdateCleanCharacterFiles;
            chkUseLogging.Checked = GlobalSettings.UseLogging;
            cboUseLoggingApplicationInsights.Enabled = chkUseLogging.Checked;
            PopulateApplicationInsightsOptions();
            PopulateColorModes();
            PopulateDpiScalingMethods();

            chkLifeModule.Checked = GlobalSettings.LifeModuleEnabled;
            chkStartupFullscreen.Checked = GlobalSettings.StartupFullscreen;
            chkSingleDiceRoller.Checked = GlobalSettings.SingleDiceRoller;
            chkDatesIncludeTime.Checked = GlobalSettings.DatesIncludeTime;
            chkPrintToFileFirst.Checked = GlobalSettings.PrintToFileFirst;
            chkPrintExpenses.Checked = GlobalSettings.PrintExpenses;
            chkPrintFreeExpenses.Enabled = GlobalSettings.PrintExpenses;
            chkPrintFreeExpenses.Checked = chkPrintFreeExpenses.Enabled && GlobalSettings.PrintFreeExpenses;
            chkPrintNotes.Checked = GlobalSettings.PrintNotes;
            chkPrintSkillsWithZeroRating.Checked = GlobalSettings.PrintSkillsWithZeroRating;
            nudBrowserVersion.Value = GlobalSettings.EmulatedBrowserVersion;
            txtPDFAppPath.Text = GlobalSettings.PdfAppPath;
            cmdRemovePDFAppPath.Enabled = txtPDFAppPath.TextLength > 0;
            txtCharacterRosterPath.Text = GlobalSettings.CharacterRosterPath;
            cmdRemoveCharacterRoster.Enabled = txtCharacterRosterPath.TextLength > 0;
            chkHideMasterIndex.Checked = GlobalSettings.HideMasterIndex;
            chkHideCharacterRoster.Checked = GlobalSettings.HideCharacterRoster;
            chkCreateBackupOnCareer.Checked = GlobalSettings.CreateBackupOnCareer;
            chkConfirmDelete.Checked = GlobalSettings.ConfirmDelete;
            chkConfirmKarmaExpense.Checked = GlobalSettings.ConfirmKarmaExpense;
            chkHideItemsOverAvail.Checked = GlobalSettings.HideItemsOverAvailLimit;
            chkAllowHoverIncrement.Checked = GlobalSettings.AllowHoverIncrement;
            chkSearchInCategoryOnly.Checked = GlobalSettings.SearchInCategoryOnly;
            chkAllowSkillDiceRolling.Checked = GlobalSettings.AllowSkillDiceRolling;
            chkAllowEasterEggs.Checked = GlobalSettings.AllowEasterEggs;
            chkEnablePlugins.Checked = GlobalSettings.PluginsEnabled;
            chkCustomDateTimeFormats.Checked = GlobalSettings.CustomDateTimeFormats;
            if (!chkCustomDateTimeFormats.Checked)
            {
                txtDateFormat.Text = GlobalSettings.CultureInfo.DateTimeFormat.ShortDatePattern;
                txtTimeFormat.Text = GlobalSettings.CultureInfo.DateTimeFormat.ShortTimePattern;
            }
            else
            {
                txtDateFormat.Text = GlobalSettings.CustomDateFormat;
                txtTimeFormat.Text = GlobalSettings.CustomTimeFormat;
            }
            PluginsShowOrHide(chkEnablePlugins.Checked);
        }

        private void SaveGlobalOptions()
        {
            GlobalSettings.AutomaticUpdate = chkAutomaticUpdate.Checked;
            GlobalSettings.LiveCustomData = chkLiveCustomData.Checked;
            GlobalSettings.LiveUpdateCleanCharacterFiles = chkLiveUpdateCleanCharacterFiles.Checked;
            GlobalSettings.UseLogging = chkUseLogging.Checked;
            if (Enum.TryParse(cboUseLoggingApplicationInsights.SelectedValue.ToString(), out UseAILogging useAI))
                GlobalSettings.UseLoggingApplicationInsightsPreference = useAI;

            if (string.IsNullOrEmpty(_strSelectedLanguage))
            {
                // We have this set differently because changing the selected language also changes the selected default character sheet
                _strSelectedLanguage = GlobalSettings.DefaultLanguage;
                try
                {
                    _objSelectedCultureInfo = CultureInfo.GetCultureInfo(_strSelectedLanguage);
                }
                catch (CultureNotFoundException)
                {
                    _objSelectedCultureInfo = GlobalSettings.SystemCultureInfo;
                }
            }
            GlobalSettings.Language = _strSelectedLanguage;
            GlobalSettings.ColorModeSetting = _eSelectedColorModeSetting;
            GlobalSettings.DpiScalingMethodSetting = cboDpiScalingMethod.SelectedIndex >= 0
                ? (DpiScalingMethod)Enum.Parse(typeof(DpiScalingMethod), cboDpiScalingMethod.SelectedValue.ToString())
                : GlobalSettings.DefaultDpiScalingMethod;
            GlobalSettings.StartupFullscreen = chkStartupFullscreen.Checked;
            GlobalSettings.SingleDiceRoller = chkSingleDiceRoller.Checked;
            GlobalSettings.DefaultCharacterSheet = cboXSLT.SelectedValue?.ToString() ?? GlobalSettings.DefaultCharacterSheetDefaultValue;
            GlobalSettings.DatesIncludeTime = chkDatesIncludeTime.Checked;
            GlobalSettings.PrintToFileFirst = chkPrintToFileFirst.Checked;
            GlobalSettings.PrintExpenses = chkPrintExpenses.Checked;
            GlobalSettings.PrintFreeExpenses = chkPrintFreeExpenses.Checked;
            GlobalSettings.PrintNotes = chkPrintNotes.Checked;
            GlobalSettings.PrintSkillsWithZeroRating = chkPrintSkillsWithZeroRating.Checked;
            GlobalSettings.EmulatedBrowserVersion = decimal.ToInt32(nudBrowserVersion.Value);
            GlobalSettings.PdfAppPath = txtPDFAppPath.Text;
            GlobalSettings.PdfParameters = cboPDFParameters.SelectedValue?.ToString() ?? string.Empty;
            GlobalSettings.LifeModuleEnabled = chkLifeModule.Checked;
            GlobalSettings.PreferNightlyBuilds = chkPreferNightlyBuilds.Checked;
            GlobalSettings.CharacterRosterPath = txtCharacterRosterPath.Text;
            GlobalSettings.HideMasterIndex = chkHideMasterIndex.Checked;
            GlobalSettings.HideCharacterRoster = chkHideCharacterRoster.Checked;
            GlobalSettings.CreateBackupOnCareer = chkCreateBackupOnCareer.Checked;
            GlobalSettings.ConfirmDelete = chkConfirmDelete.Checked;
            GlobalSettings.ConfirmKarmaExpense = chkConfirmKarmaExpense.Checked;
            GlobalSettings.HideItemsOverAvailLimit = chkHideItemsOverAvail.Checked;
            GlobalSettings.AllowHoverIncrement = chkAllowHoverIncrement.Checked;
            GlobalSettings.SearchInCategoryOnly = chkSearchInCategoryOnly.Checked;
            GlobalSettings.AllowSkillDiceRolling = chkAllowSkillDiceRolling.Checked;
            GlobalSettings.DefaultCharacterSetting = cboDefaultCharacterSetting.SelectedValue?.ToString()
                                                   ?? GlobalSettings.DefaultCharacterSettingDefaultValue;
            GlobalSettings.DefaultMasterIndexSetting = cboDefaultMasterIndexSetting.SelectedValue?.ToString()
                                                      ?? GlobalSettings.DefaultMasterIndexSettingDefaultValue;
            GlobalSettings.AllowEasterEggs = chkAllowEasterEggs.Checked;
            GlobalSettings.PluginsEnabled = chkEnablePlugins.Checked;
            switch (cboMugshotCompression.SelectedValue)
            {
                case "jpeg_automatic":
                    GlobalSettings.SavedImageQuality = -1;
                    break;
                case "jpeg_manual":
                    GlobalSettings.SavedImageQuality = nudMugshotCompressionQuality.ValueAsInt;
                    break;
                default:
                    GlobalSettings.SavedImageQuality = int.MaxValue;
                    break;
            }
            GlobalSettings.CustomDateTimeFormats = chkCustomDateTimeFormats.Checked;
            if (GlobalSettings.CustomDateTimeFormats)
            {
                GlobalSettings.CustomDateFormat = txtDateFormat.Text;
                GlobalSettings.CustomTimeFormat = txtTimeFormat.Text;
            }

            GlobalSettings.CustomDataDirectoryInfos.Clear();
            foreach (CustomDataDirectoryInfo objInfo in _setCustomDataDirectoryInfos)
                GlobalSettings.CustomDataDirectoryInfos.Add(objInfo);
            XmlManager.RebuildDataDirectoryInfo(GlobalSettings.CustomDataDirectoryInfos);
            GlobalSettings.SourcebookInfos.Clear();
            foreach (SourcebookInfo objInfo in _dicSourcebookInfos.Values)
                GlobalSettings.SourcebookInfos.Add(objInfo.Code, objInfo);
        }

        /// <summary>
        /// Save the global settings to the registry.
        /// </summary>
        private void SaveRegistrySettings()
        {
            SaveGlobalOptions();

            GlobalSettings.SaveOptionsToRegistry();
        }

        private void PopulateDefaultCharacterSettingLists()
        {
            List<ListItem> lstCharacterSettings = new List<ListItem>(SettingsManager.LoadedCharacterSettings.Count);
            foreach (KeyValuePair<string, CharacterSettings> kvpLoopCharacterOptions in SettingsManager.LoadedCharacterSettings)
            {
                string strId = kvpLoopCharacterOptions.Key;
                if (!string.IsNullOrEmpty(strId))
                {
                    string strName = kvpLoopCharacterOptions.Value.Name;
                    if (strName.IsGuid() || (strName.StartsWith('{') && strName.EndsWith('}')))
                        strName = LanguageManager.GetString(strName.TrimStartOnce('{').TrimEndOnce('}'), _strSelectedLanguage);
                    lstCharacterSettings.Add(new ListItem(strId, strName));
                }
            }
            lstCharacterSettings.Sort(CompareListItems.CompareNames);

            string strOldSelectedDefaultCharacterSetting = cboDefaultCharacterSetting.SelectedValue?.ToString() ?? GlobalSettings.DefaultCharacterSetting;

            cboDefaultCharacterSetting.BeginUpdate();
            cboDefaultCharacterSetting.PopulateWithListItems(lstCharacterSettings);
            if (!string.IsNullOrEmpty(strOldSelectedDefaultCharacterSetting))
            {
                cboDefaultCharacterSetting.SelectedValue = strOldSelectedDefaultCharacterSetting;
                if (cboDefaultCharacterSetting.SelectedIndex == -1 && lstCharacterSettings.Count > 0)
                    cboDefaultCharacterSetting.SelectedIndex = 0;
            }
            cboDefaultCharacterSetting.EndUpdate();

            string strOldSelectedDefaultMasterIndexSetting = cboDefaultMasterIndexSetting.SelectedValue?.ToString() ?? GlobalSettings.DefaultMasterIndexSetting;

            cboDefaultMasterIndexSetting.BeginUpdate();
            cboDefaultMasterIndexSetting.PopulateWithListItems(lstCharacterSettings);
            if (!string.IsNullOrEmpty(strOldSelectedDefaultMasterIndexSetting))
            {
                cboDefaultMasterIndexSetting.SelectedValue = strOldSelectedDefaultMasterIndexSetting;
                if (cboDefaultMasterIndexSetting.SelectedIndex == -1 && lstCharacterSettings.Count > 0)
                    cboDefaultMasterIndexSetting.SelectedIndex = 0;
            }
            cboDefaultMasterIndexSetting.EndUpdate();
        }

        private void PopulateMugshotCompressionOptions()
        {
            List<ListItem> lstMugshotCompressionOptions = new List<ListItem>(2)
            {
                new ListItem("png", LanguageManager.GetString("String_Lossless_Compression_Option")),
                new ListItem("jpeg_automatic", LanguageManager.GetString("String_Lossy_Automatic_Compression_Option")),
                new ListItem("jpeg_manual", LanguageManager.GetString("String_Lossy_Manual_Compression_Option"))
            };

            string strOldSelected = cboMugshotCompression.SelectedValue?.ToString();

            if (_blnLoading)
            {
                int intQuality = GlobalSettings.SavedImageQuality;
                if (intQuality == int.MaxValue)
                {
                    strOldSelected = "png";
                    intQuality = 90;
                }
                else if (intQuality < 0)
                {
                    strOldSelected = "jpeg_automatic";
                    intQuality = 90;
                }
                else
                {
                    strOldSelected = "jpeg_manual";
                }

                nudMugshotCompressionQuality.ValueAsInt = intQuality;
            }

            cboMugshotCompression.BeginUpdate();
            cboMugshotCompression.PopulateWithListItems(lstMugshotCompressionOptions);
            if (!string.IsNullOrEmpty(strOldSelected))
            {
                cboMugshotCompression.SelectedValue = strOldSelected;
                if (cboMugshotCompression.SelectedIndex == -1 && lstMugshotCompressionOptions.Count > 0)
                    cboMugshotCompression.SelectedIndex = 0;
            }

            cboMugshotCompression.EndUpdate();
            bool blnShowQualitySelector = Equals(cboMugshotCompression.SelectedValue, "jpeg_manual");
            lblMugshotCompressionQuality.Visible = blnShowQualitySelector;
            nudMugshotCompressionQuality.Visible = blnShowQualitySelector;
        }

        private void PopulatePdfParameters()
        {
            List<ListItem> lstPdfParameters = new List<ListItem>();

            int intIndex = 0;

            foreach (XPathNavigator objXmlNode in XmlManager.LoadXPath("options.xml", null, _strSelectedLanguage).SelectAndCacheExpression("/chummer/pdfarguments/pdfargument"))
            {
                string strValue = objXmlNode.SelectSingleNodeAndCacheExpression("value")?.Value;
                lstPdfParameters.Add(new ListItem(strValue, objXmlNode.SelectSingleNodeAndCacheExpression("translate")?.Value ?? objXmlNode.SelectSingleNodeAndCacheExpression("name")?.Value ?? string.Empty));
                if (!string.IsNullOrWhiteSpace(GlobalSettings.PdfParameters) && GlobalSettings.PdfParameters == strValue)
                {
                    intIndex = lstPdfParameters.Count - 1;
                }
            }

            string strOldSelected = cboPDFParameters.SelectedValue?.ToString();

            cboPDFParameters.BeginUpdate();
            cboPDFParameters.PopulateWithListItems(lstPdfParameters);
            cboPDFParameters.SelectedIndex = intIndex;
            if (!string.IsNullOrEmpty(strOldSelected))
            {
                cboPDFParameters.SelectedValue = strOldSelected;
                if (cboPDFParameters.SelectedIndex == -1 && lstPdfParameters.Count > 0)
                    cboPDFParameters.SelectedIndex = 0;
            }

            cboPDFParameters.EndUpdate();
        }

        private void PopulateApplicationInsightsOptions()
        {
            string strOldSelected = cboUseLoggingApplicationInsights.SelectedValue?.ToString() ?? GlobalSettings.UseLoggingApplicationInsights.ToString();

            List<ListItem> lstUseAIOptions = new List<ListItem>(6);
            foreach (UseAILogging eOption in Enum.GetValues(typeof(UseAILogging)))
            {
                //we don't want to allow the user to set the logging options in stable builds to higher than "not set".
                if (Assembly.GetAssembly(typeof(Program)).GetName().Version.Build == 0 && !Debugger.IsAttached && eOption > UseAILogging.NotSet)
                    continue;
                lstUseAIOptions.Add(new ListItem(eOption, LanguageManager.GetString("String_ApplicationInsights_" + eOption, _strSelectedLanguage)));
            }

            cboUseLoggingApplicationInsights.BeginUpdate();
            cboUseLoggingApplicationInsights.PopulateWithListItems(lstUseAIOptions);
            if (!string.IsNullOrEmpty(strOldSelected))
                cboUseLoggingApplicationInsights.SelectedValue = Enum.Parse(typeof(UseAILogging), strOldSelected);
            if (cboUseLoggingApplicationInsights.SelectedIndex == -1 && lstUseAIOptions.Count > 0)
                cboUseLoggingApplicationInsights.SelectedIndex = 0;
            cboUseLoggingApplicationInsights.EndUpdate();
        }

        private void PopulateColorModes()
        {
            string strOldSelected = cboColorMode.SelectedValue?.ToString() ?? GlobalSettings.ColorModeSetting.ToString();

            List<ListItem> lstColorModes = new List<ListItem>(3);
            foreach (ColorMode eLoopColorMode in Enum.GetValues(typeof(ColorMode)))
            {
                lstColorModes.Add(new ListItem(eLoopColorMode, LanguageManager.GetString("String_" + eLoopColorMode, _strSelectedLanguage)));
            }

            cboColorMode.BeginUpdate();
            cboColorMode.PopulateWithListItems(lstColorModes);
            if (!string.IsNullOrEmpty(strOldSelected))
                cboColorMode.SelectedValue = Enum.Parse(typeof(ColorMode), strOldSelected);
            if (cboColorMode.SelectedIndex == -1 && lstColorModes.Count > 0)
                cboColorMode.SelectedIndex = 0;
            cboColorMode.EndUpdate();
        }

        private void PopulateDpiScalingMethods()
        {
            string strOldSelected = cboDpiScalingMethod.SelectedValue?.ToString() ?? GlobalSettings.DpiScalingMethodSetting.ToString();

            List<ListItem> lstDpiScalingMethods = new List<ListItem>(3);
            foreach (DpiScalingMethod eLoopDpiScalingMethod in Enum.GetValues(typeof(DpiScalingMethod)))
            {
                switch (eLoopDpiScalingMethod)
                {
                    case DpiScalingMethod.Rescale:
                        if (Environment.OSVersion.Version < new Version(6, 3, 0)) // Need at least Windows 8.1 to get PerMonitor/PerMonitorV2 Scaling
                            continue;
                        break;

                    case DpiScalingMethod.SmartZoom:
                        if (Environment.OSVersion.Version < new Version(10, 0, 17763)) // Need at least Windows 10 Version 1809 to get GDI+ Scaling
                            continue;
                        break;
                }
                lstDpiScalingMethods.Add(new ListItem(eLoopDpiScalingMethod, LanguageManager.GetString("String_" + eLoopDpiScalingMethod, _strSelectedLanguage)));
            }

            cboDpiScalingMethod.BeginUpdate();
            cboDpiScalingMethod.PopulateWithListItems(lstDpiScalingMethods);
            if (!string.IsNullOrEmpty(strOldSelected))
                cboDpiScalingMethod.SelectedValue = Enum.Parse(typeof(DpiScalingMethod), strOldSelected);
            if (cboDpiScalingMethod.SelectedIndex == -1 && lstDpiScalingMethods.Count > 0)
                cboDpiScalingMethod.SelectedIndex = 0;
            cboDpiScalingMethod.EndUpdate();
        }

        private void SetToolTips()
        {
            cboUseLoggingApplicationInsights.SetToolTip(string.Format(_objSelectedCultureInfo, LanguageManager.GetString("Tip_Options_TelemetryId", _strSelectedLanguage),
                Properties.Settings.Default.UploadClientId.ToString("D", GlobalSettings.InvariantCultureInfo)).WordWrap());
        }

        private void PopulateLanguageList()
        {
            string languageDirectoryPath = Path.Combine(Utils.GetStartupPath, "lang");
            string[] languageFilePaths = Directory.GetFiles(languageDirectoryPath, "*.xml");
            List<ListItem> lstLanguages = new List<ListItem>(languageFilePaths.Length);
            foreach (string filePath in languageFilePaths)
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

                XPathNavigator node = xmlDocument.CreateNavigator().SelectSingleNode("/chummer/name");
                if (node == null)
                    continue;

                lstLanguages.Add(new ListItem(Path.GetFileNameWithoutExtension(filePath), node.Value));
            }

            lstLanguages.Sort(CompareListItems.CompareNames);

            cboLanguage.BeginUpdate();
            cboLanguage.PopulateWithListItems(lstLanguages);
            cboLanguage.EndUpdate();
        }

        private void PopulateSheetLanguageList()
        {
            cboSheetLanguage.BeginUpdate();
            cboSheetLanguage.PopulateWithListItems(GetSheetLanguageList());
            cboSheetLanguage.EndUpdate();
        }

        public static List<ListItem> GetSheetLanguageList()
        {
            HashSet<string> setLanguagesWithSheets = new HashSet<string>();

            // Populate the XSL list with all of the manifested XSL files found in the sheets\[language] directory.
            foreach (XPathNavigator xmlSheetLanguage in XmlManager.LoadXPath("sheets.xml").SelectAndCacheExpression("/chummer/sheets/@lang"))
            {
                setLanguagesWithSheets.Add(xmlSheetLanguage.Value);
            }

            string languageDirectoryPath = Path.Combine(Utils.GetStartupPath, "lang");
            string[] languageFilePaths = Directory.GetFiles(languageDirectoryPath, "*.xml");
            List<ListItem> lstSheetLanguages = new List<ListItem>(languageFilePaths.Length);
            foreach (string filePath in languageFilePaths)
            {
                string strLanguageName = Path.GetFileNameWithoutExtension(filePath);
                if (!setLanguagesWithSheets.Contains(strLanguageName))
                    continue;

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

                lstSheetLanguages.Add(new ListItem(strLanguageName, node.Value));
            }

            lstSheetLanguages.Sort(CompareListItems.CompareNames);

            return lstSheetLanguages;
        }

        private static List<ListItem> GetXslFilesFromLocalDirectory(string strLanguage)
        {
            List<ListItem> lstSheets = new List<ListItem>();

            // Populate the XSL list with all of the manifested XSL files found in the sheets\[language] directory.
            foreach (XPathNavigator xmlSheet in XmlManager.LoadXPath("sheets.xml", null, strLanguage).SelectAndCacheExpression("/chummer/sheets[@lang='" + strLanguage + "']/sheet[not(hide)]"))
            {
                string strFile = xmlSheet.SelectSingleNodeAndCacheExpression("filename")?.Value ?? string.Empty;
                lstSheets.Add(new ListItem(
                    !strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase)
                        ? Path.Combine(strLanguage, strFile)
                        : strFile, xmlSheet.SelectSingleNodeAndCacheExpression("name")?.Value ?? string.Empty));
            }

            return lstSheets;
        }

        private void PopulateXsltList()
        {
            string strSelectedSheetLanguage = cboSheetLanguage.SelectedValue?.ToString();
            imgSheetLanguageFlag.Image = Math.Min(imgSheetLanguageFlag.Width, imgSheetLanguageFlag.Height) >= 32
                ? FlagImageGetter.GetFlagFromCountryCode192Dpi(strSelectedSheetLanguage?.Substring(3, 2))
                : FlagImageGetter.GetFlagFromCountryCode(strSelectedSheetLanguage?.Substring(3, 2));

            List<ListItem> lstFiles = GetXslFilesFromLocalDirectory(strSelectedSheetLanguage);

            string strOldSelected = cboXSLT.SelectedValue?.ToString() ?? string.Empty;
            // Strip away the language prefix
            int intPos = strOldSelected.LastIndexOf(Path.DirectorySeparatorChar);
            if (intPos != -1)
                strOldSelected = strOldSelected.Substring(intPos + 1);

            cboXSLT.BeginUpdate();
            cboXSLT.PopulateWithListItems(lstFiles);
            if (!string.IsNullOrEmpty(strOldSelected))
            {
                cboXSLT.SelectedValue =
                    !string.IsNullOrEmpty(strSelectedSheetLanguage) &&
                    !strSelectedSheetLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase)
                        ? Path.Combine(strSelectedSheetLanguage, strOldSelected)
                        : strOldSelected;
                // If the desired sheet was not found, fall back to the Shadowrun 5 sheet.
                if (cboXSLT.SelectedIndex == -1 && lstFiles.Count > 0)
                {
                    cboXSLT.SelectedValue =
                        !string.IsNullOrEmpty(strSelectedSheetLanguage) &&
                        !strSelectedSheetLanguage.Equals(GlobalSettings.DefaultLanguage,
                            StringComparison.OrdinalIgnoreCase)
                            ? Path.Combine(strSelectedSheetLanguage, GlobalSettings.DefaultCharacterSheetDefaultValue)
                            : GlobalSettings.DefaultCharacterSheetDefaultValue;
                    if (cboXSLT.SelectedIndex == -1)
                    {
                        cboXSLT.SelectedIndex = 0;
                    }
                }
            }

            cboXSLT.EndUpdate();
        }

        private void SetDefaultValueForLanguageList()
        {
            cboLanguage.SelectedValue = GlobalSettings.Language;

            if (cboLanguage.SelectedIndex == -1)
                cboLanguage.SelectedValue = GlobalSettings.DefaultLanguage;
        }

        private void SetDefaultValueForSheetLanguageList()
        {
            string strDefaultCharacterSheet = GlobalSettings.DefaultCharacterSheet;
            if (string.IsNullOrEmpty(strDefaultCharacterSheet) || strDefaultCharacterSheet == "Shadowrun (Rating greater 0)")
                strDefaultCharacterSheet = GlobalSettings.DefaultCharacterSheetDefaultValue;

            string strDefaultSheetLanguage = GlobalSettings.Language;
            int intLastIndexDirectorySeparator = strDefaultCharacterSheet.LastIndexOf(Path.DirectorySeparatorChar);
            if (intLastIndexDirectorySeparator != -1)
            {
                string strSheetLanguage = strDefaultCharacterSheet.Substring(0, intLastIndexDirectorySeparator);
                if (strSheetLanguage.Length == 5)
                    strDefaultSheetLanguage = strSheetLanguage;
            }

            cboSheetLanguage.SelectedValue = strDefaultSheetLanguage;

            if (cboSheetLanguage.SelectedIndex == -1)
                cboSheetLanguage.SelectedValue = GlobalSettings.DefaultLanguage;
        }

        private void SetDefaultValueForXsltList()
        {
            if (string.IsNullOrEmpty(GlobalSettings.DefaultCharacterSheet))
                GlobalSettings.DefaultCharacterSheet = GlobalSettings.DefaultCharacterSheetDefaultValue;

            cboXSLT.SelectedValue = GlobalSettings.DefaultCharacterSheet;
            if (cboXSLT.SelectedValue == null && cboXSLT.Items.Count > 0)
            {
                int intNameIndex;
                string strLanguage = _strSelectedLanguage;
                if (string.IsNullOrEmpty(strLanguage) || strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    intNameIndex = cboXSLT.FindStringExact(GlobalSettings.DefaultCharacterSheet);
                else
                    intNameIndex = cboXSLT.FindStringExact(GlobalSettings.DefaultCharacterSheet.Substring(GlobalSettings.DefaultLanguage.LastIndexOf(Path.DirectorySeparatorChar) + 1));
                cboXSLT.SelectedIndex = Math.Max(0, intNameIndex);
            }
        }

        private void UpdateSourcebookInfoPath(string strPath)
        {
            string strTag = lstGlobalSourcebookInfos.SelectedValue?.ToString() ?? string.Empty;
            SourcebookInfo objFoundSource = _dicSourcebookInfos.ContainsKey(strTag) ? _dicSourcebookInfos[strTag] : null;

            if (objFoundSource != null)
            {
                objFoundSource.Path = strPath;
            }
            else
            {
                // If the Sourcebook was not found in the options, add it.
                _dicSourcebookInfos.Add(strTag, new SourcebookInfo
                {
                    Code = strTag,
                    Path = strPath
                });
            }
        }

        private void OptionsChanged(object sender, EventArgs e)
        {
            if (!_blnLoading)
            {
                _blnDirty = true;
            }
        }

        private void PluginsShowOrHide(bool show)
        {
            if (show)
            {
                if (!tabOptions.TabPages.Contains(tabPlugins))
                    tabOptions.TabPages.Add(tabPlugins);
            }
            else
            {
                if (tabOptions.TabPages.Contains(tabPlugins))
                    tabOptions.TabPages.Remove(tabPlugins);
            }
        }

        #endregion Methods

        private async void bScanForPDFs_Click(object sender, EventArgs e)
        {
            // Prompt the user to select a save file to associate with this Contact.
            using (new CursorWait(this))
            {
                Task<XPathNavigator> tskLoadBooks = XmlManager.LoadXPathAsync("books.xml", strLanguage: _strSelectedLanguage);
                using (FolderBrowserDialog fbd = new FolderBrowserDialog {ShowNewFolderButton = false})
                {
                    DialogResult result = fbd.ShowDialog();

                    if (result != DialogResult.OK || string.IsNullOrWhiteSpace(fbd.SelectedPath))
                        return;
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    string[] files = Directory.GetFiles(fbd.SelectedPath, "*.pdf", SearchOption.TopDirectoryOnly);
                    XPathNavigator books = await tskLoadBooks;
                    XPathNodeIterator matches = books.Select("/chummer/books/book/matches/match[language=\"" + _strSelectedLanguage + "\"]");
                    using (frmLoading frmProgressBar = frmChummerMain.CreateAndShowProgressBar(fbd.SelectedPath, files.Length))
                    {
                        List<SourcebookInfo> list = null;
                        await Task.Run(() => list = ScanFilesForPDFTexts(files, matches, frmProgressBar).ToList());
                        sw.Stop();
                        StringBuilder sbdFeedback = new StringBuilder(Environment.NewLine + Environment.NewLine)
                                                    .AppendLine(
                                                        "-------------------------------------------------------------")
                                                    .AppendFormat(GlobalSettings.InvariantCultureInfo,
                                                                  "Scan for PDFs in Folder {0} completed in {1}ms.{2}{3} sourcebook(s) was/were found:",
                                                                  fbd.SelectedPath, sw.ElapsedMilliseconds,
                                                                  Environment.NewLine, list.Count).AppendLine()
                                                    .AppendLine();
                        foreach(SourcebookInfo sourcebook in list)
                        {
                            sbdFeedback.AppendFormat(GlobalSettings.InvariantCultureInfo,
                                                     "{0} with Offset {1} path: {2}", sourcebook.Code,
                                                     sourcebook.Offset, sourcebook.Path).AppendLine();
                        }

                        sbdFeedback.AppendLine()
                                   .AppendLine("-------------------------------------------------------------");
                        Log.Info(sbdFeedback.ToString());

                        string message = string.Format(_objSelectedCultureInfo, LanguageManager.GetString("Message_FoundPDFsInFolder", _strSelectedLanguage), list.Count, fbd.SelectedPath);
                        string title = LanguageManager.GetString("MessageTitle_FoundPDFsInFolder", _strSelectedLanguage);
                        Program.MainForm.ShowMessageBox(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private ICollection<SourcebookInfo> ScanFilesForPDFTexts(IEnumerable<string> files, XPathNodeIterator matches, frmLoading frmProgressBar)
        {
            ParallelOptions parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = 10
            };
            // LockingDictionary makes sure we don't pick out multiple files for the same sourcebook
            using (LockingDictionary<string, SourcebookInfo> resultCollection
                = new LockingDictionary<string, SourcebookInfo>())
            {
                Parallel.ForEach(files, parallelOptions, file =>
                {
                    FileInfo fileInfo = new FileInfo(file);
                    frmProgressBar.PerformStep(fileInfo.Name, frmLoading.ProgressBarTextPatterns.Scanning);
                    SourcebookInfo info = ScanPDFForMatchingText(fileInfo, matches);
                    if (info == null || resultCollection.ContainsKey(info.Code))
                        return;
                    resultCollection.TryAdd(info.Code, info);
                });
                foreach (KeyValuePair<string, SourcebookInfo> kvpInfo in resultCollection)
                    _dicSourcebookInfos[kvpInfo.Key] = kvpInfo.Value;

                return resultCollection.Values;
            }
        }

        private SourcebookInfo ScanPDFForMatchingText(FileSystemInfo fileInfo, XPathNodeIterator xmlMatches)
        {
            //Search the first 10 pages for all the text
            for (int intPage = 1; intPage <= 10; intPage++)
            {
                string text = GetPageTextFromPDF(fileInfo, intPage);
                if (string.IsNullOrEmpty(text))
                    continue;

                foreach (XPathNavigator xmlMatch in xmlMatches)
                {
                    string strLanguageText = xmlMatch.SelectSingleNodeAndCacheExpression("text")?.Value ?? string.Empty;
                    if (!text.Contains(strLanguageText))
                        continue;
                    int trueOffset = intPage - xmlMatch.SelectSingleNodeAndCacheExpression("page")?.ValueAsInt ?? 0;

                    xmlMatch.MoveToParent();
                    xmlMatch.MoveToParent();

                    return new SourcebookInfo
                    {
                        Code = xmlMatch.SelectSingleNodeAndCacheExpression("code")?.Value,
                        Offset = trueOffset,
                        Path = fileInfo.FullName
                    };
                }
            }
            return null;

            string GetPageTextFromPDF(FileSystemInfo objInnerFileInfo, int intPage)
            {
                PdfReader objPdfReader = null;
                PdfDocument objPdfDocument = null;
                try
                {
                    try
                    {
                        objPdfReader = new PdfReader(objInnerFileInfo.FullName);
                        objPdfDocument = new PdfDocument(objPdfReader);
                    }
                    catch (iText.IO.Exceptions.IOException e)
                    {
                        if (e.Message == "PDF header not found.")
                            return string.Empty;
                        throw;
                    }
                    catch (Exception e)
                    {
                        //Loading failed, probably not a PDF file
                        Log.Warn(
                            e,
                            "Could not load file " + objInnerFileInfo.FullName
                                                   + " and open it as PDF to search for text.");
                        return null;
                    }

                    List<string> lstStringFromPdf = new List<string>(30);
                    // Loop through each page, starting at the listed page + offset.
                    if (intPage >= objPdfDocument.GetNumberOfPages())
                        return null;

                    int intProcessedStrings = lstStringFromPdf.Count;
                    try
                    {
                        // each page should have its own text extraction strategy for it to work properly
                        // this way we don't need to check for previous page appearing in the current page
                        // https://stackoverflow.com/questions/35911062/why-are-gettextfrompage-from-itextsharp-returning-longer-and-longer-strings
                        string strPageText = iText.Kernel.Pdf.Canvas.Parser.PdfTextExtractor.GetTextFromPage(
                                                      objPdfDocument.GetPage(intPage),
                                                      new SimpleTextExtractionStrategy())
                                                  .CleanStylisticLigatures().NormalizeWhiteSpace()
                                                  .NormalizeLineEndings();

                        // don't trust it to be correct, trim all whitespace and remove empty strings before we even start
                        lstStringFromPdf.AddRange(
                            strPageText.SplitNoAlloc(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                                       .Where(s => !string.IsNullOrWhiteSpace(s)).Select(x => x.Trim()));
                    }
                    // Need to catch all sorts of exceptions here just in case weird stuff happens in the scanner
                    catch (Exception e)
                    {
                        Utils.BreakIfDebug();
                        Log.Error(e);
                        return null;
                    }

                    StringBuilder sbdAllLines = new StringBuilder();
                    for (int i = intProcessedStrings; i < lstStringFromPdf.Count; i++)
                    {
                        string strCurrentLine = lstStringFromPdf[i];
                        sbdAllLines.AppendLine(strCurrentLine);
                    }

                    return sbdAllLines.ToString();
                }
                finally
                {
                    objPdfDocument?.Close();
                    objPdfReader?.Close();
                }
            }
        }
    }
}
