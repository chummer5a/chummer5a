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
using System.Windows.Forms;

namespace Chummer
{
    public partial class SelectOptionalPower : Form
    {
        private string _strReturnPower = string.Empty;
        private string _strReturnExtra = string.Empty;
        private readonly Character _objCharacter;
        private readonly List<Tuple<string, string>> _lstPowerExtraPairs;

        #region Control Events

        public SelectOptionalPower(Character objCharacter, params Tuple<string, string>[] lstPowerExtraPairs)
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();

            _objCharacter = objCharacter;
            _lstPowerExtraPairs = lstPowerExtraPairs.ToList();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (cboPower.SelectedValue is Tuple<string, string> objSelectedItem)
            {
                _strReturnPower = objSelectedItem.Item1;
                _strReturnExtra = objSelectedItem.Item2;
                DialogResult = DialogResult.OK;
            }
        }

        private async void SelectOptionalPower_Load(object sender, EventArgs e)
        {
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstPowerItems))
            {
                foreach ((string strPowerName, string strPowerExtra) in _lstPowerExtraPairs)
                {
                    string strName = string.IsNullOrEmpty(strPowerExtra)
                        ? await _objCharacter.TranslateExtraAsync(strPowerName)
                        : await _objCharacter.TranslateExtraAsync(strPowerName)
                          + await LanguageManager.GetStringAsync("String_Space") + '('
                          + await _objCharacter.TranslateExtraAsync(strPowerExtra) + ')';
                    lstPowerItems.Add(new ListItem(new Tuple<string, string>(strPowerName, strPowerExtra), strName));
                }
                
                await cboPower.PopulateWithListItemsAsync(lstPowerItems);
                if (lstPowerItems.Count >= 1)
                    cboPower.SelectedIndex = 0;
                else
                    cmdOK.Enabled = false;
                if (lstPowerItems.Count == 1)
                    cmdOK_Click(sender, e);
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Power that was selected in the dialogue.
        /// </summary>
        public string SelectedPower => _strReturnPower;

        public string SelectedPowerExtra => _strReturnExtra;

        /// <summary>
        /// Description to display on the form.
        /// </summary>
        public string Description
        {
            set => lblDescription.Text = value;
        }

        #endregion Properties
    }
}
