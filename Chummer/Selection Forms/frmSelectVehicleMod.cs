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
 using System.Globalization;
 using System.Linq;
using System.Windows.Forms;
using System.Xml.XPath;
 using Chummer.Backend.Equipment;
using System.Text;

namespace Chummer
{
    public partial class frmSelectVehicleMod : Form
    {
        private Vehicle _objVehicle;
        private int _intWeaponMountSlots;
        private int _intModMultiplier = 1;
        private int _intMarkup;
        private bool _blnLoading = true;
        private bool _blnSkipUpdate;
        private static string s_StrSelectCategory = string.Empty;

        private static readonly string[] s_LstCategories = { "Powertrain", "Protection", "Weapons", "Body", "Electromagnetic", "Cosmetic" };
        private bool _blnAddAgain;

        private readonly XPathNavigator _xmlBaseVehicleDataNode;
        private readonly Character _objCharacter;
        private bool _blnBlackMarketDiscount;
        private readonly string _strLimitToCategories = string.Empty;
        private readonly List<ListItem> _lstCategory = new List<ListItem>();
        private readonly HashSet<string> _setBlackMarketMaps;
        private readonly List<VehicleMod> _lstMods = new List<VehicleMod>();

        #region Control Events
        public frmSelectVehicleMod(Character objCharacter, Vehicle objVehicle, IEnumerable<VehicleMod> lstExistingMods = null)
        {
            InitializeComponent();
            this.TranslateWinForm();
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            _objVehicle = objVehicle ?? throw new ArgumentNullException(nameof(objVehicle));
            // Load the Vehicle information.
            _xmlBaseVehicleDataNode = XmlManager.Load("vehicles.xml").GetFastNavigator().SelectSingleNode("/chummer");
            if (_xmlBaseVehicleDataNode != null)
                _setBlackMarketMaps = _objCharacter.GenerateBlackMarketMappings(_xmlBaseVehicleDataNode.SelectSingleNode("modcategories"));
            if (lstExistingMods != null)
                _lstMods.AddRange(lstExistingMods);
        }

        private void frmSelectVehicleMod_Load(object sender, EventArgs e)
        {
            if (_objCharacter.Created)
            {
                lblMarkupLabel.Visible = true;
                nudMarkup.Visible = true;
                lblMarkupPercentLabel.Visible = true;
                chkHideOverAvailLimit.Visible = false;
                chkHideOverAvailLimit.Checked = false;
            }
            else
            {
                lblMarkupLabel.Visible = false;
                nudMarkup.Visible = false;
                lblMarkupPercentLabel.Visible = false;
                chkHideOverAvailLimit.Text = string.Format(GlobalOptions.CultureInfo, chkHideOverAvailLimit.Text, _objCharacter.MaximumAvailability.ToString(GlobalOptions.CultureInfo));
                chkHideOverAvailLimit.Checked = _objCharacter.Options.HideItemsOverAvailLimit;
            }
            chkBlackMarketDiscount.Visible = _objCharacter.BlackMarketDiscount;

            string[] strValues = _strLimitToCategories.Split(',', StringSplitOptions.RemoveEmptyEntries);

            // Populate the Category list.
            foreach (XPathNavigator objXmlCategory in _xmlBaseVehicleDataNode.Select("modcategories/category"))
            {
                string strInnerText = objXmlCategory.Value;
                if (string.IsNullOrEmpty(_strLimitToCategories) || strValues.Any(value => value == strInnerText))
                {
                    _lstCategory.Add(new ListItem(strInnerText, objXmlCategory.SelectSingleNode("@translate")?.Value ?? strInnerText));
                }
            }
            _lstCategory.Sort(CompareListItems.CompareNames);
            if (_lstCategory.Count > 0)
            {
                _lstCategory.Insert(0, new ListItem("Show All", LanguageManager.GetString("String_ShowAll")));
            }
            cboCategory.BeginUpdate();
            cboCategory.ValueMember = nameof(ListItem.Value);
            cboCategory.DisplayMember = nameof(ListItem.Name);
            cboCategory.DataSource = _lstCategory;

            // Select the first Category in the list.
            if (!string.IsNullOrEmpty(s_StrSelectCategory))
                cboCategory.SelectedValue = s_StrSelectCategory;

            if (cboCategory.SelectedIndex == -1 && _lstCategory.Count > 0)
                cboCategory.SelectedIndex = 0;

            cboCategory.EndUpdate();

            _blnLoading = false;
            UpdateGearInfo();
        }

        private void lstMod_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateGearInfo();
        }

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            BuildModList();
        }

        private void nudRating_ValueChanged(object sender, EventArgs e)
        {
            UpdateGearInfo();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            _blnAddAgain = false;
            AcceptForm();
        }

        private void chkBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
        {
            UpdateGearInfo();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            s_StrSelectCategory = string.Empty;
            DialogResult = DialogResult.Cancel;
        }

        private void lstMod_DoubleClick(object sender, EventArgs e)
        {
            _blnAddAgain = false;
            AcceptForm();
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            AcceptForm();
        }

        private void chkFreeItem_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShowOnlyAffordItems.Checked)
                BuildModList();
            UpdateGearInfo();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            BuildModList();
        }

        private void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            if (chkShowOnlyAffordItems.Checked && !chkFreeItem.Checked)
                BuildModList();
            UpdateGearInfo();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (lstMod.SelectedIndex + 1 < lstMod.Items.Count)
                {
                    lstMod.SelectedIndex++;
                }
                else if (lstMod.Items.Count > 0)
                {
                    lstMod.SelectedIndex = 0;
                }
            }
            if (e.KeyCode == Keys.Up)
            {
                if (lstMod.SelectedIndex - 1 >= 0)
                {
                    lstMod.SelectedIndex--;
                }
                else if (lstMod.Items.Count > 0)
                {
                    lstMod.SelectedIndex = lstMod.Items.Count - 1;
                }
            }
        }

        private void txtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
                txtSearch.Select(txtSearch.Text.Length, 0);
        }

        private void chkHideOverAvailLimit_CheckedChanged(object sender, EventArgs e)
        {
            BuildModList();
        }

        private void chkShowOnlyAffordItems_CheckedChanged(object sender, EventArgs e)
        {
            BuildModList();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Whether or not the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain => _blnAddAgain;

        /// <summary>
        /// Whether or not the selected Vehicle is used.
        /// </summary>
        public bool BlackMarketDiscount => _blnBlackMarketDiscount;

        /// <summary>
        /// The slots taken up by a weapon mount to which the vehicle mod might be being added
        /// </summary>
        public int WeaponMountSlots
        {
            set => _intWeaponMountSlots = value;
        }

        /// <summary>
        /// Name of the Mod that was selected in the dialogue.
        /// </summary>
        public string SelectedMod { get; private set; } = string.Empty;

        /// <summary>
        /// Rating that was selected in the dialogue.
        /// </summary>
        public int SelectedRating { get; private set; }

        /// <summary>
        /// Whether or not the item should be added for free.
        /// </summary>
        public bool FreeCost => chkFreeItem.Checked;

        /// <summary>
        /// Markup percentage.
        /// </summary>
        public int Markup => _intMarkup;

        /// <summary>
        /// Is the mod being added to a vehicle weapon mount?
        /// </summary>
        public bool VehicleMountMods { get; set; }

        #endregion

        #region Methods
        /// <summary>
        /// Build the list of Mods.
        /// </summary>
        private void BuildModList()
        {
            string strCategory = cboCategory.SelectedValue?.ToString();
            string strFilter = '(' + _objCharacter.Options.BookXPath() + ')';
            if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All" && (string.IsNullOrWhiteSpace(txtSearch.Text) || _objCharacter.Options.SearchInCategoryOnly))
                strFilter += " and category = \"" + strCategory + '\"';
            /*
            else if (!string.IsNullOrEmpty(AllowedCategories))
            {
                StringBuilder objCategoryFilter = new StringBuilder();
                foreach (string strItem in _lstCategory.Select(x => x.Value))
                {
                    if (!string.IsNullOrEmpty(strItem))
                        objCategoryFilter.Append("category = \"" + strItem + "\" or ");
                }
                if (objCategoryFilter.Length > 0)
                {
                    strFilter += " and (" + objCategoryFilter.ToString().TrimEndOnce(" or ") + ')';
                }
            }
            */

            strFilter += CommonFunctions.GenerateSearchXPath(txtSearch.Text);

            // Retrieve the list of Mods for the selected Category.
            XPathNodeIterator objXmlModList = VehicleMountMods
                ? _xmlBaseVehicleDataNode.Select("weaponmountmods/mod[" + strFilter + "]")
                : _xmlBaseVehicleDataNode.Select("mods/mod[" + strFilter + "]");
            // Update the list of Mods based on the selected Category.
            int intOverLimit = 0;
            XPathNavigator objXmlVehicleNode = _objVehicle.GetNode().CreateNavigator();
            List<ListItem> lstMods = new List<ListItem>();
            foreach (XPathNavigator objXmlMod in objXmlModList)
            {
                XPathNavigator xmlTestNode = objXmlMod.SelectSingleNode("forbidden/vehicledetails");
                if (xmlTestNode != null)
                {
                    // Assumes topmost parent is an AND node
                    if (objXmlVehicleNode.ProcessFilterOperationNode(xmlTestNode, false))
                    {
                        continue;
                    }
                }
                xmlTestNode = objXmlMod.SelectSingleNode("required/vehicledetails");
                if (xmlTestNode != null)
                {
                    // Assumes topmost parent is an AND node
                    if (!objXmlVehicleNode.ProcessFilterOperationNode(xmlTestNode, false))
                    {
                        continue;
                    }
                }

                xmlTestNode = objXmlMod.SelectSingleNode("forbidden/oneof");
                if (xmlTestNode != null)
                {
                    //Add to set for O(N log M) runtime instead of O(N * M)

                    HashSet<string> setForbiddenAccessory = new HashSet<string>();
                    foreach (XPathNavigator node in xmlTestNode.Select("mods"))
                    {
                        setForbiddenAccessory.Add(node.Value);
                    }

                    if (_lstMods.Any(objAccessory => setForbiddenAccessory.Contains(objAccessory.Name)))
                    {
                        continue;
                    }
                }

                xmlTestNode = objXmlMod.SelectSingleNode("required/oneof");
                if (xmlTestNode != null)
                {
                    //Add to set for O(N log M) runtime instead of O(N * M)

                    HashSet<string> setRequiredAccessory = new HashSet<string>();
                    foreach (XPathNavigator node in xmlTestNode.Select("mods"))
                    {
                        setRequiredAccessory.Add(node.Value);
                    }

                    if (!_lstMods.Any(objAccessory => setRequiredAccessory.Contains(objAccessory.Name)))
                    {
                        continue;
                    }
                }

                xmlTestNode = objXmlMod.SelectSingleNode("requires");
                if (xmlTestNode != null)
                {
                    if (_objVehicle.Seats < Convert.ToInt32(xmlTestNode.SelectSingleNode("seats")?.Value, GlobalOptions.InvariantCultureInfo))
                    {
                        continue;
                    }
                }

                decimal decCostMultiplier = 1 + (nudMarkup.Value / 100.0m);
                if (_setBlackMarketMaps.Contains(objXmlMod.SelectSingleNode("category")?.Value))
                    decCostMultiplier *= 0.9m;
                if ((!chkHideOverAvailLimit.Checked || objXmlMod.CheckAvailRestriction(_objCharacter)) &&
                    (!chkShowOnlyAffordItems.Checked || chkFreeItem.Checked || objXmlMod.CheckNuyenRestriction(_objCharacter.Nuyen, decCostMultiplier)))
                {
                    lstMods.Add(new ListItem(objXmlMod.SelectSingleNode("id")?.Value, objXmlMod.SelectSingleNode("translate")?.Value ?? objXmlMod.SelectSingleNode("name")?.Value ?? LanguageManager.GetString("String_Unknown")));
                }
                else
                    ++intOverLimit;
            }
            lstMods.Sort(CompareListItems.CompareNames);
            if (intOverLimit > 0)
            {
                // Add after sort so that it's always at the end
                lstMods.Add(new ListItem(string.Empty,
                    LanguageManager.GetString("String_RestrictedItemsHidden")
                    .Replace("{0}", intOverLimit.ToString(GlobalOptions.CultureInfo))));
            }
            string strOldSelected = lstMod.SelectedValue?.ToString();
            _blnLoading = true;
            lstMod.BeginUpdate();
            lstMod.ValueMember = nameof(ListItem.Value);
            lstMod.DisplayMember = nameof(ListItem.Name);
            lstMod.DataSource = lstMods;
            _blnLoading = false;
            if (string.IsNullOrEmpty(strOldSelected))
                lstMod.SelectedIndex = -1;
            else
                lstMod.SelectedValue = strOldSelected;
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
                XPathNavigator xmlVehicleMod = _xmlBaseVehicleDataNode.SelectSingleNode((VehicleMountMods ? "weaponmountmods" : "mods") + "/mod[id = \"" + strSelectedId + "\"]");
                if (xmlVehicleMod != null)
                {
                    SelectedMod = strSelectedId;
                    SelectedRating = decimal.ToInt32(nudRating.Value);
                    _intMarkup = decimal.ToInt32(nudMarkup.Value);
                    _blnBlackMarketDiscount = chkBlackMarketDiscount.Checked;
                    s_StrSelectCategory = (_objCharacter.Options.SearchInCategoryOnly || txtSearch.TextLength == 0) ? cboCategory.SelectedValue?.ToString() : xmlVehicleMod.SelectSingleNode("category")?.Value;
                    DialogResult = DialogResult.OK;
                }
            }
        }

        /// <summary>
        /// Update the Mod's information based on the Mod selected and current Rating.
        /// </summary>
        private void UpdateGearInfo()
        {
            if (_blnLoading || _blnSkipUpdate)
                return;

            _blnSkipUpdate = true;
            string strSpace = LanguageManager.GetString("String_Space");
            XPathNavigator xmlVehicleMod = null;
            string strSelectedId = lstMod.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Retireve the information for the selected Mod.
                // Filtering is also done on the Category in case there are non-unique names across categories.
                xmlVehicleMod = VehicleMountMods
                    ? _xmlBaseVehicleDataNode.SelectSingleNode("weaponmountmods/mod[id = \"" + strSelectedId + "\"]")
                    : _xmlBaseVehicleDataNode.SelectSingleNode("mods/mod[id = \"" + strSelectedId + "\"]");
            }

            if (xmlVehicleMod != null)
            {
                chkBlackMarketDiscount.Enabled = _objCharacter.BlackMarketDiscount;

                if (!chkBlackMarketDiscount.Checked)
                {
                    chkBlackMarketDiscount.Checked = GlobalOptions.AssumeBlackMarket &&
                                                     _setBlackMarketMaps.Contains(xmlVehicleMod.SelectSingleNode("category")
                                                         ?.Value);
                }
                else if (!_setBlackMarketMaps.Contains(xmlVehicleMod.SelectSingleNode("category")?.Value))
                {
                    //Prevent chkBlackMarketDiscount from being checked if the gear category doesn't match.
                    chkBlackMarketDiscount.Checked = false;
                }

                // Extract the Avil and Cost values from the Gear info since these may contain formulas and/or be based off of the Rating.
                // This is done using XPathExpression.

                int intMinRating = 1;
                string strMinRating = xmlVehicleMod.SelectSingleNode("minrating")?.Value;
                if (strMinRating?.Length > 0)
                {
                    strMinRating = ReplaceStrings(strMinRating);
                    object objTempProcess = CommonFunctions.EvaluateInvariantXPath(strMinRating, out bool blnTempIsSuccess);
                    if (blnTempIsSuccess)
                        intMinRating = Convert.ToInt32(objTempProcess, GlobalOptions.InvariantCultureInfo);
                }
                lblRatingLabel.Visible = true;
                string strRating = xmlVehicleMod.SelectSingleNode("rating")?.Value;
                if (string.IsNullOrEmpty(strRating))
                {
                    lblRatingLabel.Text = LanguageManager.GetString("Label_Rating");
                    nudRating.Minimum = 0;
                    nudRating.Maximum = 0;
                    nudRating.Visible = false;
                    lblRatingNALabel.Visible = true;
                }
                // If the rating is "qty", we're looking at Tires instead of actual Rating, so update the fields appropriately.
                else if (strRating.Equals("qty", StringComparison.OrdinalIgnoreCase))
                {
                    lblRatingLabel.Text = LanguageManager.GetString("Label_Qty");
                    nudRating.Maximum = 20;
                    nudRating.Visible = true;
                    lblRatingNALabel.Visible = false;
                }
                //Used for the Armor modifications.
                else if (strRating.Equals("body", StringComparison.OrdinalIgnoreCase))
                {
                    lblRatingLabel.Text = LanguageManager.GetString("Label_Body");
                    nudRating.Maximum = _objVehicle.Body;
                    nudRating.Visible = true;
                    lblRatingNALabel.Visible = false;
                }
                //Used for Metahuman Adjustments.
                else if (strRating.Equals("seats", StringComparison.OrdinalIgnoreCase))
                {
                    lblRatingLabel.Text = LanguageManager.GetString("Label_Seats");
                    nudRating.Maximum = _objVehicle.TotalSeats;
                    nudRating.Visible = true;
                    lblRatingNALabel.Visible = false;
                }
                else
                {
                    lblRatingLabel.Text = LanguageManager.GetString("Label_Rating");
                    int intRating = Convert.ToInt32(strRating, GlobalOptions.InvariantCultureInfo);
                    if (intRating > 0)
                    {
                        nudRating.Maximum = intRating;
                        nudRating.Visible = true;
                        lblRatingNALabel.Visible = false;
                    }
                    else
                    {
                        nudRating.Minimum = 0;
                        nudRating.Maximum = 0;
                        lblRatingNALabel.Visible = true;
                        nudRating.Visible = false;
                    }
                }
                if (nudRating.Visible)
                {
                    if (chkHideOverAvailLimit.Checked)
                    {
                        while (nudRating.Maximum > intMinRating && !xmlVehicleMod.CheckAvailRestriction(_objCharacter, decimal.ToInt32(nudRating.Maximum)))
                        {
                            nudRating.Maximum -= 1;
                        }
                    }

                    if (chkShowOnlyAffordItems.Checked && !chkFreeItem.Checked)
                    {
                        decimal decCostMultiplier = 1 + (nudMarkup.Value / 100.0m);
                        if (_setBlackMarketMaps.Contains(xmlVehicleMod.SelectSingleNode("category")?.Value))
                            decCostMultiplier *= 0.9m;
                        while (nudRating.Maximum > intMinRating && !xmlVehicleMod.CheckNuyenRestriction(_objCharacter.Nuyen, decCostMultiplier, decimal.ToInt32(nudRating.Maximum)))
                        {
                            nudRating.Maximum -= 1;
                        }
                    }
                    nudRating.Minimum = intMinRating;
                    nudRating.Enabled = nudRating.Maximum != nudRating.Minimum;
                }

                // Slots.

                string strSlots = xmlVehicleMod.SelectSingleNode("slots")?.Value ?? string.Empty;
                if (strSlots.StartsWith("FixedValues(", StringComparison.Ordinal))
                {
                    string[] strValues = strSlots.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',', StringSplitOptions.RemoveEmptyEntries);
                    strSlots = strValues[decimal.ToInt32(nudRating.Value) - 1];
                }
                int.TryParse(strSlots, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out int intExtraSlots);
                strSlots = ReplaceStrings(strSlots, intExtraSlots);
                object objProcess = CommonFunctions.EvaluateInvariantXPath(strSlots, out bool blnIsSuccess);
                lblSlots.Text = blnIsSuccess ? Convert.ToInt32(objProcess, GlobalOptions.InvariantCultureInfo).ToString(GlobalOptions.CultureInfo) : strSlots;
                lblSlotsLabel.Visible = !string.IsNullOrEmpty(lblSlots.Text);

                int.TryParse(lblSlots.Text, NumberStyles.Any, GlobalOptions.CultureInfo, out intExtraSlots);

                // Avail.
                lblAvail.Text = new AvailabilityValue(Convert.ToInt32(nudRating.Value), xmlVehicleMod.SelectSingleNode("avail")?.Value).ToString();
                lblAvailLabel.Visible = !string.IsNullOrEmpty(lblAvail.Text);

                // Cost.
                decimal decItemCost = 0;
                if (chkFreeItem.Checked)
                    lblCost.Text = (0.0m).ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                else
                {
                    string strCost = xmlVehicleMod.SelectSingleNode("cost")?.Value ?? string.Empty;
                    if (strCost.StartsWith("Variable(", StringComparison.Ordinal))
                    {
                        decimal decMin;
                        decimal decMax = decimal.MaxValue;
                        strCost = strCost.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                        if (strCost.Contains('-'))
                        {
                            string[] strValues = strCost.Split('-');
                            decMin = Convert.ToDecimal(strValues[0], GlobalOptions.InvariantCultureInfo);
                            decMax = Convert.ToDecimal(strValues[1], GlobalOptions.InvariantCultureInfo);
                        }
                        else
                            decMin = Convert.ToDecimal(strCost.FastEscape('+'), GlobalOptions.InvariantCultureInfo);

                        if (decMax == decimal.MaxValue)
                            lblCost.Text = decMin.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + "¥+";
                        else
                            lblCost.Text = decMin.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo)
                                           + strSpace + '-' + strSpace
                                           + decMax.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';

                        strCost = decMin.ToString(GlobalOptions.InvariantCultureInfo);
                    }
                    else if (strCost.StartsWith("FixedValues(", StringComparison.Ordinal))
                    {
                        int intRating = decimal.ToInt32(nudRating.Value) - 1;
                        strCost = strCost.TrimStartOnce("FixedValues(", true).TrimEndOnce(')');
                        string[] strValues = strCost.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        if (intRating < 0 || intRating > strValues.Length)
                        {
                            intRating = 0;
                        }
                        strCost = strValues[intRating];
                    }
                    strCost = ReplaceStrings(strCost, intExtraSlots);

                    objProcess = CommonFunctions.EvaluateInvariantXPath(strCost, out blnIsSuccess);
                    if (blnIsSuccess)
                        decItemCost = Convert.ToDecimal(objProcess, GlobalOptions.InvariantCultureInfo);
                    decItemCost *= _intModMultiplier;

                    // Apply any markup.
                    decItemCost *= 1 + (nudMarkup.Value / 100.0m);

                    if (chkBlackMarketDiscount.Checked)
                    {
                        decItemCost *= 0.9m;
                    }

                    lblCost.Text = decItemCost.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                }
                lblCostLabel.Visible = !string.IsNullOrEmpty(lblCost.Text);

                // Update the Avail Test Label.
                lblTest.Text = _objCharacter.AvailTest(decItemCost, lblAvail.Text);
                lblTestLabel.Visible = !string.IsNullOrEmpty(lblTest.Text);

                string strCategory = xmlVehicleMod.SelectSingleNode("category")?.Value ?? string.Empty;
                if (!string.IsNullOrEmpty(strCategory))
                {
                    if (s_LstCategories.Contains(strCategory))
                    {
                        lblVehicleCapacityLabel.Visible = true;
                        lblVehicleCapacity.Visible = true;
                        lblVehicleCapacity.Text = GetRemainingModCapacity(strCategory, Convert.ToInt32(lblSlots.Text, GlobalOptions.CultureInfo));
                        lblVehicleCapacityLabel.SetToolTip(LanguageManager.GetString("Tip_RemainingVehicleModCapacity"));
                    }
                    else
                    {
                        lblVehicleCapacityLabel.Visible = false;
                        lblVehicleCapacity.Visible = false;
                    }

                    if (strCategory == "Weapon Mod")
                        lblCategory.Text = LanguageManager.GetString("String_WeaponModification");
                    // Translate the Category if possible.
                    else if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
                    {
                        XPathNavigator objXmlCategoryTranslate = _xmlBaseVehicleDataNode.SelectSingleNode("modcategories/category[. = \"" + strCategory + "\"]/@translate");
                        lblCategory.Text = objXmlCategoryTranslate?.Value ?? strCategory;
                    }
                    else
                        lblCategory.Text = strCategory;
                }
                else
                {
                    lblCategory.Text = strCategory;
                    lblVehicleCapacityLabel.Visible = false;
                    lblVehicleCapacity.Visible = false;
                }
                lblCategoryLabel.Visible = !string.IsNullOrEmpty(lblCategory.Text);

                string strLimit = xmlVehicleMod.SelectSingleNode("limit")?.Value;
                if (!string.IsNullOrEmpty(strLimit))
                {
                    // Translate the Limit if possible.
                    if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
                    {
                        XPathNavigator objXmlLimit = _xmlBaseVehicleDataNode.SelectSingleNode("limits/limit[. = \"" + strLimit + "\"/@translate]");
                        lblLimit.Text = LanguageManager.GetString("String_Space") + '(' + objXmlLimit?.Value ?? strLimit + ')';
                    }
                    else
                        lblLimit.Text = LanguageManager.GetString("String_Space") + '(' + strLimit + ')';
                }
                else
                    lblLimit.Text = string.Empty;


                lblRatingLabel.Text = xmlVehicleMod.SelectSingleNode("ratinglabel") != null
                    ? LanguageManager.GetString("Label_RatingFormat").Replace("{0}",
                        LanguageManager.GetString(xmlVehicleMod.SelectSingleNode("ratinglabel").Value,
                            GlobalOptions.Language))
                    : LanguageManager.GetString("Label_Rating");

                string strSource = xmlVehicleMod.SelectSingleNode("source")?.Value ?? LanguageManager.GetString("String_Unknown");
                string strPage = xmlVehicleMod.SelectSingleNode("altpage")?.Value ?? xmlVehicleMod.SelectSingleNode("page")?.Value ?? LanguageManager.GetString("String_Unknown");
                lblSource.Text = CommonFunctions.LanguageBookShort(strSource) + strSpace + strPage;
                lblSource.SetToolTip(CommonFunctions.LanguageBookLong(strSource) + strSpace + LanguageManager.GetString("String_Page") + strSpace + strPage);
                lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);
            }
            else
            {
                lblRatingNALabel.Visible = false;
                lblRatingLabel.Text = string.Empty;
                nudRating.Visible = false;
                lblSlotsLabel.Visible = false;
                lblSlots.Text = string.Empty;
                chkBlackMarketDiscount.Checked = false;
                lblAvailLabel.Visible = false;
                lblAvail.Text = string.Empty;
                lblCostLabel.Visible = false;
                lblCost.Text = string.Empty;
                lblTestLabel.Visible = false;
                lblTest.Text = string.Empty;
                lblCategoryLabel.Visible = false;
                lblCategory.Text = string.Empty;
                lblVehicleCapacityLabel.Visible = false;
                lblVehicleCapacity.Visible = false;
                lblLimit.Text = string.Empty;
                lblSourceLabel.Visible = false;
                lblSource.Text = string.Empty;
                lblSource.SetToolTip(string.Empty);
            }
            _blnSkipUpdate = false;
        }

        private string GetRemainingModCapacity(string strCategory, int intModSlots)
        {
            switch (strCategory)
            {
                case "Powertrain":
                    return _objVehicle.PowertrainModSlotsUsed(intModSlots);
                case "Protection":
                    return _objVehicle.ProtectionModSlotsUsed(intModSlots);
                case "Weapons":
                    return _objVehicle.WeaponModSlotsUsed(intModSlots);
                case "Body":
                    return _objVehicle.BodyModSlotsUsed(intModSlots);
                case "Electromagnetic":
                    return _objVehicle.ElectromagneticModSlotsUsed(intModSlots);
                case "Cosmetic":
                    return _objVehicle.CosmeticModSlotsUsed(intModSlots);
                default:
                    return string.Empty;
            }
        }

        private string ReplaceStrings(string strInput, int intExtraSlots = 0)
        {
            StringBuilder objInputBuilder = new StringBuilder(strInput);
            objInputBuilder.Replace("Rating", nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo));
            objInputBuilder.Replace("Vehicle Cost", _objVehicle.Cost);
            objInputBuilder.Replace("Weapon Cost", 0.ToString(GlobalOptions.InvariantCultureInfo));
            objInputBuilder.Replace("Total Cost", 0.ToString(GlobalOptions.InvariantCultureInfo));
            objInputBuilder.Replace("Body", _objVehicle.Body.ToString(GlobalOptions.InvariantCultureInfo));
            objInputBuilder.Replace("Handling", _objVehicle.Handling.ToString(GlobalOptions.InvariantCultureInfo));
            objInputBuilder.Replace("Offroad Handling", _objVehicle.OffroadHandling.ToString(GlobalOptions.InvariantCultureInfo));
            objInputBuilder.Replace("Speed", _objVehicle.Speed.ToString(GlobalOptions.InvariantCultureInfo));
            objInputBuilder.Replace("Offroad Speed", _objVehicle.OffroadSpeed.ToString(GlobalOptions.InvariantCultureInfo));
            objInputBuilder.Replace("Acceleration", _objVehicle.Accel.ToString(GlobalOptions.InvariantCultureInfo));
            objInputBuilder.Replace("Offroad Acceleration", _objVehicle.OffroadAccel.ToString(GlobalOptions.InvariantCultureInfo));
            objInputBuilder.Replace("Sensor", _objVehicle.BaseSensor.ToString(GlobalOptions.InvariantCultureInfo));
            objInputBuilder.Replace("Armor", _objVehicle.Armor.ToString(GlobalOptions.InvariantCultureInfo));
            objInputBuilder.Replace("Slots", (_intWeaponMountSlots + intExtraSlots).ToString(GlobalOptions.InvariantCultureInfo));

            return objInputBuilder.ToString();
        }

        private void OpenSourceFromLabel(object sender, EventArgs e)
        {
            CommonFunctions.OpenPDFFromControl(sender, e);
        }
        #endregion
    }
}
