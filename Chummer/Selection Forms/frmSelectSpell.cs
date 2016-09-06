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
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    public partial class frmSelectSpell : Form
    {
        private string _strSelectedSpell = "";

		private bool _blnAddAgain = false;
	    private bool _blnIgnoreRequirements = false;
		private string _strLimitCategory = "";
		private string _strForceSpell = "";
		private List<TreeNode> _lstExpandCategories;

		private XmlDocument _objXmlDocument = new XmlDocument();
		private readonly Character _objCharacter;

		#region Control Events
		public frmSelectSpell(Character objCharacter)
        {
            InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
			_objCharacter = objCharacter;

			tipTooltip.SetToolTip(chkLimited, LanguageManager.Instance.GetString("Tip_SelectSpell_LimitedSpell"));
			tipTooltip.SetToolTip(chkExtended, LanguageManager.Instance.GetString("Tip_SelectSpell_ExtendedSpell"));

			MoveControls();
        }

        private void frmSelectSpell_Load(object sender, EventArgs e)
        {
			// If a value is forced, set the name of the spell and accept the form.
			if (_strForceSpell != "")
			{
				_strSelectedSpell = _strForceSpell;
				this.DialogResult = DialogResult.OK;
			}

			foreach (Label objLabel in this.Controls.OfType<Label>())
			{
				if (objLabel.Text.StartsWith("["))
					objLabel.Text = "";
			}

        	// Load the Spells information.
			_objXmlDocument = XmlManager.Instance.Load("spells.xml");

			// Populate the Category list.
			XmlNodeList objXmlNodeList = _objXmlDocument.SelectNodes("/chummer/categories/category");
			foreach (XmlNode objXmlCategory in objXmlNodeList)
			{
				if (_strLimitCategory == "" || _strLimitCategory == objXmlCategory.InnerText)
				{
					TreeNode nodCategory = new TreeNode();
					nodCategory.Tag = objXmlCategory.InnerText;
					if (objXmlCategory.Attributes["translate"] != null)
						nodCategory.Text = objXmlCategory.Attributes["translate"].InnerText;
					else
						nodCategory.Text = objXmlCategory.InnerText;

					treSpells.Nodes.Add(nodCategory);
				}
			}

			// Don't show the Extended Spell checkbox if the option to Extend any Detection Spell is diabled.
			chkExtended.Visible = _objCharacter.Options.ExtendAnyDetectionSpell;
			string strAdditionalFilter = "";
			if (_objCharacter.Options.ExtendAnyDetectionSpell)
				strAdditionalFilter = "not(contains(name, \", Extended\"))";

            // Populate the Spell list.
			if (_strLimitCategory != "")
				objXmlNodeList = _objXmlDocument.SelectNodes("/chummer/spells/spell[category = \"" + _strLimitCategory + "\" and " + strAdditionalFilter + " and (" + _objCharacter.Options.BookXPath() + ")]");
			else
			{
				if (strAdditionalFilter == string.Empty)
					objXmlNodeList = _objXmlDocument.SelectNodes("/chummer/spells/spell[" + _objCharacter.Options.BookXPath() + "]");
				else
					objXmlNodeList = _objXmlDocument.SelectNodes("/chummer/spells/spell[" + strAdditionalFilter + " and (" + _objCharacter.Options.BookXPath() + ")]");
			}

			treSpells.TreeViewNodeSorter = new SortByName();
            treSpells.SelectedNode = treSpells.Nodes[0];
            foreach (XmlNode objXmlSpell in objXmlNodeList)
            {
                TreeNode nodSpell = new TreeNode();
                TreeNode nodParent = new TreeNode();
                bool blnInclude = false;

	            if (_blnIgnoreRequirements)
	            {
		            blnInclude = true;
	            }
	            else if (_objCharacter.AdeptEnabled && !_objCharacter.MagicianEnabled)
                {
                    if (objXmlSpell["category"].InnerText != "Rituals")
                        blnInclude = false;
                    else if (objXmlSpell["descriptor"].InnerText.Contains("Spell"))
                        blnInclude = false;
                    else
                        blnInclude = true;
                }
                else if (!_objCharacter.AdeptEnabled)
                {
                    if (objXmlSpell["descriptor"].InnerText.Contains("Adept"))
                        blnInclude = false;
                    else
                        blnInclude = true;
                }
                else
                    blnInclude = true;

                // Art requirements.
                bool blnStreetGrimoire = (_objCharacter.Options.Books.Contains("SG"));
                if (blnStreetGrimoire && !_objCharacter.Options.IgnoreArt)
                {
                    foreach (XmlNode objXmlArt in objXmlSpell.SelectNodes("required/allof/art"))
                    {
                        bool blnFound = false;
                        foreach (Art objArt in _objCharacter.Arts)
                        {
                            if (objArt.Name == objXmlArt.InnerText)
                            {
                                blnFound = true;
                                break;
                            }
                        }
                        blnInclude = blnInclude && blnFound;
                    }
                }

                if (blnInclude)
                {
                    if (objXmlSpell["translate"] != null)
                        nodSpell.Text = objXmlSpell["translate"].InnerText;
                    else
                        nodSpell.Text = objXmlSpell["name"].InnerText;
                    nodSpell.Tag = objXmlSpell["name"].InnerText;
                    // Check to see if there is already a Category node for the Spell's category.
                    foreach (TreeNode nodCategory in treSpells.Nodes)
                    {
                        if (nodCategory.Level == 0 && nodCategory.Tag.ToString() == objXmlSpell["category"].InnerText)
                        {
                            nodParent = nodCategory;
                        }
                    }

                    // Add the Spell to the Category node.
                    nodParent.Nodes.Add(nodSpell);

                    if (_strLimitCategory != "")
                        nodParent.Expand();
                }
            }

			if (_lstExpandCategories != null)
			{
				foreach (TreeNode objExpandedNode in _lstExpandCategories)
				{
					foreach (TreeNode objNode in treSpells.Nodes)
					{
						if (objNode.Text == objExpandedNode.Text)
						{
							objNode.Expand();
						}
					}
				}
			}

			if (_strLimitCategory != "")
				txtSearch.Enabled = false;
        }

        private void treSpells_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Only attempt to retrieve Spell information if a child node is selected.
            if (treSpells.SelectedNode.Level > 0)
            {
            	// Display the Spell information.
                XmlNode objXmlSpell = _objXmlDocument.SelectSingleNode("/chummer/spells/spell[name = \"" + treSpells.SelectedNode.Tag + "\"]");

				string[] strDescriptorsIn = objXmlSpell["descriptor"].InnerText.Split(',');
				
				string strDescriptors = "";
				bool blnExtendedFound = false;
				foreach (string strDescriptor in strDescriptorsIn)
				{
					switch (strDescriptor.Trim())
					{
						case "Adept":
							strDescriptors += LanguageManager.Instance.GetString("String_DescAdept") + ", ";
							break;
						case "Anchored":
							strDescriptors += LanguageManager.Instance.GetString("String_DescAnchored") + ", ";
							break;
						case "Blood":
							strDescriptors += LanguageManager.Instance.GetString("String_DescBlood") + ", ";
							break;
						case "Contractual":
							strDescriptors += LanguageManager.Instance.GetString("String_DescContractual") + ", ";
							break;
						case "Geomancy":
							strDescriptors += LanguageManager.Instance.GetString("String_DescGeomancy") + ", ";
							break;
						case "Mana":
							strDescriptors += LanguageManager.Instance.GetString("String_DescMana") + ", ";
							break;
						case "Material Link":
							strDescriptors += LanguageManager.Instance.GetString("String_DescMaterialLink") + ", ";
							break;
						case "Minion":
							strDescriptors += LanguageManager.Instance.GetString("String_DescMinion") + ", ";
							break;
						case "Organic Link":
							strDescriptors += LanguageManager.Instance.GetString("String_DescOrganicLink") + ", ";
							break;
						case "Spell":
							strDescriptors += LanguageManager.Instance.GetString("String_DescSpell") + ", ";
							break;
						case "Spotter":
							strDescriptors += LanguageManager.Instance.GetString("String_DescSpotter") + ", ";
							break;
					}
				}

				// If Extended Area was not found and the Extended checkbox is checked, add Extended Area to the list of Descriptors.
				if (chkExtended.Checked && !blnExtendedFound)
					strDescriptors += LanguageManager.Instance.GetString("String_DescExtendedArea") + ", ";

                if (chkAlchemical.Checked && !blnExtendedFound)
                    strDescriptors += LanguageManager.Instance.GetString("String_DescAlchemicalPreparation") + ", ";

                // Remove the trailing comma.
				if (strDescriptors != string.Empty)
					strDescriptors = strDescriptors.Substring(0, strDescriptors.Length - 2);
				lblDescriptors.Text = strDescriptors;

				switch (objXmlSpell["type"].InnerText)
				{
					case "M":
						lblType.Text = LanguageManager.Instance.GetString("String_SpellTypeMana");
						break;
					default:
						lblType.Text = LanguageManager.Instance.GetString("String_SpellTypePhysical");
						break;
				}

				switch (objXmlSpell["duration"].InnerText)
				{
					case "P":
						lblDuration.Text = LanguageManager.Instance.GetString("String_SpellDurationPermanent");
						break;
					case "S":
						lblDuration.Text = LanguageManager.Instance.GetString("String_SpellDurationSustained");
						break;
					default:
						lblDuration.Text = LanguageManager.Instance.GetString("String_SpellDurationInstant");
						break;
				}

				if (objXmlSpell["category"].InnerText == "Detection")
				{
					chkExtended.Enabled = true;
				}
				else
				{
					chkExtended.Checked = false;
					chkExtended.Enabled = false;
				}

				string strRange = objXmlSpell["range"].InnerText;
				strRange = strRange.Replace("Self", LanguageManager.Instance.GetString("String_SpellRangeSelf"));
				strRange = strRange.Replace("LOS", LanguageManager.Instance.GetString("String_SpellRangeLineOfSight"));
				strRange = strRange.Replace("LOI", LanguageManager.Instance.GetString("String_SpellRangeLineOfInfluence"));
				strRange = strRange.Replace("T", LanguageManager.Instance.GetString("String_SpellRangeTouch"));
				strRange = strRange.Replace("(A)", "(" + LanguageManager.Instance.GetString("String_SpellRangeArea") + ")");
				strRange = strRange.Replace("MAG", LanguageManager.Instance.GetString("String_AttributeMAGShort"));
				lblRange.Text = strRange;

				switch (objXmlSpell["damage"].InnerText)
				{
					case "P":
						lblDamage.Text = LanguageManager.Instance.GetString("String_DamagePhysical");
						break;
					case "S":
						lblDamage.Text = LanguageManager.Instance.GetString("String_DamageStun");
						break;
					default:
						lblDamage.Text = "";
						break;
				}

				string strDV = objXmlSpell["dv"].InnerText.Replace("/", "÷").Replace("F", LanguageManager.Instance.GetString("String_SpellForce"));
				strDV = strDV.Replace("Overflow damage", LanguageManager.Instance.GetString("String_SpellOverflowDamage"));
				strDV = strDV.Replace("Damage Value", LanguageManager.Instance.GetString("String_SpellDamageValue"));
				strDV = strDV.Replace("Toxin DV", LanguageManager.Instance.GetString("String_SpellToxinDV"));
				strDV = strDV.Replace("Disease DV", LanguageManager.Instance.GetString("String_SpellDiseaseDV"));
				strDV = strDV.Replace("Radiation Power", LanguageManager.Instance.GetString("String_SpellRadiationPower"));

				if (chkExtended.Checked)
				{
					// Add +2 to the DV value if Extended is selected.
					int intPos = strDV.IndexOf(')') + 1;
					string strAfter = strDV.Substring(intPos, strDV.Length - intPos);
					strDV = strDV.Remove(intPos, strDV.Length - intPos);
					if (strAfter == string.Empty)
						strAfter = "+2";
					else
					{
						int intValue = Convert.ToInt32(strAfter) + 2;
						if (intValue == 0)
							strAfter = "";
						else if (intValue > 0)
							strAfter = "+" + intValue.ToString();
						else
							strAfter = intValue.ToString();
					}
					strDV += strAfter;
				}

                if (chkLimited.Checked)
                {
                    int intPos = 0;
                    if (strDV.Contains("-"))
                    {
                        intPos = strDV.IndexOf("-") + 1;
                        string strAfter = strDV.Substring(intPos, strDV.Length - intPos);
                        strDV = strDV.Substring(0, intPos);
                        int intAfter = Convert.ToInt32(strAfter);
                        intAfter += 2;
                        strDV += intAfter.ToString();
                    }
                    else if (strDV.Contains("+"))
                    {
                        intPos = strDV.IndexOf("-");
                        string strAfter = strDV.Substring(intPos, strDV.Length - intPos);
                        strDV = strDV.Substring(0, intPos);
                        int intAfter = Convert.ToInt32(strAfter);
                        intAfter -= 2;
                        if (intAfter > 0)
                            strDV += "+" + intAfter.ToString();
                        else if (intAfter < 0)
                            strDV += intAfter.ToString();
                    }
                    else
                    {
                        strDV += "-2";
                    }
                }

				lblDV.Text = strDV;

				string strBook = _objCharacter.Options.LanguageBookShort(objXmlSpell["source"].InnerText);
				string strPage = objXmlSpell["page"].InnerText;
				if (objXmlSpell["altpage"] != null)
					strPage = objXmlSpell["altpage"].InnerText;
				lblSource.Text = strBook + " " + strPage;

				tipTooltip.SetToolTip(lblSource, _objCharacter.Options.LanguageBookLong(objXmlSpell["source"].InnerText) + " " + LanguageManager.Instance.GetString("String_Page") + " " + strPage);
            }
        }

		private void cmdOK_Click(object sender, EventArgs e)
		{
			if (!_blnAddAgain)
			{
				_lstExpandCategories = null;
			}

			if (treSpells.SelectedNode != null)
			{
				if (treSpells.SelectedNode.Level == 0)
				{
					return;
				}
				else
				{
					// Display the Spell information.
					XmlNode objXmlSpell = _objXmlDocument.SelectSingleNode("/chummer/spells/spell[name = \"" + treSpells.SelectedNode.Tag + "\"]");
					// Count the number of Spells the character currently has and make sure they do not try to select more Spells than they are allowed.
					// The maximum number of Spells a character can start with is 2 x (highest of Spellcasting or Ritual Spellcasting Skill).
					int intSpellCount = 0;
					int intRitualCount = 0;
					int intAlchPrepCount = 0;
					int intSpellLimit = 0;

					foreach (Spell objspell in _objCharacter.Spells)
					{
						if (objspell.Alchemical)
						{ intAlchPrepCount++; }
						else if (objspell.Category == "Rituals")
						{ intRitualCount++; }
						else
						{ intSpellCount++; }
					}
					if (!_objCharacter.IgnoreRules)
					{
						intSpellLimit = (_objCharacter.MAG.TotalValue * 2);
						if (chkAlchemical.Checked && (intAlchPrepCount >= intSpellLimit) && !_objCharacter.Created)
						{

							MessageBox.Show(LanguageManager.Instance.GetString("Message_SpellLimit"), LanguageManager.Instance.GetString("MessageTitle_SpellLimit"), MessageBoxButtons.OK, MessageBoxIcon.Information);
							return;
						}
						else if (objXmlSpell["category"].InnerText == "Rituals" && (intRitualCount >= intSpellLimit) && !_objCharacter.Created)
						{
							MessageBox.Show(LanguageManager.Instance.GetString("Message_SpellLimit"), LanguageManager.Instance.GetString("MessageTitle_SpellLimit"), MessageBoxButtons.OK, MessageBoxIcon.Information);
							return;
						}
						else if (intSpellCount >= intSpellLimit && !_objCharacter.Created)
						{
							MessageBox.Show(LanguageManager.Instance.GetString("Message_SpellLimit"),
								LanguageManager.Instance.GetString("MessageTitle_SpellLimit"), MessageBoxButtons.OK, MessageBoxIcon.Information);
							return;

						}
					}
				}
				try
				{
					if (treSpells.SelectedNode.Level > 0)
						AcceptForm();
				}
				catch
				{
				}
			}
		}

        private void treSpells_DoubleClick(object sender, EventArgs e)
        {
	        if (treSpells.SelectedNode.Level > 0)
	        {
				this.ExpandedCategories = null;
		        cmdOK_Click(sender, e);
	        }
		}

        private void cmdCancel_Click(object sender, EventArgs e)
        {
			_lstExpandCategories = null;
			this.DialogResult = DialogResult.Cancel;
        }

		private void txtSearch_TextChanged(object sender, EventArgs e)
		{
			string strAdditionalFilter = "";
			if (_objCharacter.Options.ExtendAnyDetectionSpell)
				strAdditionalFilter = " and ((not(contains(name, \", Extended\"))))";
			
			// Treat everything as being uppercase so the search is case-insensitive.
			string strSearch = "/chummer/spells/spell[(" + _objCharacter.Options.BookXPath() + ")" + strAdditionalFilter + " and ((contains(translate(name,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\") and not(translate)) or contains(translate(translate,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\"))]";

			treSpells.Nodes.Clear();

			// Populate the Category list.
			XmlNodeList objXmlNodeList = _objXmlDocument.SelectNodes("/chummer/categories/category");
			foreach (XmlNode objXmlCategory in objXmlNodeList)
			{
				if (_strLimitCategory == "" || _strLimitCategory == objXmlCategory.InnerText)
				{
					TreeNode nodCategory = new TreeNode();
					nodCategory.Tag = objXmlCategory.InnerText;
					if (objXmlCategory.Attributes["translate"] != null)
						nodCategory.Text = objXmlCategory.Attributes["translate"].InnerText;
					else
						nodCategory.Text = objXmlCategory.InnerText;

					treSpells.Nodes.Add(nodCategory);
				}
			}

			// Populate the Spell list.
			objXmlNodeList = _objXmlDocument.SelectNodes(strSearch);
			treSpells.TreeViewNodeSorter = new SortByName();

            foreach (XmlNode objXmlSpell in objXmlNodeList)
			{
                bool blnInclude = false;

                if (_objCharacter.AdeptEnabled && !_objCharacter.MagicianEnabled)
                {
                    if (objXmlSpell["descriptor"].InnerText.Contains("Adept"))
                        blnInclude = true;
                }
                else if (!_objCharacter.AdeptEnabled)
                {
                    if (objXmlSpell["descriptor"].InnerText.Contains("Adept"))
                        blnInclude = false;
                    else
                        blnInclude = true;
                }
                else
                    blnInclude = true;

                if (blnInclude)
                {
                    TreeNode nodSpell = new TreeNode();
                    TreeNode nodParent = new TreeNode();
                    if (objXmlSpell["translate"] != null)
                        nodSpell.Text = objXmlSpell["translate"].InnerText;
                    else
                        nodSpell.Text = objXmlSpell["name"].InnerText;
                    nodSpell.Tag = objXmlSpell["name"].InnerText;
                    // Check to see if there is already a Category node for the Spell's category.
                    foreach (TreeNode nodCategory in treSpells.Nodes)
                    {
                        if (nodCategory.Level == 0 && nodCategory.Tag.ToString() == objXmlSpell["category"].InnerText)
                        {
                            nodParent = nodCategory;
                        }
                    }

                    // Add the Spell to the Category node.
                    nodParent.Nodes.Add(nodSpell);
                    nodParent.Expand();
                }
			}

			List<TreeNode> lstRemove = new List<TreeNode>();
			foreach (TreeNode nodNode in treSpells.Nodes)
			{
				if (nodNode.Level == 0 && nodNode.Nodes.Count == 0)
					lstRemove.Add(nodNode);
			}

			foreach (TreeNode nodNode in lstRemove)
				treSpells.Nodes.Remove(nodNode);
		    if (treSpells.Nodes.Count > 0)
		    {
		        treSpells.SelectedNode = treSpells.Nodes[0];
		    }
		}

		private void cmdOKAdd_Click(object sender, EventArgs e)
		{
			_blnAddAgain = true;
			List<TreeNode> _lstExpandCategories = new List<TreeNode>();
			foreach (TreeNode objNode in treSpells.Nodes)
			{
				if (objNode.IsExpanded)
				{
					_lstExpandCategories.Add(objNode);
				}
			}
			this.ExpandedCategories = _lstExpandCategories;
			cmdOK_Click(sender, e);
		}

		private void txtSearch_KeyDown(object sender, KeyEventArgs e)
		{
			if (treSpells.SelectedNode == null)
			{
				if (treSpells.Nodes.Count > 0)
					treSpells.SelectedNode = treSpells.Nodes[0];
			}
			if (e.KeyCode == Keys.Down)
			{
				try
				{
					treSpells.SelectedNode = treSpells.SelectedNode.NextVisibleNode;
					if (treSpells.SelectedNode == null)
						treSpells.SelectedNode = treSpells.Nodes[0];
				}
				catch
				{
				}
			}
			if (e.KeyCode == Keys.Up)
			{
				try
				{
					treSpells.SelectedNode = treSpells.SelectedNode.PrevVisibleNode;
					if (treSpells.SelectedNode == null)
						treSpells.SelectedNode = treSpells.Nodes[treSpells.Nodes.Count - 1].LastNode;
				}
				catch
				{
				}
			}
		}

		private void txtSearch_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Up)
				txtSearch.Select(txtSearch.Text.Length, 0);
		}

		private void chkExtended_CheckedChanged(object sender, EventArgs e)
		{
			treSpells_AfterSelect(sender, null);
		}
		#endregion

		#region Properties
		/// <summary>
		/// Whether or not the user wants to add another item after this one.
		/// </summary>
		public bool AddAgain
		{
			get
			{
				return _blnAddAgain;
			}
		}

		public List<TreeNode> ExpandedCategories
		{
			get
			{
				return _lstExpandCategories;
			}
			set
			{
				_lstExpandCategories = value;
			}
		}
		/// <summary>
		/// Whether or not a Limited version of the Spell was selected.
		/// </summary>
		public bool Limited
		{
			get
			{
				return chkLimited.Checked;
			}
		}

		/// <summary>
		/// Whether or not an Extended version of the Spell was selected.
		/// </summary>
		public bool Extended
		{
			get
			{
				return chkExtended.Checked;
			}
		}

        /// <summary>
        /// Whether or not a Alchemical version of the Spell was selected.
        /// </summary>
        public bool Alchemical
        {
            get
            {
                return chkAlchemical.Checked;
            }
        }

        /// <summary>
		/// Limit the Spell list to a particular Category.
		/// </summary>
		public string LimitCategory
		{
			set
			{
				_strLimitCategory = value;
			}
		}

		/// <summary>
		/// Force a particular Spell to be selected.
		/// </summary>
		public string ForceSpellName
		{
			set
			{
				_strForceSpell = value;
			}
		}

		/// <summary>
        /// Spell that was selected in the dialogue.
        /// </summary>
        public string SelectedSpell
        {
            get
            {
                return _strSelectedSpell;
            }
        }

	    public bool IgnoreRequirements
	    {
		    get
		    {
			    return _blnIgnoreRequirements;
		    }
		    set
		    {
			    _blnIgnoreRequirements = value;
		    }

	    }
		#endregion

		#region Methods
		/// <summary>
		/// Accept the selected item and close the form.
		/// </summary>
        private void AcceptForm()
        {
			_strSelectedSpell = treSpells.SelectedNode.Tag.ToString();
			this.DialogResult = DialogResult.OK;
		}

		private void MoveControls()
		{
			int intWidth = Math.Max(lblDescriptorsLabel.Width, lblTypeLabel.Width);
			intWidth = Math.Max(intWidth, lblTypeLabel.Width);
			intWidth = Math.Max(intWidth, lblRangeLabel.Width);
			intWidth = Math.Max(intWidth, lblDamageLabel.Width);
			intWidth = Math.Max(intWidth, lblDurationLabel.Width);
			intWidth = Math.Max(intWidth, lblDVLabel.Width);

			lblDescriptors.Left = lblDescriptorsLabel.Left + intWidth + 6;
			lblType.Left = lblTypeLabel.Left + intWidth + 6;
			lblRange.Left = lblRangeLabel.Left + intWidth + 6;
			lblDamage.Left = lblDamageLabel.Left + intWidth + 6;
			lblDuration.Left = lblDurationLabel.Left + intWidth + 6;
			lblDV.Left = lblDVLabel.Left + intWidth + 6;

			lblSource.Left = lblSourceLabel.Left + lblSourceLabel.Width + 6;

			lblSearchLabel.Left = txtSearch.Left - 6 - lblSearchLabel.Width;
		}
		#endregion

        private void chkLimited_CheckedChanged(object sender, EventArgs e)
        {
            if (treSpells.SelectedNode.Level > 0)
            {
                // Display the Spell information.
                XmlNode objXmlSpell = _objXmlDocument.SelectSingleNode("/chummer/spells/spell[name = \"" + treSpells.SelectedNode.Tag + "\"]");
                string strDV = objXmlSpell["dv"].InnerText.Replace("/", "÷").Replace("F", LanguageManager.Instance.GetString("String_SpellForce"));
                strDV = strDV.Replace("Overflow damage", LanguageManager.Instance.GetString("String_SpellOverflowDamage"));
                strDV = strDV.Replace("Damage Value", LanguageManager.Instance.GetString("String_SpellDamageValue"));
                strDV = strDV.Replace("Toxin DV", LanguageManager.Instance.GetString("String_SpellToxinDV"));
                strDV = strDV.Replace("Disease DV", LanguageManager.Instance.GetString("String_SpellDiseaseDV"));
                strDV = strDV.Replace("Radiation Power", LanguageManager.Instance.GetString("String_SpellRadiationPower"));

                if (chkLimited.Checked && strDV.StartsWith("F"))
                {
                    int intPos = 0;
                    if (strDV.Contains("-"))
                    {
                        intPos = strDV.IndexOf("-") + 1;
                        string strAfter = strDV.Substring(intPos, strDV.Length - intPos);
                        strDV = strDV.Substring(0, intPos);
                        int intAfter = Convert.ToInt32(strAfter);
                        intAfter += 2;
                        strDV += intAfter.ToString();
                    }
                    else if (strDV.Contains("+"))
                    {
                        intPos = strDV.IndexOf("+");
                        string strAfter = strDV.Substring(intPos, strDV.Length - intPos);
                        strDV = strDV.Substring(0, intPos);
                        int intAfter = Convert.ToInt32(strAfter);
                        intAfter -= 2;
                        if (intAfter > 0)
                            strDV += "+" + intAfter.ToString();
                        else if (intAfter < 0)
                            strDV += intAfter.ToString();
                    }
                    else
                    {
                        strDV += "-2";
                    }
                }

                lblDV.Text = strDV;
            }
        }

        private void lblSource_Click(object sender, EventArgs e)
        {
            CommonFunctions objCommon = new CommonFunctions(_objCharacter);
            objCommon.OpenPDF(lblSource.Text);
        }
	}
}