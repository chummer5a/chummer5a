using System;
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
			foreach (string strFile in Directory.GetFiles(Application.StartupPath + Path.DirectorySeparatorChar + "export"))
			{
				// Only show files that end in .xsl. Do not include files that end in .xslt since they are used as "hidden" reference sheets (hidden because they are partial templates that cannot be used on their own).
				if (!strFile.EndsWith(".xslt") && strFile.EndsWith(".xsl"))
				{
					string strFileName = strFile.Replace(Application.StartupPath + Path.DirectorySeparatorChar + "export" + Path.DirectorySeparatorChar, string.Empty).Replace(".xsl", string.Empty);
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
			StreamReader objFile = new StreamReader(Application.StartupPath + Path.DirectorySeparatorChar + "export" + Path.DirectorySeparatorChar + cboXSLT.Text + ".xsl");
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
			objXSLTransform.Load(Application.StartupPath + Path.DirectorySeparatorChar + "export" + Path.DirectorySeparatorChar + cboXSLT.Text + ".xsl"); // Use the path for the export sheet.

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