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
﻿using System;
using System.Collections.Generic;
 using System.Globalization;
 using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
﻿using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class frmSelectWeaponAccessory : Form
    {
        private string _strSelectedAccessory = string.Empty;
        private decimal _decMarkup = 0;

        private string _strAllowedMounts = string.Empty;
        private decimal _decWeaponCost = 0;
        private int _intRating = 0;
        private string _strCurrentWeaponName = string.Empty;
        private bool _blnAddAgain = false;

        private readonly XmlDocument _objXmlDocument = null;
        private readonly Character _objCharacter;
        private int _intAccessoryMultiplier = 1;
        private bool _blnBlackMarketDiscount;
        private List<WeaponAccessory> _lstAccessories;

        #region Control Events
        public frmSelectWeaponAccessory(Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            lblMarkupLabel.Visible = objCharacter.Created;
            nudMarkup.Visible = objCharacter.Created;
            lblMarkupPercentLabel.Visible = objCharacter.Created;
            _objCharacter = objCharacter;
            MoveControls();
            // Load the Weapon information.
            _objXmlDocument = XmlManager.Load("weapons.xml");
        }

        private void frmSelectWeaponAccessory_Load(object sender, EventArgs e)
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
            BuildAccessoryList();

            chkBlackMarketDiscount.Visible = _objCharacter.BlackMarketDiscount;
        }

        /// <summary>
        /// Build the list of available weapon accessories.
        /// </summary>
        private void BuildAccessoryList()
        {
            List<ListItem> lstAccessories = new List<ListItem>();

            XmlNode objXmlWeaponNode = _objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + _strCurrentWeaponName + "\"]");

            // Populate the Accessory list.
            string[] strAllowed = _strAllowedMounts.Split('/');
            string strMount = string.Empty;
            foreach (string strAllowedMount in strAllowed)
            {
                if (!string.IsNullOrEmpty(strAllowedMount))
                    strMount += "contains(mount, \"" + strAllowedMount + "\") or ";
            }
            strMount += "contains(mount, \"Internal\") or contains(mount, \"None\") or ";
            strMount += "mount = \"\"";
            XmlNodeList objXmlAccessoryList = _objXmlDocument.SelectNodes("/chummer/accessories/accessory[(" + strMount + ") and (" + _objCharacter.Options.BookXPath() + ")]");
            foreach (XmlNode objXmlAccessory in objXmlAccessoryList)
            {
                if (objXmlAccessory.InnerXml.Contains("<extramount>"))
                {
                    if (strAllowed.Length > 1)
                    {
                        foreach (string strItem in (objXmlAccessory["extramount"].InnerText.Split('/')).Where(strItem => !string.IsNullOrEmpty(strItem)))
                        {
                            if (strAllowed.All(strAllowedMount => strAllowedMount != strItem))
                            {
                                goto NextItem;
                            }
                        }
                    }
                }

                if (objXmlAccessory["forbidden"]?["weapondetails"] != null)
                {
                    // Assumes topmost parent is an AND node
                    if (objXmlWeaponNode.ProcessFilterOperationNode(objXmlAccessory["forbidden"]["weapondetails"], false))
                    {
                        continue;
                    }
                }
                if (objXmlAccessory["required"]?["weapondetails"] != null)
                {
                    // Assumes topmost parent is an AND node
                    if (!objXmlWeaponNode.ProcessFilterOperationNode(objXmlAccessory["required"]["weapondetails"], false))
                    {
                        continue;
                    }
                }

                if (objXmlAccessory["forbidden"]?["oneof"] != null)
                {
                    XmlNodeList objXmlForbiddenList = objXmlAccessory.SelectNodes("forbidden/oneof/accessory");
                    //Add to set for O(N log M) runtime instead of O(N * M)

                    HashSet<string> objForbiddenAccessory = new HashSet<string>();
                    foreach (XmlNode node in objXmlForbiddenList)
                    {
                        objForbiddenAccessory.Add(node.InnerText);
                    }

                    if (_lstAccessories.Any(objAccessory => objForbiddenAccessory.Contains(objAccessory.Name)))
                    {
                        continue;
                    }
                }

                if (objXmlAccessory["required"]?["oneof"] != null)
                {
                    XmlNodeList objXmlRequiredList = objXmlAccessory.SelectNodes("required/oneof/accessory");
                    //Add to set for O(N log M) runtime instead of O(N * M)

                    HashSet<string> objRequiredAccessory = new HashSet<string>();
                    foreach (XmlNode node in objXmlRequiredList)
                    {
                        objRequiredAccessory.Add(node.InnerText);
                    }

                    if (!_lstAccessories.Any(objAccessory => objRequiredAccessory.Contains(objAccessory.Name)))
                    {
                        continue;
                    }
                }

                if (!chkHideOverAvailLimit.Checked || Backend.SelectionShared.CheckAvailRestriction(objXmlAccessory, _objCharacter))
                {
                    string strName = objXmlAccessory["name"]?.InnerText ?? string.Empty;
                    lstAccessories.Add(new ListItem(strName, objXmlAccessory["translate"]?.InnerText ?? strName));
                }
                NextItem:;
            }
            
            lstAccessories.Sort(CompareListItems.CompareNames);
            lstAccessory.BeginUpdate();
            lstAccessory.ValueMember = "Value";
            lstAccessory.DisplayMember = "Name";
            lstAccessory.DataSource = lstAccessories;
            lstAccessory.EndUpdate();
        }

        private void lstAccessory_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateGearInfo();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lstAccessory.Text))
                AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void lstAccessory_DoubleClick(object sender, EventArgs e)
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

        private void chkBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
        {
            UpdateGearInfo();
        }

        private void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            UpdateGearInfo();
        }

        private void nudRating_ValueChanged(object sender, EventArgs e)
        {
            UpdateGearInfo();
        }

        private void cboMount_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateMountFields(true);
        }

        private void cboExtraMount_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateMountFields(false);
        }
        private void chkHideOverAvailLimit_CheckedChanged(object sender, EventArgs e)
        {
            BuildAccessoryList();
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
        /// Name of Accessory that was selected in the dialogue.
        /// </summary>
        public string SelectedAccessory
        {
            get
            {
                return _strSelectedAccessory;
            }
        }

        /// <summary>
        /// Mount that was selected in the dialogue.
        /// </summary>
        public Tuple<string, string> SelectedMount
        {
            get
            {
                return new Tuple<string, string>(cboMount.SelectedItem?.ToString(), cboExtraMount.SelectedItem?.ToString());
            }
        }

        /// <summary>
        /// Rating of the Accessory.
        /// </summary>
        public string SelectedRating
        {
            get
            {
                if (nudRating.Enabled)
                {
                    return nudRating.Value.ToString(GlobalOptions.CultureInfo);
                }
                else
                {
                    // Display Rating for items without one as 0
                    return 0.ToString(GlobalOptions.CultureInfo);
                }
            }
        }

        /// <summary>
        /// Mounts that the Weapon allows to be used.
        /// </summary>
        public string AllowedMounts
        {
            set
            {
                _strAllowedMounts = value;
            }
        }

        /// <summary>
        /// GUID of the current weapon for which the accessory is being selected
        /// </summary>
        public string CurrentWeaponName
        {
            set
            {
                _strCurrentWeaponName = value;
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
        /// Weapon's Accessory Cost Multiplier.
        /// </summary>
        public int AccessoryMultiplier
        {
            set
            {
                _intAccessoryMultiplier = value;
            }
        }

        /// <summary>
        /// Weapon's Cost.
        /// </summary>
        public decimal WeaponCost
        {
            set
            {
                _decWeaponCost = value;
            }
        }

        /// <summary>
        /// Markup percentage.
        /// </summary>
        public decimal Markup
        {
            get
            {
                return _decMarkup;
            }
        }

        /// <summary>
        /// Currently Installed Accessories
        /// </summary>
        public IList<WeaponAccessory> InstalledAccessories
        {
            set
            {
                _lstAccessories = (List<WeaponAccessory>)value;
            }
        }
        #endregion

        #region Methods

        private void UpdateMountFields(bool boolChangeExtraMountFirst)
        {
            if ((cboMount.SelectedItem.ToString() != "None") && cboExtraMount.SelectedItem != null && (cboExtraMount.SelectedItem.ToString() != "None")
                && (cboMount.SelectedItem.ToString() == cboExtraMount.SelectedItem.ToString()))
            {
                if (boolChangeExtraMountFirst)
                    cboExtraMount.SelectedIndex = 0;
                else
                    cboMount.SelectedIndex = 0;
                while ((cboMount.SelectedItem.ToString() != "None") && (cboExtraMount.SelectedItem.ToString() != "None")
                    && (cboMount.SelectedItem.ToString() == cboExtraMount.SelectedItem.ToString()))
                {
                    if (boolChangeExtraMountFirst)
                        cboExtraMount.SelectedIndex += 1;
                    else
                        cboMount.SelectedIndex += 1;
                }
            }
        }

        private void UpdateGearInfo()
        {
            // Retrieve the information for the selected Accessory.
            XmlNode objXmlAccessory = _objXmlDocument.SelectSingleNode("/chummer/accessories/accessory[name = \"" + lstAccessory.SelectedValue + "\"]");
            if (objXmlAccessory == null)
                return;

            if (objXmlAccessory.InnerXml.Contains("<rc>"))
            {
                lblRC.Visible = true;
                lblRCLabel.Visible = true;
                lblRC.Text = objXmlAccessory["rc"]?.InnerText;
            }
            else
            {
                lblRC.Visible = false;
                lblRCLabel.Visible = false;
            }
            if (int.TryParse(objXmlAccessory["rating"]?.InnerText, out int intMaxRating) && intMaxRating > 0)
            {
                nudRating.Enabled = true;
                nudRating.Visible = true;
                lblRatingLabel.Visible = true;
                nudRating.Maximum = intMaxRating;
                if (chkHideOverAvailLimit.Checked)
                {
                    while (nudRating.Maximum > nudRating.Minimum && !Backend.SelectionShared.CheckAvailRestriction(objXmlAccessory, _objCharacter, decimal.ToInt32(nudRating.Maximum)))
                    {
                        nudRating.Maximum -= 1;
                    }
                }
            }
            else
            {
                nudRating.Enabled = false;
                nudRating.Visible = false;
                lblRatingLabel.Visible = false;
            }
            List<string> strMounts = new List<string>();
            foreach (string strItem in objXmlAccessory["mount"]?.InnerText?.Split('/'))
            {
                strMounts.Add(strItem);
            }
            strMounts.Add("None");

            List<string> strAllowed = new List<string>();
            foreach (string strItem in _strAllowedMounts.Split('/'))
            {
                strAllowed.Add(strItem);
            }
            strAllowed.Add("None");
            cboMount.Items.Clear();
            foreach (string strCurrentMount in strMounts)
            {
                if (!string.IsNullOrEmpty(strCurrentMount))
                {
                    foreach (string strAllowedMount in strAllowed)
                    {
                        if (strCurrentMount == strAllowedMount)
                        {
                            cboMount.Items.Add(strCurrentMount);
                        }
                    }
                }
            }
            if (cboMount.Items.Count <= 1)
            {
                cboMount.Enabled = false;
            }
            else
            {
                cboMount.Enabled = true;
            }
            cboMount.SelectedIndex = 0;

            List<string> strExtraMounts = new List<string>();
            if (objXmlAccessory.InnerXml.Contains("<extramount>"))
            {
                foreach (string strItem in objXmlAccessory["extramount"]?.InnerText?.Split('/'))
                {
                    strExtraMounts.Add(strItem);
                }
            }
            strExtraMounts.Add("None");

            cboExtraMount.Items.Clear();
            foreach (string strCurrentMount in strExtraMounts)
            {
                if (!string.IsNullOrEmpty(strCurrentMount))
                {
                    foreach (string strAllowedMount in strAllowed)
                    {
                        if (strCurrentMount == strAllowedMount)
                        {
                            cboExtraMount.Items.Add(strCurrentMount);
                        }
                    }
                }
            }
            if (cboExtraMount.Items.Count <= 1)
            {
                cboExtraMount.Enabled = false;
            }
            else
            {
                cboExtraMount.Enabled = true;
            }
            cboExtraMount.SelectedIndex = 0;
            if (cboMount.SelectedItem.ToString() != "None" && cboExtraMount.SelectedItem.ToString() != "None"
                && cboMount.SelectedItem.ToString() == cboExtraMount.SelectedItem.ToString())
                cboExtraMount.SelectedIndex += 1;
            // Avail.
            // If avail contains "F" or "R", remove it from the string so we can use the expression.
            string strAvail = string.Empty;
            string strAvailExpr = objXmlAccessory["avail"]?.InnerText;
            if (!string.IsNullOrWhiteSpace(strAvailExpr))
            {
                lblAvail.Text = strAvailExpr;
                if (strAvailExpr.EndsWith('F', 'R'))
                {
                    strAvail = strAvailExpr.Substring(strAvailExpr.Length - 1, 1);
                    if (strAvail == "R")
                        strAvail = LanguageManager.GetString("String_AvailRestricted", GlobalOptions.Language);
                    else if (strAvail == "F")
                        strAvail = LanguageManager.GetString("String_AvailForbidden", GlobalOptions.Language);
                    // Remove the trailing character if it is "F" or "R".
                    strAvailExpr = strAvailExpr.Substring(0, strAvailExpr.Length - 1);
                }
                try
                {
                    lblAvail.Text = Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strAvailExpr.Replace("Rating", nudRating.Value.ToString(GlobalOptions.CultureInfo)))).ToString() + strAvail;
                }
                catch (XPathException)
                {
                    lblAvail.Text = strAvailExpr + strAvail;
                }
            }
            else
                lblAvail.Text = string.Empty;
            if (!chkFreeItem.Checked)
            {
                string strCost = "0";
                if (objXmlAccessory.TryGetStringFieldQuickly("cost", ref strCost))
                    strCost = strCost.Replace("Weapon Cost", _decWeaponCost.ToString(GlobalOptions.InvariantCultureInfo))
                        .Replace("Rating", nudRating.Value.ToString(GlobalOptions.CultureInfo));
                if (strCost.StartsWith("Variable("))
                {
                    decimal decMin = 0;
                    decimal decMax = decimal.MaxValue;
                    strCost = strCost.TrimStart("Variable(", true).TrimEnd(')');
                    if (strCost.Contains('-'))
                    {
                        string[] strValues = strCost.Split('-');
                        decimal.TryParse(strValues[0], NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out decMin);
                        decimal.TryParse(strValues[1], NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out decMax);
                    }
                    else
                        decimal.TryParse(strCost.FastEscape('+'), NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out decMin);

                    if (decMax == decimal.MaxValue)
                    {
                        lblCost.Text = decMin.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + "¥+";
                    }
                    else
                        lblCost.Text = decMin.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + " - " + decMax.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';

                    lblTest.Text = _objCharacter.AvailTest(decMax, lblAvail.Text);
                }
                else
                {
                    decimal decCost = 0.0m;
                    try
                    {
                        decCost = Convert.ToDecimal(CommonFunctions.EvaluateInvariantXPath(strCost), GlobalOptions.InvariantCultureInfo);
                    }
                    catch (XPathException)
                    {
                    }

                    // Apply any markup.
                    decCost *= 1 + (nudMarkup.Value / 100.0m);

                    lblCost.Text = decCost.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                    lblTest.Text = _objCharacter.AvailTest(decCost, lblAvail.Text);
                }
            }
            else
            {
                lblCost.Text = 0.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                lblTest.Text = _objCharacter.AvailTest(0, lblAvail.Text);
            }

            string strBookCode = objXmlAccessory["source"]?.InnerText;
            string strBook = CommonFunctions.LanguageBookShort(strBookCode, GlobalOptions.Language);
            string strPage = objXmlAccessory["page"]?.InnerText;
            objXmlAccessory.TryGetStringFieldQuickly("altpage", ref strPage);
            lblSource.Text = strBook + " " + strPage;

            tipTooltip.SetToolTip(lblSource, CommonFunctions.LanguageBookLong(strBookCode, GlobalOptions.Language) + " " + LanguageManager.GetString("String_Page", GlobalOptions.Language) + " " + strPage);
        }
        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            _strSelectedAccessory = lstAccessory.SelectedValue.ToString();
            int.TryParse(nudRating.Value.ToString(GlobalOptions.CultureInfo), out _intRating);
            _decMarkup = nudMarkup.Value;
            _blnBlackMarketDiscount = chkBlackMarketDiscount.Checked;
            DialogResult = DialogResult.OK;
        }

        private void MoveControls()
        {
            int intWidth = Math.Max(lblRCLabel.Width, lblMountLabel.Width);
            intWidth = Math.Max(intWidth, lblAvailLabel.Width);
            intWidth = Math.Max(intWidth, lblCostLabel.Width);

            lblRC.Left = lblRCLabel.Left + intWidth + 6;
            //lblMount.Left = lblMountLabel.Left + intWidth + 6;
            lblAvail.Left = lblAvailLabel.Left + intWidth + 6;
            lblTestLabel.Left = lblAvail.Left + lblAvail.Width + 16;
            lblTest.Left = lblTestLabel.Left + lblTestLabel.Width + 6;
            lblCost.Left = lblCostLabel.Left + intWidth + 6;

            nudMarkup.Left = lblMarkupLabel.Left + lblMarkupLabel.Width + 6;
            lblMarkupPercentLabel.Left = nudMarkup.Left + nudMarkup.Width;

            lblSource.Left = lblSourceLabel.Left + lblSourceLabel.Width + 6;
        }
        #endregion

    }
}
