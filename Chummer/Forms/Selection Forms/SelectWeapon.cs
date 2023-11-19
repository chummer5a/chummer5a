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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Equipment;

// ReSharper disable LocalizableElement

namespace Chummer
{
    public partial class SelectWeapon : Form
    {
        private string _strSelectedWeapon = string.Empty;
        private decimal _decMarkup;

        private int _intLoading = 1;
        private bool _blnAddAgain;
        private bool _blnBlackMarketDiscount;
        private HashSet<string> _setLimitToCategories = Utils.StringHashSetPool.Get();
        private static string _strSelectCategory = string.Empty;
        private readonly Character _objCharacter;
        private readonly XmlDocument _objXmlDocument;
        private Weapon _objSelectedWeapon;

        private List<ListItem> _lstCategory = Utils.ListItemListPool.Get();
        private HashSet<string> _setBlackMarketMaps = Utils.StringHashSetPool.Get();
        private HashSet<string> _setMounts = Utils.StringHashSetPool.Get();

        private CancellationTokenSource _objUpdateWeaponInfoCancellationTokenSource;
        private CancellationTokenSource _objDoRefreshListCancellationTokenSource;
        private CancellationTokenSource _objWeaponSelectedIndexChangedCancellationTokenSource;
        private readonly CancellationTokenSource _objGenericCancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _objGenericToken;

        #region Control Events

        public SelectWeapon(Character objCharacter)
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            _objGenericToken = _objGenericCancellationTokenSource.Token;
            Disposed += (sender, args) =>
            {
                CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objUpdateWeaponInfoCancellationTokenSource, null);
                if (objOldCancellationTokenSource?.IsCancellationRequested == false)
                {
                    objOldCancellationTokenSource.Cancel(false);
                    objOldCancellationTokenSource.Dispose();
                }
                objOldCancellationTokenSource = Interlocked.Exchange(ref _objDoRefreshListCancellationTokenSource, null);
                if (objOldCancellationTokenSource?.IsCancellationRequested == false)
                {
                    objOldCancellationTokenSource.Cancel(false);
                    objOldCancellationTokenSource.Dispose();
                }
                objOldCancellationTokenSource = Interlocked.Exchange(ref _objWeaponSelectedIndexChangedCancellationTokenSource, null);
                if (objOldCancellationTokenSource?.IsCancellationRequested == false)
                {
                    objOldCancellationTokenSource.Cancel(false);
                    objOldCancellationTokenSource.Dispose();
                }
                _objGenericCancellationTokenSource.Dispose();
                Utils.ListItemListPool.Return(ref _lstCategory);
                Utils.StringHashSetPool.Return(ref _setBlackMarketMaps);
                Utils.StringHashSetPool.Return(ref _setLimitToCategories);
                Utils.StringHashSetPool.Return(ref _setMounts);
            };
            InitializeComponent();
            tabControl.MouseWheel += CommonFunctions.ShiftTabsOnMouseScroll;
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            // Load the Weapon information.
            _objXmlDocument = _objCharacter.LoadData("weapons.xml");
            _setBlackMarketMaps.AddRange(_objCharacter.GenerateBlackMarketMappings(_objCharacter.LoadDataXPath("weapons.xml").SelectSingleNodeAndCacheExpression("/chummer")));

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
                chkHideOverAvailLimit.Text = string.Format(
                    GlobalSettings.CultureInfo, chkHideOverAvailLimit.Text,
                    _objCharacter.Settings.MaximumAvailability);
                chkHideOverAvailLimit.Checked = GlobalSettings.HideItemsOverAvailLimit;
            }
        }

        private async void SelectWeapon_Load(object sender, EventArgs e)
        {
            try
            {
                DataGridViewCellStyle dataGridViewNuyenCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.TopRight,
                    Format = _objCharacter.Settings.NuyenFormat + await LanguageManager.GetStringAsync("String_NuyenSymbol", token: _objGenericToken).ConfigureAwait(false),
                    NullValue = null
                };
                dgvc_Cost.DefaultCellStyle = dataGridViewNuyenCellStyle;

                // Populate the Weapon Category list.
                // Populate the Category list.
                string strFilterPrefix = "/chummer/weapons/weapon[(" + await _objCharacter.Settings.BookXPathAsync(token: _objGenericToken).ConfigureAwait(false) + ") and category = ";
                using (XmlNodeList xmlCategoryList = _objXmlDocument.SelectNodes("/chummer/categories/category"))
                {
                    if (xmlCategoryList != null)
                    {
                        foreach (XmlNode objXmlCategory in xmlCategoryList)
                        {
                            string strInnerText = objXmlCategory.InnerText;
                            if ((_setLimitToCategories.Count == 0 || _setLimitToCategories.Contains(strInnerText))
                                && await BuildWeaponList(_objXmlDocument.SelectNodes(strFilterPrefix + strInnerText.CleanXPath() + ']'), true, _objGenericToken).ConfigureAwait(false))
                                _lstCategory.Add(new ListItem(strInnerText, objXmlCategory.Attributes?["translate"]?.InnerText ?? strInnerText));
                        }
                    }
                }

                _lstCategory.Sort(CompareListItems.CompareNames);

                _lstCategory.Insert(0, new ListItem("Show All", await LanguageManager.GetStringAsync("String_ShowAll", token: _objGenericToken).ConfigureAwait(false)));

                await cboCategory.PopulateWithListItemsAsync(_lstCategory, _objGenericToken).ConfigureAwait(false);
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
                }, _objGenericToken).ConfigureAwait(false);

                await chkBlackMarketDiscount.DoThreadSafeAsync(x => x.Visible = _objCharacter.BlackMarketDiscount, _objGenericToken).ConfigureAwait(false);

                Interlocked.Decrement(ref _intLoading);
                await RefreshList(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private void SelectWeapon_Closing(object sender, FormClosingEventArgs e)
        {
            CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objUpdateWeaponInfoCancellationTokenSource, null);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            objOldCancellationTokenSource = Interlocked.Exchange(ref _objDoRefreshListCancellationTokenSource, null);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            objOldCancellationTokenSource = Interlocked.Exchange(ref _objWeaponSelectedIndexChangedCancellationTokenSource, null);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            _objGenericCancellationTokenSource.Cancel(false);
        }

        private async void RefreshCurrentList(object sender, EventArgs e)
        {
            try
            {
                await RefreshList(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void lstWeapon_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_intLoading > 0)
                return;

            try
            {
                CancellationTokenSource objNewCancellationTokenSource = new CancellationTokenSource();
                CancellationToken objNewToken = objNewCancellationTokenSource.Token;
                CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objWeaponSelectedIndexChangedCancellationTokenSource, objNewCancellationTokenSource);
                if (objOldCancellationTokenSource?.IsCancellationRequested == false)
                {
                    objOldCancellationTokenSource.Cancel(false);
                    objOldCancellationTokenSource.Dispose();
                }
                using (CancellationTokenSource objJoinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_objGenericToken, objNewToken))
                {
                    CancellationToken token = objJoinedCancellationTokenSource.Token;
                    // Retireve the information for the selected Weapon.
                    XmlNode xmlWeapon = null;
                    string strSelectedId = await lstWeapon.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(strSelectedId))
                        xmlWeapon = _objXmlDocument.TryGetNodeByNameOrId("/chummer/weapons/weapon", strSelectedId);
                    if (xmlWeapon != null)
                    {
                        Weapon objWeapon = new Weapon(_objCharacter);
                        try
                        {
                            objWeapon.Create(xmlWeapon, null, true, false, true);
                            objWeapon.Parent = ParentWeapon;
                            Weapon objOldWeapon = Interlocked.Exchange(ref _objSelectedWeapon, objWeapon);
                            if (objOldWeapon != null)
                                await objOldWeapon.DisposeAsync().ConfigureAwait(false);
                        }
                        catch
                        {
                            Interlocked.CompareExchange(ref _objSelectedWeapon, null, objWeapon);
                            await objWeapon.DisposeAsync().ConfigureAwait(false);
                            throw;
                        }
                    }
                    else
                    {
                        Weapon objOldWeapon = Interlocked.Exchange(ref _objSelectedWeapon, null);
                        if (objOldWeapon != null)
                            await objOldWeapon.DisposeAsync().ConfigureAwait(false);
                    }

                    try
                    {
                        await UpdateWeaponInfo(token).ConfigureAwait(false);
                    }
                    catch
                    {
                        Weapon objOldWeapon = Interlocked.Exchange(ref _objSelectedWeapon, null);
                        if (objOldWeapon != null)
                            await objOldWeapon.DisposeAsync().ConfigureAwait(false);
                        throw;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async Task UpdateWeaponInfo(CancellationToken token = default)
        {
            if (_intLoading > 0)
                return;
            CancellationTokenSource objNewCancellationTokenSource = new CancellationTokenSource();
            CancellationToken objNewToken = objNewCancellationTokenSource.Token;
            CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objUpdateWeaponInfoCancellationTokenSource, objNewCancellationTokenSource);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            using (CancellationTokenSource objJoinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, objNewToken))
            {
                token = objJoinedCancellationTokenSource.Token;
                await this.DoThreadSafeAsync(x => x.SuspendLayout(), token: token).ConfigureAwait(false);
                try
                {
                    Weapon objSelectedWeapon = _objSelectedWeapon;
                    if (objSelectedWeapon != null)
                    {
                        bool blnCanBlackMarketDiscount = _setBlackMarketMaps.Contains(objSelectedWeapon.Category);
                        objSelectedWeapon.DiscountCost = await chkBlackMarketDiscount.DoThreadSafeFuncAsync(x =>
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

                            return x.Checked;
                        }, token: token).ConfigureAwait(false);

                        string strReach = (await objSelectedWeapon.GetTotalReachAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.CultureInfo);
                        await lblWeaponReach.DoThreadSafeAsync(x => x.Text = strReach, token: token)
                                            .ConfigureAwait(false);
                        await lblWeaponReachLabel
                                .DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strReach), token: token)
                                .ConfigureAwait(false);
                        string strDamage = objSelectedWeapon.DisplayDamage;
                        await lblWeaponDamage.DoThreadSafeAsync(x => x.Text = strDamage, token: token)
                                                .ConfigureAwait(false);
                        await lblWeaponDamageLabel
                                .DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strDamage), token: token)
                                .ConfigureAwait(false);
                        string strAP = objSelectedWeapon.DisplayTotalAP;
                        await lblWeaponAP.DoThreadSafeAsync(x => x.Text = strAP, token: token).ConfigureAwait(false);
                        await lblWeaponAPLabel
                                .DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strAP), token: token)
                                .ConfigureAwait(false);
                        string strMode = objSelectedWeapon.DisplayMode;
                        await lblWeaponMode.DoThreadSafeAsync(x => x.Text = strMode, token: token)
                                            .ConfigureAwait(false);
                        await lblWeaponModeLabel
                                .DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strMode), token: token)
                                .ConfigureAwait(false);
                        string strRC = objSelectedWeapon.DisplayTotalRC;
                        await lblWeaponRC.DoThreadSafeAsync(x => x.Text = strRC, token: token).ConfigureAwait(false);
                        await lblWeaponRC.SetToolTipAsync(objSelectedWeapon.RCToolTip, token: token)
                                            .ConfigureAwait(false);
                        await lblWeaponRCLabel
                                .DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strRC), token: token)
                                .ConfigureAwait(false);
                        string strAmmo = objSelectedWeapon.DisplayAmmo;
                        await lblWeaponAmmo.DoThreadSafeAsync(x => x.Text = strAmmo, token: token)
                                            .ConfigureAwait(false);
                        await lblWeaponAmmoLabel
                                .DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strAmmo), token: token)
                                .ConfigureAwait(false);
                        string strAccuracy = objSelectedWeapon.DisplayAccuracy;
                        await lblWeaponAccuracy.DoThreadSafeAsync(x => x.Text = strAccuracy, token: token)
                                                .ConfigureAwait(false);
                        await lblWeaponAccuracyLabel
                                .DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strAccuracy), token: token)
                                .ConfigureAwait(false);
                        string strConceal = objSelectedWeapon.DisplayConcealability;
                        await lblWeaponConceal.DoThreadSafeAsync(x => x.Text = strConceal, token: token)
                                                .ConfigureAwait(false);
                        await lblWeaponConcealLabel
                                .DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strConceal), token: token)
                                .ConfigureAwait(false);

                        decimal decItemCost = 0;
                        string strWeaponCost;
                        if (await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                        {
                            strWeaponCost
                                = 0.0m.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo)
                                    + await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token)
                                                            .ConfigureAwait(false);
                        }
                        else
                        {
                            strWeaponCost = objSelectedWeapon.DisplayCost(
                                out decItemCost,
                                await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false)
                                / 100.0m);
                        }

                        await lblWeaponCost.DoThreadSafeAsync(x => x.Text = strWeaponCost, token: token)
                                            .ConfigureAwait(false);
                        await lblWeaponCostLabel
                                .DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strWeaponCost), token: token)
                                .ConfigureAwait(false);

                        AvailabilityValue objTotalAvail = await objSelectedWeapon.TotalAvailTupleAsync(token: token).ConfigureAwait(false);
                        string strAvail = await objTotalAvail.ToStringAsync(token).ConfigureAwait(false);
                        await lblWeaponAvail.DoThreadSafeAsync(x => x.Text = strAvail, token: token)
                                            .ConfigureAwait(false);
                        await lblWeaponAvailLabel
                                .DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strAvail), token: token)
                                .ConfigureAwait(false);
                        string strTest = await _objCharacter.AvailTestAsync(decItemCost, objTotalAvail, token)
                                                            .ConfigureAwait(false);
                        await lblTest.DoThreadSafeAsync(x => x.Text = strTest, token: token).ConfigureAwait(false);
                        await lblTestLabel
                                .DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strTest), token: token)
                                .ConfigureAwait(false);
                        await objSelectedWeapon.SetSourceDetailAsync(lblSource, token: token).ConfigureAwait(false);
                        bool blnShowSource = !string.IsNullOrEmpty(
                            await lblSource.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false));
                        await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = blnShowSource, token: token)
                                            .ConfigureAwait(false);

                        string strIncludedAccessories;
                        // Build a list of included Accessories and Modifications that come with the weapon.
                        using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                        out StringBuilder sbdAccessories))
                        {
                            foreach (WeaponAccessory objAccessory in objSelectedWeapon.WeaponAccessories)
                            {
                                sbdAccessories.AppendLine(
                                    await objAccessory.GetCurrentDisplayNameAsync(token).ConfigureAwait(false));
                            }

                            if (sbdAccessories.Length > 0)
                                sbdAccessories.Length -= Environment.NewLine.Length;

                            strIncludedAccessories = sbdAccessories.Length == 0
                                ? await LanguageManager.GetStringAsync("String_None", token: token)
                                                        .ConfigureAwait(false)
                                : sbdAccessories.ToString();
                        }

                        await lblIncludedAccessories
                                .DoThreadSafeAsync(x => x.Text = strIncludedAccessories, token: token)
                                .ConfigureAwait(false);
                        await tlpRight.DoThreadSafeAsync(x => x.Visible = true, token: token).ConfigureAwait(false);
                        await gpbIncludedAccessories
                                .DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strIncludedAccessories),
                                                    token: token).ConfigureAwait(false);
                    }
                    else
                    {
                        await chkBlackMarketDiscount.DoThreadSafeAsync(x => x.Checked = false, token: token)
                                                    .ConfigureAwait(false);
                        await tlpRight.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
                        await gpbIncludedAccessories.DoThreadSafeAsync(x => x.Visible = false, token: token)
                                                    .ConfigureAwait(false);
                    }
                }
                finally
                {
                    await this.DoThreadSafeAsync(x => x.ResumeLayout(), _objGenericToken).ConfigureAwait(false);
                }
            }
        }

        private async Task<bool> BuildWeaponList(XmlNodeList objNodeList, bool blnForCategories = false, CancellationToken token = default)
        {
            await this.DoThreadSafeAsync(x => x.SuspendLayout(), token: token).ConfigureAwait(false);
            try
            {
                bool blnHideOverAvailLimit = await chkHideOverAvailLimit.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false);
                bool blnShowOnlyAffordItems = await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false);
                bool blnFreeItem = await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false);
                decimal decBaseCostMultiplier = 1 + await nudMarkup.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false) / 100.0m;
                if (await tabControl.DoThreadSafeFuncAsync(x => x.SelectedIndex, token: token).ConfigureAwait(false) == 1 && !blnForCategories)
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
                        ? _objXmlDocument.TryGetNodeById("/chummer/weapons/weapon", ParentWeapon.SourceID)
                        : null;
                    foreach (XmlNode objXmlWeapon in objNodeList)
                    {
                        if (!await objXmlWeapon.CreateNavigator().RequirementsMetAsync(_objCharacter, ParentWeapon, token: token).ConfigureAwait(false))
                            continue;

                        XPathNavigator xmlTestNode = objXmlWeapon.SelectSingleNodeAndCacheExpressionAsNavigator("forbidden/weapondetails", token);
                        if (xmlTestNode != null
                            && await xmlParentWeaponDataNode.ProcessFilterOperationNodeAsync(xmlTestNode, false, token).ConfigureAwait(false))
                        {
                            // Assumes topmost parent is an AND node
                            continue;
                        }

                        xmlTestNode = objXmlWeapon.SelectSingleNodeAndCacheExpressionAsNavigator("required/weapondetails", token);
                        if (xmlTestNode != null
                            && !await xmlParentWeaponDataNode.ProcessFilterOperationNodeAsync(xmlTestNode, false, token).ConfigureAwait(false))
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
                            && !await SelectionShared.CheckAvailRestrictionAsync(objXmlWeapon, _objCharacter, token: token).ConfigureAwait(false))
                            continue;
                        if (!blnFreeItem && blnShowOnlyAffordItems)
                        {
                            decimal decCostMultiplier = decBaseCostMultiplier;
                            if (_setBlackMarketMaps.Contains(objXmlWeapon["category"]?.InnerText))
                                decCostMultiplier *= 0.9m;
                            if (!await SelectionShared.CheckNuyenRestrictionAsync(objXmlWeapon, _objCharacter.Nuyen,
                                    decCostMultiplier, token: token).ConfigureAwait(false))
                                continue;
                        }

                        Weapon objWeapon = new Weapon(_objCharacter);
                        try
                        {
                            objWeapon.Create(objXmlWeapon, null, true, false, true);
                            objWeapon.Parent = ParentWeapon;
                            if (objWeapon.RangeType == "Ranged")
                                blnAnyRanged = true;
                            else
                                blnAnyMelee = true;
                            string strID = objWeapon.SourceIDString;
                            string strWeaponName = await objWeapon.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                            string strDice = (await objWeapon.GetDicePoolAsync(token: token).ConfigureAwait(false)).ToString(GlobalSettings.CultureInfo);
                            string strAccuracy = objWeapon.DisplayAccuracy;
                            string strDamage = objWeapon.DisplayDamage;
                            string strAP = objWeapon.DisplayTotalAP;
                            if (strAP == "-")
                                strAP = "0";
                            string strRC = objWeapon.DisplayTotalRC;
                            string strAmmo = objWeapon.DisplayAmmo;
                            string strMode = objWeapon.DisplayMode;
                            string strReach = (await objWeapon.GetTotalReachAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.CultureInfo);
                            string strConceal = objWeapon.DisplayConcealability;
                            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                          out StringBuilder sbdAccessories))
                            {
                                foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                                {
                                    sbdAccessories.AppendLine(await objAccessory.GetCurrentDisplayNameAsync(token).ConfigureAwait(false));
                                }

                                if (sbdAccessories.Length > 0)
                                    sbdAccessories.Length -= Environment.NewLine.Length;
                                AvailabilityValue objAvail = await objWeapon.TotalAvailTupleAsync(token: token).ConfigureAwait(false);
                                SourceString strSource = await SourceString.GetSourceStringAsync(objWeapon.Source,
                                    await objWeapon.DisplayPageAsync(GlobalSettings.Language, token).ConfigureAwait(false),
                                    GlobalSettings.Language,
                                    GlobalSettings.CultureInfo,
                                    _objCharacter, token).ConfigureAwait(false);
                                NuyenString strCost = new NuyenString(objWeapon.DisplayCost(out decimal _));

                                tabWeapons.Rows.Add(strID, strWeaponName, strDice, strAccuracy, strDamage, strAP, strRC,
                                                    strAmmo, strMode, strReach, strConceal, sbdAccessories.ToString(),
                                                    objAvail,
                                                    strSource, strCost);
                            }
                        }
                        finally
                        {
                            await objWeapon.DisposeAsync().ConfigureAwait(false);
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
                        }, token: token).ConfigureAwait(false);
                    }
                    else
                    {
                        await dgvWeapons.DoThreadSafeAsync(x =>
                        {
                            x.Columns[6].Visible = false;
                            x.Columns[7].Visible = false;
                            x.Columns[8].Visible = false;
                        }, token: token).ConfigureAwait(false);
                    }

                    await dgvWeapons.DoThreadSafeAsync(x =>
                    {
                        x.Columns[9].Visible = blnAnyMelee;
                        x.Columns[0].Visible = false;
                        x.Columns[13].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight;
                        x.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                        x.DataSource = set;
                        x.DataMember = "weapons";
                    }, token: token).ConfigureAwait(false);
                }
                else
                {
                    using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                                   out List<ListItem> lstWeapons))
                    {
                        int intOverLimit = 0;
                        XmlNode xmlParentWeaponDataNode = ParentWeapon != null
                            ? _objXmlDocument.TryGetNodeById("/chummer/weapons/weapon", ParentWeapon.SourceID)
                            : null;
                        foreach (XmlNode objXmlWeapon in objNodeList)
                        {
                            if (!await objXmlWeapon.CreateNavigator().RequirementsMetAsync(_objCharacter, ParentWeapon, token: token).ConfigureAwait(false))
                                continue;

                            XPathNavigator xmlTestNode = objXmlWeapon.SelectSingleNodeAndCacheExpressionAsNavigator("forbidden/weapondetails", token);
                            if (xmlTestNode != null
                                && await xmlParentWeaponDataNode.ProcessFilterOperationNodeAsync(xmlTestNode, false, token).ConfigureAwait(false))
                            {
                                // Assumes topmost parent is an AND node
                                continue;
                            }

                            xmlTestNode = objXmlWeapon.SelectSingleNodeAndCacheExpressionAsNavigator("required/weapondetails", token);
                            if (xmlTestNode != null
                                && !await xmlParentWeaponDataNode.ProcessFilterOperationNodeAsync(xmlTestNode, false, token).ConfigureAwait(false))
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
                                && !await SelectionShared.CheckAvailRestrictionAsync(objXmlWeapon, _objCharacter, token: token).ConfigureAwait(false))
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

                                if (!await SelectionShared.CheckNuyenRestrictionAsync(
                                        objXmlWeapon, _objCharacter.Nuyen, decCostMultiplier, token: token).ConfigureAwait(false))
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
                                                                          "String_RestrictedItemsHidden", token: token).ConfigureAwait(false),
                                                                      intOverLimit)));
                        }

                        string strOldSelected = await lstWeapon.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
                        Interlocked.Increment(ref _intLoading);
                        try
                        {
                            await lstWeapon.PopulateWithListItemsAsync(lstWeapons, token: token).ConfigureAwait(false);
                        }
                        finally
                        {
                            Interlocked.Decrement(ref _intLoading);
                        }
                        await lstWeapon.DoThreadSafeAsync(x =>
                        {
                            if (!string.IsNullOrEmpty(strOldSelected))
                                x.SelectedValue = strOldSelected;
                            else
                                x.SelectedIndex = -1;
                        }, token: token).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                await this.DoThreadSafeAsync(x => x.ResumeLayout(), _objGenericToken).ConfigureAwait(false);
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
            Close();
        }

        private async void txtSearch_TextChanged(object sender, EventArgs e)
        {
            try
            {
                await RefreshList(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            AcceptForm();
        }

        private async void chkFreeItem_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked, _objGenericToken).ConfigureAwait(false))
                {
                    await RefreshList(_objGenericToken).ConfigureAwait(false);
                }
                await UpdateWeaponInfo(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (await chkShowOnlyAffordItems.DoThreadSafeFuncAsync(x => x.Checked, _objGenericToken).ConfigureAwait(false)
                    && !await chkFreeItem.DoThreadSafeFuncAsync(x => x.Checked, _objGenericToken).ConfigureAwait(false))
                {
                    await RefreshList(_objGenericToken).ConfigureAwait(false);
                }
                await UpdateWeaponInfo(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
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
                txtSearch.Select(txtSearch.TextLength, 0);
        }

        private async void chkBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                await UpdateWeaponInfo(_objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
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

        public HashSet<string> Mounts => _setMounts;

        #endregion Properties

        #region Methods

        private async Task<bool> RefreshList(CancellationToken token = default)
        {
            CancellationTokenSource objNewCancellationTokenSource = new CancellationTokenSource();
            CancellationToken objNewToken = objNewCancellationTokenSource.Token;
            CancellationTokenSource objOldCancellationTokenSource = Interlocked.Exchange(ref _objDoRefreshListCancellationTokenSource, objNewCancellationTokenSource);
            if (objOldCancellationTokenSource?.IsCancellationRequested == false)
            {
                objOldCancellationTokenSource.Cancel(false);
                objOldCancellationTokenSource.Dispose();
            }
            using (CancellationTokenSource objJoinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, objNewToken))
            {
                token = objJoinedCancellationTokenSource.Token;
                string strCategory = await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
                string strFilter = string.Empty;
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdFilter))
                {
                    sbdFilter.Append('(').Append(await _objCharacter.Settings.BookXPathAsync(token: token).ConfigureAwait(false)).Append(')');
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
                return await BuildWeaponList(objXmlWeaponList, token: token).ConfigureAwait(false);
            }
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
                        objNode = _objXmlDocument.TryGetNodeByNameOrId("/chummer/weapons/weapon", strSelectedId);
                        if (objNode != null)
                        {
                            _strSelectCategory = GlobalSettings.SearchInCategoryOnly || txtSearch.TextLength == 0
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
                        if (txtSearch.TextLength > 1)
                        {
                            string strWeapon = dgvWeapons.SelectedRows[0].Cells[0].Value.ToString();
                            if (!string.IsNullOrEmpty(strWeapon))
                                strWeapon = strWeapon.Substring(0, strWeapon.LastIndexOf('(') - 1);
                            objNode = _objXmlDocument.TryGetNodeByNameOrId("/chummer/weapons/weapon", strWeapon);
                        }
                        else
                        {
                            objNode = _objXmlDocument.TryGetNodeByNameOrId("/chummer/weapons/weapon", dgvWeapons.SelectedRows[0].Cells[0].Value.ToString());
                        }
                        if (objNode != null)
                        {
                            _strSelectCategory = GlobalSettings.SearchInCategoryOnly || txtSearch.TextLength == 0 ? cboCategory.SelectedValue?.ToString() : objNode["category"]?.InnerText;
                            _strSelectedWeapon = objNode["id"]?.InnerText;
                        }
                        _decMarkup = nudMarkup.Value;

                        DialogResult = DialogResult.OK;
                    }
                    break;
            }
            Close();
        }

        private async void OpenSourceFromLabel(object sender, EventArgs e)
        {
            try
            {
                await CommonFunctions.OpenPdfFromControl(sender, _objGenericToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        #endregion Methods
    }
}
