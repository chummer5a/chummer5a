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
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Chummer.Backend.Equipment;
using NLog;

namespace Chummer
{
    public partial class SelectLifestyle : Form
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();
        private bool _blnAddAgain;
        private readonly Lifestyle _objLifestyle;
        private Lifestyle _objSourceLifestyle;
        private readonly Character _objCharacter;

        private readonly XmlDocument _objXmlDocument;

        private bool _blnSkipRefresh = true;

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
                                                     + _objCharacter.Settings.BookXPath() + ']'))
                {
                    if (xmlLifestyleList?.Count > 0)
                    {
                        foreach (XmlNode objXmlLifestyle in xmlLifestyleList)
                        {
                            string strLifeStyleId = objXmlLifestyle["id"]?.InnerText;
                            if (!string.IsNullOrEmpty(strLifeStyleId) && !strLifeStyleId.IsEmptyGuid())
                            {
                                string strName = objXmlLifestyle["name"]?.InnerText
                                                 ?? await LanguageManager.GetStringAsync("String_Unknown");
                                if (strName == _objSourceLifestyle?.BaseLifestyle)
                                    strSelectedId = strLifeStyleId;
                                lstLifestyle.Add(new ListItem(strLifeStyleId,
                                                              objXmlLifestyle["translate"]?.InnerText ?? strName));
                            }
                        }
                    }
                }
                
                await cboLifestyle.PopulateWithListItemsAsync(lstLifestyle);
                await cboLifestyle.DoThreadSafeAsync(x =>
                {
                    if (!string.IsNullOrEmpty(strSelectedId))
                        x.SelectedValue = strSelectedId;
                    if (x.SelectedIndex == -1)
                        x.SelectedIndex = 0;
                });
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
                                             ?? await LanguageManager.GetStringAsync("String_Unknown");
                            lstCity.Add(new ListItem(strName, objXmlCity["translate"]?.InnerText ?? strName));
                        }
                    }
                }
                
                await cboCity.PopulateWithListItemsAsync(lstCity);
            }

            //Populate District and Borough ComboBox for the first time
            await RefreshDistrictList();
            await RefreshBoroughList();

            string strSpace = await LanguageManager.GetStringAsync("String_Space");
            // Fill the Options list.
            using (XmlNodeList xmlLifestyleOptionsList = _objXmlDocument.SelectNodes("/chummer/qualities/quality[(source = \"SR5\" or category = \"Contracts\") and (" + _objCharacter.Settings.BookXPath() + ")]"))
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
                            strBaseString = strSpace + await LanguageManager.GetStringAsync("Label_Base");
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
                            object objProcess = CommonFunctions.EvaluateInvariantXPath(strCost, out bool blnIsSuccess);
                            decimal decCost = blnIsSuccess ? Convert.ToDecimal((double)objProcess) : 0;
                            nodOption.Text = (objXmlOption["translate"]?.InnerText ?? strOptionName)
                                             + strSpace
                                             + '['
                                             + decCost.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo)
                                             + await LanguageManager.GetStringAsync("String_NuyenSymbol") + ']';
                        }
                        await treQualities.DoThreadSafeAsync(x => x.Nodes.Add(nodOption));
                    }
                }
            }

            await SortTree(treQualities);

            if (_objSourceLifestyle != null)
            {
                await txtLifestyleName.DoThreadSafeAsync(x => x.Text = _objSourceLifestyle.Name);
                await nudRoommates.DoThreadSafeAsync(x => x.Value = _objSourceLifestyle.Roommates);
                await nudPercentage.DoThreadSafeAsync(x => x.Value = _objSourceLifestyle.Percentage);
                await treQualities.DoThreadSafeAsync(x =>
                {
                    foreach (LifestyleQuality objQuality in _objSourceLifestyle.LifestyleQualities)
                    {
                        TreeNode objNode = x.FindNode(objQuality.SourceIDString);
                        if (objNode != null)
                            objNode.Checked = true;
                    }
                });
                await chkPrimaryTenant.DoThreadSafeAsync(x => x.Checked = _objSourceLifestyle.PrimaryTenant);
                await chkTrustFund.DoThreadSafeAsync(x => x.Checked = _objSourceLifestyle.TrustFund);
            }

            _blnSkipRefresh = false;
            await CalculateValues();
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(await txtLifestyleName.DoThreadSafeFuncAsync(x => x.Text)))
            {
                Program.ShowMessageBox(
                    this, await LanguageManager.GetStringAsync("Message_SelectAdvancedLifestyle_LifestyleName"),
                    await LanguageManager.GetStringAsync("MessageTitle_SelectAdvancedLifestyle_LifestyleName"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            _blnAddAgain = false;
            await AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private async void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            await AcceptForm();
        }

        private async void treQualities_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            await CalculateValues();
        }

        private async void RefreshValues(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            await CalculateValues();
        }

        private async void nudRoommates_ValueChanged(object sender, EventArgs e)
        {
            if (nudRoommates.Value == 0)
            {
                await chkPrimaryTenant.DoThreadSafeAsync(x =>
                {
                    x.Checked = true;
                    x.Enabled = false;
                });
            }
            else
                await chkPrimaryTenant.DoThreadSafeAsync(x => x.Enabled = true);

            if (_blnSkipRefresh)
                return;
            await CalculateValues();
        }

        private async void chkTrustFund_CheckedChanged(object sender, EventArgs e)
        {
            if (await chkTrustFund.DoThreadSafeFuncAsync(x => x.Checked))
            {
                await nudRoommates.DoThreadSafeAsync(x =>
                {
                    x.Value = 0;
                    x.Enabled = false;
                });
            }
            else
                await nudRoommates.DoThreadSafeAsync(x => x.Enabled = true);

            if (_blnSkipRefresh)
                return;
            await CalculateValues();
        }

        private async void treQualities_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string strSource = string.Empty;
            string strPage = string.Empty;
            string strSourceIDString = await treQualities.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag.ToString());
            if (!string.IsNullOrEmpty(strSourceIDString))
            {
                XmlNode objXmlQuality = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[id = " + strSourceIDString.CleanXPath() + ']');
                if (objXmlQuality != null)
                {
                    strSource = objXmlQuality["source"]?.InnerText ?? string.Empty;
                    strPage = objXmlQuality["altpage"]?.InnerText ?? objXmlQuality["page"]?.InnerText ?? string.Empty;
                }
            }

            if (!string.IsNullOrEmpty(strSource) && !string.IsNullOrEmpty(strPage))
            {
                SourceString objSource = await SourceString.GetSourceStringAsync(strSource, strPage, GlobalSettings.Language,
                    GlobalSettings.CultureInfo, _objCharacter);
                await objSource.SetControlAsync(lblSource);
                await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = true);
            }
            else
            {
                lblSource.Text = string.Empty;
                await lblSource.SetToolTipAsync(string.Empty);
                await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = false);
            }
        }

        private async void cboCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            await RefreshDistrictList();
        }

        private async void cboDistrict_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            await RefreshBoroughList();
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
        private async ValueTask AcceptForm()
        {
            string strSelectedId = await cboLifestyle.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString());
            if (string.IsNullOrEmpty(strSelectedId))
                return;
            XmlNode objXmlLifestyle = _objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[id = " + strSelectedId.CleanXPath() + ']');
            if (objXmlLifestyle == null)
                return;

            _objLifestyle.Source = objXmlLifestyle["source"]?.InnerText;
            _objLifestyle.Page = objXmlLifestyle["page"]?.InnerText;
            _objLifestyle.Name = await txtLifestyleName.DoThreadSafeFuncAsync(x => x.Text);
            _objLifestyle.BaseLifestyle = objXmlLifestyle["name"]?.InnerText;
            _objLifestyle.Cost = Convert.ToDecimal(objXmlLifestyle["cost"]?.InnerText, GlobalSettings.InvariantCultureInfo);
            _objLifestyle.Roommates = _objLifestyle.TrustFund ? 0 : await nudRoommates.DoThreadSafeFuncAsync(x => x.ValueAsInt);
            _objLifestyle.Percentage = await nudPercentage.DoThreadSafeFuncAsync(x => x.Value);
            _objLifestyle.StyleType = StyleType;
            _objLifestyle.Dice = Convert.ToInt32(objXmlLifestyle["dice"]?.InnerText, GlobalSettings.InvariantCultureInfo);
            _objLifestyle.Multiplier = Convert.ToDecimal(objXmlLifestyle["multiplier"]?.InnerText, GlobalSettings.InvariantCultureInfo);
            _objLifestyle.PrimaryTenant = await chkPrimaryTenant.DoThreadSafeFuncAsync(x => x.Checked);
            _objLifestyle.TrustFund = await chkTrustFund.DoThreadSafeFuncAsync(x => x.Checked);
            _objLifestyle.City = await cboCity.DoThreadSafeFuncAsync(x => x.Text);
            _objLifestyle.District = await cboDistrict.DoThreadSafeFuncAsync(x => x.Text);
            _objLifestyle.Borough = await cboBorough.DoThreadSafeFuncAsync(x => x.Text);

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
            foreach (TreeNode objNode in await treQualities.DoThreadSafeFuncAsync(x => x.Nodes))
            {
                if (!objNode.Checked)
                    continue;
                string strLoopId = objNode.Tag.ToString();
                setLifestyleQualityIds.Add(strLoopId);
                if (_objLifestyle.LifestyleQualities.Any(x => x.SourceIDString == strLoopId))
                    continue;
                XmlNode objXmlLifestyleQuality = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[id = " + strLoopId.CleanXPath() + ']');
                LifestyleQuality objQuality = new LifestyleQuality(_objCharacter);
                objQuality.Create(objXmlLifestyleQuality, _objLifestyle, _objCharacter, QualitySource.Selected);
                await _objLifestyle.LifestyleQualities.AddAsync(objQuality);
            }

            foreach (LifestyleQuality objLifestyleQuality in _objLifestyle.LifestyleQualities.Where(x =>
                         !setLifestyleQualityIds.Contains(x.SourceIDString)).ToList())
                objLifestyleQuality.Remove(false);

            DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// Calculate the LP value for the selected items.
        /// </summary>
        private async ValueTask CalculateValues(bool blnIncludePercentage = true)
        {
            if (_blnSkipRefresh)
                return;

            decimal decRoommates = await nudRoommates.DoThreadSafeFuncAsync(x => x.Value);
            decimal decBaseCost = 0;
            decimal decCost = 0;
            decimal decMod = 0;
            string strBaseLifestyle = string.Empty;
            // Get the base cost of the lifestyle
            string strSelectedId = await cboLifestyle.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString());
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                XmlNode objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[id = " + strSelectedId.CleanXPath() + ']');

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
                        SourceString objSource = await SourceString.GetSourceStringAsync(strSource, strPage, GlobalSettings.Language,
                            GlobalSettings.CultureInfo, _objCharacter);
                        await objSource.SetControlAsync(lblSource);
                        await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = true);
                    }
                    else
                    {
                        lblSource.Text = string.Empty;
                        await lblSource.SetToolTipAsync(string.Empty);
                        await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = false);
                    }

                    // Add the flat costs from qualities
                    foreach (TreeNode objNode in await treQualities.DoThreadSafeFuncAsync(x => x.Nodes))
                    {
                        if (objNode.Checked)
                        {
                            string strCost = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[id = " + objNode.Tag.ToString().CleanXPath() + "]/cost")?.InnerText;
                            if (!string.IsNullOrEmpty(strCost))
                            {
                                object objProcess = CommonFunctions.EvaluateInvariantXPath(strCost, out bool blnIsSuccess);
                                if (blnIsSuccess)
                                    decCost += Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo);
                            }
                        }
                    }

                    decimal decBaseMultiplier = 0;
                    if (blnIncludePercentage)
                    {
                        // Add the modifiers from qualities
                        foreach (TreeNode objNode in await treQualities.DoThreadSafeFuncAsync(x => x.Nodes))
                        {
                            if (!objNode.Checked)
                                continue;
                            objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[id = " + objNode.Tag.ToString().CleanXPath() + ']');
                            if (objXmlAspect == null)
                                continue;
                            if (objXmlAspect.TryGetDecFieldQuickly("multiplier", ref decTemp))
                                decMod += decTemp / 100.0m;
                            if (objXmlAspect.TryGetDecFieldQuickly("multiplierbaseonly", ref decTemp))
                                decBaseMultiplier += decTemp / 100.0m;
                        }

                        // Check for modifiers in the improvements
                        decMod += await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.LifestyleCost) / 100.0m;
                    }

                    decBaseCost += decBaseCost * decBaseMultiplier;
                    if (decRoommates > 0)
                    {
                        decBaseCost *= 1.0m + Math.Max(decRoommates / 10.0m, 0);
                    }
                }
                else
                    await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = false);
            }
            else
                await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = false);

            decimal decNuyen = decBaseCost + decBaseCost * decMod + decCost;

            await lblCost.DoThreadSafeAsync(x => x.Text = decNuyen.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + LanguageManager.GetString("String_NuyenSymbol"));
            decimal decPercentage = await nudPercentage.DoThreadSafeFuncAsync(x => x.Value);
            if (decPercentage != 100 || decRoommates != 0 && !await chkPrimaryTenant.DoThreadSafeFuncAsync(x => x.Checked))
            {
                decimal decDiscount = decNuyen;
                decDiscount *= decPercentage / 100;
                if (decRoommates != 0)
                {
                    decDiscount /= decRoommates;
                }

                string strSpace = await LanguageManager.GetStringAsync("String_Space");
                await lblCost.DoThreadSafeAsync(x => x.Text += strSpace + '(' + decDiscount.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + LanguageManager.GetString("String_NuyenSymbol") + ')');
            }

            await lblCost.DoThreadSafeFuncAsync(x => x.Text)
                         .ContinueWith(
                             y => lblCostLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(y.Result)))
                         .Unwrap();

            // Characters with the Trust Fund Quality can have the lifestyle discounted.
            if (Lifestyle.StaticIsTrustFundEligible(_objCharacter, strBaseLifestyle))
            {
                bool blnTrustFund = _objSourceLifestyle?.TrustFund ?? !await _objCharacter.Lifestyles.AnyAsync(x => x.TrustFund);
                await chkTrustFund.DoThreadSafeAsync(x =>
                {
                    x.Visible = true;
                    x.Checked = blnTrustFund;
                });
            }
            else
            {
                await chkTrustFund.DoThreadSafeAsync(x =>
                {
                    x.Checked = false;
                    x.Visible = false;
                });
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
        private static async ValueTask SortTree(TreeView treTree)
        {
            List<TreeNode> lstNodes = await treTree.DoThreadSafeFuncAsync(x => x.Nodes.Cast<TreeNode>().ToList());
            await treTree.DoThreadSafeAsync(x => x.Nodes.Clear());
            try
            {
                lstNodes.Sort(CompareTreeNodes.CompareText);
            }
            catch (ArgumentException)
            {
                // Swallow this
            }
            await treTree.DoThreadSafeAsync(x =>
            {
                foreach (TreeNode objNode in lstNodes)
                    x.Nodes.Add(objNode);
            });
        }

        /// <summary>
        /// Populates The District list after a City was selected
        /// </summary>
        private async ValueTask RefreshDistrictList()
        {
            string strSelectedCityRefresh = await cboCity.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()) ?? string.Empty;
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
                                             ?? await LanguageManager.GetStringAsync("String_Unknown");
                            lstDistrict.Add(new ListItem(strName, objXmlDistrict["translate"]?.InnerText ?? strName));
                        }
                    }
                }
                
                await cboDistrict.PopulateWithListItemsAsync(lstDistrict);
            }
        }

        /// <summary>
        /// Refreshes the BoroughList based on the selected District to generate a cascading dropdown menu
        /// </summary>
        private async ValueTask RefreshBoroughList()
        {
            string strSelectedCityRefresh = await cboCity.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()) ?? string.Empty;
            string strSelectedDistrictRefresh = await cboDistrict.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()) ?? string.Empty;
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
                                             ?? await LanguageManager.GetStringAsync("String_Unknown");
                            lstBorough.Add(new ListItem(strName, objXmlDistrict["translate"]?.InnerText ?? strName));
                        }
                    }
                }
                
                await cboBorough.PopulateWithListItemsAsync(lstBorough);
            }
        }

        private async void OpenSourceFromLabel(object sender, EventArgs e)
        {
            await CommonFunctions.OpenPdfFromControl(sender);
        }

        #endregion Methods
    }
}
