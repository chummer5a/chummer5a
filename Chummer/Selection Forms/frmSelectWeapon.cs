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

// ReSharper disable LocalizableElement

namespace Chummer
{
    public partial class frmSelectWeapon : Form
    {
        private string _strSelectedWeapon = string.Empty;
        private decimal _decMarkup;

        private bool _blnSkipUpdate;
        private bool _blnAddAgain;
        private bool _blnBlackMarketDiscount;
        private HashSet<string> _strLimitToCategories = new HashSet<string>();
        private static string s_StrSelectCategory = string.Empty;
        private readonly Character _objCharacter;
        private readonly XmlDocument _objXmlDocument;
        private Weapon _objSelectedWeapon;

        private readonly List<ListItem> _lstCategory = new List<ListItem>();
        private readonly HashSet<string> _setBlackMarketMaps;

        #region Control Events
        public frmSelectWeapon(Character objCharacter)
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
                chkHideOverAvailLimit.Text = chkHideOverAvailLimit.Text.Replace("{0}", _objCharacter.MaximumAvailability.ToString());
                chkHideOverAvailLimit.Checked = _objCharacter.Options.HideItemsOverAvailLimit;
            }

            // Populate the Weapon Category list.
            
                // Populate the Category list.
                using (XmlNodeList xmlCategoryList = _objXmlDocument.SelectNodes("/chummer/categories/category"))
                    if (xmlCategoryList != null)
                    {
                        foreach (XmlNode objXmlCategory in xmlCategoryList)
                        {
                            string strInnerText = objXmlCategory.InnerText;
                            if (_strLimitToCategories.Count == 0 || _strLimitToCategories.Contains(strInnerText))
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

            chkBlackMarketDiscount.Visible = _objCharacter.BlackMarketDiscount;

            // Select the first Category in the list.
            if (string.IsNullOrEmpty(s_StrSelectCategory))
                cboCategory.SelectedIndex = 0;
            else
                cboCategory.SelectedValue = s_StrSelectCategory;

            if (cboCategory.SelectedIndex == -1)
                cboCategory.SelectedIndex = 0;
            cboCategory.EndUpdate();

            cboCategory_SelectedIndexChanged(sender, e);
        }

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshList();
        }

        private void lstWeapon_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Retireve the information for the selected Weapon.
            XmlNode xmlWeapon = null;
            string strSelectedId = lstWeapon.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
                xmlWeapon = _objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[id = \"" + lstWeapon.SelectedValue + "\"]");
            if (xmlWeapon != null)
            {
                Weapon objWeapon = new Weapon(_objCharacter);
                objWeapon.Create(xmlWeapon, null, true, false, true);
                _objSelectedWeapon = objWeapon;
            }
            else
                _objSelectedWeapon = null;

            UpdateWeaponInfo();
        }

        private void UpdateWeaponInfo()
        {
            if (_blnSkipUpdate)
                return;
            _blnSkipUpdate = true;
            if (_objSelectedWeapon != null)
            {
                chkBlackMarketDiscount.Enabled = true;
                chkBlackMarketDiscount.Checked = _setBlackMarketMaps.Contains(_objSelectedWeapon.Category);

                _objSelectedWeapon.DiscountCost = chkBlackMarketDiscount.Checked;

                lblWeaponReach.Text = _objSelectedWeapon.TotalReach.ToString();
                lblWeaponDamage.Text = _objSelectedWeapon.CalculatedDamage(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblWeaponAP.Text = _objSelectedWeapon.TotalAP(GlobalOptions.Language);
                lblWeaponMode.Text = _objSelectedWeapon.CalculatedMode(GlobalOptions.Language);
                lblWeaponRC.Text = _objSelectedWeapon.TotalRC;
                lblWeaponAmmo.Text = _objSelectedWeapon.CalculatedAmmo(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblWeaponAccuracy.Text = _objSelectedWeapon.TotalAccuracy.ToString();

                decimal decItemCost = 0;
                if (chkFreeItem.Checked)
                {
                    lblWeaponCost.Text = (0.0m).ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                }
                else
                {
                    lblWeaponCost.Text = _objSelectedWeapon.DisplayCost(out decItemCost, nudMarkup.Value / 100.0m);
                }

                AvailabilityValue objTotalAvail = _objSelectedWeapon.TotalAvailTuple();
                lblWeaponAvail.Text = objTotalAvail.ToString(GlobalOptions.CultureInfo, GlobalOptions.Language);
                lblTest.Text = _objCharacter.AvailTest(decItemCost, objTotalAvail);

                string strPage = _objSelectedWeapon.DisplayPage(GlobalOptions.Language);
                lblSource.Text = CommonFunctions.LanguageBookShort(_objSelectedWeapon.Source, GlobalOptions.Language) + ' ' + strPage;

                // Build a list of included Accessories and Modifications that come with the weapon.
                StringBuilder strAccessories = new StringBuilder();
                foreach (WeaponAccessory objAccessory in _objSelectedWeapon.WeaponAccessories)
                {
                    strAccessories.AppendLine(objAccessory.DisplayName(GlobalOptions.Language));
                }
                if (strAccessories.Length > 0)
                    strAccessories.Length -= Environment.NewLine.Length;

                lblIncludedAccessories.Text = strAccessories.Length == 0 ? LanguageManager.GetString("String_None", GlobalOptions.Language) : strAccessories.ToString();

                tipTooltip.SetToolTip(lblSource, CommonFunctions.LanguageBookLong(_objSelectedWeapon.Source, GlobalOptions.Language) + ' ' + LanguageManager.GetString("String_Page", GlobalOptions.Language) + ' ' + strPage);
            }
            else
            {
                chkBlackMarketDiscount.Enabled = false;
                chkBlackMarketDiscount.Checked = false;
                lblWeaponReach.Text = string.Empty;
                lblWeaponDamage.Text = string.Empty;
                lblWeaponAP.Text = string.Empty;
                lblWeaponMode.Text = string.Empty;
                lblWeaponRC.Text = string.Empty;
                lblWeaponAmmo.Text = string.Empty;
                lblWeaponAccuracy.Text = string.Empty;
                lblWeaponCost.Text = string.Empty;
                lblWeaponAvail.Text = string.Empty;
                lblTest.Text = string.Empty;
                lblSource.Text = string.Empty;
                lblIncludedAccessories.Text = string.Empty;
                tipTooltip.SetToolTip(lblSource, string.Empty);
            }
            _blnSkipUpdate = false;
        }

        private void BuildWeaponList(XmlNodeList objNodeList)
        {
            if (tabControl.SelectedIndex == 1)
            {
                DataTable tabWeapons = new DataTable("weapons");
                tabWeapons.Columns.Add("WeaponGuid");
                tabWeapons.Columns.Add("WeaponName");
                tabWeapons.Columns.Add("Dice");
                tabWeapons.Columns.Add("Accuracy");
                tabWeapons.Columns["Accuracy"].DataType = typeof(int);
                tabWeapons.Columns.Add("Damage");
                tabWeapons.Columns.Add("AP");
                tabWeapons.Columns.Add("RC");
                tabWeapons.Columns["RC"].DataType = typeof(int);
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

                foreach (XmlNode objXmlWeapon in objNodeList)
                {
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

                    Weapon objWeapon = new Weapon(_objCharacter);
                    objWeapon.Create(objXmlWeapon, null, true, false, true);

                    string strID = objWeapon.SourceID.ToString("D");
                    string strWeaponName = objWeapon.DisplayName(GlobalOptions.Language);
                    string strDice = objWeapon.GetDicePool(GlobalOptions.CultureInfo);
                    int intAccuracy = objWeapon.TotalAccuracy;
                    string strDamage = objWeapon.CalculatedDamage(GlobalOptions.CultureInfo, GlobalOptions.Language);
                    string strAP = objWeapon.TotalAP(GlobalOptions.Language);
                    if (strAP == "-")
                        strAP = "0";
                    int.TryParse(objWeapon.TotalRC, out int intRC);
                    string strAmmo = objWeapon.CalculatedAmmo(GlobalOptions.CultureInfo, GlobalOptions.Language);
                    string strMode = objWeapon.CalculatedMode(GlobalOptions.Language);
                    string strReach = objWeapon.TotalReach.ToString();
                    StringBuilder strAccessories = new StringBuilder();
                    foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                    {
                        strAccessories.AppendLine(objAccessory.DisplayName(GlobalOptions.Language));
                    }
                    if (strAccessories.Length > 0)
                        strAccessories.Length -= Environment.NewLine.Length;
                    AvailabilityValue objAvail = objWeapon.TotalAvailTuple();
                    SourceString strSource = new SourceString(objWeapon.Source, objWeapon.DisplayPage(GlobalOptions.Language));
                    NuyenString strCost = new NuyenString(objWeapon.DisplayCost(out decimal _));

                    tabWeapons.Rows.Add(strID, strWeaponName, strDice, intAccuracy, strDamage, strAP, intRC, strAmmo, strMode, strReach, strAccessories.ToString(), objAvail, strSource, strCost);
                }

                DataSet set = new DataSet("weapons");
                set.Tables.Add(tabWeapons);

                if (cboCategory.SelectedValue == null || cboCategory.SelectedValue.ToString() == "Show All")
                {
                    dgvWeapons.Columns[5].Visible = true;
                    dgvWeapons.Columns[6].Visible = true;
                    dgvWeapons.Columns[7].Visible = true;
                    dgvWeapons.Columns[8].Visible = true;
                }
                else if (cboCategory.SelectedValue.ToString() == "Blades" || cboCategory.SelectedValue.ToString() == "Clubs" || cboCategory.SelectedValue.ToString() == "Improvised Weapons" || cboCategory.SelectedValue.ToString() == "Exotic Melee Weapons" || cboCategory.SelectedValue.ToString() == "Unarmed")
                {
                    dgvWeapons.Columns[5].Visible = false;
                    dgvWeapons.Columns[6].Visible = false;
                    dgvWeapons.Columns[7].Visible = false;
                    dgvWeapons.Columns[8].Visible = true;
                }
                else
                {
                    dgvWeapons.Columns[5].Visible = true;
                    dgvWeapons.Columns[6].Visible = true;
                    dgvWeapons.Columns[7].Visible = true;
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
                List<ListItem> lstWeapons = new List<ListItem>();
                foreach (XmlNode objXmlWeapon in objNodeList)
                {
                    if (objXmlWeapon["cyberware"]?.InnerText == bool.TrueString)
                        continue;

                    string strTest = objXmlWeapon["mount"]?.InnerText;
                    if (!string.IsNullOrEmpty(strTest) && !Mounts.Contains(strTest))
                    {
                        continue;
                    }

                    strTest = objXmlWeapon["extramount"]?.InnerText;
                    if (!string.IsNullOrEmpty(strTest) && !Mounts.Contains(strTest))
                    {
                        continue;
                    }

                    if (chkHideOverAvailLimit.Checked && !SelectionShared.CheckAvailRestriction(objXmlWeapon, _objCharacter))
                    {
                        continue;
                    }
                    lstWeapons.Add(new ListItem(objXmlWeapon["id"]?.InnerText, objXmlWeapon["translate"]?.InnerText ?? objXmlWeapon["name"]?.InnerText));
                }
                lstWeapons.Sort(CompareListItems.CompareNames);
                lstWeapon.BeginUpdate();
                lstWeapon.DataSource = null;
                lstWeapon.ValueMember = "Value";
                lstWeapon.DisplayMember = "Name";
                lstWeapon.DataSource = lstWeapons;
                lstWeapon.EndUpdate();
            }
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
            UpdateWeaponInfo();
        }

        private void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
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
            set => _strLimitToCategories = new HashSet<string>(value.Split(','));
        }

        public bool Underbarrel { get; set; }
        public string Mounts { get; set; } = string.Empty;
        #endregion

        #region Methods
        private void RefreshList()
        {
            string strCategory = cboCategory.SelectedValue?.ToString();
            string strFilter = '(' + _objCharacter.Options.BookXPath() + ')';
            if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All" && (_objCharacter.Options.SearchInCategoryOnly || txtSearch.TextLength == 0))
                strFilter += " and category = \"" + strCategory + '\"';
            else
            {
                StringBuilder objCategoryFilter = new StringBuilder();
                if (_strLimitToCategories.Count > 0)
                {
                    foreach (string strLoopCategory in _strLimitToCategories)
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
                    strFilter += " and (" + objCategoryFilter.ToString() + ')';
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
                        s_StrSelectCategory = (_objCharacter.Options.SearchInCategoryOnly || txtSearch.TextLength == 0) ? cboCategory.SelectedValue?.ToString() : objNode["category"]?.InnerText;
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
                            s_StrSelectCategory = (_objCharacter.Options.SearchInCategoryOnly || txtSearch.TextLength == 0) ? cboCategory.SelectedValue?.ToString() : objNode["category"]?.InnerText;
                            _strSelectedWeapon = objNode["id"]?.InnerText;
                        }
                        _decMarkup = nudMarkup.Value;

                        DialogResult = DialogResult.OK;
                    }
                    break;
            }
        }

        private void MoveControls()
        {
            int intWidth = Math.Max(lblWeaponDamageLabel.Width, lblWeaponAPLabel.Width);
            intWidth = Math.Max(intWidth, lblWeaponReachLabel.Width);
            intWidth = Math.Max(intWidth, lblWeaponAvailLabel.Width);
            intWidth = Math.Max(intWidth, lblWeaponCostLabel.Width);

            lblWeaponDamage.Left = lblWeaponDamageLabel.Left + intWidth + 6;
            lblWeaponAP.Left = lblWeaponAPLabel.Left + intWidth + 6;
            lblWeaponReach.Left = lblWeaponReachLabel.Left + intWidth + 6;
            lblWeaponAvail.Left = lblWeaponAvailLabel.Left + intWidth + 6;
            lblWeaponCost.Left = lblWeaponCostLabel.Left + intWidth + 6;

            lblWeaponRCLabel.Left = lblWeaponAP.Left + 74;
            lblWeaponRC.Left = lblWeaponRCLabel.Left + lblWeaponRCLabel.Width + 6;

            intWidth = Math.Max(lblWeaponAmmoLabel.Width, lblWeaponModeLabel.Width);
            intWidth = Math.Max(intWidth, lblTestLabel.Width);
            intWidth = Math.Max(intWidth, lblWeaponAccuracy.Width);
            lblWeaponAmmoLabel.Left = lblWeaponAP.Left + 74;
            lblWeaponAmmo.Left = lblWeaponAmmoLabel.Left + intWidth + 6;
            lblWeaponModeLabel.Left = lblWeaponAP.Left + 74;
            lblWeaponMode.Left = lblWeaponModeLabel.Left + intWidth + 6;
            lblTestLabel.Left = lblWeaponAP.Left + 74;
            lblTest.Left = lblTestLabel.Left + intWidth + 6;
            lblWeaponAccuracyLabel.Left = lblWeaponAP.Left + 74;
            lblWeaponAccuracy.Left = lblWeaponAccuracyLabel.Left + intWidth + 6;

            nudMarkup.Left = lblMarkupLabel.Left + lblMarkupLabel.Width + 6;
            lblMarkupPercentLabel.Left = nudMarkup.Left + nudMarkup.Width;

            lblSource.Left = lblSourceLabel.Left + lblSourceLabel.Width + 6;

            lblSearchLabel.Left = txtSearch.Left - 6 - lblSearchLabel.Width;
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
