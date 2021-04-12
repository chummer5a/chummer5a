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
 using System.Windows.Forms;
using System.Xml;
 using Chummer.Backend.Equipment;
 using NLog;

namespace Chummer
{
    public partial class frmSelectLifestyle : Form
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private bool _blnAddAgain;
        private readonly Lifestyle _objLifestyle;
        private Lifestyle _objSourceLifestyle;
        private readonly Character _objCharacter;

        private readonly XmlDocument _objXmlDocument;

        private bool _blnSkipRefresh = true;

        #region Control Events
        public frmSelectLifestyle(Character objCharacter)
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objCharacter = objCharacter;
            _objLifestyle = new Lifestyle(objCharacter);
            // Load the Lifestyles information.
            _objXmlDocument = objCharacter.LoadData("lifestyles.xml");
        }

        private void frmSelectLifestyle_Load(object sender, EventArgs e)
        {
            string strSelectedId = string.Empty;
            // Populate the Lifestyle ComboBoxes.
            List<ListItem> lstLifestyle = new List<ListItem>();
            using (XmlNodeList xmlLifestyleList = _objXmlDocument.SelectNodes("/chummer/lifestyles/lifestyle[" + _objCharacter.Options.BookXPath() + "]"))
                if (xmlLifestyleList?.Count > 0)
                    foreach (XmlNode objXmlLifestyle in xmlLifestyleList)
                    {
                        string strLifeStyleId = objXmlLifestyle["id"]?.InnerText;
                        if (!string.IsNullOrEmpty(strLifeStyleId) && !strLifeStyleId.IsEmptyGuid())
                        {
                            string strName = objXmlLifestyle["name"]?.InnerText ?? LanguageManager.GetString("String_Unknown");
                            if (strName == _objSourceLifestyle?.BaseLifestyle)
                                strSelectedId = strLifeStyleId;
                            lstLifestyle.Add(new ListItem(strLifeStyleId, objXmlLifestyle["translate"]?.InnerText ?? strName));
                        }
                    }

            cboLifestyle.BeginUpdate();
            cboLifestyle.ValueMember = nameof(ListItem.Value);
            cboLifestyle.DisplayMember = nameof(ListItem.Name);
            cboLifestyle.DataSource = lstLifestyle;

            if (!string.IsNullOrEmpty(strSelectedId))
                cboLifestyle.SelectedValue = strSelectedId;
            if (cboLifestyle.SelectedIndex == -1)
                cboLifestyle.SelectedIndex = 0;
            cboLifestyle.EndUpdate();

            // Populate the City ComboBox
            List<ListItem> lstCity = new List<ListItem>();

            using (XmlNodeList xmlCityList = _objXmlDocument.SelectNodes("/chummer/cities/city"))
                if (xmlCityList?.Count > 0)
                    foreach (XmlNode objXmlCity in xmlCityList)
                    {
                        string strName = objXmlCity["name"]?.InnerText ?? LanguageManager.GetString("String_Unknown");
                        lstCity.Add(new ListItem(strName, objXmlCity["translate"]?.InnerText ?? strName));
                    }
            cboCity.BeginUpdate();
            cboCity.ValueMember = nameof(ListItem.Value);
            cboCity.DisplayMember = nameof(ListItem.Name);
            cboCity.DataSource = lstCity;
            cboCity.EndUpdate();

            //Pupulate District and Borough ComboBox for the first time
            RefreshDistrictList();
            RefreshBoroughList();

            string strSpace = LanguageManager.GetString("String_Space");
            // Fill the Options list.
            using (XmlNodeList xmlLifestyleOptionsList = _objXmlDocument.SelectNodes("/chummer/qualities/quality[(source = \"SR5\" or category = \"Contracts\") and (" + _objCharacter.Options.BookXPath() + ")]"))
                if (xmlLifestyleOptionsList?.Count > 0)
                    foreach (XmlNode objXmlOption in xmlLifestyleOptionsList)
                    {
                        string strOptionName = objXmlOption["name"]?.InnerText;
                        if (string.IsNullOrEmpty(strOptionName))
                            continue;
                        XmlNode nodMultiplier = objXmlOption["multiplier"];
                        string strBaseString = string.Empty;
                        if (nodMultiplier == null)
                        {
                            nodMultiplier = objXmlOption["multiplierbaseonly"];
                            strBaseString = strSpace + LanguageManager.GetString("Label_Base");
                        }
                        TreeNode nodOption = new TreeNode
                        {
                            Tag = objXmlOption["id"]?.InnerText
                        };
                        if (nodMultiplier != null && int.TryParse(nodMultiplier.InnerText, out int intCost))
                        {
                            nodOption.Text = (objXmlOption["translate"]?.InnerText ?? strOptionName)
                                             + strSpace
                                             + (intCost > 0 ? "[+" : "[")
                                             + intCost.ToString(GlobalOptions.CultureInfo)
                                             + strBaseString + "%]";
                        }
                        else
                        {
                            string strCost = objXmlOption["cost"]?.InnerText;
                            object objProcess = CommonFunctions.EvaluateInvariantXPath(strCost, out bool blnIsSuccess);
                            decimal decCost = blnIsSuccess ? Convert.ToDecimal((double)objProcess) : 0;
                            nodOption.Text = (objXmlOption["translate"]?.InnerText ?? strOptionName)
                                             + strSpace
                                             + '['
                                             + decCost.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo)
                                             + "¥]";
                        }
                        treQualities.Nodes.Add(nodOption);
                    }

            SortTree(treQualities);



            if (_objSourceLifestyle != null)
            {
                txtLifestyleName.Text = _objSourceLifestyle.Name;
                nudRoommates.Value = _objSourceLifestyle.Roommates;
                nudPercentage.Value = _objSourceLifestyle.Percentage;

                foreach (LifestyleQuality objQuality in _objSourceLifestyle.LifestyleQualities)
                {
                    TreeNode objNode = treQualities.FindNode(objQuality.SourceIDString);
                    if (objNode != null)
                        objNode.Checked = true;
                }

                chkPrimaryTenant.Checked = _objSourceLifestyle.PrimaryTenant;
            }

            _blnSkipRefresh = false;
            CalculateValues();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtLifestyleName.Text))
            {
                Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_SelectAdvancedLifestyle_LifestyleName"), LanguageManager.GetString("MessageTitle_SelectAdvancedLifestyle_LifestyleName"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            _blnAddAgain = false;
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            AcceptForm();
        }

        private void treQualities_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            CalculateValues();
        }

        private void RefreshValues(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            CalculateValues();
        }

        private void nudRoommates_ValueChanged(object sender, EventArgs e)
        {
            if (nudRoommates.Value == 0 && !chkPrimaryTenant.Checked)
            {
                chkPrimaryTenant.Checked = true;
            }

            chkPrimaryTenant.Enabled = nudRoommates.Value > 0;

            if (_blnSkipRefresh)
                return;
            CalculateValues();
        }

        private void treQualities_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string strSource = string.Empty;
            string strPage = string.Empty;
            string strSourceIDString = treQualities.SelectedNode?.Tag.ToString();
            if (!string.IsNullOrEmpty(strSourceIDString))
            {
                XmlNode objXmlQuality = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[id = " + strSourceIDString.CleanXPath() + "]");
                if (objXmlQuality != null)
                {
                    strSource = objXmlQuality["source"]?.InnerText ?? string.Empty;
                    strPage = objXmlQuality["altpage"]?.InnerText ?? objXmlQuality["page"]?.InnerText ?? string.Empty;
                }
            }

            if (!string.IsNullOrEmpty(strSource) && !string.IsNullOrEmpty(strPage))
            {
                string strSpace = LanguageManager.GetString("String_Space");
                lblSource.Text = _objCharacter.LanguageBookShort(strSource) + strSpace + strPage;
                lblSource.SetToolTip(_objCharacter.LanguageBookLong(strSource) + strSpace + LanguageManager.GetString("String_Page") + strSpace + strPage);
            }
            else
            {
                lblSource.Text = string.Empty;
                lblSource.SetToolTip(string.Empty);
            }

            lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);
        }


        private void cboCity_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            RefreshDistrictList();
        }

        private void cboDistrict_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            RefreshBoroughList();
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
        public LifestyleType StyleType { get; set; } = LifestyleType.Standard;

        #endregion

        #region Methods
        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            string strSelectedCity = "";
            string strSelectedDistrict = "";
            string strSelectedBorough = "";

            if (!string.IsNullOrEmpty(cboCity.Text))
            {
                strSelectedCity = cboCity.Text;
            }
            if (!string.IsNullOrEmpty(cboDistrict.Text))
            {
                strSelectedDistrict = cboDistrict.Text;
            }
            if (!string.IsNullOrEmpty(cboBorough.Text))
            {
                strSelectedBorough = cboBorough.Text;
            }
  


            string strSelectedId = cboLifestyle.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                XmlNode objXmlLifestyle = _objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[id = " + strSelectedId.CleanXPath() + "]");
                if (objXmlLifestyle == null)
                    return;

                _objLifestyle.Source = objXmlLifestyle["source"]?.InnerText;
                _objLifestyle.Page = objXmlLifestyle["page"]?.InnerText;
                _objLifestyle.Name = txtLifestyleName.Text;
                _objLifestyle.BaseLifestyle = objXmlLifestyle["name"]?.InnerText;
                _objLifestyle.Cost = Convert.ToDecimal(objXmlLifestyle["cost"]?.InnerText, GlobalOptions.InvariantCultureInfo);
                _objLifestyle.Roommates = _objLifestyle.TrustFund ? 0 : nudRoommates.ValueAsInt;
                _objLifestyle.Percentage = nudPercentage.Value;
                _objLifestyle.LifestyleQualities.Clear();
                _objLifestyle.StyleType = StyleType;
                _objLifestyle.Dice = Convert.ToInt32(objXmlLifestyle["dice"]?.InnerText, GlobalOptions.InvariantCultureInfo);
                _objLifestyle.Multiplier = Convert.ToDecimal(objXmlLifestyle["multiplier"]?.InnerText, GlobalOptions.InvariantCultureInfo);
                _objLifestyle.PrimaryTenant = chkPrimaryTenant.Checked;
                _objLifestyle.City = strSelectedCity;
                _objLifestyle.District = strSelectedDistrict;
                _objLifestyle.Borough = strSelectedBorough;

                if (objXmlLifestyle.TryGetField("id", Guid.TryParse, out Guid source))
                {
                    _objLifestyle.SourceID = source;
                }
                else
                {
                    Log.Warn(new object[] { "Missing id field for xmlnode", objXmlLifestyle });
                    Utils.BreakIfDebug();
                }
                foreach (TreeNode objNode in treQualities.Nodes)
                {
                    if (!objNode.Checked) continue;
                    XmlNode objXmlLifestyleQuality = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[id = " + objNode.Tag.ToString().CleanXPath() + "]");
                    LifestyleQuality objQuality = new LifestyleQuality(_objCharacter);
                    objQuality.Create(objXmlLifestyleQuality, _objLifestyle, _objCharacter, QualitySource.Selected);
                    _objLifestyle.LifestyleQualities.Add(objQuality);
                }
                DialogResult = DialogResult.OK;
            }
        }

        /// <summary>
        /// Calculate the LP value for the selected items.
        /// </summary>
        private void CalculateValues(bool blnIncludePercentage = true)
        {
            if (_blnSkipRefresh)
                return;

            decimal decBaseCost = 0;
            decimal decCost = 0;
            decimal decMod = 0;
            // Get the base cost of the lifestyle
            string strSelectedId = cboLifestyle.SelectedValue?.ToString();
            if (strSelectedId != null)
            {
                XmlNode objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[id = " + strSelectedId.CleanXPath() + "]");

                if (objXmlAspect != null)
                {
                    decBaseCost += Convert.ToDecimal(objXmlAspect["cost"]?.InnerText, GlobalOptions.InvariantCultureInfo);
                    string strSource = objXmlAspect["source"]?.InnerText;
                    string strPage = objXmlAspect["altpage"]?.InnerText ?? objXmlAspect["page"]?.InnerText;
                    if (!string.IsNullOrEmpty(strSource) && !string.IsNullOrEmpty(strPage))
                    {
                        string strSpace = LanguageManager.GetString("String_Space");
                        lblSource.Text = _objCharacter.LanguageBookShort(strSource) + strSpace + strPage;
                        lblSource.SetToolTip(_objCharacter.LanguageBookLong(strSource) + strSpace + LanguageManager.GetString("String_Page") + strSpace + strPage);
                    }
                    else
                    {
                        lblSource.Text = LanguageManager.GetString("String_Unknown");
                        lblSource.SetToolTip(LanguageManager.GetString("String_Unknown"));
                    }

                    lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);

                    // Add the flat costs from qualities
                    foreach (TreeNode objNode in treQualities.Nodes)
                    {
                        if (objNode.Checked)
                        {
                            string strCost = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[id = " + objNode.Tag.ToString().CleanXPath() + "]/cost")?.InnerText;
                            if (!string.IsNullOrEmpty(strCost))
                            {
                                object objProcess = CommonFunctions.EvaluateInvariantXPath(strCost, out bool blnIsSuccess);
                                if (blnIsSuccess)
                                    decCost += Convert.ToDecimal(objProcess, GlobalOptions.InvariantCultureInfo);
                            }
                        }
                    }

                    decimal decBaseMultiplier = 0;
                    if (blnIncludePercentage)
                    {
                        // Add the modifiers from qualities
                        foreach (TreeNode objNode in treQualities.Nodes)
                        {
                            if (!objNode.Checked)
                                continue;
                            objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[id = " + objNode.Tag.ToString().CleanXPath() + "]");
                            if (objXmlAspect == null)
                                continue;
                            string strMultiplier = objXmlAspect["multiplier"]?.InnerText;
                            if (!string.IsNullOrEmpty(strMultiplier))
                                decMod += Convert.ToDecimal(strMultiplier, GlobalOptions.InvariantCultureInfo) / 100.0m;
                            strMultiplier = objXmlAspect["multiplierbaseonly"]?.InnerText;
                            if (!string.IsNullOrEmpty(strMultiplier))
                                decBaseMultiplier += Convert.ToDecimal(strMultiplier, GlobalOptions.InvariantCultureInfo) / 100.0m;
                        }

                        // Check for modifiers in the improvements
                        decMod += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.LifestyleCost) / 100.0m;
                    }

                    decBaseCost += decBaseCost * decBaseMultiplier;
                    if (nudRoommates.Value > 0)
                    {
                        decimal d = nudRoommates.Value * 10;
                        d += 100M;
                        d = Math.Max(d / 100, 0);
                        decBaseCost *= d;
                    }
                }
            }

            decimal decNuyen = decBaseCost + decBaseCost * decMod + decCost;

            lblCost.Text = decNuyen.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
            if (nudPercentage.Value != 100 || nudRoommates.Value != 0 && !chkPrimaryTenant.Checked)
            {
                decimal decDiscount = decNuyen;
                decDiscount *= nudPercentage.Value / 100;
                if (nudRoommates.Value != 0)
                {
                    decDiscount /= nudRoommates.Value;
                }

                lblCost.Text += LanguageManager.GetString("String_Space") + '(' + decDiscount.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + "¥)";
            }

            lblCostLabel.Visible = !string.IsNullOrEmpty(lblCost.Text);
        }

        /// <summary>
        /// Lifestyle to update when editing.
        /// </summary>
        /// <param name="objLifestyle">Lifestyle to edit.</param>
        public void SetLifestyle(Lifestyle objLifestyle)
        {
            _objSourceLifestyle = objLifestyle ?? throw new ArgumentNullException(nameof(objLifestyle));
            StyleType = objLifestyle.StyleType;
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

        /// <summary>
        /// Populates The District list after a City was selected
        /// </summary>
        private void RefreshDistrictList()
        {
            cboDistrict.DataSource = null;
            cboDistrict.Items.Clear();

            cboDistrict.BeginUpdate();
            string strSelectedCityRefresh = (string)cboCity.SelectedValue;

            List<ListItem> lstDistrict = new List<ListItem>();


            using (XmlNodeList xmlDistrictList = _objXmlDocument.SelectNodes("/chummer/cities/city[name=\"" + strSelectedCityRefresh + "\"]/district"))
                if (xmlDistrictList?.Count > 0)
                {
                    foreach (XmlNode objXmlDistrict in xmlDistrictList)
                    {
                        string strName = objXmlDistrict["name"].InnerText ?? LanguageManager.GetString("String_Unknown");
                        lstDistrict.Add(new ListItem(strName, objXmlDistrict["translate"]?.InnerText ?? strName));
                    }
                }

            cboDistrict.ValueMember = nameof(ListItem.Value);
            cboDistrict.DisplayMember = nameof(ListItem.Name);
            cboDistrict.DataSource = lstDistrict;
            cboDistrict.EndUpdate();
        }

        /// <summary>
        /// Refreshes the BoroughList based on the selected District to generate a cascading dropdown menu
        /// </summary>
        private void RefreshBoroughList()
        {
            cboBorough.DataSource = null;
            cboBorough.Items.Clear();

            
            cboBorough.BeginUpdate();
            string strSelectedDistrictRefresh = (string)cboDistrict.SelectedValue;

            List<ListItem> lstBorough = new List<ListItem>();

            using (XmlNodeList xmlBoroughList = _objXmlDocument.SelectNodes("/chummer/cities/city/district[name=\"" + strSelectedDistrictRefresh + "\"]/borough"))
                if (xmlBoroughList?.Count > 0)
                    foreach (XmlNode objXmlDistrict in xmlBoroughList)
                    {
                        string strName = objXmlDistrict["name"].InnerText ?? LanguageManager.GetString("String_Unknown");
                        lstBorough.Add(new ListItem(strName, objXmlDistrict["translate"]?.InnerText ?? strName));
                    }

            cboBorough.ValueMember = nameof(ListItem.Value);
            cboBorough.DisplayMember = nameof(ListItem.Name);
            cboBorough.DataSource = lstBorough;
            cboBorough.EndUpdate();
        }

        private void OpenSourceFromLabel(object sender, EventArgs e)
        {
            CommonFunctions.OpenPdfFromControl(sender, e);
        }
        #endregion
    }
}
