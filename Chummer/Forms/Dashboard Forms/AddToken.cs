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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chummer
{
    public partial class AddToken : Form
    {
        // used when the user has filled out the information
        private readonly InitiativeUserControl parentControl;
        
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
            string strFilter = await LanguageManager.GetStringAsync("DialogFilter_Chummer").ConfigureAwait(false) + '|'
                +
                await LanguageManager.GetStringAsync("DialogFilter_Chum5").ConfigureAwait(false) + '|' +
                await LanguageManager.GetStringAsync("DialogFilter_Chum5lz").ConfigureAwait(false) + '|' +
                await LanguageManager.GetStringAsync("DialogFilter_All").ConfigureAwait(false);
            string strFileName = string.Empty;
            DialogResult eResult = await this.DoThreadSafeFuncAsync(x =>
            {
                using (OpenFileDialog dlgOpenFile = new OpenFileDialog())
                {
                    dlgOpenFile.Filter = strFilter;
                    DialogResult eReturn = dlgOpenFile.ShowDialog(x);
                    strFileName = dlgOpenFile.FileName;
                    return eReturn;
                }
            }).ConfigureAwait(false);
            if (eResult != DialogResult.OK)
                return;
            await LoadCharacter(strFileName).ConfigureAwait(false);
        }

        /// <summary>
        /// Loads the character
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="token"></param>
        private async ValueTask LoadCharacter(string fileName, CancellationToken token = default)
        {
            if (File.Exists(fileName) && (fileName.EndsWith(".chum5", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".chum5lz", StringComparison.OrdinalIgnoreCase)))
            {
                Character objCharacter = new Character
                {
                    FileName = fileName
                };
                CursorWait objCursorWait = await CursorWait.NewAsync(this, token: token).ConfigureAwait(false);
                try
                {
                    if (!await objCharacter.LoadAsync(token: token).ConfigureAwait(false))
                    {
                        // TODO edward setup error page
                        await objCharacter.DisposeAsync().ConfigureAwait(false);
                        return; // we obviously cannot init
                    }

                    await nudInit.DoThreadSafeAsync(x => x.Value = objCharacter.InitiativeDice, token: token).ConfigureAwait(false);
                    await txtName.DoThreadSafeAsync(x => x.Text = objCharacter.Name, token: token).ConfigureAwait(false);
                    if (int.TryParse(
                            objCharacter.Initiative.SplitNoAlloc(' ', StringSplitOptions.RemoveEmptyEntries)
                                        .FirstOrDefault(), out int intTemp))
                        await nudInitStart.DoThreadSafeAsync(x => x.Value = intTemp, token: token).ConfigureAwait(false);
                    if (_character != null)
                    {
                        await _character.DisposeAsync().ConfigureAwait(false);
                    }

                    _character = objCharacter;
                    Disposed += OnDisposed;
                }
                finally
                {
                    await objCursorWait.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        private void OnDisposed(object sender, EventArgs e)
        {
            _character?.Dispose();
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
                _character.Name = await txtName.DoThreadSafeFuncAsync(x => x.Text).ConfigureAwait(false);
                _character.InitPasses = await nudInit.DoThreadSafeFuncAsync(x => x.ValueAsInt).ConfigureAwait(false);
                _character.Delayed = false;
                _character.InitialInit = await nudInitStart.DoThreadSafeFuncAsync(x => x.ValueAsInt).ConfigureAwait(false);
            }
            else
            {
                _character = new Character
                {
                    Name = await txtName.DoThreadSafeFuncAsync(x => x.Text).ConfigureAwait(false),
                    InitPasses = await nudInit.DoThreadSafeFuncAsync(x => x.ValueAsInt).ConfigureAwait(false),
                    Delayed = false,
                    InitialInit = await nudInitStart.DoThreadSafeFuncAsync(x => x.ValueAsInt).ConfigureAwait(false)
                };
            }
            if (await chkAutoRollInit.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
            {
                int intInitPasses = _character.InitPasses;
                int intInitRoll = intInitPasses;
                for (int j = 0; j < intInitPasses; ++j)
                {
                    intInitRoll += await GlobalSettings.RandomGenerator.NextD6ModuloBiasRemovedAsync().ConfigureAwait(false);
                }
                _character.InitRoll = intInitRoll + _character.InitialInit;
            }
            else
                _character.InitRoll = int.MinValue;

            Disposed -= OnDisposed;
            await parentControl.AddToken(_character).ConfigureAwait(false);
            await this.DoThreadSafeAsync(x => x.Close()).ConfigureAwait(false);
        }
    }
}
