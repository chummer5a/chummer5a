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

namespace Chummer
{
    public partial class frmSelectLifestyle : Form
    {
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
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            _objCharacter = objCharacter;
            _objLifestyle = new Lifestyle(objCharacter);
            // Load the Lifestyles information.
            _objXmlDocument = XmlManager.Load("lifestyles.xml");
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
                            string strName = objXmlLifestyle["name"]?.InnerText ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
                            if (strName == _objLifestyle?.BaseLifestyle)
                                strSelectedId = strLifeStyleId;
                            lstLifestyle.Add(new ListItem(strLifeStyleId, objXmlLifestyle["translate"]?.InnerText ?? strName));
                        }
                    }

            cboLifestyle.BeginUpdate();
            cboLifestyle.ValueMember = "Value";
            cboLifestyle.DisplayMember = "Name";
            cboLifestyle.DataSource = lstLifestyle;

            if (!string.IsNullOrEmpty(strSelectedId))
                cboLifestyle.SelectedValue = strSelectedId;
            if (cboLifestyle.SelectedIndex == -1)
                cboLifestyle.SelectedIndex = 0;
            cboLifestyle.EndUpdate();

            string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
            // Fill the Options list.
            using (XmlNodeList xmlLifestyleOptionsList = _objXmlDocument.SelectNodes("/chummer/qualities/quality[(source = \"" + "SR5" + "\" or category = \"" + "Contracts" + "\") and (" + _objCharacter.Options.BookXPath() + ")]"))
                if (xmlLifestyleOptionsList?.Count > 0)
                    foreach (XmlNode objXmlOption in xmlLifestyleOptionsList)
                    {
                        string strOptionName = objXmlOption["name"]?.InnerText;
                        if (string.IsNullOrEmpty(strOptionName))
                            continue;
                        TreeNode nodOption = new TreeNode();
                        XmlNode nodMultiplier = objXmlOption["multiplier"];
                        string strBaseString = string.Empty;
                        if (nodMultiplier == null)
                        {
                            nodMultiplier = objXmlOption["multiplierbaseonly"];
                            strBaseString = strSpaceCharacter + LanguageManager.GetString("Label_Base", GlobalOptions.Language);
                        }
                        nodOption.Tag = objXmlOption["id"]?.InnerText;
                        if (nodMultiplier != null && int.TryParse(nodMultiplier.InnerText, out int intCost))
                        {
                            nodOption.Text = intCost > 0
                                ? $"{objXmlOption["translate"]?.InnerText ?? strOptionName} [+{intCost}{strBaseString}%]"
                                : $"{objXmlOption["translate"]?.InnerText ?? strOptionName} [{intCost}{strBaseString}%]";
                        }
                        else
                        {
                            string strCost = objXmlOption["cost"]?.InnerText;
                            object objProcess = CommonFunctions.EvaluateInvariantXPath(strCost, out bool blnIsSuccess);
                            decimal decCost = blnIsSuccess ? Convert.ToDecimal((double)objProcess) : 0;
                            nodOption.Text = $"{objXmlOption["translate"]?.InnerText ?? strOptionName} [{decCost.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo)}¥]";
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
                    TreeNode objNode = treQualities.FindNode(objQuality.SourceID);
                    if (objNode != null)
                        objNode.Checked = true;
                }
            }

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
            CalculateValues();
        }

        private void cboLifestyle_SelectedIndexChanged(object sender, EventArgs e)
        {
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

        private void treQualities_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string strSource = string.Empty;
            string strPage = string.Empty;
            string strQualityId = treQualities.SelectedNode?.Tag.ToString();
            if (!string.IsNullOrEmpty(strQualityId))
            {
                XmlNode objXmlQuality = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[id = \"" + strQualityId + "\"]");
                if (objXmlQuality != null)
                {
                    strSource = objXmlQuality["source"]?.InnerText ?? string.Empty;
                    strPage = objXmlQuality["altpage"]?.InnerText ?? objXmlQuality["page"]?.InnerText ?? string.Empty;
                }
            }

            if (!string.IsNullOrEmpty(strSource) && !string.IsNullOrEmpty(strPage))
            {
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                lblSource.Text = CommonFunctions.LanguageBookShort(strSource, GlobalOptions.Language) + strSpaceCharacter + strPage;
                lblSource.SetToolTip(CommonFunctions.LanguageBookLong(strSource, GlobalOptions.Language) + strSpaceCharacter + LanguageManager.GetString("String_Page", GlobalOptions.Language) + strSpaceCharacter + strPage);
            }
            else
            {
                lblSource.Text = string.Empty;
                lblSource.SetToolTip(string.Empty);
            }

            lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);
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
            string strSelectedId = cboLifestyle.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                XmlNode objXmlLifestyle = _objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[id = \"" + strSelectedId + "\"]");
                if (objXmlLifestyle == null)
                    return;

                _objLifestyle.Source = objXmlLifestyle["source"]?.InnerText;
                _objLifestyle.Page = objXmlLifestyle["page"]?.InnerText;
                _objLifestyle.Name = txtLifestyleName.Text;
                _objLifestyle.BaseLifestyle = objXmlLifestyle["name"]?.InnerText;
                _objLifestyle.Cost = Convert.ToDecimal(objXmlLifestyle["cost"]?.InnerText, GlobalOptions.InvariantCultureInfo);
                _objLifestyle.Roommates = _objLifestyle.TrustFund ? 0 : decimal.ToInt32(nudRoommates.Value);
                _objLifestyle.Percentage = nudPercentage.Value;
                _objLifestyle.LifestyleQualities.Clear();
                _objLifestyle.StyleType = StyleType;
                _objLifestyle.Dice = Convert.ToInt32(objXmlLifestyle["dice"]?.InnerText);
                _objLifestyle.Multiplier = Convert.ToDecimal(objXmlLifestyle["multiplier"]?.InnerText, GlobalOptions.InvariantCultureInfo);

                if (objXmlLifestyle.TryGetField("id", Guid.TryParse, out Guid source))
                {
                    _objLifestyle.SourceID = source;
                }
                else
                {
                    Log.Warning(new object[] { "Missing id field for lifestyle xmlnode", objXmlLifestyle });
                    Utils.BreakIfDebug();
                }
                foreach (TreeNode objNode in treQualities.Nodes)
                {
                    if (objNode.Checked)
                    {
                        XmlNode objXmlLifestyleQuality = _objXmlDocument.SelectSingleNode($"/chummer/qualities/quality[id = \"{objNode.Tag}\"]");
                        LifestyleQuality objQuality = new LifestyleQuality(_objCharacter);
                        objQuality.Create(objXmlLifestyleQuality, _objLifestyle, _objCharacter, QualitySource.Selected);
                        _objLifestyle.LifestyleQualities.Add(objQuality);
                    }
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
                XmlNode objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[id = \"" + strSelectedId + "\"]");

                if (objXmlAspect != null)
                {
                    decBaseCost += Convert.ToDecimal(objXmlAspect["cost"]?.InnerText, GlobalOptions.InvariantCultureInfo);
                    string strSource = objXmlAspect["source"]?.InnerText;
                    string strPage = objXmlAspect["altpage"]?.InnerText ?? objXmlAspect["page"]?.InnerText;
                    if (!string.IsNullOrEmpty(strSource) && !string.IsNullOrEmpty(strPage))
                    {
                        string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                        lblSource.Text = CommonFunctions.LanguageBookShort(strSource, GlobalOptions.Language) + strSpaceCharacter + strPage;
                        lblSource.SetToolTip(CommonFunctions.LanguageBookLong(strSource, GlobalOptions.Language) + strSpaceCharacter + LanguageManager.GetString("String_Page", GlobalOptions.Language) + strSpaceCharacter + strPage);
                    }
                    else
                    {
                        lblSource.Text = LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
                        lblSource.SetToolTip(LanguageManager.GetString("String_Unknown", GlobalOptions.Language));
                    }

                    lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);

                    // Add the flat costs from qualities
                    foreach (TreeNode objNode in treQualities.Nodes)
                    {
                        if (objNode.Checked)
                        {
                            string strCost = _objXmlDocument.SelectSingleNode($"/chummer/qualities/quality[id = \"{objNode.Tag}\"]/cost")?.InnerText;
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
                            objXmlAspect = _objXmlDocument.SelectSingleNode($"/chummer/qualities/quality[id = \"{objNode.Tag}\"]");
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
                        decimal decModifier = Convert.ToDecimal(ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.LifestyleCost), GlobalOptions.InvariantCultureInfo);
                        decMod += decModifier / 100.0m;
                    }

                    decBaseCost += decBaseCost * decBaseMultiplier;
                }
            }

            decimal decNuyen = decBaseCost + decBaseCost * decMod + decCost;

            lblCost.Text = decNuyen.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
            if (nudPercentage.Value != 100)
            {
                decimal decDiscount = decNuyen;
                decDiscount = decDiscount * (nudPercentage.Value / 100);
                lblCost.Text += " (" + decDiscount.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + "¥)";
            }

            lblCostLabel.Visible = !string.IsNullOrEmpty(lblCost.Text);
        }

        /// <summary>
        /// Lifestyle to update when editing.
        /// </summary>
        /// <param name="objLifestyle">Lifestyle to edit.</param>
        public void SetLifestyle(Lifestyle objLifestyle)
        {
            _objSourceLifestyle = objLifestyle;
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

        private void OpenSourceFromLabel(object sender, EventArgs e)
        {
            CommonFunctions.OpenPDFFromControl(sender, e);
        }
        #endregion
    }
}
