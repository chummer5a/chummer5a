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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Xsl;
using Newtonsoft.Json;
using NLog;
using Formatting = Newtonsoft.Json.Formatting;

namespace Chummer
{
    public partial class ExportCharacter : Form
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();
        private readonly Character _objCharacter;
        private readonly LockingDictionary<Tuple<string, string>, Tuple<string, string>> _dicCache = new LockingDictionary<Tuple<string, string>, Tuple<string, string>>();
        private CancellationTokenSource _objCharacterXmlGeneratorCancellationTokenSource;
        private CancellationTokenSource _objXmlGeneratorCancellationTokenSource;
        private readonly CancellationTokenSource _objGenericFormClosingCancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _objGenericToken;
        private Task _tskCharacterXmlGenerator;
        private Task _tskXmlGenerator;
        private XmlDocument _objCharacterXml;
        private bool _blnSelected;
        private string _strXslt;
        private string _strExportLanguage;
        private CultureInfo _objExportCulture;
        private bool _blnLoading = true;

        #region Control Events

        public ExportCharacter(Character objCharacter)
        {
            _objGenericToken = _objGenericFormClosingCancellationTokenSource.Token;
            _objCharacter = objCharacter;
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
        }

        private async void ExportCharacter_Load(object sender, EventArgs e)
        {
            await LanguageManager.PopulateSheetLanguageListAsync(cboLanguage, GlobalSettings.DefaultCharacterSheet, _objCharacter.Yield(), _objExportCulture);
            using (new FetchSafelyFromPool<List<ListItem>>(
                       Utils.ListItemListPool, out List<ListItem> lstExportMethods))
            {
                // Populate the XSLT list with all of the XSL files found in the sheets directory.
                string exportDirectoryPath = Path.Combine(Utils.GetStartupPath, "export");
                foreach (string strFile in Directory.GetFiles(exportDirectoryPath))
                {
                    // Only show files that end in .xsl. Do not include files that end in .xslt since they are used as "hidden" reference sheets (hidden because they are partial templates that cannot be used on their own).
                    if (!strFile.EndsWith(".xslt", StringComparison.OrdinalIgnoreCase)
                        && strFile.EndsWith(".xsl", StringComparison.OrdinalIgnoreCase))
                    {
                        string strFileName = Path.GetFileNameWithoutExtension(strFile);
                        lstExportMethods.Add(new ListItem(strFileName, strFileName));
                    }
                }
                lstExportMethods.Sort();
                lstExportMethods.Insert(0, new ListItem("JSON", await LanguageManager.GetStringAsync("String_Export_JSON")));

                await cboXSLT.PopulateWithListItemsAsync(lstExportMethods);
                if (cboXSLT.Items.Count > 0)
                    cboXSLT.SelectedIndex = 0;
            }

            _blnLoading = false;
            string strText = await LanguageManager.GetStringAsync("String_Space") + _objCharacter?.Name;
            try
            {
                await Task.WhenAll(
                    this.DoThreadSafeAsync(x => x.Text += strText, _objGenericToken),
                    DoLanguageUpdate(_objGenericToken));
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void ExportCharacter_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_objXmlGeneratorCancellationTokenSource?.IsCancellationRequested == false)
            {
                _objXmlGeneratorCancellationTokenSource.Cancel(false);
                _objXmlGeneratorCancellationTokenSource.Dispose();
                _objXmlGeneratorCancellationTokenSource = null;
            }
            if (_objCharacterXmlGeneratorCancellationTokenSource?.IsCancellationRequested == false)
            {
                _objCharacterXmlGeneratorCancellationTokenSource.Cancel(false);
                _objCharacterXmlGeneratorCancellationTokenSource.Dispose();
                _objCharacterXmlGeneratorCancellationTokenSource = null;
            }
            try
            {
                await _tskXmlGenerator;
            }
            catch (OperationCanceledException)
            {
                // Swallow this
            }
            try
            {
                await _tskCharacterXmlGenerator;
            }
            catch (OperationCanceledException)
            {
                // Swallow this
            }
            _objGenericFormClosingCancellationTokenSource?.Cancel(false);
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_strXslt))
                return;

            using (CursorWait.New(this))
            {
                if (_strXslt == "JSON")
                {
                    await ExportJson();
                }
                else
                {
                    await ExportNormal();
                }
            }
        }

        private async void cboLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                await DoLanguageUpdate(_objGenericToken);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void cboXSLT_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                await DoXsltUpdate(_objGenericToken);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async Task DoLanguageUpdate(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!_blnLoading)
            {
                _strExportLanguage = cboLanguage.SelectedValue?.ToString() ?? GlobalSettings.Language;
                try
                {
                    _objExportCulture = CultureInfo.GetCultureInfo(_strExportLanguage);
                }
                catch (CultureNotFoundException)
                {
                    // Swallow this
                }

                _objCharacterXml = null;
                token.ThrowIfCancellationRequested();
                await Task.WhenAll(
                    imgSheetLanguageFlag.DoThreadSafeAsync(x =>
                                                               x.Image
                                                                   = Math.Min(imgSheetLanguageFlag.Width,
                                                                              imgSheetLanguageFlag.Height) >= 32
                                                                       ? FlagImageGetter.GetFlagFromCountryCode192Dpi(
                                                                           _strExportLanguage.Substring(3, 2))
                                                                       : FlagImageGetter.GetFlagFromCountryCode(
                                                                           _strExportLanguage.Substring(3, 2)), token),
                    DoXsltUpdate(token));
                token.ThrowIfCancellationRequested();
            }
        }

        private async Task DoXsltUpdate(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!_blnLoading)
            {
                if (_objCharacterXml != null)
                {
                    _strXslt = cboXSLT.SelectedValue?.ToString();
                    if (!string.IsNullOrEmpty(_strXslt))
                    {
                        using (CursorWait.New(this))
                        {
                            token.ThrowIfCancellationRequested();
                            string strText = await LanguageManager.GetStringAsync("String_Generating_Data");
                            await txtText.DoThreadSafeAsync(x => x.Text = strText, token);
                            (bool blnSuccess, Tuple<string, string> strBoxText)
                                = await _dicCache.TryGetValueAsync(
                                    new Tuple<string, string>(_strExportLanguage, _strXslt));
                            token.ThrowIfCancellationRequested();
                            if (blnSuccess)
                            {
                                await txtText.DoThreadSafeAsync(x => x.Text = strBoxText.Item2, token);
                            }
                            else
                            {
                                if (_objXmlGeneratorCancellationTokenSource?.IsCancellationRequested == false)
                                {
                                    _objXmlGeneratorCancellationTokenSource.Cancel(false);
                                    _objXmlGeneratorCancellationTokenSource.Dispose();
                                }
                                token.ThrowIfCancellationRequested();
                                _objXmlGeneratorCancellationTokenSource = new CancellationTokenSource();
                                try
                                {
                                    if (_tskXmlGenerator?.IsCompleted == false)
                                        await _tskXmlGenerator;
                                }
                                catch (OperationCanceledException)
                                {
                                    // Swallow this
                                }

                                _tskXmlGenerator = _strXslt == "JSON"
                                    ? Task.Run(() => GenerateJson(_objCharacterXmlGeneratorCancellationTokenSource.Token), _objXmlGeneratorCancellationTokenSource.Token)
                                    : Task.Run(() => GenerateXml(_objCharacterXmlGeneratorCancellationTokenSource.Token), _objXmlGeneratorCancellationTokenSource.Token);
                            }
                        }
                    }
                }
                else
                {
                    token.ThrowIfCancellationRequested();
                    if (_objCharacterXmlGeneratorCancellationTokenSource?.IsCancellationRequested == false)
                    {
                        _objCharacterXmlGeneratorCancellationTokenSource.Cancel(false);
                        _objCharacterXmlGeneratorCancellationTokenSource.Dispose();
                    }
                    token.ThrowIfCancellationRequested();
                    _objCharacterXmlGeneratorCancellationTokenSource = new CancellationTokenSource();
                    try
                    {
                        if (_tskCharacterXmlGenerator?.IsCompleted == false)
                            await _tskCharacterXmlGenerator;
                    }
                    catch (OperationCanceledException)
                    {
                        // Swallow this
                    }

                    _tskCharacterXmlGenerator
                        = Task.Run(() => GenerateCharacterXml(_objCharacterXmlGeneratorCancellationTokenSource.Token), _objCharacterXmlGeneratorCancellationTokenSource.Token);
                }
            }
        }

        private void txtText_Leave(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            _blnSelected = false;
        }

        private void txtText_MouseUp(object sender, MouseEventArgs e)
        {
            if (_blnLoading || _blnSelected || txtText.SelectionLength != 0)
                return;
            _blnSelected = true;
            txtText.SelectAll();
        }

        #endregion Control Events

        #region Methods

        private async Task GenerateCharacterXml(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (CursorWait.New(this))
            {
                string strText = await LanguageManager.GetStringAsync("String_Generating_Data");
                await Task.WhenAll(cmdOK.DoThreadSafeAsync(x => x.Enabled = false, token),
                                   txtText.DoThreadSafeAsync(x => x.Text = strText, token));
                token.ThrowIfCancellationRequested();
                _objCharacterXml = await _objCharacter.GenerateExportXml(_objExportCulture, _strExportLanguage,
                                                                         _objCharacterXmlGeneratorCancellationTokenSource
                                                                             .Token);
                token.ThrowIfCancellationRequested();
                if (_objCharacterXml != null)
                    await DoXsltUpdate(token);
            }
        }

        #region XML

        private async ValueTask ExportNormal(string destination = null)
        {
            string strSaveFile = destination;
            if (string.IsNullOrEmpty(destination))
            {
                // Look for the file extension information.
                string strExtension = "xml";
                string exportSheetPath = Path.Combine(Utils.GetStartupPath, "export", _strXslt + ".xsl");
                using (StreamReader objFile = new StreamReader(exportSheetPath, Encoding.UTF8, true))
                {
                    string strLine;
                    while ((strLine = await objFile.ReadLineAsync()) != null)
                    {
                        if (strLine.StartsWith("<!-- ext:", StringComparison.Ordinal))
                            strExtension = strLine.TrimStartOnce("<!-- ext:", true).FastEscapeOnceFromEnd("-->").Trim();
                    }
                }

                if (strExtension.Equals("XML", StringComparison.OrdinalIgnoreCase))
                    SaveFileDialog1.Filter = await LanguageManager.GetStringAsync("DialogFilter_Xml");
                else if (strExtension.Equals("JSON", StringComparison.OrdinalIgnoreCase))
                    SaveFileDialog1.Filter = await LanguageManager.GetStringAsync("DialogFilter_Json");
                else if (strExtension.Equals("HTM", StringComparison.OrdinalIgnoreCase) || strExtension.Equals("HTML", StringComparison.OrdinalIgnoreCase))
                    SaveFileDialog1.Filter = await LanguageManager.GetStringAsync("DialogFilter_Html");
                else
                    SaveFileDialog1.Filter = strExtension.ToUpper(GlobalSettings.CultureInfo) + "|*." + strExtension.ToLowerInvariant();
                SaveFileDialog1.Title = await LanguageManager.GetStringAsync("Button_Viewer_SaveAsHtml");
                SaveFileDialog1.ShowDialog();
                strSaveFile = SaveFileDialog1.FileName;
            }
            if (string.IsNullOrEmpty(strSaveFile))
                return;
            
            (bool blnSuccess, Tuple<string, string> strBoxText)
                = await _dicCache.TryGetValueAsync(new Tuple<string, string>(_strExportLanguage, _strXslt));
            File.WriteAllText(strSaveFile, // Change this to a proper path.
                              blnSuccess
                                  ? strBoxText.Item1
                                  : txtText.Text,
                              Encoding.UTF8);

            DialogResult = DialogResult.OK;
        }

        private async Task GenerateXml(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (CursorWait.New(this))
            {
                await cmdOK.DoThreadSafeAsync(x => x.Enabled = false, token);
                try
                {
                    token.ThrowIfCancellationRequested();
                    string exportSheetPath = Path.Combine(Utils.GetStartupPath, "export", _strXslt + ".xsl");

                    XslCompiledTransform objXslTransform;
                    try
                    {
                        objXslTransform
                            = await XslManager
                                .GetTransformForFileAsync(exportSheetPath); // Use the path for the export sheet.
                    }
                    catch (ArgumentException)
                    {
                        token.ThrowIfCancellationRequested();
                        string strReturn = "Last write time could not be fetched when attempting to load " + _strXslt +
                                           Environment.NewLine;
                        Log.Debug(strReturn);
                        SetTextToWorkerResult(strReturn);
                        return;
                    }
                    catch (PathTooLongException)
                    {
                        token.ThrowIfCancellationRequested();
                        string strReturn = "Last write time could not be fetched when attempting to load " + _strXslt +
                                           Environment.NewLine;
                        Log.Debug(strReturn);
                        SetTextToWorkerResult(strReturn);
                        return;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        token.ThrowIfCancellationRequested();
                        string strReturn = "Last write time could not be fetched when attempting to load " + _strXslt +
                                           Environment.NewLine;
                        Log.Debug(strReturn);
                        SetTextToWorkerResult(strReturn);
                        return;
                    }
                    catch (XsltException ex)
                    {
                        token.ThrowIfCancellationRequested();
                        string strReturn = "Error attempting to load " + _strXslt + Environment.NewLine;
                        Log.Debug(strReturn);
                        Log.Error("ERROR Message = " + ex.Message);
                        strReturn += ex.Message;
                        SetTextToWorkerResult(strReturn);
                        return;
                    }

                    token.ThrowIfCancellationRequested();

                    XmlWriterSettings objSettings = objXslTransform.OutputSettings.Clone();
                    objSettings.CheckCharacters = false;
                    objSettings.ConformanceLevel = ConformanceLevel.Fragment;

                    string strText;
                    using (MemoryStream objStream = new MemoryStream())
                    {
                        using (XmlWriter objWriter = XmlWriter.Create(objStream, objSettings))
                            objXslTransform.Transform(_objCharacterXml, null, objWriter);
                        token.ThrowIfCancellationRequested();
                        objStream.Position = 0;

                        // Read in the resulting code and pass it to the browser.
                        using (StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true))
                            strText = await objReader.ReadToEndAsync();
                    }
                    token.ThrowIfCancellationRequested();
                    SetTextToWorkerResult(strText);
                }
                finally
                {
                    await cmdOK.DoThreadSafeAsync(x => x.Enabled = true, token);
                }
            }
        }

        private async Task GenerateJson(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (CursorWait.New(this))
            {
                await cmdOK.DoThreadSafeAsync(x => x.Enabled = false, token);
                try
                {
                    token.ThrowIfCancellationRequested();
                    string strText = JsonConvert.SerializeXmlNode(_objCharacterXml, Formatting.Indented);
                    token.ThrowIfCancellationRequested();
                    SetTextToWorkerResult(strText);
                }
                finally
                {
                    await cmdOK.DoThreadSafeAsync(x => x.Enabled = true, token);
                }
            }
        }

        private void SetTextToWorkerResult(string strText)
        {
            string strDisplayText = strText;
            // Displayed text has all mugshots data removed because it's unreadable as Base64 strings, but massive enough to slow down the program
            strDisplayText = s_RgxMainMugshotReplaceExpression.Replace(strDisplayText, "<mainmugshotbase64>[...]</mainmugshotbase64>");
            strDisplayText = s_RgxStringBase64ReplaceExpression.Replace(strDisplayText, "<stringbase64>[...]</stringbase64>");
            strDisplayText = s_RgxBase64ReplaceExpression.Replace(strDisplayText, "base64\": \"[...]\",");
            _dicCache.AddOrUpdate(new Tuple<string, string>(_strExportLanguage, _strXslt),
                new Tuple<string, string>(strText, strDisplayText),
                (a, b) => new Tuple<string, string>(strText, strDisplayText));
            txtText.DoThreadSafe(x => x.Text = strDisplayText);
        }

        private static readonly Regex s_RgxMainMugshotReplaceExpression = new Regex(
            "<mainmugshotbase64>[^\\s\\S]*</mainmugshotbase64>",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex s_RgxStringBase64ReplaceExpression = new Regex(
            "<mainmugshotbase64>[^\\s\\S]*</mainmugshotbase64>",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex s_RgxBase64ReplaceExpression = new Regex(
            "base64\": \"[^\\\"]*\",",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        #endregion XML

        #region JSON

        private async ValueTask ExportJson(string destination = null)
        {
            string strSaveFile = destination;
            if (string.IsNullOrEmpty(strSaveFile))
            {
                SaveFileDialog1.AddExtension = true;
                SaveFileDialog1.DefaultExt = "json";
                SaveFileDialog1.Filter = await LanguageManager.GetStringAsync("DialogFilter_Json") + '|' + await LanguageManager.GetStringAsync("DialogFilter_All");
                SaveFileDialog1.Title = await LanguageManager.GetStringAsync("Button_Export_SaveJsonAs");
                SaveFileDialog1.ShowDialog();
                strSaveFile = SaveFileDialog1.FileName;
            }
            if (string.IsNullOrWhiteSpace(strSaveFile))
                return;

            (bool blnSuccess, Tuple<string, string> strBoxText)
                = await _dicCache.TryGetValueAsync(new Tuple<string, string>(_strExportLanguage, _strXslt));
            File.WriteAllText(strSaveFile, // Change this to a proper path.
                              blnSuccess
                                  ? strBoxText.Item1
                                  : txtText.Text,
                              Encoding.UTF8);

            DialogResult = DialogResult.OK;
        }

        #endregion JSON

        #endregion Methods
    }
}
