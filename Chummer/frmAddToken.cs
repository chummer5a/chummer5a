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
 using System.IO;
 using System.Windows.Forms;

namespace Chummer
{
    public partial class frmAddToken : Form
    {
        // used when the user has filled out the information
        private readonly InitiativeUserControl parentControl;
        private Character _character;

        public frmAddToken(InitiativeUserControl init)
        {
            InitializeComponent();
            //LanguageManager.Load(GlobalOptions.Language, this);
            CenterToParent();
            parentControl = init;

        }

        /// <summary>
        /// Show the Open File dialogue, then load the selected character.
        /// </summary>
        private void OpenFile(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = LanguageManager.GetString("DialogFilter_Chum5", GlobalOptions.Language) + '|' + LanguageManager.GetString("DialogFilter_All", GlobalOptions.Language)
            };

            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                LoadCharacter(openFileDialog.FileName);
        }

        /// <summary>
        /// Loads the character
        /// </summary>
        /// <param name="fileName"></param>
        private void LoadCharacter(string fileName)
        {
            if (File.Exists(fileName) && fileName.EndsWith("chum5"))
            {
                Cursor = Cursors.WaitCursor;
                Character objCharacter = new Character
                {
                    FileName = fileName
                };
                if (!objCharacter.Load())
                {
                    Cursor = Cursors.Default;   // TODO edward setup error page
                    return; // we obviously cannot init
                }

                nudInit.Value = objCharacter.InitiativeDice;
                txtName.Text = objCharacter.Name;
                if (int.TryParse(objCharacter.Initiative.Split(' ')[0], out int intTemp))
                    nudInitStart.Value = intTemp;
                _character = objCharacter;
                Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Closes the add token dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// passes the character back to the dashboard init user control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            if (_character != null)
            {
                _character.Name = txtName.Text;
                _character.InitPasses = (int)nudInit.Value;
                _character.Delayed = false;
                _character.InitialInit = (int)nudInitStart.Value;
            }
            else
            {
                _character = new Character
                {
                    Name = txtName.Text,
                    InitPasses = (int)nudInit.Value,
                    Delayed = false,
                    InitialInit = (int)nudInitStart.Value
                };
            }
            if (chkAutoRollInit.Checked)
            {
                int intInitPasses = _character.InitPasses;
                int intInitRoll = intInitPasses;
                for (int j = 0; j < intInitPasses; ++j)
                {
                    intInitRoll += GlobalOptions.RandomGenerator.NextD6ModuloBiasRemoved();
                }
                _character.InitRoll = intInitRoll + _character.InitialInit;
            }
            else
                _character.InitRoll = int.MinValue;

            parentControl.AddToken(_character);
            Close();
        }
    }
}
