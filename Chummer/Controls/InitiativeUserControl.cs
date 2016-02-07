using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using System.Windows.Forms;

namespace Chummer
{
    public partial class InitiativeUserControl : UserControl
    {
        #region Properties

        /// <summary>
        /// Fired when the current character is changed
        /// </summary>
        public event EventHandler CurrentCharacterChanged
        {
            add { chkBoxChummer.SelectedValueChanged += value; }
            remove { chkBoxChummer.SelectedValueChanged -= value; }
        }

        #endregion

        private List<Tuple<Character, bool>> _characters;
        private int index;
        private int round;
        private int turn;
        private bool finishedCombatTurn;
        private int totalChummersWithNoInit;
        private bool fastmode = true;

        /// <summary>
        /// Default constructor
        /// </summary>
        public InitiativeUserControl()
        {
            InitializeComponent();
            _characters = new List<Tuple<Character, bool>>();
            round = 1;
            turn = 0;
            lblRound.Text = lblRound.Text.Split(' ')[0] + $" {round}";
            lblTurn.Text = lblTurn.Text.Split(' ')[0] + $" {turn}";


            finishedCombatTurn = false;

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
                int selectedIndex = chkBoxChummer.SelectedIndex;
                chkBoxChummer.Items.RemoveAt(selectedIndex);
                if (chkBoxChummer.Items.Count > 0)
                    chkBoxChummer.SelectedIndex = 0; // reset the selected item to the first item in the list
                _characters.RemoveAt(selectedIndex);
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
            else if (_characters[chkBoxChummer.SelectedIndex].Item1.InitRoll - 1 >= 0)
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
            else if (_characters[chkBoxChummer.SelectedIndex].Item1.InitRoll - 5 >= 0)
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
            else if (_characters[chkBoxChummer.SelectedIndex].Item1.InitRoll - 1 >= 0)
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
            else if (_characters[chkBoxChummer.SelectedIndex].Item1.InitRoll - 1 >= 0)
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
            else if (_characters[chkBoxChummer.SelectedIndex].Item1.InitRoll - 1 >= 0)
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
                MessageBox.Show(@"Please Select a Chummer to remove");
            else if (_characters[chkBoxChummer.SelectedIndex].Item1.InitRoll - 1 >= 0)
                ApplyInitChange(10);
            else
                MessageBox.Show(@"unable to go beyond 0");
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
            SelectNextChar();
        }

        private void SelectNextChar()
        {
            if (finishedCombatTurn)
            {
                if (chkFastMode.Checked)
                {
                    ResetRound();
                    return;
                }
                this.btnNext.Enabled = false;
                MessageBox.Show("Combatturn finished, press Reset Initiave!");
                ResetRound();
                return;
            }
            if (_characters.Count == 0)
            {
                this.btnNext.Enabled = false;
                MessageBox.Show("no combatants, please add some");
                return;
            }
            if (index == _characters.Count - totalChummersWithNoInit)
            {
                NextRound();                //möglichkeit a für nextround
            }
            else
            {
                NextChar();                
            }
        }


        /// <summary>
        /// Selects the next Char in the List
        /// </summary>
        private void NextChar()
        {
            // setup the next chummer to go
            while (index < _characters.Count && _characters[index].Item1.InitRoll <= 0)
                index++;

            // check if there are no more chummer's which can move
            if (index == _characters.Count)
            {
                finishedCombatTurn = true;
                index = 0;
                chkBoxChummer.SelectedIndex = index;
                return;
            }
            chkBoxChummer.SelectedIndex = index;
            index++;
        }

        private void NextRound()
        {
            // increment the round count since we have reached the end of the list
            lblRound.Text = "Round " + (round++ + 1);
            // reset the the round with a minus ten on all
            int _index = -1;
            for (int i = 0; i < _characters.Count; i++)
            {
                if (_characters[i].Item1.InitRoll > 0)
                {
                    _index = i;
                    chkBoxChummer.SelectedIndex = i;
                    ApplyInitChange(-10);
                }
            }
            SortCharacterList();
            if (_index == -1)
            {
                Debug.Assert(_index == -1, "Gotcha, finished Combatturn");
                finishedCombatTurn = true;
            }

            if (_characters.Exists(c => c.Item1.InitRoll > 0))
            {
                index = 0;
            }
            else
            {
                this.btnNext.Enabled = false;
                finishedCombatTurn = true;
                if (chkFastMode.Checked)
                {
                    ResetRound();
                    SelectNextChar();
                }
               
            }
        }

        /*
         * Sorts the _characters based on Initiative
         */

        private void btnSort_Click(object sender, EventArgs e)
        {
            SortCharacterList();
        }

        private void SortCharacterList()
        {
            _characters = _characters.OrderByDescending(o => o.Item1.InitRoll).ToList();
            chkBoxChummer.Items.Clear();
            foreach (var character in _characters)
            {
                var i=chkBoxChummer.Items.Add(character.Item1);
                chkBoxChummer.SetItemChecked(i,character.Item2);
            }

            index = 0;
        }

        /*
         * Delays the chosen character
         */

        private void btnDelay_Click(object sender, EventArgs e)
        {
            // make sure a chummer is selected
            if (chkBoxChummer.SelectedItem == null)
                MessageBox.Show("please select a chummer");
            else if (_characters[chkBoxChummer.SelectedIndex].Item1.InitRoll < 1)
                MessageBox.Show("unable to delay chummer with no init");
            else
            {
                int selectedindex = chkBoxChummer.SelectedIndex;
                var character = _characters[selectedindex];

                // update the position of the chummer to the next highest initative - 1 in regards to other delayed _characters
                // i.e. if the chummer delaying has 29 init and their is a chummer with 30 init, move the delayed chummer above it
                int tempIndex = _characters.Count - 1;
                for (int i = 0; i < _characters.Count; i++)
                {
                    if (_characters[i].Item1.InitRoll < _characters[selectedindex].Item1.InitRoll && _characters[selectedindex].Item1.Delayed)
                    {
                        // we have found the first (since it's sorted) chummer with a larger value init roll\
                        tempIndex = i;
                        break;
                    }
                }

                _characters.RemoveAt(selectedindex);
                _characters.Insert(tempIndex, character);
                // back up one for indexing purposes
                this.index--;
                ResetListBoxChummers();
            }
        }

        /*
         * Reset button pressed
         */

        private void btnReset_Click(object sender, EventArgs e)
        {
            ResetRound();
        }

        private void ResetRound()
        {
            if (!_characters.Any())
            {
                MessageBox.Show("No Combatants found, please add some");
                return;
            }
            IncreaseTurn();
            btnNext.Enabled = true;
            // for every checked character, we re-roll init
            Random random = new Random();
            for (int i = 0; i < _characters.Count; i++)
            {
                if (chkBoxChummer.CheckedIndices.Contains(i))
                {
                    _characters[i].Item1.InitRoll =
                        random.Next(_characters[i].Item1.InitPasses, _characters[i].Item1.InitPasses*6) +
                        _characters[i].Item1.InitialInit;
                    var ch = _characters[i].Item1;
                    _characters.RemoveAt(i);
                    _characters.Insert(i, new Tuple<Character, bool>(ch, true));
                }
                else
                {
                    var ch = _characters[i].Item1;
                    _characters.RemoveAt(i);
                    _characters.Insert(i, new Tuple<Character, bool>(ch, false));
                }
            }
            // query for new initiatives
            for (int j = 0; j < _characters.Count; j++)
            {
                if (chkBoxChummer.GetItemCheckState(j) == CheckState.Unchecked)
                {
                    frmInitRoller frmHits = new frmInitRoller
                    {
                        Text = "Initiative: " + _characters[j].Item1.Name,
                        Description = "initiative result",
                        Dice = _characters[j].Item1.InitPasses
                    };
                    frmHits.ShowDialog(this);

                    if (frmHits.DialogResult != DialogResult.OK)
                        return;
                    _characters[j].Item1.InitRoll = frmHits.Result + _characters[j].Item1.InitialInit;
                }
            }
            ResetListBoxChummers();
            SortCharacterList();
            finishedCombatTurn = false;
            index = 0;
            round = 1;
            lblRound.Text = "Round 1";
            totalChummersWithNoInit = 0;
            if (chkBoxChummer.Items.Count > 0)
            {
                chkBoxChummer.SelectedIndex = 0;
            }
        }
        /// <summary>
        /// Increases the turncounter and sets the Label
        /// </summary>
        private void IncreaseTurn()
        {
            turn++;
            lblTurn.Text = $"Turn:{turn}";
        }


        /// <summary>
        /// When the index has changed for the check box list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBoxChummers_SelectedIndexChanged(object sender, EventArgs e)
        {
            // confirm if we are delaying the selected chummer, if we are, ask user if they
            // wish for the chummer to perform a delayed action
            if (chkBoxChummer.SelectedIndex < 0)
                return;
            int l_index = chkBoxChummer.SelectedIndex;
            if (_characters[l_index].Item1.Delayed && l_index != this.index)
            {
                DialogResult result = MessageBox.Show("Would you like the chummer to perform a delayed action?",
                    "Delayed Action", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    // un-delay character, and lock it in the current location
                  var character = _characters[l_index];
                    character.Item1.Delayed = false;

                    // place the chummer as the current chummer
                    _characters.RemoveAt(l_index);
                    _characters.Insert(this.index, character);

                    ResetListBoxChummers();
                }
            }
            else if (_characters[l_index].Item1.Delayed && l_index == this.index)
            {
                // it is the chummers turn and we should just turn off the delayed action
                var character = _characters[l_index];
                character.Item1.Delayed = false;
                _characters[l_index] = character;
                chkBoxChummer.Items[l_index] = _characters[l_index];
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

                frmInitRoller frmHits = new frmInitRoller();
                frmHits.Dice = _characters[chkBoxChummer.SelectedIndex].Item1.InitPasses;
                frmHits.ShowDialog(this);

                if (frmHits.DialogResult != DialogResult.OK)
                    return; // we decided not to actually change the initiative

                _characters[chkBoxChummer.SelectedIndex].Item1.InitRoll = frmHits.Result;

                chkBoxChummer.Items[chkBoxChummer.SelectedIndex] = _characters[chkBoxChummer.SelectedIndex];
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The current character in the chain of initiatives
        /// </summary>
        public Character CurrentCharacter
        {
            get
            {
                if (index >= _characters.Count)
                {
                    return null;
                }
                return _characters[index].Item1;
            }
        }
        /// <summary>
        /// The current selected Character in the Listbox
        /// </summary>
        public Character SelectedCharacter
        {
            get
            {
                if (this.chkBoxChummer.SelectedItem is Character)
                    return (Character) chkBoxChummer.SelectedItem;
                return null;
            }
        }

        /// <summary>
        /// Add's the token to the initiative chain
        /// </summary>
        /// <param name="character"></param>
        /// <param name="autoInit"></param>
        public void AddToken(Character character,bool autoInit)
        {
            if (character.InitRoll == Int32.MinValue)
            {
                frmInitRoller frmHits = new frmInitRoller();
                frmHits.Dice = character.InitPasses;
                frmHits.ShowDialog(this);

                if (frmHits.DialogResult != DialogResult.OK)
                {
                    MessageBox.Show("ERROR"); // TODO edward show error
                    return;
                }

                character.InitRoll = frmHits.Result + character.InitialInit;

            }
            else
            {
                autoInit = true;
            }
            //If you join a fight in a later Initiativeround, your Initiative is reduced by turn times 10
            character.InitRoll += round > 1 ? (round-1)*-10 : 0;
            _characters.Add(new Tuple<Character, bool>(character,autoInit));
            var lindex=chkBoxChummer.Items.Add(character);
            chkBoxChummer.SetItemChecked(lindex, autoInit);
        }

        /// <summary>
        ///  Applies the specified amount of initiative to the currently selected player
        /// </summary>
        /// <param name="value"></param>
        private void ApplyInitChange(int value)
        {
            // check if we have selected a chummer in the list
            if (chkBoxChummer.SelectedItem == null)
                MessageBox.Show("Please Select a Chummer to remove");
            else
            {
                // pull the simple character out
                int selectedIndex = chkBoxChummer.SelectedIndex;
                SetInitRollValue(value, selectedIndex);

                ResetListBoxChummers();
                chkBoxChummer.SelectedIndex = selectedIndex;
            }
        }

        private void SetInitRollValue(int value, int selectedIndex)
        {
            _characters[selectedIndex].Item1.InitRoll += value;

            // if negative or 0 init add to the count
            if (_characters[selectedIndex].Item1.InitRoll < 1)
                totalChummersWithNoInit++;
        }

        /// <summary>
        /// Resets the item in the Check Box List
        /// </summary>
        private void ResetListBoxChummers()
        {

            chkBoxChummer.Items.Clear();
            foreach (var aCharacter in _characters)
            {
               chkBoxChummer.SetItemChecked(chkBoxChummer.Items.Add(aCharacter.Item1),aCharacter.Item2);
            }
        }

        #endregion

        private void btnAddInitPass_Click(object sender, EventArgs e)
        {
            ApplyInitiativePassesChange(1);
        }
        /// <summary>
        /// Adds changeDice Dices to the InitPasses and changes the actual InitRoll accordingly
        /// </summary>
        /// <param name="changeDice"></param>
        private void ApplyInitiativePassesChange(int changeDice)
        {
        // check if we have selected a chummer in the list
            if (chkBoxChummer.SelectedItem == null)
            {
                MessageBox.Show("Please select a Chummer");
                return;
            }
            // pull the simple character out
            int selectedIndex = chkBoxChummer.SelectedIndex;
            if (changeDice > 0 && _characters[selectedIndex].Item1.InitPasses >= 5)
            {
                MessageBox.Show("No more than five initiative dice allowed");
                return;
            }
            if (changeDice < 0 && _characters[selectedIndex].Item1.InitPasses <= 0)
            {
                MessageBox.Show("Not less than zero initiative dice allowed");
                return;
            }
            _characters[selectedIndex].Item1.InitPasses += changeDice;
            int initChange;
            if (chkBoxChummer.GetItemCheckState(selectedIndex) == CheckState.Unchecked)
            {
                frmInitRoller frmHits = new frmInitRoller();
                frmHits.Text = "Initiave change: " + _characters[selectedIndex].Item1.Name;
                frmHits.Description = "dice result";
                frmHits.Dice = Math.Abs(changeDice);
                frmHits.ShowDialog(this);
                if (frmHits.DialogResult != DialogResult.OK)
                {
                    return;
                }
                initChange = changeDice > 0 ? frmHits.Result : -frmHits.Result;
            }
            else
            {
                var random = new Random();
                initChange = changeDice > 0
                    ? random.Next(1*changeDice, 6*changeDice)
                    : random.Next(6*changeDice, 1*changeDice); // if change is negative, min value is multiplied times 6
            }
            ApplyInitChange(initChange);
        }

        private void btnRemoveInitPass_Click(object sender, EventArgs e)
        {
            ApplyInitiativePassesChange(-1);
        }

        private void btnIncreaseBattleInit_Click(object sender, EventArgs e)
        {
            ApplyBattleInitChange(1);
        }

        private void ApplyBattleInitChange(int value)
        {
            if (chkBoxChummer.SelectedItem == null)
                MessageBox.Show("Please Select a Chummer to remove");
            else
            {
                // pull the simple character out
                int selectedIndex = chkBoxChummer.SelectedIndex;
                _characters[selectedIndex].Item1.InitialInit += value;
                ApplyInitChange(value);
            }
        }
        /// <summary>
        /// Applies Damagemodifier to Initiative (factors previos changes)
        /// </summary>
        /// <param name="currentNpc"></param>
        /// <param name="modOld"></param>
        public void ApplyDamage(Character currentNpc,int modOld)
        {
            var modnew = currentNpc.DamageInitModifier;
            var modold = modOld;
            var result = modnew - modold;
            ApplyBattleInitChange(result,currentNpc);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="currentNpc"></param>
        private void ApplyBattleInitChange(int value, Character currentNpc)
        {
           var i= chkBoxChummer.FindString(currentNpc.DisplayInit);
            if (i != -1)
            {
                SetInitRollValue(value,i);
                ResetListBoxChummers();
                chkBoxChummer.SelectedIndex = i;
            }
        }

        private void btnResetBattle_Click(object sender, EventArgs e)
        {
            ResetTurn();
        }

        private void ResetTurn()
        {
            turn = 0;
            ResetRound();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            _characters.Clear();
            chkBoxChummer.Items.Clear();
            ResetTurn();
        }
    }
}