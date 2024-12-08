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
    public partial class SelectSetting : Form
    {
        private string _strSettingsKey = GlobalSettings.DefaultCharacterSetting;

        #region Control Events

        public SelectSetting()
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
        }

        private async void SelectSetting_Load(object sender, EventArgs e)
        {
            // Build the list of XML files found in the settings directory.
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstSettings))
            {
                foreach (KeyValuePair<string, CharacterSettings> kvpSetting in await SettingsManager.GetLoadedCharacterSettingsAsync().ConfigureAwait(false))
                {
                    string strDisplayName = await kvpSetting.Value.GetCurrentDisplayNameAsync().ConfigureAwait(false);
                    if (string.IsNullOrEmpty(strDisplayName))
                        strDisplayName = await LanguageManager.GetStringAsync("String_Unknown").ConfigureAwait(false);
                    lstSettings.Add(new ListItem(kvpSetting.Key, strDisplayName));
                }

                lstSettings.Sort(CompareListItems.CompareNames);
                await cboSetting.PopulateWithListItemsAsync(lstSettings).ConfigureAwait(false);
                // Attempt to make default.xml the default one. If it could not be found in the list, select the first item instead.
                await cboSetting.DoThreadSafeAsync(x =>
                {
                    x.SelectedIndex = x.FindStringExact("Default Settings");
                    if (x.SelectedIndex == -1)
                        x.SelectedIndex = 0;
                }).ConfigureAwait(false);
            }
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            _strSettingsKey = cboSetting.SelectedValue.ToString();
            DialogResult = DialogResult.OK;
            Close();
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Settings file that was selected in the dialogue.
        /// </summary>
        public string SettingsKey => _strSettingsKey;

        #endregion Properties
    }
}
