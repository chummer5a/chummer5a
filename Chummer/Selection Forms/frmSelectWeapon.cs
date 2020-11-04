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
using System.Data;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;
using Chummer.Backend.Equipment;
using System.Text;
using System.Linq;

// ReSharper disable LocalizableElement

namespace Chummer
{
    public partial class frmSelectWeapon : Form
    {
        private string _strSelectedWeapon = string.Empty;
        private decimal _decMarkup;

        private bool _blnLoading = true;
        private bool _blnSkipUpdate;
        private bool _blnAddAgain;
        private bool _blnBlackMarketDiscount;
        private HashSet<string> _hashLimitToCategories = new HashSet<string>();
        private static string s_StrSelectCategory = string.Empty;
        private readonly Character _objCharacter;
        private readonly XmlDocument _objXmlDocument;
        private Weapon _objSelectedWeapon;

        private readonly List<ListItem> _lstCategory = new List<ListItem>();
        private readonly HashSet<string> _setBlackMarketMaps;

        #region Control Events
        public frmSelectWeapon(Character objCharacter)
        {
            if (objCharacter == null)
                throw new ArgumentNullException(nameof(objCharacter));
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            lblMarkupLabel.Visible = objCharacter.Created;
            nudMarkup.Visible = objCharacter.Created;
            lblMarkupPercentLabel.Visible = objCharacter.Created;
            _objCharacter = objCharacter;
            // Load the Weapon information.
            _objXmlDocument = _objCharacter.LoadData("weapons.xml");
            _setBlackMarketMaps = _objCharacter.GenerateBlackMarketMappings(_objXmlDocument);
        }

        private void frmSelectWeapon_Load(object sender, EventArgs e)
        {
            DataGridViewCellStyle dataGridViewNuyenCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.TopRight,
                Format = _objCharacter.Options.NuyenFormat + '¥',
                NullValue = null
            };
            dgvc_Cost.DefaultCellStyle = dataGridViewNuyenCellStyle;

            if (_objCharacter.Created)
            {
                chkHideOverAvailLimit.Visible = false;
                chkHideOverAvailLimit.Checked = false;
            }
            else
            {
                chkHideOverAvailLimit.Text = string.Format(GlobalOptions.CultureInfo, chkHideOverAvailLimit.Text, _objCharacter.Options.MaximumAvailability);
                chkHideOverAvailLimit.Checked = GlobalOptions.HideItemsOverAvailLimit;
            }

            // Populate the Weapon Category list.
            // Populate the Category list.
            using (XmlNodeList xmlCategoryList = _objXmlDocument.SelectNodes("/chummer/categories/category"))
            {
                if (xmlCategoryList != null)
                {
                    foreach (XmlNode objXmlCategory in xmlCategoryList)
                    {
                        string strInnerText = objXmlCategory.InnerText;
                        if (_hashLimitToCategories.Count == 0 || _hashLimitToCategories.Contains(strInnerText))
                            _lstCategory.Add(new ListItem(strInnerText, objXmlCategory.Attributes?["translate"]?.InnerText ?? strInnerText));
                    }
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

            chkBlackMarketDiscount.Visible = _objCharacter.BlackMarketDiscount;

            // Select the first Category in the list.
            if (string.IsNullOrEmpty(s_StrSelectCategory))
                cboCategory.SelectedIndex = 0;
            else
                cboCategory.SelectedValue = s_StrSelectCategory;

            if (cboCategory.SelectedIndex == -1)
                cboCategory.SelectedIndex = 0;
            cboCategory.EndUpdate();

            _blnLoading = false;
            RefreshList();
        }

        private void RefreshCurrentList(object sender, EventArgs e)
        {
            RefreshList();
        }

        private void lstWeapon_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading || _blnSkipUpdate)
                return;

            // Retireve the information for the selected Weapon.
            XmlNode xmlWeapon = null;
            string strSelectedId = lstWeapon.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
                xmlWeapon = _objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[id = \"" + lstWeapon.SelectedValue + "\"]");
            if (xmlWeapon != null)
            {
                Weapon objWeapon = new Weapon(_objCharacter);
                objWeapon.Create(xmlWeapon, null, true, false, true);
                objWeapon.Parent = ParentWeapon;
                _objSelectedWeapon = objWeapon;
            }
            else
                _objSelectedWeapon = null;

            UpdateWeaponInfo();
        }

        private void UpdateWeaponInfo()
        {
            if (_blnLoading || _blnSkipUpdate)
                return;
            _blnSkipUpdate = true;
            SuspendLayout();
            if (_objSelectedWeapon != null)
            {
                chkBlackMarketDiscount.Enabled = _objCharacter.BlackMarketDiscount;

                if (!chkBlackMarketDiscount.Checked)
                {
                    chkBlackMarketDiscount.Checked = GlobalOptions.AssumeBlackMarket &&
                                                     _setBlackMarketMaps.Contains(_objSelectedWeapon.Category);
                }
                else if (!_setBlackMarketMaps.Contains(_objSelectedWeapon.Category))
                {
                    //Prevent chkBlackMarketDiscount from being checked if the gear category doesn't match.
                    chkBlackMarketDiscount.Checked = false;
                }

                _objSelectedWeapon.DiscountCost = chkBlackMarketDiscount.Checked;

                lblWeaponReach.Text = _objSelectedWeapon.TotalReach.ToString(GlobalOptions.CultureInfo);
                lblWeaponReachLabel.Visible = !string.IsNullOrEmpty(lblWeaponReach.Text);
                lblWeaponDamage.Text = _objSelectedWeapon.DisplayDamage;
                lblWeaponDamageLabel.Visible = !string.IsNullOrEmpty(lblWeaponDamage.Text);
                lblWeaponAP.Text = _objSelectedWeapon.DisplayTotalAP;
                lblWeaponAPLabel.Visible = !string.IsNullOrEmpty(lblWeaponAP.Text);
                lblWeaponMode.Text = _objSelectedWeapon.DisplayMode;
                lblWeaponModeLabel.Visible = !string.IsNullOrEmpty(lblWeaponMode.Text);
                lblWeaponRC.Text = _objSelectedWeapon.DisplayTotalRC;
                lblWeaponRC.SetToolTip(_objSelectedWeapon.RCToolTip);
                lblWeaponRCLabel.Visible = !string.IsNullOrEmpty(lblWeaponRC.Text);
                lblWeaponAmmo.Text = _objSelectedWeapon.DisplayAmmo;
                lblWeaponAmmoLabel.Visible = !string.IsNullOrEmpty(lblWeaponAmmo.Text);
                lblWeaponAccuracy.Text = _objSelectedWeapon.DisplayAccuracy;
                lblWeaponAccuracyLabel.Visible = !string.IsNullOrEmpty(lblWeaponAccuracy.Text);
                lblWeaponConceal.Text = _objSelectedWeapon.DisplayConcealability;
                lblWeaponConcealLabel.Visible = !string.IsNullOrEmpty(lblWeaponConceal.Text);

                decimal decItemCost = 0;
                if (chkFreeItem.Checked)
                {
                    lblWeaponCost.Text = (0.0m).ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                }
                else
                {
                    lblWeaponCost.Text = _objSelectedWeapon.DisplayCost(out decItemCost, nudMarkup.Value / 100.0m);
                }
                lblWeaponCostLabel.Visible = !string.IsNullOrEmpty(lblWeaponCost.Text);

                AvailabilityValue objTotalAvail = _objSelectedWeapon.TotalAvailTuple();
                lblWeaponAvail.Text = objTotalAvail.ToString();
                lblWeaponAvailLabel.Visible = !string.IsNullOrEmpty(lblWeaponAvail.Text);
                lblTest.Text = _objCharacter.AvailTest(decItemCost, objTotalAvail);
                lblTestLabel.Visible = !string.IsNullOrEmpty(lblTest.Text);
                _objSelectedWeapon.SetSourceDetail(lblSource);
                lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);

                // Build a list of included Accessories and Modifications that come with the weapon.
                StringBuilder strAccessories = new StringBuilder();
                foreach (WeaponAccessory objAccessory in _objSelectedWeapon.WeaponAccessories)
                {
                    strAccessories.AppendLine(objAccessory.CurrentDisplayName);
                }
                if (strAccessories.Length > 0)
                    strAccessories.Length -= Environment.NewLine.Length;

                lblIncludedAccessories.Text = strAccessories.Length == 0 ? LanguageManager.GetString("String_None") : strAccessories.ToString();
                gpbIncludedAccessories.Visible = !string.IsNullOrEmpty(lblIncludedAccessories.Text);
            }
            else
            {
                chkBlackMarketDiscount.Checked = false;
                lblWeaponReach.Text = string.Empty;
                lblWeaponReachLabel.Visible = false;
                lblWeaponDamage.Text = string.Empty;
                lblWeaponDamageLabel.Visible = false;
                lblWeaponAP.Text = string.Empty;
                lblWeaponAPLabel.Visible = false;
                lblWeaponMode.Text = string.Empty;
                lblWeaponModeLabel.Visible = false;
                lblWeaponRC.Text = string.Empty;
                lblWeaponRC.SetToolTip(string.Empty);
                lblWeaponRCLabel.Visible = false;
                lblWeaponAmmo.Text = string.Empty;
                lblWeaponAmmoLabel.Visible = false;
                lblWeaponAccuracy.Text = string.Empty;
                lblWeaponAccuracyLabel.Visible = false;
                lblWeaponConceal.Text = string.Empty;
                lblWeaponConcealLabel.Visible = false;
                lblWeaponCost.Text = string.Empty;
                lblWeaponCostLabel.Visible = false;
                lblWeaponAvail.Text = string.Empty;
                lblWeaponAvailLabel.Visible = false;
                lblTest.Text = string.Empty;
                lblTestLabel.Visible = false;
                lblSource.Text = string.Empty;
                lblSourceLabel.Visible = false;
                lblIncludedAccessories.Text = string.Empty;
                gpbIncludedAccessories.Visible = false;
                lblSource.SetToolTip(string.Empty);
            }
            ResumeLayout();
            _blnSkipUpdate = false;
        }

        private void BuildWeaponList(XmlNodeList objNodeList)
        {
            SuspendLayout();
            if (tabControl.SelectedIndex == 1)
            {
                DataTable tabWeapons = new DataTable("weapons");
                tabWeapons.Columns.Add("WeaponGuid");
                tabWeapons.Columns.Add("WeaponName");
                tabWeapons.Columns.Add("Dice");
                tabWeapons.Columns.Add("Accuracy");
                tabWeapons.Columns.Add("Damage");
                tabWeapons.Columns.Add("AP");
                tabWeapons.Columns.Add("RC");
                tabWeapons.Columns.Add("Ammo");
                tabWeapons.Columns.Add("Mode");
                tabWeapons.Columns.Add("Reach");
                tabWeapons.Columns.Add("Accessories");
                tabWeapons.Columns.Add("Avail");
                tabWeapons.Columns["Avail"].DataType = typeof(AvailabilityValue);
                tabWeapons.Columns.Add("Source");
                tabWeapons.Columns["Source"].DataType = typeof(SourceString);
                tabWeapons.Columns.Add("Cost");
                tabWeapons.Columns["Cost"].DataType = typeof(NuyenString);

                XmlNode xmlParentWeaponDataNode = _objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[id = \"" + ParentWeapon?.SourceIDString + "\"]");
                foreach (XmlNode objXmlWeapon in objNodeList)
                {
                    if (!objXmlWeapon.CreateNavigator().RequirementsMet(_objCharacter, ParentWeapon))
                        continue;

                    XmlNode xmlTestNode = objXmlWeapon.SelectSingleNode("forbidden/weapondetails");
                    if (xmlTestNode != null)
                    {
                        // Assumes topmost parent is an AND node
                        if (xmlParentWeaponDataNode.ProcessFilterOperationNode(xmlTestNode, false))
                        {
                            continue;
                        }
                    }
                    xmlTestNode = objXmlWeapon.SelectSingleNode("required/weapondetails");
                    if (xmlTestNode != null)
                    {
                        // Assumes topmost parent is an AND node
                        if (!xmlParentWeaponDataNode.ProcessFilterOperationNode(xmlTestNode, false))
                        {
                            continue;
                        }
                    }
                    if (objXmlWeapon["cyberware"]?.InnerText == bool.TrueString)
                        continue;
                    string strTest = objXmlWeapon["mount"]?.InnerText;
                    if (!string.IsNullOrEmpty(strTest) && !Mounts.Contains(strTest))
                        continue;
                    strTest = objXmlWeapon["extramount"]?.InnerText;
                    if (!string.IsNullOrEmpty(strTest) && !Mounts.Contains(strTest))
                        continue;
                    if (chkHideOverAvailLimit.Checked && !SelectionShared.CheckAvailRestriction(objXmlWeapon, _objCharacter))
                        continue;
                    if (!chkFreeItem.Checked && chkShowOnlyAffordItems.Checked)
                    {
                        decimal decCostMultiplier = 1 + (nudMarkup.Value / 100.0m);
                        if (_setBlackMarketMaps.Contains(objXmlWeapon["category"]?.InnerText))
                            decCostMultiplier *= 0.9m;
                        if (!SelectionShared.CheckNuyenRestriction(objXmlWeapon, _objCharacter.Nuyen, decCostMultiplier))
                            continue;
                    }

                    Weapon objWeapon = new Weapon(_objCharacter);
                    objWeapon.Create(objXmlWeapon, null, true, false, true);
                    objWeapon.Parent = ParentWeapon;

                    string strID = objWeapon.SourceIDString;
                    string strWeaponName = objWeapon.CurrentDisplayName;
                    string strDice = objWeapon.DicePool.ToString(GlobalOptions.CultureInfo);
                    string strAccuracy = objWeapon.DisplayAccuracy;
                    string strDamage = objWeapon.DisplayDamage;
                    string strAP = objWeapon.DisplayTotalAP;
                    if (strAP == "-")
                        strAP = "0";
                    string strRC = objWeapon.DisplayTotalRC;
                    string strAmmo = objWeapon.DisplayAmmo;
                    string strMode = objWeapon.DisplayMode;
                    string strReach = objWeapon.TotalReach.ToString(GlobalOptions.CultureInfo);
                    StringBuilder sbdAccessories = new StringBuilder();
                    foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                    {
                        sbdAccessories.AppendLine(objAccessory.CurrentDisplayName);
                    }
                    if (sbdAccessories.Length > 0)
                        sbdAccessories.Length -= Environment.NewLine.Length;
                    AvailabilityValue objAvail = objWeapon.TotalAvailTuple();
                    SourceString strSource = new SourceString(objWeapon.Source, objWeapon.DisplayPage(GlobalOptions.Language), GlobalOptions.Language, GlobalOptions.CultureInfo, _objCharacter);
                    NuyenString strCost = new NuyenString(objWeapon.DisplayCost(out decimal _));

                    tabWeapons.Rows.Add(strID, strWeaponName, strDice, strAccuracy, strDamage, strAP, strRC, strAmmo, strMode, strReach, sbdAccessories.ToString(), objAvail, strSource, strCost);
                }

                DataSet set = new DataSet("weapons");
                set.Tables.Add(tabWeapons);
                string strSelectedCategory = cboCategory.SelectedValue?.ToString();
                if (string.IsNullOrEmpty(strSelectedCategory)
                    || strSelectedCategory == "Show All"
                    || !(strSelectedCategory == "Blades"
                         || strSelectedCategory == "Clubs"
                         || strSelectedCategory == "Improvised Weapons"
                         || strSelectedCategory == "Exotic Melee Weapons"
                         || strSelectedCategory == "Unarmed"))
                {
                    //dgvWeapons.Columns[5].Visible = true;
                    dgvWeapons.Columns[6].Visible = true;
                    dgvWeapons.Columns[7].Visible = true;
                    dgvWeapons.Columns[8].Visible = true;
                }
                else
                {
                    //dgvWeapons.Columns[5].Visible = false;
                    dgvWeapons.Columns[6].Visible = false;
                    dgvWeapons.Columns[7].Visible = false;
                    dgvWeapons.Columns[8].Visible = false;
                }
                dgvWeapons.Columns[0].Visible = false;
                dgvWeapons.Columns[12].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight;
                dgvWeapons.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                dgvWeapons.DataSource = set;
                dgvWeapons.DataMember = "weapons";
            }
            else
            {
                int intOverLimit = 0;
                List<ListItem> lstWeapons = new List<ListItem>();
                XmlNode xmlParentWeaponDataNode = _objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[id = \"" + ParentWeapon?.SourceIDString + "\"]");
                foreach (XmlNode objXmlWeapon in objNodeList)
                {
                    if (!objXmlWeapon.CreateNavigator().RequirementsMet(_objCharacter, ParentWeapon))
                        continue;

                    XmlNode xmlTestNode = objXmlWeapon.SelectSingleNode("forbidden/weapondetails");
                    if (xmlTestNode != null)
                    {
                        // Assumes topmost parent is an AND node
                        if (xmlParentWeaponDataNode.ProcessFilterOperationNode(xmlTestNode, false))
                        {
                            continue;
                        }
                    }
                    xmlTestNode = objXmlWeapon.SelectSingleNode("required/weapondetails");
                    if (xmlTestNode != null)
                    {
                        // Assumes topmost parent is an AND node
                        if (!xmlParentWeaponDataNode.ProcessFilterOperationNode(xmlTestNode, false))
                        {
                            continue;
                        }
                    }
                    if (objXmlWeapon["cyberware"]?.InnerText == bool.TrueString)
                        continue;

                    string strMount = objXmlWeapon["mount"]?.InnerText;
                    if (!string.IsNullOrEmpty(strMount) && !Mounts.Contains(strMount))
                    {
                        continue;
                    }

                    string strExtraMount = objXmlWeapon["extramount"]?.InnerText;
                    if (!string.IsNullOrEmpty(strExtraMount) && !Mounts.Contains(strExtraMount))
                    {
                        continue;
                    }

                    if (chkHideOverAvailLimit.Checked && !SelectionShared.CheckAvailRestriction(objXmlWeapon, _objCharacter))
                    {
                        ++intOverLimit;
                        continue;
                    }
                    if (!chkFreeItem.Checked && chkShowOnlyAffordItems.Checked)
                    {
                        decimal decCostMultiplier = 1 + (nudMarkup.Value / 100.0m);
                        if (_setBlackMarketMaps.Contains(objXmlWeapon["category"]?.InnerText))
                            decCostMultiplier *= 0.9m;
                        if (!string.IsNullOrEmpty(ParentWeapon?.DoubledCostModificationSlots) &&
                            (!string.IsNullOrEmpty(strMount) || !string.IsNullOrEmpty(strExtraMount)))
                        {
                            string[] astrParentDoubledCostModificationSlots = ParentWeapon.DoubledCostModificationSlots.Split('/', StringSplitOptions.RemoveEmptyEntries);
                            if (astrParentDoubledCostModificationSlots.Contains(strMount) || astrParentDoubledCostModificationSlots.Contains(strExtraMount))
                            {
                                decCostMultiplier *= 2;
                            }
                        }
                        if (!SelectionShared.CheckNuyenRestriction(objXmlWeapon, _objCharacter.Nuyen, decCostMultiplier))
                        {
                            ++intOverLimit;
                            continue;
                        }
                    }
                    lstWeapons.Add(new ListItem(objXmlWeapon["id"]?.InnerText, objXmlWeapon["translate"]?.InnerText ?? objXmlWeapon["name"]?.InnerText));
                }

                lstWeapons.Sort(CompareListItems.CompareNames);
                if (intOverLimit > 0)
                {
                    // Add after sort so that it's always at the end
                    lstWeapons.Add(new ListItem(string.Empty,
                        string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("String_RestrictedItemsHidden"),
                            intOverLimit)));
                }
                string strOldSelected = lstWeapon.SelectedValue?.ToString();
                _blnLoading = true;
                lstWeapon.BeginUpdate();
                lstWeapon.ValueMember = nameof(ListItem.Value);
                lstWeapon.DisplayMember = nameof(ListItem.Name);
                lstWeapon.DataSource = lstWeapons;
                _blnLoading = false;
                if (!string.IsNullOrEmpty(strOldSelected))
                    lstWeapon.SelectedValue = strOldSelected;
                else
                    lstWeapon.SelectedIndex = -1;
                lstWeapon.EndUpdate();
            }
            ResumeLayout();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            _blnAddAgain = false;
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            RefreshList();
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            AcceptForm();
        }

        private void chkFreeItem_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShowOnlyAffordItems.Checked)
            {
                RefreshList();
            }
            UpdateWeaponInfo();
        }

        private void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            if (chkShowOnlyAffordItems.Checked && !chkFreeItem.Checked)
            {
                RefreshList();
            }
            UpdateWeaponInfo();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (lstWeapon.SelectedIndex + 1 < lstWeapon.Items.Count)
                {
                    lstWeapon.SelectedIndex++;
                }
                else if (lstWeapon.Items.Count > 0)
                {
                    lstWeapon.SelectedIndex = 0;
                }
                if (dgvWeapons.SelectedRows.Count > 0 && dgvWeapons.Rows.Count > dgvWeapons.SelectedRows[0].Index + 1)
                {
                    dgvWeapons.Rows[dgvWeapons.SelectedRows[0].Index + 1].Selected = true;
                }
                else if (dgvWeapons.Rows.Count > 0)
                {
                    dgvWeapons.Rows[0].Selected = true;
                }
            }
            if (e.KeyCode == Keys.Up)
            {
                if (lstWeapon.SelectedIndex - 1 >= 0)
                {
                    lstWeapon.SelectedIndex--;
                }
                else if (lstWeapon.Items.Count > 0)
                {
                    lstWeapon.SelectedIndex = lstWeapon.Items.Count - 1;
                }
                if (dgvWeapons.SelectedRows.Count > 0 && dgvWeapons.Rows.Count > dgvWeapons.SelectedRows[0].Index - 1)
                {
                    dgvWeapons.Rows[dgvWeapons.SelectedRows[0].Index - 1].Selected = true;
                }
                else if (dgvWeapons.Rows.Count > 0)
                {
                    dgvWeapons.Rows[0].Selected = true;
                }
            }
        }

        private void txtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
                txtSearch.Select(txtSearch.Text.Length, 0);
        }

        private void chkBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
        {
            UpdateWeaponInfo();
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
        /// Name of Weapon that was selected in the dialogue.
        /// </summary>
        public string SelectedWeapon => _strSelectedWeapon;

        /// <summary>
        /// Whether or not the item should be added for free.
        /// </summary>
        public bool FreeCost => chkFreeItem.Checked;

        /// <summary>
        /// Markup percentage.
        /// </summary>
        public decimal Markup => _decMarkup;

        /// <summary>
        /// Only the provided Weapon Categories should be shown in the list.
        /// </summary>
        public string LimitToCategories
        {
            // If passed an empty string, consume it and keep _strLimitToCategories as an empty hash.
            set => _hashLimitToCategories = string.IsNullOrWhiteSpace(value) ? new HashSet<string>() : new HashSet<string>(value.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries));
        }

        public Weapon ParentWeapon { get; set; }
        public HashSet<string> Mounts { get; } = new HashSet<string>();
        #endregion

        #region Methods
        private void RefreshList()
        {
            string strCategory = cboCategory.SelectedValue?.ToString();
            string strFilter = '(' + _objCharacter.Options.BookXPath() + ')';
            if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All" && (GlobalOptions.SearchInCategoryOnly || txtSearch.TextLength == 0))
                strFilter += " and category = \"" + strCategory + '\"';
            else
            {
                StringBuilder objCategoryFilter = new StringBuilder();
                if (_hashLimitToCategories != null && _hashLimitToCategories.Count > 0)
                {
                    foreach (string strLoopCategory in _hashLimitToCategories)
                    {
                        objCategoryFilter.Append("category = \"" + strLoopCategory + "\" or ");
                    }

                    objCategoryFilter.Length -= 4;
                }
                else
                {
                    objCategoryFilter.Append("category != \"Cyberware\" and category != \"Gear\"");
                }

                if (objCategoryFilter.Length > 0)
                {
                    strFilter += " and (" + objCategoryFilter + ')';
                }
            }
            strFilter += CommonFunctions.GenerateSearchXPath(txtSearch.Text);

            XmlNodeList objXmlWeaponList = _objXmlDocument.SelectNodes("/chummer/weapons/weapon[" + strFilter + ']');
            BuildWeaponList(objXmlWeaponList);
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            XmlNode objNode;
            switch (tabControl.SelectedIndex)
            {
                case 0:
                    objNode = _objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[id = \"" + lstWeapon.SelectedValue + "\"]");
                    if (objNode != null)
                    {
                        s_StrSelectCategory = (GlobalOptions.SearchInCategoryOnly || txtSearch.TextLength == 0) ? cboCategory.SelectedValue?.ToString() : objNode["category"]?.InnerText;
                        _strSelectedWeapon = objNode["id"]?.InnerText;
                        _decMarkup = nudMarkup.Value;
                        _blnBlackMarketDiscount = chkBlackMarketDiscount.Checked;

                        DialogResult = DialogResult.OK;
                    }
                    break;
                case 1:
                    if (dgvWeapons.SelectedRows.Count == 1)
                    {
                        if (txtSearch.Text.Length > 1)
                        {
                            string strWeapon = dgvWeapons.SelectedRows[0].Cells[0].Value.ToString();
                            if (!string.IsNullOrEmpty(strWeapon))
                                strWeapon = strWeapon.Substring(0, strWeapon.LastIndexOf('(') - 1);
                            objNode = _objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[id = \"" + strWeapon + "\"]");
                        }
                        else
                        {
                            objNode = _objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[id = \"" + dgvWeapons.SelectedRows[0].Cells[0].Value + "\"]");
                        }
                        if (objNode != null)
                        {
                            s_StrSelectCategory = (GlobalOptions.SearchInCategoryOnly || txtSearch.TextLength == 0) ? cboCategory.SelectedValue?.ToString() : objNode["category"]?.InnerText;
                            _strSelectedWeapon = objNode["id"]?.InnerText;
                        }
                        _decMarkup = nudMarkup.Value;

                        DialogResult = DialogResult.OK;
                    }
                    break;
            }
        }

        private void OpenSourceFromLabel(object sender, EventArgs e)
        {
            CommonFunctions.OpenPDFFromControl(sender, e);
        }

        private void tmrSearch_Tick(object sender, EventArgs e)
        {
            tmrSearch.Stop();
            tmrSearch.Enabled = false;

            RefreshList();
        }

        private void dgvWeapons_DoubleClick(object sender, EventArgs e)
        {
            _blnAddAgain = false;
            AcceptForm();
        }
        #endregion
    }
}
