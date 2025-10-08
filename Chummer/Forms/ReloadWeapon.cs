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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class ReloadWeapon : Form
    {
        private readonly Weapon _objWeapon;
        private readonly List<Gear> _lstAmmo = new List<Gear>(5);
        private readonly List<string> _lstCount = new List<string>(30);

        #region Control Events

        public ReloadWeapon(Weapon objWeapon)
        {
            _objWeapon = objWeapon;
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();

        }

        private async void ReloadWeapon_Load(object sender, EventArgs e)
        {
            using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool,
                                                           out List<ListItem> lstAmmo))
            {
                string strSpace = await LanguageManager.GetStringAsync("String_Space").ConfigureAwait(false);
                // Add each of the items to a new List since we need to also grab their plugin information.
                foreach (Gear objGear in _lstAmmo)
                {
                    string strName = await objGear.GetCurrentDisplayNameShortAsync().ConfigureAwait(false) + strSpace + "×"
                        + objGear.Quantity.ToString(GlobalSettings.InvariantCultureInfo);
                    int intRating = await objGear.GetRatingAsync().ConfigureAwait(false);
                    if (intRating > 0)
                    {
                        strName += strSpace + "("
                                            + string.Format(
                                                GlobalSettings.CultureInfo,
                                                await LanguageManager.GetStringAsync("Label_RatingFormat")
                                                                     .ConfigureAwait(false),
                                                await LanguageManager.GetStringAsync(objGear.RatingLabel)
                                                                     .ConfigureAwait(false)) + strSpace
                                            + intRating.ToString(GlobalSettings.CultureInfo) + ")";
                    }

                    if (objGear.Parent is Gear objParent)
                    {
                        if (!string.IsNullOrEmpty(await objParent.GetCurrentDisplayNameShortAsync().ConfigureAwait(false)))
                        {
                            strName += strSpace + "(" + await objParent.GetCurrentDisplayNameShortAsync().ConfigureAwait(false);
                            if (objParent.Location != null)
                                strName += strSpace + "@" + strSpace + await objParent.Location.GetCurrentDisplayNameAsync().ConfigureAwait(false);
                            strName += ")";
                        }
                    }
                    else if (objGear.Location != null)
                        strName += strSpace + "(" + await objGear.Location.GetCurrentDisplayNameAsync().ConfigureAwait(false) + ")";

                    // Retrieve the plugin information if it has any.
                    if (await objGear.Children.GetCountAsync().ConfigureAwait(false) > 0)
                    {
                        using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                                      out StringBuilder sbdPlugins))
                        {
                            await objGear.Children.ForEachAsync(async objChild =>
                                sbdPlugins.Append(await objChild.GetCurrentDisplayNameShortAsync().ConfigureAwait(false), ',', strSpace))
                                .ConfigureAwait(false);

                            // Remove the trailing comma.
                            sbdPlugins.Length -= 1 + strSpace.Length;
                            // Append the plugin information to the name.
                            strName = sbdPlugins.Insert(0, strName, strSpace, '[').Append(']').ToString();
                        }
                    }

                    lstAmmo.Add(new ListItem(objGear.InternalId, strName));
                }

                // Populate the lists.
                await cboAmmo.PopulateWithListItemsAsync(lstAmmo).ConfigureAwait(false);
                await cboAmmo.DoThreadSafeAsync(x =>
                {
                    Gear objExistingGear = _objWeapon?.AmmoLoaded;
                    if (objExistingGear != null)
                        x.SelectedValue = objExistingGear.InternalId;
                    if (x.SelectedIndex == -1)
                        x.SelectedIndex = 0;
                }).ConfigureAwait(false);
            }

            await cboType.DoThreadSafeAsync(x =>
            {
                x.BeginUpdate();
                try
                {
                    x.DataSource = null;
                    x.DataSource = _lstCount;
                }
                finally
                {
                    x.EndUpdate();
                }
            }).ConfigureAwait(false);

            // If there's only 1 value in each list, the character doesn't have a choice, so just accept it.
            if (await cboAmmo.DoThreadSafeFuncAsync(x => x.Items.Count).ConfigureAwait(false) == 1 && await cboType.DoThreadSafeFuncAsync(x => x.Items.Count).ConfigureAwait(false) == 1)
                AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            AcceptForm();
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// List of Ammo Gear that the user can selected.
        /// </summary>
        public IEnumerable<Gear> Ammo
        {
            set
            {
                _lstAmmo.Clear();
                _lstAmmo.AddRange(value);
            }
        }

        /// <summary>
        /// List of ammunition that the user can select.
        /// </summary>
        public IEnumerable<string> Count
        {
            set
            {
                _lstCount.Clear();
                _lstCount.AddRange(value);
            }
        }

        /// <summary>
        /// Name of the ammunition that was selected.
        /// </summary>
        public async Task<string> GetSelectedAmmoAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return (await cboAmmo.DoThreadSafeFuncAsync(x => x.SelectedValue, token).ConfigureAwait(false))?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Number of rounds that were selected to be loaded.
        /// </summary>
        public async Task<decimal> GetSelectedCountAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strText = await cboType.DoThreadSafeFuncAsync(x => x.Text, token).ConfigureAwait(false);
            return decimal.TryParse(strText, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                out decimal decReturn)
                ? decReturn
                : _objWeapon?.AmmoRemaining ?? 0;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        #endregion Methods
    }
}
