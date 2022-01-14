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
            _objCharacter = objCharacter;
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
        }

        private async void frmExport_Load(object sender, EventArgs e)
        {
            LanguageManager.PopulateSheetLanguageList(cboLanguage, GlobalSettings.DefaultCharacterSheet, _objCharacter.Yield(), _objExportCulture);
            cboXSLT.BeginUpdate();
            cboXSLT.Items.Add("Export JSON");
            // Populate the XSLT list with all of the XSL files found in the sheets directory.
            string exportDirectoryPath = Path.Combine(Utils.GetStartupPath, "export");
            foreach (string strFile in Directory.GetFiles(exportDirectoryPath))
            {
                // Only show files that end in .xsl. Do not include files that end in .xslt since they are used as "hidden" reference sheets (hidden because they are partial templates that cannot be used on their own).
                if (!strFile.EndsWith(".xslt", StringComparison.OrdinalIgnoreCase) && strFile.EndsWith(".xsl", StringComparison.OrdinalIgnoreCase))
                {
                    string strFileName = Path.GetFileNameWithoutExtension(strFile);
                    cboXSLT.Items.Add(strFileName);
                }
            }

            if (cboXSLT.Items.Count > 0)
                cboXSLT.SelectedIndex = 0;
            cboXSLT.EndUpdate();
            _blnLoading = false;
            await Task.WhenAll(this.DoThreadSafeAsync(() => Text = Text + LanguageManager.GetString("String_Space") + _objCharacter?.Name), DoLanguageUpdate());
        }

        private void frmExport_FormClosing(object sender, FormClosingEventArgs e)
        {
            _objXmlGeneratorCancellationTokenSource?.Cancel(false);
            _objCharacterXmlGeneratorCancellationTokenSource?.Cancel(false);
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_strXslt))
                return;

            using (new CursorWait(this))
            {
                if (_strXslt == "Export JSON")
                {
                    ExportJson();
                }
                else
                {
                    ExportNormal();
                }
            }
        }

        private async void cboLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            await DoLanguageUpdate();
        }

        private async void cboXSLT_SelectedIndexChanged(object sender, EventArgs e)
        {
            await DoXsltUpdate();
        }

        private async Task DoLanguageUpdate()
        {
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
                await Task.WhenAll(
                    imgSheetLanguageFlag.DoThreadSafeAsync(() =>
                                                               imgSheetLanguageFlag.Image
                                                                   = Math.Min(imgSheetLanguageFlag.Width,
                                                                              imgSheetLanguageFlag.Height) >= 32
                                                                       ? FlagImageGetter.GetFlagFromCountryCode192Dpi(
                                                                           _strExportLanguage.Substring(3, 2))
                                                                       : FlagImageGetter.GetFlagFromCountryCode(
                                                                           _strExportLanguage.Substring(3, 2))),
                    DoXsltUpdate());
            }
        }

        private async Task DoXsltUpdate()
        {
            if (!_blnLoading)
            {
                if (_objCharacterXml != null)
                {
                    _strXslt = cboXSLT.Text;
                    if (!string.IsNullOrEmpty(_strXslt))
                    {
                        using (new CursorWait(this))
                        {
                            await txtText.DoThreadSafeAsync(
                                () => txtText.Text = LanguageManager.GetString("String_Generating_Data"));
                            if (_dicCache.TryGetValue(new Tuple<string, string>(_strExportLanguage, _strXslt),
                                                      out Tuple<string, string> strBoxText))
                            {
                                await txtText.DoThreadSafeAsync(() => txtText.Text = strBoxText.Item2);
                            }
                            else
                            {
                                if (_objXmlGeneratorCancellationTokenSource != null)
                                {
                                    _objXmlGeneratorCancellationTokenSource.Cancel(false);
                                    _objXmlGeneratorCancellationTokenSource.Dispose();
                                }

                                _objXmlGeneratorCancellationTokenSource = new CancellationTokenSource();
                                try
                                {
                                    if (_tskXmlGenerator?.IsCompleted == false)
                                        await _tskXmlGenerator;
                                }
                                catch (TaskCanceledException)
                                {
                                    // Swallow this
                                }

                                _tskXmlGenerator = _strXslt == "Export JSON"
                                    ? Task.Run(GenerateJson, _objXmlGeneratorCancellationTokenSource.Token)
                                    : Task.Run(GenerateXml, _objXmlGeneratorCancellationTokenSource.Token);
                            }
                        }
                    }
                }
                else
                {
                    if (_objCharacterXmlGeneratorCancellationTokenSource != null)
                    {
                        _objCharacterXmlGeneratorCancellationTokenSource.Cancel(false);
                        _objCharacterXmlGeneratorCancellationTokenSource.Dispose();
                    }

                    _objCharacterXmlGeneratorCancellationTokenSource = new CancellationTokenSource();
                    try
                    {
                        if (_tskCharacterXmlGenerator?.IsCompleted == false)
                            await _tskCharacterXmlGenerator;
                    }
                    catch (TaskCanceledException)
                    {
                        // Swallow this
                    }

                    _tskCharacterXmlGenerator
                        = Task.Run(GenerateCharacterXml, _objCharacterXmlGeneratorCancellationTokenSource.Token);
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

        private async Task GenerateCharacterXml()
        {
            using (new CursorWait(this))
            {
                await Task.WhenAll(cmdOK.DoThreadSafeAsync(() => cmdOK.Enabled = false),
                    txtText.DoThreadSafeAsync(() =>
                        txtText.Text = LanguageManager.GetString("String_Generating_Data")));
                _objCharacterXml = _objCharacter.GenerateExportXml(_objExportCulture, _strExportLanguage,
                    _objCharacterXmlGeneratorCancellationTokenSource.Token);
                if (_objCharacterXmlGeneratorCancellationTokenSource.IsCancellationRequested)
                    return;
                if (_objCharacterXml != null)
                    await DoXsltUpdate();
            }
        }

        #region XML

        private void ExportNormal(string destination = null)
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
                    while ((strLine = objFile.ReadLine()) != null)
                    {
                        if (strLine.StartsWith("<!-- ext:", StringComparison.Ordinal))
                            strExtension = strLine.TrimStartOnce("<!-- ext:", true).FastEscapeOnceFromEnd("-->").Trim();
                    }
                }

                if (strExtension.Equals("XML", StringComparison.OrdinalIgnoreCase))
                    SaveFileDialog1.Filter = LanguageManager.GetString("DialogFilter_Xml");
                else if (strExtension.Equals("JSON", StringComparison.OrdinalIgnoreCase))
                    SaveFileDialog1.Filter = LanguageManager.GetString("DialogFilter_Json");
                else if (strExtension.Equals("HTM", StringComparison.OrdinalIgnoreCase) || strExtension.Equals("HTML", StringComparison.OrdinalIgnoreCase))
                    SaveFileDialog1.Filter = LanguageManager.GetString("DialogFilter_Html");
                else
                    SaveFileDialog1.Filter = strExtension.ToUpper(GlobalSettings.CultureInfo) + "|*." + strExtension.ToLowerInvariant();
                SaveFileDialog1.Title = LanguageManager.GetString("Button_Viewer_SaveAsHtml");
                SaveFileDialog1.ShowDialog();
                strSaveFile = SaveFileDialog1.FileName;
            }
            if (string.IsNullOrEmpty(strSaveFile))
                return;

            File.WriteAllText(strSaveFile, // Change this to a proper path.
                _dicCache.TryGetValue(new Tuple<string, string>(_strExportLanguage, _strXslt), out Tuple<string, string> strBoxText)
                    ? strBoxText.Item1
                    : txtText.Text,
                Encoding.UTF8);

            DialogResult = DialogResult.OK;
        }

        private async Task GenerateXml()
        {
            using (new CursorWait(this))
            {
                await cmdOK.DoThreadSafeAsync(() => cmdOK.Enabled = false);
                try
                {
                    string exportSheetPath = Path.Combine(Utils.GetStartupPath, "export", _strXslt + ".xsl");

                    XslCompiledTransform objXslTransform;
                    try
                    {
                        objXslTransform = XslManager.GetTransformForFile(exportSheetPath); // Use the path for the export sheet.
                    }
                    catch (ArgumentException)
                    {
                        string strReturn = "Last write time could not be fetched when attempting to load " + _strXslt +
                                           Environment.NewLine;
                        Log.Debug(strReturn);
                        SetTextToWorkerResult(strReturn);
                        return;
                    }
                    catch (PathTooLongException)
                    {
                        string strReturn = "Last write time could not be fetched when attempting to load " + _strXslt +
                                           Environment.NewLine;
                        Log.Debug(strReturn);
                        SetTextToWorkerResult(strReturn);
                        return;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        string strReturn = "Last write time could not be fetched when attempting to load " + _strXslt +
                                           Environment.NewLine;
                        Log.Debug(strReturn);
                        SetTextToWorkerResult(strReturn);
                        return;
                    }
                    catch (XsltException ex)
                    {
                        string strReturn = "Error attempting to load " + _strXslt + Environment.NewLine;
                        Log.Debug(strReturn);
                        Log.Error("ERROR Message = " + ex.Message);
                        strReturn += ex.Message;
                        SetTextToWorkerResult(strReturn);
                        return;
                    }

                    if (_objXmlGeneratorCancellationTokenSource.IsCancellationRequested)
                        return;

                    XmlWriterSettings objSettings = objXslTransform.OutputSettings.Clone();
                    objSettings.CheckCharacters = false;
                    objSettings.ConformanceLevel = ConformanceLevel.Fragment;

                    string strText;
                    using (MemoryStream objStream = new MemoryStream())
                    {
                        using (XmlWriter objWriter = XmlWriter.Create(objStream, objSettings))
                            objXslTransform.Transform(_objCharacterXml, null, objWriter);
                        if (_objXmlGeneratorCancellationTokenSource.IsCancellationRequested)
                            return;
                        objStream.Position = 0;

                        // Read in the resulting code and pass it to the browser.
                        using (StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true))
                            strText = await objReader.ReadToEndAsync();
                    }

                    SetTextToWorkerResult(strText);
                }
                finally
                {
                    await cmdOK.DoThreadSafeAsync(() => cmdOK.Enabled = true);
                }
            }
        }

        private async Task GenerateJson()
        {
            using (new CursorWait(this))
            {
                await cmdOK.DoThreadSafeAsync(() => cmdOK.Enabled = false);
                try
                {
                    string strText = JsonConvert.SerializeXmlNode(_objCharacterXml, Formatting.Indented);
                    if (_objXmlGeneratorCancellationTokenSource.IsCancellationRequested)
                        return;
                    SetTextToWorkerResult(strText);
                }
                finally
                {
                    await cmdOK.DoThreadSafeAsync(() => cmdOK.Enabled = true);
                }
            }
        }

        private void SetTextToWorkerResult(string strText)
        {
            string strDisplayText = strText;
            // Displayed text has all mugshots data removed because it's unreadable as Base64 strings, but massie enough to slow down the program
            strDisplayText = Regex.Replace(strDisplayText, "<mainmugshotbase64>[^\\s\\S]*</mainmugshotbase64>", "<mainmugshotbase64>[...]</mainmugshotbase64>");
            strDisplayText = Regex.Replace(strDisplayText, "<stringbase64>[^\\s\\S]*</stringbase64>", "<stringbase64>[...]</stringbase64>");
            strDisplayText = Regex.Replace(strDisplayText, "base64\": \"[^\\\"]*\",", "base64\": \"[...]\",");
            _dicCache.AddOrUpdate(new Tuple<string, string>(_strExportLanguage, _strXslt),
                new Tuple<string, string>(strText, strDisplayText),
                (a, b) => new Tuple<string, string>(strText, strDisplayText));
            txtText.DoThreadSafe(() => txtText.Text = strDisplayText);
        }

        #endregion XML

        #region JSON

        private void ExportJson(string destination = null)
        {
            string strSaveFile = destination;
            if (string.IsNullOrEmpty(strSaveFile))
            {
                SaveFileDialog1.AddExtension = true;
                SaveFileDialog1.DefaultExt = "json";
                SaveFileDialog1.Filter = LanguageManager.GetString("DialogFilter_Json") + '|' + LanguageManager.GetString("DialogFilter_All");
                SaveFileDialog1.Title = LanguageManager.GetString("Button_Export_SaveJsonAs");
                SaveFileDialog1.ShowDialog();
                strSaveFile = SaveFileDialog1.FileName;
            }
            if (string.IsNullOrWhiteSpace(strSaveFile))
                return;

            File.WriteAllText(strSaveFile, // Change this to a proper path.
                _dicCache.TryGetValue(new Tuple<string, string>(_strExportLanguage, _strXslt), out Tuple<string, string> strBoxText)
                    ? strBoxText.Item1
                    : txtText.Text,
                Encoding.UTF8);

            DialogResult = DialogResult.OK;
        }

        #endregion JSON

        #endregion Methods
    }
}
