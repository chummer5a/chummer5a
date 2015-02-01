using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
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
            add { this.chkBoxChummer.SelectedValueChanged += value; }
            remove { this.chkBoxChummer.SelectedValueChanged -= value; }
        }
        #endregion

        private List<Character> characters;
        private int index;
        private int round;
        private bool finishedCombatTurn;
        private int totalChummersWithNoInit;

        /// <summary>
        /// Default constructor
        /// </summary>
        public InitiativeUserControl()
        {
            InitializeComponent();
            this.characters = new List<Character>();
            this.lblRound.Text = this.lblRound.Text.Split(' ')[0] + " 1";
            this.round = 1;
            this.finishedCombatTurn = false;

            // setup the list of chummers to show 
            this.chkBoxChummer.DisplayMember = "DisplayInit";
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
            if (this.chkBoxChummer.SelectedItem == null)
                MessageBox.Show("Please Select a Chummer to remove");
            else
            {
                int index = this.chkBoxChummer.SelectedIndex;
                this.chkBoxChummer.Items.RemoveAt(index);
                if (this.chkBoxChummer.Items.Count > 0)
                    this.chkBoxChummer.SelectedIndex = 0; // reset the selected item to the first item in the list
                this.characters.RemoveAt(index);
            }
        }

        /*
         * Subtracts 1 init
         */
        private void btnMinusInit1_Click(object sender, EventArgs e)
        {
            // check if we have selected a chummer in the list
            if (this.chkBoxChummer.SelectedItem == null)
                MessageBox.Show("Please Select a Chummer to remove");
            else if (this.characters[this.chkBoxChummer.SelectedIndex].InitRoll - 1 >= 0)
                this.ApplyInitChange(-1);
            else
                MessageBox.Show("unable to go beyond 0");
        }

        /*
         * Subtracts 5 init
         */
        private void btnMinus5Init_Click(object sender, EventArgs e)
        {
            // check if we have selected a chummer in the list
            if (this.chkBoxChummer.SelectedItem == null)
                MessageBox.Show("Please Select a Chummer to remove");
            else if (this.characters[this.chkBoxChummer.SelectedIndex].InitRoll - 5 >= 0)
                this.ApplyInitChange(-5);
            else
                MessageBox.Show("unable to go beyond 0");
        }

        /*
         * Subtracts 10 init
         */
        private void btnMinus10Init_Click(object sender, EventArgs e)
        {
            // check if we have selected a chummer in the list
            if (this.chkBoxChummer.SelectedItem == null)
                MessageBox.Show("Please Select a Chummer to remove");
            else if (this.characters[this.chkBoxChummer.SelectedIndex].InitRoll - 1 >= 0)
                this.ApplyInitChange(-10);
            else
                MessageBox.Show("unable to go beyond 0");
        }

        /*
         * Add's 1 init
         */
        private void btnAdd1Init_Click(object sender, EventArgs e)
        {
            // check if we have selected a chummer in the list
            if (this.chkBoxChummer.SelectedItem == null)
                MessageBox.Show("Please Select a Chummer to remove");
            else if (this.characters[this.chkBoxChummer.SelectedIndex].InitRoll - 1 >= 0)
                this.ApplyInitChange(1);
            else
                MessageBox.Show("unable to go beyond 0");
        }
        /*
         * Add's 5 init
         */
        private void btnAdd5Init_Click(object sender, EventArgs e)
        {
            // check if we have selected a chummer in the list
            if (this.chkBoxChummer.SelectedItem == null)
                MessageBox.Show("Please Select a Chummer to remove");
            else if (this.characters[this.chkBoxChummer.SelectedIndex].InitRoll - 1 >= 0)
                this.ApplyInitChange(5);
            else
                MessageBox.Show("unable to go beyond 0");
        }

        /*
         * Add's 10 init
         */
        private void btnAdd10Init_Click(object sender, EventArgs e)
        {
            // check if we have selected a chummer in the list
            if (this.chkBoxChummer.SelectedItem == null)
                MessageBox.Show("Please Select a Chummer to remove");
            else if (this.characters[this.chkBoxChummer.SelectedIndex].InitRoll - 1 >= 0)
                this.ApplyInitChange(10);
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
            if (this.finishedCombatTurn)
                return; // cannot go to "next"
            if (this.index == this.characters.Count - this.totalChummersWithNoInit)
            {
                // increment the round count since we have reached the end of the list
                this.lblRound.Text = "Round " + (this.round++ + 1);
                // reset the the round with a minus ten on all
                int _index = -1;
                for (int i = 0; i < this.characters.Count; i++)
                {
                    if (this.characters[i].InitRoll > 0)
                    {
                        this.chkBoxChummer.SelectedIndex = i;
                        this.ApplyInitChange(-10);
                        _index = i;
                    }
                }

                if (_index == -1)
                    this.finishedCombatTurn = true;

                this.index = 0;
            }
            else
            {
                // setup the next chummer to go
                while (this.index < this.characters.Count && this.characters[this.index].InitRoll <= 0)
                    this.index++;

                // check if there are no more chummer's which can move
                if (this.index == this.characters.Count)
                {
                    this.finishedCombatTurn = true;
                    this.index = 0;
                    return; // we are finished
                }

                this.chkBoxChummer.SelectedIndex = this.index;
                this.index = this.index + 1;
            }
        }

        /*
         * Sorts the characters based on Initiative
         */
        private void btnSort_Click(object sender, EventArgs e)
        {
            this.characters = this.characters.OrderByDescending(o => o.InitRoll).ToList<Character>();
            this.chkBoxChummer.Items.Clear();
            foreach (Character character in this.characters)
                this.chkBoxChummer.Items.Add(character);

            this.index = 0;
        }

        /*
         * Delays the chosen character
         */
        private void btnDelay_Click(object sender, EventArgs e)
        {
            // make sure a chummer is selected
            if (this.chkBoxChummer.SelectedItem == null)
                MessageBox.Show("please select a chummer");
            else if (this.characters[this.chkBoxChummer.SelectedIndex].InitRoll < 1)
                MessageBox.Show("unable to delay chummer with no init");
            else
            {
                int index = this.chkBoxChummer.SelectedIndex;
                Character character = this.characters[index];

                // update the position of the chummer to the next highest initative - 1 in regards to other delayed characters
                // i.e. if the chummer delaying has 29 init and their is a chummer with 30 init, move the delayed chummer above it
                int tempIndex = this.characters.Count - 1;
                for (int i = 0; i < this.characters.Count; i++)
                {
                    if (this.characters[i].InitRoll < this.characters[index].InitRoll && this.characters[index].Delayed)
                    {
                        // we have found the first (since it's sorted) chummer with a larger value init roll\
                        tempIndex = i;
                        break;
                    }
                }

                this.characters.RemoveAt(index);
                this.characters.Insert(tempIndex, character);
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
            // for every checked character, we re-roll init
            Random random = new Random();
            for (int i = 0; i < this.characters.Count; i++)
            {
                if (this.chkBoxChummer.CheckedIndices.Contains(i))
                    this.characters[i].InitRoll = random.Next(this.characters[i].InitPasses, this.characters[i].InitPasses * 6) + this.characters[i].InitialInit;
            }

            // query for new initiatives
            for (int j = 0; j < this.characters.Count; j++)
            {
                if (this.chkBoxChummer.GetItemCheckState(j) == CheckState.Unchecked)
                {
                    frmInitRoller frmHits = new frmInitRoller();
                    frmHits.Text = "Initiative: " + this.characters[j].Name;
                    frmHits.Description = "initiative result";
                    frmHits.Dice = this.characters[j].InitPasses;
                    frmHits.ShowDialog(this);

                    if (frmHits.DialogResult != DialogResult.OK)
                        return;   // we decided not to actually change the initiative
                    this.characters[j].InitRoll = frmHits.Result + this.characters[j].InitialInit;
                }
            }

            this.ResetListBoxChummers();
            this.finishedCombatTurn = false;
            this.index = 0;
            this.round = 1;
            this.lblRound.Text = "Round 1";
            this.totalChummersWithNoInit = 0;
        }

        /*
         * When the index has changed for the check box list
         */
        private void listBoxChummers_SelectedIndexChanged(object sender, EventArgs e)
        {
            // confirm if we are delaying the selected chummer, if we are, ask user if they
            // wish for the chummer to perform a delayed action
            if (this.chkBoxChummer.SelectedIndex < 0)
                return;
            int index = this.chkBoxChummer.SelectedIndex;
            if (this.characters[index].Delayed && index != this.index)
            {
                DialogResult result = MessageBox.Show("Would you like the chummer to perform a delayed action?", "Delayed Action", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    // un-delay character, and lock it in the current location
                    Character character = this.characters[index];
                    character.Delayed = false;

                    // place the chummer as the current chummer
                    this.characters.RemoveAt(index);
                    this.characters.Insert(this.index, character);

                    ResetListBoxChummers();
                }
            }
            else if (this.characters[index].Delayed && index == this.index)
            {
                // it is the chummers turn and we should just turn off the delayed action
                Character character = this.characters[index];
                character.Delayed = false;
                this.characters[index] = character;
                this.chkBoxChummer.Items[index] = this.characters[index];
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
                if (this.chkBoxChummer.SelectedItem == null)
                    MessageBox.Show("Please select a chummer before right-clicking");

                frmInitRoller frmHits = new frmInitRoller();
                frmHits.Dice = this.characters[this.chkBoxChummer.SelectedIndex].InitPasses;
                frmHits.ShowDialog(this);

                if (frmHits.DialogResult != DialogResult.OK)
                    return;   // we decided not to actually change the initiative

                this.characters[this.chkBoxChummer.SelectedIndex].InitRoll = frmHits.Result;

                this.chkBoxChummer.Items[this.chkBoxChummer.SelectedIndex] = this.characters[this.chkBoxChummer.SelectedIndex];
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// The current character in the chain of initiatives
        /// </summary>
        public Character CurrentCharacter { get { return this.characters[index]; } }

        /// <summary>
        /// Add's the token to the initiative chain
        /// </summary>
        /// <param name="character"></param>
        public void AddToken(Character character)
        {
            if (character.InitRoll == Int32.MinValue)
            {
                frmInitRoller frmHits = new frmInitRoller();
                frmHits.Dice = character.InitPasses;
                frmHits.ShowDialog(this);

                if (frmHits.DialogResult != DialogResult.OK)
                {
                    MessageBox.Show("ERROR");   // TODO edward show error
                    return;
                }

                character.InitRoll = frmHits.Result + character.InitialInit;
            }

            this.characters.Add(character);
            this.chkBoxChummer.Items.Add(character);
        }

        /*
         * Applies the specified amount of initiative to the 
         * currently selected player
         */
        private void ApplyInitChange(int value)
        {
            // check if we have selected a chummer in the list
            if (this.chkBoxChummer.SelectedItem == null)
                MessageBox.Show("Please Select a Chummer to remove");
            else
            {
                // pull the simple character out
                int index = this.chkBoxChummer.SelectedIndex;
                this.characters[index].InitRoll += value;

                // if negative or 0 init add to the count
                if (this.characters[index].InitRoll < 1)
                    this.totalChummersWithNoInit++;

                this.ResetListBoxChummers();
                this.chkBoxChummer.SelectedIndex = index;
            }
        }

        /*
         * Resets the item in the Check Box List
         */
        private void ResetListBoxChummers()
        {
            this.chkBoxChummer.Items.Clear();
            foreach (Character aCharacter in this.characters)
                this.chkBoxChummer.Items.Add(aCharacter);
        }
        #endregion
    }
}
