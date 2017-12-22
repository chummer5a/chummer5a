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
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using System.ComponentModel;
 using System.Linq;
using Codaxy.WkHtmlToPdf;
using System.Diagnostics;
using System.Globalization;

namespace Chummer
{
    public partial class frmViewer : Form
    {
        private List<Character> _lstCharacters = new List<Character>();
        private XmlDocument _objCharacterXML = new XmlDocument();
        private string _strSelectedSheet = GlobalOptions.DefaultCharacterSheet;
        private bool _blnLoading = false;
        private CultureInfo _objPrintCulture = GlobalOptions.CultureInfo;
        private readonly BackgroundWorker _workerRefresher = new BackgroundWorker();
        private readonly BackgroundWorker _workerOutputGenerator = new BackgroundWorker();

        #region Control Events
        public frmViewer()
        {
            _workerRefresher.WorkerSupportsCancellation = true;
            _workerRefresher.WorkerReportsProgress = false;
            _workerRefresher.DoWork += AsyncRefresh;
            _workerRefresher.RunWorkerCompleted += FinishRefresh;
            _workerOutputGenerator.WorkerSupportsCancellation = true;
            _workerOutputGenerator.WorkerReportsProgress = false;
            _workerOutputGenerator.DoWork += AsyncGenerateOutput;
            _workerOutputGenerator.RunWorkerCompleted += FinishGenerateOutput;
            if (_strSelectedSheet.StartsWith("Shadowrun 4"))
            {
                _strSelectedSheet = GlobalOptions.DefaultCharacterSheetDefaultValue;
            }
            if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
            {
                if (!_strSelectedSheet.Contains(Path.DirectorySeparatorChar))
                    _strSelectedSheet = Path.Combine(GlobalOptions.Language, _strSelectedSheet);
                else if (!_strSelectedSheet.Contains(GlobalOptions.Language) && _strSelectedSheet.Contains(GlobalOptions.Language.Substring(0, 2)))
                {
                    _strSelectedSheet = _strSelectedSheet.Replace(GlobalOptions.Language.Substring(0, 2), GlobalOptions.Language);
                }
            }
            else
            {
                int intLastIndexDirectorySeparator = _strSelectedSheet.LastIndexOf(Path.DirectorySeparatorChar);
                if (intLastIndexDirectorySeparator != -1)
                    _strSelectedSheet = _strSelectedSheet.Substring(intLastIndexDirectorySeparator + 1);
            }

            Microsoft.Win32.RegistryKey objRegistry = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION");
            objRegistry.SetValue(AppDomain.CurrentDomain.FriendlyName, 0x1F40, Microsoft.Win32.RegistryValueKind.DWord);
            objRegistry = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software\\WOW6432Node\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION");
            objRegistry.SetValue(AppDomain.CurrentDomain.FriendlyName, 0x1F40, Microsoft.Win32.RegistryValueKind.DWord);

            InitializeComponent();
            LanguageManager.Load(GlobalOptions.Language, this);
            ContextMenuStrip[] lstCMSToTranslate = new ContextMenuStrip[]
            {
                cmsPrintButton,
                cmsSaveButton
            };
            foreach (ContextMenuStrip objCMS in lstCMSToTranslate)
            {
                if (objCMS != null)
                {
                    foreach (ToolStripMenuItem objItem in objCMS.Items.OfType<ToolStripMenuItem>())
                    {
                        if (objItem.Tag != null)
                        {
                            objItem.Text = LanguageManager.GetString(objItem.Tag.ToString());
                        }
                    }
                }
            }
            MoveControls();
        }

        private void frmViewer_Load(object sender, EventArgs e)
        {
            _blnLoading = true;
            // Populate the XSLT list with all of the XSL files found in the sheets directory.
            PopulateLanguageList();
            PopulateXsltList();

            cboXSLT.SelectedValue = _strSelectedSheet;
            // If the desired sheet was not found, fall back to the Shadowrun 5 sheet.
            if (cboXSLT.SelectedIndex == -1)
            {
                string strLanguage = cboLanguage.SelectedValue?.ToString();
                int intNameIndex = -1;
                if (string.IsNullOrEmpty(strLanguage) || strLanguage == GlobalOptions.DefaultLanguage)
                    intNameIndex = cboXSLT.FindStringExact(GlobalOptions.DefaultCharacterSheet);
                else
                    intNameIndex = cboXSLT.FindStringExact(GlobalOptions.DefaultCharacterSheet.Substring(GlobalOptions.DefaultLanguage.LastIndexOf(Path.DirectorySeparatorChar) + 1));
                if (intNameIndex != -1)
                    cboXSLT.SelectedIndex = intNameIndex;
                else if (cboXSLT.Items.Count > 0)
                {
                    if (string.IsNullOrEmpty(strLanguage) || strLanguage == GlobalOptions.DefaultLanguage)
                        _strSelectedSheet = GlobalOptions.DefaultCharacterSheetDefaultValue;
                    else
                        _strSelectedSheet = Path.Combine(strLanguage, GlobalOptions.DefaultCharacterSheetDefaultValue);
                    cboXSLT.SelectedValue = _strSelectedSheet;
                    if (cboXSLT.SelectedIndex == -1)
                    {
                        cboXSLT.SelectedIndex = 0;
                        _strSelectedSheet = cboXSLT.SelectedValue?.ToString();
                    }
                }
            }
            _blnLoading = false;
            SetDocumentText(LanguageManager.GetString("String_Loading_Characters"));
        }

        private void cboXSLT_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Re-generate the output when a new sheet is selected.
            if (!_blnLoading)
            {
                _strSelectedSheet = cboXSLT.SelectedValue?.ToString() ?? string.Empty;
                RefreshSheet();
            }
        }

        private void cmdPrint_Click(object sender, EventArgs e)
        {
            webBrowser1.ShowPrintDialog();
        }

        private void tsPrintPreview_Click(object sender, EventArgs e)
        {
            webBrowser1.ShowPrintPreviewDialog();
        }

        private void cmdSaveHTML_Click(object sender, EventArgs e)
        {
            // Save the generated output as HTML.
            SaveFileDialog1.Filter = "HTML Page|*.htm";
            SaveFileDialog1.Title = LanguageManager.GetString("Button_Viewer_SaveAsHtml");
            SaveFileDialog1.ShowDialog();
            string strSaveFile = SaveFileDialog1.FileName;

            if (string.IsNullOrEmpty(strSaveFile))
                return;

            TextWriter objWriter = new StreamWriter(strSaveFile, false, Encoding.UTF8);
            objWriter.Write(webBrowser1.DocumentText);
            objWriter.Close();
        }

        private void tsSaveAsXml_Click(object sender, EventArgs e)
        {
            // Save the printout XML generated by the character.
            SaveFileDialog1.Filter = "XML File|*.xml";
            SaveFileDialog1.Title = LanguageManager.GetString("Button_Viewer_SaveAsXml");
            SaveFileDialog1.ShowDialog();
            string strSaveFile = SaveFileDialog1.FileName;

            if (string.IsNullOrEmpty(strSaveFile))
                return;

            try
            {
                _objCharacterXML.Save(strSaveFile);
            }
            catch (XmlException)
            {
                MessageBox.Show(LanguageManager.GetString("Message_Save_Error_Warning"));
            }
            catch (System.UnauthorizedAccessException)
            {
                MessageBox.Show(LanguageManager.GetString("Message_Save_Error_Warning"));
            }
        }

        private void frmViewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Remove the mugshots directory when the form closes.
            string mugshotsDirectoryPath = Path.Combine(Application.StartupPath, "mugshots");
            if (Directory.Exists(mugshotsDirectoryPath))
            {
                try
                {
                    Directory.Delete(mugshotsDirectoryPath, true);
                }
                catch (IOException)
                {
                }
            }

            // Clear the reference to the character's Print window.
            if (_lstCharacters.Count > 0)
            {
                foreach (Character objCharacter in _lstCharacters)
                    objCharacter.PrintWindow = null;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Set the text of the viewer to something descriptive. Also disables the Print, Print Preview, Save as HTML, and Save as PDF buttons.
        /// </summary>
        private void SetDocumentText(string strText)
        {
            cmdPrint.Enabled = false;
            tsPrintPreview.Enabled = false;
            cmdSaveHTML.Enabled = false;
            tsSaveAsPdf.Enabled = false;
            webBrowser1.DocumentText =
                "<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\" lang=\"en\">" +
                "<head><meta http-equiv=\"x - ua - compatible\" content=\"IE = Edge\"/><meta charset = \"UTF-8\" /></head>" +
                "<body style=\"width:100%;height:" + webBrowser1.Height.ToString() + ";text-align:center;vertical-align:middle;font-family:segoe, tahoma,'trebuchet ms',arial;font-size:9pt;\">" +
                strText.Replace("\n", "<br />") +
                "</body></html>";
        }

        /// <summary>
        /// Asynchronously update the characters (and therefore content) of the Viewer window.
        /// </summary>
        public void RefreshCharacters()
        {
            if (_workerRefresher.IsBusy)
                _workerRefresher.CancelAsync();
            if (_workerOutputGenerator.IsBusy)
                _workerOutputGenerator.CancelAsync();
            Cursor = Cursors.AppStarting;
            _workerRefresher.RunWorkerAsync();
        }

        /// <summary>
        /// Asynchronously update the sheet of the Viewer window.
        /// </summary>
        public void RefreshSheet()
        {
            Cursor = Cursors.AppStarting;
            SetDocumentText(LanguageManager.GetString("String_Generating_Sheet"));
            if (_workerOutputGenerator.IsBusy)
                _workerOutputGenerator.CancelAsync();
            _workerOutputGenerator.RunWorkerAsync();
        }

        /// <summary>
        /// Update the internal XML of the Viewer window.
        /// </summary>
        private void AsyncRefresh(object sender, DoWorkEventArgs e)
        {
            // Write the Character information to a MemoryStream so we don't need to create any files.
            MemoryStream objStream = new MemoryStream();
            using (XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8))
            {

                // Begin the document.
                objWriter.WriteStartDocument();

                // </characters>
                objWriter.WriteStartElement("characters");

                foreach (Character objCharacter in _lstCharacters)
                {
                    if (_workerRefresher.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
#if DEBUG
                    objCharacter.PrintToStream(objStream, objWriter, _objPrintCulture);
#else
                    objCharacter.PrintToStream(objWriter, GlobalOptions.CultureInfo);
#endif
                }

                // </characters>
                objWriter.WriteEndElement();
                if (_workerRefresher.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                // Finish the document and flush the Writer and Stream.
                objWriter.WriteEndDocument();
                objWriter.Flush();

                // Read the stream.
                StreamReader objReader = new StreamReader(objStream);
                objStream.Position = 0;
                XmlDocument objCharacterXML = new XmlDocument();

                // Put the stream into an XmlDocument and send it off to the Viewer.
                string strXML = objReader.ReadToEnd();
                if (_workerRefresher.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                objCharacterXML.LoadXml(strXML);

                if (_workerRefresher.CancellationPending)
                    e.Cancel = true;
                else
                    _objCharacterXML = objCharacterXML;
            }
        }

        private void FinishRefresh(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                tsSaveAsXml.Enabled = _objCharacterXML != null;
                RefreshSheet();
            }
        }

        /// <summary>
        /// Run the generated XML file through the XSL transformation engine to create the file output.
        /// </summary>
        private void AsyncGenerateOutput(object sender, DoWorkEventArgs e)
        {
            string strXslPath = Path.Combine(Application.StartupPath, "sheets", _strSelectedSheet + ".xsl");
            if (!File.Exists(strXslPath))
            {
                string strReturn = string.Format("File not found when attempting to load {0}\n", _strSelectedSheet);
                Log.Enter(strReturn);
                MessageBox.Show(strReturn);
                return;
            }
#if DEBUG
            XslCompiledTransform objXSLTransform = new XslCompiledTransform(true);
#else
            XslCompiledTransform objXSLTransform = new XslCompiledTransform();
#endif
            try
            {
                objXSLTransform.Load(strXslPath);
            }
            catch (Exception ex)
            {
                string strReturn = string.Format("Error attempting to load {0}\n", _strSelectedSheet);
                Log.Enter(strReturn);
                Log.Error("ERROR Message = " + ex.Message);
                strReturn += ex.Message;
                MessageBox.Show(strReturn);
                return;
            }

            if (_workerOutputGenerator.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            MemoryStream objStream = new MemoryStream();
            XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8);

            objXSLTransform.Transform(_objCharacterXML, objWriter);
            if (_workerOutputGenerator.CancellationPending)
            {
                e.Cancel = true;
                return;
            }
            objStream.Position = 0;

            // This reads from a static file, outputs to an HTML file, then has the browser read from that file. For debugging purposes.
            //objXSLTransform.Transform("D:\\temp\\print.xml", "D:\\temp\\output.htm");
            //webBrowser1.Navigate("D:\\temp\\output.htm");

            if (!GlobalOptions.PrintToFileFirst)
            {
                // Populate the browser using the DocumentStream.
                webBrowser1.DocumentStream = objStream;
            }
            else
            {
                // The DocumentStream method fails when using Wine, so we'll instead dump everything out a temporary HTML file, have the WebBrowser load that, then delete the temporary file.
                // Read in the resulting code and pass it to the browser.
                string strName = Guid.NewGuid().ToString() + ".htm";
                StreamReader objReader = new StreamReader(objStream);
                string strOutput = objReader.ReadToEnd();
                File.WriteAllText(strName, strOutput);
                string curDir = Directory.GetCurrentDirectory();
                webBrowser1.Url = new Uri(String.Format("file:///{0}/" + strName, curDir));
                File.Delete(strName);
            }
        }

        private void FinishGenerateOutput(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                cmdPrint.Enabled = true;
                tsPrintPreview.Enabled = true;
                cmdSaveHTML.Enabled = true;
                tsSaveAsPdf.Enabled = true;
            }
            Cursor = Cursors.Default;
        }

        private void MoveControls()
        {
            int intWidth = cmdPrint.Width;
            cmdPrint.AutoSize = false;
            cmdPrint.Width = intWidth + 20;
            cmdSaveHTML.Left = cmdPrint.Right + 6;

            lblCharacterSheet.Left = cboLanguage.Left - lblCharacterSheet.Width - 6;
            MinimumSize = new System.Drawing.Size(2 * cmdPrint.Left + cmdPrint.Width + cmdSaveHTML.Width + lblCharacterSheet.Width + cboLanguage.Width + cboXSLT.Width + 24, 73);
        }

        private void tsSaveAsPdf_Click(object sender, EventArgs e)
        {
            // Save the generated output as PDF.
            SaveFileDialog1.Filter = "PDF|*.pdf";
            SaveFileDialog1.Title = LanguageManager.GetString("Button_Viewer_SaveAsPdf");
            SaveFileDialog1.ShowDialog();
            string strSaveFile = SaveFileDialog1.FileName;

            if (string.IsNullOrEmpty(strSaveFile))
                return;
            //PdfDocument objpdf = TheArtOfDev.HtmlRenderer.PdfSharp.PdfGenerator.GeneratePdf(webBrowser1.DocumentText, PdfSharp.PageSize.A4);
            //objpdf.Save(strSaveFile);

            if (!Directory.Exists(Path.GetDirectoryName(strSaveFile)))
            {
                MessageBox.Show(LanguageManager.GetString("Message_File_Cannot_Be_Accessed"));
                return;
            }
            if (File.Exists(strSaveFile))
            {
                try
                {
                    File.Delete(strSaveFile);
                }
                catch (IOException)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_File_Cannot_Be_Accessed"));
                    return;
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_File_Cannot_Be_Accessed"));
                    return;
                }
            }
            
            PdfDocument objPdfDocument = new PdfDocument
            {
                Html = webBrowser1.DocumentText
            };
            objPdfDocument.ExtraParams.Add("encoding", "UTF-8");
            objPdfDocument.ExtraParams.Add("dpi", "300");
            objPdfDocument.ExtraParams.Add("margin-top", "13");
            objPdfDocument.ExtraParams.Add("margin-bottom", "19");
            objPdfDocument.ExtraParams.Add("margin-left", "13");
            objPdfDocument.ExtraParams.Add("margin-right", "13");
            objPdfDocument.ExtraParams.Add("image-quality", "100");
            objPdfDocument.ExtraParams.Add("print-media-type", "");

            PdfConvert.ConvertHtmlToPdf(objPdfDocument, new PdfConvertEnvironment
            {
                WkHtmlToPdfPath = Path.Combine(Application.StartupPath,"wkhtmltopdf.exe"),
                Timeout = 60000,
                TempFolderPath = Path.GetTempPath()
            }, new PdfOutput
            {
                OutputFilePath = strSaveFile
            });
            if (!string.IsNullOrWhiteSpace(GlobalOptions.PDFAppPath))
            {
                Uri uriPath = new Uri(strSaveFile);
                string strParams = GlobalOptions.PDFParameters;
                strParams = strParams.Replace("{page}", "1");
                strParams = strParams.Replace("{localpath}", uriPath.LocalPath);
                strParams = strParams.Replace("{absolutepath}", uriPath.AbsolutePath);
                ProcessStartInfo objProgress = new ProcessStartInfo
                {
                    FileName = GlobalOptions.PDFAppPath,
                    Arguments = strParams
                };
                Process.Start(objProgress);
            }
        }

        private static IList<ListItem> GetXslFilesFromLocalDirectory(string strLanguage)
        {
            List<ListItem> lstSheets = new List<ListItem>();

            // Populate the XSL list with all of the manifested XSL files found in the sheets\[language] directory.
            XmlDocument manifest = XmlManager.Load("sheets.xml");
            XmlNodeList sheets = manifest.SelectNodes($"/chummer/sheets[@lang='{strLanguage}']/sheet[not(hide)]");
            foreach (XmlNode sheet in sheets)
            {
                lstSheets.Add(new ListItem(strLanguage != GlobalOptions.DefaultLanguage ? Path.Combine(strLanguage, sheet["filename"].InnerText) : sheet["filename"].InnerText, sheet["name"].InnerText));
            }

            return lstSheets;
        }

        private static IList<ListItem> GetXslFilesFromOmaeDirectory()
        {
            var items = new List<ListItem>();

            // Populate the XSLT list with all of the XSL files found in the sheets\omae directory.
            string omaeDirectoryPath = Path.Combine(Application.StartupPath, "sheets", "omae");
            string menuMainOmae = LanguageManager.GetString("Menu_Main_Omae");

            // Only show files that end in .xsl. Do not include files that end in .xslt since they are used as "hidden" reference sheets 
            // (hidden because they are partial templates that cannot be used on their own).
            foreach (string fileName in ReadXslFileNamesWithoutExtensionFromDirectory(omaeDirectoryPath))
            {
                items.Add(new ListItem(Path.Combine("omae", fileName), menuMainOmae + ": " + fileName));
            }

            return items;
        }
        private static IList<string> ReadXslFileNamesWithoutExtensionFromDirectory(string path)
        {
            var names = new List<string>();

            if (Directory.Exists(path))
            {
                names = Directory.GetFiles(path, "*.xsl", SearchOption.AllDirectories).Select(Path.GetFileNameWithoutExtension).ToList();
            }

            return names;
        }

        private void PopulateXsltList()
        {
            List<ListItem> lstFiles = (List<ListItem>)GetXslFilesFromLocalDirectory(cboLanguage.SelectedValue?.ToString() ?? GlobalOptions.DefaultLanguage);
            if (GlobalOptions.OmaeEnabled)
            {
                lstFiles.AddRange(GetXslFilesFromOmaeDirectory());
            }

            cboXSLT.BeginUpdate();
            cboXSLT.ValueMember = "Value";
            cboXSLT.DisplayMember = "Name";
            cboXSLT.DataSource = lstFiles;
            cboXSLT.EndUpdate();
        }

        private void PopulateLanguageList()
        {
            List<ListItem> lstLanguages = new List<ListItem>();
            string languageDirectoryPath = Path.Combine(Application.StartupPath, "lang");
            string[] languageFilePaths = Directory.GetFiles(languageDirectoryPath, "*.xml");

            foreach (string filePath in languageFilePaths)
            {
                XmlDocument xmlDocument = new XmlDocument();

                try
                {
                    xmlDocument.Load(filePath);
                }
                catch (XmlException)
                {
                    continue;
                }

                XmlNode node = xmlDocument.SelectSingleNode("/chummer/name");

                if (node == null)
                    continue;

                string strLanguageCode = Path.GetFileNameWithoutExtension(filePath);
                if (GetXslFilesFromLocalDirectory(strLanguageCode).Count > 0)
                {
                    lstLanguages.Add(new ListItem(strLanguageCode, node.InnerText));
                }
            }
            
            lstLanguages.Sort(CompareListItems.CompareNames);

            cboLanguage.BeginUpdate();
            cboLanguage.ValueMember = "Value";
            cboLanguage.DisplayMember = "Name";
            cboLanguage.DataSource = lstLanguages;
            cboLanguage.SelectedValue = GlobalOptions.Language;
            if (cboLanguage.SelectedIndex == -1)
                cboLanguage.SelectedValue = GlobalOptions.DefaultLanguage;
            cboLanguage.EndUpdate();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Character's XmlDocument.
        /// </summary>
        public XmlDocument CharacterXML
        {
            set
            {
                if (_objCharacterXML != value)
                {
                    _objCharacterXML = value;
                    tsSaveAsXml.Enabled = value != null;
                    RefreshSheet();
                }
            }
        }

        /// <summary>
        /// Set the XSL sheet that will be selected by default.
        /// </summary>
        public string SelectedSheet
        {
            set
            {
                _strSelectedSheet = value;
            }
        }

        /// <summary>
        /// List of Characters to print.
        /// </summary>
        public List<Character> Characters
        {
            set
            {
                _lstCharacters = value;
            }
        }
#endregion

        private void ContextMenu_Opening(object sender, CancelEventArgs e)
        {
            foreach (ToolStripItem objItem in ((ContextMenuStrip)sender).Items)
            {
                if (objItem.Tag != null)
                    objItem.Text = LanguageManager.GetString(objItem.Tag.ToString());
            }
        }

        private void ContextMenu_DropDownOpening(object sender, EventArgs e)
        {
            foreach (ToolStripItem objItem in ((ToolStripDropDownItem)sender).DropDownItems)
            {
                if (objItem.Tag != null)
                    objItem.Text = LanguageManager.GetString(objItem.Tag.ToString());
            }
        }

        private void cboLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                _objPrintCulture = CultureInfo.GetCultureInfo(cboLanguage.SelectedValue.ToString());
            }
            catch (CultureNotFoundException)
            {
            }
            if (_blnLoading)
                return;
            string strOldSelected = _strSelectedSheet;
            // Strip away the language prefix
            if (strOldSelected.Contains(Path.DirectorySeparatorChar))
                strOldSelected = strOldSelected.Substring(strOldSelected.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            _blnLoading = true;
            PopulateXsltList();
            string strNewLanguage = cboLanguage.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strNewLanguage) || strNewLanguage == GlobalOptions.DefaultLanguage)
                _strSelectedSheet = strOldSelected;
            else
                _strSelectedSheet = Path.Combine(strNewLanguage, strOldSelected);
            cboXSLT.SelectedValue = _strSelectedSheet;
            // If the desired sheet was not found, fall back to the Shadowrun 5 sheet.
            if (cboXSLT.SelectedIndex == -1)
            {
                int intNameIndex = -1;
                if (string.IsNullOrEmpty(strNewLanguage) || strNewLanguage == GlobalOptions.DefaultLanguage)
                    intNameIndex = cboXSLT.FindStringExact(GlobalOptions.DefaultCharacterSheet);
                else
                    intNameIndex = cboXSLT.FindStringExact(GlobalOptions.DefaultCharacterSheet.Substring(GlobalOptions.DefaultLanguage.LastIndexOf(Path.DirectorySeparatorChar) + 1));
                if (intNameIndex != -1)
                    cboXSLT.SelectedIndex = intNameIndex;
                else if (cboXSLT.Items.Count > 0)
                {
                    if (string.IsNullOrEmpty(strNewLanguage) || strNewLanguage == GlobalOptions.DefaultLanguage)
                        _strSelectedSheet = GlobalOptions.DefaultCharacterSheetDefaultValue;
                    else
                        _strSelectedSheet = Path.Combine(strNewLanguage, GlobalOptions.DefaultCharacterSheetDefaultValue);
                    cboXSLT.SelectedValue = _strSelectedSheet;
                    if (cboXSLT.SelectedIndex == -1)
                    {
                        cboXSLT.SelectedIndex = 0;
                        _strSelectedSheet = cboXSLT.SelectedValue?.ToString();
                    }
                }
            }
            _blnLoading = false;
            RefreshCharacters();
        }
    }
}
