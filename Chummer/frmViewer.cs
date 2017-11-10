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

namespace Chummer
{
    public partial class frmViewer : Form
    {
        private List<Character> _lstCharacters = new List<Character>();
        private XmlDocument _objCharacterXML = new XmlDocument();
        private string _strSelectedSheet = GlobalOptions.DefaultCharacterSheet;
        private bool _blnLoading = false;
        #region Control Events
        public frmViewer()
        {
            if (_strSelectedSheet.StartsWith("Shadowrun 4"))
            {
                _strSelectedSheet = GlobalOptions.DefaultCharacterSheetDefaultValue;
            }
            if (GlobalOptions.Language != GlobalOptions.DefaultLanguage && !_strSelectedSheet.Contains('\\'))
                _strSelectedSheet = Path.Combine(GlobalOptions.Language, _strSelectedSheet);
            else if (GlobalOptions.Language == GlobalOptions.DefaultLanguage && _strSelectedSheet.Contains('\\'))
                _strSelectedSheet = _strSelectedSheet.Substring(_strSelectedSheet.LastIndexOf('\\') + 1, _strSelectedSheet.Length - 1 - _strSelectedSheet.LastIndexOf('\\'));

            Microsoft.Win32.RegistryKey objRegistry = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION");
            objRegistry.SetValue(AppDomain.CurrentDomain.FriendlyName, 0x1F40, Microsoft.Win32.RegistryValueKind.DWord);
            objRegistry = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software\\WOW6432Node\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION");
            objRegistry.SetValue(AppDomain.CurrentDomain.FriendlyName, 0x1F40, Microsoft.Win32.RegistryValueKind.DWord);

            InitializeComponent();
            LanguageManager.Load(GlobalOptions.Language, this);
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
                if (cboLanguage.SelectedValue.ToString() == GlobalOptions.DefaultLanguage)
                    cboXSLT.SelectedValue = GlobalOptions.DefaultCharacterSheetDefaultValue;
                else
                    _strSelectedSheet = Path.Combine(cboLanguage.SelectedValue.ToString(), GlobalOptions.DefaultCharacterSheetDefaultValue);
                if (cboXSLT.SelectedIndex == -1)
                    cboXSLT.SelectedIndex = 0;
            }
            GenerateOutput();
            _blnLoading = false;
        }

        private void cboXSLT_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Re-generate the output when a new sheet is selected.
            if (!_blnLoading)
            {
                _strSelectedSheet = cboXSLT.SelectedValue?.ToString() ?? string.Empty;
                GenerateOutput();
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
        /// Run the generated XML file through the XSL transformation engine to create the file output.
        /// </summary>
        private void GenerateOutput()
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

            MemoryStream objStream = new MemoryStream();
            XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8);

            objXSLTransform.Transform(_objCharacterXML, null, objWriter);
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

        /// <summary>
        /// Asynchronously update the contents of the Viewer window.
        /// </summary>
        public void RefreshView()
        {
            // Create a delegate to handle refreshing the window.
            MethodInvoker objRefreshDelegate = AsyncRefresh;
            objRefreshDelegate.BeginInvoke(null, null);
        }

        /// <summary>
        /// Update the contents of the Viewer window.
        /// </summary>
        private void AsyncRefresh()
        {
            // Write the Character information to a MemoryStream so we don't need to create any files.
            MemoryStream objStream = new MemoryStream();
            XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8);

            // Being the document.
            objWriter.WriteStartDocument();

            // </characters>
            objWriter.WriteStartElement("characters");

            foreach (Character objCharacter in _lstCharacters)
#if DEBUG
                objCharacter.PrintToStream(objStream, objWriter);
#else
                objCharacter.PrintToStream(objWriter);
#endif

            // </characters>
            objWriter.WriteEndElement();

            // Finish the document and flush the Writer and Stream.
            objWriter.WriteEndDocument();
            objWriter.Flush();
            objStream.Flush();

            // Read the stream.
            StreamReader objReader = new StreamReader(objStream);
            objStream.Position = 0;
            XmlDocument objCharacterXML = new XmlDocument();

            // Put the stream into an XmlDocument and send it off to the Viewer.
            string strXML = objReader.ReadToEnd();
            objCharacterXML.LoadXml(strXML);

            objWriter.Close();
            objStream.Close();

            _objCharacterXML = objCharacterXML;
            GenerateOutput();
        }

        private void MoveControls()
        {
            int intWidth = cmdPrint.Width;
            cmdPrint.AutoSize = false;
            cmdPrint.Width = intWidth + 20;
            cmdSaveHTML.Left = cmdPrint.Right + 6;
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

            Dictionary<string, string> dicParams = new Dictionary<string, string>();
            dicParams.Add("encoding", "UTF-8");
            dicParams.Add("dpi", "300");
            dicParams.Add("margin-top", "13");
            dicParams.Add("margin-bottom", "19");
            dicParams.Add("margin-left", "13");
            dicParams.Add("margin-right", "13");
            dicParams.Add("image-quality", "100");
            dicParams.Add("print-media-type", "");
            PdfConvert.ConvertHtmlToPdf(new PdfDocument
            {
                Html = webBrowser1.DocumentText,
                ExtraParams = dicParams
            }, new PdfConvertEnvironment
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

        private List<ListItem> GetXslFilesFromLocalDirectory(string strLanguage)
        {
            List<ListItem> lstSheets = new List<ListItem>();

            // Populate the XSL list with all of the manifested XSL files found in the sheets\[language] directory.
            XmlDocument objLanguageDocument = LanguageManager.XmlDoc;
            XmlDocument manifest = XmlManager.Load("sheets.xml");
            XmlNodeList sheets = manifest.SelectNodes($"/chummer/sheets[@lang='{strLanguage}']/sheet[not(hide)]");
            foreach (XmlNode sheet in sheets)
            {
                ListItem objItem = new ListItem();
                objItem.Value = strLanguage != GlobalOptions.DefaultLanguage ? Path.Combine(strLanguage, sheet["filename"].InnerText) : sheet["filename"].InnerText;
                objItem.Name = sheet["name"].InnerText;

                lstSheets.Add(objItem);
            }

            return lstSheets;
        }

        private List<ListItem> GetXslFilesFromOmaeDirectory()
        {
            var items = new List<ListItem>();

            // Populate the XSLT list with all of the XSL files found in the sheets\omae directory.
            string omaeDirectoryPath = Path.Combine(Application.StartupPath, "sheets", "omae");
            string menuMainOmae = LanguageManager.GetString("Menu_Main_Omae");

            // Only show files that end in .xsl. Do not include files that end in .xslt since they are used as "hidden" reference sheets 
            // (hidden because they are partial templates that cannot be used on their own).
            List<string> fileNames = ReadXslFileNamesWithoutExtensionFromDirectory(omaeDirectoryPath);

            foreach (string fileName in fileNames)
            {
                ListItem objItem = new ListItem();
                objItem.Value = Path.Combine("omae", fileName);
                objItem.Name = menuMainOmae + ": " + fileName;

                items.Add(objItem);
            }

            return items;
        }
        private List<string> ReadXslFileNamesWithoutExtensionFromDirectory(string path)
        {
            var names = new List<string>();

            if (Directory.Exists(path))
            {
                names = Directory.GetFiles(path)
                    .Where(s => s.EndsWith(".xsl"))
                    .Select(Path.GetFileNameWithoutExtension).ToList();
            }

            return names;
        }

        private void PopulateXsltList()
        {
            List<ListItem> lstFiles = new List<ListItem>();

            lstFiles.AddRange(GetXslFilesFromLocalDirectory(cboLanguage.SelectedValue.ToString()));
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

                string languageName = node.InnerText;

                if (GetXslFilesFromLocalDirectory(Path.GetFileNameWithoutExtension(filePath).ToString()).Count > 0)
                {
                    ListItem objItem = new ListItem();
                    objItem.Value = Path.GetFileNameWithoutExtension(filePath);
                    objItem.Name = languageName;

                    lstLanguages.Add(objItem);
                }
            }

            SortListItem objSort = new SortListItem();
            lstLanguages.Sort(objSort.Compare);

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
                _objCharacterXML = value;
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
            if (_blnLoading)
                return;
            string strOldSelected = _strSelectedSheet;
            // Strip away the language prefix
            if (strOldSelected.Contains('\\'))
                strOldSelected = strOldSelected.Substring(strOldSelected.LastIndexOf('\\') + 1, strOldSelected.Length - 1 - strOldSelected.LastIndexOf('\\'));
            _blnLoading = true;
            PopulateXsltList();
            string strNewLanguage = cboLanguage.SelectedValue.ToString();
            if (strNewLanguage == GlobalOptions.DefaultLanguage)
                _strSelectedSheet = strOldSelected;
            else
                _strSelectedSheet = Path.Combine(strNewLanguage, strOldSelected);
            cboXSLT.SelectedValue = _strSelectedSheet;
            // If the desired sheet was not found, fall back to the Shadowrun 5 sheet.
            if (cboXSLT.SelectedIndex == -1)
            {
                if (strNewLanguage == GlobalOptions.DefaultLanguage)
                    _strSelectedSheet = GlobalOptions.DefaultCharacterSheetDefaultValue;
                else
                    _strSelectedSheet = Path.Combine(strNewLanguage, GlobalOptions.DefaultCharacterSheetDefaultValue);
                cboXSLT.SelectedValue = _strSelectedSheet;
                if (cboXSLT.SelectedIndex == -1)
                {
                    cboXSLT.SelectedIndex = 0;
                    _strSelectedSheet = cboXSLT.SelectedValue.ToString();
                }
            }
            _blnLoading = false;
            GenerateOutput();
        }
    }
}
