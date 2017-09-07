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

namespace Chummer
{
    public partial class frmSelectLifestyle : Form
    {
        private bool _blnAddAgain = false;
        private Lifestyle _objLifestyle;
        private Lifestyle _objSourceLifestyle;
        private readonly Character _objCharacter;
        private LifestyleType _objType = LifestyleType.Standard;

        private XmlDocument _objXmlDocument = new XmlDocument();

        private bool _blnSkipRefresh = false;

        #region Control Events
        public frmSelectLifestyle(Lifestyle objLifestyle, Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            _objCharacter = objCharacter;
            _objLifestyle = objLifestyle;
            MoveControls();
        }

        private void frmSelectLifestyle_Load(object sender, EventArgs e)
        {
            _blnSkipRefresh = true;

            foreach (Label objLabel in Controls.OfType<Label>())
            {
                if (objLabel.Text.StartsWith("["))
                    objLabel.Text = string.Empty;
            }

            // Load the Lifestyles information.
            _objXmlDocument = XmlManager.Instance.Load("lifestyles.xml");

            // Populate the Lifestyle ComboBoxes.
            List<ListItem> lstLifestyle = (from XmlNode objXmlLifestyle in _objXmlDocument.SelectNodes("/chummer/lifestyles/lifestyle")
                let strLifeStyleName = objXmlLifestyle["name"]?.InnerText
                where !string.IsNullOrEmpty(strLifeStyleName) && strLifeStyleName != "ID ERROR. Re-add life style to fix" && _objCharacter.Options.Books.Contains(objXmlLifestyle["source"]?.InnerText)
                select new ListItem
                {
                    Value = strLifeStyleName, Name = objXmlLifestyle["translate"]?.InnerText ?? strLifeStyleName
                }).ToList();
            cboLifestyle.BeginUpdate();
            cboLifestyle.ValueMember = "Value";
            cboLifestyle.DisplayMember = "Name";
            cboLifestyle.DataSource = lstLifestyle;

            if (_objSourceLifestyle != null)
            {
                cboLifestyle.SelectedValue = _objLifestyle.BaseLifestyle;
            }
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
                    strBaseString = " " + LanguageManager.Instance.GetString("Label_Base");
                }
                nodOption.Tag = nodOption.Tag = objXmlOption["id"]?.InnerText;
                if (nodMultiplier != null && int.TryParse(nodMultiplier.InnerText, out int intCost))
                {
                    nodOption.Text = intCost > 0
                        ? $"{objXmlOption["translate"]?.InnerText ?? strOptionName} [+{intCost}{strBaseString}%"
                        : $"{objXmlOption["translate"]?.InnerText ?? strOptionName} [{intCost}{strBaseString}%";
                }
                else
                {
                    string strCost = objXmlOption["cost"]?.InnerText ?? string.Empty;
                    nodOption.Text = $"{objXmlOption["translate"]?.InnerText ?? strOptionName} [{strCost}¥";
                }
                treQualities.Nodes.Add(nodOption);
            }

            SortTree(treQualities);

            if (_objSourceLifestyle != null)
            {
                txtLifestyleName.Text = _objSourceLifestyle.Name;
                if (!string.IsNullOrEmpty(_objSourceLifestyle.BaseLifestyle))
                {
                    cboLifestyle.SelectedValue = _objSourceLifestyle.BaseLifestyle;
                }
                nudRoommates.Value = _objSourceLifestyle.Roommates;
                nudPercentage.Value = _objSourceLifestyle.Percentage;
                foreach (LifestyleQuality objQuality in _objSourceLifestyle.LifestyleQualities)
                {
                    foreach (TreeNode objNode in treQualities.Nodes)
                    {
                        if (objNode.Tag.ToString() == objQuality.SourceID)
                        {
                            objNode.Checked = true;
                            break;
                        }
                    }
                }
            }

            _blnSkipRefresh = false;
            CalculateValues();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtLifestyleName.Text))
            {
                MessageBox.Show(LanguageManager.Instance.GetString("Message_SelectAdvancedLifestyle_LifestyleName"), LanguageManager.Instance.GetString("MessageTitle_SelectAdvancedLifestyle_LifestyleName"), MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            _objLifestyle.BaseLifestyle = cboLifestyle.SelectedValue.ToString();
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
            string qualityID = treQualities.SelectedNode.Tag.ToString();
            XmlNode objXmlQuality = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[id = \"" + qualityID + "\"]");
            if (objXmlQuality == null) return;
            string strBook = objXmlQuality["altsource"] != null
                ? _objCharacter.Options.LanguageBookShort(objXmlQuality["altsource"].InnerText)
                : _objCharacter.Options.LanguageBookShort(objXmlQuality["source"].InnerText);
            string strPage = objXmlQuality["altpage"] != null
                ? _objCharacter.Options.LanguageBookShort(objXmlQuality["altpage"].InnerText)
                : _objCharacter.Options.LanguageBookShort(objXmlQuality["page"].InnerText);
            lblSource.Text = $"{strBook} {strPage}";

            tipTooltip.SetToolTip(lblSource,
                _objCharacter.Options.LanguageBookLong(strBook) + " " + LanguageManager.Instance.GetString("String_Page") + " " +
                strPage);
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
            XmlNode objXmlLifestyle = _objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"" + cboLifestyle.SelectedValue + "\"]");
            _objLifestyle.Source = objXmlLifestyle["source"].InnerText;
            _objLifestyle.Page = objXmlLifestyle["page"].InnerText;
            _objLifestyle.Name = txtLifestyleName.Text;
            _objLifestyle.LifestyleName = cboLifestyle.SelectedValue.ToString();
            _objLifestyle.BaseLifestyle = cboLifestyle.SelectedValue.ToString();
            _objLifestyle.Cost = Convert.ToInt32(objXmlLifestyle["cost"].InnerText);
            _objLifestyle.Roommates = Convert.ToInt32(nudRoommates.Value);
            _objLifestyle.Percentage = Convert.ToInt32(nudPercentage.Value);
            _objLifestyle.LifestyleQualities.Clear();
            _objLifestyle.StyleType = _objType;
            _objLifestyle.Dice = Convert.ToInt32(objXmlLifestyle["dice"].InnerText);
            _objLifestyle.Multiplier = Convert.ToInt32(objXmlLifestyle["multiplier"].InnerText);

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
            foreach (TreeNode objNode in treQualities.Nodes)
            {
                if (objNode.Checked)
                {
                    XmlNode objXmlLifestyleQuality = _objXmlDocument.SelectSingleNode($"/chummer/qualities/quality[id = \"{objNode.Tag}\"]");
                    LifestyleQuality objQuality = new LifestyleQuality(_objCharacter);
                    objQuality.Create(objXmlLifestyleQuality, _objLifestyle, _objCharacter, QualitySource.Selected, objNode);
                    _objLifestyle.LifestyleQualities.Add(objQuality);
                }
            }
            DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// Calculate the LP value for the selected items.
        /// </summary>
        private int CalculateValues(bool blnIncludePercentage = true)
        {
            if (_blnSkipRefresh)
                return 0;

            decimal decBaseCost = 0;
            decimal decCost = 0;
            decimal baseMultiplier = 0;
            // Get the base cost of the lifestyle
            XmlNode objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"" + cboLifestyle.SelectedValue + "\"]");
            decBaseCost += Convert.ToDecimal(objXmlAspect["cost"].InnerText);
            lblSource.Text = objXmlAspect["source"].InnerText + " " + objXmlAspect["page"].InnerText;

            // Add the flat costs from qualities
            foreach (TreeNode objNode in treQualities.Nodes)
            {
                if (objNode.Checked)
                {
                    XmlNode objXmlQuality = _objXmlDocument.SelectSingleNode($"/chummer/qualities/quality[id = \"{objNode.Tag}\"]");
                    if (!string.IsNullOrEmpty(objXmlQuality["cost"]?.InnerText))
                        decCost += Convert.ToDecimal(objXmlQuality["cost"].InnerText);
                }
            }

            decimal decMod = 0;
            if (blnIncludePercentage)
            {
                // Add the modifiers from qualities
                foreach (TreeNode objNode in treQualities.Nodes)
                {
                    if (!objNode.Checked) continue;
                    objXmlAspect = _objXmlDocument.SelectSingleNode($"/chummer/qualities/quality[id = \"{objNode.Tag}\"]");
                    if (!string.IsNullOrEmpty(objXmlAspect?["multiplier"]?.InnerText))
                        decMod += Convert.ToDecimal(objXmlAspect?["multiplier"]?.InnerText) / 100;
                    if (!string.IsNullOrEmpty(objXmlAspect?["multiplierbaseonly"]?.InnerText))
                        baseMultiplier += Convert.ToDecimal(objXmlAspect?["multiplierbaseonly"]?.InnerText) / 100;
                }

                // Check for modifiers in the improvements
                ImprovementManager objImprovementManager = new ImprovementManager(_objCharacter);
                decimal decModifier = Convert.ToDecimal(objImprovementManager.ValueOf(Improvement.ImprovementType.LifestyleCost), GlobalOptions.InvariantCultureInfo);
                decMod += decModifier / 100;
            }
            decBaseCost += decBaseCost * baseMultiplier;
            decimal decNuyen = decBaseCost + decBaseCost * decMod + decCost;

            if (nudPercentage.Value != 100)
            {
                decimal decDiscount = decNuyen;
                decDiscount = decDiscount * (nudPercentage.Value /100);
                lblCost.Text += " (" + $"{Convert.ToInt32(decDiscount):###,###,##0¥}" + ")";
            }
            int intNuyen = Convert.ToInt32(decNuyen);
            lblCost.Text = $"{intNuyen:###,###,##0¥}";
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

        private void lblSource_Click(object sender, EventArgs e)
        {
            CommonFunctions.StaticOpenPDF(lblSource.Text, _objCharacter);
        }
    }
}