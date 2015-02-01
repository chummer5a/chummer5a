using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
	public partial class frmSelectExoticSkill : Form
	{
		#region Control Events
		public frmSelectExoticSkill()
		{
			InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
		}

		private void cmdOK_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
		}

		private void cmdCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}

		private void frmSelectExoticSkill_Load(object sender, EventArgs e)
		{
			XmlDocument objXmlDocument = new XmlDocument();
			objXmlDocument = XmlManager.Instance.Load("skills.xml");

			List<ListItem> lstSkills = new List<ListItem>();

			// Build the list of Exotic Active Skills from the Skills file.
			XmlNodeList objXmlSkillList = objXmlDocument.SelectNodes("/chummer/skills/skill[exotic = \"Yes\"]");
			foreach (XmlNode objXmlSkill in objXmlSkillList)
			{
				ListItem objItem = new ListItem();
				objItem.Value = objXmlSkill["name"].InnerText;
				if (objXmlSkill.Attributes != null)
				{
					if (objXmlSkill["translate"] != null)
						objItem.Name = objXmlSkill["translate"].InnerText;
					else
						objItem.Name = objXmlSkill["name"].InnerText;
				}
				else
					objItem.Name = objXmlSkill["name"].InnerXml;
				lstSkills.Add(objItem);
			}
			SortListItem objSort = new SortListItem();
			lstSkills.Sort(objSort.Compare);
			cboCategory.ValueMember = "Value";
			cboCategory.DisplayMember = "Name";
			cboCategory.DataSource = lstSkills;

			// Select the first Skill in the list.
			cboCategory.SelectedIndex = 0;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Skill that was selected in the dialogue.
		/// </summary>
		public string SelectedExoticSkill
		{
			get
			{
				return cboCategory.SelectedValue.ToString();
			}
		}
		#endregion
	}
}