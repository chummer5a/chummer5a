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
﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    public partial class frmSelectLifestyleAdvanced : Form
    {
        private bool _blnAddAgain = false;
        private Lifestyle _objLifestyle;
        private Lifestyle _objSourceLifestyle;
        private readonly Character _objCharacter;
        private LifestyleType _objType = LifestyleType.Advanced;

        private XmlDocument _objXmlDocument = new XmlDocument();

        private bool _blnSkipRefresh = false;
        private int _intTravelerRdmLP = 0;

        #region Control Events
        public frmSelectLifestyleAdvanced(Lifestyle objLifestyle, Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            _objCharacter = objCharacter;
            _objLifestyle = objLifestyle;
            MoveControls();
        }

        private void frmSelectAdvancedLifestyle_Load(object sender, EventArgs e)
        {
            _blnSkipRefresh = true;
            foreach (Label objLabel in this.Controls.OfType<Label>())
            {
                if (objLabel.Text.StartsWith("["))
                    objLabel.Text = "";
            }

            // Load the Lifestyles information.
            _objXmlDocument = XmlManager.Instance.Load("lifestyles.xml");

            // Populate the Advanced Lifestyle ComboBoxes.
            // Lifestyles.
            List<ListItem> lstLifestyles = new List<ListItem>();
            foreach (XmlNode objXmlLifestyle in _objXmlDocument.SelectNodes("/chummer/lifestyles/lifestyle"))
            {
                bool blnAdd = true;
                if (_objType != LifestyleType.Advanced && objXmlLifestyle["slp"] != null)
                {
                    if (objXmlLifestyle["slp"].InnerText == "remove")
                        blnAdd = false;
                }

				if (objXmlLifestyle["name"].InnerText == "ID ERROR. Re-add life style to fix")
				{
					blnAdd = false;
				}
                if (!_objCharacter.Options.Books.Contains(objXmlLifestyle["source"].InnerText))
                {
                    blnAdd = false;
                }
                if (objXmlLifestyle["name"].InnerText.Contains("Hospitalized"))               
                {
                    blnAdd = false;
                }

                if (blnAdd)
                {
                    ListItem objItem = new ListItem();
                    objItem.Value = objXmlLifestyle["name"].InnerText;
                    if (objXmlLifestyle["translate"] != null)
                        objItem.Name = objXmlLifestyle["translate"].InnerText;
                    else
                        objItem.Name = objXmlLifestyle["name"].InnerText;
                    lstLifestyles.Add(objItem);
                }
            }
			//Populate the Qualities list.
			if (_objSourceLifestyle != null)
			{
				foreach (LifestyleQuality objQuality in _objSourceLifestyle.LifestyleQualities)
				{
					TreeNode objNode = new TreeNode();
					objNode.Name = objQuality.Name;
					objNode.Text = objQuality.DisplayName;
					objNode.Tag = objQuality.InternalId;

					objNode.ToolTipText = objQuality.Notes;

					if (objQuality.Type == QualityType.Positive)
					{
						treLifestyleQualities.Nodes[0].Nodes.Add(objNode);
						treLifestyleQualities.Nodes[0].Expand();
					}
					else if (objQuality.Type == QualityType.Negative)
					{
						treLifestyleQualities.Nodes[1].Nodes.Add(objNode);
						treLifestyleQualities.Nodes[1].Expand();
					}
					else
					{
						treLifestyleQualities.Nodes[2].Nodes.Add(objNode);
						treLifestyleQualities.Nodes[2].Expand();
					}
					_objLifestyle.LifestyleQualities.Add(objQuality);
				}
			}
			cboBaseLifestyle.ValueMember = "Value";
            cboBaseLifestyle.DisplayMember = "Name";
            cboBaseLifestyle.DataSource = lstLifestyles;

            cboBaseLifestyle.SelectedValue = _objLifestyle.BaseLifestyle;

			if (cboBaseLifestyle.SelectedIndex == -1)
            { cboBaseLifestyle.SelectedIndex = 0; }

            if (_objSourceLifestyle != null)
            {
                txtLifestyleName.Text = _objSourceLifestyle.Name;
                nudRoommates.Value = _objSourceLifestyle.Roommates;
                nudPercentage.Value = _objSourceLifestyle.Percentage;
                nudArea.Value = _objSourceLifestyle.Area;
                nudComforts.Value = _objSourceLifestyle.Comforts;
                nudSecurity.Value = _objSourceLifestyle.Security;
                cboBaseLifestyle.SelectedValue = _objSourceLifestyle.BaseLifestyle;
                try
                {
                    chkTrustFund.Checked = _objSourceLifestyle.TrustFund;
                }
                catch
                {
                }
            }
		    XmlNode objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/comforts/comfort[name = \"" + cboBaseLifestyle.SelectedValue + "\"]");
			Label_SelectAdvancedLifestyle_Base_Comforts.Text = LanguageManager.Instance.GetString("Label_SelectAdvancedLifestyle_Base_Comforts").Replace("{0}", (nudComforts.Value).ToString()).Replace("{1}", objXmlAspect["limit"].InnerText);
			Label_SelectAdvancedLifestyle_Base_Area.Text = LanguageManager.Instance.GetString("Label_SelectAdvancedLifestyle_Base_Area").Replace("{0}", (nudArea.Value).ToString()).Replace("{1}", objXmlAspect["limit"].InnerText);
			Label_SelectAdvancedLifestyle_Base_Securities.Text = LanguageManager.Instance.GetString("Label_SelectAdvancedLifestyle_Base_Security").Replace("{0}", (nudSecurity.Value).ToString()).Replace("{1}", objXmlAspect["limit"].InnerText);

            objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"" + cboBaseLifestyle.SelectedValue + "\"]");
            lblSource.Text = objXmlAspect["source"].InnerText + " " + objXmlAspect["page"].InnerText;

            _blnSkipRefresh = false;
            CalculateValues();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (txtLifestyleName.Text == "")
            {
                MessageBox.Show(LanguageManager.Instance.GetString("Message_SelectAdvancedLifestyle_LifestyleName"), LanguageManager.Instance.GetString("MessageTitle_SelectAdvancedLifestyle_LifestyleName"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

		private void chkTrustFund_Changed(object sender, EventArgs e)
		{
			CalculateValues();
		}
		private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            cmdOK_Click(sender, e);
        }

        private void trePositiveQualities_AfterCheck(object sender, TreeViewEventArgs e)
        {
            CalculateValues();
        }

        private void treNegativeQualities_AfterCheck(object sender, TreeViewEventArgs e)
        {
            CalculateValues();
        }

        private void cboBaseLifestyle_SelectedIndexChanged(object sender, EventArgs e)
        {

			if (!_blnSkipRefresh)
			{
                XmlDocument objXmlDocument = XmlManager.Instance.Load("lifestyles.xml");
                XmlNode objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"" + cboBaseLifestyle.SelectedValue + "\"]");
			    lblSource.Text = objXmlAspect["source"].InnerText+ " " + objXmlAspect["page"].InnerText;

                objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/comforts/comfort[name = \"" + cboBaseLifestyle.SelectedValue + "\"]");
                nudComforts.Minimum = Convert.ToInt32(objXmlAspect["minimum"].InnerText);
                nudComforts.Value = Convert.ToInt32(objXmlAspect["minimum"].InnerText);
                // Area.
                objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/neighborhoods/neighborhood[name = \"" + cboBaseLifestyle.SelectedValue + "\"]");
                nudArea.Minimum = Convert.ToInt32(objXmlAspect["minimum"].InnerText);
                nudArea.Value = Convert.ToInt32(objXmlAspect["minimum"].InnerText);
                // Security.
                objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/securities/security[name = \"" + cboBaseLifestyle.SelectedValue + "\"]");
                nudSecurity.Minimum = Convert.ToInt32(objXmlAspect["minimum"].InnerText);
                nudSecurity.Value = Convert.ToInt32(objXmlAspect["minimum"].InnerText);


                //This needs a handler for translations, will fix later.
                if (cboBaseLifestyle.SelectedValue.ToString() == "Bolt Hole")
				{
					bool blnAddQuality = true;
					foreach (TreeNode objNode in treLifestyleQualities.Nodes[1].Nodes)
					{
						//Bolt Holes automatically come with the Not a Home quality.
						if (objNode.Name == "Not a Home")
						{
							blnAddQuality = false;
							break;
						}
					}
					if (blnAddQuality)
					{
						XmlNode objXmlQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Not a Home\"]");
						LifestyleQuality objQuality = new LifestyleQuality(_objCharacter);
						TreeNode objNode = new TreeNode();
						try
						{
							objQuality.Create(objXmlQuality, _objCharacter, QualitySource.BuiltIn, objNode);
						}
						catch
						{
						}
						objQuality.LP = 0;
						treLifestyleQualities.Nodes[1].Nodes.Add(objNode);
						treLifestyleQualities.Nodes[1].Expand();
						_objLifestyle.LifestyleQualities.Add(objQuality);
					}
				}
				else
				{
				    if (cboBaseLifestyle.SelectedValue.ToString() == "Traveler")
				    {
				        Random rndTavelerLp = new Random();
				        _intTravelerRdmLP = rndTavelerLp.Next(1, 7);
				    }
                    //Characters with the Trust Fund Quality can have the lifestyle discounted.
					if (_objCharacter.TrustFund == 1 && cboBaseLifestyle.SelectedValue.ToString() == "Medium")
					{
						chkTrustFund.Visible = true;
					}
					else if (_objCharacter.TrustFund == 2 && cboBaseLifestyle.SelectedValue.ToString() == "Low")
					{
						chkTrustFund.Visible = true;
					}
					else if (_objCharacter.TrustFund == 3 && cboBaseLifestyle.SelectedValue.ToString() == "High")
					{
						chkTrustFund.Visible = true;
					}
					else if (_objCharacter.TrustFund == 4 && cboBaseLifestyle.SelectedValue.ToString() == "Medium")
					{
						chkTrustFund.Visible = true;
					}
					else
					{
						chkTrustFund.Checked = false;
						chkTrustFund.Visible = false;
					}

					foreach (LifestyleQuality objQuality in _objLifestyle.LifestyleQualities.ToList())
					{
                        if (objQuality.Name == "Not a Home")
                        {
                            _objLifestyle.LifestyleQualities.Remove(objQuality);
                            foreach (TreeNode objNode in treLifestyleQualities.Nodes[1].Nodes)
                            {
                                //Bolt Holes automatically come with the Not a Home quality.
                                if (objNode.Text == "Not a Home")
                                {
                                    treLifestyleQualities.Nodes[1].Nodes.Remove(objNode);
                                }
                            }
                        } else if (objQuality.Name == "Dug a Hole")
                        {
                            _objLifestyle.LifestyleQualities.Remove(objQuality);
                            foreach (TreeNode objNode in treLifestyleQualities.Nodes[0].Nodes)
                            {
                                //Bolt Holes automatically come with the Not a Home quality.
                                if (objNode.Text == "Dug a Hole")
                                {
                                    treLifestyleQualities.Nodes[1].Nodes.Remove(objNode);
                                }
                            }
                        }

                    }
				}
			}
			CalculateValues(); 
        }

        private void nudPercentage_ValueChanged(object sender, EventArgs e)
        {
            CalculateValues();
        }

        private void nudRoommates_ValueChanged(object sender, EventArgs e)
        {
            CalculateValues();
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

        /// <summary>
        /// Lifestyle that was created in the dialogue.
        /// </summary>
        public Lifestyle SelectedLifestyle
        {
            get
            {
                return _objLifestyle;
            }
        }

        /// <summary>
        /// Type of Lifestyle to create.
        /// </summary>
        public LifestyleType StyleType
        {
            get
            {
                return _objType;
            }
            set
            {
                _objType = value;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
			XmlNode objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"" + cboBaseLifestyle.SelectedValue.ToString() + "\"]");
			_objLifestyle.Source = objXmlAspect["source"].InnerText;
            _objLifestyle.Page = objXmlAspect["page"].InnerText;
            _objLifestyle.Name = txtLifestyleName.Text;
            _objLifestyle.Cost = Convert.ToInt32(objXmlAspect["cost"].InnerText);
			_objLifestyle.Roommates = Convert.ToInt32(nudRoommates.Value);
            _objLifestyle.Percentage = Convert.ToInt32(nudPercentage.Value);
            _objLifestyle.BaseLifestyle = cboBaseLifestyle.Text;
			_objLifestyle.Area = Convert.ToInt32(nudArea.Value);
			_objLifestyle.Comforts = Convert.ToInt32(nudComforts.Value);
			_objLifestyle.Security = Convert.ToInt32(nudSecurity.Value);
			_objLifestyle.BaseLifestyle = cboBaseLifestyle.SelectedValue.ToString();
			_objLifestyle.TrustFund = chkTrustFund.Checked;
			//_objLifestyle.LifestyleQualities.Clear();

			// Get the starting Nuyen information.
			
            _objLifestyle.Dice = Convert.ToInt32(objXmlAspect["dice"].InnerText);
            _objLifestyle.Multiplier = Convert.ToInt32(objXmlAspect["multiplier"].InnerText);
            _objLifestyle.StyleType = _objType;

			XmlDocument objXmlDocument = XmlManager.Instance.Load("lifestyles.xml");

			Guid source;
			if (objXmlAspect.TryGetField("id", Guid.TryParse, out source))
			{
				_objLifestyle.SourceID = source;
			}
			else
			{
				Log.Warning(new object[] { "Missing id field for lifestyle xmlnode", objXmlAspect });
				if (System.Diagnostics.Debugger.IsAttached)
					{
						System.Diagnostics.Debugger.Break();
					}
			}
            this.DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// Calculate the LP value for the selected items.
        /// </summary>
        private int CalculateValues()
        {
            if (_blnSkipRefresh)
                return 0;

            int intLP = 0;
			int intBaseNuyen = 0;
            int intNuyen = 0;
            int intMultiplier = 0;
            int intExtraCostAssets = 0;
            int intExtraCostServicesOutings = 0;

            // Calculate the limits of the 3 aspects.
            // Comforts. 
            XmlNode objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/comforts/comfort[name = \"" + cboBaseLifestyle.SelectedValue + "\"]");
            int intMinComfort = Convert.ToInt32(objXmlAspect["minimum"].InnerText);
            int intMaxComfort = Convert.ToInt32(objXmlAspect["limit"].InnerText);
            // Area.
            objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/neighborhoods/neighborhood[name = \"" + cboBaseLifestyle.SelectedValue + "\"]");
            int intMinArea = Convert.ToInt32(objXmlAspect["minimum"].InnerText);
            int intMaxArea = Convert.ToInt32(objXmlAspect["limit"].InnerText);           
            // Security.
            objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/securities/security[name = \"" + cboBaseLifestyle.SelectedValue + "\"]");
            int intMinSec = Convert.ToInt32(objXmlAspect["minimum"].InnerText);
            int intMaxSec = Convert.ToInt32(objXmlAspect["limit"].InnerText);
            // Determine the base Nuyen cost.
            XmlNode objXmlLifestyle = _objXmlDocument.SelectSingleNode("chummer/lifestyles/lifestyle[name = \"" + cboBaseLifestyle.SelectedValue + "\"]");

			// Calculate the cost of Positive Qualities.
			foreach (LifestyleQuality objNode in _objLifestyle.LifestyleQualities)
			{
				if (objNode.Type == QualityType.Positive)
				{
					objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objNode.Name.ToString() + "\" and category = \"Positive\"]");
					if (objXmlAspect != null)
					{
						intLP -= Convert.ToInt32(objXmlAspect["lp"].InnerText);
                        if (objXmlAspect["comforts"] != null)
                        {
                            intMaxComfort += Convert.ToInt32(objXmlAspect["comforts"].InnerText);
                            if (objXmlAspect["comfortsMinimum"] != null)
                            {
                                intMinComfort += Convert.ToInt32(objXmlAspect["comfortsMinimum"].InnerText);
                            }                           
                        }
                        if (objXmlAspect["security"] != null)
                        {
                            intMaxSec += Convert.ToInt32(objXmlAspect["security"].InnerText);
                            if (objXmlAspect["securityMinimum"] != null)
                            {
                                intMinSec += Convert.ToInt32(objXmlAspect["securityMinimum"].InnerText);
                            }
                        }
                        if (objXmlAspect["multiplier"] != null)
						{
							intMultiplier += Convert.ToInt32(objXmlAspect["multiplier"].InnerText);
						}
						if (objXmlAspect["cost"] != null)
						{
							intNuyen += Convert.ToInt32(objXmlAspect["cost"].InnerText);
						}                        
                    }
				}
				// Calculate the cost of Negative Qualities.
				else if (objNode.Type == QualityType.Negative)
				{
					objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objNode.Name.ToString() + "\" and category = \"Negative\"]");
					if (objXmlAspect != null)
					{
						intLP -= Convert.ToInt32(objXmlAspect["lp"].InnerText);
						if (objXmlAspect["neighborhood"] != null)
						{
							intMaxArea += Convert.ToInt32(objXmlAspect["neighborhood"].InnerText);
                        }

						if (objXmlAspect["comforts"] != null)
						{
							intMaxComfort += Convert.ToInt32(objXmlAspect["comforts"].InnerText);
                        }
					    if (objXmlAspect["security"] != null)
					    {
					        intMaxSec += Convert.ToInt32(objXmlAspect["security"].InnerText);
                        }
						if (objXmlAspect["multiplier"] != null)
						{
							intMultiplier += Convert.ToInt32(objXmlAspect["multiplier"].InnerText);
						}
						if (objXmlAspect["cost"] != null)
						{
                            intNuyen += Convert.ToInt32(objXmlAspect["cost"].InnerText);
						}
					}
				}
                // Calculate the cost of Entertainments.
				else if (objNode.Type == QualityType.Entertainment)
				{
					objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objNode.Name + "\"]");
					string strLifestyleEntertainments = objXmlAspect["allowed"].InnerText;					
					if (objXmlAspect != null)
					{
						intLP -= Convert.ToInt32(objXmlAspect["lp"].InnerText);
					}
				    string strLifestyleEquivalent = cboBaseLifestyle.SelectedValue.ToString();
				    if (strLifestyleEquivalent == "Bolt Hole")
				    {
				        strLifestyleEquivalent = "Squatter";
				    } else if (strLifestyleEquivalent == "Traveler")
				    {
                        strLifestyleEquivalent = "Low";
                    } else if (strLifestyleEquivalent == "Commercial")
				    {
                        strLifestyleEquivalent = "Medium";
                    } else if (strLifestyleEquivalent.StartsWith("Hospitalized"))
				    {
                        strLifestyleEquivalent = "High";
                    }
				    bool blnEntertainmentFree = strLifestyleEntertainments.Contains(cboBaseLifestyle.SelectedValue.ToString());
				    bool blnEntertainmentFreeEqui = strLifestyleEntertainments.Contains(strLifestyleEquivalent); 

					if (!(blnEntertainmentFreeEqui || blnEntertainmentFree))
					{
						if (objXmlAspect["cost"] != null)
						{
						    if (objXmlAspect["category"].InnerText.Equals("Entertainment - Outing") ||
						        objXmlAspect["category"].InnerText.Equals("Entertainment - Service"))
						    {
						        intExtraCostServicesOutings += Convert.ToInt32(objXmlAspect["cost"].InnerText);
						    }
						    else
						    {
						        intExtraCostAssets += Convert.ToInt32(objXmlAspect["cost"].InnerText);
						    }
						}
					}
				}
			}
            _blnSkipRefresh = true;

            nudComforts.Minimum = intMinComfort;
            nudComforts.Maximum = intMaxComfort;
            nudArea.Minimum = intMinArea;
            nudArea.Maximum = intMaxArea;
            nudSecurity.Minimum = intMinSec;
            nudSecurity.Maximum = intMaxSec;

            _blnSkipRefresh = false;
            //set the Labels for current/maximum
            Label_SelectAdvancedLifestyle_Base_Comforts.Text = LanguageManager.Instance.GetString("Label_SelectAdvancedLifestyle_Base_Comforts").Replace("{0}", (nudComforts.Value).ToString()).Replace("{1}", nudComforts.Maximum.ToString());
            Label_SelectAdvancedLifestyle_Base_Securities.Text = LanguageManager.Instance.GetString("Label_SelectAdvancedLifestyle_Base_Security").Replace("{0}", (nudSecurity.Value).ToString()).Replace("{1}", nudSecurity.Maximum.ToString());
            Label_SelectAdvancedLifestyle_Base_Area.Text = LanguageManager.Instance.GetString("Label_SelectAdvancedLifestyle_Base_Area").Replace("{0}", (nudArea.Value).ToString()).Replace("{1}", nudArea.Maximum.ToString());

            //calculate the total LP
            objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/lifestylePoints/lifestylePoint[name = \"" + cboBaseLifestyle.SelectedValue + "\"]");
            intLP += Convert.ToInt32(objXmlAspect["amount"].InnerText);
            intLP -= (Convert.ToInt32(nudComforts.Value) - Convert.ToInt32(nudComforts.Minimum));
            intLP -= (Convert.ToInt32(nudArea.Value) - Convert.ToInt32(nudArea.Minimum));
            intLP -= (Convert.ToInt32(nudSecurity.Value) - Convert.ToInt32(nudSecurity.Minimum));
            intLP += Convert.ToInt32(nudRoommates.Value);
            if (cboBaseLifestyle.SelectedValue.ToString() == "Traveler")
            {
                intLP += _intTravelerRdmLP;
            }

            foreach (Improvement objImprovement in _objCharacter.Improvements)
			{
				if (objImprovement.ImproveType == Improvement.ImprovementType.LifestyleCost)
				{
					intMultiplier += objImprovement.Value;
				}
			}
            if (intLP < 0)
            {
                intNuyen -= intLP * 5000;
            }
            if (cboBaseLifestyle.SelectedValue.ToString() == "Street")
            {
                intNuyen += Convert.ToInt32(nudSecurity.Value - intMinSec) * 50;
                intNuyen += Convert.ToInt32(nudArea.Value - intMinArea) * 50;
                intNuyen += Convert.ToInt32(nudComforts.Value - intMinComfort) * 50;
            }

			intMultiplier += Convert.ToInt32(nudRoommates.Value * 10);
            intMultiplier += Convert.ToInt32(nudSecurity.Value - intMinSec) * 10;
            intMultiplier += Convert.ToInt32(nudArea.Value - intMinArea) * 10;
            intMultiplier += Convert.ToInt32(nudComforts.Value - intMinComfort) * 10;
            intBaseNuyen += Convert.ToInt32(objXmlLifestyle["cost"].InnerText);
			intNuyen += Convert.ToInt32(intBaseNuyen * (intMultiplier / 100.0));
            intNuyen += intBaseNuyen;
            intNuyen += intExtraCostAssets;
			intNuyen = Convert.ToInt32(Convert.ToDouble(intNuyen, GlobalOptions.Instance.CultureInfo) * Convert.ToDouble(nudPercentage.Value / 100, GlobalOptions.Instance.CultureInfo));
            intNuyen = Convert.ToInt32(intNuyen / (Convert.ToInt32(nudRoommates.Value) + 1));
            intNuyen += intExtraCostServicesOutings;
			if (chkTrustFund.Checked)
			{
				intNuyen -= intBaseNuyen;
            }
            lblTotalLP.Text = intLP.ToString();
            lblCost.Text = String.Format("{0:###,###,##0¥}", intNuyen);

            return intNuyen;
        }

        /// <summary>
        /// Lifestyle to update when editing.
        /// </summary>
        /// <param name="objLifestyle">Lifestyle to edit.</param>
        public void SetLifestyle(Lifestyle objLifestyle)
        {
            _objSourceLifestyle = objLifestyle;
            _objType = objLifestyle.StyleType;
        }

        /// <summary>
        /// Sort the contents of a TreeView alphabetically.
        /// </summary>
        /// <param name="treTree">TreeView to sort.</param>
        private void SortTree(TreeView treTree)
        {
            List<TreeNode> lstNodes = new List<TreeNode>();
            foreach (TreeNode objNode in treTree.Nodes)
                lstNodes.Add(objNode);
            treTree.Nodes.Clear();
            try
            {
                SortByName objSort = new SortByName();
                lstNodes.Sort(objSort.Compare);
            }
            catch
            {
            }
            foreach (TreeNode objNode in lstNodes)
                treTree.Nodes.Add(objNode);
        }

        private void MoveControls()
        {
            //int intLeft = 0;
            //intLeft = Math.Max(lblLifestyleNameLabel.Left + lblLifestyleNameLabel.Width, Label_SelectAdvancedLifestyle_Upgrade_Comforts.Left + Label_SelectAdvancedLifestyle_Upgrade_Comforts.Width);
            //intLeft = Math.Max(intLeft, lblNeighborhood.Left + lblNeighborhood.Width);
            //intLeft = Math.Max(intLeft, lblSecurity.Left + lblSecurity.Width);

            //txtLifestyleName.Left = intLeft + 6;
            //cboBaseLifestyle.Left = intLeft + 6;
            //cboBaseLifestyle.Left = intLeft + 6;
            //cboBaseLifestyle.Left = intLeft + 6;
        }
        #endregion

        private void cmdAddQuality_Click(object sender, EventArgs e)
        {
            frmSelectLifestyleQuality frmSelectLifestyleQuality = new frmSelectLifestyleQuality(_objCharacter, cboBaseLifestyle.SelectedValue.ToString());
            frmSelectLifestyleQuality.ShowDialog(this);

                        // Don't do anything else if the form was canceled.
            if (frmSelectLifestyleQuality.DialogResult == DialogResult.Cancel)
                return;

            XmlDocument objXmlDocument = XmlManager.Instance.Load("lifestyles.xml");
            XmlNode objXmlQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + frmSelectLifestyleQuality.SelectedQuality + "\"]");

            TreeNode objNode = new TreeNode();
            LifestyleQuality objQuality = new LifestyleQuality(_objCharacter);

            try { objQuality.Create(objXmlQuality, _objCharacter, QualitySource.Selected, objNode); }
            catch { }
            //objNode.ContextMenuStrip = cmsQuality;
            if (objQuality.InternalId == Guid.Empty.ToString())
                return;

            if (frmSelectLifestyleQuality.FreeCost)
                objQuality.LP = 0;
			
            // Make sure that adding the Quality would not cause the character to exceed their BP limits.
            bool blnAddItem = true;

            if (blnAddItem)
            {
                // Add the Quality to the appropriate parent node.
                if (objQuality.Type == QualityType.Positive)
                {
                    treLifestyleQualities.Nodes[0].Nodes.Add(objNode);
                    treLifestyleQualities.Nodes[0].Expand();
                }
                else if (objQuality.Type == QualityType.Negative)
                {
                    treLifestyleQualities.Nodes[1].Nodes.Add(objNode);
                    treLifestyleQualities.Nodes[1].Expand();
                }
                else
                {
                    treLifestyleQualities.Nodes[2].Nodes.Add(objNode);
                    treLifestyleQualities.Nodes[2].Expand();
                }
                _objLifestyle.LifestyleQualities.Add(objQuality);

                CalculateValues();

                if (frmSelectLifestyleQuality.AddAgain)
                    cmdAddQuality_Click(sender, e);
            }
        }

        private void cmdDeleteQuality_Click(object sender, EventArgs e)
        {
            // Locate the selected Quality.
            if (treLifestyleQualities.SelectedNode.Level == 0)
                return;
            else
            {
                String strQualityName = treLifestyleQualities.SelectedNode.Name;
                if (strQualityName == "Not a Home" && cboBaseLifestyle.SelectedValue.ToString() == "Bolt Hole")
                {
                    return;
                }
                _objLifestyle.LifestyleQualities.Remove(_objLifestyle.LifestyleQualities.First(x => x.Name.Equals(strQualityName)));
                treLifestyleQualities.SelectedNode.Remove();
                CalculateValues();
            }
        }

        private void lblSource_Click(object sender, EventArgs e)
        {
            CommonFunctions objCommon = new CommonFunctions(_objCharacter);
            objCommon.OpenPDF(lblSource.Text);
        }

        private void treLifestyleQualities_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Locate the selected Quality.
            lblQualitySource.Text = "";
            lblQualityLp.Text = "";
            tipTooltip.SetToolTip(lblQualitySource, null);
            try
            {
                if (treLifestyleQualities.SelectedNode.Level == 0)
                    return;
            }
            catch
            {
                return;
            }

            string strLifestyleQualityName = treLifestyleQualities.SelectedNode.Text;
            XmlNode objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + strLifestyleQualityName + "\"]");


            string strBook = objXmlAspect["source"].InnerText;
            string strPage = objXmlAspect["page"].InnerText;
            lblQualitySource.Text = strBook + " " + strPage;
            tipTooltip.SetToolTip(lblQualitySource, strBook + " " + LanguageManager.Instance.GetString("String_Page") + " " + strPage);
            lblQualityLp.Text = objXmlAspect["lp"].InnerText;
        }

        private void lblQualitySource_Click(object sender, EventArgs e)
        {
            CommonFunctions objCommon = new CommonFunctions(_objCharacter);
            objCommon.OpenPDF(lblQualitySource.Text);
        }
    }
}