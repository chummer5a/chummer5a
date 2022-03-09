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
                lblResult.Text = strSpace + '+' + strSpace + Extra.ToString("#,0", GlobalSettings.CultureInfo) + ')' +
                                 strSpace + '×'
                                 + strSpace + (SelectedLifestyle?.Multiplier ?? 0).ToString(
                                     _objCharacter.Settings.NuyenFormat + '¥', GlobalSettings.CultureInfo)
                                 + strSpace + '=' + strSpace +
                                 StartingNuyen.ToString(_objCharacter.Settings.NuyenFormat + '¥',
                                                        GlobalSettings.CultureInfo);
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
            if (cboSelectLifestyle.SelectedIndex < 0)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this);
            try
            {
                _objLifestyle = ((ListItem) cboSelectLifestyle.SelectedItem).Value as Lifestyle;
                lblDice.Text = string.Format(GlobalSettings.CultureInfo,
                                             await LanguageManager.GetStringAsync("Label_LifestyleNuyen_ResultOf"),
                                             SelectedLifestyle?.Dice ?? 0);
                await RefreshCalculation();
                cmdRoll.Enabled = SelectedLifestyle?.Dice > 0;
            }
            finally
            {
                await objCursorWait.DisposeAsync();
            }
        }

        private ValueTask RefreshSelectLifestyle()
        {
            _blnIsSelectLifestyleRefreshing = true;
            using (CursorWait.New(this))
            {
                try
                {
                    Lifestyle objPreferredLifestyle = null;
                    ListItem objPreferredLifestyleItem = default;
                    Lifestyle objCurrentlySelectedLifestyle = cboSelectLifestyle.SelectedIndex >= 0
                        ? ((ListItem)cboSelectLifestyle.SelectedItem).Value as Lifestyle
                        : null;
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

                        cboSelectLifestyle.BeginUpdate();
                        cboSelectLifestyle.PopulateWithListItems(lstLifestyleItems);
                        cboSelectLifestyle.SelectedItem = objPreferredLifestyleItem;
                        if (cboSelectLifestyle.SelectedIndex < 0 && lstLifestyleItems.Count > 0)
                            cboSelectLifestyle.SelectedIndex = 0;
                        cboSelectLifestyle.Enabled = lstLifestyleItems.Count > 1;
                        cboSelectLifestyle.EndUpdate();
                    }
                }
                finally
                {
                    _blnIsSelectLifestyleRefreshing = false;
                }
                return RefreshBaseLifestyle();
            }
        }

        private ValueTask RefreshCalculation()
        {
            using (CursorWait.New(this))
            {
                nudDiceResult.SuspendLayout();
                nudDiceResult.MinimumAsInt =
                    int.MinValue; // Temporarily set this to avoid crashing if we shift from something with more than 6 dice to something with less.
                nudDiceResult.MaximumAsInt = SelectedLifestyle?.Dice * 6 ?? 0;
                nudDiceResult.MinimumAsInt = SelectedLifestyle?.Dice ?? 0;
                nudDiceResult.ResumeLayout();
                return RefreshResultLabel();
            }
        }

        private async void cmdRoll_Click(object sender, EventArgs e)
        {
            if (SelectedLifestyle == null)
                return;
            CursorWait objCursorWait = await CursorWait.NewAsync(this);
            try
            {
                int intResult = 0;
                for (int i = 0; i < SelectedLifestyle.Dice; ++i)
                {
                    intResult += await GlobalSettings.RandomGenerator.NextD6ModuloBiasRemovedAsync();
                }

                nudDiceResult.ValueAsInt = intResult;
            }
            finally
            {
                await objCursorWait.DisposeAsync();
            }
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
