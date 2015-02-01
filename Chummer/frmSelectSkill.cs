using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    public partial class frmSelectSkill : Form
    {
        private string _strReturnValue = "";
        private string _strIncludeCategory = "";
        private string _strExcludeCategory = "";
		private string _strIncludeSkillGroup = "";
		private string _strExcludeSkillGroup = "";
		private string _strLimitToSkill = "";
		private string _strForceSkill = "";
		private bool _blnKnowledgeSkill = false;

		private XmlDocument _objXmlDocument = new XmlDocument();
		private readonly Character _objCharacter;

		#region Control Events
		public frmSelectSkill(Character objCharacter)
        {
            InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
			_objCharacter = objCharacter;
        }

        private void frmSelectSkill_Load(object sender, EventArgs e)
        {
			List<ListItem> lstSkills = new List<ListItem>();

			if (!_blnKnowledgeSkill)
			{
				_objXmlDocument = XmlManager.Instance.Load("skills.xml");

				// Build the list of non-Exotic Skills from the Skills file.
				XmlNodeList objXmlSkillList;
				if (_strForceSkill != "")
				{
					objXmlSkillList = _objXmlDocument.SelectNodes("/chummer/skills/skill[name = \"" + _strForceSkill + "\" and not(exotic)]");
				}
				else
				{
					if (_strIncludeCategory != "")
						objXmlSkillList = _objXmlDocument.SelectNodes("/chummer/skills/skill[category = \"" + _strIncludeCategory + "\" and not(exotic)]");
					else if (_strExcludeCategory != "")
					{
						string[] strExcludes = _strExcludeCategory.Split(',');
						string strExclude = "";
						for (int i = 0; i <= strExcludes.Length - 1; i++)
							strExclude += "category != \"" + strExcludes[i].Trim() + "\" and ";
						// Remove the trailing " and ";
						strExclude = strExclude.Substring(0, strExclude.Length - 5);
						objXmlSkillList = _objXmlDocument.SelectNodes("/chummer/skills/skill[" + strExclude + " and not(exotic)]");
					}
					else if (_strIncludeSkillGroup != "")
						objXmlSkillList = _objXmlDocument.SelectNodes("/chummer/skills/skill[skillgroup = \"" + _strIncludeSkillGroup + "\" and not(exotic)]");
					else if (_strExcludeSkillGroup != "")
						objXmlSkillList = _objXmlDocument.SelectNodes("/chummer/skills/skill[skillgroup != \"" + _strExcludeSkillGroup + "\" and not(exotic)]");
					else if (_strLimitToSkill != "")
					{
						string strFilter = "not(exotic) and (";
						string[] strValue = _strLimitToSkill.Split(',');
						foreach (string strSkill in strValue)
							strFilter += "name = \"" + strSkill.Trim() + "\" or ";
						// Remove the trailing " or ".
						strFilter = strFilter.Substring(0, strFilter.Length - 4);
						strFilter += ")";
						objXmlSkillList = _objXmlDocument.SelectNodes("/chummer/skills/skill[" + strFilter + "]");
					}
					else
						objXmlSkillList = _objXmlDocument.SelectNodes("/chummer/skills/skill[not(exotic)]");
				}

				// Add the Skills to the list.
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

				// Add in any Exotic Skills the character has.
				foreach (Skill objSkill in _objCharacter.Skills)
				{
					if (objSkill.ExoticSkill)
					{
						bool blnAddSkill = true;
						if (_strForceSkill != "")
							blnAddSkill = _strForceSkill == objSkill.Name + " (" + objSkill.Specialization + ")";
						else
						{
							if (_strIncludeCategory != "")
								blnAddSkill = _strIncludeCategory == objSkill.SkillCategory;
							else if (_strExcludeCategory != "")
								blnAddSkill = !_strExcludeCategory.Contains(objSkill.SkillCategory);
							else if (_strIncludeSkillGroup != "")
								blnAddSkill = _strIncludeSkillGroup == objSkill.SkillGroup;
							else if (_strExcludeSkillGroup != "")
								blnAddSkill = _strExcludeSkillGroup != objSkill.SkillGroup;
							else if (_strLimitToSkill != "")
								blnAddSkill = _strLimitToSkill.Contains(objSkill.Name);
						}

						if (blnAddSkill)
						{
							ListItem objItem = new ListItem();
							objItem.Value = objSkill.Name + " (" + objSkill.Specialization + ")";
							// Use the translated Exotic Skill name if available.
							XmlNode objXmlSkill = _objXmlDocument.SelectSingleNode("/chummer/skills/skill[exotic = \"Yes\" and name = \"" + objSkill.Name + "\"]");
							if (objXmlSkill["translate"] != null)
								objItem.Name = objXmlSkill["translate"].InnerText + " (" + objSkill.Specialization + ")";
							else
								objItem.Name = objSkill.Name + " (" + objSkill.Specialization + ")";
							lstSkills.Add(objItem);
						}
					}
				}
			}
			else
			{
				// Instead of showing all available Active Skills, show a list of Knowledge Skills that the character currently has.
				foreach (Skill objKnow in _objCharacter.Skills)
				{
					if (objKnow.KnowledgeSkill)
					{
						ListItem objSkill = new ListItem();
						objSkill.Value = objKnow.Name;
						objSkill.Name = objKnow.DisplayName;
						lstSkills.Add(objSkill);
					}
				}
			}
			SortListItem objSort = new SortListItem();
			lstSkills.Sort(objSort.Compare);
			cboSkill.ValueMember = "Value";
			cboSkill.DisplayMember = "Name";
			cboSkill.DataSource = lstSkills;

            // Select the first Skill in the list.
            cboSkill.SelectedIndex = 0;

			if (cboSkill.Items.Count == 1)
				cmdOK_Click(sender, e);
        }

		private void cmdOK_Click(object sender, EventArgs e)
        {
            _strReturnValue = cboSkill.SelectedValue.ToString();
            this.DialogResult = DialogResult.OK;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
		#endregion

		#region Properties
		/// <summary>
		/// Only Skills of the selected Category should be in the list.
		/// </summary>
        public string OnlyCategory
        {
            set
            {
                _strIncludeCategory = value;
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

		/// <summary>
		/// Only Skills in the selected Skill Group should be in the list.
		/// </summary>
		public string OnlySkillGroup
		{
			set
			{
				_strIncludeSkillGroup = value;
			}
		}

		/// <summary>
		/// Restrict the list to only a single Skill.
		/// </summary>
		public string OnlySkill
		{
			set
			{
				_strForceSkill = value.Replace(", " + LanguageManager.Instance.GetString("Label_SelectGear_Hacked"), string.Empty);
			}
		}

		/// <summary>
		/// Only Skills not in the selected Skill Group should be in the list.
		/// </summary>
		public string ExcludeSkillGroup
		{
			set
			{
				_strExcludeSkillGroup = value;
			}
		}

		/// <summary>
		/// Only the provided Skills should be shown in the list.
		/// </summary>
		public string LimitToSkill
		{
			set
			{
				_strLimitToSkill = value;
			}
		}

		/// <summary>
		/// Skill that was selected in the dialogue.
		/// </summary>
        public string SelectedSkill
        {
            get
            {
                return _strReturnValue;
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
		/// Whether or not Knowledge Skills should be shown instead.
		/// </summary>
		public bool ShowKnowledgeSkills
		{
			set
			{
				_blnKnowledgeSkill = value;
			}
		}
		#endregion
    }
}