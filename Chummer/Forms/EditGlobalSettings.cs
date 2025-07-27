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
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.XPath;
using Chummer.Plugins;
using iText.Kernel.Pdf;
#if DEBUG
using Microsoft.IO;
#endif
using NLog;
using Application = System.Windows.Forms.Application;

namespace Chummer
{
    public partial class EditGlobalSettings : Form
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;

        // List of custom data directories possible to be added to a character
        private readonly HashSet<CustomDataDirectoryInfo> _setCustomDataDirectoryInfos;

        // List of sourcebook infos, needed to make sure we don't directly modify ones in the options unless we save our options
        private ConcurrentDictionary<string, SourcebookInfo> _dicSourcebookInfos;

        private bool _blnDirty;
        private int _intSkipRefresh;
        private int _intLoading = 1;
        private string _strSelectedLanguage = GlobalSettings.Language;
        private CultureInfo _objSelectedCultureInfo = GlobalSettings.CultureInfo;
        private ColorMode _eSelectedColorModeSetting = GlobalSettings.ColorModeSetting;
        private Color _objSelectedHasNotesColor = GlobalSettings.DefaultHasNotesColor;

        private readonly ConcurrentDictionary<string, HashSet<string>> _dicCachedPdfAppNames
            = new ConcurrentDictionary<string, HashSet<string>>();
        private readonly ConcurrentDictionary<string, string> _dicCachedLanguageDocumentNames =
            new ConcurrentDictionary<string, string>();

        #region Form Events

        public EditGlobalSettings(string strActiveTab = "", CancellationToken token = default)
        {
            InitializeComponent();
#if !DEBUG
            // tabPage3 only contains cmdUploadPastebin, which is not used if DEBUG is not enabled
            // Remove this line if cmdUploadPastebin_Click has some functionality if DEBUG is not enabled or if tabPage3 gets some other control that can be used if DEBUG is not enabled
            tabOptions.TabPages.Remove(tabGitHubIssues);
#endif
            tabOptions.MouseWheel += CommonFunctions.ShiftTabsOnMouseScroll;
            this.UpdateLightDarkMode(token: token);
            this.TranslateWinForm(token: token);
            pnlHasNotesColorPreview.BackColor = _objSelectedHasNotesColor;
            _setCustomDataDirectoryInfos
                = new HashSet<CustomDataDirectoryInfo>(GlobalSettings.CustomDataDirectoryInfos);
            Disposed += (sender, args) =>
            {
                Stack<HashSet<string>> stkToReturn = new Stack<HashSet<string>>(_dicCachedPdfAppNames.GetValuesToListSafe());
                _dicCachedPdfAppNames.Clear();
                while (stkToReturn.Count > 0)
                {
                    HashSet<string> setLoop = stkToReturn.Pop();
                    Utils.StringHashSetPool.Return(ref setLoop);
                }
            };
            if (!string.IsNullOrEmpty(strActiveTab))
            {
                int intActiveTabIndex = tabOptions.TabPages.IndexOfKey(strActiveTab);
                if (intActiveTabIndex > 0)
                    tabOptions.SelectedTab = tabOptions.TabPages[intActiveTabIndex];
            }
        }

        private async void EditGlobalSettings_Load(object sender, EventArgs e)
        {
            _dicSourcebookInfos =
                new ConcurrentDictionary<string, SourcebookInfo>(await GlobalSettings.GetSourcebookInfosAsync()
                    .ConfigureAwait(false));
            await PopulateDefaultCharacterSettingLists().ConfigureAwait(false);
            await PopulateMugshotCompressionOptions().ConfigureAwait(false);
            await PopulateChum5lzCompressionLevelOptions().ConfigureAwait(false);
            await SetToolTips().ConfigureAwait(false);
            await PopulateOptions().ConfigureAwait(false);
            await RefreshLanguageDocumentNames().ConfigureAwait(false);
            await PopulateLanguageList().ConfigureAwait(false);
            await SetDefaultValueForLanguageList().ConfigureAwait(false);
            await PopulateSheetLanguageList().ConfigureAwait(false);
            await SetDefaultValueForSheetLanguageList().ConfigureAwait(false);
            await PopulateXsltList().ConfigureAwait(false);
            await SetDefaultValueForXsltList().ConfigureAwait(false);
            await PopulatePdfParameters().ConfigureAwait(false);

            Interlocked.Decrement(ref _intLoading);

            if (_blnPromptPdfReaderOnLoad)
            {
                _blnPromptPdfReaderOnLoad = false;
                await PromptPdfAppPath().ConfigureAwait(false);
            }

            if (!string.IsNullOrEmpty(_strSelectCodeOnRefresh))
            {
                bool blnDoPdfPrompt = await lstGlobalSourcebookInfos.DoThreadSafeFuncAsync(x =>
                {
                    x.SelectedValue = _strSelectCodeOnRefresh;
                    return x.SelectedIndex >= 0;
                }).ConfigureAwait(false);
                if (blnDoPdfPrompt)
                    await PromptPdfLocation().ConfigureAwait(false);
                _strSelectCodeOnRefresh = string.Empty;
            }
        }

        #endregion Form Events

        #region Control Events

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this).ConfigureAwait(false);
            try
            {
                if (_blnDirty)
                {
                    string text = await LanguageManager.GetStringAsync("Message_Options_SaveForms",
                                                                       _strSelectedLanguage).ConfigureAwait(false);
                    string caption
                        = await LanguageManager.GetStringAsync("MessageTitle_Options_CloseForms", _strSelectedLanguage)
                                               .ConfigureAwait(false);

                    if (await Program.ShowScrollableMessageBoxAsync(this, text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question).ConfigureAwait(false)
                        != DialogResult.Yes)
                        return;
                }

                await this.DoThreadSafeAsync(x => x.DialogResult = DialogResult.OK).ConfigureAwait(false);
                await SaveRegistrySettings().ConfigureAwait(false);
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }

            if (_blnDirty)
            {
                await Utils.RestartApplication(_strSelectedLanguage, "Message_Options_CloseForms")
                           .ConfigureAwait(false);
            }

            await this.DoThreadSafeAsync(x => x.Close()).ConfigureAwait(false);
        }

        private async void cboLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                _strSelectedLanguage
                    = await cboLanguage.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false)
                      ?? GlobalSettings.DefaultLanguage;
                try
                {
                    _objSelectedCultureInfo = CultureInfo.GetCultureInfo(_strSelectedLanguage);
                }
                catch (CultureNotFoundException)
                {
                    _objSelectedCultureInfo = GlobalSettings.SystemCultureInfo;
                }

                await imgLanguageFlag.DoThreadSafeAsync(x =>
                    x.Image = FlagImageGetter.GetFlagFromCountryCode(_strSelectedLanguage.Substring(3, 2),
                        Math.Min(x.Width, x.Height))).ConfigureAwait(false);

                bool isEnabled = !string.IsNullOrEmpty(_strSelectedLanguage)
                                 && !_strSelectedLanguage.Equals(GlobalSettings.DefaultLanguage,
                                                                 StringComparison.OrdinalIgnoreCase);
                await cmdVerify.DoThreadSafeAsync(x => x.Enabled = isEnabled).ConfigureAwait(false);
                await cmdVerifyData.DoThreadSafeAsync(x => x.Enabled = isEnabled).ConfigureAwait(false);

                int intLoading = Interlocked.Increment(ref _intLoading);
                try
                {
                    if (intLoading == 1)
                    {
                        CursorWait objCursorWait = await CursorWait.NewAsync(this).ConfigureAwait(false);
                        try
                        {
                            await TranslateForm().ConfigureAwait(false);
                        }
                        finally
                        {
                            await objCursorWait.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                }
                finally
                {
                    Interlocked.Decrement(ref _intLoading);
                }

                OptionsChanged(sender, e);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Log.Error(ex, "How the hell? Give me the callstack! " + ex);
            }
        }

        private async void cboSheetLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this).ConfigureAwait(false);
            try
            {
                await PopulateXsltList().ConfigureAwait(false);
                OptionsChanged(sender, e);
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async void cmdVerify_Click(object sender, EventArgs e)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this).ConfigureAwait(false);
            try
            {
                await LanguageManager.VerifyStrings(_strSelectedLanguage).ConfigureAwait(false);
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async void cmdVerifyData_Click(object sender, EventArgs e)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this).ConfigureAwait(false);
            try
            {
                string strSelectedLanguage = _strSelectedLanguage;
                // Build a list of Sourcebooks that will be passed to the Verify method.
                // This is done since not all of the books are available in every language or the user may only wish to verify the content of certain books.
                using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                out HashSet<string> setBooks))
                {
                    foreach (ListItem objItem in await lstGlobalSourcebookInfos.DoThreadSafeFuncAsync(x => x.Items)
                                                                               .ConfigureAwait(false))
                    {
                        string strItemValue = objItem.Value?.ToString();
                        setBooks.Add(strItemValue);
                    }

                    await XmlManager.Verify(strSelectedLanguage, setBooks).ConfigureAwait(false);
                }

                string strFilePath
                    = Path.Combine(Utils.GetStartupPath, "lang", "results_" + strSelectedLanguage + ".xml");
                await Program.ShowScrollableMessageBoxAsync(
                    this,
                    string.Format(_objSelectedCultureInfo,
                        await LanguageManager.GetStringAsync("Message_Options_ValidationResults",
                            _strSelectedLanguage).ConfigureAwait(false),
                        strFilePath),
                    await LanguageManager.GetStringAsync("MessageTitle_Options_ValidationResults",
                        _strSelectedLanguage).ConfigureAwait(false),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information).ConfigureAwait(false);
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async void cmdPDFAppPath_Click(object sender, EventArgs e)
        {
            await PromptPdfAppPath().ConfigureAwait(false);
        }

        private async void cmdPDFLocation_Click(object sender, EventArgs e)
        {
            await PromptPdfLocation().ConfigureAwait(false);
        }

        private void lstGlobalSourcebookInfos_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strSelectedCode = lstGlobalSourcebookInfos.SelectedValue?.ToString() ?? string.Empty;

            // Find the selected item in the Sourcebook List.
            if (_dicSourcebookInfos.TryGetValue(strSelectedCode, out SourcebookInfo objSource) && objSource != null)
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
            if (_intSkipRefresh > 0 || _intLoading > 0)
                return;

            int intOffset = nudPDFOffset.ValueAsInt;
            string strTag = lstGlobalSourcebookInfos.SelectedValue?.ToString() ?? string.Empty;
            if (_dicSourcebookInfos.TryGetValue(strTag, out SourcebookInfo objFoundSource) && objFoundSource != null)
            {
                objFoundSource.Offset = intOffset;
            }
            else
            {
                objFoundSource = new SourcebookInfo
                {
                    Code = strTag,
                    Offset = intOffset
                };
                // If the Sourcebook was not found in the options, add it.
                _dicSourcebookInfos.AddOrUpdate(strTag, objFoundSource, (x, y) =>
                {
                    y.Offset = intOffset;
                    objFoundSource.Dispose();
                    return y;
                });
            }
        }

        private async void cmdPDFTest_Click(object sender, EventArgs e)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this).ConfigureAwait(false);
            try
            {
                await CommonFunctions.OpenPdf(
                    await lstGlobalSourcebookInfos.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false)
                    + " 3", null,
                    await cboPDFParameters.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false)
                    ?? string.Empty,
                    await txtPDFAppPath.DoThreadSafeFuncAsync(x => x.Text).ConfigureAwait(false)).ConfigureAwait(false);
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async void cboUseLoggingApplicationInsights_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_intLoading > 0)
                return;
            UseAILogging useAI = await cboUseLoggingApplicationInsights
                                       .DoThreadSafeFuncAsync(x => (UseAILogging)((ListItem) x.SelectedItem).Value)
                                       .ConfigureAwait(false);
            GlobalSettings.UseLoggingResetCounter = 10;
            if (useAI > UseAILogging.Info
                && GlobalSettings.UseLoggingApplicationInsightsPreference <= UseAILogging.Info
                && await Program.ShowScrollableMessageBoxAsync(this,
                    (await LanguageManager
                        .GetStringAsync(
                            "Message_Options_ConfirmTelemetry",
                            _strSelectedLanguage).ConfigureAwait(false))
                    .WordWrap(),
                    await LanguageManager
                        .GetStringAsync(
                            "MessageTitle_Options_ConfirmTelemetry",
                            _strSelectedLanguage).ConfigureAwait(false),
                    MessageBoxButtons.YesNo).ConfigureAwait(false) != DialogResult.Yes)
            {
                int intLoading = Interlocked.Increment(ref _intLoading);
                try
                {
                    if (intLoading == 1)
                    {
                        CursorWait objCursorWait = await CursorWait.NewAsync(this).ConfigureAwait(false);
                        try
                        {
                            await cboUseLoggingApplicationInsights
                                  .DoThreadSafeAsync(x => x.SelectedItem = UseAILogging.Info).ConfigureAwait(false);
                        }
                        finally
                        {
                            await objCursorWait.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                }
                finally
                {
                    Interlocked.Decrement(ref _intLoading);
                }

                return;
            }

            OptionsChanged(sender, e);
        }

        private async void chkUseLogging_CheckedChanged(object sender, EventArgs e)
        {
            if (_intLoading > 0)
                return;
            if (await chkUseLogging.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false)
                && !GlobalSettings.UseLogging
                && await Program.ShowScrollableMessageBoxAsync(
                    this,
                    (await LanguageManager
                        .GetStringAsync("Message_Options_ConfirmDetailedTelemetry", _strSelectedLanguage)
                        .ConfigureAwait(false)).WordWrap(),
                    await LanguageManager
                        .GetStringAsync("MessageTitle_Options_ConfirmDetailedTelemetry", _strSelectedLanguage)
                        .ConfigureAwait(false), MessageBoxButtons.YesNo).ConfigureAwait(false) != DialogResult.Yes)
            {
                int intLoading = Interlocked.Increment(ref _intLoading);
                try
                {
                    if (intLoading == 1)
                    {
                        CursorWait objCursorWait = await CursorWait.NewAsync(this).ConfigureAwait(false);
                        try
                        {
                            await chkUseLogging.DoThreadSafeAsync(x => x.Checked = false).ConfigureAwait(false);
                        }
                        finally
                        {
                            await objCursorWait.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                }
                finally
                {
                    Interlocked.Decrement(ref _intLoading);
                }

                return;
            }

            bool blnEnabled = await chkUseLogging.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false);
            await cboUseLoggingApplicationInsights.DoThreadSafeAsync(x => x.Enabled = blnEnabled).ConfigureAwait(false);
            OptionsChanged(sender, e);
        }

        private void cboUseLoggingHelp_Click(object sender, EventArgs e)
        {
            //open the telemetry document
            Process.Start(
                "https://docs.google.com/document/d/1LThAg6U5qXzHAfIRrH0Kb7griHrPN0hy7ab8FSJDoFY/edit?usp=sharing");
        }

        private void cmdPluginsHelp_Click(object sender, EventArgs e)
        {
            Process.Start(
                "https://docs.google.com/document/d/1WOPB7XJGgcmxg7REWxF6HdP3kQdtHpv6LJOXZtLggxM/edit?usp=sharing");
        }

        private void chkCustomDateTimeFormats_CheckedChanged(object sender, EventArgs e)
        {
            if (chkCustomDateTimeFormats.Checked)
            {
                grpDateFormat.Enabled = true;
                grpTimeFormat.Enabled = true;
            }
            else
            {
                grpDateFormat.Enabled = false;
                grpTimeFormat.Enabled = false;
                txtDateFormat.Text = GlobalSettings.CultureInfo.DateTimeFormat.ShortDatePattern;
                txtTimeFormat.Text = GlobalSettings.CultureInfo.DateTimeFormat.ShortTimePattern;
            }

            OptionsChanged(sender, e);
        }

        private async void txtDateFormat_TextChanged(object sender, EventArgs e)
        {
            string strText;
            try
            {
                strText = DateTime.Now.ToString(
                    await txtDateFormat.DoThreadSafeFuncAsync(x => x.Text).ConfigureAwait(false),
                    _objSelectedCultureInfo);
            }
            catch
            {
                strText = await LanguageManager.GetStringAsync("String_Error", _strSelectedLanguage)
                                               .ConfigureAwait(false);
            }

            await txtDateFormatView.DoThreadSafeAsync(x => x.Text = strText).ConfigureAwait(false);
            OptionsChanged(sender, e);
        }

        private async void txtTimeFormat_TextChanged(object sender, EventArgs e)
        {
            string strText;
            try
            {
                strText = DateTime.Now.ToString(
                    await txtTimeFormat.DoThreadSafeFuncAsync(x => x.Text).ConfigureAwait(false),
                    _objSelectedCultureInfo);
            }
            catch
            {
                strText = await LanguageManager.GetStringAsync("String_Error", _strSelectedLanguage)
                                               .ConfigureAwait(false);
            }

            await txtTimeFormatView.DoThreadSafeAsync(x => x.Text = strText).ConfigureAwait(false);
            OptionsChanged(sender, e);
        }

        private void cboMugshotCompression_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_intLoading > 0)
                return;
            bool blnShowQualitySelector = Equals(cboMugshotCompression.SelectedValue, "jpeg_manual");
            lblMugshotCompressionQuality.Visible = blnShowQualitySelector;
            nudMugshotCompressionQuality.Visible = blnShowQualitySelector;
            OptionsChanged(sender, e);
        }

        private async void cboColorMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_intLoading > 0)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this).ConfigureAwait(false);
            try
            {
                if (Enum.TryParse(
                        await cboColorMode.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString()).ConfigureAwait(false),
                        true,
                        out ColorMode eNewColorMode) && _eSelectedColorModeSetting != eNewColorMode)
                {
                    _eSelectedColorModeSetting = eNewColorMode;
                    switch (eNewColorMode)
                    {
                        case ColorMode.Automatic:
                            bool blnLightMode = !ColorManager.DoesRegistrySayDarkMode();
                            await this.UpdateLightDarkModeAsync(blnLightMode).ConfigureAwait(false);
                            pnlHasNotesColorPreview.BackColor = blnLightMode ? _objSelectedHasNotesColor : ColorManager.GenerateDarkModeColor(_objSelectedHasNotesColor);
                            break;

                        case ColorMode.Light:
                            await this.UpdateLightDarkModeAsync(true).ConfigureAwait(false);
                            pnlHasNotesColorPreview.BackColor = _objSelectedHasNotesColor;
                            break;

                        case ColorMode.Dark:
                            await this.UpdateLightDarkModeAsync(false).ConfigureAwait(false);
                            pnlHasNotesColorPreview.BackColor = ColorManager.GenerateDarkModeColor(_objSelectedHasNotesColor);
                            break;
                    }
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }

            OptionsChanged(sender, e);
        }

        private async void btnHasNotesColorSelect_Click(object sender, EventArgs e)
        {
            Color objOldColor = _objSelectedHasNotesColor;
            if (_eSelectedColorModeSetting == ColorMode.Dark || (_eSelectedColorModeSetting == ColorMode.Automatic && ColorManager.DoesRegistrySayDarkMode()))
                objOldColor = ColorManager.GenerateDarkModeColor(objOldColor);
            await this.DoThreadSafeAsync(() => dlgColor.Color = objOldColor).ConfigureAwait(false);
            if (await this.DoThreadSafeFuncAsync(x => dlgColor.ShowDialog(x)).ConfigureAwait(false) != DialogResult.OK)
                return;
            Color objNewColor = await this.DoThreadSafeFuncAsync(() => dlgColor.Color).ConfigureAwait(false);
            if (_eSelectedColorModeSetting == ColorMode.Dark || (_eSelectedColorModeSetting == ColorMode.Automatic && ColorManager.DoesRegistrySayDarkMode()))
                objNewColor = ColorManager.GenerateInverseDarkModeColor(objNewColor);
            if (objNewColor != _objSelectedHasNotesColor)
            {
                _objSelectedHasNotesColor = objNewColor;
                if (_eSelectedColorModeSetting == ColorMode.Dark || (_eSelectedColorModeSetting == ColorMode.Automatic && ColorManager.DoesRegistrySayDarkMode()))
                    pnlHasNotesColorPreview.BackColor = ColorManager.GenerateDarkModeColor(_objSelectedHasNotesColor);
                else
                    pnlHasNotesColorPreview.BackColor = _objSelectedHasNotesColor;
                OptionsChanged(sender, e);
            }
        }

        private void chkPrintExpenses_CheckedChanged(object sender, EventArgs e)
        {
            if (_intLoading > 0)
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

        private void cmdRemovePDFLocation_Click(object sender, EventArgs e)
        {
            UpdateSourcebookInfoPath(string.Empty);
            txtPDFLocation.Text = string.Empty;
        }

        private void txtPDFLocation_TextChanged(object sender, EventArgs e)
        {
            if (_intLoading > 0)
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

        private async void chkLifeModules_CheckedChanged(object sender, EventArgs e)
        {
            if (_intLoading > 0 || !await chkLifeModule.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                return;
            if (await Program.ShowScrollableMessageBoxAsync(
                    this,
                    await LanguageManager.GetStringAsync("Tip_LifeModule_Warning", _strSelectedLanguage)
                        .ConfigureAwait(false), Application.ProductName,
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Warning).ConfigureAwait(false) != DialogResult.OK)
                await chkLifeModule.DoThreadSafeAsync(x => x.Checked = false).ConfigureAwait(false);
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

        private async void cmdAddCustomDirectory_Click(object sender, EventArgs e)
        {
            // Prompt the user to select a save file to associate with this Contact.
            string strSelectedPath = string.Empty;
            DialogResult eResult = await this.DoThreadSafeFuncAsync(x =>
            {
                using (FolderBrowserDialog dlgSelectFolder = new FolderBrowserDialog())
                {
                    dlgSelectFolder.SelectedPath = Utils.GetStartupPath;
                    DialogResult eReturn = dlgSelectFolder.ShowDialog(x);
                    strSelectedPath = dlgSelectFolder.SelectedPath;
                    return eReturn;
                }
            }).ConfigureAwait(false);

            if (eResult != DialogResult.OK)
                return;

            string strDescription
                = await LanguageManager.GetStringAsync("String_CustomItem_SelectText", _strSelectedLanguage)
                                       .ConfigureAwait(false);
            using (ThreadSafeForm<SelectText> frmSelectCustomDirectoryName =
                   await ThreadSafeForm<SelectText>.GetAsync(() => new SelectText
                   {
                       Description = strDescription
                   }).ConfigureAwait(false))
            {
                if (await frmSelectCustomDirectoryName.ShowDialogSafeAsync(this).ConfigureAwait(false)
                    != DialogResult.OK)
                    return;
                CustomDataDirectoryInfo objNewCustomDataDirectory
                    = await CustomDataDirectoryInfo.CreateAsync(frmSelectCustomDirectoryName.MyForm.SelectedValue, strSelectedPath).ConfigureAwait(false);
                if (objNewCustomDataDirectory.XmlException != default)
                {
                    await Program.ShowScrollableMessageBoxAsync(this,
                        string.Format(_objSelectedCultureInfo,
                            await LanguageManager
                                .GetStringAsync(
                                    "Message_FailedLoad", _strSelectedLanguage)
                                .ConfigureAwait(false),
                            objNewCustomDataDirectory.XmlException.Message),
                        string.Format(_objSelectedCultureInfo,
                            await LanguageManager
                                .GetStringAsync(
                                    "MessageTitle_FailedLoad", _strSelectedLanguage)
                                .ConfigureAwait(false) +
                            await LanguageManager
                                .GetStringAsync("String_Space", _strSelectedLanguage)
                                .ConfigureAwait(false) + objNewCustomDataDirectory.Name
                            + Path.DirectorySeparatorChar + "manifest.xml"),
                        MessageBoxButtons.OK, MessageBoxIcon.Error).ConfigureAwait(false);
                    return;
                }

                string strDirectoryPath = objNewCustomDataDirectory.DirectoryPath;
                if (_setCustomDataDirectoryInfos.Any(x => x.DirectoryPath == strDirectoryPath))
                {
                    await Program.ShowScrollableMessageBoxAsync(this,
                        string.Format(
                            await LanguageManager.GetStringAsync(
                                "Message_Duplicate_CustomDataDirectoryPath",
                                _strSelectedLanguage).ConfigureAwait(false),
                            objNewCustomDataDirectory.Name),
                        await LanguageManager.GetStringAsync(
                            "MessageTitle_Duplicate_CustomDataDirectoryPath",
                            _strSelectedLanguage).ConfigureAwait(false), MessageBoxButtons.OK,
                        MessageBoxIcon.Error).ConfigureAwait(false);
                    return;
                }

                if (_setCustomDataDirectoryInfos.Contains(objNewCustomDataDirectory))
                {
                    CustomDataDirectoryInfo objExistingInfo
                        = _setCustomDataDirectoryInfos.FirstOrDefault(x => x.Equals(objNewCustomDataDirectory));
                    if (objExistingInfo != null)
                    {
                        if (objNewCustomDataDirectory.HasManifest)
                        {
                            if (objExistingInfo.HasManifest)
                            {
                                await Program.ShowScrollableMessageBoxAsync(
                                    string.Format(
                                        await LanguageManager.GetStringAsync(
                                            "Message_Duplicate_CustomDataDirectory").ConfigureAwait(false),
                                        objExistingInfo.Name, objNewCustomDataDirectory.Name),
                                    await LanguageManager.GetStringAsync(
                                        "MessageTitle_Duplicate_CustomDataDirectory").ConfigureAwait(false),
                                    MessageBoxButtons.OK, MessageBoxIcon.Error).ConfigureAwait(false);
                                return;
                            }

                            _setCustomDataDirectoryInfos.Remove(objExistingInfo);
                            do
                            {
                                objExistingInfo.RandomizeGuid();
                            } while (objExistingInfo.Equals(objNewCustomDataDirectory)
                                     || _setCustomDataDirectoryInfos.Contains(objExistingInfo));

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
                                                         objNewCustomDataDirectory.CharacterSettingsSaveKey.Equals(
                                                             x.CharacterSettingsSaveKey,
                                                             StringComparison.OrdinalIgnoreCase))
                    && await Program.ShowScrollableMessageBoxAsync(this,
                        string.Format(
                            await LanguageManager.GetStringAsync(
                                "Message_Duplicate_CustomDataDirectoryName",
                                _strSelectedLanguage).ConfigureAwait(false),
                            objNewCustomDataDirectory.Name),
                        await LanguageManager.GetStringAsync(
                            "MessageTitle_Duplicate_CustomDataDirectoryName",
                            _strSelectedLanguage).ConfigureAwait(false), MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning).ConfigureAwait(false) != DialogResult.Yes)
                    return;
                _setCustomDataDirectoryInfos.Add(objNewCustomDataDirectory);
                PopulateCustomDataDirectoryListBox();
            }
        }

        private void cmdRemoveCustomDirectory_Click(object sender, EventArgs e)
        {
            if (lsbCustomDataDirectories.SelectedIndex == -1)
                return;
            ListItem objSelected = (ListItem)lsbCustomDataDirectories.SelectedItem;
            CustomDataDirectoryInfo objInfoToRemove = (CustomDataDirectoryInfo) objSelected.Value;
            if (!_setCustomDataDirectoryInfos.Remove(objInfoToRemove))
                return;
            OptionsChanged(sender, e);
            PopulateCustomDataDirectoryListBox();
        }

        private async void cmdRenameCustomDataDirectory_Click(object sender, EventArgs e)
        {
            if (await lsbCustomDataDirectories.DoThreadSafeFuncAsync(x => x.SelectedIndex).ConfigureAwait(false) == -1)
                return;
            ListItem objSelected = await lsbCustomDataDirectories.DoThreadSafeFuncAsync(x => (ListItem) x.SelectedItem)
                                                                 .ConfigureAwait(false);
            CustomDataDirectoryInfo objInfoToRename = (CustomDataDirectoryInfo) objSelected.Value;
            string strDescription
                = await LanguageManager.GetStringAsync("String_CustomItem_SelectText", _strSelectedLanguage)
                                       .ConfigureAwait(false);
            using (ThreadSafeForm<SelectText> frmSelectCustomDirectoryName = await ThreadSafeForm<SelectText>.GetAsync(
                       () => new SelectText
                       {
                           Description = strDescription
                       }).ConfigureAwait(false))
            {
                if (await frmSelectCustomDirectoryName.ShowDialogSafeAsync(this).ConfigureAwait(false)
                    != DialogResult.OK)
                    return;
                CustomDataDirectoryInfo objNewInfo
                    = await CustomDataDirectoryInfo.CreateAsync(frmSelectCustomDirectoryName.MyForm.SelectedValue, objInfoToRename.DirectoryPath).ConfigureAwait(false);
                if (!objNewInfo.HasManifest)
                    objNewInfo.CopyGuid(objInfoToRename);
                if (objNewInfo.XmlException != default)
                {
                    await Program.ShowScrollableMessageBoxAsync(this,
                        string.Format(_objSelectedCultureInfo,
                            await LanguageManager
                                .GetStringAsync(
                                    "Message_FailedLoad", _strSelectedLanguage)
                                .ConfigureAwait(false),
                            objNewInfo.XmlException.Message),
                        string.Format(_objSelectedCultureInfo,
                            await LanguageManager
                                .GetStringAsync(
                                    "MessageTitle_FailedLoad", _strSelectedLanguage)
                                .ConfigureAwait(false) +
                            await LanguageManager
                                .GetStringAsync("String_Space", _strSelectedLanguage)
                                .ConfigureAwait(false) + objNewInfo.Name
                            + Path.DirectorySeparatorChar + "manifest.xml"),
                        MessageBoxButtons.OK, MessageBoxIcon.Error).ConfigureAwait(false);
                    return;
                }

                if (_setCustomDataDirectoryInfos.Any(x => x != objInfoToRename &&
                                                          objNewInfo.CharacterSettingsSaveKey.Equals(
                                                              x.CharacterSettingsSaveKey,
                                                              StringComparison.OrdinalIgnoreCase)) &&
                    await Program.ShowScrollableMessageBoxAsync(this,
                        string.Format(
                            await LanguageManager.GetStringAsync(
                                "Message_Duplicate_CustomDataDirectoryName",
                                _strSelectedLanguage).ConfigureAwait(false), objNewInfo.Name),
                        await LanguageManager.GetStringAsync(
                            "MessageTitle_Duplicate_CustomDataDirectoryName",
                            _strSelectedLanguage).ConfigureAwait(false), MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning).ConfigureAwait(false) != DialogResult.Yes)
                    return;
                _setCustomDataDirectoryInfos.Remove(objInfoToRename);
                _setCustomDataDirectoryInfos.Add(objNewInfo);
                PopulateCustomDataDirectoryListBox();
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void chkEnablePlugins_CheckedChanged(object sender, EventArgs e)
        {
            PluginsShowOrHide(chkEnablePlugins.Checked);
            OptionsChanged(sender, e);
        }

#if DEBUG
        private async void cmdUploadPastebin_Click(object sender, EventArgs e)
        {
            const string strFilePath = "Insert local file here";
            System.Collections.Specialized.NameValueCollection data
                = new System.Collections.Specialized.NameValueCollection();
            string line;
            using (FileStream objFileStream
                   = new FileStream(strFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader sr = new StreamReader(objFileStream, Encoding.UTF8, true))
            {
                line = await sr.ReadToEndAsync().ConfigureAwait(false);
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
                    bytes = await wb.UploadValuesTaskAsync("https://pastebin.com/api/api_post.php", data)
                                    .ConfigureAwait(false);
                }
                catch (System.Net.WebException)
                {
                    return;
                }

                using (RecyclableMemoryStream objStream = new RecyclableMemoryStream(Utils.MemoryStreamManager))
                {
                    await objStream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
                    using (StreamReader reader = new StreamReader(objStream, Encoding.UTF8, true))
                    {
                        string response = await reader.ReadToEndAsync().ConfigureAwait(false);
                        Clipboard.SetText(response);
                    }
                }
            }
        }
#else
        private void cmdUploadPastebin_Click(object sender, EventArgs e)
        {
            // Method intentionally left empty.
        }
#endif

        private async void clbPlugins_VisibleChanged(object sender, EventArgs e)
        {
            await clbPlugins.DoThreadSafeAsync(x => x.Items.Clear()).ConfigureAwait(false);
            if (await Program.PluginLoader.MyPlugins.GetCountAsync().ConfigureAwait(false) == 0)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this).ConfigureAwait(false);
            try
            {
                await Program.PluginLoader.MyPlugins.ForEachAsync(async objPlugin =>
                {
                    try
                    {
                        await Program.MainForm.DoThreadSafeAsync(objPlugin.CustomInitialize)
                                     .ConfigureAwait(false);
                        if (GlobalSettings.PluginsEnabledDic.TryGetValue(objPlugin.ToString(), out bool blnChecked))
                        {
                            await clbPlugins.DoThreadSafeAsync(x => x.Items.Add(objPlugin, blnChecked))
                                            .ConfigureAwait(false);
                        }
                        else
                        {
                            await clbPlugins.DoThreadSafeAsync(x => x.Items.Add(objPlugin)).ConfigureAwait(false);
                        }
                    }
                    catch (ApplicationException ae)
                    {
                        Log.Debug(ae);
                    }
                }).ConfigureAwait(false);

                await clbPlugins.DoThreadSafeAsync(x =>
                {
                    if (x.Items.Count > 0)
                    {
                        x.SelectedIndex = 0;
                    }
                }).ConfigureAwait(false);
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private void clbPlugins_SelectedValueChanged(object sender, EventArgs e)
        {
            UserControl pluginControl = (clbPlugins.SelectedItem as IPlugin)?.GetOptionsControl();
            if (pluginControl != null)
            {
                pnlPluginOption.Controls.Clear();
                pnlPluginOption.Controls.Add(pluginControl);
            }
        }

        private async void clbPlugins_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this).ConfigureAwait(false);
            try
            {
                string strPlugin = (await clbPlugins.DoThreadSafeFuncAsync(x => x.Items[e.Index]).ConfigureAwait(false))?.ToString() ?? string.Empty;
                bool blnNewValue = e.NewValue == CheckState.Checked;
                GlobalSettings.PluginsEnabledDic.AddOrUpdate(strPlugin, blnNewValue, (x, y) => blnNewValue);
                OptionsChanged(sender, e);
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private void txtPDFAppPath_TextChanged(object sender, EventArgs e)
        {
            cmdRemovePDFAppPath.Enabled = txtPDFAppPath.TextLength > 0;
            cmdPDFTest.Enabled = txtPDFAppPath.TextLength > 0 && txtPDFLocation.TextLength > 0;
            OptionsChanged(sender, e);
        }

        private async void lsbCustomDataDirectories_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_intSkipRefresh > 0)
                return;
            ListItem objSelectedItem = await lsbCustomDataDirectories
                                             .DoThreadSafeFuncAsync(x => (ListItem) x.SelectedItem)
                                             .ConfigureAwait(false);
            CustomDataDirectoryInfo objSelected = (CustomDataDirectoryInfo) objSelectedItem.Value;
            if (objSelected == null)
            {
                await gpbDirectoryInfo.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                return;
            }

            await gpbDirectoryInfo.DoThreadSafeAsync(x => x.SuspendLayout()).ConfigureAwait(false);
            try
            {
                string strDescription = await objSelected.GetDisplayDescriptionAsync(_strSelectedLanguage)
                                                         .ConfigureAwait(false);
                await txtDirectoryDescription.DoThreadSafeAsync(x => x.Text = strDescription).ConfigureAwait(false);
                await lblDirectoryVersion.DoThreadSafeAsync(x => x.Text = objSelected.MyVersion.ToString())
                                         .ConfigureAwait(false);
                string strAuthors = await objSelected
                                          .GetDisplayAuthorsAsync(_strSelectedLanguage, _objSelectedCultureInfo)
                                          .ConfigureAwait(false);
                await lblDirectoryAuthors.DoThreadSafeAsync(x => x.Text = strAuthors).ConfigureAwait(false);
                await lblDirectoryName.DoThreadSafeAsync(x => x.Text = objSelected.Name).ConfigureAwait(false);
                string strText = objSelected.DirectoryPath.Replace(Utils.GetStartupPath,
                                                                   await LanguageManager
                                                                         .GetStringAsync(
                                                                             "String_Chummer5a", _strSelectedLanguage)
                                                                         .ConfigureAwait(false));
                await lblDirectoryPath.DoThreadSafeAsync(x => x.Text = strText).ConfigureAwait(false);

                if (objSelected.DependenciesList.Count > 0)
                {
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdDependencies))
                    {
                        foreach (DirectoryDependency dependency in objSelected.DependenciesList)
                            sbdDependencies.AppendLine(await dependency.GetDisplayNameAsync().ConfigureAwait(false));
                        await lblDependencies.DoThreadSafeAsync(x => x.Text = sbdDependencies.ToString())
                                             .ConfigureAwait(false);
                    }
                }
                else
                {
                    //Make sure all old information is discarded
                    await lblDependencies.DoThreadSafeAsync(x => x.Text = string.Empty).ConfigureAwait(false);
                }

                if (objSelected.IncompatibilitiesList.Count > 0)
                {
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdIncompatibilities))
                    {
                        foreach (DirectoryDependency exclusivity in objSelected.IncompatibilitiesList)
                        {
                            sbdIncompatibilities.AppendLine(
                                await exclusivity.GetDisplayNameAsync().ConfigureAwait(false));
                        }

                        await lblIncompatibilities.DoThreadSafeAsync(x => x.Text = sbdIncompatibilities.ToString())
                                                  .ConfigureAwait(false);
                    }
                }
                else
                {
                    //Make sure all old information is discarded
                    await lblIncompatibilities.DoThreadSafeAsync(x => x.Text = string.Empty).ConfigureAwait(false);
                }

                await gpbDirectoryInfo.DoThreadSafeAsync(x => x.Visible = true).ConfigureAwait(false);
            }
            finally
            {
                await gpbDirectoryInfo.DoThreadSafeAsync(x => x.ResumeLayout()).ConfigureAwait(false);
            }
        }

        #endregion Control Events

        #region Methods

        private bool _blnPromptPdfReaderOnLoad;

        public Task DoLinkPdfReader(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_intLoading > 0)
                _blnPromptPdfReaderOnLoad = true;
            else
                return PromptPdfAppPath(token);
            return Task.CompletedTask;
        }

        private string _strSelectCodeOnRefresh = string.Empty;

        public async Task DoLinkPdf(string strCode, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_intLoading > 0)
                _strSelectCodeOnRefresh = strCode;
            else
            {
                bool blnDoPromptPdf = await lstGlobalSourcebookInfos.DoThreadSafeFuncAsync(x =>
                {
                    if (!string.IsNullOrEmpty(strCode))
                        x.SelectedValue = strCode;
                    return x.SelectedIndex >= 0;
                }, token).ConfigureAwait(false);
                if (blnDoPromptPdf)
                    await PromptPdfLocation(token).ConfigureAwait(false);
            }
        }

        private async Task PromptPdfLocation(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!await txtPDFLocation.DoThreadSafeFuncAsync(x => x.Enabled, token).ConfigureAwait(false))
                return;
            // Prompt the user to select a save file to associate with this Contact.
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                string strNewFileName = string.Empty;
                string strFilter
                    = await LanguageManager.GetStringAsync("DialogFilter_Pdf", token: token).ConfigureAwait(false) + '|'
                    +
                    await LanguageManager.GetStringAsync("DialogFilter_All", token: token).ConfigureAwait(false);
                DialogResult eResult = await this.DoThreadSafeFuncAsync(x =>
                {
                    using (OpenFileDialog dlgOpenFile = new OpenFileDialog())
                    {
                        dlgOpenFile.Filter = strFilter;
                        if (!string.IsNullOrEmpty(txtPDFLocation.Text) && File.Exists(txtPDFLocation.Text))
                        {
                            dlgOpenFile.InitialDirectory = Path.GetDirectoryName(txtPDFLocation.Text);
                            dlgOpenFile.FileName = Path.GetFileName(txtPDFLocation.Text);
                        }

                        DialogResult eReturn = dlgOpenFile.ShowDialog(x);
                        strNewFileName = dlgOpenFile.FileName;
                        return eReturn;
                    }
                }, token: token).ConfigureAwait(false);

                if (eResult != DialogResult.OK)
                    return;

                try
                {
                    PdfReader objPdfReader = null;
                    try
                    {
                        objPdfReader = new PdfReader(strNewFileName);
                    }
                    finally
                    {
                        objPdfReader?.Close();
                    }
                }
                catch
                {
                    await Program.ShowScrollableMessageBoxAsync(this, string.Format(
                            await LanguageManager.GetStringAsync(
                                "Message_Options_FileIsNotPDF",
                                _strSelectedLanguage, token: token).ConfigureAwait(false),
                            Path.GetFileName(strNewFileName)),
                        await LanguageManager.GetStringAsync(
                            "MessageTitle_Options_FileIsNotPDF",
                            _strSelectedLanguage, token: token).ConfigureAwait(false),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error, token: token).ConfigureAwait(false);
                    return;
                }

                await lstGlobalSourcebookInfos.DoThreadSafeAsync(() => UpdateSourcebookInfoPath(strNewFileName), token: token).ConfigureAwait(false);
                await txtPDFLocation.DoThreadSafeAsync(x => x.Text = strNewFileName, token).ConfigureAwait(false);
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async Task PromptPdfAppPath(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // Prompt the user to select a save file to associate with this Contact.
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                string strFileName = string.Empty;
                string strFilter
                    = await LanguageManager.GetStringAsync("DialogFilter_Exe", token: token).ConfigureAwait(false) + '|'
                    +
                    await LanguageManager.GetStringAsync("DialogFilter_All", token: token).ConfigureAwait(false);
                DialogResult eResult = await this.DoThreadSafeFuncAsync(x =>
                {
                    using (OpenFileDialog dlgOpenFile = new OpenFileDialog())
                    {
                        dlgOpenFile.Filter = strFilter;
                        if (!string.IsNullOrEmpty(txtPDFAppPath.Text) && File.Exists(txtPDFAppPath.Text))
                        {
                            dlgOpenFile.InitialDirectory = Path.GetDirectoryName(txtPDFAppPath.Text);
                            dlgOpenFile.FileName = Path.GetFileName(txtPDFAppPath.Text);
                        }

                        DialogResult eReturn = dlgOpenFile.ShowDialog(x);
                        strFileName = dlgOpenFile.FileName;
                        return eReturn;
                    }
                }, token: token).ConfigureAwait(false);

                if (eResult != DialogResult.OK)
                    return;
                await txtPDFAppPath.DoThreadSafeAsync(x => x.Text = strFileName, token).ConfigureAwait(false);

                string strAppNameUpper = Path.GetFileName(strFileName).ToUpperInvariant();
                string strSelectedPdfParams = await cboPDFParameters
                                                    .DoThreadSafeFuncAsync(
                                                        x => x.SelectedValue?.ToString() ?? string.Empty, token: token)
                                                    .ConfigureAwait(false);
                if (!string.IsNullOrEmpty(strSelectedPdfParams)
                    && _dicCachedPdfAppNames.TryGetValue(strSelectedPdfParams, out HashSet<string> setAppNames)
                    && setAppNames.Contains(strAppNameUpper))
                    return;

                await _dicCachedPdfAppNames.ForEachWithBreakAsync(async kvpEntry =>
                {
                    if (kvpEntry.Value.Contains(strAppNameUpper))
                    {
                        string strNewSelected = kvpEntry.Key;
                        await cboPDFParameters.DoThreadSafeAsync(x =>
                        {
                            if (!string.IsNullOrEmpty(strNewSelected))
                                x.SelectedValue = strNewSelected;
                            if (x.SelectedIndex == -1)
                            {
                                if (!string.IsNullOrEmpty(strSelectedPdfParams))
                                    x.SelectedValue = strSelectedPdfParams;
                                if (x.SelectedIndex == -1)
                                    x.SelectedIndex = 0;
                            }
                        }, token).ConfigureAwait(false);
                        return false;
                    }

                    return true;
                }, token: token).ConfigureAwait(false);
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async Task TranslateForm(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await this.TranslateWinFormAsync(_strSelectedLanguage, token: token).ConfigureAwait(false);
            await PopulateDefaultCharacterSettingLists(token).ConfigureAwait(false);
            await PopulateMugshotCompressionOptions(token).ConfigureAwait(false);
            await PopulateChum5lzCompressionLevelOptions(token).ConfigureAwait(false);
            await SetToolTips(token).ConfigureAwait(false);

            await cboSheetLanguage.DoThreadSafeAsync(x =>
            {
                string strSheetLanguage = x.SelectedValue?.ToString();
                if (strSheetLanguage != _strSelectedLanguage
                    && x.Items.Cast<ListItem>().Any(y => y.Value.ToString() == _strSelectedLanguage))
                {
                    x.SelectedValue = _strSelectedLanguage;
                }
            }, token).ConfigureAwait(false);

            await PopulatePdfParameters(token).ConfigureAwait(false);
            PopulateCustomDataDirectoryListBox();
            await PopulateApplicationInsightsOptions(token).ConfigureAwait(false);
            await PopulateColorModes(token).ConfigureAwait(false);
            await PopulateDpiScalingMethods(token).ConfigureAwait(false);
        }

        private async Task RefreshGlobalSourcebookInfosListView(CancellationToken token = default)
        {
            // Load the Sourcebook information.
            // Put the Sourcebooks into a List so they can first be sorted.
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                           out List<ListItem> lstSourcebookInfos))
            {
                foreach (XPathNavigator objXmlBook in (await XmlManager
                                                                   .LoadXPathAsync(
                                                                       "books.xml", strLanguage: _strSelectedLanguage,
                                                                       token: token).ConfigureAwait(false))
                                                            .SelectAndCacheExpression(
                                                                "/chummer/books/book", token: token))
                {
                    string strCode = objXmlBook.SelectSingleNodeAndCacheExpression("code", token: token)?.Value;
                    if (!string.IsNullOrEmpty(strCode))
                    {
                        ListItem objBookInfo
                            = new ListItem(
                                strCode,
                                objXmlBook.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value
                                ?? objXmlBook.SelectSingleNodeAndCacheExpression("name", token: token)?.Value ?? strCode);
                        lstSourcebookInfos.Add(objBookInfo);
                    }
                }

                lstSourcebookInfos.Sort(CompareListItems.CompareNames);
                string strOldSelected;
                Interlocked.Increment(ref _intSkipRefresh);
                try
                {
                    strOldSelected = await lstGlobalSourcebookInfos
                                           .DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token)
                                           .ConfigureAwait(false);
                    await lstGlobalSourcebookInfos.PopulateWithListItemsAsync(lstSourcebookInfos, token)
                                                  .ConfigureAwait(false);
                }
                finally
                {
                    Interlocked.Decrement(ref _intSkipRefresh);
                }

                await lstGlobalSourcebookInfos.DoThreadSafeAsync(x =>
                {
                    if (string.IsNullOrEmpty(strOldSelected))
                        x.SelectedIndex = -1;
                    else
                        x.SelectedValue = strOldSelected;
                }, token).ConfigureAwait(false);
            }
        }

        private void PopulateCustomDataDirectoryListBox()
        {
            ListItem objOldSelected;
            Interlocked.Increment(ref _intSkipRefresh);
            try
            {
                objOldSelected = lsbCustomDataDirectories.SelectedIndex != -1
                    ? (ListItem)lsbCustomDataDirectories.SelectedItem
                    : ListItem.Blank;
                lsbCustomDataDirectories.BeginUpdate();
                try
                {
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
                            CustomDataDirectoryInfo objExistingInfo
                                = (CustomDataDirectoryInfo)objExistingItem.Value;
                            if (!_setCustomDataDirectoryInfos.Contains(objExistingInfo))
                                lsbCustomDataDirectories.Items.RemoveAt(iI);
                            else
                                setListedInfos.Add(objExistingInfo);
                        }

                        foreach (CustomDataDirectoryInfo objCustomDataDirectory in _setCustomDataDirectoryInfos
                                     .Where(
                                         y => !setListedInfos.Contains(y)))
                        {
                            ListItem objItem = new ListItem(objCustomDataDirectory, objCustomDataDirectory.Name);
                            lsbCustomDataDirectories.Items.Add(objItem);
                        }
                    }

                    if (_intLoading > 0)
                    {
                        lsbCustomDataDirectories.DisplayMember = nameof(ListItem.Name);
                        lsbCustomDataDirectories.ValueMember = nameof(ListItem.Value);
                    }
                }
                finally
                {
                    lsbCustomDataDirectories.EndUpdate();
                }
            }
            finally
            {
                Interlocked.Decrement(ref _intSkipRefresh);
            }

            lsbCustomDataDirectories.SelectedItem = objOldSelected;
        }

        /// <summary>
        /// Set the values for all of the controls based on the Options for the selected Setting.
        /// </summary>
        private async Task PopulateOptions(CancellationToken token = default)
        {
            await RefreshGlobalSourcebookInfosListView(token).ConfigureAwait(false);
            PopulateCustomDataDirectoryListBox();

            await chkAutomaticUpdate.DoThreadSafeAsync(x => x.Checked = GlobalSettings.AutomaticUpdate, token)
                                    .ConfigureAwait(false);
            await chkPreferNightlyBuilds.DoThreadSafeAsync(x => x.Checked = GlobalSettings.PreferNightlyBuilds, token)
                                        .ConfigureAwait(false);
            await chkLiveCustomData.DoThreadSafeAsync(x => x.Checked = GlobalSettings.LiveCustomData, token)
                                   .ConfigureAwait(false);
            await chkLiveUpdateCleanCharacterFiles
                  .DoThreadSafeAsync(x => x.Checked = GlobalSettings.LiveUpdateCleanCharacterFiles, token)
                  .ConfigureAwait(false);
            await chkUseLogging.DoThreadSafeAsync(x => x.Checked = GlobalSettings.UseLogging, token)
                               .ConfigureAwait(false);
            await cboUseLoggingApplicationInsights.DoThreadSafeAsync(x => x.Enabled = GlobalSettings.UseLogging, token)
                                                  .ConfigureAwait(false);
            await PopulateApplicationInsightsOptions(token).ConfigureAwait(false);
            await PopulateColorModes(token).ConfigureAwait(false);
            await PopulateDpiScalingMethods(token).ConfigureAwait(false);

            await chkLifeModule.DoThreadSafeAsync(x => x.Checked = GlobalSettings.LifeModuleEnabled, token)
                               .ConfigureAwait(false);
            await chkStartupFullscreen.DoThreadSafeAsync(x => x.Checked = GlobalSettings.StartupFullscreen, token)
                                      .ConfigureAwait(false);
            await chkSingleDiceRoller.DoThreadSafeAsync(x => x.Checked = GlobalSettings.SingleDiceRoller, token)
                                     .ConfigureAwait(false);
            await chkDatesIncludeTime.DoThreadSafeAsync(x => x.Checked = GlobalSettings.DatesIncludeTime, token)
                                     .ConfigureAwait(false);
            await chkPrintToFileFirst.DoThreadSafeAsync(x => x.Checked = GlobalSettings.PrintToFileFirst, token)
                                     .ConfigureAwait(false);
            await chkPrintExpenses.DoThreadSafeAsync(x => x.Checked = GlobalSettings.PrintExpenses, token)
                                  .ConfigureAwait(false);
            await chkPrintFreeExpenses.DoThreadSafeAsync(x =>
            {
                x.Enabled = GlobalSettings.PrintExpenses;
                x.Checked = x.Enabled && GlobalSettings.PrintFreeExpenses;
            }, token).ConfigureAwait(false);
            await chkPrintNotes.DoThreadSafeAsync(x => x.Checked = GlobalSettings.PrintNotes, token)
                               .ConfigureAwait(false);
            await chkPrintSkillsWithZeroRating
                  .DoThreadSafeAsync(x => x.Checked = GlobalSettings.PrintSkillsWithZeroRating, token)
                  .ConfigureAwait(false);
            await chkInsertPdfNotesIfAvailable
                  .DoThreadSafeAsync(x => x.Checked = GlobalSettings.InsertPdfNotesIfAvailable, token)
                  .ConfigureAwait(false);
            await nudBrowserVersion.DoThreadSafeAsync(x => x.Value = GlobalSettings.EmulatedBrowserVersion, token)
                                   .ConfigureAwait(false);
            bool blnEnabled = await txtPDFAppPath.DoThreadSafeFuncAsync(x =>
            {
                x.Text = GlobalSettings.PdfAppPath;
                return x.TextLength > 0;
            }, token).ConfigureAwait(false);
            await cmdRemovePDFAppPath.DoThreadSafeAsync(x => x.Enabled = blnEnabled, token).ConfigureAwait(false);
            bool blnEnabled2 = await txtCharacterRosterPath.DoThreadSafeFuncAsync(x =>
            {
                x.Text = GlobalSettings.CharacterRosterPath;
                return x.TextLength > 0;
            }, token).ConfigureAwait(false);
            await cmdRemoveCharacterRoster.DoThreadSafeAsync(x => x.Enabled = blnEnabled2, token).ConfigureAwait(false);
            await chkHideMasterIndex.DoThreadSafeAsync(x => x.Checked = GlobalSettings.HideMasterIndex, token)
                                    .ConfigureAwait(false);
            await chkHideCharacterRoster.DoThreadSafeAsync(x => x.Checked = GlobalSettings.HideCharacterRoster, token)
                                        .ConfigureAwait(false);
            await chkCreateBackupOnCareer.DoThreadSafeAsync(x => x.Checked = GlobalSettings.CreateBackupOnCareer, token)
                                         .ConfigureAwait(false);
            await chkConfirmDelete.DoThreadSafeAsync(x => x.Checked = GlobalSettings.ConfirmDelete, token)
                                  .ConfigureAwait(false);
            await chkConfirmKarmaExpense.DoThreadSafeAsync(x => x.Checked = GlobalSettings.ConfirmKarmaExpense, token)
                                        .ConfigureAwait(false);
            await chkHideItemsOverAvail
                  .DoThreadSafeAsync(x => x.Checked = GlobalSettings.HideItemsOverAvailLimit, token)
                  .ConfigureAwait(false);
            await chkAllowHoverIncrement.DoThreadSafeAsync(x => x.Checked = GlobalSettings.AllowHoverIncrement, token)
                                        .ConfigureAwait(false);
            await chkSearchInCategoryOnly.DoThreadSafeAsync(x => x.Checked = GlobalSettings.SearchInCategoryOnly, token)
                                         .ConfigureAwait(false);
            await chkAllowSkillDiceRolling
                  .DoThreadSafeAsync(x => x.Checked = GlobalSettings.AllowSkillDiceRolling, token)
                  .ConfigureAwait(false);
            await chkAllowEasterEggs.DoThreadSafeAsync(x => x.Checked = GlobalSettings.AllowEasterEggs, token)
                                    .ConfigureAwait(false);
            await chkEnablePlugins.DoThreadSafeAsync(x => x.Checked = GlobalSettings.PluginsEnabled, token)
                                  .ConfigureAwait(false);
            await chkCustomDateTimeFormats
                  .DoThreadSafeAsync(x => x.Checked = GlobalSettings.CustomDateTimeFormats, token)
                  .ConfigureAwait(false);
            if (!await chkCustomDateTimeFormats.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false))
            {
                await txtDateFormat
                      .DoThreadSafeAsync(x => x.Text = GlobalSettings.CultureInfo.DateTimeFormat.ShortDatePattern,
                                         token).ConfigureAwait(false);
                await txtTimeFormat
                      .DoThreadSafeAsync(x => x.Text = GlobalSettings.CultureInfo.DateTimeFormat.ShortTimePattern,
                                         token).ConfigureAwait(false);
            }
            else
            {
                await txtDateFormat.DoThreadSafeAsync(x => x.Text = GlobalSettings.CustomDateFormat, token)
                                   .ConfigureAwait(false);
                await txtTimeFormat.DoThreadSafeAsync(x => x.Text = GlobalSettings.CustomTimeFormat, token)
                                   .ConfigureAwait(false);
            }

            bool blnShowHidePlugins = await chkEnablePlugins.DoThreadSafeFuncAsync(x => x.Checked, token)
                .ConfigureAwait(false);
            await tabOptions.DoThreadSafeAsync(() => PluginsShowOrHide(blnShowHidePlugins), token).ConfigureAwait(false);
        }

        private async Task SaveGlobalOptions(CancellationToken token = default)
        {
            GlobalSettings.AutomaticUpdate
                = await chkAutomaticUpdate.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false);
            GlobalSettings.LiveCustomData
                = await chkLiveCustomData.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false);
            GlobalSettings.LiveUpdateCleanCharacterFiles = await chkLiveUpdateCleanCharacterFiles
                                                                 .DoThreadSafeFuncAsync(x => x.Checked, token)
                                                                 .ConfigureAwait(false);
            GlobalSettings.UseLogging
                = await chkUseLogging.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false);
            if (Enum.TryParse(
                    await cboUseLoggingApplicationInsights.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token)
                                                          .ConfigureAwait(false), out UseAILogging useAI))
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

            await GlobalSettings.SetLanguageAsync(_strSelectedLanguage, token).ConfigureAwait(false);
            await GlobalSettings.SetColorModeSettingAsync(_eSelectedColorModeSetting, token).ConfigureAwait(false);
            GlobalSettings.DefaultHasNotesColor = _objSelectedHasNotesColor;
            GlobalSettings.DpiScalingMethodSetting = await cboDpiScalingMethod.DoThreadSafeFuncAsync(
                x => x.SelectedIndex >= 0
                    ? (DpiScalingMethod) Enum.Parse(typeof(DpiScalingMethod), x.SelectedValue.ToString())
                    : GlobalSettings.DefaultDpiScalingMethod, token).ConfigureAwait(false);
            GlobalSettings.StartupFullscreen = await chkStartupFullscreen.DoThreadSafeFuncAsync(x => x.Checked, token)
                                                                         .ConfigureAwait(false);
            GlobalSettings.SingleDiceRoller = await chkSingleDiceRoller.DoThreadSafeFuncAsync(x => x.Checked, token)
                                                                       .ConfigureAwait(false);
            GlobalSettings.DefaultCharacterSheet
                = await cboXSLT.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token).ConfigureAwait(false)
                  ?? GlobalSettings.DefaultCharacterSheetDefaultValue;
            GlobalSettings.DatesIncludeTime = await chkDatesIncludeTime.DoThreadSafeFuncAsync(x => x.Checked, token)
                                                                       .ConfigureAwait(false);
            GlobalSettings.PrintToFileFirst = await chkPrintToFileFirst.DoThreadSafeFuncAsync(x => x.Checked, token)
                                                                       .ConfigureAwait(false);
            GlobalSettings.PrintExpenses
                = await chkPrintExpenses.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false);
            GlobalSettings.PrintFreeExpenses = await chkPrintFreeExpenses.DoThreadSafeFuncAsync(x => x.Checked, token)
                                                                         .ConfigureAwait(false);
            GlobalSettings.PrintNotes
                = await chkPrintNotes.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false);
            GlobalSettings.PrintSkillsWithZeroRating = await chkPrintSkillsWithZeroRating
                                                             .DoThreadSafeFuncAsync(x => x.Checked, token)
                                                             .ConfigureAwait(false);
            GlobalSettings.InsertPdfNotesIfAvailable = await chkInsertPdfNotesIfAvailable
                                                             .DoThreadSafeFuncAsync(x => x.Checked, token)
                                                             .ConfigureAwait(false);
            GlobalSettings.EmulatedBrowserVersion
                = decimal.ToInt32(await nudBrowserVersion.DoThreadSafeFuncAsync(x => x.Value, token)
                                                         .ConfigureAwait(false));
            GlobalSettings.PdfAppPath
                = await txtPDFAppPath.DoThreadSafeFuncAsync(x => x.Text, token).ConfigureAwait(false);
            GlobalSettings.PdfParameters
                = await cboPDFParameters.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token)
                                        .ConfigureAwait(false) ?? string.Empty;
            GlobalSettings.LifeModuleEnabled
                = await chkLifeModule.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false);
            GlobalSettings.PreferNightlyBuilds = await chkPreferNightlyBuilds
                                                       .DoThreadSafeFuncAsync(x => x.Checked, token)
                                                       .ConfigureAwait(false);
            GlobalSettings.CharacterRosterPath = await txtCharacterRosterPath.DoThreadSafeFuncAsync(x => x.Text, token)
                                                                             .ConfigureAwait(false);
            GlobalSettings.HideMasterIndex
                = await chkHideMasterIndex.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false);
            GlobalSettings.HideCharacterRoster = await chkHideCharacterRoster
                                                       .DoThreadSafeFuncAsync(x => x.Checked, token)
                                                       .ConfigureAwait(false);
            GlobalSettings.CreateBackupOnCareer = await chkCreateBackupOnCareer
                                                        .DoThreadSafeFuncAsync(x => x.Checked, token)
                                                        .ConfigureAwait(false);
            GlobalSettings.ConfirmDelete
                = await chkConfirmDelete.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false);
            GlobalSettings.ConfirmKarmaExpense = await chkConfirmKarmaExpense
                                                       .DoThreadSafeFuncAsync(x => x.Checked, token)
                                                       .ConfigureAwait(false);
            GlobalSettings.HideItemsOverAvailLimit = await chkHideItemsOverAvail
                                                           .DoThreadSafeFuncAsync(x => x.Checked, token)
                                                           .ConfigureAwait(false);
            GlobalSettings.AllowHoverIncrement = await chkAllowHoverIncrement
                                                       .DoThreadSafeFuncAsync(x => x.Checked, token)
                                                       .ConfigureAwait(false);
            GlobalSettings.SearchInCategoryOnly = await chkSearchInCategoryOnly
                                                        .DoThreadSafeFuncAsync(x => x.Checked, token)
                                                        .ConfigureAwait(false);
            GlobalSettings.AllowSkillDiceRolling = await chkAllowSkillDiceRolling
                                                         .DoThreadSafeFuncAsync(x => x.Checked, token)
                                                         .ConfigureAwait(false);
            GlobalSettings.DefaultCharacterSetting = await cboDefaultCharacterSetting
                                                           .DoThreadSafeFuncAsync(
                                                               x => x.SelectedValue?.ToString(), token)
                                                           .ConfigureAwait(false)
                                                     ?? GlobalSettings.DefaultCharacterSettingDefaultValue;
            GlobalSettings.DefaultMasterIndexSetting = await cboDefaultMasterIndexSetting
                                                             .DoThreadSafeFuncAsync(
                                                                 x => x.SelectedValue?.ToString(), token)
                                                             .ConfigureAwait(false)
                                                       ?? GlobalSettings.DefaultMasterIndexSettingDefaultValue;
            GlobalSettings.AllowEasterEggs
                = await chkAllowEasterEggs.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false);
            GlobalSettings.PluginsEnabled
                = await chkEnablePlugins.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false);
            switch (await cboMugshotCompression.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token)
                                               .ConfigureAwait(false))
            {
                case "jpeg_automatic":
                    GlobalSettings.SavedImageQuality = -1;
                    break;
                case "jpeg_manual":
                    GlobalSettings.SavedImageQuality = await nudMugshotCompressionQuality
                                                             .DoThreadSafeFuncAsync(x => x.ValueAsInt, token)
                                                             .ConfigureAwait(false);
                    break;
                default:
                    GlobalSettings.SavedImageQuality = int.MaxValue;
                    break;
            }

            GlobalSettings.Chum5lzCompressionLevel = await cboChum5lzCompressionLevel.DoThreadSafeFuncAsync(
                x => x.SelectedIndex >= 0
                    ? (LzmaHelper.ChummerCompressionPreset) Enum.Parse(typeof(LzmaHelper.ChummerCompressionPreset),
                                                                       x.SelectedValue.ToString())
                    : GlobalSettings.DefaultChum5lzCompressionLevel, token).ConfigureAwait(false);
            GlobalSettings.CustomDateTimeFormats = await chkCustomDateTimeFormats
                                                         .DoThreadSafeFuncAsync(x => x.Checked, token)
                                                         .ConfigureAwait(false);
            if (GlobalSettings.CustomDateTimeFormats)
            {
                GlobalSettings.CustomDateFormat
                    = await txtDateFormat.DoThreadSafeFuncAsync(x => x.Text, token).ConfigureAwait(false);
                GlobalSettings.CustomTimeFormat
                    = await txtTimeFormat.DoThreadSafeFuncAsync(x => x.Text, token).ConfigureAwait(false);
            }

            token.ThrowIfCancellationRequested();
            GlobalSettings.CustomDataDirectoryInfos.Clear();
            token.ThrowIfCancellationRequested();
            foreach (CustomDataDirectoryInfo objInfo in _setCustomDataDirectoryInfos)
                GlobalSettings.CustomDataDirectoryInfos.Add(objInfo);
            await XmlManager.RebuildDataDirectoryInfoAsync(GlobalSettings.CustomDataDirectoryInfos, token)
                            .ConfigureAwait(false);
            await GlobalSettings.SetSourcebookInfosAsync(_dicSourcebookInfos, false, token).ConfigureAwait(false);
            await GlobalSettings.ReloadCustomSourcebookInfosAsync(token).ConfigureAwait(false);
        }

        /// <summary>
        /// Save the global settings to the registry.
        /// </summary>
        private async Task SaveRegistrySettings(CancellationToken token = default)
        {
            await SaveGlobalOptions(token).ConfigureAwait(false);
            await GlobalSettings.SaveOptionsToRegistry(token).ConfigureAwait(false);
        }

        private async Task PopulateDefaultCharacterSettingLists(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                           out List<ListItem> lstCharacterSettings))
            {
                foreach (KeyValuePair<string, CharacterSettings> kvpLoopCharacterOptions in await SettingsManager
                             .GetLoadedCharacterSettingsAsync(token).ConfigureAwait(false))
                {
                    string strId = kvpLoopCharacterOptions.Key;
                    if (!string.IsNullOrEmpty(strId))
                    {
                        string strName = kvpLoopCharacterOptions.Value.Name;
                        if (strName.IsGuid() || (strName.StartsWith('{') && strName.EndsWith('}')))
                        {
                            strName = await LanguageManager.GetStringAsync(strName.TrimStartOnce('{').TrimEndOnce('}'),
                                                                           _strSelectedLanguage, token: token)
                                                           .ConfigureAwait(false);
                        }

                        lstCharacterSettings.Add(new ListItem(strId, strName));
                    }
                }

                lstCharacterSettings.Sort(CompareListItems.CompareNames);

                string strOldSelectedDefaultCharacterSetting
                    = await cboDefaultCharacterSetting.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token)
                                                      .ConfigureAwait(false)
                      ?? GlobalSettings.DefaultCharacterSetting;

                await cboDefaultCharacterSetting.PopulateWithListItemsAsync(lstCharacterSettings, token)
                                                .ConfigureAwait(false);
                if (!string.IsNullOrEmpty(strOldSelectedDefaultCharacterSetting))
                {
                    await cboDefaultCharacterSetting.DoThreadSafeAsync(x =>
                    {
                        x.SelectedValue = strOldSelectedDefaultCharacterSetting;
                        if (x.SelectedIndex == -1 && lstCharacterSettings.Count > 0)
                            x.SelectedIndex = 0;
                    }, token).ConfigureAwait(false);
                }

                string strOldSelectedDefaultMasterIndexSetting
                    = await cboDefaultMasterIndexSetting.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token)
                                                        .ConfigureAwait(false)
                      ?? GlobalSettings.DefaultMasterIndexSetting;

                await cboDefaultMasterIndexSetting.PopulateWithListItemsAsync(lstCharacterSettings, token)
                                                  .ConfigureAwait(false);
                if (!string.IsNullOrEmpty(strOldSelectedDefaultMasterIndexSetting))
                {
                    await cboDefaultMasterIndexSetting.DoThreadSafeAsync(x =>
                    {
                        x.SelectedValue = strOldSelectedDefaultMasterIndexSetting;
                        if (x.SelectedIndex == -1 && lstCharacterSettings.Count > 0)
                            x.SelectedIndex = 0;
                    }, token).ConfigureAwait(false);
                }
            }
        }

        private async Task PopulateChum5lzCompressionLevelOptions(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                       out List<ListItem> lstChum5lzCompressionLevelOptions))
            {
                lstChum5lzCompressionLevelOptions.Add(new ListItem(LzmaHelper.ChummerCompressionPreset.Fastest,
                    await LanguageManager.GetStringAsync(
                            "String_Fastest_Option", token: token)
                        .ConfigureAwait(false)));
                lstChum5lzCompressionLevelOptions.Add(new ListItem(LzmaHelper.ChummerCompressionPreset.Fast,
                    await LanguageManager.GetStringAsync(
                            "String_Fast_Option", token: token)
                        .ConfigureAwait(false)));
                lstChum5lzCompressionLevelOptions.Add(new ListItem(LzmaHelper.ChummerCompressionPreset.Balanced,
                    await LanguageManager.GetStringAsync(
                            "String_Balanced_Option", token: token)
                        .ConfigureAwait(false)));
                lstChum5lzCompressionLevelOptions.Add(new ListItem(LzmaHelper.ChummerCompressionPreset.Thorough,
                    await LanguageManager.GetStringAsync(
                            "String_Thorough_Option", token: token)
                        .ConfigureAwait(false)));

                LzmaHelper.ChummerCompressionPreset eOldSelected
                    = await cboChum5lzCompressionLevel.DoThreadSafeFuncAsync(
                        x => x.SelectedIndex >= 0
                            ? (LzmaHelper.ChummerCompressionPreset)Enum.Parse(
                                typeof(LzmaHelper.ChummerCompressionPreset),
                                x.SelectedValue.ToString())
                            : GlobalSettings.Chum5lzCompressionLevel, token).ConfigureAwait(false);
                await cboChum5lzCompressionLevel.PopulateWithListItemsAsync(lstChum5lzCompressionLevelOptions, token)
                    .ConfigureAwait(false);
                await cboChum5lzCompressionLevel.DoThreadSafeAsync(x =>
                {
                    x.SelectedValue = eOldSelected;
                    if (x.SelectedIndex == -1 && lstChum5lzCompressionLevelOptions.Count > 0)
                    {
                        x.SelectedValue = GlobalSettings.DefaultChum5lzCompressionLevel;
                        if (x.SelectedIndex == -1)
                            x.SelectedIndex = 0;
                    }
                }, token).ConfigureAwait(false);
            }
        }

        private async Task PopulateMugshotCompressionOptions(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                           out List<ListItem> lstMugshotCompressionOptions))
            {
                lstMugshotCompressionOptions.Add(
                    new ListItem(
                        "png",
                        await LanguageManager.GetStringAsync("String_Lossless_Compression_Option", token: token)
                                             .ConfigureAwait(false)));
                lstMugshotCompressionOptions.Add(new ListItem("jpeg_automatic",
                                                              await LanguageManager.GetStringAsync(
                                                                  "String_Lossy_Automatic_Compression_Option",
                                                                  token: token).ConfigureAwait(false)));
                lstMugshotCompressionOptions.Add(new ListItem("jpeg_manual",
                                                              await LanguageManager.GetStringAsync(
                                                                  "String_Lossy_Manual_Compression_Option",
                                                                  token: token).ConfigureAwait(false)));

                string strOldSelected = await cboMugshotCompression
                                              .DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token)
                                              .ConfigureAwait(false);

                if (_intLoading > 0)
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

                    await nudMugshotCompressionQuality.DoThreadSafeAsync(x => x.Value = intQuality, token)
                                                      .ConfigureAwait(false);
                }

                await cboMugshotCompression.PopulateWithListItemsAsync(lstMugshotCompressionOptions, token)
                                           .ConfigureAwait(false);
                if (!string.IsNullOrEmpty(strOldSelected))
                {
                    await cboMugshotCompression.DoThreadSafeAsync(x =>
                    {
                        x.SelectedValue = strOldSelected;
                        if (x.SelectedIndex == -1 && lstMugshotCompressionOptions.Count > 0)
                            x.SelectedIndex = 0;
                    }, token).ConfigureAwait(false);
                }
            }

            bool blnShowQualitySelector
                = Equals(
                    await cboMugshotCompression.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token)
                                               .ConfigureAwait(false), "jpeg_manual");
            await lblMugshotCompressionQuality.DoThreadSafeAsync(x => x.Visible = blnShowQualitySelector, token)
                                              .ConfigureAwait(false);
            await nudMugshotCompressionQuality.DoThreadSafeAsync(x => x.Visible = blnShowQualitySelector, token)
                                              .ConfigureAwait(false);
        }

        private async Task PopulatePdfParameters(CancellationToken token = default)
        {
            int intIndex = 0;

            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                           out List<ListItem> lstPdfParameters))
            {
                foreach (XPathNavigator objXmlNode in (await XmlManager
                                                                   .LoadXPathAsync(
                                                                       "options.xml", strLanguage: _strSelectedLanguage,
                                                                       token: token).ConfigureAwait(false))
                                                            .SelectAndCacheExpression(
                                                                "/chummer/pdfarguments/pdfargument", token: token))
                {
                    string strValue = objXmlNode.SelectSingleNodeAndCacheExpression("value", token: token)?.Value;
                    if (string.IsNullOrEmpty(strValue))
                        continue;
                    lstPdfParameters.Add(new ListItem(
                                             strValue,
                                             objXmlNode
                                                 .SelectSingleNodeAndCacheExpression("translate", token: token)?.Value
                                             ?? objXmlNode
                                                 .SelectSingleNodeAndCacheExpression("name", token: token)?.Value
                                             ?? string.Empty));
                    if (!string.IsNullOrWhiteSpace(GlobalSettings.PdfParameters)
                        && GlobalSettings.PdfParameters == strValue)
                    {
                        intIndex = lstPdfParameters.Count - 1;
                    }

                    if (!_dicCachedPdfAppNames.TryGetValue(strValue, out HashSet<string> setAppNames))
                    {
                        setAppNames = Utils.StringHashSetPool.Get();
                        HashSet<string> setTemp = _dicCachedPdfAppNames.GetOrAdd(strValue, setAppNames);
                        if (!ReferenceEquals(setAppNames, setTemp))
                        {
                            Utils.StringHashSetPool.Return(ref setAppNames);
                            setAppNames = setTemp;
                        }
                    }

                    foreach (XPathNavigator objAppNameNode in objXmlNode
                                                                    .SelectAndCacheExpression(
                                                                        "appnames/appname", token))
                    {
                        setAppNames.Add(objAppNameNode.Value.ToUpperInvariant());
                    }
                }

                string strOldSelected = await cboPDFParameters
                                              .DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token)
                                              .ConfigureAwait(false);
                await cboPDFParameters.PopulateWithListItemsAsync(lstPdfParameters, token).ConfigureAwait(false);
                await cboPDFParameters.DoThreadSafeAsync(x =>
                {
                    x.SelectedIndex = intIndex;
                    if (!string.IsNullOrEmpty(strOldSelected))
                    {
                        x.SelectedValue = strOldSelected;
                        if (x.SelectedIndex == -1 && lstPdfParameters.Count > 0)
                            x.SelectedIndex = 0;
                    }
                }, token).ConfigureAwait(false);
            }
        }

        private async Task PopulateApplicationInsightsOptions(CancellationToken token = default)
        {
            string strOldSelected
                = await cboUseLoggingApplicationInsights.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token)
                                                        .ConfigureAwait(false)
                  ?? GlobalSettings.UseLoggingApplicationInsights.ToString();

            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                           out List<ListItem> lstUseAIOptions))
            {
                foreach (UseAILogging eOption in Enum.GetValues(typeof(UseAILogging)))
                {
                    //we don't want to allow the user to set the logging options in stable builds to higher than "not set".
                    if (Utils.IsMilestoneVersion && !Debugger.IsAttached
                                                 && eOption > UseAILogging.NotSet)
                        continue;
                    lstUseAIOptions.Add(new ListItem(
                                            eOption,
                                            await LanguageManager.GetStringAsync(
                                                "String_ApplicationInsights_" + eOption,
                                                _strSelectedLanguage, token: token).ConfigureAwait(false)));
                }

                await cboUseLoggingApplicationInsights.PopulateWithListItemsAsync(lstUseAIOptions, token)
                                                      .ConfigureAwait(false);
                await cboUseLoggingApplicationInsights.DoThreadSafeAsync(x =>
                {
                    if (!string.IsNullOrEmpty(strOldSelected))
                        x.SelectedValue = Enum.Parse(typeof(UseAILogging), strOldSelected);
                    if (x.SelectedIndex == -1 && lstUseAIOptions.Count > 0)
                        x.SelectedIndex = 0;
                }, token).ConfigureAwait(false);
            }
        }

        private async Task PopulateColorModes(CancellationToken token = default)
        {
            string strOldSelected = await cboColorMode.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token)
                                                      .ConfigureAwait(false)
                                    ?? GlobalSettings.ColorModeSetting.ToString();

            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                           out List<ListItem> lstColorModes))
            {
                foreach (ColorMode eLoopColorMode in Enum.GetValues(typeof(ColorMode)))
                {
                    lstColorModes.Add(new ListItem(eLoopColorMode,
                                                   await LanguageManager.GetStringAsync(
                                                                            "String_" + eLoopColorMode,
                                                                            _strSelectedLanguage, token: token)
                                                                        .ConfigureAwait(false)));
                }

                await cboColorMode.PopulateWithListItemsAsync(lstColorModes, token).ConfigureAwait(false);
                await cboColorMode.DoThreadSafeAsync(x =>
                {
                    if (!string.IsNullOrEmpty(strOldSelected))
                        x.SelectedValue = Enum.Parse(typeof(ColorMode), strOldSelected);
                    if (x.SelectedIndex == -1 && lstColorModes.Count > 0)
                        x.SelectedIndex = 0;
                }, token).ConfigureAwait(false);
            }
        }

        private async Task PopulateDpiScalingMethods(CancellationToken token = default)
        {
            string strOldSelected = await cboDpiScalingMethod.DoThreadSafeFuncAsync(
                                                                 x => x.SelectedValue?.ToString()
                                                                      ?? GlobalSettings.DpiScalingMethodSetting
                                                                          .ToString(), token)
                                                             .ConfigureAwait(false);

            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                           out List<ListItem> lstDpiScalingMethods))
            {
                foreach (DpiScalingMethod eLoopDpiScalingMethod in Enum.GetValues(typeof(DpiScalingMethod)))
                {
                    switch (eLoopDpiScalingMethod)
                    {
                        case DpiScalingMethod.Rescale:
                            if (Environment.OSVersion.Version
                                < new ValueVersion(
                                    6, 3, 0)) // Need at least Windows 8.1 to get PerMonitor/PerMonitorV2 Scaling
                                continue;
                            break;

                        case DpiScalingMethod.SmartZoom:
                            if (Environment.OSVersion.Version
                                < new ValueVersion(
                                    10, 0, 17763)) // Need at least Windows 10 Version 1809 to get GDI+ Scaling
                                continue;
                            break;
                    }

                    lstDpiScalingMethods.Add(new ListItem(eLoopDpiScalingMethod,
                                                          await LanguageManager.GetStringAsync(
                                                                                   "String_" + eLoopDpiScalingMethod,
                                                                                   _strSelectedLanguage, token: token)
                                                                               .ConfigureAwait(false)));
                }

                await cboDpiScalingMethod.PopulateWithListItemsAsync(lstDpiScalingMethods, token).ConfigureAwait(false);
                await cboDpiScalingMethod.DoThreadSafeAsync(x =>
                {
                    if (!string.IsNullOrEmpty(strOldSelected))
                        x.SelectedValue = Enum.Parse(typeof(DpiScalingMethod), strOldSelected);
                    if (x.SelectedIndex == -1 && lstDpiScalingMethods.Count > 0)
                        x.SelectedIndex = 0;
                }, token).ConfigureAwait(false);
            }
        }

        private async Task SetToolTips(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await cboUseLoggingApplicationInsights.SetToolTipAsync(string.Format(_objSelectedCultureInfo,
                                                                             await LanguageManager.GetStringAsync(
                                                                                     "Tip_Options_TelemetryId",
                                                                                     _strSelectedLanguage, token: token)
                                                                                 .ConfigureAwait(false),
                                                                             Properties.Settings.Default.UploadClientId
                                                                                 .ToString(
                                                                                     "D",
                                                                                     GlobalSettings
                                                                                         .InvariantCultureInfo))
                                                                         .WordWrap(), token).ConfigureAwait(false);
        }

        private async Task RefreshLanguageDocumentNames(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            _dicCachedLanguageDocumentNames.Clear();
            token.ThrowIfCancellationRequested();
            foreach (string strFilePath in Directory.EnumerateFiles(Utils.GetLanguageFolderPath, "*.xml"))
            {
                token.ThrowIfCancellationRequested();
                if (strFilePath.EndsWith("_data.xml"))
                    continue;
                string strLanguageName = await LanguageManager.GetLanguageNameFromFileNameAsync(strFilePath, token: token).ConfigureAwait(false);
                if (string.IsNullOrEmpty(strLanguageName))
                    continue;
                token.ThrowIfCancellationRequested();
                _dicCachedLanguageDocumentNames.AddOrUpdate(Path.GetFileNameWithoutExtension(strFilePath), x => strLanguageName, (x, y) => strLanguageName);
            }
        }

        private async Task PopulateLanguageList(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstLanguages))
            {
                foreach (KeyValuePair<string, string> kvpLanguages in _dicCachedLanguageDocumentNames)
                {
                    token.ThrowIfCancellationRequested();
                    lstLanguages.Add(new ListItem(kvpLanguages.Key, kvpLanguages.Value));
                }

                token.ThrowIfCancellationRequested();
                lstLanguages.Sort(CompareListItems.CompareNames);
                await cboLanguage.PopulateWithListItemsAsync(lstLanguages, token).ConfigureAwait(false);
            }
        }

        private async Task PopulateSheetLanguageList(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                                                            out HashSet<string> setLanguagesWithSheets))
            {
                // Populate the XSL list with all of the manifested XSL files found in the sheets\[language] directory.
                foreach (XPathNavigator xmlSheetLanguage in (await XmlManager
                                                                         .LoadXPathAsync("sheets.xml", token: token)
                                                                         .ConfigureAwait(false))
                                                                  .SelectAndCacheExpression(
                                                                      "/chummer/sheets/@lang", token: token))
                {
                    setLanguagesWithSheets.Add(xmlSheetLanguage.Value);
                }

                token.ThrowIfCancellationRequested();

                using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                               out List<ListItem> lstSheetLanguages))
                {
                    foreach (KeyValuePair<string, string> kvpLanguages in _dicCachedLanguageDocumentNames)
                    {
                        token.ThrowIfCancellationRequested();
                        string strLanguageName = kvpLanguages.Key;
                        if (!setLanguagesWithSheets.Contains(strLanguageName))
                            continue;
                        lstSheetLanguages.Add(new ListItem(strLanguageName, kvpLanguages.Value));
                    }

                    token.ThrowIfCancellationRequested();
                    lstSheetLanguages.Sort(CompareListItems.CompareNames);
                    await cboSheetLanguage.PopulateWithListItemsAsync(lstSheetLanguages, token).ConfigureAwait(false);
                }
            }
        }

        private async Task PopulateXsltList(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strSelectedSheetLanguage = await cboSheetLanguage
                                                    .DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token)
                                                    .ConfigureAwait(false);
            await imgSheetLanguageFlag
                .DoThreadSafeAsync(
                    x => x.Image = FlagImageGetter.GetFlagFromCountryCode(strSelectedSheetLanguage?.Substring(3, 2),
                        Math.Min(x.Width, x.Height)), token).ConfigureAwait(false);

            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstFiles))
            {
                // Populate the XSL list with all of the manifested XSL files found in the sheets\[language] directory.
                foreach (XPathNavigator xmlSheet in (await XmlManager.LoadXPathAsync("sheets.xml", token: token)
                                                                           .ConfigureAwait(false))
                                                          .SelectAndCacheExpression(
                                                              "/chummer/sheets[@lang="
                                                              + GlobalSettings.Language.CleanXPath()
                                                              + "]/sheet[not(hide)]", token: token))
                {
                    string strFile
                        = xmlSheet.SelectSingleNodeAndCacheExpression("filename", token: token)?.Value ?? string.Empty;
                    lstFiles.Add(new ListItem(
                                     !GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage,
                                                                     StringComparison.OrdinalIgnoreCase)
                                         ? Path.Combine(GlobalSettings.Language, strFile)
                                         : strFile,
                                     xmlSheet.SelectSingleNodeAndCacheExpression("name", token: token)?.Value ?? string.Empty));
                }

                string strOldSelected;
                try
                {
                    strOldSelected = await cboXSLT.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token)
                                                  .ConfigureAwait(false) ?? string.Empty;
                }
                catch (IndexOutOfRangeException)
                {
                    strOldSelected = string.Empty;
                }

                // Strip away the language prefix
                int intPos = strOldSelected.LastIndexOf(Path.DirectorySeparatorChar);
                if (intPos != -1)
                    strOldSelected = strOldSelected.Substring(intPos + 1);

                await cboXSLT.PopulateWithListItemsAsync(lstFiles, token).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(strOldSelected))
                {
                    await cboXSLT.DoThreadSafeAsync(x =>
                    {
                        x.SelectedValue =
                            !string.IsNullOrEmpty(strSelectedSheetLanguage) &&
                            !strSelectedSheetLanguage.Equals(GlobalSettings.DefaultLanguage,
                                                             StringComparison.OrdinalIgnoreCase)
                                ? Path.Combine(strSelectedSheetLanguage, strOldSelected)
                                : strOldSelected;
                        // If the desired sheet was not found, fall back to the Shadowrun 5 sheet.
                        if (x.SelectedIndex == -1 && lstFiles.Count > 0)
                        {
                            x.SelectedValue =
                                !string.IsNullOrEmpty(strSelectedSheetLanguage) &&
                                !strSelectedSheetLanguage.Equals(GlobalSettings.DefaultLanguage,
                                                                 StringComparison.OrdinalIgnoreCase)
                                    ? Path.Combine(strSelectedSheetLanguage,
                                                   GlobalSettings.DefaultCharacterSheetDefaultValue)
                                    : GlobalSettings.DefaultCharacterSheetDefaultValue;
                            if (x.SelectedIndex == -1)
                            {
                                x.SelectedIndex = 0;
                            }
                        }
                    }, token).ConfigureAwait(false);
                }
            }
        }

        private Task SetDefaultValueForLanguageList(CancellationToken token = default)
        {
            return cboLanguage.DoThreadSafeAsync(x =>
            {
                x.SelectedValue = GlobalSettings.Language;
                if (x.SelectedIndex == -1)
                    x.SelectedValue = GlobalSettings.DefaultLanguage;
            }, token);
        }

        private Task SetDefaultValueForSheetLanguageList(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strDefaultCharacterSheet = GlobalSettings.DefaultCharacterSheet;
            if (string.IsNullOrEmpty(strDefaultCharacterSheet)
                || strDefaultCharacterSheet == "Shadowrun (Rating greater 0)")
                strDefaultCharacterSheet = GlobalSettings.DefaultCharacterSheetDefaultValue;

            string strDefaultSheetLanguage = GlobalSettings.Language;
            int intLastIndexDirectorySeparator = strDefaultCharacterSheet.LastIndexOf(Path.DirectorySeparatorChar);
            if (intLastIndexDirectorySeparator != -1)
            {
                string strSheetLanguage = strDefaultCharacterSheet.Substring(0, intLastIndexDirectorySeparator);
                if (strSheetLanguage.Length == 5)
                    strDefaultSheetLanguage = strSheetLanguage;
            }

            return cboSheetLanguage.DoThreadSafeAsync(x =>
            {
                if (!string.IsNullOrEmpty(strDefaultSheetLanguage))
                    x.SelectedValue = strDefaultSheetLanguage;
                if (x.SelectedIndex == -1)
                    x.SelectedValue = GlobalSettings.DefaultLanguage;
            }, token);
        }

        private Task SetDefaultValueForXsltList(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(GlobalSettings.DefaultCharacterSheet))
                GlobalSettings.DefaultCharacterSheet = GlobalSettings.DefaultCharacterSheetDefaultValue;
            return cboXSLT.DoThreadSafeAsync(x =>
            {
                x.SelectedValue = GlobalSettings.DefaultCharacterSheet;
                if (cboXSLT.SelectedValue == null && cboXSLT.Items.Count > 0)
                {
                    int intNameIndex;
                    string strLanguage = _strSelectedLanguage;
                    if (string.IsNullOrEmpty(strLanguage)
                        || strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    {
                        intNameIndex = x.FindStringExact(GlobalSettings.DefaultCharacterSheet);
                    }
                    else
                    {
                        intNameIndex = x.FindStringExact(
                            GlobalSettings.DefaultCharacterSheet.Substring(
                                GlobalSettings.DefaultLanguage.LastIndexOf(Path.DirectorySeparatorChar) + 1));
                    }

                    x.SelectedIndex = Math.Max(0, intNameIndex);
                }
            }, token);
        }

        private void UpdateSourcebookInfoPath(string strPath)
        {
            string strTag = lstGlobalSourcebookInfos.SelectedValue?.ToString() ?? string.Empty;
            if (_dicSourcebookInfos.TryGetValue(strTag, out SourcebookInfo objFoundSource) && objFoundSource != null)
            {
                objFoundSource.Path = strPath;
            }
            else
            {
                objFoundSource = new SourcebookInfo
                {
                    Code = strTag,
                    Path = strPath
                };
                // If the Sourcebook was not found in the options, add it.
                _dicSourcebookInfos.AddOrUpdate(strTag, objFoundSource, (x, y) =>
                {
                    y.Path = strPath;
                    objFoundSource.Dispose();
                    return y;
                });
            }
        }

        private void OptionsChanged(object sender, EventArgs e)
        {
            if (_intLoading == 0)
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
            else if (tabOptions.TabPages.Contains(tabPlugins))
                tabOptions.TabPages.Remove(tabPlugins);
        }

        #endregion Methods

        private async void bScanForPDFs_Click(object sender, EventArgs e)
        {
            // Prompt the user to select a save file to associate with this Contact.
            CursorWait objCursorWait = await CursorWait.NewAsync(this).ConfigureAwait(false);
            try
            {
                Task<XPathNavigator> tskLoadBooks
                    = XmlManager.LoadXPathAsync("books.xml", strLanguage: _strSelectedLanguage);
                string strSelectedPath = string.Empty;
                string strDialogDescription = await LanguageManager.GetStringAsync("String_SelectFolderForPDFScan", _strSelectedLanguage).ConfigureAwait(false);
                DialogResult eResult = await this.DoThreadSafeFuncAsync(x =>
                {
                    using (FolderBrowserDialog dlgSelectFolder = new FolderBrowserDialog())
                    {
                        dlgSelectFolder.Description = strDialogDescription;
                        dlgSelectFolder.ShowNewFolderButton = false;
                        DialogResult eReturn = dlgSelectFolder.ShowDialog(x);
                        strSelectedPath = dlgSelectFolder.SelectedPath;
                        return eReturn;
                    }
                }).ConfigureAwait(false);

                if (eResult != DialogResult.OK || string.IsNullOrWhiteSpace(strSelectedPath))
                    return;

                eResult = await Program.ShowScrollableMessageBoxAsync(
                    await LanguageManager.GetStringAsync("Message_ScanFoldersRecursively", _strSelectedLanguage).ConfigureAwait(false),
                    buttons: MessageBoxButtons.YesNoCancel,
                    icon: MessageBoxIcon.Question).ConfigureAwait(false);
                if (eResult == DialogResult.Cancel)
                    return;
                SearchOption eOption = eResult == DialogResult.Yes
                    ? SearchOption.AllDirectories
                    : SearchOption.TopDirectoryOnly;

                using (new FetchSafelyFromSafeObjectPool<Stopwatch>(Utils.StopwatchPool, out Stopwatch sw))
                {
                    sw.Start();
                    XPathNavigator objBooks = await tskLoadBooks.ConfigureAwait(false);
                    string[] astrFiles = Directory.GetFiles(strSelectedPath, "*.pdf", eOption);
                    ConcurrentDictionary<string, Tuple<string, int>> dicPatternsToMatch
                        = new ConcurrentDictionary<string, Tuple<string, int>>();
                    ConcurrentDictionary<string, Tuple<string, int>> dicBackupPatternsToMatch
                        = new ConcurrentDictionary<string, Tuple<string, int>>();
                    foreach (XPathNavigator objBook in objBooks
                                 .SelectAndCacheExpression(
                                     "/chummer/books/book[matches/match/language = "
                                     + _strSelectedLanguage.CleanXPath() + ']'))
                    {
                        string strCode
                            = objBook.SelectSingleNodeAndCacheExpression("code")
                            ?.Value;
                        if (string.IsNullOrEmpty(strCode))
                            continue;
                        XPathNavigator objMatch
                            = objBook.SelectSingleNodeAndCacheExpression(
                                    "matches/match[language = " + _strSelectedLanguage.CleanXPath() + ']');
                        if (objMatch == null)
                            continue;
                        string strMatchText
                            = objMatch.SelectSingleNodeAndCacheExpression("text")
                            ?.Value;
                        if (string.IsNullOrEmpty(strMatchText))
                            continue;
                        if (!int.TryParse(
                                objMatch.SelectSingleNodeAndCacheExpression("page")
                                ?.Value, out int intMatchPage))
                            continue;
                        Tuple<string, int> tupValue = new Tuple<string, int>(strMatchText, intMatchPage);
                        dicPatternsToMatch.AddOrUpdate(strCode, tupValue, (x, y) => tupValue);
                    }

                    foreach (XPathNavigator objBook in objBooks
                                 .SelectAndCacheExpression("/chummer/books/book[not(matches/match/language = "
                                     + _strSelectedLanguage.CleanXPath() + ")]"))
                    {
                        string strCode
                            = objBook.SelectSingleNodeAndCacheExpression("code")
                            ?.Value;
                        if (string.IsNullOrEmpty(strCode))
                            continue;
                        XPathNavigator objMatch = null;
                        if (_strSelectedLanguage != GlobalSettings.DefaultLanguage)
                        {
                            objMatch = objBook.SelectSingleNodeAndCacheExpression(
                                    "matches/match[language = " + GlobalSettings.DefaultLanguage.CleanXPath() + ']');
                        }
                        if (objMatch == null)
                        {
                            objMatch = objBook.SelectSingleNodeAndCacheExpression("matches/match[not(language = " + _strSelectedLanguage.CleanXPath() + ")]");
                            if (objMatch == null)
                                continue;
                        }
                        string strMatchText
                            = objMatch.SelectSingleNodeAndCacheExpression("text")
                            ?.Value;
                        if (string.IsNullOrEmpty(strMatchText))
                            continue;
                        if (dicPatternsToMatch.TryGetValue(strCode, out Tuple<string, int> tupMainValue)
                            && string.Equals(strMatchText, tupMainValue.Item1))
                            continue;
                        if (!int.TryParse(
                                objMatch.SelectSingleNodeAndCacheExpression("page")
                                ?.Value, out int intMatchPage))
                            continue;
                        Tuple<string, int> tupValue = new Tuple<string, int>(strMatchText, intMatchPage);
                        dicBackupPatternsToMatch.AddOrUpdate(strCode, tupValue, (x, y) => tupValue);
                    }

                    using (ThreadSafeForm<LoadingBar> frmLoadingBar
                           = await Program.CreateAndShowProgressBarAsync(strSelectedPath, dicPatternsToMatch.IsEmpty || dicBackupPatternsToMatch.IsEmpty ? astrFiles.Length : astrFiles.Length * 2)
                               .ConfigureAwait(false))
                    {
                        List<SourcebookInfo> list =
                            await ScanFilesForPDFTexts(astrFiles, dicPatternsToMatch, dicBackupPatternsToMatch, frmLoadingBar.MyForm)
                                .ConfigureAwait(false);
                        sw.Stop();
                        using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                   out StringBuilder sbdFeedback))
                        {
                            sbdFeedback.AppendLine().AppendLine()
                                .AppendLine("-------------------------------------------------------------")
                                .AppendFormat(GlobalSettings.InvariantCultureInfo,
                                    "Scan for PDFs in Folder {0} completed in {1}ms.{2}{3} sourcebook(s) was/were found:",
                                    strSelectedPath, sw.ElapsedMilliseconds, Environment.NewLine,
                                    list.Count).AppendLine().AppendLine();
                            foreach (SourcebookInfo sourcebook in list)
                            {
                                sbdFeedback.AppendFormat(GlobalSettings.InvariantCultureInfo,
                                    "{0} with Offset {1} path: {2}", sourcebook.Code,
                                    sourcebook.Offset, sourcebook.Path).AppendLine();
                            }

                            sbdFeedback.AppendLine()
                                .AppendLine("-------------------------------------------------------------");
                            Log.Info(sbdFeedback.ToString());
                        }

                        string message = string.Format(_objSelectedCultureInfo,
                            await LanguageManager.GetStringAsync(
                                    "Message_FoundPDFsInFolder",
                                    _strSelectedLanguage)
                                .ConfigureAwait(false),
                            list.Count, strSelectedPath);
                        string title
                            = await LanguageManager.GetStringAsync("MessageTitle_FoundPDFsInFolder",
                                _strSelectedLanguage).ConfigureAwait(false);
                        await Program.ShowScrollableMessageBoxAsync(message, title, MessageBoxButtons.OK,
                            MessageBoxIcon.Information).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async Task<List<SourcebookInfo>> ScanFilesForPDFTexts(string[] lstFiles,
                                                                      ConcurrentDictionary<string, Tuple<string, int>> dicPatternsToMatch,
                                                                      ConcurrentDictionary<string, Tuple<string, int>> dicBackupPatternsToMatch,
                                                                      LoadingBar frmProgressBar,
                                                                      CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // ConcurrentDictionary makes sure we don't pick out multiple files for the same sourcebook
            ConcurrentDictionary<string, SourcebookInfo>
                dicResults = new ConcurrentDictionary<string, SourcebookInfo>();
            List<Task<List<SourcebookInfo>>> lstLoadingTasks = new List<Task<List<SourcebookInfo>>>(Utils.MaxParallelBatchSize);
            int intCounter = 0;
            if (dicPatternsToMatch?.IsEmpty == false)
            {
                int intFileCounter = 0;
                foreach (string strFile in lstFiles)
                {
                    token.ThrowIfCancellationRequested();
                    lstLoadingTasks.Add(GetSourcebookInfo(strFile, dicPatternsToMatch));
                    ++intFileCounter;
                    if (++intCounter != Utils.MaxParallelBatchSize)
                        continue;
                    await Task.WhenAll(lstLoadingTasks).ConfigureAwait(false);
                    foreach (Task<List<SourcebookInfo>> tskLoop in lstLoadingTasks)
                    {
                        foreach (SourcebookInfo objInfo in await tskLoop.ConfigureAwait(false))
                        {
                            // ReSharper disable once AccessToDisposedClosure
                            if (objInfo == null)
                                continue;
                            dicResults.AddOrUpdate(objInfo.Code, objInfo, (x, y) =>
                            {
                                y.Path = objInfo.Path;
                                y.Offset = objInfo.Offset;
                                objInfo.Dispose();
                                return y;
                            });
                        }
                    }

                    intCounter = 0;
                    lstLoadingTasks.Clear();
                    if (dicPatternsToMatch.IsEmpty)
                    {
                        for (; intFileCounter <= lstFiles.Length; ++intFileCounter)
                        {
                            await frmProgressBar
                              .PerformStepAsync(eUseTextPattern: LoadingBar.ProgressBarTextPatterns.Scanning, token: token)
                              .ConfigureAwait(false);
                        }
                        break;
                    }
                }

                await Task.WhenAll(lstLoadingTasks).ConfigureAwait(false);
                foreach (Task<List<SourcebookInfo>> tskLoop in lstLoadingTasks)
                {
                    foreach (SourcebookInfo objInfo in await tskLoop.ConfigureAwait(false))
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        if (objInfo == null)
                            continue;
                        dicResults.AddOrUpdate(objInfo.Code, objInfo, (x, y) =>
                        {
                            y.Path = objInfo.Path;
                            y.Offset = objInfo.Offset;
                            objInfo.Dispose();
                            return y;
                        });
                    }
                }
            }
            if (dicBackupPatternsToMatch?.IsEmpty == false)
            {
                intCounter = 0;
                lstLoadingTasks.Clear();
                string strFallbackFormat = await LanguageManager.GetStringAsync("String_Fallback_Pattern", _strSelectedLanguage, token: token).ConfigureAwait(false);
                foreach (string strFile in lstFiles)
                {
                    token.ThrowIfCancellationRequested();
                    lstLoadingTasks.Add(GetSourcebookInfo(strFile, dicBackupPatternsToMatch, strFallbackFormat));
                    if (++intCounter != Utils.MaxParallelBatchSize)
                        continue;
                    await Task.WhenAll(lstLoadingTasks).ConfigureAwait(false);
                    foreach (Task<List<SourcebookInfo>> tskLoop in lstLoadingTasks)
                    {
                        foreach (SourcebookInfo objInfo in await tskLoop.ConfigureAwait(false))
                        {
                            // ReSharper disable once AccessToDisposedClosure
                            if (objInfo == null)
                                continue;
                            dicResults.AddOrUpdate(objInfo.Code, objInfo, (x, y) =>
                            {
                                y.Path = objInfo.Path;
                                y.Offset = objInfo.Offset;
                                objInfo.Dispose();
                                return y;
                            });
                        }
                    }

                    intCounter = 0;
                    lstLoadingTasks.Clear();
                    if (dicBackupPatternsToMatch.IsEmpty)
                        break;
                }

                await Task.WhenAll(lstLoadingTasks).ConfigureAwait(false);
                foreach (Task<List<SourcebookInfo>> tskLoop in lstLoadingTasks)
                {
                    foreach (SourcebookInfo objInfo in await tskLoop.ConfigureAwait(false))
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        if (objInfo == null)
                            continue;
                        dicResults.AddOrUpdate(objInfo.Code, objInfo, (x, y) =>
                        {
                            y.Path = objInfo.Path;
                            y.Offset = objInfo.Offset;
                            objInfo.Dispose();
                            return y;
                        });
                    }
                }
            }

            async Task<List<SourcebookInfo>> GetSourcebookInfo(string strBookFile, ConcurrentDictionary<string, Tuple<string, int>> dicPatternsToUse, string strProgressBarTextFormat = "")
            {
                FileInfo objFileInfo = new FileInfo(strBookFile);
                string strText = string.IsNullOrEmpty(strProgressBarTextFormat)
                    ? objFileInfo.Name
                    : string.Format(_objSelectedCultureInfo, strProgressBarTextFormat, objFileInfo.Name);
                await frmProgressBar
                      .PerformStepAsync(strText, LoadingBar.ProgressBarTextPatterns.Scanning, token)
                      .ConfigureAwait(false);
                return await ScanPDFForMatchingText(objFileInfo.FullName, dicPatternsToUse, token).ConfigureAwait(false);
            }

            List<SourcebookInfo> lstReturn
                = new List<SourcebookInfo>(dicResults.Count);
            foreach (KeyValuePair<string, SourcebookInfo> kvpInfo in dicResults)
            {
                token.ThrowIfCancellationRequested();
                lstReturn.Add(_dicSourcebookInfos.AddOrUpdate(kvpInfo.Key, kvpInfo.Value, (x, y) =>
                {
                    y.Path = kvpInfo.Value.Path;
                    y.Offset = kvpInfo.Value.Offset;
                    kvpInfo.Value.Dispose();
                    return y;
                }));
            }

            return lstReturn;
        }

        private static async Task<List<SourcebookInfo>> ScanPDFForMatchingText(
            string strPath, ConcurrentDictionary<string, Tuple<string, int>> dicPatternsToMatch, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<SourcebookInfo> lstReturn = new List<SourcebookInfo>();
            if (dicPatternsToMatch.IsEmpty)
                return lstReturn;
            PdfReader objPdfReader = null;
            PdfDocument objPdfDocument = null;
            try
            {
                try
                {
                    objPdfReader = new PdfReader(strPath);
                    objPdfDocument = new PdfDocument(objPdfReader);
                }
                catch (iText.IO.Exceptions.IOException e)
                {
                    if (e.Message == "PDF header not found.")
                        return lstReturn;
                    throw;
                }
                catch (Exception e)
                {
                    //Loading failed, probably not a PDF file
                    Log.Warn(
                        e,
                        "Could not load file " + strPath
                                               + " and open it as PDF to search for text.");
                    return lstReturn;
                }

                token.ThrowIfCancellationRequested();
                List<string> lstKeysToLoop = dicPatternsToMatch.GetKeysToListSafe();
                //Search the first 15 pages for all the text
                int intMaxPage = Math.Min(15, objPdfDocument.GetNumberOfPages());
                for (int intPage = 1; intPage <= intMaxPage; intPage++)
                {
                    token.ThrowIfCancellationRequested();
                    // No more patterns to match, exit early
                    if (dicPatternsToMatch.IsEmpty)
                        break;
                    string strText = await GetPageTextFromPDF(objPdfDocument, intPage).ConfigureAwait(false);
                    if (string.IsNullOrEmpty(strText))
                        continue;

                    for (int i = lstKeysToLoop.Count - 1; i >= 0; --i)
                    {
                        token.ThrowIfCancellationRequested();
                        // No more patterns to match, exit early
                        if (dicPatternsToMatch.IsEmpty)
                            break;
                        string strKey = lstKeysToLoop[i];
                        token.ThrowIfCancellationRequested();
                        // We already got a match elsewhere, skip this going forward
                        if (!dicPatternsToMatch.TryGetValue(strKey, out Tuple<string, int> tupValue))
                        {
                            lstKeysToLoop.RemoveAt(i);
                            continue;
                        }
                        token.ThrowIfCancellationRequested();
                        if (!strText.Contains(tupValue.Item1))
                            continue;
                        token.ThrowIfCancellationRequested();
                        lstKeysToLoop.RemoveAt(i);
                        token.ThrowIfCancellationRequested();
                        // We already got a match elsewhere, skip this going forward
                        if (!dicPatternsToMatch.TryRemove(strKey, out _))
                            continue;
                        token.ThrowIfCancellationRequested();
                        int intTrueOffset = intPage - tupValue.Item2;
                        if (lstReturn.Count == 0)
                        {
                            lstReturn.Add(new SourcebookInfo(strPath, objPdfReader, objPdfDocument)
                            {
                                Code = strKey,
                                Offset = intTrueOffset
                            });
                        }
                        else
                        {
                            // Subsequent additions should not get a reader assigned to them because we want separate readers for each sourcebook info
                            lstReturn.Add(new SourcebookInfo
                            {
                                Code = strKey,
                                Offset = intTrueOffset,
                                Path = strPath
                            });
                        }
                    }
                }
            }
            finally
            {
                if (lstReturn.Count == 0)
                {
                    objPdfDocument?.Close();
                    objPdfReader?.Close();
                }
            }

            return lstReturn;

            async Task<string> GetPageTextFromPDF(PdfDocument objInnerPdfDocument, int intPage)
            {
                // Loop through each page, starting at the listed page + offset.
                if (intPage >= objInnerPdfDocument.GetNumberOfPages())
                    return string.Empty;

                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sbdAllLines))
                {
                    try
                    {
                        // each page should have its own text extraction strategy for it to work properly
                        // this way we don't need to check for previous page appearing in the current page
                        // https://stackoverflow.com/questions/35911062/why-are-gettextfrompage-from-itextsharp-returning-longer-and-longer-strings
                        string strPageText = await CommonFunctions
                                                   .GetPdfTextFromPageSafeAsync(objInnerPdfDocument, intPage, token)
                                                   .ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        strPageText = strPageText.CleanStylisticLigatures();
                        token.ThrowIfCancellationRequested();
                        strPageText = strPageText.NormalizeWhiteSpace();
                        token.ThrowIfCancellationRequested();
                        strPageText = strPageText.NormalizeLineEndings();
                        token.ThrowIfCancellationRequested();
                        strPageText = strPageText.CleanOfInvalidUnicodeChars();
                        token.ThrowIfCancellationRequested();
                        // don't trust it to be correct, trim all whitespace and remove empty strings before we even start
                        foreach (string strLine in strPageText.SplitNoAlloc(Environment.NewLine,
                                     StringSplitOptions.RemoveEmptyEntries, StringComparison.OrdinalIgnoreCase))
                        {
                            token.ThrowIfCancellationRequested();
                            if (!string.IsNullOrEmpty(strLine))
                                sbdAllLines.AppendLine(strLine.Trim());
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                        // simply rethrow this
                    }
                    // Need to catch all sorts of exceptions here just in case weird stuff happens in the scanner
                    catch (Exception e)
                    {
                        Utils.BreakIfDebug();
                        Log.Error(e);
                        return string.Empty;
                    }

                    return sbdAllLines.ToString();
                }
            }
        }
    }
}
