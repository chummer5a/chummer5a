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
  using System;
using System.Collections.Generic;
  using System.Linq;
using System.Windows.Forms;
using System.Xml;
 ﻿using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class frmSelectLifestyleAdvanced : Form
    {
        private bool _blnAddAgain;
        private readonly Lifestyle _objLifestyle;
        private Lifestyle _objSourceLifestyle;
        private readonly Character _objCharacter;
        private LifestyleType _eType = LifestyleType.Advanced;

        private readonly XmlDocument _xmlDocument;

        private bool _blnSkipRefresh = true;
        private int _intTravelerRdmLP;

        #region Control Events
        public frmSelectLifestyleAdvanced(Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            _objCharacter = objCharacter;
            _objLifestyle = new Lifestyle(objCharacter);
            MoveControls();
            // Load the Lifestyles information.
            _xmlDocument = XmlManager.Load("lifestyles.xml");
        }

        private void frmSelectAdvancedLifestyle_Load(object sender, EventArgs e)
        {
            // Populate the Advanced Lifestyle ComboBoxes.
            // Lifestyles.
            List<ListItem> lstLifestyles = new List<ListItem>();
            using (XmlNodeList xmlLifestyleList = _xmlDocument.SelectNodes("/chummer/lifestyles/lifestyle[" + _objCharacter.Options.BookXPath() + "]"))
                if (xmlLifestyleList?.Count > 0)
                    foreach (XmlNode objXmlLifestyle in xmlLifestyleList)
                    {
                        string strLifestyleName = objXmlLifestyle["name"]?.InnerText;

                        if (!string.IsNullOrEmpty(strLifestyleName) &&
                            strLifestyleName != "ID ERROR. Re-add life style to fix" &&
                            (_eType == LifestyleType.Advanced || objXmlLifestyle["slp"]?.InnerText == "remove") &&
                            !strLifestyleName.Contains("Hospitalized") &&
                            _objCharacter.Options.Books.Contains(objXmlLifestyle["source"]?.InnerText))
                        {
                            lstLifestyles.Add(new ListItem(strLifestyleName, objXmlLifestyle["translate"]?.InnerText ?? strLifestyleName));
                        }
                    }
            // Populate the Qualities list.
            if (_objSourceLifestyle != null)
            {
                foreach (LifestyleQuality objQuality in _objSourceLifestyle.LifestyleQualities)
                {
                    TreeNode nodParent;
                    // Add the Quality to the appropriate parent node.
                    if (objQuality.Type == QualityType.Positive)
                    {
                        nodParent = treLifestyleQualities.Nodes[0];
                    }
                    else if (objQuality.Type == QualityType.Negative)
                    {
                        nodParent = treLifestyleQualities.Nodes[1];
                    }
                    else
                    {
                        nodParent = treLifestyleQualities.Nodes[2];
                    }
                    nodParent.Nodes.Add(objQuality.CreateTreeNode());
                    nodParent.Expand();
                    _objLifestyle.LifestyleQualities.Add(objQuality);
                }
                TreeNode nodGridsParent = treLifestyleQualities.Nodes[3];
                foreach (LifestyleQuality objQuality in _objSourceLifestyle.FreeGrids)
                {
                    _objLifestyle.FreeGrids.Add(objQuality);

                    TreeNode objLoopNode = objQuality.CreateTreeNode();
                    if (objLoopNode != null)
                    {
                        nodGridsParent.Nodes.Add(objLoopNode);
                        nodGridsParent.Expand();
                    }
                }
            }
            cboBaseLifestyle.BeginUpdate();
            cboBaseLifestyle.ValueMember = "Value";
            cboBaseLifestyle.DisplayMember = "Name";
            cboBaseLifestyle.DataSource = lstLifestyles;

            cboBaseLifestyle.SelectedValue = _objLifestyle.BaseLifestyle;

            if (cboBaseLifestyle.SelectedIndex == -1)
                cboBaseLifestyle.SelectedIndex = 0;

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

            cboBaseLifestyle.EndUpdate();
            _blnSkipRefresh = false;
            RefreshSelectedLifestyle();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtLifestyleName.Text))
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectAdvancedLifestyle_LifestyleName", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectAdvancedLifestyle_LifestyleName", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            _blnAddAgain = false;
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void chkTrustFund_Changed(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;

            if (chkTrustFund.Checked)
            {
                nudRoommates.Value = 0;
            }

            nudRoommates.Enabled = !chkTrustFund.Checked;

            CalculateValues();
        }

        private void chkPrimaryTenant_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;

            CalculateValues();
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            AcceptForm();
        }

        private void cboBaseLifestyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            RefreshSelectedLifestyle();
        }

        private void nudPercentage_ValueChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            CalculateValues();
        }

        private void nudRoommates_ValueChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;

            if (nudRoommates.Value == 0 && !chkPrimaryTenant.Checked)
            {
                chkPrimaryTenant.Checked = true;
            }
            CalculateValues();
        }

        private void cmdAddQuality_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            do
            {
                frmSelectLifestyleQuality frmSelectLifestyleQuality = new frmSelectLifestyleQuality(_objCharacter, cboBaseLifestyle.SelectedValue.ToString(), _objLifestyle.LifestyleQualities);
                frmSelectLifestyleQuality.ShowDialog(this);

                // Don't do anything else if the form was canceled.
                if (frmSelectLifestyleQuality.DialogResult == DialogResult.Cancel)
                {
                    frmSelectLifestyleQuality.Close();
                    return;
                }
                blnAddAgain = frmSelectLifestyleQuality.AddAgain;
                
                XmlNode objXmlQuality = _xmlDocument.SelectSingleNode("/chummer/qualities/quality[id = \"" + frmSelectLifestyleQuality.SelectedQuality + "\"]");

                LifestyleQuality objQuality = new LifestyleQuality(_objCharacter);

                objQuality.Create(objXmlQuality, _objLifestyle, _objCharacter, QualitySource.Selected);
                objQuality.Free = frmSelectLifestyleQuality.FreeCost;
                frmSelectLifestyleQuality.Close();
                //objNode.ContextMenuStrip = cmsQuality;
                if (objQuality.InternalId.IsEmptyGuid())
                    continue;
                
                _objLifestyle.LifestyleQualities.Add(objQuality);

                TreeNode objLoopNode = objQuality.CreateTreeNode();
                if (objLoopNode != null)
                {
                    TreeNode nodParent;
                    // Add the Quality to the appropriate parent node.
                    if (objQuality.Type == QualityType.Positive)
                    {
                        nodParent = treLifestyleQualities.Nodes[0];
                    }
                    else if (objQuality.Type == QualityType.Negative)
                    {
                        nodParent = treLifestyleQualities.Nodes[1];
                    }
                    else
                    {
                        nodParent = treLifestyleQualities.Nodes[2];
                    }

                    nodParent.Nodes.Add(objQuality.CreateTreeNode());
                    nodParent.Expand();
                }

                CalculateValues();
            }
            while (blnAddAgain);
        }

        private void cmdDeleteQuality_Click(object sender, EventArgs e)
        {
            // Locate the selected Quality.
            if (treLifestyleQualities.SelectedNode.Level == 0 || treLifestyleQualities.SelectedNode.Parent.Name == "nodFreeMatrixGrids")
                return;

            string strQualityId = treLifestyleQualities.SelectedNode?.Tag.ToString();
            if (!string.IsNullOrEmpty(strQualityId))
            {
                LifestyleQuality objQuality = _objLifestyle.LifestyleQualities.FirstOrDefault(x => x.InternalId == strQualityId);
                if (objQuality != null)
                {
                    if (objQuality.Name == "Not a Home" && cboBaseLifestyle.SelectedValue?.ToString() == "Bolt Hole")
                    {
                        return;
                    }
                    _objLifestyle.LifestyleQualities.Remove(objQuality);
                    treLifestyleQualities.SelectedNode.Remove();
                    CalculateValues();
                }
            }
        }

        private void treLifestyleQualities_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string strSelectedQuality = treLifestyleQualities.SelectedNode?.Tag.ToString();
            LifestyleQuality objQuality = null;
            // Locate the selected Quality.
            if (!string.IsNullOrEmpty(strSelectedQuality))
                objQuality = _objLifestyle.LifestyleQualities.FindById(strSelectedQuality) ?? _objLifestyle.FreeGrids.FindById(strSelectedQuality);
            if (objQuality != null)
            {
                lblQualityLp.Text = objQuality.LP.ToString();
                lblQualityCost.Text = objQuality.Cost.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                string strPage = objQuality.Page(GlobalOptions.Language);
                lblQualitySource.Text = CommonFunctions.LanguageBookShort(objQuality.Source, GlobalOptions.Language) + ' ' + strPage;
                tipTooltip.SetToolTip(lblQualitySource, CommonFunctions.LanguageBookLong(objQuality.Source, GlobalOptions.Language) + ' ' + LanguageManager.GetString("String_Page", GlobalOptions.Language) + ' ' + strPage);
                cmdDeleteQuality.Enabled = !(objQuality.Free || objQuality.OriginSource == QualitySource.BuiltIn);
            }
            else
            {
                lblQualityLp.Text = string.Empty;
                lblQualityCost.Text = string.Empty;
                lblQualitySource.Text = string.Empty;
                tipTooltip.SetToolTip(lblQualitySource, null);
                cmdDeleteQuality.Enabled = false;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Whether or not the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain => _blnAddAgain;

        /// <summary>
        /// Lifestyle that was created in the dialogue.
        /// </summary>
        public Lifestyle SelectedLifestyle => _objLifestyle;

        /// <summary>
        /// Type of Lifestyle to create.
        /// </summary>
        public LifestyleType StyleType
        {
            get => _eType;
            set => _eType = value;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            string strBaseLifestyle = cboBaseLifestyle.SelectedValue.ToString();
            XmlNode objXmlLifestyle = _xmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"" + strBaseLifestyle + "\"]");
            if (objXmlLifestyle == null)
                return;
            _objLifestyle.Source = objXmlLifestyle["source"]?.InnerText;
            _objLifestyle.Page = objXmlLifestyle["page"]?.InnerText;
            _objLifestyle.Name = txtLifestyleName.Text;
            _objLifestyle.Cost = Convert.ToInt32(objXmlLifestyle["cost"]?.InnerText);
            _objLifestyle.Percentage = nudPercentage.Value;
            _objLifestyle.BaseLifestyle = strBaseLifestyle;
            _objLifestyle.Area = decimal.ToInt32(nudArea.Value);
            _objLifestyle.Comforts = decimal.ToInt32(nudComforts.Value);
            _objLifestyle.Security = decimal.ToInt32(nudSecurity.Value);
            _objLifestyle.TrustFund = chkTrustFund.Checked;
            _objLifestyle.Roommates = _objLifestyle.TrustFund ? 0 : decimal.ToInt32(nudRoommates.Value);
            _objLifestyle.PrimaryTenant = chkPrimaryTenant.Checked;

            // Get the starting Nuyen information.
            _objLifestyle.Dice = Convert.ToInt32(objXmlLifestyle["dice"]?.InnerText);
            _objLifestyle.Multiplier = Convert.ToDecimal(objXmlLifestyle["multiplier"]?.InnerText, GlobalOptions.InvariantCultureInfo);
            _objLifestyle.StyleType = _eType;

            if (objXmlLifestyle.TryGetField("id", Guid.TryParse, out Guid source))
            {
                _objLifestyle.SourceID = source;
            }
            else
            {
                Log.Warning(new object[] { "Missing id field for lifestyle xmlnode", objXmlLifestyle });
                Utils.BreakIfDebug();
            }
            DialogResult = DialogResult.OK;
        }

        private void RefreshSelectedLifestyle()
        {
            string strBaseLifestyle = cboBaseLifestyle.SelectedValue?.ToString();
            _objLifestyle.BaseLifestyle = strBaseLifestyle;
            XmlNode xmlAspect = _xmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"" + strBaseLifestyle + "\"]");
            XmlNodeList lstGridNodes = null;
            if (xmlAspect != null)
            {
                lstGridNodes = xmlAspect.SelectNodes("freegrids/freegrid");
                string strSource = xmlAspect["source"]?.InnerText ?? string.Empty;
                string strPage = xmlAspect["altpage"]?.InnerText ?? xmlAspect["page"]?.InnerText ?? string.Empty;
                if (!string.IsNullOrEmpty(strSource) && !string.IsNullOrEmpty(strPage))
                {
                    lblSource.Text = CommonFunctions.LanguageBookShort(strSource, GlobalOptions.Language) + ' ' + strPage;
                    tipTooltip.SetToolTip(lblSource, CommonFunctions.LanguageBookLong(strSource, GlobalOptions.Language) + ' ' + LanguageManager.GetString("String_Page", GlobalOptions.Language) + ' ' + strPage);
                }
                else
                {
                    lblSource.Text = string.Empty;
                    tipTooltip.SetToolTip(lblSource, string.Empty);
                }
            }
            else
            {
                lblSource.Text = string.Empty;
                tipTooltip.SetToolTip(lblSource, string.Empty);
            }
            
            //This needs a handler for translations, will fix later.
            if (strBaseLifestyle == "Bolt Hole")
            {
                if (_objLifestyle.LifestyleQualities.All(x => x.Name != "Not a Home"))
                {
                    XmlNode xmlQuality = _xmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"Not a Home\"]");
                    LifestyleQuality objQuality = new LifestyleQuality(_objCharacter);
                    objQuality.Create(xmlQuality, _objLifestyle, _objCharacter, QualitySource.BuiltIn);

                    _objLifestyle.LifestyleQualities.Add(objQuality);

                    TreeNode objLoopNode = objQuality.CreateTreeNode();
                    if (objLoopNode != null)
                    {
                        TreeNode nodParent = treLifestyleQualities.Nodes[1];
                        nodParent.Nodes.Add(objQuality.CreateTreeNode());
                        nodParent.Expand();
                    }
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
                        int intModuloTemp;
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
                    //Bolt Holes automatically come with the Not a Home quality.
                    if (objQuality.Name == "Not a Home" || objQuality.Name == "Dug a Hole")
                    {
                        _objLifestyle.LifestyleQualities.Remove(objQuality);
                        treLifestyleQualities.FindNode(objQuality.InternalId)?.Remove();
                    }
                }
            }

            if (lstGridNodes != null)
            {
                _objLifestyle.FreeGrids.Clear();
                treLifestyleQualities.Nodes[3].Nodes.Clear();
                foreach (XmlNode xmlNode in lstGridNodes)
                {
                    XmlNode xmlQuality = _xmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + xmlNode.InnerText + "\"]");
                    LifestyleQuality objQuality = new LifestyleQuality(_objCharacter);
                    string strPush = xmlNode.Attributes?["select"]?.InnerText;
                    if (!string.IsNullOrWhiteSpace(strPush))
                    {
                        _objCharacter.Pushtext.Push(strPush);
                    }
                    objQuality.Create(xmlQuality, _objLifestyle, _objCharacter, QualitySource.BuiltIn);

                    _objLifestyle.FreeGrids.Add(objQuality);

                    TreeNode objLoopNode = objQuality.CreateTreeNode();
                    if (objLoopNode != null)
                    {
                        TreeNode nodParent = treLifestyleQualities.Nodes[3];
                        nodParent.Nodes.Add(objQuality.CreateTreeNode());
                        nodParent.Expand();
                    }
                }
            }

            CalculateValues();
        }

        /// <summary>
        /// Calculate the LP value for the selected items.
        /// </summary>
        private void CalculateValues()
        {
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
            XmlNode xmlNode = _xmlDocument.SelectSingleNode("/chummer/comforts/comfort[name = \"" + strBaseLifestyle + "\"]");
            xmlNode.TryGetInt32FieldQuickly("minimum", ref intMinComfort);
            xmlNode.TryGetInt32FieldQuickly("limit", ref intMaxComfort);
            if (intMaxComfort < intMinComfort)
                intMaxComfort = intMinComfort;
            // Area.
            xmlNode = _xmlDocument.SelectSingleNode("/chummer/neighborhoods/neighborhood[name = \"" + strBaseLifestyle + "\"]");
            xmlNode.TryGetInt32FieldQuickly("minimum", ref intMinArea);
            xmlNode.TryGetInt32FieldQuickly("limit", ref intMaxArea);
            if (intMaxArea < intMinArea)
                intMaxArea = intMinArea;
            // Security.
            xmlNode = _xmlDocument.SelectSingleNode("/chummer/securities/security[name = \"" + strBaseLifestyle + "\"]");
            xmlNode.TryGetInt32FieldQuickly("minimum", ref intMinSec);
            xmlNode.TryGetInt32FieldQuickly("limit", ref intMaxSec);
            if (intMaxSec < intMinSec)
                intMaxSec = intMinSec;

            // Calculate the cost of Positive Qualities.
            foreach (LifestyleQuality objQuality in _objLifestyle.LifestyleQualities)
            {
                intLP -= objQuality.LP;
                intMultiplier += objQuality.Multiplier;
                intMultiplierBaseOnly += objQuality.BaseMultiplier;
                intMaxArea += objQuality.AreaMaximum;
                intMaxComfort += objQuality.ComfortMaximum;
                intMaxSec += objQuality.SecurityMaximum;
                intMinArea += objQuality.Area;
                intMinComfort += objQuality.Comfort;
                intMinSec += objQuality.Security;

                decimal decCost = objQuality.Cost;
                // Calculate the cost of Entertainments.
                if (decCost != 0 && (objQuality.Type == QualityType.Entertainment || objQuality.Type == QualityType.Contracts))
                {
                    if (objQuality.Type == QualityType.Contracts)
                    {
                        decExtraCostContracts += decCost;
                    }
                    else if (objQuality.Category == "Entertainment - Outing" || objQuality.Category == "Entertainment - Service")
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
            
            nudComforts.Maximum = Math.Max(intMaxComfort - intMinComfort, 0);
            nudArea.Maximum = Math.Max(intMaxArea - intMinArea, 0);
            nudSecurity.Maximum = Math.Max(intMaxSec - intMinSec, 0);
            int intComfortsValue = decimal.ToInt32(nudComforts.Value);
            int intAreaValue = decimal.ToInt32(nudArea.Value);
            int intSecurityValue = decimal.ToInt32(nudSecurity.Value);
            int intRoommatesValue = decimal.ToInt32(nudRoommates.Value);

            _blnSkipRefresh = false;
            //set the Labels for current/maximum
            Label_SelectAdvancedLifestyle_Base_Comforts.Text = LanguageManager.GetString("Label_SelectAdvancedLifestyle_Base_Comforts", GlobalOptions.Language)
                .Replace("{0}", (nudComforts.Value + intMinComfort).ToString(GlobalOptions.CultureInfo))
                .Replace("{1}", (nudComforts.Maximum + intMinComfort).ToString(GlobalOptions.CultureInfo));
            Label_SelectAdvancedLifestyle_Base_Securities.Text = LanguageManager.GetString("Label_SelectAdvancedLifestyle_Base_Security", GlobalOptions.Language)
                .Replace("{0}", (nudSecurity.Value + intMinSec).ToString(GlobalOptions.CultureInfo))
                .Replace("{1}", (nudSecurity.Maximum + intMinSec).ToString(GlobalOptions.CultureInfo));
            Label_SelectAdvancedLifestyle_Base_Area.Text = LanguageManager.GetString("Label_SelectAdvancedLifestyle_Base_Area", GlobalOptions.Language)
                .Replace("{0}", (nudArea.Value + intMinArea).ToString(GlobalOptions.CultureInfo))
                .Replace("{1}", (nudArea.Maximum + intMinArea).ToString(GlobalOptions.CultureInfo));

            //calculate the total LP
            xmlNode = _xmlDocument.SelectSingleNode("chummer/lifestyles/lifestyle[name = \"" + strBaseLifestyle + "\"]");
            intLP += Convert.ToInt32(xmlNode?["lp"]?.InnerText);
            intLP -= intComfortsValue;
            intLP -= intAreaValue;
            intLP -= intSecurityValue;
            intLP += intRoommatesValue;

            if (strBaseLifestyle == "Street")
            {
                decNuyen += intComfortsValue * 50;
                decNuyen += intAreaValue * 50;
                decNuyen += intSecurityValue * 50;
            }
            else if (strBaseLifestyle == "Traveler")
            {
                intLP += _intTravelerRdmLP;
            }

            if (!chkTrustFund.Checked)
            {
                // Determine the base Nuyen cost.
                string strCost = xmlNode?["cost"]?.InnerText;
                if (!string.IsNullOrEmpty(strCost))
                    decBaseNuyen += Convert.ToDecimal(strCost, GlobalOptions.InvariantCultureInfo);
                decBaseNuyen += decBaseNuyen * ((intMultiplier + intMultiplierBaseOnly) / 100.0m);
                decNuyen += decBaseNuyen;
            }
            decNuyen += decExtraCostAssets + (decExtraCostAssets * (intMultiplier / 100.0m));
            decNuyen *= nudPercentage.Value / 100.0m;
            if (!chkPrimaryTenant.Checked)
            {
                decNuyen /= intRoommatesValue + 1.0m;
            }
            decNuyen += decExtraCostServicesOutings + (decExtraCostServicesOutings * (intMultiplier / 100.0m));
            decNuyen += decExtraCostContracts;
            lblTotalLP.Text = intLP.ToString();
            lblCost.Text = decNuyen.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
        }

        /// <summary>
        /// Lifestyle to update when editing.
        /// </summary>
        /// <param name="objLifestyle">Lifestyle to edit.</param>
        public void SetLifestyle(Lifestyle objLifestyle)
        {
            _objSourceLifestyle = objLifestyle;
            _eType = objLifestyle.StyleType;
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
