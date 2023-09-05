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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Chummer.Backend.Equipment;
using NLog;
using Timer = System.Windows.Forms.Timer;

namespace Chummer
{
    public partial class SelectLifestyle : Form
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
        private bool _blnAddAgain;
        private readonly Lifestyle _objLifestyle;
        private Lifestyle _objSourceLifestyle;
        private readonly Character _objCharacter;

        private readonly XmlDocument _objXmlDocument;

        private int _intSkipRefresh = 1;
        private int _intUpdatingCity = 1;
        private int _intUpdatingDistrict = 1;
        private int _intUpdatingBorough = 1;
        private readonly Timer _tmrCityChangeTimer;
        private readonly Timer _tmrDistrictChangeTimer;
        private readonly Timer _tmrBoroughChangeTimer;

        #region Control Events

        public SelectLifestyle(Character objCharacter)
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objCharacter = objCharacter;
            _objLifestyle = new Lifestyle(objCharacter);
            // Load the Lifestyles information.
            _objXmlDocument = objCharacter.LoadData("lifestyles.xml");
            _tmrCityChangeTimer = new Timer { Interval = 1000 };
            _tmrDistrictChangeTimer = new Timer { Interval = 1000 };
            _tmrBoroughChangeTimer = new Timer { Interval = 1000 };
            Disposed += (o, args) =>
            {
                _tmrCityChangeTimer?.Dispose();
                _tmrDistrictChangeTimer?.Dispose();
                _tmrBoroughChangeTimer?.Dispose();
            };
            _tmrCityChangeTimer.Tick += CityChangeTimer_Tick;
            _tmrCityChangeTimer.Tick += DistrictChangeTimer_Tick;
            _tmrCityChangeTimer.Tick += BoroughChangeTimer_Tick;
        }

        private async void SelectLifestyle_Load(object sender, EventArgs e)
        {
            string strSelectedId = string.Empty;
            // Populate the Lifestyle ComboBoxes.
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                           out List<ListItem> lstLifestyle))
            {
                using (XmlNodeList xmlLifestyleList
                       = _objXmlDocument.SelectNodes("/chummer/lifestyles/lifestyle["
                                                     + await _objCharacter.Settings.BookXPathAsync().ConfigureAwait(false) + ']'))
                {
                    if (xmlLifestyleList?.Count > 0)
                    {
                        foreach (XmlNode objXmlLifestyle in xmlLifestyleList)
                        {
                            string strLifeStyleId = objXmlLifestyle["id"]?.InnerText;
                            if (!string.IsNullOrEmpty(strLifeStyleId) && !strLifeStyleId.IsEmptyGuid())
                            {
                                string strName = objXmlLifestyle["name"]?.InnerText
                                                 ?? await LanguageManager.GetStringAsync("String_Unknown").ConfigureAwait(false);
                                if (strName == _objSourceLifestyle?.BaseLifestyle)
                                    strSelectedId = strLifeStyleId;
                                lstLifestyle.Add(new ListItem(strLifeStyleId,
                                                              objXmlLifestyle["translate"]?.InnerText ?? strName));
                            }
                        }
                    }
                }

                await cboLifestyle.PopulateWithListItemsAsync(lstLifestyle).ConfigureAwait(false);
                await cboLifestyle.DoThreadSafeAsync(x =>
                {
                    if (!string.IsNullOrEmpty(strSelectedId))
                        x.SelectedValue = strSelectedId;
                    if (x.SelectedIndex == -1)
                        x.SelectedIndex = 0;
                }).ConfigureAwait(false);
            }

            // Populate the City ComboBox
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                           out List<ListItem> lstCity))
            {
                using (XmlNodeList xmlCityList = _objXmlDocument.SelectNodes("/chummer/cities/city"))
                {
                    if (xmlCityList?.Count > 0)
                    {
                        foreach (XmlNode objXmlCity in xmlCityList)
                        {
                            string strName = objXmlCity["name"]?.InnerText
                                             ?? await LanguageManager.GetStringAsync("String_Unknown").ConfigureAwait(false);
                            lstCity.Add(new ListItem(strName, objXmlCity["translate"]?.InnerText ?? strName));
                        }
                    }
                }

                lstCity.Sort();
                await cboCity.PopulateWithListItemsAsync(lstCity).ConfigureAwait(false);
            }

            //Populate District and Borough ComboBox for the first time
            await RefreshDistrictList().ConfigureAwait(false);
            await RefreshBoroughList().ConfigureAwait(false);

            string strSpace = await LanguageManager.GetStringAsync("String_Space").ConfigureAwait(false);
            // Fill the Options list.
            using (XmlNodeList xmlLifestyleOptionsList = _objXmlDocument.SelectNodes("/chummer/qualities/quality[(source = \"SR5\" or category = \"Contracts\") and (" + await _objCharacter.Settings.BookXPathAsync().ConfigureAwait(false) + ")]"))
            {
                if (xmlLifestyleOptionsList?.Count > 0)
                {
                    foreach (XmlNode objXmlOption in xmlLifestyleOptionsList)
                    {
                        string strOptionName = objXmlOption["name"]?.InnerText;
                        if (string.IsNullOrEmpty(strOptionName))
                            continue;
                        XmlNode nodMultiplier = objXmlOption["multiplier"];
                        string strBaseString = string.Empty;
                        if (nodMultiplier == null)
                        {
                            nodMultiplier = objXmlOption["multiplierbaseonly"];
                            strBaseString = strSpace + await LanguageManager.GetStringAsync("Label_Base").ConfigureAwait(false);
                        }
                        TreeNode nodOption = new TreeNode
                        {
                            Tag = objXmlOption["id"]?.InnerText
                        };
                        if (nodMultiplier != null && int.TryParse(nodMultiplier.InnerText, out int intCost))
                        {
                            nodOption.Text = (objXmlOption["translate"]?.InnerText ?? strOptionName)
                                             + strSpace
                                             + (intCost > 0 ? "[+" : "[")
                                             + intCost.ToString(GlobalSettings.CultureInfo)
                                             + strBaseString + "%]";
                        }
                        else
                        {
                            string strCost = objXmlOption["cost"]?.InnerText;
                            (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strCost).ConfigureAwait(false);
                            decimal decCost = blnIsSuccess ? Convert.ToDecimal((double)objProcess) : 0;
                            nodOption.Text = (objXmlOption["translate"]?.InnerText ?? strOptionName)
                                             + strSpace
                                             + '['
                                             + decCost.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo)
                                             + await LanguageManager.GetStringAsync("String_NuyenSymbol").ConfigureAwait(false) + ']';
                        }
                        await treQualities.DoThreadSafeAsync(x => x.Nodes.Add(nodOption)).ConfigureAwait(false);
                    }
                }
            }

            await SortTree(treQualities).ConfigureAwait(false);

            if (_objSourceLifestyle != null)
            {
                await txtLifestyleName.DoThreadSafeAsync(x => x.Text = _objSourceLifestyle.Name).ConfigureAwait(false);
                await nudRoommates.DoThreadSafeAsync(x => x.Value = _objSourceLifestyle.Roommates).ConfigureAwait(false);
                await nudPercentage.DoThreadSafeAsync(x => x.Value = _objSourceLifestyle.Percentage).ConfigureAwait(false);
                await treQualities.DoThreadSafeAsync(x =>
                {
                    foreach (LifestyleQuality objQuality in _objSourceLifestyle.LifestyleQualities)
                    {
                        TreeNode objNode = x.FindNode(objQuality.SourceIDString);
                        if (objNode != null)
                            objNode.Checked = true;
                    }
                }).ConfigureAwait(false);
                await chkPrimaryTenant.DoThreadSafeAsync(x => x.Checked = _objSourceLifestyle.PrimaryTenant).ConfigureAwait(false);
                await chkTrustFund.DoThreadSafeAsync(x => x.Checked = _objSourceLifestyle.TrustFund).ConfigureAwait(false);
                string strCity = await _objSourceLifestyle.GetCityAsync().ConfigureAwait(false);
                string strDistrict = await _objSourceLifestyle.GetDistrictAsync().ConfigureAwait(false);
                string strBorough = await _objSourceLifestyle.GetBoroughAsync().ConfigureAwait(false);
                await cboCity.DoThreadSafeAsync(x =>
                {
                    x.SelectedValue = strCity;
                    if (x.SelectedIndex < 0)
                        x.SelectedText = strCity;
                }).ConfigureAwait(false);
                await cboDistrict.DoThreadSafeAsync(x =>
                {
                    x.SelectedValue = strDistrict;
                    if (x.SelectedIndex < 0)
                        x.SelectedText = strDistrict;
                }).ConfigureAwait(false);
                await cboBorough.DoThreadSafeAsync(x =>
                {
                    x.SelectedValue = strBorough;
                    if (x.SelectedIndex < 0)
                        x.SelectedText = strBorough;
                }).ConfigureAwait(false);
            }

            Interlocked.Decrement(ref _intSkipRefresh);
            Interlocked.Decrement(ref _intUpdatingCity);
            Interlocked.Decrement(ref _intUpdatingDistrict);
            Interlocked.Decrement(ref _intUpdatingBorough);
            await CalculateValues().ConfigureAwait(false);
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(await txtLifestyleName.DoThreadSafeFuncAsync(x => x.Text).ConfigureAwait(false)))
            {
                Program.ShowScrollableMessageBox(
                    this, await LanguageManager.GetStringAsync("Message_SelectAdvancedLifestyle_LifestyleName").ConfigureAwait(false),
                    await LanguageManager.GetStringAsync("MessageTitle_SelectAdvancedLifestyle_LifestyleName").ConfigureAwait(false),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            _blnAddAgain = false;
            await AcceptForm().ConfigureAwait(false);
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            await AcceptForm().ConfigureAwait(false);
        }

        private async void treQualities_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (_intSkipRefresh > 0)
                return;
            await CalculateValues().ConfigureAwait(false);
        }

        private async void RefreshValues(object sender, EventArgs e)
        {
            if (_intSkipRefresh > 0)
                return;
            await CalculateValues().ConfigureAwait(false);
        }

        private async void nudRoommates_ValueChanged(object sender, EventArgs e)
        {
            if (nudRoommates.Value == 0)
            {
                await chkPrimaryTenant.DoThreadSafeAsync(x =>
                {
                    x.Checked = true;
                    x.Enabled = false;
                }).ConfigureAwait(false);
            }
            else
                await chkPrimaryTenant.DoThreadSafeAsync(x => x.Enabled = true).ConfigureAwait(false);

            if (_intSkipRefresh > 0)
                return;
            await CalculateValues().ConfigureAwait(false);
        }

        private async void chkTrustFund_CheckedChanged(object sender, EventArgs e)
        {
            if (await chkTrustFund.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
            {
                await nudRoommates.DoThreadSafeAsync(x =>
                {
                    x.Value = 0;
                    x.Enabled = false;
                }).ConfigureAwait(false);
            }
            else
                await nudRoommates.DoThreadSafeAsync(x => x.Enabled = true).ConfigureAwait(false);

            if (_intSkipRefresh > 0)
                return;
            await CalculateValues().ConfigureAwait(false);
        }

        private async void treQualities_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string strSource = string.Empty;
            string strPage = string.Empty;
            string strSourceIDString = await treQualities.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag.ToString()).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(strSourceIDString))
            {
                XmlNode objXmlQuality = _objXmlDocument.TryGetNodeByNameOrId("/chummer/qualities/quality", strSourceIDString);
                if (objXmlQuality != null)
                {
                    strSource = objXmlQuality["source"]?.InnerText ?? string.Empty;
                    strPage = objXmlQuality["altpage"]?.InnerText ?? objXmlQuality["page"]?.InnerText ?? string.Empty;
                }
            }

            if (!string.IsNullOrEmpty(strSource) && !string.IsNullOrEmpty(strPage))
            {
                SourceString objSource = await SourceString.GetSourceStringAsync(strSource, strPage, GlobalSettings.Language,
                    GlobalSettings.CultureInfo, _objCharacter).ConfigureAwait(false);
                await objSource.SetControlAsync(lblSource).ConfigureAwait(false);
                await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = true).ConfigureAwait(false);
            }
            else
            {
                lblSource.Text = string.Empty;
                await lblSource.SetToolTipAsync(string.Empty).ConfigureAwait(false);
                await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
            }
        }

        // Hacky solutions to data binding causing cursor to reset whenever the user is typing something in: have text changes start a timer, and have a 1s delay in the timer update fire the text update
        private void cboCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_tmrCityChangeTimer == null)
                return;
            if (_tmrCityChangeTimer.Enabled)
                _tmrCityChangeTimer.Stop();
            if (_intSkipRefresh > 0 || _intUpdatingCity > 0)
                return;
            _tmrCityChangeTimer.Start();
        }

        private void cboDistrict_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_tmrDistrictChangeTimer == null)
                return;
            if (_tmrDistrictChangeTimer.Enabled)
                _tmrDistrictChangeTimer.Stop();
            if (_intSkipRefresh > 0 || _intUpdatingDistrict > 0)
                return;
            _tmrDistrictChangeTimer.Start();
        }

        private void cboBorough_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_tmrBoroughChangeTimer == null)
                return;
            if (_tmrBoroughChangeTimer.Enabled)
                _tmrBoroughChangeTimer.Stop();
            if (_intSkipRefresh > 0 || _intUpdatingBorough > 0)
                return;
            _tmrBoroughChangeTimer.Start();
        }

        private async void CityChangeTimer_Tick(object sender, EventArgs e)
        {
            _tmrCityChangeTimer.Stop();
            Interlocked.Increment(ref _intUpdatingCity);
            try
            {
                string strText = await cboCity.DoThreadSafeFuncAsync(x => x.Text)
                                              .ConfigureAwait(false);
                await _objLifestyle.SetCityAsync(strText).ConfigureAwait(false);
            }
            finally
            {
                Interlocked.Decrement(ref _intUpdatingCity);
            }
            await RefreshDistrictList().ConfigureAwait(false);
        }

        private async void DistrictChangeTimer_Tick(object sender, EventArgs e)
        {
            _tmrDistrictChangeTimer.Stop();
            Interlocked.Increment(ref _intUpdatingDistrict);
            try
            {
                string strText = await cboDistrict.DoThreadSafeFuncAsync(x => x.Text)
                                              .ConfigureAwait(false);
                await _objLifestyle.SetDistrictAsync(strText).ConfigureAwait(false);
            }
            finally
            {
                Interlocked.Decrement(ref _intUpdatingDistrict);
            }
            await RefreshBoroughList().ConfigureAwait(false);
        }

        private async void BoroughChangeTimer_Tick(object sender, EventArgs e)
        {
            _tmrBoroughChangeTimer.Stop();
            Interlocked.Increment(ref _intUpdatingBorough);
            try
            {
                string strText = await cboBorough.DoThreadSafeFuncAsync(x => x.Text)
                                              .ConfigureAwait(false);
                await _objLifestyle.SetBoroughAsync(strText).ConfigureAwait(false);
            }
            finally
            {
                Interlocked.Decrement(ref _intUpdatingBorough);
            }
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Whether or not the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain => _blnAddAgain;

        /// <summary>
        /// Lifestyle that was created in the dialogue.
        /// </summary>
        public Lifestyle SelectedLifestyle => _objLifestyle;

        /// <summary>
        /// Type of Lifestyle to create.
        /// </summary>
        public LifestyleType StyleType { get; set; } = LifestyleType.Standard;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private async ValueTask AcceptForm(CancellationToken token = default)
        {
            string strSelectedId = await cboLifestyle.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strSelectedId))
                return;
            XmlNode objXmlLifestyle = _objXmlDocument.TryGetNodeByNameOrId("/chummer/lifestyles/lifestyle", strSelectedId);
            if (objXmlLifestyle == null)
                return;

            _objLifestyle.Source = objXmlLifestyle["source"]?.InnerText;
            _objLifestyle.Page = objXmlLifestyle["page"]?.InnerText;
            _objLifestyle.Name = await txtLifestyleName.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false);
            _objLifestyle.BaseLifestyle = objXmlLifestyle["name"]?.InnerText;
            _objLifestyle.Cost = Convert.ToDecimal(objXmlLifestyle["cost"]?.InnerText, GlobalSettings.InvariantCultureInfo);
            _objLifestyle.Roommates = _objLifestyle.TrustFund ? 0 : await nudRoommates.DoThreadSafeFuncAsync(x => x.ValueAsInt, token: token).ConfigureAwait(false);
            _objLifestyle.Percentage = await nudPercentage.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false);
            _objLifestyle.StyleType = StyleType;
            _objLifestyle.Dice = Convert.ToInt32(objXmlLifestyle["dice"]?.InnerText, GlobalSettings.InvariantCultureInfo);
            _objLifestyle.Multiplier = Convert.ToDecimal(objXmlLifestyle["multiplier"]?.InnerText, GlobalSettings.InvariantCultureInfo);
            _objLifestyle.PrimaryTenant = await chkPrimaryTenant.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false);
            _objLifestyle.TrustFund = await chkTrustFund.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false);
            _objLifestyle.City = await cboCity.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false);
            _objLifestyle.District = await cboDistrict.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false);
            _objLifestyle.Borough = await cboBorough.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false);

            if (objXmlLifestyle.TryGetField("id", Guid.TryParse, out Guid source))
            {
                _objLifestyle.SourceID = source;
            }
            else
            {
                Log.Warn(new object[] { "Missing id field for xmlnode", objXmlLifestyle });
                Utils.BreakIfDebug();
            }

            HashSet<string> setLifestyleQualityIds = new HashSet<string>();
            foreach (TreeNode objNode in await treQualities.DoThreadSafeFuncAsync(x => x.Nodes, token: token).ConfigureAwait(false))
            {
                if (!objNode.Checked)
                    continue;
                string strLoopId = objNode.Tag.ToString();
                setLifestyleQualityIds.Add(strLoopId);
                if (_objLifestyle.LifestyleQualities.Any(x => x.SourceIDString == strLoopId))
                    continue;
                XmlNode objXmlLifestyleQuality = _objXmlDocument.TryGetNodeByNameOrId("/chummer/qualities/quality", strLoopId);
                LifestyleQuality objQuality = new LifestyleQuality(_objCharacter);
                try
                {
                    objQuality.Create(objXmlLifestyleQuality, _objLifestyle, _objCharacter, QualitySource.Selected);
                    await _objLifestyle.LifestyleQualities.AddAsync(objQuality, token: token).ConfigureAwait(false);
                }
                catch
                {
                    try
                    {
                        await objQuality.RemoveAsync(false, token).ConfigureAwait(false);
                    }
                    catch
                    {
                        await objQuality.DisposeAsync().ConfigureAwait(false);
                        // Swallow removal exceptions here because we already want to throw an exception
                    }

                    throw;
                }
            }

            foreach (LifestyleQuality objLifestyleQuality in await _objLifestyle.LifestyleQualities.ToListAsync(
                         x => !setLifestyleQualityIds.Contains(x.SourceIDString), token: token).ConfigureAwait(false))
                await objLifestyleQuality.RemoveAsync(false, token).ConfigureAwait(false);
            await this.DoThreadSafeAsync(x =>
            {
                x.DialogResult = DialogResult.OK;
                x.Close();
            }, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Calculate the LP value for the selected items.
        /// </summary>
        private async ValueTask CalculateValues(bool blnIncludePercentage = true, CancellationToken token = default)
        {
            if (_intSkipRefresh > 0)
                return;

            decimal decRoommates
                = await nudRoommates.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false);
            decimal decBaseCost = 0;
            decimal decCost = 0;
            decimal decMod = 0;
            string strBaseLifestyle = string.Empty;
            // Get the base cost of the lifestyle
            string strSelectedId = await cboLifestyle
                                         .DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token)
                                         .ConfigureAwait(false);
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                XmlNode objXmlAspect
                    = _objXmlDocument.TryGetNodeByNameOrId("/chummer/lifestyles/lifestyle", strSelectedId);

                if (objXmlAspect != null)
                {
                    objXmlAspect.TryGetStringFieldQuickly("name", ref strBaseLifestyle);
                    decimal decTemp = 0;
                    if (objXmlAspect.TryGetDecFieldQuickly("cost", ref decTemp))
                        decBaseCost += decTemp;
                    string strSource = objXmlAspect["source"]?.InnerText;
                    string strPage = objXmlAspect["altpage"]?.InnerText ?? objXmlAspect["page"]?.InnerText;
                    if (!string.IsNullOrEmpty(strSource) && !string.IsNullOrEmpty(strPage))
                    {
                        SourceString objSource = await SourceString.GetSourceStringAsync(
                            strSource, strPage, GlobalSettings.Language,
                            GlobalSettings.CultureInfo, _objCharacter, token).ConfigureAwait(false);
                        await objSource.SetControlAsync(lblSource, token).ConfigureAwait(false);
                        await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = true, token: token)
                                            .ConfigureAwait(false);
                    }
                    else
                    {
                        lblSource.Text = string.Empty;
                        await lblSource.SetToolTipAsync(string.Empty, token: token).ConfigureAwait(false);
                        await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = false, token: token)
                                            .ConfigureAwait(false);
                    }

                    // Add the flat costs from qualities
                    foreach (TreeNode objNode in await treQualities.DoThreadSafeFuncAsync(x => x.Nodes, token: token)
                                                                   .ConfigureAwait(false))
                    {
                        if (objNode.Checked)
                        {
                            string strCost = _objXmlDocument
                                             .SelectSingleNode(
                                                 "/chummer/qualities/quality[id = "
                                                 + objNode.Tag.ToString().CleanXPath() + "]/cost")?.InnerText;
                            if (!string.IsNullOrEmpty(strCost))
                            {
                                (bool blnIsSuccess, object objProcess) = await CommonFunctions
                                                                               .EvaluateInvariantXPathAsync(
                                                                                   strCost, token)
                                                                               .ConfigureAwait(false);
                                if (blnIsSuccess)
                                    decCost += Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo);
                            }
                        }
                    }

                    decimal decBaseMultiplier = 0;
                    if (blnIncludePercentage)
                    {
                        // Add the modifiers from qualities
                        foreach (TreeNode objNode in await treQualities
                                                           .DoThreadSafeFuncAsync(x => x.Nodes, token: token)
                                                           .ConfigureAwait(false))
                        {
                            if (!objNode.Checked)
                                continue;
                            objXmlAspect = _objXmlDocument.SelectSingleNode(
                                "/chummer/qualities/quality[id = " + objNode.Tag.ToString().CleanXPath() + ']');
                            if (objXmlAspect == null)
                                continue;
                            if (objXmlAspect.TryGetDecFieldQuickly("multiplier", ref decTemp))
                                decMod += decTemp / 100.0m;
                            if (objXmlAspect.TryGetDecFieldQuickly("multiplierbaseonly", ref decTemp))
                                decBaseMultiplier += decTemp / 100.0m;
                        }

                        // Check for modifiers in the improvements
                        decMod += await ImprovementManager
                                        .ValueOfAsync(_objCharacter, Improvement.ImprovementType.LifestyleCost,
                                                      token: token).ConfigureAwait(false) / 100.0m;
                    }

                    decBaseCost += decBaseCost * decBaseMultiplier;
                    if (decRoommates > 0)
                    {
                        decBaseCost *= 1.0m + Math.Max(decRoommates / 10.0m, 0);
                    }
                }
                else
                    await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);
            }
            else
                await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = false, token: token).ConfigureAwait(false);

            decimal decNuyen = decBaseCost + decBaseCost * decMod + decCost;

            string strNuyenSymbol = await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token).ConfigureAwait(false);
            await lblCost
                  .DoThreadSafeAsync(
                      x => x.Text = decNuyen.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo)
                                    + strNuyenSymbol, token: token)
                  .ConfigureAwait(false);
            decimal decPercentage
                = await nudPercentage.DoThreadSafeFuncAsync(x => x.Value, token: token).ConfigureAwait(false);
            if (decPercentage != 100 || decRoommates != 0 && !await chkPrimaryTenant
                                                                    .DoThreadSafeFuncAsync(x => x.Checked, token: token)
                                                                    .ConfigureAwait(false))
            {
                decimal decDiscount = decNuyen;
                decDiscount *= decPercentage / 100;
                if (decRoommates != 0)
                {
                    decDiscount /= decRoommates;
                }

                string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token)
                                                       .ConfigureAwait(false);
                await lblCost
                      .DoThreadSafeAsync(
                          x => x.Text += strSpace + '('
                                                  + decDiscount.ToString(
                                                      _objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo)
                                                  + strNuyenSymbol + ')', token: token)
                      .ConfigureAwait(false);
            }

            bool blnShowCost
                = !string.IsNullOrEmpty(await lblCost.DoThreadSafeFuncAsync(x => x.Text, token: token)
                                                     .ConfigureAwait(false));
            await lblCostLabel.DoThreadSafeAsync(x => x.Visible = blnShowCost, token: token).ConfigureAwait(false);

            // Characters with the Trust Fund Quality can have the lifestyle discounted.
            if (Lifestyle.StaticIsTrustFundEligible(_objCharacter, strBaseLifestyle))
            {
                bool blnTrustFund = _objSourceLifestyle?.TrustFund ?? !await _objCharacter.Lifestyles
                    .AnyAsync(x => x.TrustFund, token: token).ConfigureAwait(false);
                await chkTrustFund.DoThreadSafeAsync(x =>
                {
                    x.Visible = true;
                    x.Checked = blnTrustFund;
                }, token: token).ConfigureAwait(false);
            }
            else
            {
                await chkTrustFund.DoThreadSafeAsync(x =>
                {
                    x.Checked = false;
                    x.Visible = false;
                }, token: token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Lifestyle to update when editing.
        /// </summary>
        /// <param name="objLifestyle">Lifestyle to edit.</param>
        public void SetLifestyle(Lifestyle objLifestyle)
        {
            _objSourceLifestyle = objLifestyle ?? throw new ArgumentNullException(nameof(objLifestyle));
            StyleType = objLifestyle.StyleType;
        }

        /// <summary>
        /// Sort the contents of a TreeView alphabetically.
        /// </summary>
        /// <param name="treTree">TreeView to sort.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        private static async ValueTask SortTree(TreeView treTree, CancellationToken token = default)
        {
            TreeNode[] lstNodes = await treTree.DoThreadSafeFuncAsync(x => x.Nodes.Cast<TreeNode>().ToArray(), token: token).ConfigureAwait(false);
            await treTree.DoThreadSafeAsync(x => x.Nodes.Clear(), token: token).ConfigureAwait(false);
            try
            {
                Array.Sort(lstNodes, CompareTreeNodes.CompareText);
            }
            catch (ArgumentException)
            {
                // Swallow this
            }
            await treTree.DoThreadSafeAsync(x => x.Nodes.AddRange(lstNodes), token: token).ConfigureAwait(false);
        }

        /// <summary>
        /// Populates The District list after a City was selected
        /// </summary>
        private async ValueTask RefreshDistrictList(CancellationToken token = default)
        {
            string strSelectedCityRefresh = await cboCity.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString() ?? x.SelectedText, token: token).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strSelectedCityRefresh))
                strSelectedCityRefresh = string.Empty;
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                           out List<ListItem> lstDistrict))
            {
                using (XmlNodeList xmlDistrictList
                       = _objXmlDocument.SelectNodes("/chummer/cities/city[name = "
                                                     + strSelectedCityRefresh.CleanXPath() + "]/district"))
                {
                    if (xmlDistrictList?.Count > 0)
                    {
                        foreach (XmlNode objXmlDistrict in xmlDistrictList)
                        {
                            string strName = objXmlDistrict["name"]?.InnerText
                                             ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
                            lstDistrict.Add(new ListItem(strName, objXmlDistrict["translate"]?.InnerText ?? strName));
                        }
                    }
                }

                lstDistrict.Sort();
                await cboDistrict.PopulateWithListItemsAsync(lstDistrict, token: token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Refreshes the BoroughList based on the selected District to generate a cascading dropdown menu
        /// </summary>
        private async ValueTask RefreshBoroughList(CancellationToken token = default)
        {
            string strSelectedCityRefresh = await cboCity.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString() ?? x.SelectedText, token: token).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strSelectedCityRefresh))
                strSelectedCityRefresh = string.Empty;
            string strSelectedDistrictRefresh = await cboDistrict.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString() ?? x.SelectedText, token: token).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strSelectedDistrictRefresh))
                strSelectedDistrictRefresh = string.Empty;
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                           out List<ListItem> lstBorough))
            {
                using (XmlNodeList xmlBoroughList = _objXmlDocument.SelectNodes(
                           "/chummer/cities/city[name = " + strSelectedCityRefresh.CleanXPath() + "]/district[name = "
                           + strSelectedDistrictRefresh.CleanXPath() + "]/borough"))
                {
                    if (xmlBoroughList?.Count > 0)
                    {
                        foreach (XmlNode objXmlDistrict in xmlBoroughList)
                        {
                            string strName = objXmlDistrict["name"]?.InnerText
                                             ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
                            lstBorough.Add(new ListItem(strName, objXmlDistrict["translate"]?.InnerText ?? strName));
                        }
                    }
                }

                lstBorough.Sort();
                await cboBorough.PopulateWithListItemsAsync(lstBorough, token: token).ConfigureAwait(false);
            }
        }

        private async void OpenSourceFromLabel(object sender, EventArgs e)
        {
            await CommonFunctions.OpenPdfFromControl(sender).ConfigureAwait(false);
        }

        #endregion Methods
    }
}
