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
        private readonly Lifestyle _objLifestyle;
        private Lifestyle _objSourceLifestyle;
        private readonly Character _objCharacter;
        private LifestyleType _eType = LifestyleType.Advanced;

        private readonly XmlDocument _objXmlDocument = null;

        private bool _blnSkipRefresh = true;
        private int _intTravelerRdmLP = 0;

        #region Control Events
        public frmSelectLifestyleAdvanced(Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            _objCharacter = objCharacter;
            _objLifestyle = new Lifestyle(objCharacter);
            MoveControls();
            // Load the Lifestyles information.
            _objXmlDocument = XmlManager.Load("lifestyles.xml");
        }

        private void frmSelectAdvancedLifestyle_Load(object sender, EventArgs e)
        {
            // Populate the Advanced Lifestyle ComboBoxes.
            // Lifestyles.
            List<ListItem> lstLifestyles = new List<ListItem>();
            foreach (XmlNode objXmlLifestyle in _objXmlDocument.SelectNodes("/chummer/lifestyles/lifestyle[" + _objCharacter.Options.BookXPath() + "]"))
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
                    TreeNode objNode = new TreeNode
                    {
                        Name = objQuality.Name,
                        Text = objQuality.FormattedDisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language),
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
                        Text = objQuality.FormattedDisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language),
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
            string strBaseLifestyle = cboBaseLifestyle.SelectedValue.ToString();
            XmlNode objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/comforts/comfort[name = \"" + strBaseLifestyle + "\"]");
            Label_SelectAdvancedLifestyle_Base_Comforts.Text = LanguageManager.GetString("Label_SelectAdvancedLifestyle_Base_Comforts", GlobalOptions.Language).Replace("{0}", (nudComforts.Value).ToString(GlobalOptions.CultureInfo)).Replace("{1}", objXmlAspect["limit"].InnerText);
            Label_SelectAdvancedLifestyle_Base_Area.Text = LanguageManager.GetString("Label_SelectAdvancedLifestyle_Base_Area", GlobalOptions.Language).Replace("{0}", (nudArea.Value).ToString(GlobalOptions.CultureInfo)).Replace("{1}", objXmlAspect["limit"].InnerText);
            Label_SelectAdvancedLifestyle_Base_Securities.Text = LanguageManager.GetString("Label_SelectAdvancedLifestyle_Base_Security", GlobalOptions.Language).Replace("{0}", (nudSecurity.Value).ToString(GlobalOptions.CultureInfo)).Replace("{1}", objXmlAspect["limit"].InnerText);

            objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"" + strBaseLifestyle + "\"]");

            string strSource = objXmlAspect["source"]?.InnerText ?? string.Empty;
            string strPage = objXmlAspect["altpage"]?.InnerText ?? objXmlAspect["page"]?.InnerText ?? string.Empty;
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

            cboBaseLifestyle.EndUpdate();
            _blnSkipRefresh = false;
            CalculateValues();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtLifestyleName.Text))
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectAdvancedLifestyle_LifestyleName", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectAdvancedLifestyle_LifestyleName", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void chkPrimaryTenant_CheckedChanged(object sender, EventArgs e)
        {
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
                string strBaseLifestyle = cboBaseLifestyle.SelectedValue?.ToString();
                _objLifestyle.BaseLifestyle = strBaseLifestyle;
                XmlDocument objXmlDocument = XmlManager.Load("lifestyles.xml");
                XmlNode objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"" + strBaseLifestyle + "\"]");
                XmlNodeList objGridNodes = null;
                if (objXmlAspect != null)
                {
                    objGridNodes = objXmlAspect.SelectNodes("freegrids/freegrid");
                    string strSource = objXmlAspect["source"]?.InnerText ?? string.Empty;
                    string strPage = objXmlAspect["altpage"]?.InnerText ?? objXmlAspect["page"]?.InnerText ?? string.Empty;
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


                objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/comforts/comfort[name = \"" + strBaseLifestyle + "\"]/limit");
                nudComforts.Maximum = Convert.ToInt32(objXmlAspect?.InnerText);
                // Area.
                objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/neighborhoods/neighborhood[name = \"" + strBaseLifestyle + "\"]/limit");
                nudArea.Maximum = Convert.ToInt32(objXmlAspect?.InnerText);
                // Security.
                objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/securities/security[name = \"" + strBaseLifestyle + "\"]/limit");
                nudSecurity.Maximum = Convert.ToInt32(objXmlAspect?.InnerText);

                nudArea.Value = 0;
                nudComforts.Value = 0;
                nudSecurity.Value = 0;

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
                        objQuality.Create(objXmlQuality, _objLifestyle, _objCharacter, QualitySource.BuiltIn);
                        treLifestyleQualities.Nodes[1].Nodes.Add(objQuality.CreateTreeNode());
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
                        string push = objXmlNode.Attributes?["select"]?.InnerText;
                        if (!string.IsNullOrWhiteSpace(push))
                        {
                            _objCharacter.Pushtext.Push(push);
                        }
                        objQuality.Create(objXmlQuality, _objLifestyle, _objCharacter, QualitySource.BuiltIn);
                        treLifestyleQualities.Nodes[3].Nodes.Add(objQuality.CreateTreeNode());
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
            if (!_blnSkipRefresh)
            {
                if (nudRoommates.Value == 0 && !chkPrimaryTenant.Checked)
                {
                    chkPrimaryTenant.Checked = true;
                }
                CalculateValues();
            }
        }

        private void cmdAddQuality_Click(object sender, EventArgs e)
        {
            bool blnAddAgain = false;
            do
            {
                frmSelectLifestyleQuality frmSelectLifestyleQuality = new frmSelectLifestyleQuality(_objCharacter, cboBaseLifestyle.SelectedValue.ToString());
                frmSelectLifestyleQuality.ShowDialog(this);

                // Don't do anything else if the form was canceled.
                if (frmSelectLifestyleQuality.DialogResult == DialogResult.Cancel)
                {
                    frmSelectLifestyleQuality.Close();
                    return;
                }
                blnAddAgain = frmSelectLifestyleQuality.AddAgain;

                XmlDocument objXmlDocument = XmlManager.Load("lifestyles.xml");
                XmlNode objXmlQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[id = \"" + frmSelectLifestyleQuality.SelectedQuality + "\"]");

                LifestyleQuality objQuality = new LifestyleQuality(_objCharacter);

                objQuality.Create(objXmlQuality, _objLifestyle, _objCharacter, QualitySource.Selected);
                objQuality.Free = frmSelectLifestyleQuality.FreeCost;
                frmSelectLifestyleQuality.Close();
                //objNode.ContextMenuStrip = cmsQuality;
                if (objQuality.InternalId.IsEmptyGuid())
                    continue;

                // Add the Quality to the appropriate parent node.
                if (objQuality.Type == QualityType.Positive)
                {
                    treLifestyleQualities.Nodes[0].Nodes.Add(objQuality.CreateTreeNode());
                    treLifestyleQualities.Nodes[0].Expand();
                }
                else if (objQuality.Type == QualityType.Negative)
                {
                    treLifestyleQualities.Nodes[1].Nodes.Add(objQuality.CreateTreeNode());
                    treLifestyleQualities.Nodes[1].Expand();
                }
                else
                {
                    treLifestyleQualities.Nodes[2].Nodes.Add(objQuality.CreateTreeNode());
                    treLifestyleQualities.Nodes[2].Expand();
                }
                _objLifestyle.LifestyleQualities.Add(objQuality);

                CalculateValues();
            }
            while (blnAddAgain);
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
        public Lifestyle SelectedLifestyle => _objLifestyle;

        /// <summary>
        /// Type of Lifestyle to create.
        /// </summary>
        public LifestyleType StyleType
        {
            get
            {
                return _eType;
            }
            set
            {
                _eType = value;
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
            _objLifestyle.Cost = Convert.ToInt32(objXmlLifestyle["cost"].InnerText);
            _objLifestyle.Percentage = nudPercentage.Value;
            _objLifestyle.BaseLifestyle = strBaseLifestyle;
            _objLifestyle.Area = decimal.ToInt32(nudArea.Value);
            _objLifestyle.Comforts = decimal.ToInt32(nudComforts.Value);
            _objLifestyle.Security = decimal.ToInt32(nudSecurity.Value);
            _objLifestyle.TrustFund = chkTrustFund.Checked;
            _objLifestyle.Roommates = _objLifestyle.TrustFund ? 0 : decimal.ToInt32(nudRoommates.Value);
            _objLifestyle.PrimaryTenant = chkPrimaryTenant.Checked;

            // Get the starting Nuyen information.
            _objLifestyle.Dice = Convert.ToInt32(objXmlLifestyle["dice"].InnerText);
            _objLifestyle.Multiplier = Convert.ToDecimal(objXmlLifestyle["multiplier"].InnerText, GlobalOptions.InvariantCultureInfo);
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

        /// <summary>
        /// Calculate the LP value for the selected items.
        /// </summary>
        private void CalculateValues()
        {
            if (_blnSkipRefresh)
                return;

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
                    else
                    {
                        string strCategory = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objQuality.Name + "\"]/category")?.InnerText;
                        if (strCategory.Equals("Entertainment - Outing") || strCategory.Equals("Entertainment - Service"))
                        {
                            decExtraCostServicesOutings += decCost;
                        }
                        else
                        {
                            decExtraCostAssets += decCost;
                        }
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
            Label_SelectAdvancedLifestyle_Base_Comforts.Text = LanguageManager.GetString("Label_SelectAdvancedLifestyle_Base_Comforts", GlobalOptions.Language).Replace("{0}", (nudComforts.Value + intMinComfort).ToString(GlobalOptions.CultureInfo)).Replace("{1}", (nudComforts.Maximum + intMinComfort).ToString(GlobalOptions.CultureInfo));
            Label_SelectAdvancedLifestyle_Base_Securities.Text = LanguageManager.GetString("Label_SelectAdvancedLifestyle_Base_Security", GlobalOptions.Language).Replace("{0}", (nudSecurity.Value + intMinSec).ToString(GlobalOptions.CultureInfo)).Replace("{1}", (nudSecurity.Maximum + intMinSec).ToString(GlobalOptions.CultureInfo));
            Label_SelectAdvancedLifestyle_Base_Area.Text = LanguageManager.GetString("Label_SelectAdvancedLifestyle_Base_Area", GlobalOptions.Language).Replace("{0}", (nudArea.Value + intMinArea).ToString(GlobalOptions.CultureInfo)).Replace("{1}", (nudArea.Maximum + intMinArea).ToString(GlobalOptions.CultureInfo));

            //calculate the total LP
            intLP += Convert.ToInt32(_objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"" + strBaseLifestyle + "\"]/lp")?.InnerText);
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
                XmlNode objXmlLifestyle = _objXmlDocument.SelectSingleNode("chummer/lifestyles/lifestyle[name = \"" + strBaseLifestyle + "\"]");
                if (objXmlLifestyle?["cost"] != null)
                    decBaseNuyen += Convert.ToDecimal(objXmlLifestyle["cost"].InnerText, GlobalOptions.InvariantCultureInfo);
                decBaseNuyen += decBaseNuyen * ((intMultiplier + intMultiplierBaseOnly) / 100.0m);
                decNuyen += decBaseNuyen;
            }
            decNuyen += decExtraCostAssets + (decExtraCostAssets * (intMultiplier / 100.0m));
            decNuyen *= nudPercentage.Value / 100.0m;
            if (!chkPrimaryTenant.Checked)
            {
                decNuyen /= intRoommatesValue + 1.0m;
            }
            decNuyen += decExtraCostServicesOutings + (decExtraCostServicesOutings * (intMultiplier / 100.0m)); ;
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

        /// <summary>
        /// Sort the contents of a TreeView alphabetically.
        /// </summary>
        /// <param name="treTree">TreeView to sort.</param>
        private static void SortTree(TreeView treTree)
        {
            List<TreeNode> lstNodes = new List<TreeNode>();
            foreach (TreeNode objNode in treTree.Nodes)
                lstNodes.Add(objNode);
            treTree.Nodes.Clear();
            try
            {
                lstNodes.Sort(CompareTreeNodes.CompareText);
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
