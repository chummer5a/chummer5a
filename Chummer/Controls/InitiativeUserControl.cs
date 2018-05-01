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
        #endregion

        private List<Character> characters;
        private int _intIndex;
        private int _intRound;
        private bool _blnFinishedCombatTurn;
        private int totalChummersWithNoInit;

        /// <summary>
        /// Default constructor
        /// </summary>
        public InitiativeUserControl()
        {
            InitializeComponent();
            characters = new List<Character>();
            lblRound.Text = lblRound.Text.Split(' ')[0] + " 1";
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
            new frmAddToken(this).Show();
        }

        /*
         * Removes the chosen chummer from the list
         */
        private void btnRemove_Click(object sender, EventArgs e)
        {
            // check if we have selected a chummer in the list
            if (chkBoxChummer.SelectedItem == null)
                MessageBox.Show("Please Select a Chummer to remove");
            else
            {
                int index = chkBoxChummer.SelectedIndex;
                chkBoxChummer.Items.RemoveAt(index);
                if (chkBoxChummer.Items.Count > 0)
                    chkBoxChummer.SelectedIndex = 0; // reset the selected item to the first item in the list
                characters[index].DeleteCharacter();
                characters[index] = null;
                characters.RemoveAt(index);
            }
        }

        /*
         * Subtracts 1 init
         */
        private void btnMinusInit1_Click(object sender, EventArgs e)
        {
            // check if we have selected a chummer in the list
            if (chkBoxChummer.SelectedItem == null)
                MessageBox.Show("Please Select a Chummer to remove");
            else if (characters[chkBoxChummer.SelectedIndex].InitRoll - 1 >= 0)
                ApplyInitChange(-1);
            else
                MessageBox.Show("unable to go beyond 0");
        }

        /*
         * Subtracts 5 init
         */
        private void btnMinus5Init_Click(object sender, EventArgs e)
        {
            // check if we have selected a chummer in the list
            if (chkBoxChummer.SelectedItem == null)
                MessageBox.Show("Please Select a Chummer to remove");
            else if (characters[chkBoxChummer.SelectedIndex].InitRoll - 5 >= 0)
                ApplyInitChange(-5);
            else
                MessageBox.Show("unable to go beyond 0");
        }

        /*
         * Subtracts 10 init
         */
        private void btnMinus10Init_Click(object sender, EventArgs e)
        {
            // check if we have selected a chummer in the list
            if (chkBoxChummer.SelectedItem == null)
                MessageBox.Show("Please Select a Chummer to remove");
            else if (characters[chkBoxChummer.SelectedIndex].InitRoll - 1 >= 0)
                ApplyInitChange(-10);
            else
                MessageBox.Show("unable to go beyond 0");
        }

        /*
         * Add's 1 init
         */
        private void btnAdd1Init_Click(object sender, EventArgs e)
        {
            // check if we have selected a chummer in the list
            if (chkBoxChummer.SelectedItem == null)
                MessageBox.Show("Please Select a Chummer to remove");
            else if (characters[chkBoxChummer.SelectedIndex].InitRoll - 1 >= 0)
                ApplyInitChange(1);
            else
                MessageBox.Show("unable to go beyond 0");
        }
        /*
         * Add's 5 init
         */
        private void btnAdd5Init_Click(object sender, EventArgs e)
        {
            // check if we have selected a chummer in the list
            if (chkBoxChummer.SelectedItem == null)
                MessageBox.Show("Please Select a Chummer to remove");
            else if (characters[chkBoxChummer.SelectedIndex].InitRoll - 1 >= 0)
                ApplyInitChange(5);
            else
                MessageBox.Show("unable to go beyond 0");
        }

        /*
         * Add's 10 init
         */
        private void btnAdd10Init_Click(object sender, EventArgs e)
        {
            // check if we have selected a chummer in the list
            if (chkBoxChummer.SelectedItem == null)
                MessageBox.Show("Please Select a Chummer to remove");
            else if (characters[chkBoxChummer.SelectedIndex].InitRoll - 1 >= 0)
                ApplyInitChange(10);
            else
                MessageBox.Show("unable to go beyond 0");
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
        private void btnNext_Click(object sender, EventArgs e)
        {
            if (_blnFinishedCombatTurn)
                return; // cannot go to "next"
            if (_intIndex == characters.Count - totalChummersWithNoInit)
            {
                // increment the round count since we have reached the end of the list
                lblRound.Text = "Round " + (_intRound++ + 1);
                // reset the the round with a minus ten on all
                int index = -1;
                for (int i = 0; i < characters.Count; i++)
                {
                    if (characters[i].InitRoll > 0)
                    {
                        chkBoxChummer.SelectedIndex = i;
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
                while (_intIndex < characters.Count && characters[_intIndex].InitRoll <= 0)
                    _intIndex++;

                // check if there are no more chummer's which can move
                if (_intIndex == characters.Count)
                {
                    _blnFinishedCombatTurn = true;
                    _intIndex = 0;
                    return; // we are finished
                }

                chkBoxChummer.SelectedIndex = _intIndex;
                _intIndex += 1;
            }
        }

        /*
         * Sorts the characters based on Initiative
         */
        private void btnSort_Click(object sender, EventArgs e)
        {
            characters = characters.OrderByDescending(o => o.InitRoll).ToList();
            chkBoxChummer.Items.Clear();
            foreach (Character character in characters)
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
                MessageBox.Show("please select a chummer");
            else if (characters[chkBoxChummer.SelectedIndex].InitRoll < 1)
                MessageBox.Show("unable to delay chummer with no init");
            else
            {
                int index = chkBoxChummer.SelectedIndex;
                Character character = characters[index];

                // update the position of the chummer to the next highest initative - 1 in regards to other delayed characters
                // i.e. if the chummer delaying has 29 init and their is a chummer with 30 init, move the delayed chummer above it
                int tempIndex = characters.Count - 1;
                for (int i = 0; i < characters.Count; i++)
                {
                    if (characters[i].InitRoll < characters[index].InitRoll && characters[index].Delayed)
                    {
                        // we have found the first (since it's sorted) chummer with a larger value init roll\
                        tempIndex = i;
                        break;
                    }
                }

                characters.RemoveAt(index);
                characters.Insert(tempIndex, character);
                // back up one for indexing purposes
                _intIndex -= 1;
                ResetListBoxChummers();
            }
        }

        /*
         * Reset button pressed
         */
        private void btnReset_Click(object sender, EventArgs e)
        {
            // for every checked character, we re-roll init
            for (int i = 0; i < characters.Count; i++)
            {
                if (chkBoxChummer.CheckedIndices.Contains(i))
                {
                    Character objLoopCharacter = characters[i];
                    int intInitPasses = objLoopCharacter.InitPasses;
                    int intInitRoll = intInitPasses;
                    for (int j = 0; j < intInitPasses; j++)
                    {
                        intInitRoll += GlobalOptions.RandomGenerator.NextD6ModuloBiasRemoved();
                    }
                    objLoopCharacter.InitRoll = intInitRoll + objLoopCharacter.InitialInit;
                }
            }

            // query for new initiatives
            for (int j = 0; j < characters.Count; j++)
            {
                if (chkBoxChummer.GetItemCheckState(j) == CheckState.Unchecked)
                {
                    Character objLoopCharacter = characters[j];
                    frmInitRoller frmHits = new frmInitRoller
                    {
                        Text = "Initiative: " + objLoopCharacter.Name,
                        Description = "initiative result",
                        Dice = objLoopCharacter.InitPasses
                    };
                    frmHits.ShowDialog(this);

                    if (frmHits.DialogResult != DialogResult.OK)
                        return;   // we decided not to actually change the initiative
                    objLoopCharacter.InitRoll = frmHits.Result + objLoopCharacter.InitialInit;
                }
            }

            ResetListBoxChummers();
            _blnFinishedCombatTurn = false;
            _intIndex = 0;
            _intRound = 1;
            lblRound.Text = "Round 1";
            totalChummersWithNoInit = 0;
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
            if (characters[index].Delayed && index != _intIndex)
            {
                DialogResult result = MessageBox.Show("Would you like the chummer to perform a delayed action?", "Delayed Action", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    // un-delay character, and lock it in the current location
                    Character character = characters[index];
                    character.Delayed = false;

                    // place the chummer as the current chummer
                    characters.RemoveAt(index);
                    characters.Insert(_intIndex, character);

                    ResetListBoxChummers();
                }
            }
            else if (characters[index].Delayed && index == _intIndex)
            {
                // it is the chummers turn and we should just turn off the delayed action
                Character character = characters[index];
                character.Delayed = false;
                characters[index] = character;
                chkBoxChummer.Items[index] = characters[index];
            }
        }

        /*
         * Should work...
         * When the user right-clicks somewhere in the check box list.
         */
        private void chkBoxChummer_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // confirm we are selecting a chummer
                if (chkBoxChummer.SelectedItem == null)
                    MessageBox.Show("Please select a chummer before right-clicking");

                frmInitRoller frmHits = new frmInitRoller
                {
                    Dice = characters[chkBoxChummer.SelectedIndex].InitPasses
                };
                frmHits.ShowDialog(this);

                if (frmHits.DialogResult != DialogResult.OK)
                    return;   // we decided not to actually change the initiative

                characters[chkBoxChummer.SelectedIndex].InitRoll = frmHits.Result;

                chkBoxChummer.Items[chkBoxChummer.SelectedIndex] = characters[chkBoxChummer.SelectedIndex];
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// The current character in the chain of initiatives
        /// </summary>
        public Character CurrentCharacter => characters[_intIndex];

        /// <summary>
        /// Add's the token to the initiative chain
        /// </summary>
        /// <param name="character"></param>
        public void AddToken(Character character)
        {
            if (character.InitRoll == int.MinValue)
            {
                frmInitRoller frmHits = new frmInitRoller
                {
                    Dice = character.InitPasses
                };
                frmHits.ShowDialog(this);

                if (frmHits.DialogResult != DialogResult.OK)
                {
                    MessageBox.Show("ERROR");   // TODO edward show error
                    return;
                }

                character.InitRoll = frmHits.Result + character.InitialInit;
            }

            characters.Add(character);
            chkBoxChummer.Items.Add(character);
        }

        /*
         * Applies the specified amount of initiative to the 
         * currently selected player
         */
        private void ApplyInitChange(int value)
        {
            // check if we have selected a chummer in the list
            if (chkBoxChummer.SelectedItem == null)
                MessageBox.Show("Please Select a Chummer to remove");
            else
            {
                // pull the simple character out
                int index = chkBoxChummer.SelectedIndex;
                characters[index].InitRoll += value;

                // if negative or 0 init add to the count
                if (characters[index].InitRoll < 1)
                    totalChummersWithNoInit++;

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
            foreach (Character aCharacter in characters)
                chkBoxChummer.Items.Add(aCharacter);
        }
        #endregion
    }
}
