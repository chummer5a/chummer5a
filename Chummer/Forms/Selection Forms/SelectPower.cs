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
using System.Text;
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
            _blnLoading = false;
            await BuildPowerList();
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            AddAgain = false;
            await AcceptForm();
        }

        private async void lstPowers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;

            string strSelectedId = lstPowers.SelectedValue?.ToString();
            XPathNavigator objXmlPower = null;
            if (!string.IsNullOrEmpty(strSelectedId))
                objXmlPower = _xmlBasePowerDataNode.SelectSingleNode("powers/power[id = " + strSelectedId.CleanXPath() + ']');

            if (objXmlPower != null)
            {
                string strSpace = await LanguageManager.GetStringAsync("String_Space");
                // Display the information for the selected Power.
                string strPowerPointsText = objXmlPower.SelectSingleNode("points")?.Value ?? string.Empty;
                if (objXmlPower.SelectSingleNode("levels")?.Value == bool.TrueString)
                {
                    strPowerPointsText += strSpace + '/' + strSpace + await LanguageManager.GetStringAsync("Label_Power_Level");
                }
                string strExtrPointCost = objXmlPower.SelectSingleNode("extrapointcost")?.Value;
                if (!string.IsNullOrEmpty(strExtrPointCost))
                {
                    strPowerPointsText = strExtrPointCost + strSpace + '+' + strSpace + strPowerPointsText;
                }
                lblPowerPoints.Text = strPowerPointsText;

                string strSource = objXmlPower.SelectSingleNode("source")?.Value ?? await LanguageManager.GetStringAsync("String_Unknown");
                string strPage = (await objXmlPower.SelectSingleNodeAndCacheExpressionAsync("altpage"))?.Value ?? objXmlPower.SelectSingleNode("page")?.Value ?? await LanguageManager.GetStringAsync("String_Unknown");
                SourceString objSource = await SourceString.GetSourceStringAsync(strSource, strPage, GlobalSettings.Language,
                    GlobalSettings.CultureInfo, _objCharacter);
                lblSource.Text = objSource.ToString();
                lblSource.SetToolTip(objSource.LanguageBookTooltip);
                lblPowerPointsLabel.Visible = !string.IsNullOrEmpty(lblPowerPoints.Text);
                lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);
                tlpRight.Visible = true;
            }
            else
            {
                tlpRight.Visible = false;
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private async void cmdOKAdd_Click(object sender, EventArgs e)
        {
            AddAgain = true;
            await AcceptForm();
        }

        private async void txtSearch_TextChanged(object sender, EventArgs e)
        {
            await BuildPowerList();
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
                txtSearch.Select(txtSearch.Text.Length, 0);
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Whether or not the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain { get; private set; }

        /// <summary>
        /// Whether or not we should ignore how many of a given power may be taken. Generally used when bonding Qi Foci.
        /// </summary>
        public bool IgnoreLimits { get; set; }

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

        private async ValueTask BuildPowerList()
        {
            if (_blnLoading)
                return;

            string strFilter = '(' + _objCharacter.Settings.BookXPath() + ')';
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

            if (!string.IsNullOrEmpty(txtSearch.Text))
                strFilter += " and " + CommonFunctions.GenerateSearchXPath(txtSearch.Text);

            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstPower))
            {
                foreach (XPathNavigator objXmlPower in _xmlBasePowerDataNode.Select("powers/power[" + strFilter + ']'))
                {
                    decimal decPoints
                        = Convert.ToDecimal((await objXmlPower.SelectSingleNodeAndCacheExpressionAsync("points"))?.Value,
                                            GlobalSettings.InvariantCultureInfo);
                    string strExtraPointCost = (await objXmlPower.SelectSingleNodeAndCacheExpressionAsync("extrapointcost"))?.Value;
                    string strName = (await objXmlPower.SelectSingleNodeAndCacheExpressionAsync("name"))?.Value
                                     ?? await LanguageManager.GetStringAsync("String_Unknown");
                    if (!string.IsNullOrEmpty(strExtraPointCost)
                        && !_objCharacter.Powers.Any(power => power.Name == strName && power.TotalRating > 0))
                    {
                        //If this power has already had its rating paid for with PP, we don't care about the extrapoints cost.
                        decPoints += Convert.ToDecimal(strExtraPointCost, GlobalSettings.InvariantCultureInfo);
                    }

                    if (_decLimitToRating > 0 && decPoints > _decLimitToRating)
                    {
                        continue;
                    }

                    if (!objXmlPower.RequirementsMet(_objCharacter, null, string.Empty, string.Empty, string.Empty,
                                                     string.Empty, IgnoreLimits))
                        continue;

                    lstPower.Add(new ListItem(
                                     (await objXmlPower.SelectSingleNodeAndCacheExpressionAsync("id"))?.Value ?? string.Empty,
                                     (await objXmlPower.SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value ?? strName));
                }

                lstPower.Sort(CompareListItems.CompareNames);
                _blnLoading = true;
                string strOldSelected = lstPowers.SelectedValue?.ToString();
                lstPowers.BeginUpdate();
                lstPowers.PopulateWithListItems(lstPower);
                _blnLoading = false;
                if (!string.IsNullOrEmpty(strOldSelected))
                    lstPowers.SelectedValue = strOldSelected;
                else
                    lstPowers.SelectedIndex = -1;
                lstPowers.EndUpdate();
            }
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private async ValueTask AcceptForm()
        {
            string strSelectedId = lstPowers.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Check to see if the user needs to select anything for the Power.
                XPathNavigator objXmlPower = _xmlBasePowerDataNode.SelectSingleNode("powers/power[id = " + strSelectedId.CleanXPath() + ']');

                if (objXmlPower.RequirementsMet(_objCharacter, null, await LanguageManager.GetStringAsync("String_Power"), string.Empty, string.Empty, string.Empty, IgnoreLimits))
                {
                    SelectedPower = strSelectedId;
                    DialogResult = DialogResult.OK;
                }
            }
        }

        private async void OpenSourceFromLabel(object sender, EventArgs e)
        {
            await CommonFunctions.OpenPdfFromControl(sender);
        }

        #endregion Methods
    }
}
