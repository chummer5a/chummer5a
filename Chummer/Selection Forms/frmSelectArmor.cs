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
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Chummer.Backend.Equipment;

// ReSharper disable LocalizableElement

namespace Chummer
{
    public partial class frmSelectArmor : Form
    {
        private string _strSelectedArmor = string.Empty;

        private bool _blnLoading = true;
        private bool _blnSkipUpdate;
        private bool _blnAddAgain;
        private static string _strSelectCategory = string.Empty;
        private decimal _decMarkup;
        private Armor _objSelectedArmor;

        private readonly XmlDocument _objXmlDocument;
        private readonly Character _objCharacter;

        private readonly List<ListItem> _lstCategory = new List<ListItem>();
        private readonly HashSet<string> _setBlackMarketMaps;
        private int _intRating;
        private bool _blnBlackMarketDiscount;

        #region Control Events

        public frmSelectArmor(Character objCharacter)
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            // Load the Armor information.
            _objXmlDocument = objCharacter.LoadData("armor.xml");
            _setBlackMarketMaps = objCharacter.GenerateBlackMarketMappings(objCharacter.LoadDataXPath("armor.xml").SelectSingleNode("/chummer"));
        }

        private void frmSelectArmor_Load(object sender, EventArgs e)
        {
            if (_objCharacter.Created)
            {
                chkHideOverAvailLimit.Visible = false;
                chkHideOverAvailLimit.Checked = false;
                lblMarkupLabel.Visible = true;
                nudMarkup.Visible = true;
                lblMarkupPercentLabel.Visible = true;
            }
            else
            {
                chkHideOverAvailLimit.Text = string.Format(GlobalSettings.CultureInfo, chkHideOverAvailLimit.Text, _objCharacter.Settings.MaximumAvailability);
                chkHideOverAvailLimit.Checked = GlobalSettings.HideItemsOverAvailLimit;
                lblMarkupLabel.Visible = false;
                nudMarkup.Visible = false;
                lblMarkupPercentLabel.Visible = false;
            }

            DataGridViewCellStyle dataGridViewNuyenCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.TopRight,
                Format = _objCharacter.Settings.NuyenFormat + '¥',
                NullValue = null
            };
            Cost.DefaultCellStyle = dataGridViewNuyenCellStyle;

            // Populate the Armor Category list.
            string strFilterPrefix = "/chummer/armors/armor[(" + _objCharacter.Settings.BookXPath() + ") and category = ";
            XmlNodeList objXmlCategoryList = _objXmlDocument.SelectNodes("/chummer/categories/category");
            if (objXmlCategoryList != null)
            {
                foreach (XmlNode objXmlCategory in objXmlCategoryList)
                {
                    string strInnerText = objXmlCategory.InnerText;
                    if (_objXmlDocument.SelectSingleNode(strFilterPrefix + strInnerText.CleanXPath() + "]") != null)
                    {
                        _lstCategory.Add(new ListItem(strInnerText,
                            objXmlCategory.Attributes?["translate"]?.InnerText ?? strInnerText));
                    }
                }
            }
            _lstCategory.Sort(CompareListItems.CompareNames);

            if (_lstCategory.Count > 0)
            {
                _lstCategory.Insert(0, new ListItem("Show All", LanguageManager.GetString("String_ShowAll")));
            }

            cboCategory.BeginUpdate();
            cboCategory.PopulateWithListItems(_lstCategory);
            chkBlackMarketDiscount.Visible = _objCharacter.BlackMarketDiscount;
            // Select the first Category in the list.
            if (!string.IsNullOrEmpty(_strSelectCategory))
                cboCategory.SelectedValue = _strSelectCategory;

            if (cboCategory.SelectedIndex == -1)
                cboCategory.SelectedIndex = 0;
            cboCategory.EndUpdate();

            _blnLoading = false;

            RefreshList();
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

        private void lstArmor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;

            XmlNode xmlArmor = null;
            string strSelectedId = lstArmor.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
                xmlArmor = _objXmlDocument.SelectSingleNode("/chummer/armors/armor[id = " + strSelectedId.CleanXPath() + "]");
            if (xmlArmor != null)
            {
                // Create the Armor so we can show its Total Avail (some Armor includes Chemical Seal which adds +6 which wouldn't be factored in properly otherwise).
                Armor objArmor = new Armor(_objCharacter);
                List<Weapon> lstWeapons = new List<Weapon>();
                objArmor.Create(xmlArmor, 0, lstWeapons, true, true, true);

                _objSelectedArmor = objArmor;

                int intRating = 0;
                if (xmlArmor.TryGetInt32FieldQuickly("rating", ref intRating))
                {
                    nudRating.Maximum = intRating;
                    if (chkHideOverAvailLimit.Checked)
                    {
                        while (nudRating.Maximum > 1 && !SelectionShared.CheckAvailRestriction(xmlArmor, _objCharacter, nudRating.MaximumAsInt))
                        {
                            --nudRating.Maximum;
                        }
                    }

                    if (chkShowOnlyAffordItems.Checked && !chkFreeItem.Checked)
                    {
                        decimal decCostMultiplier = 1 + (nudMarkup.Value / 100.0m);
                        if (_setBlackMarketMaps.Contains(xmlArmor.SelectSingleNode("category")?.Value))
                            decCostMultiplier *= 0.9m;
                        while (nudRating.Maximum > 1 && !SelectionShared.CheckNuyenRestriction(xmlArmor, _objCharacter.Nuyen, decCostMultiplier, nudRating.MaximumAsInt))
                        {
                            --nudRating.Maximum;
                        }
                    }
                    lblRatingLabel.Visible = true;
                    nudRating.Minimum = 1;
                    nudRating.Value = 1;
                    nudRating.Enabled = nudRating.Minimum != nudRating.Maximum;
                    nudRating.Visible = true;
                    lblRatingNALabel.Visible = false;
                }
                else
                {
                    lblRatingLabel.Visible = true;
                    lblRatingNALabel.Visible = true;
                    nudRating.Minimum = 0;
                    nudRating.Maximum = 0;
                    nudRating.Value = 0;
                    nudRating.Enabled = false;
                    nudRating.Visible = false;
                }

                string strRatingLabel = xmlArmor.SelectSingleNode("ratinglabel")?.Value;
                lblRatingLabel.Text = !string.IsNullOrEmpty(strRatingLabel)
                    ? string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Label_RatingFormat"),
                        LanguageManager.GetString(strRatingLabel))
                    : LanguageManager.GetString("Label_Rating");
            }
            else
            {
                _objSelectedArmor = null;
                lblRatingLabel.Visible = false;
                lblRatingNALabel.Visible = false;
                nudRating.Visible = false;
                nudRating.Enabled = false;
                nudRating.Minimum = 0;
                nudRating.Value = 0;
            }

            UpdateArmorInfo();
        }

        private void RefreshCurrentList(object sender, EventArgs e)
        {
            RefreshList();
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            AcceptForm();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            RefreshList();
        }

        private void chkFreeItem_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShowOnlyAffordItems.Checked)
            {
                RefreshList();
            }
            UpdateArmorInfo();
        }

        private void chkBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
        {
            UpdateArmorInfo();
        }

        private void nudRating_ValueChanged(object sender, EventArgs e)
        {
            UpdateArmorInfo();
        }

        private void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            if (chkShowOnlyAffordItems.Checked && !chkFreeItem.Checked)
            {
                RefreshList();
            }
            UpdateArmorInfo();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down when lstArmor.SelectedIndex + 1 < lstArmor.Items.Count:
                    ++lstArmor.SelectedIndex;
                    break;

                case Keys.Down:
                    {
                        if (lstArmor.Items.Count > 0)
                        {
                            lstArmor.SelectedIndex = 0;
                        }

                        break;
                    }
                case Keys.Up when lstArmor.SelectedIndex - 1 >= 0:
                    --lstArmor.SelectedIndex;
                    break;

                case Keys.Up:
                    {
                        if (lstArmor.Items.Count > 0)
                        {
                            lstArmor.SelectedIndex = lstArmor.Items.Count - 1;
                        }

                        break;
                    }
            }
        }

        private void txtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
                txtSearch.Select(txtSearch.Text.Length, 0);
        }

        private void tmrSearch_Tick(object sender, EventArgs e)
        {
            tmrSearch.Stop();
            tmrSearch.Enabled = false;

            RefreshList();
        }

        private void dgvArmor_DoubleClick(object sender, EventArgs e)
        {
            AcceptForm();
        }

        #endregion Control Events

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
        /// Armor that was selected in the dialogue.
        /// </summary>
        public string SelectedArmor => _strSelectedArmor;

        /// <summary>
        /// Whether or not the item should be added for free.
        /// </summary>
        public bool FreeCost => chkFreeItem.Checked;

        /// <summary>
        /// Markup percentage.
        /// </summary>
        public decimal Markup => _decMarkup;

        /// <summary>
        /// Markup percentage.
        /// </summary>
        public int Rating => _intRating;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Refreshes the displayed lists
        /// </summary>
        private void RefreshList()
        {
            if (_blnLoading)
                return;

            StringBuilder sbdFilter = new StringBuilder('(' + _objCharacter.Settings.BookXPath() + ')');

            string strCategory = cboCategory.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All" && (GlobalSettings.SearchInCategoryOnly || txtSearch.TextLength == 0))
                sbdFilter.Append(" and category = ").Append(strCategory.CleanXPath());
            else
            {
                StringBuilder sbdCategoryFilter = new StringBuilder();
                foreach (string strItem in _lstCategory.Select(x => x.Value))
                {
                    if (!string.IsNullOrEmpty(strItem))
                        sbdCategoryFilter.Append("category = ").Append(strItem.CleanXPath()).Append(" or ");
                }
                if (sbdCategoryFilter.Length > 0)
                {
                    sbdCategoryFilter.Length -= 4;
                    sbdFilter.Append(" and (").Append(sbdCategoryFilter).Append(')');
                }
            }
            if (!string.IsNullOrEmpty(txtSearch.Text))
                sbdFilter.Append(" and ").Append(CommonFunctions.GenerateSearchXPath(txtSearch.Text));

            BuildArmorList(_objXmlDocument.SelectNodes("/chummer/armors/armor[" + sbdFilter + ']'));
        }

        /// <summary>
        /// Builds the list of Armors to render in the active tab.
        /// </summary>
        /// <param name="objXmlArmorList">XmlNodeList of Armors to render.</param>
        private void BuildArmorList(XmlNodeList objXmlArmorList)
        {
            switch (tabControl.SelectedIndex)
            {
                case 1:
                    DataTable tabArmor = new DataTable("armor");
                    tabArmor.Columns.Add("ArmorGuid");
                    tabArmor.Columns.Add("ArmorName");
                    tabArmor.Columns.Add("Armor");
                    tabArmor.Columns["Armor"].DataType = typeof(int);
                    tabArmor.Columns.Add("Capacity");
                    tabArmor.Columns["Capacity"].DataType = typeof(decimal);
                    tabArmor.Columns.Add("Avail");
                    tabArmor.Columns["Avail"].DataType = typeof(AvailabilityValue);
                    tabArmor.Columns.Add("Special");
                    tabArmor.Columns.Add("Source");
                    tabArmor.Columns["Source"].DataType = typeof(SourceString);
                    tabArmor.Columns.Add("Cost");
                    tabArmor.Columns["Cost"].DataType = typeof(NuyenString);

                    // Populate the Armor list.
                    foreach (XmlNode objXmlArmor in objXmlArmorList)
                    {
                        decimal decCostMultiplier = 1 + (nudMarkup.Value / 100.0m);
                        if (_setBlackMarketMaps.Contains(objXmlArmor["category"]?.InnerText))
                            decCostMultiplier *= 0.9m;
                        if (!chkHideOverAvailLimit.Checked
                            || SelectionShared.CheckAvailRestriction(objXmlArmor, _objCharacter) && (chkFreeItem.Checked
                                || !chkShowOnlyAffordItems.Checked
                                || SelectionShared.CheckNuyenRestriction(
                                    objXmlArmor, _objCharacter.Nuyen, decCostMultiplier)))
                        {
                            Armor objArmor = new Armor(_objCharacter);
                            List<Weapon> lstWeapons = new List<Weapon>();
                            objArmor.Create(objXmlArmor, 0, lstWeapons, true, true, true);

                            string strArmorGuid = objArmor.SourceIDString;
                            string strArmorName = objArmor.CurrentDisplayName;
                            int intArmor = objArmor.TotalArmor;
                            decimal decCapacity = Convert.ToDecimal(objArmor.CalculatedCapacity, GlobalSettings.CultureInfo);
                            AvailabilityValue objAvail = objArmor.TotalAvailTuple();
                            StringBuilder strAccessories = new StringBuilder();
                            foreach (ArmorMod objMod in objArmor.ArmorMods)
                            {
                                strAccessories.AppendLine(objMod.CurrentDisplayName);
                            }
                            foreach (Gear objGear in objArmor.GearChildren)
                            {
                                strAccessories.AppendLine(objGear.CurrentDisplayName);
                            }
                            if (strAccessories.Length > 0)
                                strAccessories.Length -= Environment.NewLine.Length;
                            SourceString strSource = new SourceString(objArmor.Source, objArmor.DisplayPage(GlobalSettings.Language), GlobalSettings.Language, GlobalSettings.CultureInfo, _objCharacter);
                            NuyenString strCost = new NuyenString(objArmor.DisplayCost(out decimal _, false));

                            tabArmor.Rows.Add(strArmorGuid, strArmorName, intArmor, decCapacity, objAvail, strAccessories.ToString(), strSource, strCost);
                        }
                    }

                    DataSet set = new DataSet("armor");
                    set.Tables.Add(tabArmor);

                    dgvArmor.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                    dgvArmor.DataSource = set;
                    dgvArmor.DataMember = "armor";
                    break;

                default:
                    List<ListItem> lstArmors = new List<ListItem>();
                    int intOverLimit = 0;
                    string strSpace = LanguageManager.GetString("String_Space");
                    foreach (XmlNode objXmlArmor in objXmlArmorList)
                    {
                        decimal decCostMultiplier = 1 + (nudMarkup.Value / 100.0m);
                        if (_setBlackMarketMaps.Contains(objXmlArmor["category"]?.InnerText))
                            decCostMultiplier *= 0.9m;
                        if ((!chkHideOverAvailLimit.Checked
                            || (chkHideOverAvailLimit.Checked
                                && SelectionShared.CheckAvailRestriction(objXmlArmor, _objCharacter)))
                             && (chkFreeItem.Checked
                                || !chkShowOnlyAffordItems.Checked
                                || (chkShowOnlyAffordItems.Checked
                                    && SelectionShared.CheckNuyenRestriction(objXmlArmor, _objCharacter.Nuyen, decCostMultiplier))))
                        {
                            string strDisplayName = objXmlArmor["translate"]?.InnerText ?? objXmlArmor["name"]?.InnerText;
                            if (!GlobalSettings.SearchInCategoryOnly && txtSearch.TextLength != 0)
                            {
                                string strCategory = objXmlArmor["category"]?.InnerText;
                                if (!string.IsNullOrEmpty(strCategory))
                                {
                                    ListItem objFoundItem = _lstCategory.Find(objFind => objFind.Value.ToString() == strCategory);
                                    if (!string.IsNullOrEmpty(objFoundItem.Name))
                                    {
                                        strDisplayName += strSpace + '[' + objFoundItem.Name + ']';
                                    }
                                }
                            }

                            lstArmors.Add(new ListItem(objXmlArmor["id"]?.InnerText, strDisplayName));
                        }
                        else
                            ++intOverLimit;
                    }

                    lstArmors.Sort(CompareListItems.CompareNames);
                    if (intOverLimit > 0)
                    {
                        // Add after sort so that it's always at the end
                        lstArmors.Add(new ListItem(string.Empty,
                            string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("String_RestrictedItemsHidden"),
                                intOverLimit)));
                    }
                    _blnLoading = true;
                    string strOldSelected = lstArmor.SelectedValue?.ToString();
                    lstArmor.BeginUpdate();
                    lstArmor.PopulateWithListItems(lstArmors);
                    _blnLoading = false;
                    if (!string.IsNullOrEmpty(strOldSelected))
                        lstArmor.SelectedValue = strOldSelected;
                    else
                        lstArmor.SelectedIndex = -1;
                    lstArmor.EndUpdate();
                    break;
            }
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            string strSelectedId = string.Empty;
            switch (tabControl.SelectedIndex)
            {
                case 0:
                    strSelectedId = lstArmor.SelectedValue?.ToString();
                    break;

                case 1:
                    strSelectedId = dgvArmor.SelectedRows[0].Cells[0].Value?.ToString();
                    break;
            }
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                _strSelectCategory = (GlobalSettings.SearchInCategoryOnly || txtSearch.TextLength == 0)
                    ? cboCategory.SelectedValue?.ToString()
                    : _objXmlDocument.SelectSingleNode("/chummer/armors/armor[id = " + strSelectedId.CleanXPath() + "]/category")?.InnerText;
                _strSelectedArmor = strSelectedId;
                _decMarkup = nudMarkup.Value;
                _intRating = nudRating.ValueAsInt;
                _blnBlackMarketDiscount = chkBlackMarketDiscount.Checked;

                DialogResult = DialogResult.OK;
            }
        }

        private void UpdateArmorInfo()
        {
            if (_blnLoading || _blnSkipUpdate)
                return;

            _blnSkipUpdate = true;
            if (_objSelectedArmor != null)
            {
                bool blnCanBlackMarketDiscount = _setBlackMarketMaps.Contains(_objSelectedArmor.Category);
                chkBlackMarketDiscount.Enabled = blnCanBlackMarketDiscount;
                if (!chkBlackMarketDiscount.Checked)
                {
                    chkBlackMarketDiscount.Checked = GlobalSettings.AssumeBlackMarket && blnCanBlackMarketDiscount;
                }
                else if (!blnCanBlackMarketDiscount)
                {
                    //Prevent chkBlackMarketDiscount from being checked if the category doesn't match.
                    chkBlackMarketDiscount.Checked = false;
                }

                _objSelectedArmor.DiscountCost = chkBlackMarketDiscount.Checked;
                _objSelectedArmor.Rating = nudRating.ValueAsInt;

                lblSource.Text = _objSelectedArmor.SourceDetail.ToString();
                lblSource.SetToolTip(_objSelectedArmor.SourceDetail.LanguageBookTooltip);
                lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);

                lblArmorValueLabel.Visible = true;
                lblArmorValue.Text = _objSelectedArmor.DisplayArmorValue;
                lblCapacityLabel.Visible = true;
                lblCapacity.Text = _objSelectedArmor.CalculatedCapacity;

                lblCostLabel.Visible = true;
                decimal decItemCost = 0;
                if (chkFreeItem.Checked)
                {
                    lblCost.Text = (0.0m).ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                }
                else
                {
                    lblCost.Text = _objSelectedArmor.DisplayCost(out decItemCost, true, nudMarkup.Value / 100.0m);
                }

                AvailabilityValue objTotalAvail = _objSelectedArmor.TotalAvailTuple();
                lblAvailLabel.Visible = true;
                lblTestLabel.Visible = true;
                lblAvail.Text = objTotalAvail.ToString();
                lblTest.Text = _objCharacter.AvailTest(decItemCost, objTotalAvail);
            }
            else
            {
                chkBlackMarketDiscount.Enabled = false;
                chkBlackMarketDiscount.Checked = false;
                lblSourceLabel.Visible = false;
                lblSource.Text = string.Empty;
                lblSource.SetToolTip(string.Empty);

                lblArmorValueLabel.Visible = false;
                lblArmorValue.Text = string.Empty;
                lblCapacityLabel.Visible = false;
                lblCapacity.Text = string.Empty;
                lblCostLabel.Visible = false;
                lblCost.Text = string.Empty;
                lblAvailLabel.Visible = false;
                lblTestLabel.Visible = false;
                lblAvail.Text = string.Empty;
                lblTest.Text = string.Empty;
            }
            _blnSkipUpdate = false;
        }

        private void OpenSourceFromLabel(object sender, EventArgs e)
        {
            CommonFunctions.OpenPdfFromControl(sender, e);
        }

        #endregion Methods
    }
}
