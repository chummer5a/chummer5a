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
        public frmSelectVehicleMod(Character objCharacter, IEnumerable<VehicleMod> lstExistingMods)
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            _objCharacter = objCharacter;
            MoveControls();
            // Load the Vehicle information.
            _xmlBaseVehicleDataNode = XmlManager.Load("vehicles.xml").GetFastNavigator().SelectSingleNode("/chummer");
            _setBlackMarketMaps = _objCharacter.GenerateBlackMarketMappings(_xmlBaseVehicleDataNode);
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
                chkHideOverAvailLimit.Text = chkHideOverAvailLimit.Text.Replace("{0}", _objCharacter.MaximumAvailability.ToString());
                chkHideOverAvailLimit.Checked = _objCharacter.Options.HideItemsOverAvailLimit;
            }

            string[] strValues = _strLimitToCategories.Split(',');

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
                _lstCategory.Insert(0, new ListItem("Show All", LanguageManager.GetString("String_ShowAll", GlobalOptions.Language)));
            }
            cboCategory.BeginUpdate();
            cboCategory.ValueMember = "Value";
            cboCategory.DisplayMember = "Name";
            cboCategory.DataSource = _lstCategory;

            // Select the first Category in the list.
            if (!string.IsNullOrEmpty(s_StrSelectCategory))
                cboCategory.SelectedValue = s_StrSelectCategory;

            if (cboCategory.SelectedIndex == -1 && _lstCategory.Count > 0)
                cboCategory.SelectedIndex = 0;

            cboCategory.EndUpdate();

            chkBlackMarketDiscount.Visible = _objCharacter.BlackMarketDiscount;

            _blnSkipUpdate = false;
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
            UpdateGearInfo();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            BuildModList();
        }

        private void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
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
        /// Vehicle's Cost.
        /// </summary>
        public Vehicle SelectedVehicle
        {
            set => _objVehicle = value;
        }

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
                    if (_objVehicle.Seats < Convert.ToInt32(xmlTestNode.SelectSingleNode("seats")?.Value))
                    {
                        continue;
                    }
                }

                if (!chkHideOverAvailLimit.Checked || SelectionShared.CheckAvailRestriction(objXmlMod, _objCharacter))
                {
                    lstMods.Add(new ListItem(objXmlMod.SelectSingleNode("id")?.Value, objXmlMod.SelectSingleNode("translate")?.Value ?? objXmlMod.SelectSingleNode("name")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language)));
                }
            }
            lstMods.Sort(CompareListItems.CompareNames);
            lstMod.BeginUpdate();
            lstMod.DataSource = null;
            lstMod.ValueMember = "Value";
            lstMod.DisplayMember = "Name";
            lstMod.DataSource = lstMods;
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
            if (_blnSkipUpdate)
                return;

            _blnSkipUpdate = true;
            XPathNavigator xmlVehicleMod = null;
            string strSelectedId = lstMod.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Retireve the information for the selected Mod.
                // Filtering is also done on the Category in case there are non-unique names across categories.
                xmlVehicleMod = VehicleMountMods
                    ? _xmlBaseVehicleDataNode.SelectSingleNode($"weaponmountmods/mod[id = \"{strSelectedId}\"]")
                    : _xmlBaseVehicleDataNode.SelectSingleNode($"mods/mod[id = \"{strSelectedId}\"]");
            }

            if (xmlVehicleMod != null)
            {
                // Extract the Avil and Cost values from the Gear info since these may contain formulas and/or be based off of the Rating.
                // This is done using XPathExpression.

                int intMinRating = 1;
                string strMinRating = xmlVehicleMod.SelectSingleNode("minrating")?.Value;
                if (strMinRating?.Length > 0)
                {
                    strMinRating = ReplaceStrings(strMinRating);
                    object objTempProcess = CommonFunctions.EvaluateInvariantXPath(strMinRating, out bool blnTempIsSuccess);
                    if (blnTempIsSuccess)
                        intMinRating = Convert.ToInt32(objTempProcess);
                }
                bool blnDisableRating = false;
                string strRating = xmlVehicleMod.SelectSingleNode("rating")?.Value.ToLower();
                // If the rating is "qty", we're looking at Tires instead of actual Rating, so update the fields appropriately.
                if (strRating == "qty")
                {
                    nudRating.Maximum = 20;
                    lblRatingLabel.Text = LanguageManager.GetString("Label_Qty", GlobalOptions.Language);
                }
                //Used for the Armor modifications.
                else if (strRating == "body")
                {
                    nudRating.Maximum = _objVehicle.Body;
                    lblRatingLabel.Text = LanguageManager.GetString("Label_Body", GlobalOptions.Language);
                }
                //Used for Metahuman Adjustments.
                else if (strRating == "seats")
                {
                    nudRating.Maximum = _objVehicle.TotalSeats;
                    lblRatingLabel.Text = LanguageManager.GetString("Label_Seats", GlobalOptions.Language);
                }
                else
                {
                    lblRatingLabel.Text = LanguageManager.GetString("Label_Rating", GlobalOptions.Language);
                    int intRating = Convert.ToInt32(strRating);
                    if (intRating > 0)
                    {
                        nudRating.Maximum = intRating;
                    }
                    else
                    {
                        blnDisableRating = true;
                        nudRating.Minimum = 0;
                        nudRating.Maximum = 0;
                        nudRating.Enabled = false;
                    }
                }
                if (!blnDisableRating)
                {
                    if (chkHideOverAvailLimit.Checked)
                    {
                        while (nudRating.Maximum > intMinRating && !SelectionShared.CheckAvailRestriction(xmlVehicleMod, _objCharacter, decimal.ToInt32(nudRating.Maximum)))
                        {
                            nudRating.Maximum -= 1;
                        }
                    }
                    nudRating.Minimum = intMinRating;
                    nudRating.Enabled = true;
                }

                // Slots.

                string strSlots = xmlVehicleMod.SelectSingleNode("slots")?.Value ?? string.Empty;
                if (strSlots.StartsWith("FixedValues("))
                {
                    string[] strValues = strSlots.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                    strSlots = strValues[decimal.ToInt32(nudRating.Value) - 1];
                }
                int.TryParse(strSlots, out int intExtraSlots);
                strSlots = ReplaceStrings(strSlots, intExtraSlots);
                object objProcess = CommonFunctions.EvaluateInvariantXPath(strSlots, out bool blnIsSuccess);
                lblSlots.Text = blnIsSuccess ? Convert.ToInt32(objProcess).ToString() : strSlots;

                int.TryParse(lblSlots.Text, out intExtraSlots);

                // Avail.
                string strAvailExpr = xmlVehicleMod.SelectSingleNode("avail")?.Value ?? string.Empty;
                if (strAvailExpr.StartsWith("FixedValues("))
                {
                    int intRating = decimal.ToInt32(nudRating.Value - 1);
                    strAvailExpr = strAvailExpr.TrimStartOnce("FixedValues(", true).TrimEndOnce(')');
                    string[] strValues = strAvailExpr.Split(',');
                    if (intRating > strValues.Length || intRating < 0)
                    {
                        intRating = strValues.Length - 1;
                    }
                    strAvailExpr = strValues[intRating];
                }

                // If avail contains "F" or "R", remove it from the string so we can use the expression.
                string strSuffix = string.Empty;
                char chrLastChar = strAvailExpr.Length > 0 ? strAvailExpr[strAvailExpr.Length - 1] : ' ';
                if (chrLastChar == 'F')
                {
                    strSuffix = LanguageManager.GetString("String_AvailForbidden", GlobalOptions.Language);
                    strAvailExpr = strAvailExpr.Substring(0, strAvailExpr.Length - 1);
                }
                else if (chrLastChar == 'R')
                {
                    strSuffix = LanguageManager.GetString("String_AvailRestricted", GlobalOptions.Language);
                    strAvailExpr = strAvailExpr.Substring(0, strAvailExpr.Length - 1);
                }

                strAvailExpr = ReplaceStrings(strAvailExpr);
                objProcess = CommonFunctions.EvaluateInvariantXPath(strAvailExpr, out blnIsSuccess);
                lblAvail.Text = (blnIsSuccess ? Convert.ToInt32(objProcess).ToString() : strAvailExpr) + strSuffix;

                // Cost.
                chkBlackMarketDiscount.Enabled = true;
                chkBlackMarketDiscount.Checked = _setBlackMarketMaps.Contains(xmlVehicleMod.SelectSingleNode("category")?.Value);

                decimal decItemCost = 0;
                if (chkFreeItem.Checked)
                    lblCost.Text = (0.0m).ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                else
                {
                    string strCost = xmlVehicleMod.SelectSingleNode("cost")?.Value ?? string.Empty;
                    if (strCost.StartsWith("Variable("))
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
                            lblCost.Text = decMin.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + " - " + decMax.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';

                        strCost = decMin.ToString(GlobalOptions.InvariantCultureInfo);
                    }
                    else if (strCost.StartsWith("FixedValues("))
                    {
                        int intRating = decimal.ToInt32(nudRating.Value) - 1;
                        strCost = strCost.TrimStartOnce("FixedValues(", true).TrimEndOnce(')');
                        string[] strValues = strCost.Split(',');
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

                // Update the Avail Test Label.
                lblTest.Text = _objCharacter.AvailTest(decItemCost, lblAvail.Text);

                string strCategory = xmlVehicleMod.SelectSingleNode("category")?.Value ?? string.Empty;
                if (!string.IsNullOrEmpty(strCategory))
                {
                    if (s_LstCategories.Contains(strCategory))
                    {
                        lblVehicleCapacityLabel.Visible = true;
                        lblVehicleCapacity.Visible = true;
                        lblVehicleCapacity.Text = GetRemainingModCapacity(strCategory, Convert.ToInt32(lblSlots.Text));
                        GlobalOptions.ToolTipProcessor.SetToolTip(lblVehicleCapacityLabel, LanguageManager.GetString("Tip_RemainingVehicleModCapacity", GlobalOptions.Language));
                    }
                    else
                    {
                        lblVehicleCapacityLabel.Visible = false;
                        lblVehicleCapacity.Visible = false;
                    }

                    if (strCategory == "Weapon Mod")
                        lblCategory.Text = LanguageManager.GetString("String_WeaponModification", GlobalOptions.Language);
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

                string strLimit = xmlVehicleMod.SelectSingleNode("limit")?.Value;
                if (!string.IsNullOrEmpty(strLimit))
                {
                    // Translate the Limit if possible.
                    if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
                    {
                        XPathNavigator objXmlLimit = _xmlBaseVehicleDataNode.SelectSingleNode("limits/limit[. = \"" + strLimit + "\"/@translate]");
                        lblLimit.Text = " (" + objXmlLimit?.Value ?? strLimit + ')';
                    }
                    else
                        lblLimit.Text = " (" + strLimit + ')';
                }
                else
                    lblLimit.Text = string.Empty;

                string strSource = xmlVehicleMod.SelectSingleNode("source")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
                string strPage = xmlVehicleMod.SelectSingleNode("altpage")?.Value ?? xmlVehicleMod.SelectSingleNode("page")?.Value ?? LanguageManager.GetString("String_Unknown", GlobalOptions.Language);
                string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                lblSource.Text = CommonFunctions.LanguageBookShort(strSource, GlobalOptions.Language) + strSpaceCharacter + strPage;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblSource, CommonFunctions.LanguageBookLong(strSource, GlobalOptions.Language) + strSpaceCharacter + LanguageManager.GetString("String_Page", GlobalOptions.Language) + strSpaceCharacter + strPage);
            }
            else
            {
                lblRatingLabel.Text = string.Empty;
                nudRating.Minimum = 0;
                nudRating.Maximum = 0;
                nudRating.Enabled = false;
                lblSlots.Text = string.Empty;
                chkBlackMarketDiscount.Enabled = false;
                chkBlackMarketDiscount.Checked = false;
                lblAvail.Text = string.Empty;
                lblCost.Text = string.Empty;
                lblTest.Text = string.Empty;
                lblCategory.Text = string.Empty;
                lblVehicleCapacityLabel.Visible = false;
                lblVehicleCapacity.Visible = false;
                lblLimit.Text = string.Empty;
                lblSource.Text = string.Empty;
                GlobalOptions.ToolTipProcessor.SetToolTip(lblSource, string.Empty);
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

        private void MoveControls()
        {
            int intWidth = Math.Max(lblCategoryLabel.Width, lblAvailLabel.Width);
            intWidth = Math.Max(intWidth, lblCostLabel.Width);
            intWidth = Math.Max(intWidth, lblSlotsLabel.Width);
            intWidth = Math.Max(intWidth, lblRatingLabel.Width);

            lblCategory.Left = lblCategoryLabel.Left + intWidth + 6;
            lblLimit.Left = lblCategoryLabel.Left + intWidth + 6;
            lblAvail.Left = lblAvailLabel.Left + intWidth + 6;
            lblTestLabel.Left = lblAvail.Left + lblAvail.Width + 16;
            lblTest.Left = lblTestLabel.Left + lblTestLabel.Width + 6;
            lblCost.Left = lblCostLabel.Left + intWidth + 6;
            lblSlots.Left = lblSlotsLabel.Left + intWidth + 6;
            nudRating.Left = lblRatingLabel.Left + intWidth + 6;

            nudMarkup.Left = lblMarkupLabel.Left + lblMarkupLabel.Width + 6;
            lblMarkupPercentLabel.Left = nudMarkup.Left + nudMarkup.Width;

            lblSource.Left = lblSourceLabel.Left + lblSourceLabel.Width + 6;

            lblSearchLabel.Left = txtSearch.Left - 6 - lblSearchLabel.Width;
        }
        private string ReplaceStrings(string strInput, int intExtraSlots = 0)
        {
            StringBuilder objInputBuilder = new StringBuilder(strInput);
            objInputBuilder.Replace("Rating", nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo));
            objInputBuilder.Replace("Vehicle Cost", _objVehicle.Cost);
            objInputBuilder.Replace("Weapon Cost", 0.ToString());
            objInputBuilder.Replace("Total Cost", 0.ToString());
            objInputBuilder.Replace("Body", _objVehicle.Body.ToString());
            objInputBuilder.Replace("Handling", _objVehicle.Handling.ToString());
            objInputBuilder.Replace("Offroad Handling", _objVehicle.OffroadHandling.ToString());
            objInputBuilder.Replace("Speed", _objVehicle.Speed.ToString());
            objInputBuilder.Replace("Offroad Speed", _objVehicle.OffroadSpeed.ToString());
            objInputBuilder.Replace("Acceleration", _objVehicle.Accel.ToString());
            objInputBuilder.Replace("Offroad Acceleration", _objVehicle.OffroadAccel.ToString());
            objInputBuilder.Replace("Sensor", _objVehicle.BaseSensor.ToString());
            objInputBuilder.Replace("Armor", _objVehicle.Armor.ToString());
            objInputBuilder.Replace("Slots", (_intWeaponMountSlots + intExtraSlots).ToString());

            return objInputBuilder.ToString();
        }
        #endregion
    }
}
