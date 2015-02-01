using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    public partial class frmSelectSkillGroup : Form
    {
        private string _strReturnValue = "";
		private string _strForceValue = "";
		private string _strExcludeCategory = "";

		private XmlDocument _objXmlDocument = new XmlDocument();

		#region Control Events
		public frmSelectSkillGroup()
        {
            InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
        }

        private void frmSelectSkillGroup_Load(object sender, EventArgs e)
        {
			List<ListItem> lstGroups = new List<ListItem>();
			_objXmlDocument = XmlManager.Instance.Load("skills.xml");

			if (_strForceValue == "")
			{
				// Build the list of Skill Groups found in the Skills file.
				XmlNodeList objXmlSkillList = _objXmlDocument.SelectNodes("/chummer/skillgroups/name");
				foreach (XmlNode objXmlSkill in objXmlSkillList)
				{
					bool blnAdd = true;
					if (_strExcludeCategory != string.Empty)
					{
						blnAdd = false;
						string[] strExcludes = _strExcludeCategory.Split(',');
						string strExclude = "";
						for (int i = 0; i <= strExcludes.Length - 1; i++)
							strExclude += "category != \"" + strExcludes[i].Trim() + "\" and ";
						// Remove the trailing " and ";
						strExclude = strExclude.Substring(0, strExclude.Length - 5);

						XmlNodeList objXmlNodeList = _objXmlDocument.SelectNodes("/chummer/skills/skill[" + strExclude + " and skillgroup = \"" + objXmlSkill.InnerText + "\"]");
						blnAdd = objXmlNodeList.Count > 0;
					}

					if (blnAdd)
					{
						ListItem objItem = new ListItem();
						objItem.Value = objXmlSkill.InnerText;
						if (objXmlSkill.Attributes != null)
						{
							if (objXmlSkill.Attributes["translate"] != null)
								objItem.Name = objXmlSkill.Attributes["translate"].InnerText;
							else
								objItem.Name = objXmlSkill.InnerText;
						}
						else
							objItem.Name = objXmlSkill.InnerXml;
						lstGroups.Add(objItem);
					}
				}
			}
			else
			{
				ListItem objItem = new ListItem();
				objItem.Value = _strForceValue;
				objItem.Name = _strForceValue;
				lstGroups.Add(objItem);
			}
			SortListItem objSort = new SortListItem();
			lstGroups.Sort(objSort.Compare);
			cboSkillGroup.ValueMember = "Value";
			cboSkillGroup.DisplayMember = "Name";
			cboSkillGroup.DataSource = lstGroups;

            // Select the first Skill in the list.
            cboSkillGroup.SelectedIndex = 0;

			if (cboSkillGroup.Items.Count == 1)
				cmdOK_Click(sender, e);
        }

		private void cmdOK_Click(object sender, EventArgs e)
        {
            _strReturnValue = cboSkillGroup.SelectedValue.ToString();
            this.DialogResult = DialogResult.OK;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
		#endregion

		#region Properties
		// Skill Group that was selected in the dialogue.
        public string SelectedSkillGroup
        {
            get
            {
                return _strReturnValue;
            }
        }

        // Description to show in the window.
        public string Description
        {
            set
            {
                lblDescription.Text = value;
            }
        }

		/// <summary>
		/// Force a specific SkillGroup to be selected.
		/// </summary>
		public string OnlyGroup
		{
			set
			{
				_strForceValue = value;
			}
		}

		/// <summary>
		/// Only Skills not in the selected Category should be in the list.
		/// </summary>
		public string ExcludeCategory
		{
			set
			{
				_strExcludeCategory = value;
			}
		}
		#endregion
    }
}