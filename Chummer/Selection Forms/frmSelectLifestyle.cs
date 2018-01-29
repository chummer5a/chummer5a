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
 using Chummer.Backend.Equipment;
using System.Xml.XPath;
using System.Globalization;

namespace Chummer
{
    public partial class frmSelectLifestyle : Form
    {
        private bool _blnAddAgain = false;
        private Lifestyle _objLifestyle;
        private Lifestyle _objSourceLifestyle;
        private readonly Character _objCharacter;
        private LifestyleType _objType = LifestyleType.Standard;

        private readonly XmlDocument _objXmlDocument = null;

        private bool _blnSkipRefresh = true;

        #region Control Events
        public frmSelectLifestyle(Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            _objCharacter = objCharacter;
            _objLifestyle = new Lifestyle(objCharacter);
            MoveControls();
            // Load the Lifestyles information.
            _objXmlDocument = XmlManager.Load("lifestyles.xml");
        }

        private void frmSelectLifestyle_Load(object sender, EventArgs e)
        {
            string strSelectedId = string.Empty;
            // Populate the Lifestyle ComboBoxes.
            List<ListItem> lstLifestyle = new List<ListItem>();
            foreach (XmlNode objXmlLifestyle in _objXmlDocument.SelectNodes("/chummer/lifestyles/lifestyle[" + _objCharacter.Options.BookXPath() + "]"))
            {
                string strLifeStyleId = objXmlLifestyle["id"]?.InnerText;
                if (!string.IsNullOrEmpty(strLifeStyleId) && !strLifeStyleId.IsEmptyGuid())
                {
                    string strName = objXmlLifestyle["name"].InnerText;
                    if (strName == _objLifestyle?.BaseLifestyle)
                        strSelectedId = strLifeStyleId;
                    lstLifestyle.Add(new ListItem(strLifeStyleId, objXmlLifestyle["translate"]?.InnerText ?? objXmlLifestyle["name"].InnerText));
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
            
            // Fill the Options list.
            foreach (XmlNode objXmlOption in _objXmlDocument.SelectNodes("/chummer/qualities/quality[(source = \"" + "SR5" + "\" or category = \"" + "Contracts" + "\") and (" + _objCharacter.Options.BookXPath() + ")]"))
            {
                TreeNode nodOption = new TreeNode();

                string strOptionName = objXmlOption["name"]?.InnerText;
                if (string.IsNullOrEmpty(strOptionName)) continue;
                XmlNode nodMultiplier = objXmlOption["multiplier"];
                string strBaseString = string.Empty;
                if (nodMultiplier == null)
                {
                    nodMultiplier = objXmlOption["multiplierbaseonly"];
                    strBaseString = ' ' + LanguageManager.GetString("Label_Base", GlobalOptions.Language);
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
                    string strCost = objXmlOption["cost"]?.InnerText ?? string.Empty;
                    if (!decimal.TryParse(strCost, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out decimal decCost))
                    {
                        try
                        {
                            decCost = Convert.ToDecimal(CommonFunctions.EvaluateInvariantXPath(strCost), GlobalOptions.InvariantCultureInfo);
                        }
                        catch (XPathException)
                        {
                            decCost = 0.0m;
                        }
                    }
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
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            cmdOK_Click(sender, e);
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
                lblSource.Text = CommonFunctions.LanguageBookShort(strSource, GlobalOptions.Language) + ' ' + strPage;
                tipTooltip.SetToolTip(lblSource, CommonFunctions.LanguageBookLong(strSource, GlobalOptions.Language) + ' ' + LanguageManager.GetString("String_Page", GlobalOptions.Language) + ' ' + strPage);
            }
            else
            {
                lblSource.Text = string.Empty;
                tipTooltip.SetToolTip(lblSource, string.Empty);
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
            string strSelectedId = cboLifestyle.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                XmlNode objXmlLifestyle = _objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[id = \"" + strSelectedId + "\"]");
                _objLifestyle.Source = objXmlLifestyle["source"].InnerText;
                _objLifestyle.Page = objXmlLifestyle["page"].InnerText;
                _objLifestyle.Name = txtLifestyleName.Text;
                _objLifestyle.BaseLifestyle = objXmlLifestyle["name"].InnerText.ToString();
                _objLifestyle.Cost = Convert.ToDecimal(objXmlLifestyle["cost"].InnerText, GlobalOptions.InvariantCultureInfo);
                _objLifestyle.Roommates = _objLifestyle.TrustFund ? 0 : decimal.ToInt32(nudRoommates.Value);
                _objLifestyle.Percentage = nudPercentage.Value;
                _objLifestyle.LifestyleQualities.Clear();
                _objLifestyle.StyleType = _objType;
                _objLifestyle.Dice = Convert.ToInt32(objXmlLifestyle["dice"].InnerText);
                _objLifestyle.Multiplier = Convert.ToDecimal(objXmlLifestyle["multiplier"].InnerText, GlobalOptions.InvariantCultureInfo);

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
                        objQuality.Create(objXmlLifestyleQuality, _objLifestyle, _objCharacter, QualitySource.Selected, objNode.Text);
                        _objLifestyle.LifestyleQualities.Add(objQuality);
                    }
                }
                DialogResult = DialogResult.OK;
            }
        }

        /// <summary>
        /// Calculate the LP value for the selected items.
        /// </summary>
        private decimal CalculateValues(bool blnIncludePercentage = true)
        {
            if (_blnSkipRefresh)
                return 0;

            decimal decBaseCost = 0;
            decimal decCost = 0;
            decimal decMod = 0;
            // Get the base cost of the lifestyle
            string strSelectedId = cboLifestyle.SelectedValue?.ToString();
            if (strSelectedId != null)
            {
                XmlNode objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[id = \"" + strSelectedId + "\"]");
                decBaseCost += Convert.ToDecimal(objXmlAspect["cost"].InnerText, GlobalOptions.InvariantCultureInfo);

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
                // Add the flat costs from qualities
                foreach (TreeNode objNode in treQualities.Nodes)
                {
                    if (objNode.Checked)
                    {
                        XmlNode objXmlQuality = _objXmlDocument.SelectSingleNode($"/chummer/qualities/quality[id = \"{objNode.Tag}\"]");
                        if (!string.IsNullOrEmpty(objXmlQuality["cost"]?.InnerText))
                        {
                            if (!decimal.TryParse(objXmlQuality["cost"].InnerText, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out decimal decLoopCost))
                            {
                                try
                                {
                                    decLoopCost = Convert.ToDecimal(CommonFunctions.EvaluateInvariantXPath(objXmlQuality["cost"].InnerText), GlobalOptions.InvariantCultureInfo);
                                }
                                catch (XPathException)
                                {
                                    decLoopCost = 0.0m;
                                }
                            }
                            decCost += decLoopCost;
                        }
                    }
                }

                decimal decBaseMultiplier = 0;
                if (blnIncludePercentage)
                {
                    // Add the modifiers from qualities
                    foreach (TreeNode objNode in treQualities.Nodes)
                    {
                        if (!objNode.Checked) continue;
                        objXmlAspect = _objXmlDocument.SelectSingleNode($"/chummer/qualities/quality[id = \"{objNode.Tag}\"]");
                        if (!string.IsNullOrEmpty(objXmlAspect?["multiplier"]?.InnerText))
                            decMod += Convert.ToDecimal(objXmlAspect["multiplier"].InnerText, GlobalOptions.InvariantCultureInfo) / 100.0m;
                        if (!string.IsNullOrEmpty(objXmlAspect?["multiplierbaseonly"]?.InnerText))
                            decBaseMultiplier += Convert.ToDecimal(objXmlAspect["multiplierbaseonly"].InnerText, GlobalOptions.InvariantCultureInfo) / 100.0m;
                    }

                    // Check for modifiers in the improvements
                    decimal decModifier = Convert.ToDecimal(ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.LifestyleCost), GlobalOptions.InvariantCultureInfo);
                    decMod += decModifier / 100.0m;
                }
                decBaseCost += decBaseCost * decBaseMultiplier;
            }

            decimal decNuyen = decBaseCost + decBaseCost * decMod + decCost;

            lblCost.Text = decNuyen.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
            if (nudPercentage.Value != 100)
            {
                decimal decDiscount = decNuyen;
                decDiscount = decDiscount * (nudPercentage.Value / 100);
                lblCost.Text += " (" + decDiscount.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + "¥)";
            }

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
            int intLeft = 0;
            intLeft = Math.Max(lblLifestyleNameLabel.Left + lblLifestyleNameLabel.Width, lblLifestyles.Left + lblLifestyles.Width);

            txtLifestyleName.Left = intLeft + 6;
            cboLifestyle.Left = intLeft + 6;
        }
        #endregion
    }
}
