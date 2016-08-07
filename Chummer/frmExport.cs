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
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Xsl;

namespace Chummer
{
	public partial class frmExport : Form
	{
		private XmlDocument _objCharacterXML = new XmlDocument();

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

			if (cboXSLT.Items.Count > 0)
				cboXSLT.SelectedIndex = 0;
		}

		private void cmdCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}

		private void cmdOK_Click(object sender, EventArgs e)
		{
			if (cboXSLT.Text == string.Empty)
				return;

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

			string strSaveFile = "";
			SaveFileDialog1.Filter = strExtension.ToUpper() + "|*." + strExtension;
			SaveFileDialog1.Title = LanguageManager.Instance.GetString("Button_Viewer_SaveAsHtml");
			SaveFileDialog1.ShowDialog();
			strSaveFile = SaveFileDialog1.FileName;

			if (strSaveFile == "")
				return;

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
			File.WriteAllText(strSaveFile, objReader.ReadToEnd()); // Change this to a proper path.

			this.DialogResult = DialogResult.OK;
		}
		#endregion

		#region Methods
		private void MoveControls()
		{
			cboXSLT.Left = lblExport.Left + lblExport.Width + 6;
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
		#endregion
	}
}