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
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
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
    public partial class ExportCharacter : Form, IHasCharacterObjects
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

        public Character CharacterObject => _objCharacter;

        public IEnumerable<Character> CharacterObjects => _objCharacter?.Yield() ?? Enumerable.Empty<Character>();

        #region Control Events

        public ExportCharacter(Character objCharacter)
        {
            _objGenericToken = _objGenericFormClosingCancellationTokenSource.Token;
            _objCharacter = objCharacter;
            Program.MainForm.OpenCharacterExportForms.Add(this);
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
        }

        private async void ExportCharacter_Load(object sender, EventArgs e)
        {
            try
            {
                _objCharacter.PropertyChanged += ObjCharacterOnPropertyChanged;
                _objCharacter.SettingsPropertyChanged += ObjCharacterOnSettingsPropertyChanged;
                await LanguageManager.PopulateSheetLanguageListAsync(cboLanguage, GlobalSettings.DefaultCharacterSheet, _objCharacter.Yield(), _objExportCulture, token: _objGenericToken);
                using (new FetchSafelyFromPool<List<ListItem>>(
                           Utils.ListItemListPool, out List<ListItem> lstExportMethods))
                {
                    // Populate the XSLT list with all of the XSL files found in the sheets directory.
                    foreach (string strFile in Directory.EnumerateFiles(Path.Combine(Utils.GetStartupPath, "export")))
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
                    lstExportMethods.Insert(0, new ListItem("JSON", string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("String_Export_Blank", token: _objGenericToken), "JSON")));

                    await cboXSLT.PopulateWithListItemsAsync(lstExportMethods, _objGenericToken);
                    await cboXSLT.DoThreadSafeAsync(x =>
                    {
                        if (x.Items.Count > 0)
                            x.SelectedIndex = 0;
                    }, _objGenericToken);
                }

                _blnLoading = false;
                await Task.WhenAll(
                    UpdateWindowTitleAsync(_objGenericToken),
                    DoLanguageUpdate(_objGenericToken));
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void ObjCharacterOnSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName == nameof(CharacterSettings.Name))
                    await UpdateWindowTitleAsync(_objGenericToken);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void ObjCharacterOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName == nameof(Character.CharacterName) || e.PropertyName == nameof(Character.Created))
                    await UpdateWindowTitleAsync(_objGenericToken);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void ExportCharacter_FormClosing(object sender, FormClosingEventArgs e)
        {
            CancellationTokenSource objTemp = Interlocked.Exchange(ref _objXmlGeneratorCancellationTokenSource, null);
            if (objTemp?.IsCancellationRequested == false)
            {
                objTemp.Cancel(false);
                objTemp.Dispose();
            }
            objTemp = Interlocked.Exchange(ref _objCharacterXmlGeneratorCancellationTokenSource, null);
            if (objTemp?.IsCancellationRequested == false)
            {
                objTemp.Cancel(false);
                objTemp.Dispose();
            }

            _objCharacter.PropertyChanged -= ObjCharacterOnPropertyChanged;
            _objCharacter.SettingsPropertyChanged -= ObjCharacterOnSettingsPropertyChanged;

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
        
        private async void cmdExport_Click(object sender, EventArgs e)
        {
            try
            {
                await DoExport(_objGenericToken);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void cmdExportClose_Click(object sender, EventArgs e)
        {
            try
            {
                await DoExport(_objGenericToken);
                await this.DoThreadSafeAsync(x =>
                {
                    x.DialogResult = DialogResult.OK;
                    x.Close();
                }, _objGenericToken);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async ValueTask DoExport(CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(_strXslt))
                return;

            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token);
            try
            {
                if (_strXslt == "JSON")
                {
                    await ExportJson(token: token);
                }
                else
                {
                    await ExportNormal(token: token);
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync();
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
                _strExportLanguage = await cboLanguage.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token) ?? GlobalSettings.Language;
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
                                                                   = Math.Min(x.Width, x.Height) >= 32
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
                    _strXslt = await cboXSLT.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token);
                    if (!string.IsNullOrEmpty(_strXslt))
                    {
                        CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token);
                        try
                        {
                            token.ThrowIfCancellationRequested();
                            string strText = await LanguageManager.GetStringAsync("String_Generating_Data", token: token);
                            await txtText.DoThreadSafeAsync(x => x.Text = strText, token);
                            (bool blnSuccess, Tuple<string, string> strBoxText)
                                = await _dicCache.TryGetValueAsync(
                                    new Tuple<string, string>(_strExportLanguage, _strXslt), token);
                            token.ThrowIfCancellationRequested();
                            if (blnSuccess)
                            {
                                await txtText.DoThreadSafeAsync(x => x.Text = strBoxText.Item2, token);
                            }
                            else
                            {
                                CancellationTokenSource objNewSource = new CancellationTokenSource();
                                CancellationTokenSource objTemp
                                    = Interlocked.Exchange(ref _objXmlGeneratorCancellationTokenSource, objNewSource);
                                if (objTemp?.IsCancellationRequested == false)
                                {
                                    objTemp.Cancel(false);
                                    objTemp.Dispose();
                                }

                                try
                                {
                                    token.ThrowIfCancellationRequested();
                                }
                                catch (OperationCanceledException)
                                {
                                    Interlocked.CompareExchange(ref _objXmlGeneratorCancellationTokenSource, null,
                                                                objNewSource);
                                    objNewSource.Dispose();
                                    throw;
                                }

                                try
                                {
                                    if (_tskXmlGenerator?.IsCompleted == false)
                                        await _tskXmlGenerator;
                                }
                                catch (OperationCanceledException)
                                {
                                    // Swallow this
                                }
                                catch
                                {
                                    Interlocked.CompareExchange(ref _objXmlGeneratorCancellationTokenSource, null, objNewSource);
                                    objNewSource.Dispose();
                                    throw;
                                }

                                CancellationToken objToken = objNewSource.Token;
                                _tskXmlGenerator = _strXslt == "JSON"
                                    ? Task.Run(() => GenerateJson(objToken), objToken)
                                    : Task.Run(() => GenerateXml(objToken), objToken);
                            }
                        }
                        finally
                        {
                            await objCursorWait.DisposeAsync();
                        }
                    }
                }
                else
                {
                    token.ThrowIfCancellationRequested();
                    CancellationTokenSource objNewSource = new CancellationTokenSource();
                    CancellationTokenSource objTemp
                        = Interlocked.Exchange(ref _objCharacterXmlGeneratorCancellationTokenSource, objNewSource);
                    if (objTemp?.IsCancellationRequested == false)
                    {
                        objTemp.Cancel(false);
                        objTemp.Dispose();
                    }

                    try
                    {
                        token.ThrowIfCancellationRequested();
                    }
                    catch (OperationCanceledException)
                    {
                        Interlocked.CompareExchange(ref _objCharacterXmlGeneratorCancellationTokenSource, null,
                                                    objNewSource);
                        objNewSource.Dispose();
                        throw;
                    }

                    try
                    {
                        if (_tskCharacterXmlGenerator?.IsCompleted == false)
                            await _tskCharacterXmlGenerator;
                    }
                    catch (OperationCanceledException)
                    {
                        // Swallow this
                    }
                    catch
                    {
                        Interlocked.CompareExchange(ref _objCharacterXmlGeneratorCancellationTokenSource, null, objNewSource);
                        objNewSource.Dispose();
                        throw;
                    }

                    CancellationToken objToken = objNewSource.Token;
                    _tskCharacterXmlGenerator = Task.Run(() => GenerateCharacterXml(objToken), objToken);
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
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token);
            try
            {
                await Task.WhenAll(cmdExport.DoThreadSafeAsync(x => x.Enabled = false, token),
                                   cmdExportClose.DoThreadSafeAsync(x => x.Enabled = false, token),
                                   LanguageManager.GetStringAsync("String_Generating_Data", token: token)
                                                  .ContinueWith(
                                                      y => txtText.DoThreadSafeAsync(x => x.Text = y.Result, token),
                                                      token).Unwrap());
                token.ThrowIfCancellationRequested();
                using (token.Register(() => _objCharacterXmlGeneratorCancellationTokenSource.Cancel(false)))
                    _objCharacterXml = await _objCharacter.GenerateExportXml(_objExportCulture, _strExportLanguage,
                                                                             _objCharacterXmlGeneratorCancellationTokenSource
                                                                                 .Token);
                token.ThrowIfCancellationRequested();
                if (_objCharacterXml != null)
                    await DoXsltUpdate(token);
            }
            finally
            {
                await objCursorWait.DisposeAsync();
            }
        }

        /// <summary>
        /// Update the Window title to show the Character's name and unsaved changes status.
        /// </summary>
        protected async Task UpdateWindowTitleAsync(CancellationToken token = default)
        {
            string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token);
            string strTitle = await LanguageManager.GetStringAsync("Title_ExportCharacter", token: token) + ':' + strSpace
                              + CharacterObject.CharacterName + strSpace + '-' + strSpace
                              + await LanguageManager.GetStringAsync(
                                  CharacterObject.Created ? "Title_CareerMode" : "Title_CreateNewCharacter", token: token) + strSpace
                              + '(' + CharacterObject.Settings.Name + ')';
            await this.DoThreadSafeAsync(x => x.Text = strTitle, token);
        }

        #region XML

        private async ValueTask ExportNormal(string destination = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
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
                        token.ThrowIfCancellationRequested();
                        if (strLine.StartsWith("<!-- ext:", StringComparison.Ordinal))
                            strExtension = strLine.TrimStartOnce("<!-- ext:", true).FastEscapeOnceFromEnd("-->").Trim();
                    }
                }

                string strExtensionLower = strExtension.ToLowerInvariant();
                if (strExtension.Equals("XML", StringComparison.OrdinalIgnoreCase))
                    dlgSaveFile.Filter = await LanguageManager.GetStringAsync("DialogFilter_Xml", token: token);
                else if (strExtension.Equals("JSON", StringComparison.OrdinalIgnoreCase))
                    dlgSaveFile.Filter = await LanguageManager.GetStringAsync("DialogFilter_Json", token: token);
                else if (strExtension.Equals("HTM", StringComparison.OrdinalIgnoreCase) || strExtension.Equals("HTML", StringComparison.OrdinalIgnoreCase))
                    dlgSaveFile.Filter = await LanguageManager.GetStringAsync("DialogFilter_Html", token: token);
                else
                    dlgSaveFile.Filter = strExtension.ToUpper(GlobalSettings.CultureInfo) + "|*." + strExtensionLower;
                dlgSaveFile.Title = await LanguageManager.GetStringAsync("Button_Viewer_SaveAsHtml", token: token);
                if (await this.DoThreadSafeFuncAsync(x => dlgSaveFile.ShowDialog(x), token) != DialogResult.OK)
                    return;
                strSaveFile = dlgSaveFile.FileName;
                if (string.IsNullOrEmpty(strSaveFile))
                    return;
                if (!strSaveFile.EndsWith('.' + strExtensionLower, StringComparison.OrdinalIgnoreCase)
                    && (!strExtensionLower.TrimEndOnce('l').Equals("htm", StringComparison.OrdinalIgnoreCase)
                        || !strSaveFile.TrimEndOnce('l', 'L').EndsWith(".htm", StringComparison.OrdinalIgnoreCase)))
                    strSaveFile += '.' + strExtensionLower;
            }
            if (string.IsNullOrEmpty(strSaveFile))
                return;
            
            (bool blnSuccess, Tuple<string, string> strBoxText)
                = await _dicCache.TryGetValueAsync(new Tuple<string, string>(_strExportLanguage, _strXslt), token);
            File.WriteAllText(strSaveFile, // Change this to a proper path.
                              blnSuccess
                                  ? strBoxText.Item1
                                  : txtText.Text,
                              Encoding.UTF8);
        }

        private async Task GenerateXml(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token);
            try
            {
                await cmdExport.DoThreadSafeAsync(x => x.Enabled = false, token);
                await cmdExportClose.DoThreadSafeAsync(x => x.Enabled = false, token);
                try
                {
                    token.ThrowIfCancellationRequested();
                    string exportSheetPath = Path.Combine(Utils.GetStartupPath, "export", _strXslt + ".xsl");

                    XslCompiledTransform objXslTransform;
                    try
                    {
                        objXslTransform
                            = await XslManager
                                .GetTransformForFileAsync(exportSheetPath, token); // Use the path for the export sheet.
                    }
                    catch (ArgumentException)
                    {
                        token.ThrowIfCancellationRequested();
                        string strReturn = "Last write time could not be fetched when attempting to load " + _strXslt +
                                           Environment.NewLine;
                        Log.Debug(strReturn);
                        await SetTextToWorkerResult(strReturn, token);
                        return;
                    }
                    catch (PathTooLongException)
                    {
                        token.ThrowIfCancellationRequested();
                        string strReturn = "Last write time could not be fetched when attempting to load " + _strXslt +
                                           Environment.NewLine;
                        Log.Debug(strReturn);
                        await SetTextToWorkerResult(strReturn, token);
                        return;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        token.ThrowIfCancellationRequested();
                        string strReturn = "Last write time could not be fetched when attempting to load " + _strXslt +
                                           Environment.NewLine;
                        Log.Debug(strReturn);
                        await SetTextToWorkerResult(strReturn, token);
                        return;
                    }
                    catch (XsltException ex)
                    {
                        token.ThrowIfCancellationRequested();
                        string strReturn = "Error attempting to load " + _strXslt + Environment.NewLine;
                        Log.Debug(strReturn);
                        Log.Error("ERROR Message = " + ex.Message);
                        strReturn += ex.Message;
                        await SetTextToWorkerResult(strReturn, token);
                        return;
                    }

                    token.ThrowIfCancellationRequested();

                    XmlWriterSettings objSettings = objXslTransform.OutputSettings?.Clone();
                    if (objSettings != null)
                    {
                        objSettings.CheckCharacters = false;
                        objSettings.ConformanceLevel = ConformanceLevel.Fragment;
                    }

                    string strText;
                    using (MemoryStream objStream = new MemoryStream())
                    {
                        using (XmlWriter objWriter = objSettings != null
                                   ? XmlWriter.Create(objStream, objSettings)
                                   : Utils.GetXslTransformXmlWriter(objStream))
                        {
                            token.ThrowIfCancellationRequested();
                            await Task.Run(() => objXslTransform.Transform(_objCharacterXml, objWriter), token);
                        }

                        token.ThrowIfCancellationRequested();
                        objStream.Position = 0;

                        // Read in the resulting code and pass it to the browser.
                        using (StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true))
                            strText = await objReader.ReadToEndAsync();
                    }

                    token.ThrowIfCancellationRequested();
                    await SetTextToWorkerResult(strText, token);
                }
                finally
                {
                    await cmdExport.DoThreadSafeAsync(x => x.Enabled = true, token);
                    await cmdExportClose.DoThreadSafeAsync(x => x.Enabled = true, token);
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync();
            }
        }

        private async Task GenerateJson(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token);
            try
            {
                await cmdExport.DoThreadSafeAsync(x => x.Enabled = false, token);
                await cmdExportClose.DoThreadSafeAsync(x => x.Enabled = false, token);
                try
                {
                    token.ThrowIfCancellationRequested();
                    string strText = JsonConvert.SerializeXmlNode(_objCharacterXml, Formatting.Indented);
                    token.ThrowIfCancellationRequested();
                    await SetTextToWorkerResult(strText, token);
                }
                finally
                {
                    await cmdExport.DoThreadSafeAsync(x => x.Enabled = true, token);
                    await cmdExportClose.DoThreadSafeAsync(x => x.Enabled = true, token);
                }
            }
            finally
            {
                await objCursorWait.DisposeAsync();
            }
        }

        private Task SetTextToWorkerResult(string strText, CancellationToken token = default)
        {
            string strDisplayText = strText;
            // Displayed text has all mugshots data removed because it's unreadable as Base64 strings, but massive enough to slow down the program
            strDisplayText = s_RgxMainMugshotReplaceExpression.Replace(strDisplayText, "<mainmugshotbase64>[...]</mainmugshotbase64>");
            strDisplayText = s_RgxStringBase64ReplaceExpression.Replace(strDisplayText, "<stringbase64>[...]</stringbase64>");
            strDisplayText = s_RgxBase64ReplaceExpression.Replace(strDisplayText, "base64\": \"[...]\",");
            _dicCache.AddOrUpdate(new Tuple<string, string>(_strExportLanguage, _strXslt),
                new Tuple<string, string>(strText, strDisplayText),
                (a, b) => new Tuple<string, string>(strText, strDisplayText));
            return txtText.DoThreadSafeAsync(x => x.Text = strDisplayText, token);
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

        private async ValueTask ExportJson(string destination = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strSaveFile = destination;
            if (string.IsNullOrEmpty(strSaveFile))
            {
                dlgSaveFile.Filter = await LanguageManager.GetStringAsync("DialogFilter_Json", token: token) + '|' + await LanguageManager.GetStringAsync("DialogFilter_All", token: token);
                dlgSaveFile.Title = await LanguageManager.GetStringAsync("Button_Export_SaveJsonAs", token: token);
                if (await this.DoThreadSafeFuncAsync(x => dlgSaveFile.ShowDialog(x), token) != DialogResult.OK)
                    return;
                strSaveFile = dlgSaveFile.FileName;
                if (string.IsNullOrWhiteSpace(strSaveFile))
                    return;
                if (!strSaveFile.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    strSaveFile += ".json";
            }
            if (string.IsNullOrWhiteSpace(strSaveFile))
                return;

            (bool blnSuccess, Tuple<string, string> strBoxText)
                = await _dicCache.TryGetValueAsync(new Tuple<string, string>(_strExportLanguage, _strXslt), token);
            File.WriteAllText(strSaveFile, // Change this to a proper path.
                              blnSuccess
                                  ? strBoxText.Item1
                                  : txtText.Text,
                              Encoding.UTF8);
        }

        #endregion JSON

        #endregion Methods
    }
}
