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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.XPath;

namespace Chummer
{
    public partial class SelectPower : Form
    {
        private bool _blnLoading = true;
        private string _strLimitToPowers;
        private decimal _decLimitToRating;

        private readonly Character _objCharacter;

        private readonly XPathNavigator _xmlBasePowerDataNode;

        #region Control Events

        public SelectPower(Character objCharacter)
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objCharacter = objCharacter;
            // Load the Powers information.
            _xmlBasePowerDataNode = _objCharacter.LoadDataXPath("powers.xml").SelectSingleNodeAndCacheExpression("/chummer");
        }

        private async void SelectPower_Load(object sender, EventArgs e)
        {
            await cmdOKAdd.DoThreadSafeAsync(x => x.Visible = !ForBonus).ConfigureAwait(false);
            _blnLoading = false;
            await BuildPowerList().ConfigureAwait(false);
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            AddAgain = false;
            await AcceptForm().ConfigureAwait(false);
        }

        private async void lstPowers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;

            string strSelectedId = await lstPowers.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false);
            XPathNavigator objXmlPower = null;
            if (!string.IsNullOrEmpty(strSelectedId))
                objXmlPower = _xmlBasePowerDataNode.TryGetNodeByNameOrId("powers/power", strSelectedId);

            if (objXmlPower != null)
            {
                string strSpace = await LanguageManager.GetStringAsync("String_Space").ConfigureAwait(false);
                // Display the information for the selected Power.
                string strPowerPointsText = objXmlPower.SelectSingleNodeAndCacheExpression("points")?.Value ?? string.Empty;
                if (objXmlPower.SelectSingleNodeAndCacheExpression("levels")?.Value == bool.TrueString)
                {
                    strPowerPointsText += strSpace + '/' + strSpace + await LanguageManager.GetStringAsync("Label_Power_Level").ConfigureAwait(false);
                }
                string strExtrPointCost = objXmlPower.SelectSingleNodeAndCacheExpression("extrapointcost")?.Value;
                if (!string.IsNullOrEmpty(strExtrPointCost))
                {
                    strPowerPointsText = strExtrPointCost + strSpace + '+' + strSpace + strPowerPointsText;
                }
                await lblPowerPoints.DoThreadSafeAsync(x => x.Text = strPowerPointsText).ConfigureAwait(false);

                string strSource = objXmlPower.SelectSingleNodeAndCacheExpression("source")?.Value ?? await LanguageManager.GetStringAsync("String_Unknown").ConfigureAwait(false);
                string strPage = objXmlPower.SelectSingleNodeAndCacheExpression("altpage")?.Value ?? objXmlPower.SelectSingleNodeAndCacheExpression("page")?.Value ?? await LanguageManager.GetStringAsync("String_Unknown").ConfigureAwait(false);
                SourceString objSource = await SourceString.GetSourceStringAsync(strSource, strPage, GlobalSettings.Language,
                    GlobalSettings.CultureInfo, _objCharacter).ConfigureAwait(false);
                await objSource.SetControlAsync(lblSource).ConfigureAwait(false);
                await lblPowerPointsLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strPowerPointsText)).ConfigureAwait(false);
                await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(objSource.ToString())).ConfigureAwait(false);
                await tlpRight.DoThreadSafeAsync(x => x.Visible = true).ConfigureAwait(false);
            }
            else
            {
                await tlpRight.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void cmdOKAdd_Click(object sender, EventArgs e)
        {
            AddAgain = true;
            await AcceptForm().ConfigureAwait(false);
        }

        private async void txtSearch_TextChanged(object sender, EventArgs e)
        {
            await BuildPowerList().ConfigureAwait(false);
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down when lstPowers.SelectedIndex + 1 < lstPowers.Items.Count:
                    ++lstPowers.SelectedIndex;
                    break;

                case Keys.Down:
                    {
                        if (lstPowers.Items.Count > 0)
                        {
                            lstPowers.SelectedIndex = 0;
                        }

                        break;
                    }
                case Keys.Up when lstPowers.SelectedIndex - 1 >= 0:
                    --lstPowers.SelectedIndex;
                    break;

                case Keys.Up:
                    {
                        if (lstPowers.Items.Count > 0)
                        {
                            lstPowers.SelectedIndex = lstPowers.Items.Count - 1;
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

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Whether the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain { get; private set; }

        /// <summary>
        /// Whether we should ignore how many of a given power may be taken. Generally used when bonding Qi Foci.
        /// </summary>
        public bool IgnoreLimits { get; set; }

        /// <summary>
        /// Whether this window is being shown to select a power for a bonus node or to just select a power for a character traditionally
        /// </summary>
        public bool ForBonus { get; set; }

        /// <summary>
        /// Power that was selected in the dialogue.
        /// </summary>
        public string SelectedPower { get; private set; } = string.Empty;

        /// <summary>
        /// Only the provided Powers should be shown in the list.
        /// </summary>
        public string LimitToPowers
        {
            set => _strLimitToPowers = value;
        }

        /// <summary>
        /// Limit the selections based on the Rating of an external source, where 1 Rating = 0.25 PP.
        /// </summary>
        public int LimitToRating
        {
            set => _decLimitToRating = value * PointsPerLevel;
        }

        /// <summary>
        /// Value of the PP per level if using LimitToRating. Defaults to 0.25.
        /// </summary>
        public decimal PointsPerLevel { set; get; } = 0.25m;

        #endregion Properties

        #region Methods

        private async Task BuildPowerList(CancellationToken token = default)
        {
            if (_blnLoading)
                return;

            string strFilter = '(' + await _objCharacter.Settings.BookXPathAsync(token: token).ConfigureAwait(false) + ')';
            if (!string.IsNullOrEmpty(_strLimitToPowers))
            {
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdFilter))
                {
                    foreach (string strPower in _strLimitToPowers.SplitNoAlloc(
                                 ',', StringSplitOptions.RemoveEmptyEntries))
                        sbdFilter.Append("name = ").Append(strPower.CleanXPath()).Append(" or ");
                    if (sbdFilter.Length > 0)
                    {
                        sbdFilter.Length -= 4;
                        strFilter += " and (" + sbdFilter + ')';
                    }
                }
            }

            string strSearch = await txtSearch.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(strSearch))
                strFilter += " and " + CommonFunctions.GenerateSearchXPath(strSearch);

            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstPower))
            {
                foreach (XPathNavigator objXmlPower in _xmlBasePowerDataNode.Select("powers/power[" + strFilter + ']'))
                {
                    decimal decPoints
                        = Convert.ToDecimal(objXmlPower.SelectSingleNodeAndCacheExpression("points", token: token)?.Value,
                                            GlobalSettings.InvariantCultureInfo);
                    string strExtraPointCost = objXmlPower.SelectSingleNodeAndCacheExpression("extrapointcost", token: token)?.Value;
                    string strName = objXmlPower.SelectSingleNodeAndCacheExpression("name", token: token)?.Value
                                     ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(strExtraPointCost)
                        && !await _objCharacter.Powers
                                               .AnyAsync(
                                                   async power =>
                                                       power.Name == strName && await power.GetTotalRatingAsync(token)
                                                           .ConfigureAwait(false) > 0, token).ConfigureAwait(false))
                    {
                        //If this power has already had its rating paid for with PP, we don't care about the extrapoints cost.
                        decPoints += Convert.ToDecimal(strExtraPointCost, GlobalSettings.InvariantCultureInfo);
                    }

                    if (_decLimitToRating > 0 && decPoints > _decLimitToRating)
                    {
                        continue;
                    }

                    bool blnIgnoreLimit = IgnoreLimits || (ForBonus && objXmlPower.SelectSingleNodeAndCacheExpression("levels", token: token)?.Value == bool.TrueString);

                    if (!await objXmlPower.RequirementsMetAsync(_objCharacter, blnIgnoreLimit: blnIgnoreLimit, token: token).ConfigureAwait(false))
                        continue;

                    lstPower.Add(new ListItem(
                                     objXmlPower.SelectSingleNodeAndCacheExpression("id", token: token)?.Value ?? string.Empty,
                                     objXmlPower.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value ?? strName));
                }

                lstPower.Sort(CompareListItems.CompareNames);
                _blnLoading = true;
                string strOldSelected = await lstPowers.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
                await lstPowers.PopulateWithListItemsAsync(lstPower, token: token).ConfigureAwait(false);
                _blnLoading = false;
                await lstPowers.DoThreadSafeAsync(x =>
                {
                    if (!string.IsNullOrEmpty(strOldSelected))
                        x.SelectedValue = strOldSelected;
                    else
                        x.SelectedIndex = -1;
                }, token: token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private async Task AcceptForm(CancellationToken token = default)
        {
            string strSelectedId = await lstPowers.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Check to see if the user needs to select anything for the Power.
                XPathNavigator objXmlPower = _xmlBasePowerDataNode.TryGetNodeByNameOrId("powers/power", strSelectedId);

                if (await objXmlPower.RequirementsMetAsync(_objCharacter, null, await LanguageManager.GetStringAsync("String_Power", token: token).ConfigureAwait(false), string.Empty, string.Empty, string.Empty, IgnoreLimits, token: token).ConfigureAwait(false))
                {
                    SelectedPower = strSelectedId;
                    await this.DoThreadSafeAsync(x =>
                    {
                        x.DialogResult = DialogResult.OK;
                        x.Close();
                    }, token: token).ConfigureAwait(false);
                }
            }
        }

        private async void OpenSourceFromLabel(object sender, EventArgs e)
        {
            await CommonFunctions.OpenPdfFromControl(sender).ConfigureAwait(false);
        }

        #endregion Methods
    }
}
