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
    public partial class PrintMultipleCharacters : Form
    {
        private CancellationTokenSource _objPrinterCancellationTokenSource;
        private Task _tskPrinter;
        private Character[] _aobjCharacters;
        private CharacterSheetViewer _frmPrintView;

        #region Control Events

        public PrintMultipleCharacters()
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
        }

        private async void PrintMultipleCharacters_Load(object sender, EventArgs e)
        {
            dlgOpenFile.Filter = await LanguageManager.GetStringAsync("DialogFilter_Chum5") + '|' +
                                 await LanguageManager.GetStringAsync("DialogFilter_All");
        }

        private async void PrintMultipleCharacters_FormClosing(object sender, FormClosingEventArgs e)
        {
            _objPrinterCancellationTokenSource?.Cancel(false);

            await CleanUpOldCharacters();
        }

        private async void cmdSelectCharacter_Click(object sender, EventArgs e)
        {
            // Add the selected Files to the list of characters to print.
            if (dlgOpenFile.ShowDialog(this) == DialogResult.OK)
            {
                await CancelPrint();
                foreach (string strFileName in dlgOpenFile.FileNames)
                {
                    TreeNode objNode = new TreeNode
                    {
                        Text = Path.GetFileName(strFileName) ?? await LanguageManager.GetStringAsync("String_Unknown"),
                        Tag = strFileName
                    };
                    treCharacters.Nodes.Add(objNode);
                }

                if (_frmPrintView != null)
                    await StartPrint();
            }
        }

        private async void cmdDelete_Click(object sender, EventArgs e)
        {
            if (treCharacters.SelectedNode != null)
            {
                await CancelPrint();
                treCharacters.SelectedNode.Remove();
                if (_frmPrintView != null)
                    await StartPrint();
            }
        }

        private async void cmdPrint_Click(object sender, EventArgs e)
        {
            await StartPrint();
        }

        private async ValueTask CancelPrint()
        {
            _objPrinterCancellationTokenSource?.Cancel(false);
            try
            {
                if (_tskPrinter?.IsCompleted == false)
                    await Task.WhenAll(_tskPrinter, cmdPrint.DoThreadSafeAsync(x => x.Enabled = true),
                                       prgProgress.DoThreadSafeAsync(x => ((ProgressBar)x).Value = 0));
                else
                    await Task.WhenAll(cmdPrint.DoThreadSafeAsync(x => x.Enabled = true),
                                       prgProgress.DoThreadSafeAsync(x => ((ProgressBar)x).Value = 0));
            }
            catch (TaskCanceledException)
            {
                // Swallow this
            }
        }

        private async ValueTask StartPrint()
        {
            await CancelPrint();
            _objPrinterCancellationTokenSource?.Dispose();
            _objPrinterCancellationTokenSource = new CancellationTokenSource();
            _tskPrinter = Task.Run(DoPrint, _objPrinterCancellationTokenSource.Token);
        }

        private async Task DoPrint()
        {
            using (CursorWait.New(this, true))
            {
                try
                {
                    await Task.WhenAll(cmdPrint.DoThreadSafeAsync(x => x.Enabled = false),
                        prgProgress.DoThreadSafeAsync(x =>
                        {
                            ProgressBar objBar = (ProgressBar) x;
                            objBar.Value = 0;
                            objBar.Maximum = treCharacters.Nodes.Count;
                        }));

                    // Parallelized load because this is one major bottleneck.
                    Character[] lstCharacters = new Character[treCharacters.Nodes.Count];
                    Task<Character>[] tskLoadingTasks = new Task<Character>[treCharacters.Nodes.Count];
                    for (int i = 0; i < tskLoadingTasks.Length; ++i)
                    {
                        string strLoopFile = treCharacters.Nodes[i].Tag.ToString();
                        tskLoadingTasks[i] = Task.Run(() => InnerLoad(strLoopFile), _objPrinterCancellationTokenSource.Token);
                    }

                    async Task<Character> InnerLoad(string strLoopFile)
                    {
                        if (_objPrinterCancellationTokenSource.IsCancellationRequested)
                        {
                            _objPrinterCancellationTokenSource?.Cancel(false);
                            return null;
                        }

                        Character objReturn = await Program.LoadCharacterAsync(strLoopFile, string.Empty, false, false, false);
                        bool blnLoadSuccessful = objReturn != null;
                        if (_objPrinterCancellationTokenSource.IsCancellationRequested)
                        {
                            _objPrinterCancellationTokenSource?.Cancel(false);
                            return null;
                        }
                        if (blnLoadSuccessful)
                            await prgProgress.DoThreadSafeAsync(() => ++prgProgress.Value);
                        return objReturn;
                    }

                    await Task.WhenAll(tskLoadingTasks);
                    if (_objPrinterCancellationTokenSource.IsCancellationRequested)
                        return;
                    try
                    {
                        for (int i = 0; i < lstCharacters.Length; ++i)
                            lstCharacters[i] = await tskLoadingTasks[i];
                    }
                    catch (TaskCanceledException)
                    {
                        return; // We cancelled the task, so just exit
                    }
                    if (_objPrinterCancellationTokenSource.IsCancellationRequested)
                        return;
                    await CleanUpOldCharacters();
                    if (_objPrinterCancellationTokenSource.IsCancellationRequested)
                        return;
                    _aobjCharacters = lstCharacters;

                    if (_frmPrintView == null)
                    {
                        await this.DoThreadSafeFunc(async () =>
                        {
                            _frmPrintView = new CharacterSheetViewer();
                            await _frmPrintView.SetSelectedSheet("Game Master Summary");
                            await _frmPrintView.SetCharacters(_aobjCharacters);
                            _frmPrintView.Show();
                        });
                    }
                    else
                    {
                        await _frmPrintView.DoThreadSafeFunc(async x =>
                        {
                            CharacterSheetViewer objSheetViewer = (CharacterSheetViewer)x;
                            await objSheetViewer.SetCharacters(_aobjCharacters);
                            objSheetViewer.Activate();
                        });
                    }
                }
                finally
                {
                    await Task.WhenAll(cmdPrint.DoThreadSafeAsync(x => x.Enabled = true),
                        prgProgress.DoThreadSafeAsync(x => ((ProgressBar)x).Value = 0));
                }
            }
        }

        private async ValueTask CleanUpOldCharacters()
        {
            if (!(_aobjCharacters?.Length > 0))
                return;
            // Dispose of any characters who were previous loaded but are no longer needed and don't have any linked characters
            bool blnAnyChanges = true;
            while (blnAnyChanges)
            {
                blnAnyChanges = false;
                foreach (Character objCharacter in _aobjCharacters)
                {
                    if (!await Program.OpenCharacters.ContainsAsync(objCharacter) ||
                        Program.MainForm.OpenCharacterForms.Any(x => x.CharacterObject == objCharacter) ||
                        Program.OpenCharacters.Any(x => x.LinkedCharacters.Contains(objCharacter)))
                        continue;
                    blnAnyChanges = true;
                    Program.OpenCharacters.Remove(objCharacter);
                    await objCharacter.DisposeAsync();
                }
            }
        }

        #endregion Control Events
    }
}
