using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
	public partial class frmCreateImprovement : Form
	{
		private readonly Character _objCharacter;
		private XmlDocument _objDocument = new XmlDocument();
		private string _strSelect = "";
		private Improvement _objEditImprovement;

		#region Control Events
		public frmCreateImprovement(Character objCharacter)
		{
			InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
			_objCharacter = objCharacter;
			MoveControls();
		}

		private void frmCreateImprovement_Load(object sender, EventArgs e)
		{
			List<ListItem> lstTypes = new List<ListItem>();
			_objDocument = XmlManager.Instance.Load("improvements.xml");

			// Populate the Improvement Type list.
			XmlNodeList objXmlImprovementList = _objDocument.SelectNodes("/chummer/improvements/improvement");
			foreach (XmlNode objXmlImprovement in objXmlImprovementList)
			{
				ListItem objItem = new ListItem();
				objItem.Value = objXmlImprovement["id"].InnerText;
				if (objXmlImprovement["translate"] != null)
					objItem.Name = objXmlImprovement["translate"].InnerText;
				else
					objItem.Name = objXmlImprovement["name"].InnerText;
				lstTypes.Add(objItem);
			}

			SortListItem objSort = new SortListItem();
			lstTypes.Sort(objSort.Compare);
			cboImprovemetType.ValueMember = "Value";
			cboImprovemetType.DisplayMember = "Name";
			cboImprovemetType.DataSource = lstTypes;

			// Load the information from the passed Improvement if one has been given.
			if (_objEditImprovement != null)
			{
				cboImprovemetType.SelectedValue = _objEditImprovement.CustomId;
				txtName.Text = _objEditImprovement.CustomName;
				if (nudMax.Visible)
					nudMax.Value = _objEditImprovement.Maximum;
				if (nudMin.Visible)
					nudMin.Value = _objEditImprovement.Minimum;
				if (nudVal.Visible)
				{
					// specificattribute stores the Value in Augmented instead.
					if (_objEditImprovement.CustomId == "specificattribute")
						nudVal.Value = _objEditImprovement.Augmented;
					else
						nudVal.Value = _objEditImprovement.Value;
				}
				if (chkApplyToRating.Visible)
					chkApplyToRating.Checked = _objEditImprovement.AddToRating;
				else
					chkApplyToRating.Checked = false;
				if (txtSelect.Visible)
					txtSelect.Text = _objEditImprovement.ImprovedName;
			}
		}

		private void cmdOK_Click(object sender, EventArgs e)
		{
			AcceptForm();
		}

		private void cmdCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}

		private void cboImprovemetType_SelectedIndexChanged(object sender, EventArgs e)
		{
			XmlNode objFetchNode = _objDocument.SelectSingleNode("/chummer/improvements/improvement[id = \"" + cboImprovemetType.SelectedValue + "\"]");

			lblVal.Visible = false;
			lblMin.Visible = false;
			lblMax.Visible = false;
			lblAug.Visible = false;
			nudVal.Visible = false;
			nudMin.Visible = false;
			nudMax.Visible = false;
			nudAug.Visible = false;
			chkApplyToRating.Visible = false;
			chkApplyToRating.Checked = false;

			lblSelect.Visible = false;
			txtSelect.Visible = false;
			txtSelect.Text = "";
			cmdChangeSelection.Visible = false;
			_strSelect = "";

			foreach (XmlNode objNode in objFetchNode.SelectNodes("fields/field"))
			{
				if (objNode.InnerText == "val")
				{
					lblVal.Visible = true;
					nudVal.Visible = true;
				}
				if (objNode.InnerText == "min")
				{
					lblMin.Visible = true;
					nudMin.Visible = true;
				}
				if (objNode.InnerText == "max")
				{
					lblMax.Visible = true;
					nudMax.Visible = true;
				}
				if (objNode.InnerText == "aug")
				{
					lblAug.Visible = true;
					nudAug.Visible = true;
				}
				if (objNode.InnerText == "applytorating")
				{
					chkApplyToRating.Visible = true;
				}
				if (objNode.InnerText.StartsWith("Select"))
				{
					lblSelect.Visible = true;
					txtSelect.Visible = true;
					cmdChangeSelection.Visible = true;
					_strSelect = objNode.InnerText;
				}
			}

			// Display the help information.
			if (objFetchNode["altpage"] != null)
				lblHelp.Text = objFetchNode["altpage"].InnerText;
			else
				lblHelp.Text = objFetchNode["page"].InnerText;
		}

		private void cmdChangeSelection_Click(object sender, EventArgs e)
		{
			if (_strSelect == "SelectAttribute")
			{
				frmSelectAttribute frmPickAttribute = new frmSelectAttribute();
				frmPickAttribute.Description = LanguageManager.Instance.GetString("Title_SelectAttribute");
				if (_objCharacter.MAGEnabled)
					frmPickAttribute.AddMAG();
				if (_objCharacter.RESEnabled)
					frmPickAttribute.AddRES();
				frmPickAttribute.ShowDialog(this);

				if (frmPickAttribute.DialogResult == DialogResult.OK)
					txtSelect.Text = frmPickAttribute.SelectedAttribute;
			}
			if (_strSelect == "SelectPhysicalAttribute")
			{
				frmSelectAttribute frmPickAttribute = new frmSelectAttribute();
				frmPickAttribute.Description = LanguageManager.Instance.GetString("Title_SelectAttribute");

				List<string> strValue = new List<string>();
				strValue.Add("LOG");
				strValue.Add("WIL");
				strValue.Add("INT");
				strValue.Add("CHA");
				strValue.Add("EDG");
				strValue.Add("MAG");
				strValue.Add("RES");
				frmPickAttribute.RemoveFromList(strValue);

				frmPickAttribute.ShowDialog(this);

				if (frmPickAttribute.DialogResult == DialogResult.OK)
					txtSelect.Text = frmPickAttribute.SelectedAttribute;
			}
			if (_strSelect == "SelectMentalAttribute")
			{
				frmSelectAttribute frmPickAttribute = new frmSelectAttribute();
				frmPickAttribute.Description = LanguageManager.Instance.GetString("Title_SelectAttribute");

				List<string> strValue = new List<string>();
				strValue.Add("BOD");
				strValue.Add("AGI");
				strValue.Add("REA");
				strValue.Add("STR");
				strValue.Add("EDG");
				strValue.Add("MAG");
				strValue.Add("RES");
				frmPickAttribute.RemoveFromList(strValue);

				frmPickAttribute.ShowDialog(this);

				if (frmPickAttribute.DialogResult == DialogResult.OK)
					txtSelect.Text = frmPickAttribute.SelectedAttribute;
			}
			if (_strSelect == "SelectSkill")
			{
				frmSelectSkill frmPickSkill = new frmSelectSkill(_objCharacter);
				frmPickSkill.Description = LanguageManager.Instance.GetString("Title_SelectSkill");
				frmPickSkill.ShowDialog(this);

				if (frmPickSkill.DialogResult == DialogResult.OK)
					txtSelect.Text = frmPickSkill.SelectedSkill;
			}
			if (_strSelect == "SelectKnowSkill")
			{
				frmSelectSkill frmPickSkill = new frmSelectSkill(_objCharacter);
				frmPickSkill.ShowKnowledgeSkills = true;
				frmPickSkill.Description = LanguageManager.Instance.GetString("Title_SelectSkill");
				frmPickSkill.ShowDialog(this);

				if (frmPickSkill.DialogResult == DialogResult.OK)
					txtSelect.Text = frmPickSkill.SelectedSkill;
			}
			if (_strSelect == "SelectSkillCategory")
			{
				frmSelectSkillCategory frmPickSkillCategory = new frmSelectSkillCategory();
				frmPickSkillCategory.Description = LanguageManager.Instance.GetString("Title_SelectSkillCategory");
				frmPickSkillCategory.ShowDialog(this);

				if (frmPickSkillCategory.DialogResult == DialogResult.OK)
					txtSelect.Text = frmPickSkillCategory.SelectedCategory;
			}
			if (_strSelect == "SelectSkillGroup")
			{
				frmSelectSkillGroup frmPickSkillGroup = new frmSelectSkillGroup();
				frmPickSkillGroup.Description = LanguageManager.Instance.GetString("Title_SelectSkillGroup");
				frmPickSkillGroup.ShowDialog(this);

				if (frmPickSkillGroup.DialogResult == DialogResult.OK)
					txtSelect.Text = frmPickSkillGroup.SelectedSkillGroup;
			}
			if (_strSelect == "SelectWeaponCategory")
			{
				frmSelectWeaponCategory frmPickWeaponCategory = new frmSelectWeaponCategory();
				frmPickWeaponCategory.Description = LanguageManager.Instance.GetString("Title_SelectWeaponCategory");
				frmPickWeaponCategory.ShowDialog(this);

				if (frmPickWeaponCategory.DialogResult == DialogResult.OK)
					txtSelect.Text = frmPickWeaponCategory.SelectedCategory;
			}
			if (_strSelect == "SelectSpellCategory")
			{
				frmSelectSpellCategory frmPickSpellCategory = new frmSelectSpellCategory();
				frmPickSpellCategory.Description = LanguageManager.Instance.GetString("Title_SelectSpellCategory");
				frmPickSpellCategory.ShowDialog(this);

				if (frmPickSpellCategory.DialogResult == DialogResult.OK)
					txtSelect.Text = frmPickSpellCategory.SelectedCategory;
			}
		}
		#endregion

		#region Methods
		/// <summary>
		/// Accept the values on the Form and create the required XML data.
		/// </summary>
		private void AcceptForm()
		{
			// Make sure a value has been selected if necessary.
			if (txtSelect.Visible && txtSelect.Text == string.Empty)
			{
				MessageBox.Show(LanguageManager.Instance.GetString("Message_SelectItem"), LanguageManager.Instance.GetString("MessageTitle_SelectItem"), MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// Make sure a value has been provided for the name.
			if (txtName.Text == string.Empty)
			{
				MessageBox.Show(LanguageManager.Instance.GetString("Message_ImprovementName"), LanguageManager.Instance.GetString("MessageTitle_ImprovementName"), MessageBoxButtons.OK, MessageBoxIcon.Error);
				txtName.Focus();
				return;
			}

			MemoryStream objStream = new MemoryStream();
			XmlWriter objWriter = XmlWriter.Create(objStream);

			// Build the XML for the Improvement.
			XmlNode objFetchNode = _objDocument.SelectSingleNode("/chummer/improvements/improvement[id = \"" + cboImprovemetType.SelectedValue + "\"]");
			objWriter.WriteStartDocument();
			// <bonus>
			objWriter.WriteStartElement("bonus");
			// <whatever element>
			objWriter.WriteStartElement(objFetchNode["internal"].InnerText);

			string strRating = "";
			if (chkApplyToRating.Checked)
				strRating = "<applytorating>yes</applytorating>";

			// Retrieve the XML data from the document and replace the values as necessary.
			string strXml = objFetchNode["xml"].InnerText;
			strXml = strXml.Replace("{val}", nudVal.Value.ToString());
			strXml = strXml.Replace("{min}", nudMin.Value.ToString());
			strXml = strXml.Replace("{max}", nudMax.Value.ToString());
			strXml = strXml.Replace("{aug}", nudAug.Value.ToString());
			strXml = strXml.Replace("{select}", txtSelect.Text);
			strXml = strXml.Replace("{applytorating}", strRating);
			objWriter.WriteRaw(strXml);

			// Write the rest of the document.
			// </whatever element>
			objWriter.WriteEndElement();
			// </bonus>
			objWriter.WriteEndElement();
			objWriter.WriteEndDocument();
			objWriter.Flush();
			objStream.Flush();

			objStream.Position = 0;

			// Read it back in as an XmlDocument.
			StreamReader objReader = new StreamReader(objStream);
			XmlDocument objBonusXML = new XmlDocument();
			string strXML = objReader.ReadToEnd();
			objBonusXML.LoadXml(strXML);

			objWriter.Close();
			objStream.Close();

			// Pluck out the bonus information.
			XmlNode objNode = objBonusXML.SelectSingleNode("/bonus");

			// Pass it to the Improvement Manager so that it can be added to the character.
			ImprovementManager objImprovementManager = new ImprovementManager(_objCharacter);
			string strGuid = Guid.NewGuid().ToString();
			objImprovementManager.CreateImprovements(Improvement.ImprovementSource.Custom, strGuid, objNode, false, 1, txtName.Text);

			// If an Improvement was passed in, remove it from the character.
			string strNotes = "";
			int intOrder = 0;
			if (_objEditImprovement != null)
			{
				// Copy the notes over to the new item.
				strNotes = _objEditImprovement.Notes;
				intOrder = _objEditImprovement.SortOrder;
				objImprovementManager.RemoveImprovements(Improvement.ImprovementSource.Custom, _objEditImprovement.SourceName);
			}

			// Find the newly-created Improvement and attach its custom name.
			foreach (Improvement objImprovement in _objCharacter.Improvements)
			{
				if (objImprovement.SourceName == strGuid)
				{
					objImprovement.CustomName = txtName.Text;
					objImprovement.CustomId = cboImprovemetType.SelectedValue.ToString();
					objImprovement.Custom = true;
					objImprovement.Notes = strNotes;
					objImprovement.SortOrder = intOrder;
				}
			}

			this.DialogResult = DialogResult.OK;
		}

		private void MoveControls()
		{
			int intWidth = 0;
			intWidth = Math.Max(lblImprovementType.Width, lblName.Width);
			intWidth = Math.Max(intWidth, lblSelect.Width);
			intWidth = Math.Max(intWidth, lblVal.Width);
			intWidth = Math.Max(intWidth, lblMin.Width);
			intWidth = Math.Max(intWidth, lblMax.Width);
			intWidth = Math.Max(intWidth, lblAug.Width);

			cboImprovemetType.Left = lblImprovementType.Left + intWidth + 6;
			txtName.Left = cboImprovemetType.Left;
			txtSelect.Left = cboImprovemetType.Left;
			cmdChangeSelection.Left = txtSelect.Left + txtSelect.Width + 6;
			nudVal.Left = cboImprovemetType.Left;
			nudMin.Left = cboImprovemetType.Left;
			nudMax.Left = cboImprovemetType.Left;
			nudAug.Left = cboImprovemetType.Left;
			chkApplyToRating.Left = nudVal.Left + nudVal.Width + 6;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Improvement object to edit.
		/// </summary>
		public Improvement EditImprovementObject
		{
			set
			{
				_objEditImprovement = value;
			}
		}
		#endregion
	}
}