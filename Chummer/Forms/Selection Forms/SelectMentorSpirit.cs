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
using System.Windows.Forms;
using System.Xml.XPath;

namespace Chummer
{
    public partial class SelectMentorSpirit : Form
    {
        private bool _blnSkipRefresh = true;
        private string _strForceMentor = string.Empty;

        private readonly XPathNavigator _xmlBaseMentorSpiritDataNode;
        private readonly Character _objCharacter;

        private string _strChoice1 = string.Empty;
        private string _strChoice2 = string.Empty;

        #region Control Events

        public SelectMentorSpirit(Character objCharacter, string strXmlFile = "mentors.xml")
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            InitializeComponent();
            if (strXmlFile == "paragons.xml")
                Tag = "Title_SelectMentorSpirit_Paragon";
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            this.UpdateParentForToolTipControls();
            // Load the Mentor information.
            _xmlBaseMentorSpiritDataNode = objCharacter.LoadDataXPath(strXmlFile).SelectSingleNodeAndCacheExpression("/chummer");
        }

        private async void lstMentor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            await this.DoThreadSafeAsync(x => x.SuspendLayout()).ConfigureAwait(false);
            try
            {
                XPathNavigator objXmlMentor = null;
                if (lstMentor.SelectedIndex >= 0)
                {
                    string strSelectedId = await lstMentor.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(strSelectedId))
                    {
                        objXmlMentor =
                            _xmlBaseMentorSpiritDataNode.TryGetNodeByNameOrId("mentors/mentor", strSelectedId);
                    }
                }

                if (objXmlMentor != null)
                {
                    // If the Mentor offers a choice of bonuses, build the list and let the user select one.
                    XPathNavigator xmlChoices = objXmlMentor.SelectSingleNodeAndCacheExpression("choices");
                    if (xmlChoices != null)
                    {
                        using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstChoice1))
                        using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                                       out List<ListItem> lstChoice2))
                        {
                            foreach (XPathNavigator objChoice in xmlChoices.SelectAndCacheExpression("choice"))
                            {
                                string strName = objChoice.SelectSingleNodeAndCacheExpression("name")?.Value
                                                 ?? string.Empty;
                                if ((await _objCharacter.GetAdeptEnabledAsync().ConfigureAwait(false) ||
                                     !strName.StartsWith("Adept:", StringComparison.Ordinal)) &&
                                    (await _objCharacter.GetMagicianEnabledAsync().ConfigureAwait(false) ||
                                     !strName.StartsWith("Magician:", StringComparison.Ordinal)))
                                {
                                    if (objChoice.SelectSingleNodeAndCacheExpression("@set")?.Value == "2")
                                    {
                                        lstChoice2.Add(new ListItem(strName,
                                                                    objChoice
                                                                        .SelectSingleNodeAndCacheExpression(
                                                                            "translate")
                                                                    ?.Value ?? strName));
                                    }
                                    else
                                    {
                                        lstChoice1.Add(new ListItem(strName,
                                                                    objChoice
                                                                        .SelectSingleNodeAndCacheExpression(
                                                                            "translate")
                                                                    ?.Value ?? strName));
                                    }
                                }
                            }

                            //If there is only a single option, show it as a label.
                            //If there are more, show the drop down menu
                            if (lstChoice1.Count > 0)
                                await cboChoice1.PopulateWithListItemsAsync(lstChoice1).ConfigureAwait(false);
                            await cboChoice1.DoThreadSafeAsync(x => x.Visible = lstChoice1.Count > 1).ConfigureAwait(false);
                            await lblBonusText1.DoThreadSafeAsync(x =>
                            {
                                x.Visible = lstChoice1.Count == 1;
                                if (lstChoice1.Count == 1)
                                    x.Text = lstChoice1[0].Name;
                            }).ConfigureAwait(false);
                            if (lstChoice2.Count > 0)
                                await cboChoice2.PopulateWithListItemsAsync(lstChoice2).ConfigureAwait(false);
                            await cboChoice2.DoThreadSafeAsync(x => x.Visible = lstChoice2.Count > 1).ConfigureAwait(false);
                            await lblBonusText2.DoThreadSafeAsync(x =>
                            {
                                x.Visible = lstChoice2.Count == 1;
                                if (lstChoice2.Count == 1)
                                    x.Text = lstChoice2[0].Name;
                            }).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        await cboChoice1.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                        await cboChoice2.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                        await lblBonusText1.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                        await lblBonusText2.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                    }

                    bool blnTemp = await cboChoice1.DoThreadSafeFuncAsync(x => x.Visible).ConfigureAwait(false);
                    await lblChoice1.DoThreadSafeAsync(x => x.Visible = blnTemp).ConfigureAwait(false);
                    bool blnTemp2 = await cboChoice2.DoThreadSafeFuncAsync(x => x.Visible).ConfigureAwait(false);
                    await lblChoice2.DoThreadSafeAsync(x => x.Visible = blnTemp2).ConfigureAwait(false);
                    bool blnTemp3 = await lblBonusText1.DoThreadSafeFuncAsync(x => x.Visible).ConfigureAwait(false);
                    await lblBonus1.DoThreadSafeAsync(x => x.Visible = blnTemp3).ConfigureAwait(false);
                    bool blnTemp4 = await lblBonusText2.DoThreadSafeFuncAsync(x => x.Visible).ConfigureAwait(false);
                    await lblBonus2.DoThreadSafeAsync(x => x.Visible = blnTemp4).ConfigureAwait(false);

                    // Get the information for the selected Mentor.
                    string strUnknown = await LanguageManager.GetStringAsync("String_Unknown").ConfigureAwait(false);
                    string strAdvantage = objXmlMentor.SelectSingleNodeAndCacheExpression("altadvantage")?.Value ??
                                          objXmlMentor.SelectSingleNodeAndCacheExpression("advantage")?.Value ??
                                          strUnknown;
                    await lblAdvantage.DoThreadSafeAsync(x => x.Text = strAdvantage).ConfigureAwait(false);
                    await lblAdvantageLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strAdvantage)).ConfigureAwait(false);
                    string strDisadvantage = objXmlMentor.SelectSingleNodeAndCacheExpression("altdisadvantage")?.Value ??
                                             objXmlMentor.SelectSingleNodeAndCacheExpression("disadvantage")?.Value ??
                                             strUnknown;
                    await lblDisadvantage.DoThreadSafeAsync(x => x.Text = strDisadvantage).ConfigureAwait(false);
                    await lblDisadvantageLabel.DoThreadSafeAsync(x => x.Visible = !string.IsNullOrEmpty(strDisadvantage)).ConfigureAwait(false);

                    string strSource = objXmlMentor.SelectSingleNodeAndCacheExpression("source")?.Value ??
                                       await LanguageManager.GetStringAsync("String_Unknown").ConfigureAwait(false);
                    string strPage = objXmlMentor.SelectSingleNodeAndCacheExpression("altpage")?.Value ??
                                     objXmlMentor.SelectSingleNodeAndCacheExpression("page")?.Value ??
                                     await LanguageManager.GetStringAsync("String_Unknown").ConfigureAwait(false);
                    SourceString objSourceString = await SourceString.GetSourceStringAsync(strSource, strPage, GlobalSettings.Language,
                        GlobalSettings.CultureInfo, _objCharacter).ConfigureAwait(false);
                    await objSourceString.SetControlAsync(lblSource, this).ConfigureAwait(false);
                    bool blnSourceEmpty = string.IsNullOrEmpty(await lblSource.DoThreadSafeFuncAsync(x => x.Text).ConfigureAwait(false));
                    await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = !blnSourceEmpty).ConfigureAwait(false);
                    await cmdOK.DoThreadSafeAsync(x => x.Enabled = true).ConfigureAwait(false);
                    await tlpRight.DoThreadSafeAsync(x => x.Visible = true).ConfigureAwait(false);
                    await tlpBottomRight.DoThreadSafeAsync(x => x.Visible = true).ConfigureAwait(false);
                }
                else
                {
                    await tlpRight.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                    await tlpBottomRight.DoThreadSafeAsync(x => x.Visible = false).ConfigureAwait(false);
                    await cmdOK.DoThreadSafeAsync(x => x.Enabled = false).ConfigureAwait(false);
                }
            }
            finally
            {
                await this.DoThreadSafeAsync(x => x.ResumeLayout()).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm(object sender, EventArgs e)
        {
            string strSelectedId = lstMentor.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                XPathNavigator objXmlMentor = _xmlBaseMentorSpiritDataNode.TryGetNodeByNameOrId("mentors/mentor", strSelectedId);
                if (objXmlMentor == null)
                    return;

                SelectedMentor = strSelectedId;
                _strChoice1 = cboChoice1.SelectedValue?.ToString() ?? string.Empty;
                _strChoice2 = cboChoice2.SelectedValue?.ToString() ?? string.Empty;
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        /// <summary>
        /// Populate the Mentor list.
        /// </summary>
        private async void RefreshMentorsList(object sender, EventArgs e)
        {
            string strForceId = string.Empty;

            string strFilter = "(" + await (await _objCharacter.GetSettingsAsync().ConfigureAwait(false)).BookXPathAsync().ConfigureAwait(false) + ")";
            string strSearch = await txtSearch.DoThreadSafeFuncAsync(x => x.Text).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(strSearch))
                strFilter += " and " + CommonFunctions.GenerateSearchXPath(strSearch);
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstMentors))
            {
                foreach (XPathNavigator objXmlMentor in _xmlBaseMentorSpiritDataNode.Select(
                             "mentors/mentor[" + strFilter + "]"))
                {
                    if (!await objXmlMentor.RequirementsMetAsync(_objCharacter).ConfigureAwait(false))
                        continue;

                    string strName = objXmlMentor.SelectSingleNodeAndCacheExpression("name")?.Value
                                     ?? await LanguageManager.GetStringAsync("String_Unknown").ConfigureAwait(false);
                    string strId = objXmlMentor.SelectSingleNodeAndCacheExpression("id")?.Value ?? string.Empty;
                    if (strName == _strForceMentor)
                        strForceId = strId;
                    lstMentors.Add(new ListItem(
                                       strId,
                                       objXmlMentor.SelectSingleNodeAndCacheExpression("translate")?.Value ?? strName));
                }

                lstMentors.Sort(CompareListItems.CompareNames);
                string strOldSelected = await lstMentor.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false);
                _blnSkipRefresh = true;
                try
                {
                    await lstMentor.PopulateWithListItemsAsync(lstMentors).ConfigureAwait(false);
                }
                finally
                {
                    _blnSkipRefresh = false;
                }
                await lstMentor.DoThreadSafeAsync(x =>
                {
                    if (!string.IsNullOrEmpty(strOldSelected))
                        x.SelectedValue = strOldSelected;
                    else
                        x.SelectedIndex = -1;
                    if (!string.IsNullOrEmpty(strForceId))
                    {
                        x.SelectedValue = strForceId;
                        x.Enabled = false;
                    }
                }).ConfigureAwait(false);
            }
        }

        private async void OpenSourceFromLabel(object sender, EventArgs e)
        {
            await CommonFunctions.OpenPdfFromControl(sender).ConfigureAwait(false);
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Forced selection for mentor spirit
        /// </summary>
        public string ForcedMentor
        {
            set => _strForceMentor = value;
        }

        /// <summary>
        /// Mentor that was selected in the dialogue.
        /// </summary>
        public string SelectedMentor { get; private set; } = string.Empty;

        /// <summary>
        /// First choice that was selected in the dialogue.
        /// </summary>
        public string Choice1 => _strChoice1;

        /// <summary>
        /// Second choice that was selected in the dialogue.
        /// </summary>
        public string Choice2 => _strChoice2;

        #endregion Properties
    }
}
