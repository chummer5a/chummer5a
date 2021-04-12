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
 using System.Text;
 using System.Windows.Forms;
using System.Xml;
 using System.Xml.XPath;
 using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class frmSelectArmorMod : Form
    {
        private bool _blnLoading = true;
        private decimal _decArmorCapacity;
        private decimal _decArmorCost;
        private CapacityStyle _eCapacityStyle = CapacityStyle.Standard;
        private readonly XPathNavigator _objParentNode;
        private readonly XPathNavigator _xmlBaseDataNode;
        private readonly Character _objCharacter;
        private readonly Armor _objArmor;
        private readonly HashSet<string> _setBlackMarketMaps;

        #region Control Events
        public frmSelectArmorMod(Character objCharacter, Armor objParentNode = null)
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            // Load the Armor information.
            _xmlBaseDataNode = _objCharacter.LoadDataXPath("armor.xml").SelectSingleNode("/chummer");
            _objArmor = objParentNode;
            _objParentNode = (_objArmor as IHasXmlNode)?.GetNode()?.CreateNavigator();
            if (_xmlBaseDataNode != null)
                _setBlackMarketMaps = _objCharacter.GenerateBlackMarketMappings(_xmlBaseDataNode.SelectSingleNode("modcategories"));
        }

        private void frmSelectArmorMod_Load(object sender, EventArgs e)
        {
            if (_objCharacter.Created)
            {
                chkHideOverAvailLimit.Visible = false;
                chkHideOverAvailLimit.Checked = false;
                lblMarkupLabel.Visible = true;
                nudMarkup.Visible = true;
                lblMarkupPercentLabel.Visible = true;
            }
            else
            {
                chkHideOverAvailLimit.Text = string.Format(GlobalOptions.CultureInfo, chkHideOverAvailLimit.Text, _objCharacter.Options.MaximumAvailability);
                chkHideOverAvailLimit.Checked = GlobalOptions.HideItemsOverAvailLimit;
                lblMarkupLabel.Visible = false;
                nudMarkup.Visible = false;
                lblMarkupPercentLabel.Visible = false;
            }
            chkBlackMarketDiscount.Visible = _objCharacter.BlackMarketDiscount;
            _blnLoading = false;
            RefreshList();
        }

        private void lstMod_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelectedArmor();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            AddAgain = false;
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void nudRating_ValueChanged(object sender, EventArgs e)
        {
            UpdateSelectedArmor();
        }

        private void lstMod_DoubleClick(object sender, EventArgs e)
        {
            AddAgain = false;
            AcceptForm();
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            AddAgain = true;
            AcceptForm();
        }

        private void chkFreeItem_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShowOnlyAffordItems.Checked)
            {
                RefreshList();
            }
            UpdateSelectedArmor();
        }

        private void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            if (chkShowOnlyAffordItems.Checked && !chkFreeItem.Checked)
            {
                RefreshList();
            }
            UpdateSelectedArmor();
        }
		#endregion

		#region Properties
		/// <summary>
		/// Whether or not the user wants to add another item after this one.
		/// </summary>
		public bool AddAgain { get; private set; }

		/// <summary>
		/// Armor's Cost.
		/// </summary>
		public decimal ArmorCost
        {
            set => _decArmorCost = value;
        }

        /// <summary>
        /// Armor's Cost.
        /// </summary>
        public decimal ArmorCapacity
        {
            set => _decArmorCapacity = value;
        }

		/// <summary>
		/// Whether or not the selected Vehicle is used.
		/// </summary>
		public bool BlackMarketDiscount { get; private set; }

		/// <summary>
		/// Name of Accessory that was selected in the dialogue.
		/// </summary>
		public string SelectedArmorMod { get; private set; } = string.Empty;

        /// <summary>
        /// Rating that was selected in the dialogue.
        /// </summary>
        public int SelectedRating => nudRating.ValueAsInt;

		/// <summary>
		/// Categories that the Armor allows to be used.
		/// </summary>
		public string AllowedCategories { get; set; } = string.Empty;

		/// <summary>
		/// Whether or not the General category should be included.
		/// </summary>
		public bool ExcludeGeneralCategory { get; set; }

		/// <summary>
		/// Whether or not the item should be added for free.
		/// </summary>
		public bool FreeCost => chkFreeItem.Checked;

		/// <summary>
		/// Markup percentage.
		/// </summary>
		public decimal Markup { get; private set; }

		/// <summary>
		/// Capacity display style.
		/// </summary>
		public CapacityStyle CapacityDisplayStyle
        {
            set => _eCapacityStyle = value;
        }
        #endregion

        #region Methods
        private void chkBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSelectedArmor();
        }
        /// <summary>
        /// Update the information for the selected Armor Mod.
        /// </summary>
        private void UpdateSelectedArmor()
        {
            if (_blnLoading)
                return;

            string strSelectedId = lstMod.SelectedValue?.ToString();
            XPathNavigator objXmlMod = null;
            if (!string.IsNullOrEmpty(strSelectedId))
                objXmlMod = _xmlBaseDataNode.SelectSingleNode("/chummer/mods/mod[id = " + strSelectedId.CleanXPath() + "]");
            if (objXmlMod == null)
            {
                lblALabel.Visible = false;
                lblA.Text = string.Empty;
                lblRatingLabel.Visible = false;
                lblRatingNALabel.Visible = false;
                nudRating.Enabled = false;
                nudRating.Visible = false;
                lblAvailLabel.Visible = false;
                lblAvail.Text = string.Empty;
                lblCostLabel.Visible = false;
                lblCost.Text = string.Empty;
                chkBlackMarketDiscount.Checked = false;
                lblCapacityLabel.Visible = false;
                lblCapacity.Text = string.Empty;
                lblSourceLabel.Visible = false;
                lblSource.Text = string.Empty;
                lblSource.SetToolTip(string.Empty);
                return;
            }
            // Extract the Avil and Cost values from the Cyberware info since these may contain formulas and/or be based off of the Rating.
            // This is done using XPathExpression.

            lblA.Text = objXmlMod.SelectSingleNode("armor")?.Value;
            lblALabel.Visible = !string.IsNullOrEmpty(lblA.Text);

            string strRatingLabel = objXmlMod.SelectSingleNode("ratinglabel")?.Value;
            lblRatingLabel.Text = !string.IsNullOrEmpty(strRatingLabel)
                ? string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("Label_RatingFormat"),
                    LanguageManager.GetString(strRatingLabel))
                : LanguageManager.GetString("Label_Rating");
            nudRating.Maximum = Convert.ToDecimal(objXmlMod.SelectSingleNode("maxrating")?.Value, GlobalOptions.InvariantCultureInfo);
            if (chkHideOverAvailLimit.Checked)
            {
                while (nudRating.Maximum > 1 && !objXmlMod.CheckAvailRestriction(_objCharacter, nudRating.MaximumAsInt))
                {
                    nudRating.Maximum -= 1;
                }
            }

            if (chkShowOnlyAffordItems.Checked && !chkFreeItem.Checked)
            {
                decimal decCostMultiplier = 1 + (nudMarkup.Value / 100.0m);
                if (_setBlackMarketMaps.Contains(objXmlMod.SelectSingleNode("category")?.Value))
                    decCostMultiplier *= 0.9m;
                while (nudRating.Maximum > 1 && !objXmlMod.CheckNuyenRestriction(_objCharacter.Nuyen, decCostMultiplier, nudRating.MaximumAsInt))
                {
                    nudRating.Maximum -= 1;
                }
            }

            lblRatingLabel.Visible = true;
            if (nudRating.Maximum <= 1)
            {
                lblRatingNALabel.Visible = true;
                nudRating.Visible = false;
                nudRating.Enabled = false;
            }
            else
            {
                nudRating.Enabled = nudRating.Minimum != nudRating.Maximum;
                if (nudRating.Minimum == 0)
                {
                    nudRating.Value = 1;
                    nudRating.Minimum = 1;
                }
                nudRating.Visible = true;
                lblRatingNALabel.Visible = false;
            }

            lblAvail.Text = new AvailabilityValue(Convert.ToInt32(nudRating.Value), objXmlMod.SelectSingleNode("avail")?.Value).ToString();
            lblAvailLabel.Visible = !string.IsNullOrEmpty(lblAvail.Text);

            // Cost.
            chkBlackMarketDiscount.Enabled = _objCharacter.BlackMarketDiscount;

            if (!chkBlackMarketDiscount.Checked)
            {
                chkBlackMarketDiscount.Checked = GlobalOptions.AssumeBlackMarket &&
                                                 _setBlackMarketMaps.Contains(objXmlMod.SelectSingleNode("category")
                                                     ?.Value);
            }
            else if (!_setBlackMarketMaps.Contains(objXmlMod.SelectSingleNode("category")?.Value))
            {
                //Prevent chkBlackMarketDiscount from being checked if the gear category doesn't match.
                chkBlackMarketDiscount.Checked = false;
            }

            object objProcess;
            bool blnIsSuccess;
            if (chkFreeItem.Checked)
            {
                lblCost.Text = (0.0m).ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                lblTest.Text = _objCharacter.AvailTest(0, lblAvail.Text);
            }
            else
            {
                string strCostElement = objXmlMod.SelectSingleNode("cost")?.Value ?? string.Empty;
                if (strCostElement.StartsWith("FixedValues(", StringComparison.Ordinal))
                {
                    string strSuffix = string.Empty;
                    if (!strCostElement.EndsWith(')'))
                    {
                        strSuffix = strCostElement.Substring(strCostElement.LastIndexOf(')') + 1);
                        strCostElement = strCostElement.TrimEndOnce(strSuffix);
                    }
                    string[] strValues = strCostElement.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',', StringSplitOptions.RemoveEmptyEntries);
                    strCostElement = strValues[Math.Max(Math.Min(Convert.ToInt32(nudRating.Value), strValues.Length) - 1, 0)];
                    strCostElement += strSuffix;
                }
                if (strCostElement.StartsWith("Variable(", StringComparison.Ordinal))
                {
                    decimal decMin;
                    decimal decMax = decimal.MaxValue;
                    string strCost = strCostElement.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                    if (strCost.Contains('-'))
                    {
                        string[] strValues = strCost.Split('-');
                        decMin = Convert.ToDecimal(strValues[0], GlobalOptions.InvariantCultureInfo);
                        decMax = Convert.ToDecimal(strValues[1], GlobalOptions.InvariantCultureInfo);
                    }
                    else
                        decMin = Convert.ToDecimal(strCost.FastEscape('+'), GlobalOptions.InvariantCultureInfo);

                    lblCost.Text = decMax == decimal.MaxValue
                        ? decMin.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + "¥+"
                        : decMin.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo)
                          + LanguageManager.GetString("String_Space") + '-' + LanguageManager.GetString("String_Space")
                          + decMax.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';

                    lblTest.Text = _objCharacter.AvailTest(decMin, lblAvail.Text);
                }
                else
                {
                    string strCost = strCostElement.CheapReplace("Rating", () => nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo))
                        .CheapReplace("Armor Cost", () => _decArmorCost.ToString(GlobalOptions.InvariantCultureInfo));

                    // Apply any markup.
                    objProcess = CommonFunctions.EvaluateInvariantXPath(strCost, out blnIsSuccess);
                    decimal decCost = blnIsSuccess ? Convert.ToDecimal(objProcess, GlobalOptions.InvariantCultureInfo) : 0;
                    decCost *= 1 + (nudMarkup.Value / 100.0m);

                    lblCost.Text = decCost.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';

                    lblTest.Text = _objCharacter.AvailTest(decCost, lblAvail.Text);
                }
            }

            lblCostLabel.Visible = !string.IsNullOrEmpty(lblCost.Text);
            lblTestLabel.Visible = !string.IsNullOrEmpty(lblTest.Text);

            // Capacity.
            // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
            string strCapacity = objXmlMod.SelectSingleNode("armorcapacity")?.Value;

            // Handle YNT Softweave
            if (_eCapacityStyle == CapacityStyle.Zero || string.IsNullOrEmpty(strCapacity))
                lblCapacity.Text = '['+ 0.ToString(GlobalOptions.CultureInfo) + ']';
            else
            {
                if (strCapacity.StartsWith("FixedValues(", StringComparison.Ordinal))
                {
                    string[] strValues = strCapacity.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',', StringSplitOptions.RemoveEmptyEntries);
                    strCapacity = strValues[nudRating.ValueAsInt - 1];
                }

                strCapacity = strCapacity.CheapReplace("Capacity", () => _decArmorCapacity.ToString(GlobalOptions.InvariantCultureInfo))
                    .CheapReplace("Rating", () => nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo));
                bool blnSquareBrackets = strCapacity.StartsWith('[');
                if (blnSquareBrackets)
                    strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);

                //Rounding is always 'up'. For items that generate capacity, this means making it a larger negative number.
                objProcess = CommonFunctions.EvaluateInvariantXPath(strCapacity, out blnIsSuccess);
                string strReturn = blnIsSuccess ? ((double)objProcess).ToString("#,0.##", GlobalOptions.CultureInfo) : strCapacity;
                if (blnSquareBrackets)
                    strReturn = '[' + strReturn + ']';

                lblCapacity.Text = strReturn;
            }

            lblCapacityLabel.Visible = !string.IsNullOrEmpty(lblCapacity.Text);

            string strSource = objXmlMod.SelectSingleNode("source")?.Value ?? LanguageManager.GetString("String_Unknown");
            string strPage = objXmlMod.SelectSingleNode("altpage")?.Value ?? objXmlMod.SelectSingleNode("page")?.Value ?? LanguageManager.GetString("String_Unknown");
            string strSpace = LanguageManager.GetString("String_Space");
            lblSource.Text = _objCharacter.LanguageBookShort(strSource) + strSpace + strPage;
            lblSource.SetToolTip(_objCharacter.LanguageBookLong(strSource) + strSpace + LanguageManager.GetString("String_Page") + ' ' + strPage);
            lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);
        }

        private void RefreshCurrentList(object sender, EventArgs e)
        {
            RefreshList();
        }

        /// <summary>
        /// 
        /// </summary>
        private void RefreshList()
        {
            List<ListItem> lstMods = new List<ListItem>();

            // Populate the Mods list.
            StringBuilder sbdFilter = new StringBuilder('(' + _objCharacter.Options.BookXPath() + ')');
            StringBuilder sbdCategoryFilter = new StringBuilder(ExcludeGeneralCategory ? string.Empty : "category = \"General\" or ");
            foreach (string strCategory in AllowedCategories.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries))
            {
                if (!string.IsNullOrEmpty(strCategory))
                    sbdCategoryFilter.Append("category = " + strCategory.CleanXPath() + " or ");
            }
            if (sbdCategoryFilter.Length > 0)
            {
                sbdCategoryFilter.Length -= 4;
                sbdFilter.Append(" and (").Append(sbdCategoryFilter).Append(")");
            }
            if (!string.IsNullOrEmpty(txtSearch.Text))
                sbdFilter.Append(" and ").Append(CommonFunctions.GenerateSearchXPath(txtSearch.Text));

            int intOverLimit = 0;
            XPathNodeIterator objXmlModList =
                _xmlBaseDataNode.Select("/chummer/mods/mod[" + sbdFilter + ']');
            if (objXmlModList.Count > 0)
            {
                foreach (XPathNavigator objXmlMod in objXmlModList)
                {
                    XPathNavigator xmlTestNode = objXmlMod.SelectSingleNode("forbidden/parentdetails");
                    if (xmlTestNode != null)
                    {
                        // Assumes topmost parent is an AND node
                        if (_objParentNode.ProcessFilterOperationNode(xmlTestNode, false))
                        {
                            continue;
                        }
                    }

                    xmlTestNode = objXmlMod.SelectSingleNode("required/parentdetails");
                    if (xmlTestNode != null)
                    {
                        // Assumes topmost parent is an AND node
                        if (!_objParentNode.ProcessFilterOperationNode(xmlTestNode, false))
                        {
                            continue;
                        }
                    }

                    string strId = objXmlMod.SelectSingleNode("id")?.Value;
                    if (string.IsNullOrEmpty(strId)) continue;
                    decimal decCostMultiplier = 1 + (nudMarkup.Value / 100.0m);
                    if (_setBlackMarketMaps.Contains(objXmlMod.SelectSingleNode("category")?.Value))
                        decCostMultiplier *= 0.9m;
                    if (!chkHideOverAvailLimit.Checked || objXmlMod.CheckAvailRestriction(_objCharacter) &&
                        (chkFreeItem.Checked || !chkShowOnlyAffordItems.Checked ||
                         objXmlMod.CheckNuyenRestriction(_objCharacter.Nuyen, decCostMultiplier)) && objXmlMod.RequirementsMet(_objCharacter, _objArmor))
                    {
                        lstMods.Add(new ListItem(strId, objXmlMod.SelectSingleNode("translate")?.Value ?? objXmlMod.SelectSingleNode("name")?.Value ?? LanguageManager.GetString("String_Unknown")));
                    }
                    else
                        ++intOverLimit;
                }
            }

            lstMods.Sort(CompareListItems.CompareNames);
            if (intOverLimit > 0)
            {
                // Add after sort so that it's always at the end
                lstMods.Add(new ListItem(string.Empty,
                    string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("String_RestrictedItemsHidden"),
                        intOverLimit)));
            }
            string strOldSelected = lstMod.SelectedValue?.ToString();
            _blnLoading = true;
            lstMod.BeginUpdate();
            lstMod.ValueMember = nameof(ListItem.Value);
            lstMod.DisplayMember = nameof(ListItem.Name);
            lstMod.DataSource = lstMods;
            _blnLoading = false;
            if (!string.IsNullOrEmpty(strOldSelected))
                lstMod.SelectedValue = strOldSelected;
            else
                lstMod.SelectedIndex = -1;
            lstMod.EndUpdate();
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            string strSelectedId = lstMod.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                SelectedArmorMod = strSelectedId;
                Markup = nudMarkup.Value;
                BlackMarketDiscount = chkBlackMarketDiscount.Checked;
                DialogResult = DialogResult.OK;
            }
        }

        private void OpenSourceFromLabel(object sender, EventArgs e)
        {
            CommonFunctions.OpenPdfFromControl(sender, e);
        }
        #endregion
    }
}
