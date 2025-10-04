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

namespace Chummer
{
    public sealed partial class SelectLimitModifier : Form
    {
        private string _strReturnName = string.Empty;
        private int _intBonus = 1;
        private string _strCondition = string.Empty;
        private string _strLimitType = string.Empty;
        private readonly LimitModifier _objLimitModifier;
        private readonly Character _objCharacter;
        private readonly string[] _lstLimits;

        #region Control Events

        public SelectLimitModifier(LimitModifier objLimitModifier = null, params string[] lstLimits)
        {
            _objLimitModifier = objLimitModifier;
            _objCharacter = objLimitModifier?.CharacterObject;
            _lstLimits = lstLimits;
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();

        }

        private async void SelectLimitModifier_Load(object sender, EventArgs e)
        {
            // Build the list of Limits.
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstLimitItems))
            {
                foreach (string strLimit in _lstLimits)
                {
                    lstLimitItems.Add(
                        new ListItem(strLimit, await LanguageManager.GetStringAsync("String_Limit" + strLimit + "Short").ConfigureAwait(false)));
                }

                await cboLimit.PopulateWithListItemsAsync(lstLimitItems).ConfigureAwait(false);
                if (lstLimitItems.Count >= 1)
                    await cboLimit.DoThreadSafeAsync(x => x.SelectedIndex = 0).ConfigureAwait(false);
                else
                    await cmdOK.DoThreadSafeAsync(x => x.Enabled = false).ConfigureAwait(false);
            }

            if (_objLimitModifier != null)
            {
                _strReturnName = _objLimitModifier.Name;
                _intBonus = _objLimitModifier.Bonus;
                _strLimitType = _objLimitModifier.Limit;
                _strCondition = await _objLimitModifier.GetConditionAsync().ConfigureAwait(false);
                string strName = await LanguageManager.TranslateExtraAsync(_strReturnName, objCharacter: _objCharacter).ConfigureAwait(false);
                string strCondition = await _objLimitModifier.GetCurrentDisplayConditionAsync().ConfigureAwait(false);
                await cboLimit.DoThreadSafeAsync(x => x.SelectedValue = _strLimitType).ConfigureAwait(false);
                await txtName.DoThreadSafeAsync(x => x.Text = strName).ConfigureAwait(false);
                await nudBonus.DoThreadSafeAsync(x => x.Value = _intBonus).ConfigureAwait(false);
                await txtCondition.DoThreadSafeAsync(x => x.Text = strCondition).ConfigureAwait(false);
            }
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            string strName = await txtName.DoThreadSafeFuncAsync(x => x.Text).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(strName))
            {
                string strLimitType = await cboLimit.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false) ?? string.Empty;
                if (!string.IsNullOrEmpty(strLimitType))
                {
                    strName = await LanguageManager.ReverseTranslateExtraAsync(strName, objCharacter: _objCharacter).ConfigureAwait(false);
                    int intBonus = await nudBonus.DoThreadSafeFuncAsync(x => x.ValueAsInt).ConfigureAwait(false);
                    string strCondition = await LanguageManager.ReverseTranslateExtraAsync(await txtCondition.DoThreadSafeFuncAsync(x => x.Text).ConfigureAwait(false), objCharacter: _objCharacter).ConfigureAwait(false);
                    await this.DoThreadSafeAsync(x =>
                    {
                        _strReturnName = strName;
                        _intBonus = intBonus;
                        _strCondition = strCondition;
                        _strLimitType = strLimitType;
                        x.DialogResult = DialogResult.OK;
                        x.Close();
                    }).ConfigureAwait(false);
                }
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Modifier name that was entered in the dialogue.
        /// </summary>
        public string SelectedName => _strReturnName;

        /// <summary>
        /// Modifier condition that was entered in the dialogue.
        /// </summary>
        public string SelectedCondition => _strCondition;

        /// <summary>
        /// Modifier Bonus that was entered in the dialogue.
        /// </summary>
        public int SelectedBonus => _intBonus;

        /// <summary>
        /// Modifier limit type that was entered in the dialogue.
        /// </summary>
        public string SelectedLimitType => _strLimitType;

        #endregion Properties

        private void ToggleOkEnabled(object sender, EventArgs e)
        {
            cmdOK.Enabled = cboLimit.Items.Count > 0 && txtName.TextLength > 0 && !string.IsNullOrEmpty(cboLimit.SelectedValue?.ToString());
        }
    }
}
