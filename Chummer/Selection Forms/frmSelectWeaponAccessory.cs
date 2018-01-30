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
using System.Text;
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

        private bool _blnLoading = true;
        private List<string> _strAllowedMounts = new List<string>();
        private int _intRating = 0;
        private Weapon _objParentWeapon;
        private bool _blnAddAgain = false;

        private readonly XmlDocument _objXmlDocument = null;
        private readonly Character _objCharacter;
        private bool _blnBlackMarketDiscount;
        //private readonly List<string> _blackMarketMaps = new List<string>();

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
            //TODO: Accessories don't use a category mapping, so this doesn't work.
            //CommonFunctions.GenerateBlackMarketMappings(_objCharacter, _objXmlDocument, _blackMarketMaps);
        }

        private void frmSelectWeaponAccessory_Load(object sender, EventArgs e)
        {
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

            chkBlackMarketDiscount.Visible = _objCharacter.BlackMarketDiscount;

            _blnLoading = false;
            BuildAccessoryList();
        }

        /// <summary>
        /// Build the list of available weapon accessories.
        /// </summary>
        private void BuildAccessoryList()
        {
            List<ListItem> lstAccessories = new List<ListItem>();
            
            // Populate the Accessory list.
            StringBuilder strMount = new StringBuilder("contains(mount, \"Internal\") or contains(mount, \"None\") or mount = \"\"");
            foreach (string strAllowedMount in _strAllowedMounts)
            {
                if (!string.IsNullOrEmpty(strAllowedMount))
                    strMount.Append(" or contains(mount, \"" + strAllowedMount + "\")");
            }
            XmlNode xmlParentWeaponDataNode = _objParentWeapon.GetNode();
            XmlNodeList objXmlAccessoryList = _objXmlDocument.SelectNodes("/chummer/accessories/accessory[(" + strMount.ToString() + ") and (" + _objCharacter.Options.BookXPath() + ")]");
            foreach (XmlNode objXmlAccessory in objXmlAccessoryList)
            {
                XmlNode xmlExtraMountNode = objXmlAccessory["extramount"];
                if (xmlExtraMountNode != null)
                {
                    if (_strAllowedMounts.Count > 1)
                    {
                        foreach (string strItem in (xmlExtraMountNode.InnerText.Split('/')).Where(strItem => !string.IsNullOrEmpty(strItem)))
                        {
                            if (_strAllowedMounts.All(strAllowedMount => strAllowedMount != strItem))
                            {
                                goto NextItem;
                            }
                        }
                    }
                }

                XmlNode xmlTestNode = objXmlAccessory.SelectSingleNode("forbidden/weapondetails");
                if (xmlTestNode != null)
                {
                    // Assumes topmost parent is an AND node
                    if (xmlParentWeaponDataNode.ProcessFilterOperationNode(xmlTestNode, false))
                    {
                        continue;
                    }
                }
                xmlTestNode = objXmlAccessory.SelectSingleNode("required/weapondetails");
                if (xmlTestNode != null)
                {
                    // Assumes topmost parent is an AND node
                    if (!xmlParentWeaponDataNode.ProcessFilterOperationNode(xmlTestNode, false))
                    {
                        continue;
                    }
                }

                xmlTestNode = objXmlAccessory.SelectSingleNode("forbidden/oneof");
                if (xmlTestNode != null)
                {
                    XmlNodeList objXmlForbiddenList = xmlTestNode.SelectNodes("accessory");
                    //Add to set for O(N log M) runtime instead of O(N * M)

                    HashSet<string> objForbiddenAccessory = new HashSet<string>();
                    foreach (XmlNode node in objXmlForbiddenList)
                    {
                        objForbiddenAccessory.Add(node.InnerText);
                    }

                    if (_objParentWeapon.WeaponAccessories.Any(objAccessory => objForbiddenAccessory.Contains(objAccessory.Name)))
                    {
                        continue;
                    }
                }

                xmlTestNode = objXmlAccessory.SelectSingleNode("required/oneof");
                if (xmlTestNode != null)
                {
                    XmlNodeList objXmlRequiredList = xmlTestNode.SelectNodes("accessory");
                    //Add to set for O(N log M) runtime instead of O(N * M)

                    HashSet<string> objRequiredAccessory = new HashSet<string>();
                    foreach (XmlNode node in objXmlRequiredList)
                    {
                        objRequiredAccessory.Add(node.InnerText);
                    }

                    if (!_objParentWeapon.WeaponAccessories.Any(objAccessory => objRequiredAccessory.Contains(objAccessory.Name)))
                    {
                        continue;
                    }
                }

                if (!chkHideOverAvailLimit.Checked || Backend.SelectionShared.CheckAvailRestriction(objXmlAccessory, _objCharacter))
                {
                    lstAccessories.Add(new ListItem(objXmlAccessory["id"].InnerText, objXmlAccessory["translate"]?.InnerText ?? objXmlAccessory["name"]?.InnerText ?? string.Empty));
                }
                NextItem:;
            }
            
            lstAccessories.Sort(CompareListItems.CompareNames);
            string strOldSelected = lstAccessory.SelectedValue?.ToString();
            _blnLoading = true;
            lstAccessory.BeginUpdate();
            lstAccessory.ValueMember = "Value";
            lstAccessory.DisplayMember = "Name";
            lstAccessory.DataSource = lstAccessories;
            _blnLoading = false;
            if (!string.IsNullOrEmpty(strOldSelected))
                lstAccessory.SelectedValue = strOldSelected;
            else
                lstAccessory.SelectedIndex = -1;
            lstAccessory.EndUpdate();
        }

        private void lstAccessory_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateGearInfo();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void lstAccessory_DoubleClick(object sender, EventArgs e)
        {
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
        public decimal SelectedRating
        {
            get
            {
                if (nudRating.Enabled)
                {
                    return nudRating.Value;
                }
                else
                {
                    // Display Rating for items without one as 0
                    return 0;
                }
            }
        }
        
        /// <summary>
        /// GUID of the current weapon for which the accessory is being selected
        /// </summary>
        public Weapon ParentWeapon
        {
            set
            {
                _objParentWeapon = value;
                _strAllowedMounts.Clear();
                XmlNodeList objXmlMountList = value.GetNode()?.SelectNodes("accessorymounts/mount");
                foreach (XmlNode objXmlMount in objXmlMountList)
                {
                    string strLoopMount = objXmlMount.InnerText;
                    // Run through the Weapon's currenct Accessories and filter out any used up Mount points.
                    if (!_objParentWeapon.WeaponAccessories.Any(objMod => objMod.Mount == strLoopMount || objMod.ExtraMount == strLoopMount))
                    {
                        _strAllowedMounts.Add(strLoopMount);
                    }
                }
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
        /// Markup percentage.
        /// </summary>
        public decimal Markup
        {
            get
            {
                return _decMarkup;
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
            if (_blnLoading)
                return;

            XmlNode xmlAccessory = null;
            string strSelectedId = lstAccessory.SelectedValue?.ToString();
            // Retrieve the information for the selected Accessory.
            if (!string.IsNullOrEmpty(strSelectedId))
                xmlAccessory = _objXmlDocument.SelectSingleNode("/chummer/accessories/accessory[id = \"" + strSelectedId + "\"]");
            if (xmlAccessory == null)
            {
                lblRC.Visible = false;
                lblRCLabel.Visible = false;
                nudRating.Enabled = false;
                nudRating.Visible = false;
                lblRatingLabel.Visible = false;
                cboMount.Visible = false;
                cboMount.Items.Clear();
                cboExtraMount.Visible = false;
                cboExtraMount.Items.Clear();
                lblAvail.Text = string.Empty;
                lblCost.Text = string.Empty;
                lblTest.Text = string.Empty;
                lblSource.Text = string.Empty;
                tipTooltip.SetToolTip(lblSource, string.Empty);
                return;
            }

            string strRC = xmlAccessory["rc"]?.InnerText;
            if (!string.IsNullOrEmpty(strRC))
            {
                lblRC.Visible = true;
                lblRCLabel.Visible = true;
                lblRC.Text = strRC;
            }
            else
            {
                lblRC.Visible = false;
                lblRCLabel.Visible = false;
            }
            if (int.TryParse(xmlAccessory["rating"]?.InnerText, out int intMaxRating) && intMaxRating > 0)
            {
                nudRating.Enabled = true;
                nudRating.Visible = true;
                lblRatingLabel.Visible = true;
                nudRating.Maximum = intMaxRating;
                if (chkHideOverAvailLimit.Checked)
                {
                    while (nudRating.Maximum > nudRating.Minimum && !Backend.SelectionShared.CheckAvailRestriction(xmlAccessory, _objCharacter, decimal.ToInt32(nudRating.Maximum)))
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
            foreach (string strItem in xmlAccessory["mount"]?.InnerText?.Split('/'))
            {
                strMounts.Add(strItem);
            }
            strMounts.Add("None");

            List<string> strAllowed = new List<string>();
            foreach (string strItem in _strAllowedMounts)
            {
                strAllowed.Add(strItem);
            }
            strAllowed.Add("None");
            cboMount.Visible = true;
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
            string strExtraMount = xmlAccessory["extramount"]?.InnerText;
            if (!string.IsNullOrEmpty(strExtraMount))
            {
                foreach (string strItem in strExtraMount.Split('/'))
                {
                    strExtraMounts.Add(strItem);
                }
            }
            strExtraMounts.Add("None");

            cboExtraMount.Visible = true;
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
            string strSuffix = string.Empty;
            string strAvail = xmlAccessory["avail"]?.InnerText;
            if (!string.IsNullOrWhiteSpace(strAvail))
            {
                char chrLastAvailChar = strAvail[strAvail.Length - 1];
                if (chrLastAvailChar == 'F')
                {
                    strSuffix = LanguageManager.GetString("String_AvailForbidden", GlobalOptions.Language);
                    strAvail = strAvail.Substring(0, strAvail.Length - 1);
                }
                else if (chrLastAvailChar == 'R')
                {
                    strSuffix = LanguageManager.GetString("String_AvailRestricted", GlobalOptions.Language);
                    strAvail = strAvail.Substring(0, strAvail.Length - 1);
                }
                try
                {
                    lblAvail.Text = Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strAvail.Replace("Rating", nudRating.Value.ToString(GlobalOptions.CultureInfo)))).ToString() + strSuffix;
                }
                catch (XPathException)
                {
                    lblAvail.Text = strAvail + strSuffix;
                }
            }
            else
                lblAvail.Text = string.Empty;
            if (!chkFreeItem.Checked)
            {
                string strCost = "0";
                if (xmlAccessory.TryGetStringFieldQuickly("cost", ref strCost))
                    strCost = strCost.CheapReplace("Weapon Cost", () => _objParentWeapon.OwnCost.ToString(GlobalOptions.InvariantCultureInfo))
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
                lblCost.Text = (0.0m).ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                lblTest.Text = _objCharacter.AvailTest(0, lblAvail.Text);
            }
            /*TODO: Accessories don't use a category mapping, so this doesn't work. 
            if (_blackMarketMaps != null)
                chkBlackMarketDiscount.Checked =
                    _blackMarketMaps.Contains(objXmlAccessory["category"]?.InnerText);
            */
            string strSource = xmlAccessory["source"].InnerText;
            string strPage = xmlAccessory["altpage"]?.InnerText ?? xmlAccessory["page"].InnerText;
            lblSource.Text = CommonFunctions.LanguageBookShort(strSource, GlobalOptions.Language) + ' ' + strPage;

            tipTooltip.SetToolTip(lblSource, CommonFunctions.LanguageBookLong(strSource, GlobalOptions.Language) + ' ' + LanguageManager.GetString("String_Page", GlobalOptions.Language) + ' ' + strPage);
        }
        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            string strSelectedId = lstAccessory.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                _strSelectedAccessory = strSelectedId;
                int.TryParse(nudRating.Value.ToString(GlobalOptions.CultureInfo), out _intRating);
                _decMarkup = nudMarkup.Value;
                _blnBlackMarketDiscount = chkBlackMarketDiscount.Checked;
                DialogResult = DialogResult.OK;
            }
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
