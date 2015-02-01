using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
	public partial class frmSelectSetting : Form
	{
		private string _strSettingsFile = "default.xml";

		#region Control Events
		public frmSelectSetting()
		{
			InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
			MoveControls();
		}

		private void frmSelectSetting_Load(object sender, EventArgs e)
		{
			// Build the list of XML files found in the settings directory.
			List<ListItem> lstSettings = new List<ListItem>();
			foreach (string strFileName in Directory.GetFiles(Path.Combine(Application.StartupPath, "settings"), "*.xml"))
			{
				// Remove the path from the file name.
				string strSettingsFile = strFileName;
				strSettingsFile = strSettingsFile.Replace(Path.Combine(Application.StartupPath, "settings"), string.Empty);
				strSettingsFile = strSettingsFile.Replace(Path.DirectorySeparatorChar, ' ').Trim();

				// Load the file so we can get the Setting name.
				XmlDocument objXmlDocument = new XmlDocument();
				objXmlDocument.Load(strFileName);
				string strSettingsName = objXmlDocument.SelectSingleNode("/settings/name").InnerText;

				ListItem objItem = new ListItem();
				objItem.Value = strSettingsFile;
				objItem.Name = strSettingsName;

				lstSettings.Add(objItem);
			}
			SortListItem objSort = new SortListItem();
			lstSettings.Sort(objSort.Compare);
			cboSetting.DataSource = lstSettings;
			cboSetting.ValueMember = "Value";
			cboSetting.DisplayMember = "Name";

			// Attempt to make default.xml the default one. If it could not be found in the list, select the first item instead.
			cboSetting.SelectedIndex = cboSetting.FindStringExact("Default Settings");
			if (cboSetting.SelectedIndex == -1)
				cboSetting.SelectedIndex = 0;
		}

		private void cmdCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}

		private void cmdOK_Click(object sender, EventArgs e)
		{
			_strSettingsFile = cboSetting.SelectedValue.ToString();
			this.DialogResult = DialogResult.OK;
		}
		#endregion

		#region Methods
		private void MoveControls()
		{
			cboSetting.Left = lblSetting.Left + lblSetting.Width + 6;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Settings file that was selected in the dialogue.
		/// </summary>
		public string SettingsFile
		{
			get
			{
				return _strSettingsFile;
			}
		}
		#endregion
	}
}