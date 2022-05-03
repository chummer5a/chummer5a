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
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private async void SelectLifestyleStartingNuyen_Load(object sender, EventArgs e)
        {
            await RefreshSelectLifestyle();
        }

        private async void nudDiceResult_ValueChanged(object sender, EventArgs e)
        {
            await RefreshResultLabel();
        }

        private async ValueTask RefreshResultLabel()
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this);
            try
            {
                string strSpace = await LanguageManager.GetStringAsync("String_Space");
                await lblResult.DoThreadSafeAsync(x => x.Text = strSpace + '+' + strSpace
                                                                + Extra.ToString("#,0", GlobalSettings.CultureInfo)
                                                                + ')' +
                                                                strSpace + '×'
                                                                + strSpace + (SelectedLifestyle?.Multiplier ?? 0)
                                                                .ToString(
                                                                    _objCharacter.Settings.NuyenFormat
                                                                    + LanguageManager.GetString("String_NuyenSymbol"),
                                                                    GlobalSettings.CultureInfo)
                                                                + strSpace + '=' + strSpace +
                                                                StartingNuyen.ToString(
                                                                    _objCharacter.Settings.NuyenFormat
                                                                    + LanguageManager.GetString("String_NuyenSymbol"),
                                                                    GlobalSettings.CultureInfo));
            }
            finally
            {
                await objCursorWait.DisposeAsync();
            }
        }

        private async void cboSelectLifestyle_SelectionChanged(object sender, EventArgs e)
        {
            await RefreshBaseLifestyle();
        }

        private async ValueTask RefreshBaseLifestyle()
        {
            if (_blnIsSelectLifestyleRefreshing)
                return;
            if (await cboSelectLifestyle.DoThreadSafeFuncAsync(x => x.SelectedIndex) < 0)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this);
            try
            {
                _objLifestyle
                    = ((ListItem) await cboSelectLifestyle.DoThreadSafeFuncAsync(x => x.SelectedItem))
                    .Value as Lifestyle;
                string strDice = string.Format(GlobalSettings.CultureInfo,
                                               await LanguageManager.GetStringAsync("Label_LifestyleNuyen_ResultOf"),
                                               SelectedLifestyle?.Dice ?? 0);
                await lblDice.DoThreadSafeAsync(x => x.Text = strDice);
                await RefreshCalculation();
                await cmdRoll.DoThreadSafeAsync(x => x.Enabled = SelectedLifestyle?.Dice > 0);
                await DoRoll();
            }
            finally
            {
                await objCursorWait.DisposeAsync();
            }
        }

        private async ValueTask RefreshSelectLifestyle()
        {
            _blnIsSelectLifestyleRefreshing = true;
            CursorWait objCursorWait = await CursorWait.NewAsync(this);
            try
            {
                try
                {
                    Lifestyle objPreferredLifestyle = null;
                    ListItem objPreferredLifestyleItem = default;
                    Lifestyle objCurrentlySelectedLifestyle = await cboSelectLifestyle.DoThreadSafeFuncAsync(
                        x => x.SelectedIndex >= 0
                            ? ((ListItem) x.SelectedItem).Value as Lifestyle
                            : null);
                    using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                                   out List<ListItem> lstLifestyleItems))
                    {
                        foreach (Lifestyle objLifestyle in _objCharacter.Lifestyles)
                        {
                            ListItem objLifestyleItem = new ListItem(objLifestyle, objLifestyle.CurrentDisplayName);
                            lstLifestyleItems.Add(new ListItem(objLifestyle, objLifestyle.CurrentDisplayName));
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
                        }

                        lstLifestyleItems.Sort(CompareListItems.CompareNames);

                        await cboSelectLifestyle.PopulateWithListItemsAsync(lstLifestyleItems);
                        await cboSelectLifestyle.DoThreadSafeAsync(x =>
                        {
                            x.SelectedItem = objPreferredLifestyleItem;
                            if (x.SelectedIndex < 0 && lstLifestyleItems.Count > 0)
                                x.SelectedIndex = 0;
                            x.Enabled = lstLifestyleItems.Count > 1;
                        });
                    }
                }
                finally
                {
                    _blnIsSelectLifestyleRefreshing = false;
                }

                await RefreshBaseLifestyle();
            }
            finally
            {
                await objCursorWait.DisposeAsync();
            }
        }

        private async ValueTask RefreshCalculation()
        {
            CursorWait objCursorWait = await CursorWait.NewAsync(this);
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
                });
                await RefreshResultLabel();
            }
            finally
            {
                await objCursorWait.DisposeAsync();
            }
        }

        private async void cmdRoll_Click(object sender, EventArgs e)
        {
            if (SelectedLifestyle == null)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this);
            try
            {
                await DoRoll();
            }
            finally
            {
                await objCursorWait.DisposeAsync();
            }
        }

        private async ValueTask DoRoll()
        {
            int intResult = 0;
            for (int i = 0; i < SelectedLifestyle.Dice; ++i)
            {
                intResult += await GlobalSettings.RandomGenerator.NextD6ModuloBiasRemovedAsync();
            }

            await nudDiceResult.DoThreadSafeAsync(x => x.ValueAsInt = intResult);
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
