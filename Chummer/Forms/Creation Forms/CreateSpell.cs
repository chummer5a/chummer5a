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
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.XPath;

namespace Chummer
{
    public partial class CreateSpell : Form
    {
        private readonly XPathNavigator _objXmlDocument;
        private bool _blnLoading = true;
        private int _intSkipRefresh;
        private readonly Character _objCharacter;
        private Spell _objSpell = null;

        #region Control Events

        public CreateSpell(Character objCharacter)
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            this.UpdateParentForToolTipControls();
            _objXmlDocument = objCharacter.LoadDataXPath("spells.xml");
        }

        private async void CreateSpell_Load(object sender, EventArgs e)
        {
            await lblDV.DoThreadSafeAsync(x => x.Text = 0.ToString(GlobalSettings.CultureInfo)).ConfigureAwait(false);

            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                           out List<ListItem> lstCategory))
            {
                // Populate the list of Spell Categories.
                foreach (XPathNavigator objXmlCategory in _objXmlDocument.SelectAndCacheExpression(
                             "/chummer/categories/category"))
                {
                    string strInnerText = objXmlCategory.Value;
                    lstCategory.Add(new ListItem(strInnerText,
                                                 objXmlCategory.SelectSingleNodeAndCacheExpression("@translate")?.Value
                                                 ?? strInnerText));
                }

                await cboCategory.PopulateWithListItemsAsync(lstCategory).ConfigureAwait(false);
            }

            await cboCategory.DoThreadSafeAsync(x => x.SelectedIndex = 0).ConfigureAwait(false);

            // Populate the list of Spell Types.
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstTypes))
            {
                lstTypes.Add(new ListItem("P", await LanguageManager.GetStringAsync("String_DescPhysical").ConfigureAwait(false)));
                lstTypes.Add(new ListItem("M", await LanguageManager.GetStringAsync("String_DescMana").ConfigureAwait(false)));
                await cboType.PopulateWithListItemsAsync(lstTypes).ConfigureAwait(false);
            }

            await cboType.DoThreadSafeAsync(x => x.SelectedIndex = 0).ConfigureAwait(false);

            // Populate the list of Ranges.
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstRanges))
            {
                lstRanges.Add(new ListItem("T", await LanguageManager.GetStringAsync("String_SpellRangeTouchLong").ConfigureAwait(false)));
                lstRanges.Add(new ListItem("LOS", await LanguageManager.GetStringAsync("String_SpellRangeLineOfSight").ConfigureAwait(false)));
                await cboRange.PopulateWithListItemsAsync(lstRanges).ConfigureAwait(false);
            }

            await cboRange.DoThreadSafeAsync(x => x.SelectedIndex = 0).ConfigureAwait(false);

            // Populate the list of Durations.
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstDurations))
            {
                lstDurations.Add(new ListItem("I", await LanguageManager.GetStringAsync("String_SpellDurationInstantLong").ConfigureAwait(false)));
                lstDurations.Add(new ListItem("P", await LanguageManager.GetStringAsync("String_SpellDurationPermanentLong").ConfigureAwait(false)));
                lstDurations.Add(new ListItem("S", await LanguageManager.GetStringAsync("String_SpellDurationSustainedLong").ConfigureAwait(false)));
                await cboDuration.PopulateWithListItemsAsync(lstDurations).ConfigureAwait(false);
            }

            await cboDuration.DoThreadSafeAsync(x => x.SelectedIndex = 0).ConfigureAwait(false);

            _blnLoading = false;

            await CalculateDrain().ConfigureAwait(false);
        }

        private async void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString()).ConfigureAwait(false) == "Health")
            {
                await chkArea.DoThreadSafeAsync(x =>
                {
                    x.Checked = false;
                    x.Enabled = false;
                }).ConfigureAwait(false);
            }
            else
                await chkArea.DoThreadSafeAsync(x => x.Enabled = true).ConfigureAwait(false);

            await ChangeModifiers().ConfigureAwait(false);
            await CalculateDrain().ConfigureAwait(false);
        }

        private async void cboType_SelectedIndexChanged(object sender, EventArgs e)
        {
            await CalculateDrain().ConfigureAwait(false);
        }

        private async void cboRange_SelectedIndexChanged(object sender, EventArgs e)
        {
            await CalculateDrain().ConfigureAwait(false);
        }

        private async void cboDuration_SelectedIndexChanged(object sender, EventArgs e)
        {
            await CalculateDrain().ConfigureAwait(false);
        }

        private async void chkModifier_CheckedChanged(object sender, EventArgs e)
        {
            await cboType.DoThreadSafeAsync(x => x.Enabled = true).ConfigureAwait(false);
            if (_intSkipRefresh > 0)
                return;

            switch (await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString()).ConfigureAwait(false))
            {
                case "Combat":
                    {
                        // Direct and Indirect cannot be selected at the same time.
                        if (await chkModifier1.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                        {
                            Interlocked.Increment(ref _intSkipRefresh);
                            try
                            {
                                await chkModifier2.DoThreadSafeAsync(x =>
                                {
                                    x.Checked = false;
                                    x.Enabled = false;
                                    x.Enabled = false;
                                }).ConfigureAwait(false);
                                await chkModifier3.DoThreadSafeAsync(x =>
                                {
                                    x.Checked = false;
                                    x.Enabled = false;
                                }).ConfigureAwait(false);
                                await nudNumberOfEffects.DoThreadSafeAsync(x => x.Enabled = false)
                                                        .ConfigureAwait(false);
                            }
                            finally
                            {
                                Interlocked.Decrement(ref _intSkipRefresh);
                            }
                        }
                        else
                        {
                            await chkModifier2.DoThreadSafeAsync(x => x.Enabled = true).ConfigureAwait(false);
                            await chkModifier3.DoThreadSafeAsync(x => x.Enabled = true).ConfigureAwait(false);
                            await nudNumberOfEffects.DoThreadSafeAsync(x => x.Enabled = true).ConfigureAwait(false);
                        }

                        // Indirect Combat Spells must always be physical. Direct and Indirect cannot be selected at the same time.
                        if (await chkModifier2.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                        {
                            Interlocked.Increment(ref _intSkipRefresh);
                            try
                            {
                                await chkModifier1.DoThreadSafeAsync(x =>
                                {
                                    x.Checked = false;
                                    x.Enabled = false;
                                }).ConfigureAwait(false);
                                await cboType.DoThreadSafeAsync(x =>
                                {
                                    x.SelectedValue = "P";
                                    x.Enabled = false;
                                }).ConfigureAwait(false);
                            }
                            finally
                            {
                                Interlocked.Decrement(ref _intSkipRefresh);
                            }
                        }
                        else
                        {
                            await chkModifier1.DoThreadSafeAsync(x => x.Enabled = true).ConfigureAwait(false);
                        }

                        // Elemental effect spells must be Indirect (and consequently physical as well).
                        if (await chkModifier3.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                        {
                            await chkModifier2.DoThreadSafeAsync(x => x.Checked = true).ConfigureAwait(false);
                        }

                        // Physical damage and Stun damage cannot be selected at the same time.
                        if (await chkModifier4.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                        {
                            Interlocked.Increment(ref _intSkipRefresh);
                            try
                            {
                                await chkModifier5.DoThreadSafeAsync(x =>
                                {
                                    x.Checked = false;
                                    x.Enabled = false;
                                }).ConfigureAwait(false);
                            }
                            finally
                            {
                                Interlocked.Decrement(ref _intSkipRefresh);
                            }
                        }
                        else
                        {
                            await chkModifier5.DoThreadSafeAsync(x => x.Enabled = true).ConfigureAwait(false);
                        }
                        if (await chkModifier5.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                        {
                            Interlocked.Increment(ref _intSkipRefresh);
                            try
                            {
                                await chkModifier4.DoThreadSafeAsync(x =>
                                {
                                    x.Checked = false;
                                    x.Enabled = false;
                                }).ConfigureAwait(false);
                            }
                            finally
                            {
                                Interlocked.Decrement(ref _intSkipRefresh);
                            }
                        }
                        else
                        {
                            await chkModifier4.DoThreadSafeAsync(x => x.Enabled = true).ConfigureAwait(false);
                        }

                        break;
                    }
                case "Detection":
                    {
                        // Directional, and Area cannot be selected at the same time.
                        if (await chkModifier1.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                        {
                            Interlocked.Increment(ref _intSkipRefresh);
                            try
                            {
                                await chkModifier2.DoThreadSafeAsync(x =>
                                {
                                    x.Checked = false;
                                    x.Enabled = false;
                                }).ConfigureAwait(false);
                            }
                            finally
                            {
                                Interlocked.Decrement(ref _intSkipRefresh);
                            }
                        }
                        if (await chkModifier2.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                        {
                            Interlocked.Increment(ref _intSkipRefresh);
                            try
                            {
                                await chkModifier1.DoThreadSafeAsync(x =>
                                {
                                    x.Checked = false;
                                    x.Enabled = false;
                                }).ConfigureAwait(false);
                            }
                            finally
                            {
                                Interlocked.Decrement(ref _intSkipRefresh);
                            }
                        }

                        // Active and Passive cannot be selected at the same time.
                        if (await chkModifier4.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                        {
                            Interlocked.Increment(ref _intSkipRefresh);
                            try
                            {
                                await chkModifier5.DoThreadSafeAsync(x =>
                                {
                                    x.Checked = false;
                                    x.Enabled = false;
                                }).ConfigureAwait(false);
                            }
                            finally
                            {
                                Interlocked.Decrement(ref _intSkipRefresh);
                            }
                        }
                        else
                        {
                            await chkModifier5.DoThreadSafeAsync(x => x.Enabled = true).ConfigureAwait(false);
                        }

                        if (await chkModifier5.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                        {
                            Interlocked.Increment(ref _intSkipRefresh);
                            try
                            {
                                await chkModifier4.DoThreadSafeAsync(x =>
                                {
                                    x.Checked = false;
                                    x.Enabled = false;
                                }).ConfigureAwait(false);
                            }
                            finally
                            {
                                Interlocked.Decrement(ref _intSkipRefresh);
                            }
                        }
                        else
                        {
                            await chkModifier4.DoThreadSafeAsync(x => x.Enabled = true).ConfigureAwait(false);
                        }

                        // If Extended Area is selected, Area must also be selected.
                        if (await chkModifier4.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                        {
                            await chkModifier1.DoThreadSafeAsync(x => x.Checked = false).ConfigureAwait(false);
                            await chkModifier3.DoThreadSafeAsync(x => x.Checked = false).ConfigureAwait(false);
                            await chkModifier2.DoThreadSafeAsync(x => x.Checked = true).ConfigureAwait(false);
                        }

                        break;
                    }
                case "Health":
                    // Nothing special for Health Spells.
                    break;

                case "Illusion":
                    {
                        // Obvious and Realistic cannot be selected at the same time.
                        if (await chkModifier1.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                        {
                            Interlocked.Increment(ref _intSkipRefresh);
                            try
                            {
                                await chkModifier2.DoThreadSafeAsync(x =>
                                {
                                    x.Checked = false;
                                    x.Enabled = false;
                                }).ConfigureAwait(false);
                            }
                            finally
                            {
                                Interlocked.Decrement(ref _intSkipRefresh);
                            }
                        }
                        else
                        {
                            await chkModifier2.DoThreadSafeAsync(x => x.Enabled = true).ConfigureAwait(false);
                        }

                        if (await chkModifier2.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                        {
                            Interlocked.Increment(ref _intSkipRefresh);
                            try
                            {
                                await chkModifier1.DoThreadSafeAsync(x =>
                                {
                                    x.Checked = false;
                                    x.Enabled = false;
                                }).ConfigureAwait(false);
                            }
                            finally
                            {
                                Interlocked.Decrement(ref _intSkipRefresh);
                            }
                        }
                        else
                        {
                            await chkModifier1.DoThreadSafeAsync(x => x.Enabled = true).ConfigureAwait(false);
                        }

                        // Single-Sense and Multi-Sense cannot be selected at the same time.
                        if (await chkModifier3.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                        {
                            Interlocked.Increment(ref _intSkipRefresh);
                            try
                            {
                                await chkModifier4.DoThreadSafeAsync(x =>
                                {
                                    x.Checked = false;
                                    x.Enabled = false;
                                }).ConfigureAwait(false);
                            }
                            finally
                            {
                                Interlocked.Decrement(ref _intSkipRefresh);
                            }
                        }
                        else
                        {
                            await chkModifier4.DoThreadSafeAsync(x => x.Enabled = true).ConfigureAwait(false);
                        }
                        if (await chkModifier4.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                        {
                            Interlocked.Increment(ref _intSkipRefresh);
                            try
                            {
                                await chkModifier3.DoThreadSafeAsync(x =>
                                {
                                    x.Checked = false;
                                    x.Enabled = false;
                                }).ConfigureAwait(false);
                            }
                            finally
                            {
                                Interlocked.Decrement(ref _intSkipRefresh);
                            }
                        }
                        else
                        {
                            await chkModifier3.DoThreadSafeAsync(x => x.Enabled = true).ConfigureAwait(false);
                        }

                        break;
                    }
                case "Manipulation":
                    {
                        // Environmental, Mental, and Physical cannot be selected at the same time.
                        if (await chkModifier1.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                        {
                            Interlocked.Increment(ref _intSkipRefresh);
                            try
                            {
                                await chkModifier2.DoThreadSafeAsync(x =>
                                {
                                    x.Checked = false;
                                    x.Enabled = false;
                                }).ConfigureAwait(false);
                                await chkModifier3.DoThreadSafeAsync(x =>
                                {
                                    x.Checked = false;
                                    x.Enabled = false;
                                }).ConfigureAwait(false);
                            }
                            finally
                            {
                                Interlocked.Decrement(ref _intSkipRefresh);
                            }
                        }
                        if (await chkModifier2.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                        {
                            Interlocked.Increment(ref _intSkipRefresh);
                            try
                            {
                                await chkModifier1.DoThreadSafeAsync(x =>
                                {
                                    x.Checked = false;
                                    x.Enabled = false;
                                }).ConfigureAwait(false);
                                await chkModifier3.DoThreadSafeAsync(x =>
                                {
                                    x.Checked = false;
                                    x.Enabled = false;
                                }).ConfigureAwait(false);
                            }
                            finally
                            {
                                Interlocked.Decrement(ref _intSkipRefresh);
                            }
                        }
                        if (await chkModifier3.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                        {
                            Interlocked.Increment(ref _intSkipRefresh);
                            try
                            {
                                await chkModifier1.DoThreadSafeAsync(x =>
                                {
                                    x.Checked = false;
                                    x.Enabled = false;
                                }).ConfigureAwait(false);
                                await chkModifier2.DoThreadSafeAsync(x =>
                                {
                                    x.Checked = false;
                                    x.Enabled = false;
                                }).ConfigureAwait(false);
                            }
                            finally
                            {
                                Interlocked.Decrement(ref _intSkipRefresh);
                            }
                        }
                        await chkModifier1.DoThreadSafeAsync(x => x.Enabled = !chkModifier2.Checked && !chkModifier3.Checked).ConfigureAwait(false);
                        await chkModifier2.DoThreadSafeAsync(x => x.Enabled = !chkModifier1.Checked && !chkModifier3.Checked).ConfigureAwait(false);
                        await chkModifier3.DoThreadSafeAsync(x => x.Enabled = !chkModifier1.Checked && !chkModifier2.Checked).ConfigureAwait(false);

                        // Minor Change and Major Change cannot be selected at the same time.
                        if (await chkModifier4.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                        {
                            Interlocked.Increment(ref _intSkipRefresh);
                            try
                            {
                                await chkModifier5.DoThreadSafeAsync(x =>
                                {
                                    x.Checked = false;
                                    x.Enabled = false;
                                }).ConfigureAwait(false);
                            }
                            finally
                            {
                                Interlocked.Decrement(ref _intSkipRefresh);
                            }
                        }
                        else
                        {
                            await chkModifier5.DoThreadSafeAsync(x => x.Enabled = true).ConfigureAwait(false);
                        }
                        if (await chkModifier5.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
                        {
                            Interlocked.Increment(ref _intSkipRefresh);
                            try
                            {
                                await chkModifier4.DoThreadSafeAsync(x =>
                                {
                                    x.Checked = false;
                                    x.Enabled = false;
                                }).ConfigureAwait(false);
                            }
                            finally
                            {
                                Interlocked.Decrement(ref _intSkipRefresh);
                            }
                        }
                        else
                        {
                            await chkModifier4.DoThreadSafeAsync(x => x.Enabled = true).ConfigureAwait(false);
                        }

                        break;
                    }
            }

            await CalculateDrain().ConfigureAwait(false);
        }

        private async void chkRestricted_CheckedChanged(object sender, EventArgs e)
        {
            await chkVeryRestricted.DoThreadSafeAsync(x => x.Enabled = !chkRestricted.Checked).ConfigureAwait(false);
            await CalculateDrain().ConfigureAwait(false);
            await txtRestriction.DoThreadSafeAsync(x =>
            {
                x.Enabled = chkRestricted.Checked || chkVeryRestricted.Checked;
                if (!x.Enabled)
                    x.Text = string.Empty;
            }).ConfigureAwait(false);
        }

        private async void chkVeryRestricted_CheckedChanged(object sender, EventArgs e)
        {
            await chkRestricted.DoThreadSafeAsync(x => x.Enabled = !chkVeryRestricted.Checked).ConfigureAwait(false);
            await CalculateDrain().ConfigureAwait(false);
            await txtRestriction.DoThreadSafeAsync(x =>
            {
                x.Enabled = chkRestricted.Checked || chkVeryRestricted.Checked;
                if (!x.Enabled)
                    x.Text = string.Empty;
            }).ConfigureAwait(false);
        }

        private async void nudNumberOfEffects_ValueChanged(object sender, EventArgs e)
        {
            await CalculateDrain().ConfigureAwait(false);
        }

        private async void chkArea_CheckedChanged(object sender, EventArgs e)
        {
            await CalculateDrain().ConfigureAwait(false);
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            await AcceptForm().ConfigureAwait(false);
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        #endregion Control Events

        #region Methods

        /// <summary>
        /// Re-calculate the Drain modifiers based on the currently-selected options.
        /// </summary>
        private async Task ChangeModifiers(CancellationToken token = default)
        {
            foreach (Control objControl in await flpModifiers.DoThreadSafeFuncAsync(x => x.Controls, token: token).ConfigureAwait(false))
            {
                switch (objControl)
                {
                    case ColorableCheckBox chkCheckbox:
                    {
                        await chkCheckbox.DoThreadSafeAsync(x =>
                        {
                            x.Enabled = true;
                            x.Checked = false;
                            x.Text = string.Empty;
                            x.Tag = string.Empty;
                            x.Visible = false;
                        }, token: token).ConfigureAwait(false);
                        break;
                    }
                    case Panel panChild:
                    {
                        await panChild.DoThreadSafeAsync(x =>
                        {
                            foreach (CheckBox chkCheckbox in x.Controls.OfType<CheckBox>())
                            {
                                chkCheckbox.Enabled = true;
                                chkCheckbox.Checked = false;
                                chkCheckbox.Text = string.Empty;
                                chkCheckbox.Tag = string.Empty;
                                chkCheckbox.Visible = false;
                            }
                        }, token: token).ConfigureAwait(false);
                        break;
                    }
                }
            }

            await nudNumberOfEffects.DoThreadSafeAsync(x =>
            {
                x.Visible = false;
                x.Enabled = true;
            }, token: token).ConfigureAwait(false);

            switch (await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token: token).ConfigureAwait(false))
            {
                case "Detection":
                {
                    string strText1 = await LanguageManager.GetStringAsync("Checkbox_DetectionSpell1", token: token).ConfigureAwait(false);
                    await chkModifier1.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+0";
                        x.Text = strText1;
                    }, token: token).ConfigureAwait(false);
                    string strText2 = await LanguageManager.GetStringAsync("Checkbox_DetectionSpell2", token: token).ConfigureAwait(false);
                    await chkModifier2.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+0";
                        x.Text = strText2;
                    }, token: token).ConfigureAwait(false);
                    string strText3 = await LanguageManager.GetStringAsync("Checkbox_DetectionSpell3", token: token).ConfigureAwait(false);
                    await chkModifier3.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+0";
                        x.Text = strText3;
                    }, token: token).ConfigureAwait(false);
                    string strText4 = await LanguageManager.GetStringAsync("Checkbox_DetectionSpell4", token: token).ConfigureAwait(false);
                    await chkModifier4.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+0";
                        x.Text = strText4;
                    }, token: token).ConfigureAwait(false);
                    string strText5 = await LanguageManager.GetStringAsync("Checkbox_DetectionSpell5", token: token).ConfigureAwait(false);
                    await chkModifier5.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+0";
                        x.Text = strText5;
                    }, token: token).ConfigureAwait(false);
                    string strText6 = await LanguageManager.GetStringAsync("Checkbox_DetectionSpell6", token: token).ConfigureAwait(false);
                    await chkModifier6.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+0";
                        x.Text = strText6;
                    }, token: token).ConfigureAwait(false);
                    string strText7 = await LanguageManager.GetStringAsync("Checkbox_DetectionSpell7", token: token).ConfigureAwait(false);
                    await chkModifier7.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+1";
                        x.Text = strText7;
                    }, token: token).ConfigureAwait(false);
                    string strText8 = await LanguageManager.GetStringAsync("Checkbox_DetectionSpell8", token: token).ConfigureAwait(false);
                    await chkModifier8.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+1";
                        x.Text = strText8;
                    }, token: token).ConfigureAwait(false);
                    string strText9 = await LanguageManager.GetStringAsync("Checkbox_DetectionSpell9", token: token).ConfigureAwait(false);
                    await chkModifier9.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+2";
                        x.Text = strText9;
                    }, token: token).ConfigureAwait(false);
                    string strText10 = await LanguageManager.GetStringAsync("Checkbox_DetectionSpell10", token: token).ConfigureAwait(false);
                    await chkModifier10.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+4";
                        x.Text = strText10;
                    }, token: token).ConfigureAwait(false);
                    string strText11 = await LanguageManager.GetStringAsync("Checkbox_DetectionSpell11", token: token).ConfigureAwait(false);
                    await chkModifier11.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+1";
                        x.Text = strText11;
                    }, token: token).ConfigureAwait(false);
                    string strText12 = await LanguageManager.GetStringAsync("Checkbox_DetectionSpell12", token: token).ConfigureAwait(false);
                    await chkModifier12.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+2";
                        x.Text = strText12;
                    }, token: token).ConfigureAwait(false);
                    string strText13 = await LanguageManager.GetStringAsync("Checkbox_DetectionSpell13", token: token).ConfigureAwait(false);
                    await chkModifier13.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+4";
                        x.Text = strText13;
                    }, token: token).ConfigureAwait(false);
                    string strText14 = await LanguageManager.GetStringAsync("Checkbox_DetectionSpell14", token: token).ConfigureAwait(false);
                    await chkModifier14.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+2";
                        x.Text = strText14;
                    }, token: token).ConfigureAwait(false);
                    break;
                }
                case "Health":
                {
                    string strText1 = await LanguageManager.GetStringAsync("Checkbox_HealthSpell1", token: token).ConfigureAwait(false);
                    await chkModifier1.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+0";
                        x.Text = strText1;
                    }, token: token).ConfigureAwait(false);
                    string strText2 = await LanguageManager.GetStringAsync("Checkbox_HealthSpell2", token: token).ConfigureAwait(false);
                    await chkModifier2.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+4";
                        x.Text = strText2;
                    }, token: token).ConfigureAwait(false);
                    string strText3 = await LanguageManager.GetStringAsync("Checkbox_HealthSpell3", token: token).ConfigureAwait(false);
                    await chkModifier3.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "-2";
                        x.Text = strText3;
                    }, token: token).ConfigureAwait(false);
                    string strText4 = await LanguageManager.GetStringAsync("Checkbox_HealthSpell4", token: token).ConfigureAwait(false);
                    await chkModifier4.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+2";
                        x.Text = strText4;
                    }, token: token).ConfigureAwait(false);
                    string strText5 = await LanguageManager.GetStringAsync("Checkbox_HealthSpell5", token: token).ConfigureAwait(false);
                    await chkModifier5.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "-2";
                        x.Text = strText5;
                    }, token: token).ConfigureAwait(false);
                    break;
                }
                case "Illusion":
                {
                    string strText1 = await LanguageManager.GetStringAsync("Checkbox_IllusionSpell1", token: token).ConfigureAwait(false);
                    await chkModifier1.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "-1";
                        x.Text = strText1;
                    }, token: token).ConfigureAwait(false);
                    string strText2 = await LanguageManager.GetStringAsync("Checkbox_IllusionSpell2", token: token).ConfigureAwait(false);
                    await chkModifier2.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+0";
                        x.Text = strText2;
                    }, token: token).ConfigureAwait(false);
                    string strText3 = await LanguageManager.GetStringAsync("Checkbox_IllusionSpell3", token: token).ConfigureAwait(false);
                    await chkModifier3.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "-2";
                        x.Text = strText3;
                    }, token: token).ConfigureAwait(false);
                    string strText4 = await LanguageManager.GetStringAsync("Checkbox_IllusionSpell4", token: token).ConfigureAwait(false);
                    await chkModifier4.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+0";
                        x.Text = strText4;
                    }, token: token).ConfigureAwait(false);
                    string strText5 = await LanguageManager.GetStringAsync("Checkbox_IllusionSpell5", token: token).ConfigureAwait(false);
                    await chkModifier5.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+2";
                        x.Text = strText5;
                    }, token: token).ConfigureAwait(false);
                    break;
                }
                case "Manipulation":
                {
                    string strText1 = await LanguageManager.GetStringAsync("Checkbox_ManipulationSpell1", token: token).ConfigureAwait(false);
                    await chkModifier1.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "-2";
                        x.Text = strText1;
                    }, token: token).ConfigureAwait(false);
                    string strText2 = await LanguageManager.GetStringAsync("Checkbox_ManipulationSpell2", token: token).ConfigureAwait(false);
                    await chkModifier2.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+0";
                        x.Text = strText2;
                    }, token: token).ConfigureAwait(false);
                    string strText3 = await LanguageManager.GetStringAsync("Checkbox_ManipulationSpell3", token: token).ConfigureAwait(false);
                    await chkModifier3.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+0";
                        x.Text = strText3;
                    }, token: token).ConfigureAwait(false);
                    string strText4 = await LanguageManager.GetStringAsync("Checkbox_ManipulationSpell4", token: token).ConfigureAwait(false);
                    await chkModifier4.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+0";
                        x.Text = strText4;
                    }, token: token).ConfigureAwait(false);
                    string strText5 = await LanguageManager.GetStringAsync("Checkbox_ManipulationSpell5", token: token).ConfigureAwait(false);
                    await chkModifier5.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+2";
                        x.Text = strText5;
                    }, token: token).ConfigureAwait(false);
                    string strText6 = await LanguageManager.GetStringAsync("Checkbox_ManipulationSpell6", token: token).ConfigureAwait(false);
                    await chkModifier6.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+2";
                        x.Text = strText6;
                    }, token: token).ConfigureAwait(false);
                    int intTop = await chkModifier6.DoThreadSafeFuncAsync(x => x.Top, token: token).ConfigureAwait(false);
                    await nudNumberOfEffects.DoThreadSafeAsync(x =>
                    {
                        x.Visible = true;
                        x.Top = intTop - 1;
                    }, token: token).ConfigureAwait(false);
                    break;
                }
                default:
                {
                    // Combat.
                    string strText1 = await LanguageManager.GetStringAsync("Checkbox_CombatSpell1", token: token).ConfigureAwait(false);
                    await chkModifier1.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+0";
                        x.Text = strText1;
                    }, token: token).ConfigureAwait(false);
                    string strText2 = await LanguageManager.GetStringAsync("Checkbox_CombatSpell2", token: token).ConfigureAwait(false);
                    await chkModifier2.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+0";
                        x.Text = strText2;
                    }, token: token).ConfigureAwait(false);
                    string strText3 = await LanguageManager.GetStringAsync("Checkbox_CombatSpell3", token: token).ConfigureAwait(false);
                    await chkModifier3.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+2";
                        x.Text = strText3;
                    }, token: token).ConfigureAwait(false);
                    string strText4 = await LanguageManager.GetStringAsync("Checkbox_CombatSpell4", token: token).ConfigureAwait(false);
                    await chkModifier4.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "+0";
                        x.Text = strText4;
                    }, token: token).ConfigureAwait(false);
                    string strText5 = await LanguageManager.GetStringAsync("Checkbox_CombatSpell5", token: token).ConfigureAwait(false);
                    await chkModifier5.DoThreadSafeAsync(x =>
                    {
                        x.Tag = "-1";
                        x.Text = strText5;
                    }, token: token).ConfigureAwait(false);
                    int intTop = await chkModifier3.DoThreadSafeFuncAsync(x => x.Top, token: token).ConfigureAwait(false);
                    await nudNumberOfEffects.DoThreadSafeAsync(x =>
                    {
                        x.Visible = true;
                        x.Top = intTop - 1;
                    }, token: token).ConfigureAwait(false);
                    break;
                }
            }

            string strCheckBoxFormat = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false) + "({0})";
            foreach (Control objControl in await flpModifiers.DoThreadSafeFuncAsync(x => x.Controls, token: token).ConfigureAwait(false))
            {
                switch (objControl)
                {
                    case CheckBox chkCheckbox:
                    {
                        await chkCheckbox.DoThreadSafeAsync(x =>
                        {
                            if (!string.IsNullOrEmpty(x.Text))
                            {
                                x.Visible = true;
                                x.Text += string.Format(GlobalSettings.CultureInfo, strCheckBoxFormat, x.Tag);
                            }
                        }, token: token).ConfigureAwait(false);
                        break;
                    }
                    case Panel pnlControl:
                    {
                        await pnlControl.DoThreadSafeAsync(x =>
                        {
                            foreach (CheckBox chkInnerCheckbox in x.Controls.OfType<CheckBox>())
                            {
                                if (string.IsNullOrEmpty(chkInnerCheckbox.Text))
                                    continue;
                                chkInnerCheckbox.Visible = true;
                                chkInnerCheckbox.Text += string.Format(GlobalSettings.CultureInfo, strCheckBoxFormat, chkInnerCheckbox.Tag);
                            }
                        }, token: token).ConfigureAwait(false);
                        break;
                    }
                }
            }

            if (await nudNumberOfEffects.DoThreadSafeFuncAsync(x => x.Visible, token: token).ConfigureAwait(false))
            {
                switch (await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token: token).ConfigureAwait(false))
                {
                    case "Combat":
                    {
                        int intBase = await chkModifier3.DoThreadSafeFuncAsync(x => x.Left + x.Width, token: token).ConfigureAwait(false);
                        await nudNumberOfEffects.DoThreadSafeAsync(x => x.Left = intBase + 6, token: token).ConfigureAwait(false);
                        break;
                    }
                    case "Manipulation":
                    {
                        int intBase = await chkModifier6.DoThreadSafeFuncAsync(x => x.Left + x.Width, token: token).ConfigureAwait(false);
                        await nudNumberOfEffects.DoThreadSafeAsync(x => x.Left = intBase + 6, token: token).ConfigureAwait(false);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Calculate the Spell's Drain Value based on the currently-selected options.
        /// </summary>
        private async Task<string> CalculateDrain(CancellationToken token = default)
        {
            if (_blnLoading)
                return string.Empty;

            int intDV = 0;

            // Type DV.
            if (await cboType.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token: token).ConfigureAwait(false) != "M")
                ++intDV;

            // Range DV.
            if (await cboRange.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token: token).ConfigureAwait(false) == "T")
                intDV -= 2;

            if (chkArea.Checked)
                intDV += 2;

            // Restriction DV.
            if (await chkRestricted.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                --intDV;
            if (await chkVeryRestricted.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                intDV -= 2;

            string strCategory = await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token: token).ConfigureAwait(false);

            // Duration DV.
            // Curative Health Spells do not have a modifier for Permanent duration.
            if (await cboDuration.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token: token).ConfigureAwait(false) == "P" && (strCategory != "Health" || !await chkModifier1.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false)))
                intDV += 2;

            int intNumberOfEffects = await nudNumberOfEffects.DoThreadSafeFuncAsync(x => x.ValueAsInt, token: token).ConfigureAwait(false);

            // Include any checked modifiers.
            foreach (CheckBox chkModifier in await flpModifiers.DoThreadSafeFuncAsync(x => x.Controls.OfType<CheckBox>(), token: token).ConfigureAwait(false))
            {
                await chkModifier.DoThreadSafeAsync(x =>
                {
                    if (x.Visible && x.Checked && int.TryParse(x.Tag.ToString(), NumberStyles.Integer,
                                GlobalSettings.InvariantCultureInfo, out int intDummy))
                    {
                        if (x == chkModifier3 && strCategory == "Combat")
                            intDV += intDummy * intNumberOfEffects;
                        else if (x == chkModifier6 && strCategory == "Manipulation")
                            intDV += intDummy * intNumberOfEffects;
                        else
                            intDV += intDummy;
                    }
                }, token: token).ConfigureAwait(false);
            }
            foreach (Panel panChild in await flpModifiers.DoThreadSafeFuncAsync(x => x.Controls.OfType<Panel>(), token: token).ConfigureAwait(false))
            {
                foreach (CheckBox chkModifier in await panChild.DoThreadSafeFuncAsync(x => x.Controls.OfType<CheckBox>(), token: token).ConfigureAwait(false))
                {
                    await chkModifier.DoThreadSafeAsync(x =>
                    {
                        if (x.Visible && x.Checked && int.TryParse(x.Tag.ToString(), NumberStyles.Integer,
                                GlobalSettings.InvariantCultureInfo, out int intDummy))
                        {
                            if (x == chkModifier3 && strCategory == "Combat")
                                intDV += intDummy * intNumberOfEffects;
                            else if (x == chkModifier6 && strCategory == "Manipulation")
                                intDV += intDummy * intNumberOfEffects;
                            else
                                intDV += intDummy;
                        }
                    }, token: token).ConfigureAwait(false);
                }
            }

            string strBase;
            if (strCategory == "Health" && await chkModifier1.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
            {
                // Health Spells use (Damage Value) as their base.
                strBase = '(' + await LanguageManager.GetStringAsync("String_SpellDamageValue", token: token).ConfigureAwait(false) + ')';
            }
            else
            {
                // All other spells use (F/2) as their base.
                strBase = "(F/2)";
            }

            string strDV = intDV.ToString(GlobalSettings.InvariantCultureInfo);
            if (intDV > 0)
                strDV = '+' + strDV;
            if (intDV == 0)
                strDV = string.Empty;
            string strText = await (strBase + strDV).Replace('/', '÷').Replace('*', '×')
                                                    .CheapReplaceAsync(
                                                        "F", () => LanguageManager.GetStringAsync("String_SpellForce", token: token), token: token)
                                                    .CheapReplaceAsync("Damage Value",
                                                                       () => LanguageManager.GetStringAsync(
                                                                           "String_SpellDamageValue", token: token), token: token).ConfigureAwait(false);
            await lblDV.DoThreadSafeAsync(x => x.Text = strText, token: token).ConfigureAwait(false);

            return strBase + strDV;
        }

        /// <summary>
        /// Accept the values of the form.
        /// </summary>
        private async Task AcceptForm(CancellationToken token = default)
        {
            string strMessage = string.Empty;
            // Make sure a name has been provided.
            if (string.IsNullOrWhiteSpace(await txtName.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false)))
            {
                if (!string.IsNullOrEmpty(strMessage))
                    strMessage += Environment.NewLine;
                strMessage += await LanguageManager.GetStringAsync("Message_SpellName", token: token).ConfigureAwait(false);
            }

            // Make sure a Restricted value if the field is enabled.
            if (txtRestriction.Enabled && string.IsNullOrWhiteSpace(await txtRestriction.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false)))
            {
                if (!string.IsNullOrEmpty(strMessage))
                    strMessage += Environment.NewLine;
                strMessage += await LanguageManager.GetStringAsync("Message_SpellRestricted", token: token).ConfigureAwait(false);
            }

            switch (await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token: token).ConfigureAwait(false))
            {
                // Make sure the Spell has met all of its requirements.
                case "Combat":
                    {
                        // Either Direct or Indirect must be selected.
                        if (!await chkModifier1.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false) && !await chkModifier2.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                        {
                            if (!string.IsNullOrEmpty(strMessage))
                                strMessage += Environment.NewLine;
                            strMessage += await LanguageManager.GetStringAsync("Message_CombatSpellRequirement1", token: token).ConfigureAwait(false);
                        }

                        // Either Physical damage or Stun damage must be selected.
                        if (!await chkModifier4.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false) && !await chkModifier5.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                        {
                            if (!string.IsNullOrEmpty(strMessage))
                                strMessage += Environment.NewLine;
                            strMessage += await LanguageManager.GetStringAsync("Message_CombatSpellRequirement2", token: token).ConfigureAwait(false);
                        }

                        break;
                    }
                case "Detection":
                    {
                        // Either Directional, Area, or Psychic must be selected.
                        if (!await chkModifier1.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false) && !await chkModifier2.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false) && !await chkModifier3.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                        {
                            if (!string.IsNullOrEmpty(strMessage))
                                strMessage += Environment.NewLine;
                            strMessage += await LanguageManager.GetStringAsync("Message_DetectionSpellRequirement1", token: token).ConfigureAwait(false);
                        }

                        // Either Active or Passive must be selected.
                        if (!await chkModifier4.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false) && !await chkModifier5.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                        {
                            if (!string.IsNullOrEmpty(strMessage))
                                strMessage += Environment.NewLine;
                            strMessage += await LanguageManager.GetStringAsync("Message_DetectionSpellRequirement2", token: token).ConfigureAwait(false);
                        }

                        break;
                    }
                case "Health":
                    // Nothing special.
                    break;

                case "Illusion":
                    {
                        // Either Obvious or Realistic must be selected.
                        if (!await chkModifier1.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false) && !await chkModifier2.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                        {
                            if (!string.IsNullOrEmpty(strMessage))
                                strMessage += Environment.NewLine;
                            strMessage += await LanguageManager.GetStringAsync("Message_IllusionSpellRequirement1", token: token).ConfigureAwait(false);
                        }

                        // Either Single-Sense or Multi-Sense must be selected.
                        if (!await chkModifier3.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false) && !await chkModifier4.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                        {
                            if (!string.IsNullOrEmpty(strMessage))
                                strMessage += Environment.NewLine;
                            strMessage += await LanguageManager.GetStringAsync("Message_IllusionSpellRequirement2", token: token).ConfigureAwait(false);
                        }

                        break;
                    }
                case "Manipulation":
                    {
                        // Either Environmental, Mental, or Physical must be selected.
                        if (!await chkModifier1.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false) && !await chkModifier2.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false) && !await chkModifier3.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                        {
                            if (!string.IsNullOrEmpty(strMessage))
                                strMessage += Environment.NewLine;
                            strMessage += await LanguageManager.GetStringAsync("Message_ManipulationSpellRequirement1", token: token).ConfigureAwait(false);
                        }

                        // Either Minor Change or Major Change must be selected.
                        if (!await chkModifier4.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false) && !await chkModifier5.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                        {
                            if (!string.IsNullOrEmpty(strMessage))
                                strMessage += Environment.NewLine;
                            strMessage += await LanguageManager.GetStringAsync("Message_ManipulationSpellRequirement2", token: token).ConfigureAwait(false);
                        }

                        break;
                    }
            }

            // Show the message if necessary.
            if (!string.IsNullOrEmpty(strMessage))
            {
                await Program.ShowScrollableMessageBoxAsync(this, strMessage, await LanguageManager.GetStringAsync("Title_CreateSpell", token: token).ConfigureAwait(false), MessageBoxButtons.OK, MessageBoxIcon.Error, token: token).ConfigureAwait(false);
                return;
            }

            string strRange = await cboRange.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token: token).ConfigureAwait(false);
            if (await chkArea.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                strRange += "(A)";

            // If we're made it this far, everything is OK, so create the Spell.
            string strDescriptors = string.Empty;
            switch (await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token: token).ConfigureAwait(false))
            {
                case "Detection":
                    if (await chkModifier4.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                        strDescriptors += "Active, ";
                    if (await chkModifier5.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                        strDescriptors += "Passive, ";
                    if (await chkModifier1.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                        strDescriptors += "Directional, ";
                    if (await chkModifier3.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                        strDescriptors += "Psychic, ";
                    if (await chkModifier2.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                    {
                        if (!await chkModifier14.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                            strDescriptors += "Area, ";
                        else
                            strDescriptors += "Extended Area, ";
                    }
                    break;

                case "Health":
                    if (await chkModifier4.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                        strDescriptors += "Negative, ";
                    break;

                case "Illusion":
                    if (await chkModifier1.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                        strDescriptors += "Obvious, ";
                    if (await chkModifier2.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                        strDescriptors += "Realistic, ";
                    if (await chkModifier3.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                        strDescriptors += "Single-Sense, ";
                    if (await chkModifier4.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                        strDescriptors += "Multi-Sense, ";
                    if (await chkArea.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                        strDescriptors += "Area, ";
                    break;

                case "Manipulation":
                    if (await chkModifier1.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                        strDescriptors += "Environmental, ";
                    if (await chkModifier2.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                        strDescriptors += "Mental, ";
                    if (await chkModifier3.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                        strDescriptors += "Physical, ";
                    if (await chkArea.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                        strDescriptors += "Area, ";
                    break;

                default:
                    // Combat.
                    if (await chkModifier1.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                        strDescriptors += "Direct, ";
                    if (await chkModifier2.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                        strDescriptors += "Indirect, ";
                    if ((await cboRange.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token: token).ConfigureAwait(false)).Contains("(A)"))
                        strDescriptors += "Area, ";
                    if (await chkModifier3.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false))
                        strDescriptors += "Elemental, ";
                    break;
            }

            // Remove the trailing ", " from the Descriptors string.
            if (!string.IsNullOrEmpty(strDescriptors))
                strDescriptors = strDescriptors.Substring(0, strDescriptors.Length - 2);

            Spell objSpell = new Spell(_objCharacter);
            try
            {
                objSpell.Name = await txtName.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false);
                objSpell.Source = "SM";
                objSpell.Page = "159";
                objSpell.Category = await cboCategory.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token: token).ConfigureAwait(false);
                objSpell.Descriptors = strDescriptors;
                objSpell.Range = strRange;
                objSpell.Type = await cboType.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token: token).ConfigureAwait(false);
                objSpell.Limited = await chkLimited.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false);
                if (objSpell.Category == "Combat")
                    objSpell.Damage = await chkModifier4.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false) ? "P" : "S";
                objSpell.DvBase = await CalculateDrain(token).ConfigureAwait(false);
                string strExtra = await txtRestriction.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(strExtra))
                    objSpell.Extra = strExtra;
                objSpell.Duration = await cboDuration.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token: token).ConfigureAwait(false);
            }
            catch
            {
                await objSpell.RemoveAsync(false, CancellationToken.None).ConfigureAwait(false);
                throw;
            }
            _objSpell = objSpell;

            await this.DoThreadSafeAsync(x =>
            {
                x.DialogResult = DialogResult.OK;
                x.Close();
            }, token: token).ConfigureAwait(false);
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Spell that was created in the dialogue.
        /// </summary>
        public Spell SelectedSpell => _objSpell;

        #endregion Properties
    }
}
