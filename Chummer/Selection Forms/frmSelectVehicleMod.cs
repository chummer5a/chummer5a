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
        private List<VehicleMod> _lstMods;

        #region Control Events
        public frmSelectVehicleMod(Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            lblMarkupLabel.Visible = objCharacter.Created;
            nudMarkup.Visible = objCharacter.Created;
            lblMarkupPercentLabel.Visible = objCharacter.Created;
            _objCharacter = objCharacter;
            MoveControls();
            // Load the Vehicle information.
            _objXmlDocument = XmlManager.Load("vehicles.xml");
        }

        private void frmSelectVehicleMod_Load(object sender, EventArgs e)
        {
            foreach (Label objLabel in Controls.OfType<Label>())
            {
                if (objLabel.Text.StartsWith('['))
                    objLabel.Text = string.Empty;
            }

            if (_objCharacter.Created)
            {
                chkHideOverAvailLimit.Visible = false;
                chkHideOverAvailLimit.Checked = false;
            }
            else
            {
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
            if (string.IsNullOrEmpty(s_StrSelectCategory))
                cboCategory.SelectedIndex = 0;
            else
                cboCategory.SelectedValue = s_StrSelectCategory;

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
            if (!string.IsNullOrEmpty(lstMod.Text))
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
        /// Whether or not the Vehicle has the Modular Electronics Vehicle Mod.
        /// </summary>
        /*
        public bool HasModularElectronics
        {
            set
            {
                _blnModularElectronics = value;

                if (_blnModularElectronics)
                {
                    _intMaxResponse = 10;
                    _intMaxSystem = 10;
                    _intMaxFirewall = 10;
                    _intMaxSignal = 10;
                }
            }
        }*/

        /// <summary>
        /// Vehicle's Device Rating.
        /// </summary>
        /*
        public int DeviceRating
        {
            set
            {
                _intMaxResponse = value + 2;
                _intMaxSystem = value;
                _intMaxFirewall = value;
                _intMaxSignal = value + 2;
                _intDeviceRating = value;

                if (_blnModularElectronics)
                {
                    _intMaxResponse = 10;
                    _intMaxSystem = 10;
                    _intMaxFirewall = 10;
                    _intMaxSignal = 10;
                }
            }
        }*/

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
        #endregion

        #region Methods
        /// <summary>
        /// Build the list of Mods.
        /// </summary>
        private void BuildModList()
        {
            string strCategory = cboCategory.SelectedValue?.ToString();
            string strFilter = "(" + _objCharacter.Options.BookXPath() + ")";
            if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All" && (string.IsNullOrWhiteSpace(txtSearch.Text) || _objCharacter.Options.SearchInCategoryOnly))
                strFilter += " and category = \"" + strCategory + "\"";
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
                    strFilter += " and (" + objCategoryFilter.ToString().TrimEnd(" or ") + ")";
                }
            }
            if (txtSearch.TextLength != 0)
            {
                // Treat everything as being uppercase so the search is case-insensitive.
                string strSearchText = txtSearch.Text.ToUpper();
                strFilter += " and ((contains(translate(name,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + strSearchText + "\") and not(translate)) or contains(translate(translate,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + strSearchText + "\"))";
            }

            // Retrieve the list of Mods for the selected Category.
            XmlNodeList objXmlModList = _objXmlDocument.SelectNodes("/chummer/mods/mod[" + strFilter + "]");

            // Update the list of Mods based on the selected Category.
            XmlNode objXmlVehicleNode = _objVehicle.GetNode();
            List<ListItem> lstMods = new List<ListItem>();
            foreach (XmlNode objXmlMod in objXmlModList)
            {
                if (objXmlMod["forbidden"]?["vehicledetails"] != null)
                {
                    // Assumes topmost parent is an AND node
                    if (objXmlVehicleNode.ProcessFilterOperationNode(objXmlMod["forbidden"]["vehicledetails"], false))
                    {
                        continue;
                    }
                }
                if (objXmlMod["required"]?["vehicledetails"] != null)
                {
                    // Assumes topmost parent is an AND node
                    if (!objXmlVehicleNode.ProcessFilterOperationNode(objXmlMod["required"]["vehicledetails"], false))
                    {
                        continue;
                    }
                }

                if (objXmlMod["forbidden"]?["oneof"] != null)
                {
                    XmlNodeList objXmlForbiddenList = objXmlMod.SelectNodes("forbidden/oneof/mods");
                    //Add to set for O(N log M) runtime instead of O(N * M)

                    HashSet<string> objForbiddenAccessory = new HashSet<string>();
                    foreach (XmlNode node in objXmlForbiddenList)
                    {
                        objForbiddenAccessory.Add(node.InnerText);
                    }

                    if (_lstMods.Any(objAccessory => objForbiddenAccessory.Contains(objAccessory.Name)))
                    {
                        continue;
                    }
                }

                if (objXmlMod["required"]?["oneof"] != null)
                {
                    XmlNodeList objXmlRequiredList = objXmlMod.SelectNodes("required/oneof/mods");
                    //Add to set for O(N log M) runtime instead of O(N * M)

                    HashSet<string> objRequiredAccessory = new HashSet<string>();
                    foreach (XmlNode node in objXmlRequiredList)
                    {
                        objRequiredAccessory.Add(node.InnerText);
                    }

                    if (!_lstMods.Any(objAccessory => objRequiredAccessory.Contains(objAccessory.Name)))
                    {
                        continue;
                    }
                }

                XmlNode objXmlRequirements = objXmlMod.SelectSingleNode("requires");
                if (objXmlRequirements != null)
                {
                    if (_objVehicle.Seats < Convert.ToInt32(objXmlRequirements["seats"]?.InnerText))
                    {
                        continue;
                    }
                }

                if (Backend.SelectionShared.CheckAvailRestriction(objXmlMod, _objCharacter, chkHideOverAvailLimit.Checked))
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
            _strSelectedMod = lstMod.SelectedValue.ToString();
            _intSelectedRating = decimal.ToInt32(nudRating.Value);
            _intMarkup = decimal.ToInt32(nudMarkup.Value);
            _blnBlackMarketDiscount = chkBlackMarketDiscount.Checked;
            s_StrSelectCategory = cboCategory.SelectedValue.ToString();
            DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// Update the Mod's information based on the Mod selected and current Rating.
        /// </summary>
        private void UpdateGearInfo()
        {
            if (_blnSkipUpdate) return;
            _blnSkipUpdate = true;
            if (!string.IsNullOrEmpty(lstMod.Text))
            {
                // Retireve the information for the selected Mod.
                // Filtering is also done on the Category in case there are non-unique names across categories.
                XmlNode objXmlMod = _objXmlDocument.SelectSingleNode("/chummer/mods/mod[id = \"" + lstMod.SelectedValue + "\"]");

                // Extract the Avil and Cost values from the Gear info since these may contain formulas and/or be based off of the Rating.
                // This is done using XPathExpression.

                int intMinRating = 1;
                if (objXmlMod["minrating"]?.InnerText.Length > 0)
                {
                    string strMinRating = ReplaceStrings(objXmlMod["minrating"]?.InnerText);
                    intMinRating = Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strMinRating));
                }
                // If the rating is "qty", we're looking at Tires instead of actual Rating, so update the fields appropriately.
                if (objXmlMod["rating"].InnerText == "qty")
                {
                    nudRating.Enabled = true;
                    nudRating.Maximum = 20;
                    while (nudRating.Maximum > intMinRating && !Backend.SelectionShared.CheckAvailRestriction(objXmlMod, _objCharacter, chkHideOverAvailLimit.Checked, decimal.ToInt32(nudRating.Maximum)))
                    {
                        nudRating.Maximum -= 1;
                    }
                    nudRating.Minimum = intMinRating;
                    lblRatingLabel.Text = LanguageManager.GetString("Label_Qty", GlobalOptions.Language);
                }
                //Used for the Armor modifications.
                else if (objXmlMod["rating"].InnerText.ToLower() == "body")
                {
                    nudRating.Maximum = _objVehicle.Body;
                    while (nudRating.Maximum > intMinRating && !Backend.SelectionShared.CheckAvailRestriction(objXmlMod, _objCharacter, chkHideOverAvailLimit.Checked, decimal.ToInt32(nudRating.Maximum)))
                    {
                        nudRating.Maximum -= 1;
                    }
                    nudRating.Minimum = intMinRating;
                    nudRating.Enabled = true;
                    lblRatingLabel.Text = LanguageManager.GetString("Label_Body", GlobalOptions.Language);
                }
                //Used for Metahuman Adjustments.
                else if (objXmlMod["rating"].InnerText.ToLower() == "seats")
                {
                    nudRating.Maximum = _objVehicle.TotalSeats;
                    while (nudRating.Maximum > intMinRating && !Backend.SelectionShared.CheckAvailRestriction(objXmlMod, _objCharacter, chkHideOverAvailLimit.Checked, decimal.ToInt32(nudRating.Maximum)))
                    {
                        nudRating.Maximum -= 1;
                    }
                    nudRating.Minimum = intMinRating;
                    nudRating.Enabled = true;
                    lblRatingLabel.Text = LanguageManager.GetString("Label_Qty", GlobalOptions.Language);
                }
                else
                {
                    if (Convert.ToInt32(objXmlMod["rating"].InnerText) > 0)
                    {
                        nudRating.Maximum = Convert.ToInt32(objXmlMod["rating"].InnerText);
                        while (nudRating.Maximum > intMinRating && !Backend.SelectionShared.CheckAvailRestriction(objXmlMod, _objCharacter, chkHideOverAvailLimit.Checked, decimal.ToInt32(nudRating.Maximum)))
                        {
                            nudRating.Maximum -= 1;
                        }
                        nudRating.Minimum = intMinRating;
                        nudRating.Enabled = true;
                        lblRatingLabel.Text = LanguageManager.GetString("Label_Rating", GlobalOptions.Language);
                    }
                    else
                    {
                        nudRating.Minimum = 0;
                        nudRating.Maximum = 0;
                        nudRating.Enabled = false;
                        lblRatingLabel.Text = LanguageManager.GetString("Label_Rating", GlobalOptions.Language);
                    }
                }

                // Avail.
                // If avail contains "F" or "R", remove it from the string so we can use the expression.
                string strAvail = string.Empty;
                string strAvailExpr = objXmlMod["avail"].InnerText;
                if (strAvailExpr.StartsWith("FixedValues"))
                {
                    int intRating = decimal.ToInt32(nudRating.Value - 1);
                    strAvailExpr = strAvailExpr.TrimStart("FixedValues", true).Trim("()".ToCharArray());
                    string[] strValues = strAvailExpr.Split(',');
                    if (intRating > strValues.Length || intRating < 0)
                    {
                        intRating = strValues.Length -1;
                    }
                    strAvailExpr = strValues[intRating];
                }

                if (strAvailExpr.EndsWith('F') || strAvailExpr.EndsWith('R'))
                {
                    strAvail = strAvailExpr.Substring(strAvailExpr.Length - 1, 1);
                    // Translate the Avail string.
                    if (strAvail == "R")
                        strAvail = LanguageManager.GetString("String_AvailRestricted", GlobalOptions.Language);
                    else if (strAvail == "F")
                        strAvail = LanguageManager.GetString("String_AvailForbidden", GlobalOptions.Language);
                    // Remove the trailing character if it is "F" or "R".
                    strAvailExpr = strAvailExpr.Substring(0, strAvailExpr.Length - 1);
                }
                try
                {
                    lblAvail.Text = Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(ReplaceStrings(strAvailExpr))).ToString();
                }
                catch (XPathException)
                {
                    lblAvail.Text = objXmlMod["avail"].InnerText;
                }
                lblAvail.Text = lblAvail.Text + strAvail;

                // Cost.
                decimal decItemCost = 0;
                if (objXmlMod["cost"].InnerText.StartsWith("Variable"))
                {
                    decimal decMin = 0;
                    decimal decMax = decimal.MaxValue;
                    string strCost = objXmlMod["cost"].InnerText;
                    strCost = strCost.TrimStart("Variable", true).Trim("()".ToCharArray());
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
                else
                {
                    string strCost = string.Empty;
                    if (chkFreeItem.Checked)
                        strCost = "0";
                    else
                    {
                        strCost = objXmlMod["cost"].InnerText;
                        if (strCost.StartsWith("FixedValues"))
                        {
                            int intRating = decimal.ToInt32(nudRating.Value) - 1;
                            strCost = strCost.TrimStart("FixedValues", true).Trim("()".ToCharArray());
                            string[] strValues = strCost.Split(',');
                            if (intRating < 0 || intRating > strValues.Length)
                            {
                                intRating = 0;
                            }
                            strCost = strValues[intRating];
                        }
                        strCost = ReplaceStrings(strCost);
                    }

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

                // Slots.

                string strSlots = string.Empty;
                if (objXmlMod["slots"].InnerText.StartsWith("FixedValues"))
                {
                    string[] strValues = objXmlMod["slots"].InnerText.TrimStart("FixedValues", true).Trim("()".ToCharArray()).Split(',');
                    strSlots = strValues[decimal.ToInt32(nudRating.Value) - 1];
                }
                else
                {
                    strSlots = objXmlMod["slots"].InnerText;
                }
                strSlots = ReplaceStrings(strSlots);
                lblSlots.Text = CommonFunctions.EvaluateInvariantXPath(strSlots).ToString();

                if (objXmlMod["category"].InnerText != null)
                {
                    if (s_LstCategories.Contains(objXmlMod["category"].InnerText))
                    {
                        lblVehicleCapacityLabel.Visible = true;
                        lblVehicleCapacity.Visible = true;
                        lblVehicleCapacity.Text = GetRemainingModCapacity(objXmlMod["category"].InnerText, Convert.ToInt32(lblSlots.Text));
                        tipTooltip.SetToolTip(lblVehicleCapacityLabel, LanguageManager.GetString("Tip_RemainingVehicleModCapacity", GlobalOptions.Language));
                    }
                    else
                    {
                        lblVehicleCapacityLabel.Visible = false;
                        lblVehicleCapacity.Visible = false;
                    }

                    lblCategory.Text = objXmlMod["category"].InnerText;
                    if (objXmlMod["category"].InnerText == "Weapon Mod")
                        lblCategory.Text = LanguageManager.GetString("String_WeaponModification", GlobalOptions.Language);
                    // Translate the Category if possible.
                    else if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
                    {
                        XmlNode objXmlCategory = _objXmlDocument.SelectSingleNode("/chummer/modcategories/category[. = \"" + objXmlMod["category"].InnerText + "\"]");
                        if (objXmlCategory?.Attributes["translate"] != null)
                        {
                            lblCategory.Text = objXmlCategory.Attributes["translate"].InnerText;
                        }
                    }
                }

                if (objXmlMod["limit"] != null)
                {
                    // Translate the Limit if possible.
                    if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
                    {
                        XmlNode objXmlLimit = _objXmlDocument.SelectSingleNode("/chummer/limits/limit[. = \"" + objXmlMod["limit"].InnerText + "\"]");
                        lblLimit.Text = objXmlLimit.Attributes["translate"] != null
                            ? " (" + objXmlLimit.Attributes["translate"].InnerText + ")"
                            : " (" + objXmlMod["limit"].InnerText + ")";
                    }
                    else
                        lblLimit.Text = " (" + objXmlMod["limit"].InnerText + ")";
                }
                else
                    lblLimit.Text = string.Empty;

                string strBook = _objCharacter.Options.LanguageBookShort(objXmlMod["source"].InnerText, GlobalOptions.Language);
                string strPage = objXmlMod["page"].InnerText;
                if (objXmlMod["altpage"] != null)
                    strPage = objXmlMod["altpage"].InnerText;
                lblSource.Text = strBook + " " + strPage;

                tipTooltip.SetToolTip(lblSource, _objCharacter.Options.LanguageBookLong(objXmlMod["source"].InnerText, GlobalOptions.Language) + " " + LanguageManager.GetString("String_Page", GlobalOptions.Language) + " " + strPage);
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
        private string ReplaceStrings(string strInput)
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

            return objInputBuilder.ToString();
        }
        #endregion

        private void lblSource_Click(object sender, EventArgs e)
        {
            CommonFunctions.OpenPDF(lblSource.Text, _objCharacter);
        }
    }
}
