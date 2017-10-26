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
  using System.Drawing;
  using System.Linq;
using System.Windows.Forms;
using System.Xml;
﻿﻿using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class frmSelectLifestyleAdvanced : Form
    {
        private bool _blnAddAgain = false;
        private Lifestyle _objLifestyle;
        private Lifestyle _objSourceLifestyle;
        private readonly Character _objCharacter;
        private LifestyleType _objType = LifestyleType.Advanced;

        private readonly XmlDocument _objXmlDocument = new XmlDocument();

        private bool _blnSkipRefresh = false;
        private int _intTravelerRdmLP = 0;

        #region Control Events
        public frmSelectLifestyleAdvanced(Lifestyle objLifestyle, Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.Load(GlobalOptions.Language, this);
            _objCharacter = objCharacter;
            _objLifestyle = objLifestyle;
            MoveControls();
            // Load the Lifestyles information.
            _objXmlDocument = XmlManager.Load("lifestyles.xml");
        }

        private void frmSelectAdvancedLifestyle_Load(object sender, EventArgs e)
        {
            _blnSkipRefresh = true;
            foreach (Label objLabel in Controls.OfType<Label>())
            {
                if (objLabel.Text.StartsWith("["))
                    objLabel.Text = string.Empty;
            }

            foreach (TreeNode objNode in treLifestyleQualities.Nodes)
            {
                if (objNode.Tag != null)
                    objNode.Text = LanguageManager.GetString(objNode.Tag.ToString());
            }

            // Populate the Advanced Lifestyle ComboBoxes.
            // Lifestyles.
            List<ListItem> lstLifestyles = new List<ListItem>();
            foreach (XmlNode objXmlLifestyle in _objXmlDocument.SelectNodes("/chummer/lifestyles/lifestyle"))
            {
                string strLifestyleName = objXmlLifestyle["name"]?.InnerText;

                if (!string.IsNullOrEmpty(strLifestyleName) &&
                    strLifestyleName != "ID ERROR. Re-add life style to fix" &&
                    (_objType == LifestyleType.Advanced || objXmlLifestyle["slp"]?.InnerText == "remove") &&
                    !strLifestyleName.Contains("Hospitalized") &&
                    _objCharacter.Options.Books.Contains(objXmlLifestyle["source"]?.InnerText))
                {
                    ListItem objItem = new ListItem
                    {
                        Value = strLifestyleName,
                        Name = objXmlLifestyle["translate"]?.InnerText ?? strLifestyleName
                    };
                    lstLifestyles.Add(objItem);
                }
            }
            //Populate the Qualities list.
            if (_objSourceLifestyle != null)
            {
                foreach (LifestyleQuality objQuality in _objSourceLifestyle.LifestyleQualities)
                {
                    TreeNode objNode = new TreeNode
                    {
                        Name = objQuality.Name,
                        Text = objQuality.DisplayName,
                        Tag = objQuality.InternalId,
                        ToolTipText = objQuality.Notes
                    };

                    if (objQuality.OriginSource == QualitySource.BuiltIn)
                    {
                        objNode.ForeColor = SystemColors.GrayText;
                    }

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
                foreach (LifestyleQuality objQuality in _objSourceLifestyle.FreeGrids)
                {
                    TreeNode objNode = new TreeNode
                    {
                        Name = objQuality.Name,
                        Text = objQuality.DisplayName,
                        Tag = objQuality.InternalId,
                        ToolTipText = objQuality.Notes,
                        ForeColor = SystemColors.GrayText
                    };

                    treLifestyleQualities.Nodes[3].Nodes.Add(objNode);
                    treLifestyleQualities.Nodes[3].Expand();
                    _objLifestyle.FreeGrids.Add(objQuality);
                }
            }
            cboBaseLifestyle.BeginUpdate();
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
                chkTrustFund.Checked = _objSourceLifestyle.TrustFund;
            }
            string strBaseLifestyle = cboBaseLifestyle.SelectedValue.ToString();
            XmlNode objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/comforts/comfort[name = \"" + strBaseLifestyle + "\"]");
            Label_SelectAdvancedLifestyle_Base_Comforts.Text = LanguageManager.GetString("Label_SelectAdvancedLifestyle_Base_Comforts").Replace("{0}", (nudComforts.Value).ToString(GlobalOptions.CultureInfo)).Replace("{1}", objXmlAspect["limit"].InnerText);
            Label_SelectAdvancedLifestyle_Base_Area.Text = LanguageManager.GetString("Label_SelectAdvancedLifestyle_Base_Area").Replace("{0}", (nudArea.Value).ToString(GlobalOptions.CultureInfo)).Replace("{1}", objXmlAspect["limit"].InnerText);
            Label_SelectAdvancedLifestyle_Base_Securities.Text = LanguageManager.GetString("Label_SelectAdvancedLifestyle_Base_Security").Replace("{0}", (nudSecurity.Value).ToString(GlobalOptions.CultureInfo)).Replace("{1}", objXmlAspect["limit"].InnerText);

            objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"" + strBaseLifestyle + "\"]");
            if (objXmlAspect?["source"] != null && objXmlAspect["page"] != null)
                lblSource.Text = objXmlAspect["source"].InnerText + " " + objXmlAspect["page"].InnerText;

            cboBaseLifestyle.EndUpdate();
            _blnSkipRefresh = false;
            CalculateValues();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtLifestyleName.Text))
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectAdvancedLifestyle_LifestyleName"), LanguageManager.GetString("MessageTitle_SelectAdvancedLifestyle_LifestyleName"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void chkTrustFund_Changed(object sender, EventArgs e)
        {
            if (chkTrustFund.Checked)
            {
                nudRoommates.Value = 0;
            }
            nudRoommates.Enabled = !chkTrustFund.Checked;
            CalculateValues();
        }
        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            cmdOK_Click(sender, e);
        }

        private void cboBaseLifestyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_blnSkipRefresh)
            {
                string strBaseLifestyle = cboBaseLifestyle.SelectedValue.ToString();
                _objLifestyle.BaseLifestyle = strBaseLifestyle;
                XmlDocument objXmlDocument = XmlManager.Load("lifestyles.xml");
                XmlNode objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"" + strBaseLifestyle + "\"]");
                XmlNodeList objGridNodes = null;
                if (objXmlAspect != null)
                {
                    objGridNodes = objXmlAspect.SelectNodes("freegrids/freegrid");
                    if (objXmlAspect["source"] != null && objXmlAspect["page"] != null)
                        lblSource.Text = objXmlAspect["source"].InnerText + " " + objXmlAspect["page"].InnerText;
                }

                objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/comforts/comfort[name = \"" + strBaseLifestyle + "\"]");
                nudComforts.Minimum = objXmlAspect?["minimum"] != null ? Convert.ToInt32(objXmlAspect["minimum"].InnerText) : 0;
                nudComforts.Value = nudComforts.Minimum;
                // Area.
                objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/neighborhoods/neighborhood[name = \"" + strBaseLifestyle + "\"]");
                nudArea.Minimum = objXmlAspect?["minimum"] != null ? Convert.ToInt32(objXmlAspect["minimum"].InnerText) : 0;
                nudArea.Value = nudArea.Minimum;
                // Security.
                objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/securities/security[name = \"" + strBaseLifestyle + "\"]");
                nudSecurity.Minimum = objXmlAspect?["minimum"] != null ? Convert.ToInt32(objXmlAspect["minimum"].InnerText) : 0;
                nudSecurity.Value = nudSecurity.Minimum;


                //This needs a handler for translations, will fix later.
                if (strBaseLifestyle == "Bolt Hole")
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
                        objQuality.Create(objXmlQuality, _objLifestyle, _objCharacter, QualitySource.BuiltIn, objNode);
                        treLifestyleQualities.Nodes[1].Nodes.Add(objNode);
                        treLifestyleQualities.Nodes[1].Expand();
                        _objLifestyle.LifestyleQualities.Add(objQuality);
                    }
                }
                else
                {
                    //Characters with the Trust Fund Quality can have the lifestyle discounted.
                    if (strBaseLifestyle == "Medium" &&
                        (_objCharacter.TrustFund == 1 || _objCharacter.TrustFund == 4))
                    {
                        chkTrustFund.Visible = true;
                    }
                    else if (strBaseLifestyle == "Low" &&
                        _objCharacter.TrustFund == 2)
                    {
                        chkTrustFund.Visible = true;
                    }
                    else if (strBaseLifestyle == "High" &&
                        _objCharacter.TrustFund == 3)
                    {
                        chkTrustFund.Visible = true;
                    }
                    else
                    {
                        chkTrustFund.Checked = false;
                        chkTrustFund.Visible = false;
                        if (strBaseLifestyle == "Traveler")
                        {
                            Random rndTavelerLp = MersenneTwister.SfmtRandom.Create();
                            int intModuloTemp = 0;
                            do
                            {
                                intModuloTemp = rndTavelerLp.Next();
                            }
                            while (intModuloTemp >= int.MaxValue - 1); // Modulo bias removal for 1d6
                            _intTravelerRdmLP = 1 + intModuloTemp % 6;
                        }
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
                        }
                        else if (objQuality.Name == "Dug a Hole")
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

                if (objGridNodes != null)
                {
                    _objLifestyle.FreeGrids.Clear();
                    treLifestyleQualities.Nodes[3].Nodes.Clear();
                    foreach (XmlNode objXmlNode in objGridNodes)
                    {
                        XmlNode objXmlQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objXmlNode.InnerText + "\"]");
                        LifestyleQuality objQuality = new LifestyleQuality(_objCharacter);
                        TreeNode objNode = new TreeNode();
                        string push = objXmlNode.Attributes?["select"]?.InnerText;
                        if (!string.IsNullOrWhiteSpace(push))
                        {
                            _objCharacter.Pushtext.Push(push);
                        }
                        objQuality.Create(objXmlQuality, _objLifestyle, _objCharacter, QualitySource.BuiltIn, objNode);
                        objNode.Text = objQuality.DisplayName;
                        treLifestyleQualities.Nodes[3].Nodes.Add(objNode);
                        treLifestyleQualities.Nodes[3].Expand();
                        _objLifestyle.FreeGrids.Add(objQuality);
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

        private void cmdAddQuality_Click(object sender, EventArgs e)
        {
            frmSelectLifestyleQuality frmSelectLifestyleQuality = new frmSelectLifestyleQuality(_objCharacter, cboBaseLifestyle.SelectedValue.ToString());
            frmSelectLifestyleQuality.ShowDialog(this);

            // Don't do anything else if the form was canceled.
            if (frmSelectLifestyleQuality.DialogResult == DialogResult.Cancel)
                return;

            XmlDocument objXmlDocument = XmlManager.Load("lifestyles.xml");
            XmlNode objXmlQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + frmSelectLifestyleQuality.SelectedQuality + "\"]");

            TreeNode objNode = new TreeNode();
            LifestyleQuality objQuality = new LifestyleQuality(_objCharacter);

            objQuality.Create(objXmlQuality, _objLifestyle, _objCharacter, QualitySource.Selected, objNode);
            //objNode.ContextMenuStrip = cmsQuality;
            if (objQuality.InternalId == Guid.Empty.ToString())
                return;

            objQuality.Free = frmSelectLifestyleQuality.FreeCost;

            // Make sure that adding the Quality would not cause the character to exceed their BP limits.
            //bool blnAddItem = true;

            //if (blnAddItem)
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
            if (treLifestyleQualities.SelectedNode.Level == 0 || treLifestyleQualities.SelectedNode.Parent.Name == "nodFreeMatrixGrids")
                return;
            else
            {
                string strQualityName = treLifestyleQualities.SelectedNode.Name;
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
            CommonFunctions.OpenPDF(lblSource.Text, _objCharacter);
        }

        private void treLifestyleQualities_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Locate the selected Quality.
            lblQualitySource.Text = string.Empty;
            lblQualityLp.Text = string.Empty;
            tipTooltip.SetToolTip(lblQualitySource, null);
            if (treLifestyleQualities.SelectedNode == null || treLifestyleQualities.SelectedNode.Level == 0)
            {
                return;
            }
            LifestyleQuality objQuality =
                    CommonFunctions.FindByIdWithNameCheck(treLifestyleQualities.SelectedNode.Tag.ToString(),
                        _objLifestyle.LifestyleQualities) ??
                    CommonFunctions.FindByIdWithNameCheck(treLifestyleQualities.SelectedNode.Tag.ToString(),
                            _objLifestyle.FreeGrids);
            lblQualityLp.Text = objQuality.LP.ToString();
            lblQualityCost.Text = $"{objQuality.Cost:###,###,##0.##¥}";
            lblQualitySource.Text = $@"{objQuality.Source} {objQuality.Page}";
            tipTooltip.SetToolTip(lblQualitySource, objQuality.SourceTooltip);
            cmdDeleteQuality.Enabled = !(objQuality.Free || objQuality.OriginSource == QualitySource.BuiltIn);
        }

        private void lblQualitySource_Click(object sender, EventArgs e)
        {
            CommonFunctions.OpenPDF(lblQualitySource.Text, _objCharacter);
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
            string strBaseLifestyle = cboBaseLifestyle.SelectedValue.ToString();
            XmlNode objXmlLifestyle = _objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"" + strBaseLifestyle + "\"]");
            _objLifestyle.Source = objXmlLifestyle["source"].InnerText;
            _objLifestyle.Page = objXmlLifestyle["page"].InnerText;
            _objLifestyle.Name = txtLifestyleName.Text;
            //_objLifestyle.Cost = Convert.ToInt32(objXmlAspect["cost"].InnerText);
            _objLifestyle.Cost = CalculateValues();
            _objLifestyle.Percentage = Convert.ToInt32(nudPercentage.Value);
            _objLifestyle.BaseLifestyle = strBaseLifestyle;
            _objLifestyle.Area = Convert.ToInt32(nudArea.Value);
            _objLifestyle.Comforts = Convert.ToInt32(nudComforts.Value);
            _objLifestyle.Security = Convert.ToInt32(nudSecurity.Value);
            _objLifestyle.TrustFund = chkTrustFund.Checked;
            _objLifestyle.Roommates = _objLifestyle.TrustFund ? 0 : Convert.ToInt32(nudRoommates.Value);

            // Get the starting Nuyen information.
            _objLifestyle.Dice = Convert.ToInt32(objXmlLifestyle["dice"].InnerText);
            _objLifestyle.Multiplier = Convert.ToDecimal(objXmlLifestyle["multiplier"].InnerText, GlobalOptions.InvariantCultureInfo);
            _objLifestyle.StyleType = _objType;

            Guid source;
            if (objXmlLifestyle.TryGetField("id", Guid.TryParse, out source))
            {
                _objLifestyle.SourceID = source;
            }
            else
            {
                Log.Warning(new object[] { "Missing id field for lifestyle xmlnode", objXmlLifestyle });
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    System.Diagnostics.Debugger.Break();
                }
            }
            DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// Calculate the LP value for the selected items.
        /// </summary>
        private decimal CalculateValues()
        {
            if (_blnSkipRefresh)
                return 0;

            int intLP = 0;
            decimal decBaseNuyen = 0;
            decimal decNuyen = 0;
            int intMultiplier = 0;
            int intMultiplierBaseOnly = 0;
            decimal decExtraCostAssets = 0;
            decimal decExtraCostServicesOutings = 0;
            decimal decExtraCostContracts = 0;
            int intMinComfort = 0;
            int intMaxComfort = 0;
            int intMinArea = 0;
            int intMaxArea = 0;
            int intMinSec = 0;
            int intMaxSec = 0;
            string strBaseLifestyle = cboBaseLifestyle.SelectedValue.ToString();

            // Calculate the limits of the 3 aspects.
            // Comforts.
            XmlNode objXmlNode = _objXmlDocument.SelectSingleNode("/chummer/comforts/comfort[name = \"" + strBaseLifestyle + "\"]");
            objXmlNode.TryGetInt32FieldQuickly("minimum", ref intMinComfort);
            objXmlNode.TryGetInt32FieldQuickly("limit", ref intMaxComfort);
            if (intMaxComfort < intMinComfort)
                intMaxComfort = intMinComfort;
            // Area.
            objXmlNode = _objXmlDocument.SelectSingleNode("/chummer/neighborhoods/neighborhood[name = \"" + strBaseLifestyle + "\"]");
            objXmlNode.TryGetInt32FieldQuickly("minimum", ref intMinArea);
            objXmlNode.TryGetInt32FieldQuickly("limit", ref intMaxArea);
            if (intMaxArea < intMinArea)
                intMaxArea = intMinArea;
            // Security.
            objXmlNode = _objXmlDocument.SelectSingleNode("/chummer/securities/security[name = \"" + strBaseLifestyle + "\"]");
            objXmlNode.TryGetInt32FieldQuickly("minimum", ref intMinSec);
            objXmlNode.TryGetInt32FieldQuickly("limit", ref intMaxSec);
            if (intMaxSec < intMinSec)
                intMaxSec = intMinSec;

            // Calculate the cost of Positive Qualities.
            foreach (LifestyleQuality objQuality in _objLifestyle.LifestyleQualities)
            {
                intLP -= objQuality.LP;
                intMultiplier += objQuality.Multiplier;
                intMultiplierBaseOnly += objQuality.BaseMultiplier;
                intMaxArea += objQuality.AreaCost;
                intMaxComfort += objQuality.ComfortCost;
                intMaxSec += objQuality.SecurityCost;
                intMinArea += objQuality.AreaMinimum;
                intMinComfort += objQuality.ComfortMinimum;
                intMinSec += objQuality.SecurityMinimum;

                decimal decCost = objQuality.Cost;
                // Calculate the cost of Entertainments.
                if (decCost != 0 && (objQuality.Type == QualityType.Entertainment || objQuality.Type == QualityType.Contracts))
                {
                    objXmlNode = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objQuality.Name + "\"]");
                    if (objQuality.Type == QualityType.Contracts)
                    {
                        decExtraCostContracts += decCost;
                    }
                    else if (objXmlNode?["category"] != null &&
                        (objXmlNode["category"].InnerText.Equals("Entertainment - Outing") ||
                        objXmlNode["category"].InnerText.Equals("Entertainment - Service")))
                    {
                        decExtraCostServicesOutings += decCost;
                    }
                    else
                    {
                        decExtraCostAssets += decCost;
                    }
                }
                else
                    decBaseNuyen += decCost;
            }
            _blnSkipRefresh = true;

            nudComforts.Minimum = intMinComfort;
            nudComforts.Maximum = intMaxComfort;
            nudArea.Minimum = intMinArea;
            nudArea.Maximum = intMaxArea;
            nudSecurity.Minimum = intMinSec;
            nudSecurity.Maximum = intMaxSec;
            int intComfortsValue = Convert.ToInt32(nudComforts.Value);
            int intAreaValue = Convert.ToInt32(nudArea.Value);
            int intSecurityValue = Convert.ToInt32(nudSecurity.Value);
            int intRoommatesValue = Convert.ToInt32(nudRoommates.Value);

            _blnSkipRefresh = false;
            //set the Labels for current/maximum
            Label_SelectAdvancedLifestyle_Base_Comforts.Text = LanguageManager.GetString("Label_SelectAdvancedLifestyle_Base_Comforts").Replace("{0}", (nudComforts.Value).ToString(GlobalOptions.CultureInfo)).Replace("{1}", nudComforts.Maximum.ToString(GlobalOptions.CultureInfo));
            Label_SelectAdvancedLifestyle_Base_Securities.Text = LanguageManager.GetString("Label_SelectAdvancedLifestyle_Base_Security").Replace("{0}", (nudSecurity.Value).ToString(GlobalOptions.CultureInfo)).Replace("{1}", nudSecurity.Maximum.ToString(GlobalOptions.CultureInfo));
            Label_SelectAdvancedLifestyle_Base_Area.Text = LanguageManager.GetString("Label_SelectAdvancedLifestyle_Base_Area").Replace("{0}", (nudArea.Value).ToString(GlobalOptions.CultureInfo)).Replace("{1}", nudArea.Maximum.ToString(GlobalOptions.CultureInfo));

            //calculate the total LP
            objXmlNode = _objXmlDocument.SelectSingleNode("/chummer/lifestylePoints/lifestylePoint[name = \"" + strBaseLifestyle + "\"]");
            intLP += Convert.ToInt32(objXmlNode?["amount"]?.InnerText);
            intLP -= intComfortsValue - intMinComfort;
            intLP -= intAreaValue - intMinArea;
            intLP -= intSecurityValue - intMinSec;
            intLP += intRoommatesValue;

            intMultiplier += _objCharacter.Improvements.Where(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.LifestyleCost).Sum(objImprovement => objImprovement.Value);

            if (strBaseLifestyle.ToString() == "Street")
            {
                decNuyen += (intComfortsValue - intMinComfort) * 50;
                decNuyen += (intAreaValue - intMinArea) * 50;
                decNuyen += (intSecurityValue - intMinSec) * 50;
            }
            else if (strBaseLifestyle == "Traveler")
            {
                intLP += _intTravelerRdmLP;
            }

            intMultiplier += intRoommatesValue * 10;
            intMultiplier += (intComfortsValue - intMinComfort) * 10;
            intMultiplier += (intAreaValue - intMinArea) * 10;
            intMultiplier += (intSecurityValue - intMinSec) * 10;
            if (!chkTrustFund.Checked)
            {
                // Determine the base Nuyen cost.
                XmlNode objXmlLifestyle = _objXmlDocument.SelectSingleNode("chummer/lifestyles/lifestyle[name = \"" + strBaseLifestyle + "\"]");
                if (objXmlLifestyle?["cost"] != null)
                    decBaseNuyen += Convert.ToDecimal(objXmlLifestyle["cost"].InnerText, GlobalOptions.InvariantCultureInfo);
                decBaseNuyen += decBaseNuyen * ((intMultiplier + intMultiplierBaseOnly) / 100.0m);
                decNuyen += decBaseNuyen;
            }
            decNuyen += decExtraCostAssets + (decExtraCostAssets * (intMultiplier / 100.0m));
            decNuyen *= nudPercentage.Value / 100.0m;
            decNuyen /= (intRoommatesValue + 1.0m);
            decNuyen += decExtraCostServicesOutings + (decExtraCostServicesOutings * (intMultiplier / 100.0m)); ;
            decNuyen += decExtraCostContracts;
            lblTotalLP.Text = intLP.ToString();
            lblCost.Text = $"{decNuyen:###,###,##0.##¥}";

            return decNuyen;
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
            catch (ArgumentException)
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
    }
}
