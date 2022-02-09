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
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chummer
{
    public partial class AddToken : Form
    {
        // used when the user has filled out the information
        private readonly InitiativeUserControl parentControl;

        private bool _blnCharacterAdded;
        private Character _character;

        public AddToken(InitiativeUserControl init)
        {
            InitializeComponent();
            //LanguageManager.Load(GlobalSettings.Language, this);
            CenterToParent();
            parentControl = init;
        }

        /// <summary>
        /// Show the Open File dialogue, then load the selected character.
        /// </summary>
        private async void OpenFile(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = await LanguageManager.GetStringAsync("DialogFilter_Chum5") + '|' + await LanguageManager.GetStringAsync("DialogFilter_All")
            })
                if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                    await LoadCharacter(openFileDialog.FileName);
        }

        /// <summary>
        /// Loads the character
        /// </summary>
        /// <param name="fileName"></param>
        private async Task LoadCharacter(string fileName)
        {
            if (File.Exists(fileName) && fileName.EndsWith(".chum5", StringComparison.OrdinalIgnoreCase))
            {
                Character objCharacter = new Character
                {
                    FileName = fileName
                };
                using (new CursorWait(this))
                {
                    if (!await objCharacter.LoadAsync())
                    {
                        // TODO edward setup error page
                        objCharacter.Dispose();
                        return; // we obviously cannot init
                    }

                    nudInit.Value = objCharacter.InitiativeDice;
                    txtName.Text = objCharacter.Name;
                    if (int.TryParse(objCharacter.Initiative.SplitNoAlloc(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(), out int intTemp))
                        nudInitStart.Value = intTemp;
                    if (_character != null)
                    {
                        _character.Dispose();
                        _blnCharacterAdded = false;
                    }

                    _character = objCharacter;
                }
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
        private async void btnOK_Click(object sender, EventArgs e)
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
                    intInitRoll += GlobalSettings.RandomGenerator.NextD6ModuloBiasRemoved();
                }
                _character.InitRoll = intInitRoll + _character.InitialInit;
            }
            else
                _character.InitRoll = int.MinValue;

            _blnCharacterAdded = true;
            await parentControl.AddToken(_character);
            Close();
        }
    }
}
