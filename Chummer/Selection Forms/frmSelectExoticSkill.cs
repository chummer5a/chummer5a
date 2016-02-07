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
			List<ListItem> lstSkillSpecialisations = new List<ListItem>();

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

			XmlNodeList objXmlSelectedSkill = objXmlDocument.SelectNodes("/chummer/skills/skill[name = \"" + cboCategory.SelectedValue.ToString() + "\"]/specs/spec");
			XmlDocument objXmlWeaponDocument = XmlManager.Instance.Load("weapons.xml");
			XmlNodeList objXmlWeaponList = objXmlWeaponDocument.SelectNodes("/chummer/weapons/weapon[category = \"" + cboCategory.SelectedValue.ToString() + "s\" or useskill = \"" + cboCategory.SelectedValue.ToString() + "\"]");
			foreach (XmlNode objXmlWeapon in objXmlWeaponList)
			{
				ListItem objItem = new ListItem();
				objItem.Value = objXmlWeapon["name"].InnerText;
					if (objXmlWeapon.Attributes != null)
					{
						if (objXmlWeapon["translate"] != null)
						{
							objItem.Name = objXmlWeapon["translate"].InnerText;
						}
						else
						{
							objItem.Name = objXmlWeapon["name"].InnerText;
						}
					}
					else
					{
						objItem.Name = objXmlWeapon["name"].InnerXml;
					}

				lstSkillSpecialisations.Add(objItem);
			}
			foreach (XmlNode objXmlSpecialization in objXmlSelectedSkill)
			{
				ListItem objItem = new ListItem();
				objItem.Value = objXmlSpecialization.InnerText;
				if (objXmlSpecialization.Attributes != null)
				{
					if (objXmlSpecialization["translate"] != null)
						objItem.Name = objXmlSpecialization["translate"].InnerText;
					else
						objItem.Name = objXmlSpecialization.InnerText;
				}
				else
					objItem.Name = objXmlSpecialization["name"].InnerXml;
				lstSkillSpecialisations.Add(objItem);
			}
			cboSkillSpecialisations.ValueMember = "Value";
			cboSkillSpecialisations.DisplayMember = "Name";
			cboSkillSpecialisations.DataSource = lstSkillSpecialisations;

			// Select the first Skill in the list.
			cboSkillSpecialisations.SelectedIndex = 0;
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
		/// <summary>
		/// Skill specialisation that was selected in the dialogue.
		/// </summary>
		public string SelectedExoticSkillSpecialisation
		{
			get
			{
				if (cboSkillSpecialisations.SelectedValue == null)
				{
					return cboSkillSpecialisations.Text;
				}
				else
				{
					return cboSkillSpecialisations.SelectedValue.ToString();
                }
			}
		}
		#endregion
	}
}