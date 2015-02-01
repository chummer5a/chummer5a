using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
	public partial class frmSelectSkillCategory : Form
	{
		private string _strSelectedCategory = "";
		private string _strForceCategory = "";

		private XmlDocument _objXmlDocument = new XmlDocument();

		#region Control Events
		public frmSelectSkillCategory()
		{
			InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
		}

		private void frmSelectSkillCategory_Load(object sender, EventArgs e)
		{
			_objXmlDocument = XmlManager.Instance.Load("skills.xml");

			// Build the list of Skill Categories found in the Skills file.
			XmlNodeList objXmlCategoryList;
			List<ListItem> lstCategory = new List<ListItem>();
			if (_strForceCategory != "")
			{
				objXmlCategoryList = _objXmlDocument.SelectNodes("/chummer/categories/category[. = \"" + _strForceCategory + "\"]");
			}
			else
			{
				objXmlCategoryList = _objXmlDocument.SelectNodes("/chummer/categories/category");
			}

			foreach (XmlNode objXmlCategory in objXmlCategoryList)
			{
				ListItem objItem = new ListItem();
				objItem.Value = objXmlCategory.InnerText;
				if (objXmlCategory.Attributes != null)
				{
					if (objXmlCategory.Attributes["translate"] != null)
						objItem.Name = objXmlCategory.Attributes["translate"].InnerText;
					else
						objItem.Name = objXmlCategory.InnerText;
				}
				else
					objItem.Name = objXmlCategory.InnerText;
				lstCategory.Add(objItem);
			}

			cboCategory.ValueMember = "Value";
			cboCategory.DisplayMember = "Name";
			cboCategory.DataSource = lstCategory;

			// Select the first Skill in the list.
			cboCategory.SelectedIndex = 0;

			if (cboCategory.Items.Count == 1)
				cmdOK_Click(sender, e);
		}

		private void cmdOK_Click(object sender, EventArgs e)
		{
			_strSelectedCategory = cboCategory.SelectedValue.ToString();
			this.DialogResult = DialogResult.OK;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Weapon Category that was selected in the dialogue.
		/// </summary>
		public string SelectedCategory
		{
			get
			{
				return _strSelectedCategory;
			}
		}

		/// <summary>
		/// Description to show in the window.
		/// </summary>
		public string Description
		{
			set
			{
				lblDescription.Text = value;
			}
		}

		/// <summary>
		/// Restrict the list to only a single Category.
		/// </summary>
		public string OnlyCategory
		{
			set
			{
				_strForceCategory = value;
			}
		}
		#endregion
	}
}