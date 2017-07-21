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
        private string _strInputFile = "vehicles";
        private int _intMarkup = 0;
        private bool _blnSkipUpdate = false;
        private static string _strSelectCategory = string.Empty;

        readonly string[] _arrCategories = new string[6] { "Powertrain", "Protection", "Weapons", "Body", "Electromagnetic", "Cosmetic" };
        private string _strAllowedCategories = string.Empty;
        private bool _blnAddAgain = false;

        private XmlNodeList objXmlCategoryList;
        private XmlDocument _objXmlDocument = new XmlDocument();
        private readonly Character _objCharacter;
        private bool _blnBlackMarketDiscount;
        private string _strLimitToCategories = string.Empty;
        private List<ListItem> _lstCategory = new List<ListItem>();
        private List<VehicleMod> _lstMods;

        #region Control Events
        public frmSelectVehicleMod(Character objCharacter, bool blnCareer = false)
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            lblMarkupLabel.Visible = blnCareer;
            nudMarkup.Visible = blnCareer;
            lblMarkupPercentLabel.Visible = blnCareer;
            _objCharacter = objCharacter;
            MoveControls();
        }

        private void frmSelectVehicleMod_Load(object sender, EventArgs e)
        {
            chkHideOverAvailLimit.Text = chkHideOverAvailLimit.Text.Replace("{0}",
                    _objCharacter.Options.Availability.ToString());
            chkHideOverAvailLimit.Checked = _objCharacter.Options.HideItemsOverAvailLimit;
            // Load the Mod information.
            _objXmlDocument = XmlManager.Instance.Load(_strInputFile + ".xml");

            string[] strValues = _strLimitToCategories.Split(',');

            // Populate the Category list.
            // Populate the Category list.
            XmlNodeList objXmlNodeList = _objXmlDocument.SelectNodes("/chummer/modcategories/category");
            foreach (XmlNode objXmlCategory in objXmlNodeList)
            {
                if (_strLimitToCategories != "" && strValues.All(value => value != objXmlCategory.InnerText))
                    continue;
                ListItem objItem = new ListItem();
                objItem.Value = objXmlCategory.InnerText;
                objItem.Name = objXmlCategory.Attributes?["translate"]?.InnerText ?? objXmlCategory.InnerText;
                _lstCategory.Add(objItem);
            }
            SortListItem objSort = new SortListItem();
            _lstCategory.Sort(objSort.Compare);
            cboCategory.BeginUpdate();
            cboCategory.ValueMember = "Value";
            cboCategory.DisplayMember = "Name";
            cboCategory.DataSource = _lstCategory;
            cboCategory.EndUpdate();

            BuildModList();

            chkBlackMarketDiscount.Visible = _objCharacter.BlackMarketDiscount;

            if (_strInputFile == "weapons")
                Text = LanguageManager.Instance.GetString("Title_SelectVehicleMod_Weapon");
            _blnSkipUpdate = false;
            UpdateGearInfo();
        }

        private void lstMod_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateGearInfo();
        }

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Load the Vehicle information.
            _objXmlDocument = XmlManager.Instance.Load("vehicles.xml");

            XmlNode objXmlVehicleNode = _objXmlDocument.SelectSingleNode("/chummer/vehicles/vehicle[id = \"" + _objVehicle.SourceID + "\"]");

            List<ListItem> lstMods = new List<ListItem>();
            XmlNodeList objXmlModList = null;
            // Populate the Mod list.
            objXmlModList = cboCategory.SelectedValue != null && (string) cboCategory.SelectedValue != "All"
            ? _objXmlDocument.SelectNodes("/chummer/mods/mod[(" + _objCharacter.Options.BookXPath() + ") and category = \"" + cboCategory.SelectedValue + "\"]")
            : _objXmlDocument.SelectNodes("/chummer/mods/mod[" + _objCharacter.Options.BookXPath() + "]");
            if (objXmlModList != null)
                foreach (XmlNode objXmlMod in objXmlModList)
                {
                    if (objXmlMod["hidden"] != null)
                        continue;

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
                    if (!Backend.Shared_Methods.SelectionShared.CheckAvailRestriction(objXmlMod, _objCharacter,chkHideOverAvailLimit.Checked, Convert.ToInt32(nudRating.Value)))
                    {
                        continue;
                    }
                    ListItem objItem = new ListItem {Value = objXmlMod["id"]?.InnerText};
                    objItem.Name = objXmlMod["translate"]?.InnerText ?? objXmlMod["name"]?.InnerText;
                    lstMods.Add(objItem);
                }
            lstMod.BeginUpdate();
            lstMod.DataSource = null;
            lstMod.ValueMember = "Value";
            lstMod.DisplayMember = "Name";
            lstMod.DataSource = lstMods;
            lstMod.EndUpdate();
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
            _strSelectCategory = string.Empty;
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
        /// Which XML file should the window read from (default vehicles).
        /// </summary>
        public string InputFile
        {
            set
            {
                _strInputFile = value;
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
        public List<VehicleMod> InstalledMods
        {
            set
            {
                _lstMods = value;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Build the list of Mods.
        /// </summary>
        private void BuildModList()
        {

            // Select the first Category in the list.
            if (string.IsNullOrEmpty(_strSelectCategory))
                cboCategory.SelectedIndex = 0;
            else
                cboCategory.SelectedValue = _strSelectCategory;

            foreach (Label objLabel in Controls.OfType<Label>())
            {
                if (objLabel.Text.StartsWith("["))
                    objLabel.Text = string.Empty;
            }

            // Update the list of Mods based on the selected Category.
            XmlNodeList objXmlModList;

            // Load the Mod information.
            _objXmlDocument = XmlManager.Instance.Load(_strInputFile + ".xml");

            // Load the Vehicle information.
            _objXmlDocument = XmlManager.Instance.Load("vehicles.xml");

            XmlNode objXmlVehicleNode = _objXmlDocument.SelectSingleNode("/chummer/vehicles/vehicle[name = \"" + _objVehicle.Name + "\"]");

            // Retrieve the list of Mods for the selected Category.
            if (string.IsNullOrEmpty(txtSearch.Text))
                objXmlModList = _objXmlDocument.SelectNodes("/chummer/mods/mod[(" + _objCharacter.Options.BookXPath() + ") and category != \"Special\"]");
            else
                objXmlModList = _objXmlDocument.SelectNodes("/chummer/mods/mod[(" + _objCharacter.Options.BookXPath() + ") and category != \"Special\" and ((contains(translate(name,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\") and not(translate)) or contains(translate(translate,'abcdefghijklmnopqrstuvwxyzàáâãäåçèéêëìíîïñòóôõöùúûüýß','ABCDEFGHIJKLMNOPQRSTUVWXYZÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝß'), \"" + txtSearch.Text.ToUpper() + "\"))]");
            List<ListItem> lstMods = new List<ListItem>();
            if (objXmlModList != null)
                foreach (XmlNode objXmlMod in objXmlModList)
                {
                    if (objXmlMod["hidden"] != null)
                        continue;

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

                        foreach (VehicleMod objAccessory in _lstMods.Where(objAccessory => objForbiddenAccessory.Contains(objAccessory.Name)))
                        {
                            goto NextItem;
                        }
                    }

                    if (objXmlMod["required"]?["oneof"] != null)
                    {
                        bool boolCanAdd = false;
                        XmlNodeList objXmlRequiredList = objXmlMod.SelectNodes("required/oneof/mods");
                        //Add to set for O(N log M) runtime instead of O(N * M)

                        HashSet<string> objRequiredAccessory = new HashSet<string>();
                        foreach (XmlNode node in objXmlRequiredList)
                        {
                            objRequiredAccessory.Add(node.InnerText);
                        }

                        foreach (VehicleMod objAccessory in _lstMods.Where(objAccessory => objRequiredAccessory.Contains(objAccessory.Name)))
                        {
                            boolCanAdd = true;
                            break;
                        }
                        if (!boolCanAdd)
                            continue;
                    }

                    XmlNode objXmlRequirements = objXmlMod.SelectSingleNode("requires");
                    if (objXmlRequirements != null)
                    {
                        if (_objVehicle.Seats < Convert.ToInt32(objXmlRequirements["seats"]?.InnerText))
                        {
                            continue;
                        }
                    }

                    ListItem objItem = new ListItem();
                    objItem.Value = objXmlMod["id"].InnerText;
                    objItem.Name = objXmlMod["translate"]?.InnerText ?? objXmlMod["name"].InnerText;
                    lstMods.Add(objItem);
                NextItem:;
                }
            SortListItem objSort = new SortListItem();
            lstMods.Sort(objSort.Compare);
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
            _intSelectedRating = Convert.ToInt32(nudRating.Value);
            _intMarkup = Convert.ToInt32(nudMarkup.Value);
            _blnBlackMarketDiscount = chkBlackMarketDiscount.Checked;
            _strSelectCategory = cboCategory.SelectedValue.ToString();
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
                XPathNavigator nav = _objXmlDocument.CreateNavigator();

                int intMinRating = 1;
                if (objXmlMod["minrating"]?.InnerText.Length > 0)
                {
                    string strMinRating = ReplaceStrings(objXmlMod["minrating"]?.InnerText);
                    XPathExpression xprRating = nav.Compile(strMinRating);
                    intMinRating = Convert.ToInt32(nav.Evaluate(xprRating).ToString());
                }
                // If the rating is "qty", we're looking at Tires instead of actual Rating, so update the fields appropriately.
                if (objXmlMod["rating"].InnerText == "qty")
                {
                    nudRating.Enabled = true;
                    nudRating.Maximum = 20;
                    nudRating.Minimum = intMinRating;
                    lblRatingLabel.Text = LanguageManager.Instance.GetString("Label_Qty");
                }
                //Used for the Armor modifications.
                else if (objXmlMod["rating"].InnerText.ToLower() == "body")
                {
                    nudRating.Maximum = _objVehicle.Body;
                    nudRating.Minimum = intMinRating;
                    nudRating.Enabled = true;
                    lblRatingLabel.Text = LanguageManager.Instance.GetString("Label_Body");
                }
                //Used for Metahuman Adjustments.
                else if (objXmlMod["rating"].InnerText.ToLower() == "seats")
                {
                    nudRating.Maximum = _objVehicle.TotalSeats;
                    nudRating.Minimum = intMinRating;
                    nudRating.Enabled = true;
                    lblRatingLabel.Text = LanguageManager.Instance.GetString("Label_Qty");
                }
                else
                {
                    if (Convert.ToInt32(objXmlMod["rating"].InnerText) > 0)
                    {
                        nudRating.Maximum = Convert.ToInt32(objXmlMod["rating"].InnerText);
                        nudRating.Minimum = intMinRating;
                        nudRating.Enabled = true;
                        lblRatingLabel.Text = LanguageManager.Instance.GetString("Label_Rating");
                    }
                    else
                    {
                        nudRating.Minimum = 0;
                        nudRating.Maximum = 0;
                        nudRating.Enabled = false;
                        lblRatingLabel.Text = LanguageManager.Instance.GetString("Label_Rating");
                    }
                }

                // Avail.
                // If avail contains "F" or "R", remove it from the string so we can use the expression.
                string strAvail = string.Empty;
                string strAvailExpr = string.Empty;
                if (objXmlMod["avail"].InnerText.StartsWith("FixedValues"))
                {
                    int intRating = Convert.ToInt32(nudRating.Value - 1);
                    string[] strValues = objXmlMod["avail"].InnerText.Replace("FixedValues(", string.Empty).Replace(")", string.Empty).Split(',');
                    if (intRating > strValues.Length || intRating < 0)
                    {
                        intRating = strValues.Length -1;
                    }
                        strAvailExpr = strValues[intRating];
                }
                else
                {
                    strAvailExpr = objXmlMod["avail"].InnerText;
                }

                XPathExpression xprAvail;
                if (strAvailExpr.Substring(strAvailExpr.Length - 1, 1) == "F" || strAvailExpr.Substring(strAvailExpr.Length - 1, 1) == "R")
                {
                    strAvail = strAvailExpr.Substring(strAvailExpr.Length - 1, 1);
                    // Remove the trailing character if it is "F" or "R".
                    strAvailExpr = strAvailExpr.Substring(0, strAvailExpr.Length - 1);
                }
                try
                {
                    xprAvail = nav.Compile(strAvailExpr.Replace("Rating", Math.Max(nudRating.Value,1).ToString(GlobalOptions.InvariantCultureInfo)));
                    lblAvail.Text = Convert.ToInt32(nav.Evaluate(xprAvail)) + strAvail;
                }
                catch (XPathException)
                {
                    lblAvail.Text = objXmlMod["avail"].InnerText;
                }
                lblAvail.Text = lblAvail.Text.Replace("R", LanguageManager.Instance.GetString("String_AvailRestricted")).Replace("F", LanguageManager.Instance.GetString("String_AvailForbidden"));

                // Cost.
                int intItemCost = 0;
                if (objXmlMod["cost"].InnerText.StartsWith("Variable"))
                {
                    int intMin = 0;
                    int intMax = 0;
                    string strCost = objXmlMod["cost"].InnerText.Replace("Variable(", string.Empty).Replace(")", string.Empty);
                    if (strCost.Contains("-"))
                    {
                        string[] strValues = strCost.Split('-');
                        intMin = Convert.ToInt32(strValues[0]);
                        intMax = Convert.ToInt32(strValues[1]);
                    }
                    else
                        intMin = Convert.ToInt32(strCost.Replace("+", string.Empty));

                    if (intMax == 0)
                    {
                        intMax = 1000000;
                        lblCost.Text = string.Format("{0:###,###,##0¥+}", intMin);
                    }
                    else
                        lblCost.Text = string.Format("{0:###,###,##0}", intMin) + "-" + string.Format("{0:###,###,##0¥}", intMax);

                    intItemCost = intMin;
                }
                else
                {
                    string strCost = string.Empty;
                    if (chkFreeItem.Checked)
                        strCost = "0";
                    else
                    {
                        if (objXmlMod["cost"].InnerText.StartsWith("FixedValues"))
                        {
                            int intRating = Convert.ToInt32(nudRating.Value) - 1;
                            string[] strValues = objXmlMod["cost"].InnerText.Replace("FixedValues(", string.Empty).Replace(")", string.Empty).Split(',');
                            if (intRating < 0 || intRating > strValues.Length)
                            {
                                intRating = 0;
                            }
                            strCost = strValues[intRating];
                        }
                        else
                        {
                            strCost = objXmlMod["cost"].InnerText;
                        }
                        strCost = ReplaceStrings(strCost);
                    }

                    XPathExpression xprCost = nav.Compile(strCost);
                    int intCost = Convert.ToInt32(Convert.ToDouble(nav.Evaluate(xprCost), GlobalOptions.InvariantCultureInfo));
                    intCost *= _intModMultiplier;

                    // Apply any markup.
                    double dblCost = Convert.ToDouble(intCost, GlobalOptions.InvariantCultureInfo);
                    dblCost *= 1 + (Convert.ToDouble(nudMarkup.Value, GlobalOptions.CultureInfo) / 100.0);

                    if (chkBlackMarketDiscount.Checked)
                    {
                        dblCost = dblCost - (dblCost*0.90);
                    }

                    intCost = Convert.ToInt32(dblCost);
                    lblCost.Text = string.Format("{0:###,###,##0¥}", intCost);

                    intItemCost = intCost;
                }

                // Update the Avail Test Label.
                lblTest.Text = _objCharacter.AvailTest(intItemCost, lblAvail.Text);

                // Slots.

                string strSlots = string.Empty;
                if (objXmlMod["slots"].InnerText.StartsWith("FixedValues"))
                {
                    string[] strValues = objXmlMod["slots"].InnerText.Replace("FixedValues(", string.Empty).Replace(")", string.Empty).Split(',');
                    strSlots = strValues[Convert.ToInt32(nudRating.Value) - 1];
                }
                else
                {
                    strSlots = objXmlMod["slots"].InnerText;
                }
                strSlots = ReplaceStrings(strSlots);
                XPathExpression xprSlots = nav.Compile(strSlots);
                lblSlots.Text = nav.Evaluate(xprSlots).ToString();

                if (objXmlMod["category"].InnerText != null)
                {
                    if (_arrCategories.Contains(objXmlMod["category"].InnerText))
                    {
                        lblVehicleCapacityLabel.Visible = true;
                        lblVehicleCapacity.Visible = true;
                        lblVehicleCapacity.Text = GetRemainingModCapacity(objXmlMod["category"].InnerText, Convert.ToInt32(lblSlots.Text));
                        tipTooltip.SetToolTip(lblVehicleCapacityLabel, LanguageManager.Instance.GetString("Tip_RemainingVehicleModCapacity"));
                    }
                    else
                    {
                        lblVehicleCapacityLabel.Visible = false;
                        lblVehicleCapacity.Visible = false;
                    }

                    lblCategory.Text = objXmlMod["category"].InnerText;
                    if (objXmlMod["category"].InnerText == "Weapon Mod")
                        lblCategory.Text = LanguageManager.Instance.GetString("String_WeaponModification");
                    // Translate the Category if possible.
                    else if (GlobalOptions.Instance.Language != "en-us")
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
                    if (GlobalOptions.Instance.Language != "en-us")
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

                string strBook = _objCharacter.Options.LanguageBookShort(objXmlMod["source"].InnerText);
                string strPage = objXmlMod["page"].InnerText;
                if (objXmlMod["altpage"] != null)
                    strPage = objXmlMod["altpage"].InnerText;
                lblSource.Text = strBook + " " + strPage;

                tipTooltip.SetToolTip(lblSource, _objCharacter.Options.LanguageBookLong(objXmlMod["source"].InnerText) + " " + LanguageManager.Instance.GetString("String_Page") + " " + strPage);
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
            strInput = strInput.Replace("Rating", nudRating.Value.ToString(GlobalOptions.InvariantCultureInfo));
            strInput = strInput.Replace("Vehicle Cost", _objVehicle.Cost);
            strInput = strInput.Replace("Weapon Cost", _intWeaponCost.ToString());
            strInput = strInput.Replace("Total Cost", _intTotalWeaponCost.ToString());
            strInput = strInput.Replace("Body", _objVehicle.Body.ToString());
            strInput = strInput.Replace("Handling", _objVehicle.Handling.ToString());
            strInput = strInput.Replace("Offroad Handling", _objVehicle.OffroadHandling.ToString());
            strInput = strInput.Replace("Speed", _objVehicle.Speed.ToString());
            strInput = strInput.Replace("Offroad Speed", _objVehicle.OffroadSpeed.ToString());
            strInput = strInput.Replace("Acceleration", _objVehicle.Accel.ToString());
            strInput = strInput.Replace("Offroad Acceleration", _objVehicle.OffroadAccel.ToString());
            strInput = strInput.Replace("Sensor", _objVehicle.BaseSensor.ToString());
            strInput = strInput.Replace("Armor", _objVehicle.Armor.ToString());

            return strInput;
        }
        #endregion

        private void lblSource_Click(object sender, EventArgs e)
        {
            CommonFunctions.StaticOpenPDF(lblSource.Text, _objCharacter);
        }
    }
}
