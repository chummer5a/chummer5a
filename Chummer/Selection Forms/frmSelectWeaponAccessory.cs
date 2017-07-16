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
        private int _intMarkup = 0;

        private string _strAllowedMounts = string.Empty;
        private int _intWeaponCost = 0;
        private int _intRating = 0;
        private string _strCurrentWeaponName = string.Empty;
        private bool _blnAddAgain = false;

        private XmlDocument _objXmlDocument = new XmlDocument();
        private readonly Character _objCharacter;
        private int _intAccessoryMultiplier = 1;
        private bool _blnBlackMarketDiscount;
        private List<WeaponAccessory> _lstAccessories;

        #region Control Events
        public frmSelectWeaponAccessory(Character objCharacter, bool blnCareer = false)
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            lblMarkupLabel.Visible = blnCareer;
            nudMarkup.Visible = blnCareer;
            lblMarkupPercentLabel.Visible = blnCareer;
            _objCharacter = objCharacter;
            MoveControls();
        }

        private void frmSelectWeaponAccessory_Load(object sender, EventArgs e)
        {
            foreach (Label objLabel in Controls.OfType<Label>())
            {
                if (objLabel.Text.StartsWith("["))
                    objLabel.Text = string.Empty;
            }
            chkHideOverAvailLimit.Text = chkHideOverAvailLimit.Text.Replace("{0}",
                    _objCharacter.Options.Availability.ToString());
            chkHideOverAvailLimit.Checked = _objCharacter.Options.HideItemsOverAvailLimit;
            BuildAccessoryList();

            chkBlackMarketDiscount.Visible = _objCharacter.BlackMarketDiscount;
        }

        /// <summary>
        /// Build the list of available weapon accessories.
        /// </summary>
        private void BuildAccessoryList()
        {
            List<ListItem> lstAccessories = new List<ListItem>();

            // Load the Weapon information.
            _objXmlDocument = XmlManager.Instance.Load("weapons.xml");

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
            if (objXmlAccessoryList != null)
                foreach (XmlNode objXmlAccessory in objXmlAccessoryList)
                {
                    bool boolCanAdd = true;
                    if (objXmlAccessory.InnerXml.Contains("<extramount>"))
                    {
                        if (strAllowed.Length > 1)
                        {
                            foreach (string strItem in (objXmlAccessory["extramount"].InnerText.Split('/')).Where(strItem => !string.IsNullOrEmpty(strItem)))
                            {
                                if (strAllowed.All(strAllowedMount => strAllowedMount != strItem))
                                {
                                    boolCanAdd = false;
                                }
                                if (boolCanAdd)
                                    break;
                            }
                        }
                    }

                    if (objXmlAccessory["forbidden"]?["weapondetails"] != null)
                    {
                        // Assumes topmost parent is an AND node
                        if (objXmlWeaponNode.ProcessFilterOperationNode(objXmlAccessory["forbidden"]["weapondetails"], false))
                        {
                            goto NextItem;
                        }
                    }
                    if (objXmlAccessory["required"]?["weapondetails"] != null)
                    {
                        // Assumes topmost parent is an AND node
                        if (!objXmlWeaponNode.ProcessFilterOperationNode(objXmlAccessory["required"]["weapondetails"], false))
                        {
                            goto NextItem;
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
                            goto NextItem;
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

                        boolCanAdd = _lstAccessories.Any(objAccessory => objRequiredAccessory.Contains(objAccessory.Name));
                    }

                    boolCanAdd = Backend.Shared_Methods.SelectionShared.CheckAvailRestriction(objXmlAccessory, _objCharacter, chkHideOverAvailLimit.Checked, Convert.ToInt32(nudRating.Value), 0, boolCanAdd);

                    if (!boolCanAdd)
                        continue;
                    ListItem objItem = new ListItem();
                    objItem.Value = objXmlAccessory["name"]?.InnerText;
                    objItem.Name = objXmlAccessory["translate"]?.InnerText ?? objItem.Value;
                    lstAccessories.Add(objItem);
                    NextItem:;
                }

            SortListItem objSort = new SortListItem();
            lstAccessories.Sort(objSort.Compare);
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

        private void lblSource_Click(object sender, EventArgs e)
        {
            CommonFunctions.StaticOpenPDF(lblSource.Text, _objCharacter);
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
        public int WeaponCost
        {
            set
            {
                _intWeaponCost = value;
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
        public List<WeaponAccessory> InstalledAccessories
        {
            set
            {
                _lstAccessories = value;
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
            int intMaxRating;
            if (int.TryParse(objXmlAccessory["rating"]?.InnerText, out intMaxRating) && intMaxRating > 1)
            {
                nudRating.Enabled = true;
                nudRating.Visible = true;
                lblRatingLabel.Visible = true;
                nudRating.Maximum = intMaxRating;
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
            XPathNavigator nav = _objXmlDocument.CreateNavigator();
            // Avail.
            // If avail contains "F" or "R", remove it from the string so we can use the expression.
            string strAvail = string.Empty;
            string strAvailExpr = objXmlAccessory["avail"]?.InnerText;
            if (!string.IsNullOrWhiteSpace(strAvailExpr))
            {
                lblAvail.Text = strAvailExpr;
                if (strAvailExpr.Substring(strAvailExpr.Length - 1, 1) == "F" ||
                    strAvailExpr.Substring(strAvailExpr.Length - 1, 1) == "R")
                {
                    strAvail = strAvailExpr.Substring(strAvailExpr.Length - 1, 1);
                    // Remove the trailing character if it is "F" or "R".
                    strAvailExpr = strAvailExpr.Substring(0, strAvailExpr.Length - 1);
                }
                try
                {
                    XPathExpression xprAvail = nav.Compile(strAvailExpr.Replace("Rating", nudRating.Value.ToString(GlobalOptions.CultureInfo)));
                    int intTmp;
                    if (int.TryParse(nav.Evaluate(xprAvail)?.ToString(), out intTmp))
                        lblAvail.Text = intTmp.ToString() + strAvail;
                }
                catch (XPathException)
                {
                }
            }
            else
                lblAvail.Text = "";
            lblAvail.Text = lblAvail.Text.Replace("R", LanguageManager.Instance.GetString("String_AvailRestricted")).Replace("F", LanguageManager.Instance.GetString("String_AvailForbidden"));
            if (!chkFreeItem.Checked)
            {
                string strCost = "0";
                if (objXmlAccessory.TryGetStringFieldQuickly("cost", ref strCost))
                    strCost = strCost.Replace("Weapon Cost", _intWeaponCost.ToString())
                        .Replace("Rating", nudRating.Value.ToString(GlobalOptions.CultureInfo));
                if (strCost.StartsWith("Variable"))
                {
                    int intMin = 0;
                    int intMax = int.MaxValue;
                    strCost = strCost.Replace("Variable", string.Empty).Trim("()".ToCharArray());
                    if (strCost.Contains("-"))
                    {
                        string[] strValues = strCost.Split('-');
                        int.TryParse(strValues[0], out intMin);
                        int.TryParse(strValues[1], out intMax);
                    }
                    else
                        int.TryParse(strCost.Replace("+", string.Empty), out intMin);

                    if (intMax == int.MaxValue)
                    {
                        lblCost.Text = $"{intMin:###,###,##0¥+}";
                    }
                    else
                        lblCost.Text = $"{intMin:###,###,##0} - {intMax:###,###,##0¥}";

                    lblTest.Text = _objCharacter.AvailTest(intMin, lblAvail.Text);
                }
                else
                {
                    double dblCost = 0;
                    try
                    {
                        XPathExpression xprCost = nav.Compile(strCost);
                        dblCost = Convert.ToDouble(nav.Evaluate(xprCost), GlobalOptions.InvariantCultureInfo);
                    }
                    catch (XPathException)
                    {
                    }

                    // Apply any markup.
                    dblCost *= 1 + (Convert.ToDouble(nudMarkup.Value, GlobalOptions.CultureInfo) / 100.0);
                    int intCost = Convert.ToInt32(dblCost);

                    lblCost.Text = $"{intCost:###,###,##0¥}";
                    lblTest.Text = _objCharacter.AvailTest(intCost, lblAvail.Text);
                }
            }
            else
            {
                lblCost.Text = $"{0:###,###,##0¥}";
                lblTest.Text = _objCharacter.AvailTest(0, lblAvail.Text);
            }

            string strBookCode = objXmlAccessory["source"]?.InnerText;
            string strBook = _objCharacter.Options.LanguageBookShort(strBookCode);
            string strPage = objXmlAccessory["page"]?.InnerText;
            objXmlAccessory.TryGetStringFieldQuickly("altpage", ref strPage);
            lblSource.Text = strBook + " " + strPage;

            tipTooltip.SetToolTip(lblSource, _objCharacter.Options.LanguageBookLong(strBookCode) + " " + LanguageManager.Instance.GetString("String_Page") + " " + strPage);
        }
        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            _strSelectedAccessory = lstAccessory.SelectedValue.ToString();
            int.TryParse(nudRating.Value.ToString(GlobalOptions.CultureInfo), out _intRating);
            int.TryParse(nudMarkup.Value.ToString(GlobalOptions.CultureInfo), out _intMarkup);
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