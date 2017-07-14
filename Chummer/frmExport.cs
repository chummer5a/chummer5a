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
 using System.IO;
 using System.Linq;
 using System.Text;
 using System.Windows.Forms;
using System.Xml;
using System.Xml.Xsl;
 using Amazon.Runtime.Internal.Transform;
 using Newtonsoft.Json;
 using Formatting = Newtonsoft.Json.Formatting;

namespace Chummer
{
    public partial class frmExport : Form
    {
        private XmlDocument _objCharacterXML = new XmlDocument();
        private readonly Dictionary<string,string> _dictCache = new Dictionary<string, string>();
        private bool _blnSelected = false;

        #region Control Events
        public frmExport()
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            MoveControls();
        }

        private void frmExport_Load(object sender, EventArgs e)
        {
            // Populate the XSLT list with all of the XSL files found in the sheets directory.
            string exportDirectoryPath = Path.Combine(Application.StartupPath, "export");
            foreach (string strFile in Directory.GetFiles(exportDirectoryPath))
            {
                // Only show files that end in .xsl. Do not include files that end in .xslt since they are used as "hidden" reference sheets (hidden because they are partial templates that cannot be used on their own).
                if (!strFile.EndsWith(".xslt") && strFile.EndsWith(".xsl"))
                {
                    string strFileName = Path.GetFileNameWithoutExtension(strFile);
                    cboXSLT.Items.Add(strFileName);
                }
            }

            cboXSLT.Items.Add("Export JSON");

            if (cboXSLT.Items.Count > 0)
                cboXSLT.SelectedIndex = 0;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cboXSLT.Text))
                return;

            if (cboXSLT.Text == "Export JSON")
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
            if (cboXSLT.Text == string.Empty)
                return;

            string strBoxText;
            if (_dictCache.TryGetValue(cboXSLT.Text, out strBoxText))
            {
                rtbText.Text = strBoxText;
            }

            if (cboXSLT.Text == "Export JSON")
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
            string strLine = "";
            string strExtension = "xml";
            string exportSheetPath = Path.Combine(Application.StartupPath, "export", cboXSLT.Text + ".xsl");
            StreamReader objFile = new StreamReader(exportSheetPath);
            while ((strLine = objFile.ReadLine()) != null)
            {
                if (strLine.StartsWith("<!-- ext:"))
                    strExtension = strLine.Replace("<!-- ext:", string.Empty).Replace("-->", string.Empty).Trim();
            }
            objFile.Close();

            SaveFileDialog1.Filter = strExtension.ToUpper() + "|*." + strExtension;
            SaveFileDialog1.Title = LanguageManager.Instance.GetString("Button_Viewer_SaveAsHtml");
            SaveFileDialog1.ShowDialog();
            string strSaveFile = SaveFileDialog1.FileName;

            if (string.IsNullOrEmpty(strSaveFile))
                return;
            
            File.WriteAllText(strSaveFile, rtbText.Text); // Change this to a proper path.

            this.DialogResult = DialogResult.OK;
        }

        private void GenerateXml()
        {
            string exportSheetPath = Path.Combine(Application.StartupPath, "export", cboXSLT.Text + ".xsl");

            XslCompiledTransform objXSLTransform = new XslCompiledTransform();
            objXSLTransform.Load(exportSheetPath); // Use the path for the export sheet.

            XmlWriterSettings objSettings = objXSLTransform.OutputSettings.Clone();
            objSettings.CheckCharacters = false;
            objSettings.ConformanceLevel = ConformanceLevel.Fragment;

            MemoryStream objStream = new MemoryStream();
            XmlWriter objWriter = XmlWriter.Create(objStream, objSettings);

            objXSLTransform.Transform(_objCharacterXML, null, objWriter);
            objStream.Position = 0;

            // Read in the resulting code and pass it to the browser.
            StreamReader objReader = new StreamReader(objStream);
            rtbText.Text = objReader.ReadToEnd();

            if (!_dictCache.ContainsKey(cboXSLT.Text))
            {
                _dictCache.Add(new KeyValuePair<string, string>(cboXSLT.Text, rtbText.Text));
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
                _dictCache.Add(new KeyValuePair<string, string>(cboXSLT.Text, rtbText.Text));
            }
        }

        private void ExportJson()
        {
            SaveFileDialog1.AddExtension = true;
            SaveFileDialog1.DefaultExt = "json";
            SaveFileDialog1.Filter = "JSON File|*.json";
            SaveFileDialog1.Title = "Save JSON as";
            SaveFileDialog1.ShowDialog();

            if (string.IsNullOrWhiteSpace(SaveFileDialog1.FileName))
                return;

            File.WriteAllText(SaveFileDialog1.FileName, rtbText.Text, Encoding.UTF8);

            this.DialogResult = DialogResult.OK;
        }
        #endregion
        #endregion

        #region Properties
        /// <summary>
        /// Character's XmlDocument.
        /// </summary>
        public XmlDocument CharacterXml
        {
            set
            {
                _objCharacterXML = value;
            }
        }
        #endregion

    }
}