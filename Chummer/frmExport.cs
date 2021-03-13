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
 using System.ComponentModel;
 using System.Globalization;
 using System.IO;
 using System.Text;
 using System.Text.RegularExpressions;
 using System.Windows.Forms;
using System.Xml;
using System.Xml.Xsl;
 using Newtonsoft.Json;
 using Formatting = Newtonsoft.Json.Formatting;

namespace Chummer
{
    public partial class frmExport : Form
    {
        private readonly Character _objCharacter;
        private readonly ConcurrentDictionary<Tuple<string, string>, Tuple<string, string>> _dicCache = new ConcurrentDictionary<Tuple<string, string>, Tuple<string, string>>();
        private readonly BackgroundWorker _workerJsonLoader = new BackgroundWorker();
        private readonly BackgroundWorker _workerXmlLoader = new BackgroundWorker();
        private readonly BackgroundWorker _workerXmlGenerator = new BackgroundWorker();
        private XmlDocument _objCharacterXml;
        private bool _blnSelected;
        private string _strXslt;
        private string _strExportLanguage;
        private CultureInfo _objExportCulture;
        private bool _blnLoading = true;

        #region Control Events
        public frmExport(Character objCharacter)
        {
            _objCharacter = objCharacter;
            _workerJsonLoader.WorkerSupportsCancellation = true;
            _workerJsonLoader.WorkerReportsProgress = false;
            _workerJsonLoader.DoWork += GenerateJson;
            _workerJsonLoader.RunWorkerCompleted += SetTextToWorkerResult;
            _workerXmlLoader.WorkerSupportsCancellation = true;
            _workerXmlLoader.WorkerReportsProgress = false;
            _workerXmlLoader.DoWork += GenerateXml;
            _workerXmlLoader.RunWorkerCompleted += SetTextToWorkerResult;
            _workerXmlGenerator.WorkerSupportsCancellation = true;
            _workerXmlGenerator.WorkerReportsProgress = false;
            _workerXmlGenerator.DoWork += GenerateCharacterXml;
            _workerXmlGenerator.RunWorkerCompleted += FinalizeCharacterXml;
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
        }

        private void frmExport_Load(object sender, EventArgs e)
        {
            LanguageManager.PopulateSheetLanguageList(cboLanguage, GlobalOptions.DefaultCharacterSheet, _objCharacter.Yield());
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
            cboLanguage_SelectedIndexChanged(sender, e);
        }

        private void frmExport_FormClosing(object sender, FormClosingEventArgs e)
        {
            _workerJsonLoader.CancelAsync();
            _workerXmlLoader.CancelAsync();
            _workerXmlGenerator.CancelAsync();
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

        private void cboLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            _strExportLanguage = cboLanguage.SelectedValue?.ToString() ?? GlobalOptions.Language;
            imgSheetLanguageFlag.Image = FlagImageGetter.GetFlagFromCountryCode(_strExportLanguage.Substring(3, 2));
            try
            {
                _objExportCulture = CultureInfo.GetCultureInfo(_strExportLanguage);
            }
            catch (CultureNotFoundException)
            {
            }

            _objCharacterXml = null;
            cboXSLT_SelectedIndexChanged(sender, e);
        }

        private void cboXSLT_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            if (_objCharacterXml == null)
            {
                UseWaitCursor = true;
                cmdOK.Enabled = false;
                txtText.Text = LanguageManager.GetString("String_Generating_Data");
                _workerXmlGenerator.RunWorkerAsync();
                return;
            }
            _strXslt = cboXSLT.Text;
            if (string.IsNullOrEmpty(_strXslt))
                return;

            UseWaitCursor = true;
            cmdOK.Enabled = false;
            txtText.Text = LanguageManager.GetString("String_Generating_Data");
            if (_dicCache.TryGetValue(new Tuple<string, string>(_strExportLanguage, _strXslt), out Tuple<string, string> tstrBoxText))
            {
                txtText.Text = tstrBoxText.Item2;
                cmdOK.Enabled = true;
                UseWaitCursor = false;
            }
            else
            {
                if (_strXslt == "Export JSON")
                {
                    _workerJsonLoader.RunWorkerAsync();
                }
                else
                {
                    _workerXmlLoader.RunWorkerAsync();
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

        #endregion

        #region Methods
        private void GenerateCharacterXml(object sender, DoWorkEventArgs e)
        {
            XmlDocument objCharacterXml = new XmlDocument
            {
                XmlResolver = null
            };
            // Write the Character information to a MemoryStream so we don't need to create any files.
            MemoryStream objStream = new MemoryStream();
            using (XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8))
            {
                // Being the document.
                objWriter.WriteStartDocument();

                // </characters>
                objWriter.WriteStartElement("characters");

#if DEBUG
                _objCharacter.PrintToStream(objStream, objWriter, _objExportCulture, _strExportLanguage);
#else
                _objCharacter.PrintToStream(objWriter, _objExportCulture, _strExportLanguage);
#endif

                // </characters>
                objWriter.WriteEndElement();

                if (e.Cancel)
                    return;

                // Finish the document and flush the Writer and Stream.
                objWriter.WriteEndDocument();
                objWriter.Flush();

                if (e.Cancel)
                    return;

                // Read the stream.
                objStream.Position = 0;
                using (StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true))
                    using (XmlReader objXmlReader = XmlReader.Create(objReader, GlobalOptions.SafeXmlReaderSettings))
                        objCharacterXml.Load(objXmlReader);
            }
            e.Result = objCharacterXml;
        }

        private void FinalizeCharacterXml(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                cmdOK.Enabled = true;
                UseWaitCursor = false;
                return;
            }

            _objCharacterXml = e.Result as XmlDocument;
            if (_objCharacterXml != null)
                cboXSLT_SelectedIndexChanged(this, EventArgs.Empty);
            else
            {
                cmdOK.Enabled = true;
                UseWaitCursor = false;
            }
        }

        #region XML
        private void ExportNormal()
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
                SaveFileDialog1.Filter = strExtension.ToUpper(GlobalOptions.CultureInfo) + "|*." + strExtension.ToLowerInvariant();
            SaveFileDialog1.Title = LanguageManager.GetString("Button_Viewer_SaveAsHtml");
            SaveFileDialog1.ShowDialog();
            string strSaveFile = SaveFileDialog1.FileName;

            if (string.IsNullOrEmpty(strSaveFile))
                return;

            File.WriteAllText(strSaveFile, // Change this to a proper path.
                _dicCache.TryGetValue(new Tuple<string, string>(_strExportLanguage, _strXslt), out Tuple<string, string> tstrBoxText)
                    ? tstrBoxText.Item1
                    : txtText.Text,
                Encoding.UTF8);

            DialogResult = DialogResult.OK;
        }

        private void GenerateXml(object sender, DoWorkEventArgs e)
        {
            string exportSheetPath = Path.Combine(Utils.GetStartupPath, "export", _strXslt + ".xsl");

            XslCompiledTransform objXSLTransform = new XslCompiledTransform();
            objXSLTransform.Load(exportSheetPath); // Use the path for the export sheet.
            if (e.Cancel)
                return;
            XmlWriterSettings objSettings = objXSLTransform.OutputSettings.Clone();
            objSettings.CheckCharacters = false;
            objSettings.ConformanceLevel = ConformanceLevel.Fragment;

            using (MemoryStream objStream = new MemoryStream())
            {
                using (XmlWriter objWriter = XmlWriter.Create(objStream, objSettings))
                    objXSLTransform.Transform(_objCharacterXml, null, objWriter);
                if (e.Cancel)
                    return;
                objStream.Position = 0;

                // Read in the resulting code and pass it to the browser.
                using (StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true))
                    e.Result = objReader.ReadToEnd();
            }
        }

        private void GenerateJson(object sender, DoWorkEventArgs e)
        {
            e.Result = JsonConvert.SerializeXmlNode(_objCharacterXml, Formatting.Indented);
        }

        private void SetTextToWorkerResult(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                cmdOK.Enabled = true;
                UseWaitCursor = false;
                return;
            }
            string strText = e.Result.ToString();
            string strDisplayText = strText;
            // Displayed text has all mugshots data removed because it's unreadable as Base64 strings, but massie enough to slow down the program
            strDisplayText = Regex.Replace(strDisplayText, "<mainmugshotbase64>[^\\s\\S]*</mainmugshotbase64>", "<mainmugshotbase64>[...]</mainmugshotbase64>");
            strDisplayText = Regex.Replace(strDisplayText, "<stringbase64>[^\\s\\S]*</stringbase64>", "<stringbase64>[...]</stringbase64>");
            strDisplayText = Regex.Replace(strDisplayText, "base64\": \"[^\\\"]*\",", "base64\": \"[...]\",");
            _dicCache.AddOrUpdate(new Tuple<string, string>(_strExportLanguage, _strXslt), x => new Tuple<string, string>(strText, strDisplayText), (a, b) => new Tuple<string, string>(strText, strDisplayText));
            txtText.Text = strDisplayText;
            cmdOK.Enabled = true;
            UseWaitCursor = false;
        }
        #endregion
        #region JSON

        private void ExportJson()
        {
            SaveFileDialog1.AddExtension = true;
            SaveFileDialog1.DefaultExt = "json";
            SaveFileDialog1.Filter = LanguageManager.GetString("DialogFilter_Json") + '|' + LanguageManager.GetString("DialogFilter_All");
            SaveFileDialog1.Title = LanguageManager.GetString("Button_Export_SaveJsonAs");
            SaveFileDialog1.ShowDialog();
            string strSaveFile = SaveFileDialog1.FileName;
            
            if (string.IsNullOrWhiteSpace(strSaveFile))
                return;

            File.WriteAllText(strSaveFile, // Change this to a proper path.
                _dicCache.TryGetValue(new Tuple<string, string>(_strExportLanguage, _strXslt), out Tuple<string, string> tstrBoxText)
                    ? tstrBoxText.Item1
                    : txtText.Text,
                Encoding.UTF8);

            DialogResult = DialogResult.OK;
        }
        #endregion

        #endregion
    }
}
