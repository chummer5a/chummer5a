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

namespace Chummer
{
    public sealed partial class InitiativeUserControl : UserControl
    {
        #region Properties

        /// <summary>
        /// Fired when the current character is changed
        /// </summary>
        public event EventHandler CurrentCharacterChanged
        {
            add => chkBoxChummer.SelectedValueChanged += value;
            remove => chkBoxChummer.SelectedValueChanged -= value;
        }

        #endregion Properties

        private readonly List<Character> _lstCharacters = new List<Character>(5);
        private int _intIndex;
        private int _intRound;
        private bool _blnFinishedCombatTurn;
        private int _intTotalChummersWithNoInit;

        /// <summary>
        /// Default constructor
        /// </summary>
        public InitiativeUserControl()
        {
            InitializeComponent();
            lblRound.Text = LanguageManager.GetString("Label_Round") + LanguageManager.GetString("String_Space") + 1.ToString(GlobalSettings.CultureInfo);
            _intRound = 1;

            // setup the list of chummers to show
            chkBoxChummer.DisplayMember = "DisplayInit";
        }

        #region Events

        /*
         * Queries the user for the chummer to add to the list
         */

        private void btnAdd_Click(object sender, EventArgs e)
        {
            AddToken frmAdd = new AddToken(this);
            frmAdd.Show();
        }

        /*
         * Removes the chosen chummer from the list
         */

        private async void btnRemove_Click(object sender, EventArgs e)
        {
            // check if we have selected a chummer in the list
            if (await chkBoxChummer.DoThreadSafeFuncAsync(x => x.SelectedItem) == null)
                Program.ShowMessageBox("Please Select a Chummer to remove");
            else
            {
                int index = 0;
                await chkBoxChummer.DoThreadSafeAsync(x =>
                {
                    index = x.SelectedIndex;
                    x.Items.RemoveAt(index);
                    if (x.Items.Count > 0)
                        x.SelectedIndex = 0; // reset the selected item to the first item in the list
                });
                await _lstCharacters[index].DisposeAsync();
                _lstCharacters.RemoveAt(index);
            }
        }

        /*
         * Subtracts 1 init
         */

        private void btnMinusInit1_Click(object sender, EventArgs e)
        {
            // check if we have selected a chummer in the list
            if (chkBoxChummer.SelectedItem == null)
                Program.ShowMessageBox("Please Select a Chummer to remove");
            else if (_lstCharacters[chkBoxChummer.SelectedIndex].InitRoll - 1 >= 0)
                ApplyInitChange(-1);
            else
                Program.ShowMessageBox("unable to go beyond 0");
        }

        /*
         * Subtracts 5 init
         */

        private void btnMinus5Init_Click(object sender, EventArgs e)
        {
            // check if we have selected a chummer in the list
            if (chkBoxChummer.SelectedItem == null)
                Program.ShowMessageBox("Please Select a Chummer to remove");
            else if (_lstCharacters[chkBoxChummer.SelectedIndex].InitRoll - 5 >= 0)
                ApplyInitChange(-5);
            else
                Program.ShowMessageBox("unable to go beyond 0");
        }

        /*
         * Subtracts 10 init
         */

        private void btnMinus10Init_Click(object sender, EventArgs e)
        {
            // check if we have selected a chummer in the list
            if (chkBoxChummer.SelectedItem == null)
                Program.ShowMessageBox("Please Select a Chummer to remove");
            else if (_lstCharacters[chkBoxChummer.SelectedIndex].InitRoll - 1 >= 0)
                ApplyInitChange(-10);
            else
                Program.ShowMessageBox("unable to go beyond 0");
        }

        /*
         * Adds 1 init
         */

        private void btnAdd1Init_Click(object sender, EventArgs e)
        {
            // check if we have selected a chummer in the list
            if (chkBoxChummer.SelectedItem == null)
                Program.ShowMessageBox("Please Select a Chummer to remove");
            else if (_lstCharacters[chkBoxChummer.SelectedIndex].InitRoll - 1 >= 0)
                ApplyInitChange(1);
            else
                Program.ShowMessageBox("unable to go beyond 0");
        }

        /*
         * Adds 5 init
         */

        private void btnAdd5Init_Click(object sender, EventArgs e)
        {
            // check if we have selected a chummer in the list
            if (chkBoxChummer.SelectedItem == null)
                Program.ShowMessageBox("Please Select a Chummer to remove");
            else if (_lstCharacters[chkBoxChummer.SelectedIndex].InitRoll - 1 >= 0)
                ApplyInitChange(5);
            else
                Program.ShowMessageBox("unable to go beyond 0");
        }

        /*
         * Adds 10 init
         */

        private void btnAdd10Init_Click(object sender, EventArgs e)
        {
            // check if we have selected a chummer in the list
            if (chkBoxChummer.SelectedItem == null)
                Program.ShowMessageBox("Please Select a Chummer to remove");
            else if (_lstCharacters[chkBoxChummer.SelectedIndex].InitRoll - 1 >= 0)
                ApplyInitChange(10);
            else
                Program.ShowMessageBox("unable to go beyond 0");
        }

        /*
         * Applies the specified interrupt action
         */

        private void btnApplyInterrupt_Click(object sender, EventArgs e)
        {
            // TODO edward
        }

        /*
         * goes to the next chummer in the list
         */

        private async void btnNext_Click(object sender, EventArgs e)
        {
            if (_blnFinishedCombatTurn)
                return; // cannot go to "next"
            if (_intIndex == _lstCharacters.Count - _intTotalChummersWithNoInit)
            {
                // increment the round count since we have reached the end of the list
                string strText = await LanguageManager.GetStringAsync("Label_Round") + await LanguageManager.GetStringAsync("String_Space") + (++_intRound).ToString(GlobalSettings.CultureInfo);
                await lblRound.DoThreadSafeAsync(x => x.Text = strText);
                // reset the the round with a minus ten on all
                int index = -1;
                for (int i = 0; i < _lstCharacters.Count; i++)
                {
                    if (_lstCharacters[i].InitRoll > 0)
                    {
                        await chkBoxChummer.DoThreadSafeAsync(x => x.SelectedIndex = i);
                        ApplyInitChange(-10);
                        index = i;
                    }
                }

                if (index == -1)
                    _blnFinishedCombatTurn = true;

                _intIndex = 0;
            }
            else
            {
                // setup the next chummer to go
                while (_intIndex < _lstCharacters.Count && _lstCharacters[_intIndex].InitRoll <= 0)
                    _intIndex++;

                // check if there are no more chummers which can move
                if (_intIndex == _lstCharacters.Count)
                {
                    _blnFinishedCombatTurn = true;
                    _intIndex = 0;
                    return; // we are finished
                }

                await chkBoxChummer.DoThreadSafeAsync(x => x.SelectedIndex = _intIndex);
                ++_intIndex;
            }
        }

        /*
         * Sorts the _lstCharacters based on Initiative
         */

        private void btnSort_Click(object sender, EventArgs e)
        {
            _lstCharacters.Sort((a, b) => -a.InitRoll.CompareTo(b.InitRoll));
            chkBoxChummer.Items.Clear();
            foreach (Character character in _lstCharacters)
                chkBoxChummer.Items.Add(character);

            _intIndex = 0;
        }

        /*
         * Delays the chosen character
         */

        private void btnDelay_Click(object sender, EventArgs e)
        {
            // make sure a chummer is selected
            if (chkBoxChummer.SelectedItem == null)
                Program.ShowMessageBox("please select a chummer");
            else if (_lstCharacters[chkBoxChummer.SelectedIndex].InitRoll < 1)
                Program.ShowMessageBox("unable to delay chummer with no init");
            else
            {
                int index = chkBoxChummer.SelectedIndex;
                Character character = _lstCharacters[index];

                // update the position of the chummer to the next highest initiative - 1 in regards to other delayed _lstCharacters
                // i.e. if the chummer delaying has 29 init and their is a chummer with 30 init, move the delayed chummer above it
                int tempIndex = _lstCharacters.Count - 1;
                for (int i = 0; i < _lstCharacters.Count; i++)
                {
                    if (_lstCharacters[i].InitRoll < _lstCharacters[index].InitRoll && _lstCharacters[index].Delayed)
                    {
                        // we have found the first (since it's sorted) chummer with a larger value init roll\
                        tempIndex = i;
                        break;
                    }
                }

                _lstCharacters.RemoveAt(index);
                _lstCharacters.Insert(tempIndex, character);
                // back up one for indexing purposes
                --_intIndex;
                ResetListBoxChummers();
            }
        }

        /*
         * Reset button pressed
         */

        private async void btnReset_Click(object sender, EventArgs e)
        {
            // for every checked character, we re-roll init
            for (int i = 0; i < _lstCharacters.Count; i++)
            {
                if (await chkBoxChummer.DoThreadSafeFuncAsync(x => x.CheckedIndices.Contains(i)))
                {
                    Character objLoopCharacter = _lstCharacters[i];
                    int intInitPasses = objLoopCharacter.InitPasses;
                    int intInitRoll = intInitPasses;
                    for (int j = 0; j < intInitPasses; j++)
                    {
                        intInitRoll += await GlobalSettings.RandomGenerator.NextD6ModuloBiasRemovedAsync();
                    }
                    objLoopCharacter.InitRoll = intInitRoll + objLoopCharacter.InitialInit;
                }
            }

            // query for new initiatives
            for (int j = 0; j < _lstCharacters.Count; j++)
            {
                if (await chkBoxChummer.DoThreadSafeFuncAsync(x => x.GetItemCheckState(j) == CheckState.Unchecked))
                {
                    Character objLoopCharacter = _lstCharacters[j];
                    using (InitiativeRoller frmHits = await this.DoThreadSafeFuncAsync(() => new InitiativeRoller
                           {
                               Dice = objLoopCharacter.InitPasses
                           }))
                    {
                        if (await frmHits.ShowDialogSafeAsync(this) != DialogResult.OK)
                            return; // we decided not to actually change the initiative
                        objLoopCharacter.InitRoll = frmHits.Result + objLoopCharacter.InitialInit;
                    }
                }
            }

            ResetListBoxChummers();
            _blnFinishedCombatTurn = false;
            _intIndex = 0;
            _intRound = 1;
            string strText = await LanguageManager.GetStringAsync("Label_Round") + await LanguageManager.GetStringAsync("String_Space") + 1.ToString(GlobalSettings.CultureInfo);
            await lblRound.DoThreadSafeAsync(x => x.Text = strText);
            _intTotalChummersWithNoInit = 0;
        }

        /*
         * When the index has changed for the check box list
         */

        private void listBoxChummers_SelectedIndexChanged(object sender, EventArgs e)
        {
            // confirm if we are delaying the selected chummer, if we are, ask user if they
            // wish for the chummer to perform a delayed action
            if (chkBoxChummer.SelectedIndex < 0)
                return;
            int index = chkBoxChummer.SelectedIndex;
            if (_lstCharacters[index].Delayed)
            {
                if (index != _intIndex)
                {
                    DialogResult result = Program.ShowMessageBox(
                        "Would you like the chummer to perform a delayed action?", "Delayed Action",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        // un-delay character, and lock it in the current location
                        Character character = _lstCharacters[index];
                        character.Delayed = false;

                        // place the chummer as the current chummer
                        _lstCharacters.RemoveAt(index);
                        _lstCharacters.Insert(_intIndex, character);

                        ResetListBoxChummers();
                    }
                }
                else
                {
                    // it is the chummers turn and we should just turn off the delayed action
                    Character character = _lstCharacters[index];
                    character.Delayed = false;
                    _lstCharacters[index] = character;
                    chkBoxChummer.Items[index] = _lstCharacters[index];
                }
            }
        }

        /*
         * Should work...
         * When the user right-clicks somewhere in the check box list.
         */

        private async void chkBoxChummer_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // confirm we are selecting a chummer
                if (await chkBoxChummer.DoThreadSafeFuncAsync(x => x.SelectedItem == null))
                    Program.ShowMessageBox("Please select a chummer before right-clicking");

                using (InitiativeRoller frmHits = await this.DoThreadSafeFuncAsync(() => new InitiativeRoller
                {
                    Dice = _lstCharacters[chkBoxChummer.SelectedIndex].InitPasses
                }))
                {
                    if (await frmHits.ShowDialogSafeAsync(this) != DialogResult.OK)
                        return; // we decided not to actually change the initiative

                    int intResult = await frmHits.DoThreadSafeFuncAsync(x => x.Result);
                    await chkBoxChummer.DoThreadSafeAsync(x =>
                    {
                        _lstCharacters[x.SelectedIndex].InitRoll = intResult;

                        x.Items[x.SelectedIndex] = _lstCharacters[x.SelectedIndex];
                    });
                }
            }
        }

        #endregion Events

        #region Methods

        /// <summary>
        /// The current character in the chain of initiatives
        /// </summary>
        public Character CurrentCharacter => _lstCharacters[_intIndex];

        /// <summary>
        /// Adds the token to the initiative chain
        /// </summary>
        /// <param name="character"></param>
        public async ValueTask AddToken(Character character)
        {
            if (character == null)
                return;
            if (character.InitRoll == int.MinValue)
            {
                using (InitiativeRoller frmHits = await this.DoThreadSafeFuncAsync(() => new InitiativeRoller
                {
                    Dice = character.InitPasses
                }))
                {
                    if (await frmHits.ShowDialogSafeAsync(this) != DialogResult.OK)
                    {
                        Program.ShowMessageBox("ERROR"); // TODO edward show error
                        return;
                    }

                    character.InitRoll = await frmHits.DoThreadSafeFuncAsync(x => x.Result) + character.InitialInit;
                }
            }

            _lstCharacters.Add(character);
            await chkBoxChummer.DoThreadSafeAsync(x => x.Items.Add(character));
        }

        /*
         * Applies the specified amount of initiative to the
         * currently selected player
         */

        private void ApplyInitChange(int value)
        {
            // check if we have selected a chummer in the list
            if (chkBoxChummer.SelectedItem == null)
                Program.ShowMessageBox("Please Select a Chummer to remove");
            else
            {
                // pull the simple character out
                int index = chkBoxChummer.SelectedIndex;
                _lstCharacters[index].InitRoll += value;

                // if negative or 0 init add to the count
                if (_lstCharacters[index].InitRoll < 1)
                    _intTotalChummersWithNoInit++;

                ResetListBoxChummers();
                chkBoxChummer.SelectedIndex = index;
            }
        }

        /*
         * Resets the item in the Check Box List
         */

        private void ResetListBoxChummers()
        {
            chkBoxChummer.Items.Clear();
            foreach (Character aCharacter in _lstCharacters)
                chkBoxChummer.Items.Add(aCharacter);
        }

        #endregion Methods
    }
}
