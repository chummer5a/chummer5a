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
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Chummer.Backend.Equipment;

// ReSharper disable LocalizableElement

namespace Chummer
{
    public partial class SelectWeapon : Form
    {
        private string _strSelectedWeapon = string.Empty;
        private decimal _decMarkup;

        private bool _blnLoading = true;
        private bool _blnSkipUpdate;
        private bool _blnAddAgain;
        private bool _blnBlackMarketDiscount;
        private readonly HashSet<string> _setLimitToCategories = Utils.StringHashSetPool.Get();
        private static string _strSelectCategory = string.Empty;
        private readonly Character _objCharacter;
        private readonly XmlDocument _objXmlDocument;
        private Weapon _objSelectedWeapon;

        private readonly List<ListItem> _lstCategory = Utils.ListItemListPool.Get();
        private readonly HashSet<string> _setBlackMarketMaps = Utils.StringHashSetPool.Get();

        #region Control Events

        public SelectWeapon(Character objCharacter)
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
            _setBlackMarketMaps.AddRange(_objCharacter.GenerateBlackMarketMappings(_objCharacter.LoadDataXPath("weapons.xml").SelectSingleNodeAndCacheExpression("/chummer")));
        }

        private async void SelectWeapon_Load(object sender, EventArgs e)
        {
            DataGridViewCellStyle dataGridViewNuyenCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.TopRight,
                Format = _objCharacter.Settings.NuyenFormat + '¥',
                NullValue = null
            };
            dgvc_Cost.DefaultCellStyle = dataGridViewNuyenCellStyle;

            if (_objCharacter.Created)
            {
                await chkHideOverAvailLimit.DoThreadSafeAsync(x =>
                {
                    x.Visible = false;
                    x.Checked = false;
                });
            }
            else
            {
                await chkHideOverAvailLimit.DoThreadSafeAsync(x =>
                {
                    x.Text = string.Format(
                        GlobalSettings.CultureInfo, x.Text,
                        _objCharacter.Settings.MaximumAvailability);
                    x.Checked = GlobalSettings.HideItemsOverAvailLimit;
                });
            }

            // Populate the Weapon Category list.
            // Populate the Category list.
            string strFilterPrefix = "/chummer/weapons/weapon[(" + _objCharacter.Settings.BookXPath() + ") and category = ";
            using (XmlNodeList xmlCategoryList = _objXmlDocument.SelectNodes("/chummer/categories/category"))
            {
                if (xmlCategoryList != null)
                {
                    foreach (XmlNode objXmlCategory in xmlCategoryList)
                    {
                        string strInnerText = objXmlCategory.InnerText;
                        if ((_setLimitToCategories.Count == 0 || _setLimitToCategories.Contains(strInnerText))
                            && await BuildWeaponList(_objXmlDocument.SelectNodes(strFilterPrefix + strInnerText.CleanXPath() + ']'), true))
                            _lstCategory.Add(new ListItem(strInnerText, objXmlCategory.Attributes?["translate"]?.InnerText ?? strInnerText));
                    }
                }
            }

            _lstCategory.Sort(CompareListItems.CompareNames);

            _lstCategory.Insert(0, new ListItem("Show All", await LanguageManager.GetStringAsync("String_ShowAll")));
            
            await cboCategory.PopulateWithListItemsAsync(_lstCategory);
            await cboCategory.DoThreadSafeAsync(x =>
            {
                // Select the first Category in the list.
                if (string.IsNullOrEmpty(_strSelectCategory))
                    x.SelectedIndex = 0;
                else
                {
                    x.SelectedValue = _strSelectCategory;
                    if (x.SelectedIndex == -1)
                        x.SelectedIndex = 0;
                }
            });

            await chkBlackMarketDiscount.DoThreadSafeAsync(x => x.Visible = _objCharacter.BlackMarketDiscount);

            _blnLoading = false;
            await RefreshList();
        }

        private async void RefreshCurrentList(object sender, EventArgs e)
        {
            await RefreshList();
        }

        private async void lstWeapon_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading || _blnSkipUpdate)
                return;

            // Retireve the information for the selected Weapon.
            XmlNode xmlWeapon = null;
            string strSelectedId = await lstWeapon.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString());
            if (!string.IsNullOrEmpty(strSelectedId))
                xmlWeapon = _objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[id = " + strSelectedId.CleanXPath() + ']');
            if (xmlWeapon != null)
            {
                Weapon objWeapon = new Weapon(_objCharacter);
                objWeapon.Create(xmlWeapon, null, true, false, true);
                objWeapon.Parent = ParentWeapon;
                _objSelectedWeapon?.Dispose();
                _objSelectedWeapon = objWeapon;
            }
            else
            {
                _objSelectedWeapon?.Dispose();
                _objSelectedWeapon = null;
            }

            await UpdateWeaponInfo();
        }

        private async ValueTask UpdateWeaponInfo()
        {
            if (_blnLoading || _blnSkipUpdate)
                return;
            _blnSkipUpdate = true;
            await this.DoThreadSafeAsync(x => x.SuspendLayout());
            try
            {
                if (_objSelectedWeapon != null)
                {
                    bool blnCanBlackMarketDiscount = _setBlackMarketMaps.Contains(_objSelectedWeapon.Category);
                    await chkBlackMarketDiscount.DoThreadSafeAsync(x =>
                    {
                        x.Enabled = blnCanBlackMarketDiscount;
                        if (!x.Checked)
                        {
                            x.Checked = GlobalSettings.AssumeBlackMarket && blnCanBlackMarketDiscount;
                        }
                        else if (!blnCanBlackMarketDiscount)
                        {
                            //Prevent chkBlackMarketDiscount from being checked if the category doesn't match.
                            x.Checked = false;
                        }

                        _objSelectedWeapon.DiscountCost = x.Checked;
                    });

                    string strReach = _objSelectedWeapon.TotalReach.ToString(GlobalSettings.CultureInfo);
                    await lblWeaponReach.DoThreadSafeAsync(x => x.Text = strReach);
                    await lblWeaponReachLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strReach));
                    string strDamage = _objSelectedWeapon.DisplayDamage;
                    await lblWeaponDamage.DoThreadSafeAsync(x => x.Text = strDamage);
                    await lblWeaponDamageLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strDamage));
                    string strAP = _objSelectedWeapon.DisplayTotalAP;
                    await lblWeaponAP.DoThreadSafeAsync(x => x.Text = strAP);
                    await lblWeaponAPLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strAP));
                    string strMode = _objSelectedWeapon.DisplayMode;
                    await lblWeaponMode.DoThreadSafeAsync(x => x.Text = strMode);
                    await lblWeaponModeLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strMode));
                    string strRC = _objSelectedWeapon.DisplayTotalRC;
                    await lblWeaponRC.DoThreadSafeAsync(x => x.Text = strRC);
                    await lblWeaponRC.SetToolTipAsync(_objSelectedWeapon.RCToolTip);
                    await lblWeaponRCLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strRC));
                    string strAmmo = _objSelectedWeapon.DisplayAmmo;
                    await lblWeaponAmmo.DoThreadSafeAsync(x => x.Text = strAmmo);
                    await lblWeaponAmmoLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strAmmo));
                    string strAccuracy = _objSelectedWeapon.DisplayAccuracy;
                    await lblWeaponAccuracy.DoThreadSafeAsync(x => x.Text = strAccuracy);
                    await lblWeaponAccuracyLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strAccuracy));
                    string strConceal = _objSelectedWeapon.DisplayConcealability;
                    await lblWeaponConceal.DoThreadSafeAsync(x => x.Text = strConceal);
                    await lblWeaponConcealLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strConceal));

                    decimal decItemCost = 0;
                    string strWeaponCost;
                    if (await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked))
                    {
                        strWeaponCost = (0.0m).ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                    }
                    else
                    {
                        strWeaponCost = _objSelectedWeapon.DisplayCost(out decItemCost, await nudMarkup.DoThreadSafeFuncAsync(x => x.Value) / 100.0m);
                    }

                    await lblWeaponCost.DoThreadSafeAsync(x => x.Text = strWeaponCost);
                    await lblWeaponCostLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strWeaponCost));

                    AvailabilityValue objTotalAvail = _objSelectedWeapon.TotalAvailTuple();
                    string strAvail = objTotalAvail.ToString();
                    await lblWeaponAvail.DoThreadSafeAsync(x => x.Text = strAvail);
                    await lblWeaponAvailLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strAvail));
                    string strTest = _objCharacter.AvailTest(decItemCost, objTotalAvail);
                    await lblTest.DoThreadSafeAsync(x => x.Text = strTest);
                    await lblTestLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strTest));
                    await _objSelectedWeapon.SetSourceDetailAsync(lblSource);
                    await lblSource.DoThreadSafeFuncAsync(x => x.Text)
                                   .ContinueWith(y => lblSourceLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(y.Result)))
                                   .Unwrap();

                    string strIncludedAccessores;
                    // Build a list of included Accessories and Modifications that come with the weapon.
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdAccessories))
                    {
                        foreach (WeaponAccessory objAccessory in _objSelectedWeapon.WeaponAccessories)
                        {
                            sbdAccessories.AppendLine(objAccessory.CurrentDisplayName);
                        }

                        if (sbdAccessories.Length > 0)
                            sbdAccessories.Length -= Environment.NewLine.Length;

                        strIncludedAccessores = sbdAccessories.Length == 0
                            ? await LanguageManager.GetStringAsync("String_None")
                            : sbdAccessories.ToString();
                    }

                    await lblIncludedAccessories.DoThreadSafeAsync(x => x.Text = strIncludedAccessores);
                    await tlpRight.DoThreadSafeAsync(x => x.Visible = true);
                    await gpbIncludedAccessories.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strIncludedAccessores));
                }
                else
                {
                    await chkBlackMarketDiscount.DoThreadSafeAsync(x => x.Checked = false);
                    await tlpRight.DoThreadSafeAsync(x => x.Visible = false);
                    await gpbIncludedAccessories.DoThreadSafeAsync(x => x.Visible = false);
                }
            }
            finally
            {
                await this.DoThreadSafeAsync(x => x.ResumeLayout());
            }
            _blnSkipUpdate = false;
        }

        private async ValueTask<bool> BuildWeaponList(XmlNodeList objNodeList, bool blnForCategories = false)
        {
            await this.DoThreadSafeAsync(x => x.SuspendLayout());
            try
            {
                bool blnHideOverAvailLimit = await chkHideOverAvailLimit.DoThreadSafeFuncAsync(x => x.Checked);
                bool blnShowOnlyAffordItems = await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked);
                bool blnFreeItem = await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked);
                decimal decBaseCostMultiplier = 1 + (await nudMarkup.DoThreadSafeFuncAsync(x => x.Value) / 100.0m);
                if (await tabControl.DoThreadSafeFuncAsync(x => x.SelectedIndex) == 1 && !blnForCategories)
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
                    tabWeapons.Columns.Add("Concealability");
                    tabWeapons.Columns.Add("Accessories");
                    tabWeapons.Columns.Add("Avail");
                    tabWeapons.Columns["Avail"].DataType = typeof(AvailabilityValue);
                    tabWeapons.Columns.Add("Source");
                    tabWeapons.Columns["Source"].DataType = typeof(SourceString);
                    tabWeapons.Columns.Add("Cost");
                    tabWeapons.Columns["Cost"].DataType = typeof(NuyenString);

                    bool blnAnyRanged = false;
                    bool blnAnyMelee = false;
                    XmlNode xmlParentWeaponDataNode = ParentWeapon != null
                        ? _objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[id = "
                                                           + ParentWeapon.SourceIDString.CleanXPath() + ']')
                        : null;
                    foreach (XmlNode objXmlWeapon in objNodeList)
                    {
                        if (!objXmlWeapon.CreateNavigator().RequirementsMet(_objCharacter, ParentWeapon))
                            continue;

                        XmlNode xmlTestNode = objXmlWeapon.SelectSingleNode("forbidden/weapondetails");
                        if (xmlTestNode != null
                            && xmlParentWeaponDataNode.ProcessFilterOperationNode(xmlTestNode, false))
                        {
                            // Assumes topmost parent is an AND node
                            continue;
                        }

                        xmlTestNode = objXmlWeapon.SelectSingleNode("required/weapondetails");
                        if (xmlTestNode != null
                            && !xmlParentWeaponDataNode.ProcessFilterOperationNode(xmlTestNode, false))
                        {
                            // Assumes topmost parent is an AND node
                            continue;
                        }

                        if (objXmlWeapon["cyberware"]?.InnerText == bool.TrueString)
                            continue;
                        string strTest = objXmlWeapon["mount"]?.InnerText;
                        if (!string.IsNullOrEmpty(strTest) && !Mounts.Contains(strTest))
                            continue;
                        strTest = objXmlWeapon["extramount"]?.InnerText;
                        if (!string.IsNullOrEmpty(strTest) && !Mounts.Contains(strTest))
                            continue;
                        if (blnHideOverAvailLimit
                            && !SelectionShared.CheckAvailRestriction(objXmlWeapon, _objCharacter))
                            continue;
                        if (!blnFreeItem && blnShowOnlyAffordItems)
                        {
                            decimal decCostMultiplier = decBaseCostMultiplier;
                            if (_setBlackMarketMaps.Contains(objXmlWeapon["category"]?.InnerText))
                                decCostMultiplier *= 0.9m;
                            if (!SelectionShared.CheckNuyenRestriction(objXmlWeapon, _objCharacter.Nuyen,
                                                                       decCostMultiplier))
                                continue;
                        }

                        using (Weapon objWeapon = new Weapon(_objCharacter))
                        {
                            objWeapon.Create(objXmlWeapon, null, true, false, true);
                            objWeapon.Parent = ParentWeapon;
                            if (objWeapon.RangeType == "Ranged")
                                blnAnyRanged = true;
                            else
                                blnAnyMelee = true;
                            string strID = objWeapon.SourceIDString;
                            string strWeaponName = objWeapon.CurrentDisplayName;
                            string strDice = objWeapon.DicePool.ToString(GlobalSettings.CultureInfo);
                            string strAccuracy = objWeapon.DisplayAccuracy;
                            string strDamage = objWeapon.DisplayDamage;
                            string strAP = objWeapon.DisplayTotalAP;
                            if (strAP == "-")
                                strAP = "0";
                            string strRC = objWeapon.DisplayTotalRC;
                            string strAmmo = objWeapon.DisplayAmmo;
                            string strMode = objWeapon.DisplayMode;
                            string strReach = objWeapon.TotalReach.ToString(GlobalSettings.CultureInfo);
                            string strConceal = objWeapon.DisplayConcealability;
                            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                          out StringBuilder sbdAccessories))
                            {
                                foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                                {
                                    sbdAccessories.AppendLine(objAccessory.CurrentDisplayName);
                                }

                                if (sbdAccessories.Length > 0)
                                    sbdAccessories.Length -= Environment.NewLine.Length;
                                AvailabilityValue objAvail = objWeapon.TotalAvailTuple();
                                SourceString strSource = await SourceString.GetSourceStringAsync(objWeapon.Source,
                                    await objWeapon.DisplayPageAsync(GlobalSettings.Language),
                                    GlobalSettings.Language,
                                    GlobalSettings.CultureInfo,
                                    _objCharacter);
                                NuyenString strCost = new NuyenString(objWeapon.DisplayCost(out decimal _));

                                tabWeapons.Rows.Add(strID, strWeaponName, strDice, strAccuracy, strDamage, strAP, strRC,
                                                    strAmmo, strMode, strReach, strConceal, sbdAccessories.ToString(),
                                                    objAvail,
                                                    strSource, strCost);
                            }
                        }
                    }

                    DataSet set = new DataSet("weapons");
                    set.Tables.Add(tabWeapons);
                    if (blnAnyRanged)
                    {
                        await dgvWeapons.DoThreadSafeAsync(x =>
                        {
                            x.Columns[6].Visible = true;
                            x.Columns[7].Visible = true;
                            x.Columns[8].Visible = true;
                        });
                    }
                    else
                    {
                        await dgvWeapons.DoThreadSafeAsync(x =>
                        {
                            x.Columns[6].Visible = false;
                            x.Columns[7].Visible = false;
                            x.Columns[8].Visible = false;
                        });
                    }

                    await dgvWeapons.DoThreadSafeAsync(x =>
                    {
                        x.Columns[9].Visible = blnAnyMelee;
                        x.Columns[0].Visible = false;
                        x.Columns[13].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight;
                        x.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                        x.DataSource = set;
                        x.DataMember = "weapons";
                    });
                }
                else
                {
                    using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                                   out List<ListItem> lstWeapons))
                    {
                        int intOverLimit = 0;
                        XmlNode xmlParentWeaponDataNode = ParentWeapon != null
                            ? _objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[id = "
                                                               + ParentWeapon.SourceIDString.CleanXPath() + ']')
                            : null;
                        foreach (XmlNode objXmlWeapon in objNodeList)
                        {
                            if (!objXmlWeapon.CreateNavigator().RequirementsMet(_objCharacter, ParentWeapon))
                                continue;

                            XmlNode xmlTestNode = objXmlWeapon.SelectSingleNode("forbidden/weapondetails");
                            if (xmlTestNode != null
                                && xmlParentWeaponDataNode.ProcessFilterOperationNode(xmlTestNode, false))
                            {
                                // Assumes topmost parent is an AND node
                                continue;
                            }

                            xmlTestNode = objXmlWeapon.SelectSingleNode("required/weapondetails");
                            if (xmlTestNode != null
                                && !xmlParentWeaponDataNode.ProcessFilterOperationNode(xmlTestNode, false))
                            {
                                // Assumes topmost parent is an AND node
                                continue;
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

                            if (blnForCategories)
                                return true;
                            if (blnHideOverAvailLimit
                                && !SelectionShared.CheckAvailRestriction(objXmlWeapon, _objCharacter))
                            {
                                ++intOverLimit;
                                continue;
                            }

                            if (!blnFreeItem && blnShowOnlyAffordItems)
                            {
                                decimal decCostMultiplier = decBaseCostMultiplier;
                                if (_setBlackMarketMaps.Contains(objXmlWeapon["category"]?.InnerText))
                                    decCostMultiplier *= 0.9m;
                                if (!string.IsNullOrEmpty(ParentWeapon?.DoubledCostModificationSlots) &&
                                    (!string.IsNullOrEmpty(strMount) || !string.IsNullOrEmpty(strExtraMount)))
                                {
                                    string[] astrParentDoubledCostModificationSlots
                                        = ParentWeapon.DoubledCostModificationSlots.Split(
                                            '/', StringSplitOptions.RemoveEmptyEntries);
                                    if (astrParentDoubledCostModificationSlots.Contains(strMount)
                                        || astrParentDoubledCostModificationSlots.Contains(strExtraMount))
                                    {
                                        decCostMultiplier *= 2;
                                    }
                                }

                                if (!SelectionShared.CheckNuyenRestriction(
                                        objXmlWeapon, _objCharacter.Nuyen, decCostMultiplier))
                                {
                                    ++intOverLimit;
                                    continue;
                                }
                            }

                            lstWeapons.Add(new ListItem(objXmlWeapon["id"]?.InnerText,
                                                        objXmlWeapon["translate"]?.InnerText
                                                        ?? objXmlWeapon["name"]?.InnerText));
                        }

                        if (blnForCategories)
                            return false;
                        lstWeapons.Sort(CompareListItems.CompareNames);
                        if (intOverLimit > 0)
                        {
                            // Add after sort so that it's always at the end
                            lstWeapons.Add(new ListItem(string.Empty,
                                                        string.Format(GlobalSettings.CultureInfo,
                                                                      await LanguageManager.GetStringAsync(
                                                                          "String_RestrictedItemsHidden"),
                                                                      intOverLimit)));
                        }

                        string strOldSelected = await lstWeapon.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString());
                        _blnLoading = true;
                        await lstWeapon.PopulateWithListItemsAsync(lstWeapons);
                        _blnLoading = false;
                        await lstWeapon.DoThreadSafeAsync(x =>
                        {
                            if (!string.IsNullOrEmpty(strOldSelected))
                                x.SelectedValue = strOldSelected;
                            else
                                x.SelectedIndex = -1;
                        });
                    }
                }
            }
            finally
            {
                await this.DoThreadSafeAsync(x => x.ResumeLayout());
            }
            
            return true;
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

        private async void txtSearch_TextChanged(object sender, EventArgs e)
        {
            await RefreshList();
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            AcceptForm();
        }

        private async void chkFreeItem_CheckedChanged(object sender, EventArgs e)
        {
            if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked))
            {
                await RefreshList();
            }
            await UpdateWeaponInfo();
        }

        private async void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked) && !await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked))
            {
                await RefreshList();
            }
            await UpdateWeaponInfo();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down:
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

                        break;
                    }
                case Keys.Up:
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

                        break;
                    }
            }
        }

        private void txtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
                txtSearch.Select(txtSearch.Text.Length, 0);
        }

        private async void chkBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
        {
            await UpdateWeaponInfo();
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
            set
            {
                _setLimitToCategories.Clear();
                if (string.IsNullOrWhiteSpace(value))
                    return; // If passed an empty string, consume it and keep _strLimitToCategories as an empty hash.
                foreach (string strCategory in value.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries))
                    _setLimitToCategories.Add(strCategory);
            }
        }

        public Weapon ParentWeapon { get; set; }
        public HashSet<string> Mounts { get; } = Utils.StringHashSetPool.Get();

        #endregion Properties

        #region Methods

        private ValueTask<bool> RefreshList()
        {
            string strCategory = cboCategory.SelectedValue?.ToString();
            string strFilter = string.Empty;
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdFilter))
            {
                sbdFilter.Append('(').Append(_objCharacter.Settings.BookXPath()).Append(')');
                if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All"
                                                       && (GlobalSettings.SearchInCategoryOnly
                                                           || txtSearch.TextLength == 0))
                    sbdFilter.Append(" and category = ").Append(strCategory.CleanXPath());
                else
                {
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdCategoryFilter))
                    {
                        if (_setLimitToCategories?.Count > 0)
                        {
                            foreach (string strLoopCategory in _setLimitToCategories)
                            {
                                sbdCategoryFilter.Append("category = ").Append(strLoopCategory.CleanXPath())
                                                 .Append(" or ");
                            }

                            sbdCategoryFilter.Length -= 4;
                        }
                        else
                        {
                            sbdCategoryFilter.Append("category != \"Cyberware\" and category != \"Gear\"");
                        }

                        if (sbdCategoryFilter.Length > 0)
                        {
                            sbdFilter.Append(" and (").Append(sbdCategoryFilter).Append(')');
                        }
                    }
                }

                if (!string.IsNullOrEmpty(txtSearch.Text))
                    sbdFilter.Append(" and ").Append(CommonFunctions.GenerateSearchXPath(txtSearch.Text));

                if (sbdFilter.Length > 0)
                    strFilter = '[' + sbdFilter.ToString() + ']';
            }

            XmlNodeList objXmlWeaponList = _objXmlDocument.SelectNodes("/chummer/weapons/weapon" + strFilter);
            return BuildWeaponList(objXmlWeaponList);
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
                    string strSelectedId = lstWeapon.SelectedValue?.ToString();
                    if (!string.IsNullOrEmpty(strSelectedId))
                    {
                        objNode = _objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[id = " + strSelectedId.CleanXPath() + ']');
                        if (objNode != null)
                        {
                            _strSelectCategory = (GlobalSettings.SearchInCategoryOnly || txtSearch.TextLength == 0)
                                ? cboCategory.SelectedValue?.ToString()
                                : objNode["category"]?.InnerText;
                            _strSelectedWeapon = objNode["id"]?.InnerText;
                            _decMarkup = nudMarkup.Value;
                            _blnBlackMarketDiscount = chkBlackMarketDiscount.Checked;

                            DialogResult = DialogResult.OK;
                        }
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
                            objNode = _objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[id = " + strWeapon.CleanXPath() + ']');
                        }
                        else
                        {
                            objNode = _objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[id = " + dgvWeapons.SelectedRows[0].Cells[0].Value.ToString().CleanXPath() + ']');
                        }
                        if (objNode != null)
                        {
                            _strSelectCategory = (GlobalSettings.SearchInCategoryOnly || txtSearch.TextLength == 0) ? cboCategory.SelectedValue?.ToString() : objNode["category"]?.InnerText;
                            _strSelectedWeapon = objNode["id"]?.InnerText;
                        }
                        _decMarkup = nudMarkup.Value;

                        DialogResult = DialogResult.OK;
                    }
                    break;
            }
        }

        private async void OpenSourceFromLabel(object sender, EventArgs e)
        {
            await CommonFunctions.OpenPdfFromControl(sender);
        }

        private async void tmrSearch_Tick(object sender, EventArgs e)
        {
            tmrSearch.Stop();
            tmrSearch.Enabled = false;

            await RefreshList();
        }

        #endregion Methods
    }
}
