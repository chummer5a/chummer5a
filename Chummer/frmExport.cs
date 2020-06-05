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
 using System.IO;
 using System.Text;
 using System.Windows.Forms;
using System.Xml;
using System.Xml.Xsl;
 using Newtonsoft.Json;
 using Formatting = Newtonsoft.Json.Formatting;

namespace Chummer
{
    public partial class frmExport : Form
    {
        private readonly XmlDocument _objCharacterXML;
        private readonly Dictionary<string,string> _dictCache = new Dictionary<string, string>();
        private bool _blnSelected;

        #region Control Events
        public frmExport(XmlDocument objCharacterXML)
        {
            _objCharacterXML = objCharacterXML;
            InitializeComponent();
            this.TranslateWinForm();
            MoveControls();
        }

        private void frmExport_Load(object sender, EventArgs e)
        {
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
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            string strXSLT = cboXSLT.Text;
            if (string.IsNullOrEmpty(strXSLT))
                return;

            if (strXSLT == "Export JSON")
            {
                ExportJson();
            }
            else
            {
                ExportNormal();
            }
        }

        private void cboXSLT_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strXSLT = cboXSLT.Text;
            if (string.IsNullOrEmpty(strXSLT))
                return;

            if (_dictCache.TryGetValue(strXSLT, out string strBoxText))
            {
                rtbText.Text = strBoxText;
            }

            if (strXSLT == "Export JSON")
            {
                GenerateJson();
            }
            else
            {
                GenerateXml();
            }
        }

        private void rtbText_Leave(object sender, EventArgs e)
        {
            _blnSelected = false;
        }

        private void rtbText_MouseUp(object sender, MouseEventArgs e)
        {
            if (_blnSelected || rtbText.SelectionLength != 0) return;
            _blnSelected = true;
            rtbText.SelectAll();
        }

        #endregion

        #region Methods
        private void MoveControls()
        {
            cboXSLT.Left = lblExport.Left + lblExport.Width + 6;
        }
        #region XML
        private void ExportNormal()
        {
            // Look for the file extension information.
            string strLine;
            string strExtension = "xml";
            string exportSheetPath = Path.Combine(Utils.GetStartupPath, "export", cboXSLT.Text + ".xsl");
            using (StreamReader objFile = new StreamReader(exportSheetPath, Encoding.UTF8, true))
            {
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

            File.WriteAllText(strSaveFile, rtbText.Text); // Change this to a proper path.

            DialogResult = DialogResult.OK;
        }

        private void GenerateXml()
        {
            string exportSheetPath = Path.Combine(Utils.GetStartupPath, "export", cboXSLT.Text + ".xsl");

            XslCompiledTransform objXSLTransform = new XslCompiledTransform();
            objXSLTransform.Load(exportSheetPath); // Use the path for the export sheet.

            XmlWriterSettings objSettings = objXSLTransform.OutputSettings.Clone();
            objSettings.CheckCharacters = false;
            objSettings.ConformanceLevel = ConformanceLevel.Fragment;

            MemoryStream objStream = new MemoryStream();
            using (XmlWriter objWriter = XmlWriter.Create(objStream, objSettings))
                objXSLTransform.Transform(_objCharacterXML, null, objWriter);
            objStream.Position = 0;

            // Read in the resulting code and pass it to the browser.
            using (StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true))
                rtbText.Text = objReader.ReadToEnd();

            if (!_dictCache.ContainsKey(cboXSLT.Text))
            {
                _dictCache.Add(cboXSLT.Text, rtbText.Text);
            }
        }
        #endregion
        #region JSON
        private void GenerateJson()
        {
            string json = JsonConvert.SerializeXmlNode(_objCharacterXML, Formatting.Indented);
            rtbText.Text = json;

            if (!_dictCache.ContainsKey(cboXSLT.Text))
            {
                _dictCache.Add(cboXSLT.Text, rtbText.Text);
            }
        }

        private void ExportJson()
        {
            SaveFileDialog1.AddExtension = true;
            SaveFileDialog1.DefaultExt = "json";
            SaveFileDialog1.Filter = LanguageManager.GetString("DialogFilter_Json") + '|' + LanguageManager.GetString("DialogFilter_All");
            SaveFileDialog1.Title = LanguageManager.GetString("Button_Export_SaveJsonAs");
            SaveFileDialog1.ShowDialog();

            if (string.IsNullOrWhiteSpace(SaveFileDialog1.FileName))
                return;

            File.WriteAllText(SaveFileDialog1.FileName, rtbText.Text, Encoding.UTF8);

            DialogResult = DialogResult.OK;
        }
        #endregion
        #endregion
    }
}
