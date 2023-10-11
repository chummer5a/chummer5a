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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class SelectLifestyleStartingNuyen : Form
    {
        private readonly Character _objCharacter;
        private Lifestyle _objLifestyle;
        private bool _blnIsSelectLifestyleRefreshing;

        #region Control Events

        public SelectLifestyleStartingNuyen(Character objCharacter)
        {
            _objCharacter = objCharacter;
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
        }

        [Obsolete("This constructor is for use by form designers only.", true)]
        public SelectLifestyleStartingNuyen()
        {
            InitializeComponent();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void SelectLifestyleStartingNuyen_Load(object sender, EventArgs e)
        {
            await RefreshSelectLifestyle().ConfigureAwait(false);
        }

        private async void nudDiceResult_ValueChanged(object sender, EventArgs e)
        {
            await RefreshResultLabel().ConfigureAwait(false);
        }

        private async ValueTask RefreshResultLabel(CancellationToken token = default)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
                string strFormat = _objCharacter.Settings.NuyenFormat + await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token).ConfigureAwait(false);
                string strText = strSpace + '+' + strSpace + Extra.ToString("#,0", GlobalSettings.CultureInfo) + ')' +
                                 strSpace + '×' + strSpace +
                                 (SelectedLifestyle?.Multiplier ?? 0).ToString(strFormat, GlobalSettings.CultureInfo) +
                                 strSpace + '=' + strSpace +
                                 StartingNuyen.ToString(strFormat, GlobalSettings.CultureInfo);
                await lblResult.DoThreadSafeAsync(x => x.Text = strText, token: token).ConfigureAwait(false);
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async void cboSelectLifestyle_SelectionChanged(object sender, EventArgs e)
        {
            await RefreshBaseLifestyle().ConfigureAwait(false);
        }

        private async ValueTask RefreshBaseLifestyle(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_blnIsSelectLifestyleRefreshing)
                return;
            if (await cboSelectLifestyle.DoThreadSafeFuncAsync(x => x.SelectedIndex, token).ConfigureAwait(false) < 0)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                _objLifestyle
                    = ((ListItem) await cboSelectLifestyle.DoThreadSafeFuncAsync(x => x.SelectedItem, token).ConfigureAwait(false))
                    .Value as Lifestyle;
                string strDice = string.Format(GlobalSettings.CultureInfo,
                                               await LanguageManager.GetStringAsync("Label_LifestyleNuyen_ResultOf", token: token).ConfigureAwait(false),
                                               SelectedLifestyle?.Dice ?? 0);
                await lblDice.DoThreadSafeAsync(x => x.Text = strDice, token).ConfigureAwait(false);
                await RefreshCalculation(token).ConfigureAwait(false);
                await cmdRoll.DoThreadSafeAsync(x => x.Enabled = SelectedLifestyle?.Dice > 0, token).ConfigureAwait(false);
                await DoRoll(token).ConfigureAwait(false);
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async ValueTask RefreshSelectLifestyle(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                _blnIsSelectLifestyleRefreshing = true;
                try
                {
                    Lifestyle objPreferredLifestyle = null;
                    ListItem objPreferredLifestyleItem = default;
                    Lifestyle objCurrentlySelectedLifestyle = await cboSelectLifestyle.DoThreadSafeFuncAsync(
                        x => x.SelectedIndex >= 0
                            ? ((ListItem) x.SelectedItem).Value as Lifestyle
                            : null, token).ConfigureAwait(false);
                    using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                                   out List<ListItem> lstLifestyleItems))
                    {
                        await _objCharacter.Lifestyles.ForEachAsync(async objLifestyle =>
                        {
                            ListItem objLifestyleItem = new ListItem(objLifestyle,
                                                                     await objLifestyle
                                                                           .GetCurrentDisplayNameAsync(token)
                                                                           .ConfigureAwait(false));
                            lstLifestyleItems.Add(objLifestyleItem);
                            // We already selected a lifestyle, so keep the selection if possible despite the refresh
                            if (objCurrentlySelectedLifestyle != null)
                            {
                                if (objCurrentlySelectedLifestyle == objLifestyle)
                                    objPreferredLifestyleItem = objLifestyleItem;
                            }
                            else if (objPreferredLifestyle == null ||
                                     objLifestyle.ExpectedValue > objPreferredLifestyle.ExpectedValue)
                            {
                                objPreferredLifestyleItem = objLifestyleItem;
                                objPreferredLifestyle = objLifestyle;
                            }
                        }, token).ConfigureAwait(false);

                        lstLifestyleItems.Sort(CompareListItems.CompareNames);

                        await cboSelectLifestyle.PopulateWithListItemsAsync(lstLifestyleItems, token).ConfigureAwait(false);
                        await cboSelectLifestyle.DoThreadSafeAsync(x =>
                        {
                            x.SelectedItem = objPreferredLifestyleItem;
                            if (x.SelectedIndex < 0 && lstLifestyleItems.Count > 0)
                                x.SelectedIndex = 0;
                            x.Enabled = lstLifestyleItems.Count > 1;
                        }, token).ConfigureAwait(false);
                    }
                }
                finally
                {
                    _blnIsSelectLifestyleRefreshing = false;
                }

                await RefreshBaseLifestyle(token).ConfigureAwait(false);
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async ValueTask RefreshCalculation(CancellationToken token = default)
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
            try
            {
                await nudDiceResult.DoThreadSafeAsync(x =>
                {
                    x.SuspendLayout();
                    try
                    {
                        x.MinimumAsInt =
                            int.MinValue; // Temporarily set this to avoid crashing if we shift from something with more than 6 dice to something with less.
                        x.MaximumAsInt = SelectedLifestyle?.Dice * 6 ?? 0;
                        x.MinimumAsInt = SelectedLifestyle?.Dice ?? 0;
                    }
                    finally
                    {
                        x.ResumeLayout();
                    }
                }, token: token).ConfigureAwait(false);
                await RefreshResultLabel(token).ConfigureAwait(false);
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async void cmdRoll_Click(object sender, EventArgs e)
        {
            if (SelectedLifestyle == null)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this).ConfigureAwait(false);
            try
            {
                await DoRoll().ConfigureAwait(false);
            }
            finally
            {
                await objCursorWait.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async ValueTask DoRoll(CancellationToken token = default)
        {
            int intResult = 0;
            for (int i = 0; i < SelectedLifestyle.Dice; ++i)
            {
                intResult += await GlobalSettings.RandomGenerator.NextD6ModuloBiasRemovedAsync(token: token).ConfigureAwait(false);
            }

            await nudDiceResult.DoThreadSafeAsync(x => x.ValueAsInt = intResult, token: token).ConfigureAwait(false);
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Extra number that is added to the dice roll.
        /// </summary>
        public decimal Extra { get; set; }

        /// <summary>
        /// The total amount of Nuyen resulting from the dice roll.
        /// </summary>
        public decimal StartingNuyen => ((nudDiceResult.Value + Extra) * SelectedLifestyle?.Multiplier) ?? 0;

        /// <summary>
        /// The currently selected lifestyle
        /// </summary>
        public Lifestyle SelectedLifestyle => _objLifestyle;

        #endregion Properties
    }
}
