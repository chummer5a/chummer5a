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
using System.Xml.XPath;
 using Chummer.Backend.Equipment;
using System.Text;

namespace Chummer
{
    public partial class frmSelectVehicleMod : Form
    {
        private Vehicle _objVehicle;
        private int _intWeaponMountSlots = 0;
        private string _strSelectedMod = string.Empty;
        private int _intSelectedRating = 0;
        private int _intWeaponCost = 0;
        private int _intTotalWeaponCost = 0;
        private int _intModMultiplier = 1;
        private int _intMarkup = 0;
        private bool _blnSkipUpdate = false;
        private static string s_StrSelectCategory = string.Empty;

        private static readonly string[] s_LstCategories = new string[6] { "Powertrain", "Protection", "Weapons", "Body", "Electromagnetic", "Cosmetic" };
        private string _strAllowedCategories = string.Empty;
        private bool _blnAddAgain = false;

        private readonly XmlDocument _objXmlDocument = null;
        private readonly Character _objCharacter;
        private bool _blnBlackMarketDiscount;
        private string _strLimitToCategories = string.Empty;
        private readonly List<ListItem> _lstCategory = new List<ListItem>();
        private readonly HashSet<string> _setBlackMarketMaps;
        private List<VehicleMod> _lstMods;

        #region Control Events
        public frmSelectVehicleMod(Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            _objCharacter = objCharacter;
            MoveControls();
            // Load the Vehicle information.
            _objXmlDocument = XmlManager.Load("vehicles.xml");
            _setBlackMarketMaps = _objCharacter.GenerateBlackMarketMappings(_objXmlDocument);
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
            XmlNodeList objXmlNodeList = _objXmlDocument.SelectNodes("/chummer/modcategories/category");
            foreach (XmlNode objXmlCategory in objXmlNodeList)
            {
                if (string.IsNullOrEmpty(_strLimitToCategories) || strValues.Any(value => value == objXmlCategory.InnerText))
                {
                    string strInnerText = objXmlCategory.InnerText;
                    _lstCategory.Add(new ListItem(strInnerText, objXmlCategory.Attributes?["translate"]?.InnerText ?? strInnerText));
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
            cmdOK_Click(sender, e);
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            cmdOK_Click(sender, e);
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
        public bool AddAgain
        {
            get
            {
                return _blnAddAgain;
            }
        }

        /// <summary>
        /// Whether or not the selected Vehicle is used.
        /// </summary>
        public bool BlackMarketDiscount
        {
            get
            {
                return _blnBlackMarketDiscount;
            }
        }

        /// <summary>
        /// Vehicle's Cost.
        /// </summary>
        public Vehicle SelectedVehicle
        {
            set
            {
                _objVehicle = value;
            }
        }

        /// <summary>
        /// The slots taken up by a weapon mount to which the vehicle mod might be being added
        /// </summary>
        public int WeaponMountSlots
        {
            set
            {
                _intWeaponMountSlots = value;
            }
        }

        /// <summary>
        /// Weapon's Cost.
        /// </summary>
        public int WeaponCost
        {
            set
            {
                _intWeaponCost = value;
            }
        }

        /// <summary>
        /// Weapon's Total Cost.
        /// </summary>
        public int TotalWeaponCost
        {
            set
            {
                _intTotalWeaponCost = value;
            }
        }

        /// <summary>
        /// Weapon's Modification Cost Multiplier.
        /// </summary>
        public int ModMultiplier
        {
            set
            {
                _intModMultiplier = value;
            }
        }

        /// <summary>
        /// Name of the Mod that was selected in the dialogue.
        /// </summary>
        public string SelectedMod
        {
            get
            {
                return _strSelectedMod;
            }
        }

        /// <summary>
        /// Rating that was selected in the dialogue.
        /// </summary>
        public int SelectedRating
        {
            get
            {
                return _intSelectedRating;
            }
        }

        /// <summary>
        /// Categories that the Gear allows to be used.
        /// </summary>
        public string AllowedCategories
        {
            get
            {
                return _strAllowedCategories;
            }
            set
            {
                _strAllowedCategories = value;
            }
        }

        /// <summary>
        /// Whether or not the item should be added for free.
        /// </summary>
        public bool FreeCost
        {
            get
            {
                return chkFreeItem.Checked;
            }
        }

        /// <summary>
        /// Markup percentage.
        /// </summary>
        public int Markup
        {
            get
            {
                return _intMarkup;
            }
        }

        /// <summary>
        /// Currently Installed Accessories
        /// </summary>
        public IList<VehicleMod> InstalledMods
        {
            set
            {
                _lstMods = (List<VehicleMod>)value;
            }
        }
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
            else if (!string.IsNullOrEmpty(_strAllowedCategories))
            {
                StringBuilder objCategoryFilter = new StringBuilder();
                foreach (string strItem in _lstCategory.Select(x => x.Value))
                {
                    if (!string.IsNullOrEmpty(strItem))
                        objCategoryFilter.Append("category = \"" + strItem + "\" or ");
                }
                if (objCategoryFilter.Length > 0)
                {
                    strFilter += " and (" + objCategoryFilter.ToString().TrimEnd(" or ") + ')';
                }
            }

            strFilter += CommonFunctions.GenerateSearchXPath(txtSearch.Text);

            // Retrieve the list of Mods for the selected Category.
            var objXmlModList = VehicleMountMods
                ? _objXmlDocument.SelectNodes("/chummer/weaponmountmods/mod[" + strFilter + "]")
                : _objXmlDocument.SelectNodes("/chummer/mods/mod[" + strFilter + "]");
            // Update the list of Mods based on the selected Category.
            XmlNode objXmlVehicleNode = _objVehicle.GetNode();
            List<ListItem> lstMods = new List<ListItem>();
            foreach (XmlNode objXmlMod in objXmlModList)
            {
                XmlNode xmlTestNode = objXmlMod.SelectSingleNode("forbidden/vehicledetails");
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
                    foreach (XmlNode node in xmlTestNode.SelectNodes("mods"))
                    {
                        setForbiddenAccessory.Add(node.InnerText);
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
                    foreach (XmlNode node in xmlTestNode.SelectNodes("mods"))
                    {
                        setRequiredAccessory.Add(node.InnerText);
                    }

                    if (!_lstMods.Any(objAccessory => setRequiredAccessory.Contains(objAccessory.Name)))
                    {
                        continue;
                    }
                }

                xmlTestNode = objXmlMod.SelectSingleNode("requires");
                if (xmlTestNode != null)
                {
                    if (_objVehicle.Seats < Convert.ToInt32(xmlTestNode["seats"]?.InnerText))
                    {
                        continue;
                    }
                }

                if (!chkHideOverAvailLimit.Checked || Backend.SelectionShared.CheckAvailRestriction(objXmlMod, _objCharacter))
                {
                    lstMods.Add(new ListItem(objXmlMod["id"].InnerText, objXmlMod["translate"]?.InnerText ?? objXmlMod["name"].InnerText));
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
            XmlNode xmlVehicleMod = null;
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                xmlVehicleMod = _objXmlDocument.SelectSingleNode("/chummer/" + (VehicleMountMods ? "weaponmountmods" : "mods") + "/mod[id = \"" + strSelectedId + "\"]");
                if (xmlVehicleMod != null)
                {
                    _strSelectedMod = strSelectedId;
                    _intSelectedRating = decimal.ToInt32(nudRating.Value);
                    _intMarkup = decimal.ToInt32(nudMarkup.Value);
                    _blnBlackMarketDiscount = chkBlackMarketDiscount.Checked;
                    s_StrSelectCategory = (_objCharacter.Options.SearchInCategoryOnly || txtSearch.TextLength == 0) ? cboCategory.SelectedValue?.ToString() : xmlVehicleMod["category"]?.InnerText;
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
            XmlNode xmlVehicleMod = null;
            string strSelectedId = lstMod.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Retireve the information for the selected Mod.
                // Filtering is also done on the Category in case there are non-unique names across categories.
                xmlVehicleMod = VehicleMountMods
                    ? _objXmlDocument.SelectSingleNode($"/chummer/weaponmountmods/mod[id = \"{strSelectedId}\"]")
                    : _objXmlDocument.SelectSingleNode($"/chummer/mods/mod[id = \"{strSelectedId}\"]");
            }

            if (xmlVehicleMod != null)
            {
                // Extract the Avil and Cost values from the Gear info since these may contain formulas and/or be based off of the Rating.
                // This is done using XPathExpression.

                int intMinRating = 1;
                if (xmlVehicleMod["minrating"]?.InnerText.Length > 0)
                {
                    string strMinRating = ReplaceStrings(xmlVehicleMod["minrating"]?.InnerText);
                    intMinRating = Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strMinRating));
                }
                bool blnDisableRating = false;
                string strRating = xmlVehicleMod["rating"]?.InnerText.ToLower();
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
                        while (nudRating.Maximum > intMinRating && !Backend.SelectionShared.CheckAvailRestriction(xmlVehicleMod, _objCharacter, decimal.ToInt32(nudRating.Maximum)))
                        {
                            nudRating.Maximum -= 1;
                        }
                    }
                    nudRating.Minimum = intMinRating;
                    nudRating.Enabled = true;
                }

                // Slots.

                string strSlots = xmlVehicleMod["slots"]?.InnerText ?? string.Empty;
                if (strSlots.StartsWith("FixedValues("))
                {
                    string[] strValues = strSlots.TrimStart("FixedValues(", true).TrimEnd(')').Split(',');
                    strSlots = strValues[decimal.ToInt32(nudRating.Value) - 1];
                }
                int.TryParse(strSlots, out int intExtraSlots);
                strSlots = ReplaceStrings(strSlots, intExtraSlots);
                try
                {
                    lblSlots.Text = CommonFunctions.EvaluateInvariantXPath(strSlots).ToString();
                }
                catch (XPathException)
                {
                    lblSlots.Text = strSlots;
                }
                int.TryParse(lblSlots.Text, out intExtraSlots);

                // Avail.
                string strAvailExpr = xmlVehicleMod["avail"]?.InnerText ?? string.Empty;
                if (strAvailExpr.StartsWith("FixedValues("))
                {
                    int intRating = decimal.ToInt32(nudRating.Value - 1);
                    strAvailExpr = strAvailExpr.TrimStart("FixedValues(", true).TrimEnd(')');
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
                try
                {
                    lblAvail.Text = Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(ReplaceStrings(strAvailExpr))).ToString() + strSuffix;
                }
                catch (XPathException)
                {
                    lblAvail.Text = strAvailExpr + strSuffix;
                }

                // Cost.
                chkBlackMarketDiscount.Enabled = true;
                chkBlackMarketDiscount.Checked = _setBlackMarketMaps.Contains(xmlVehicleMod["category"]?.InnerText);

                decimal decItemCost = 0;
                if (chkFreeItem.Checked)
                    lblCost.Text = "0";
                else
                {
                    string strCost = xmlVehicleMod["cost"]?.InnerText ?? string.Empty;
                    if (strCost.StartsWith("Variable("))
                    {
                        decimal decMin = 0;
                        decimal decMax = decimal.MaxValue;
                        strCost = strCost.TrimStart("Variable(", true).TrimEnd(')');
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

                        decItemCost = decMin;
                    }
                    else if (strCost.StartsWith("FixedValues("))
                    {
                        int intRating = decimal.ToInt32(nudRating.Value) - 1;
                        strCost = strCost.TrimStart("FixedValues(", true).TrimEnd(')');
                        string[] strValues = strCost.Split(',');
                        if (intRating < 0 || intRating > strValues.Length)
                        {
                            intRating = 0;
                        }
                        strCost = strValues[intRating];
                    }
                    strCost = ReplaceStrings(strCost, intExtraSlots);

                    decItemCost = Convert.ToDecimal(CommonFunctions.EvaluateInvariantXPath(strCost), GlobalOptions.InvariantCultureInfo);
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

                string strCategory = xmlVehicleMod["category"]?.InnerText ?? string.Empty;
                if (!string.IsNullOrEmpty(strCategory))
                {
                    if (s_LstCategories.Contains(strCategory))
                    {
                        lblVehicleCapacityLabel.Visible = true;
                        lblVehicleCapacity.Visible = true;
                        lblVehicleCapacity.Text = GetRemainingModCapacity(strCategory, Convert.ToInt32(lblSlots.Text));
                        tipTooltip.SetToolTip(lblVehicleCapacityLabel, LanguageManager.GetString("Tip_RemainingVehicleModCapacity", GlobalOptions.Language));
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
                        XmlNode objXmlCategoryTranslate = _objXmlDocument.SelectSingleNode("/chummer/modcategories/category[. = \"" + strCategory + "\"]/@translate");
                        lblCategory.Text = objXmlCategoryTranslate?.InnerText ?? strCategory;
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

                string strLimit = xmlVehicleMod["limit"]?.InnerText;
                if (!string.IsNullOrEmpty(strLimit))
                {
                    // Translate the Limit if possible.
                    if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
                    {
                        XmlNode objXmlLimit = _objXmlDocument.SelectSingleNode("/chummer/limits/limit[. = \"" + strLimit + "\"/@translate]");
                        lblLimit.Text = " (" + objXmlLimit?.InnerText ?? strLimit + ')';
                    }
                    else
                        lblLimit.Text = " (" + strLimit + ')';
                }
                else
                    lblLimit.Text = string.Empty;

                string strSource = xmlVehicleMod["source"]?.InnerText;
                string strPage = xmlVehicleMod["altpage"]?.InnerText ?? xmlVehicleMod["page"].InnerText;
                lblSource.Text = CommonFunctions.LanguageBookShort(strSource, GlobalOptions.Language) + ' ' + strPage;

                tipTooltip.SetToolTip(lblSource, CommonFunctions.LanguageBookLong(strSource, GlobalOptions.Language) + ' ' + LanguageManager.GetString("String_Page", GlobalOptions.Language) + ' ' + strPage);
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
                tipTooltip.SetToolTip(lblSource, string.Empty);
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
            objInputBuilder.Replace("Weapon Cost", _intWeaponCost.ToString());
            objInputBuilder.Replace("Total Cost", _intTotalWeaponCost.ToString());
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
