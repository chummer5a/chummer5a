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
        private readonly CancellationTokenSource _objGenericCancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _objGenericToken;
        private Task _tskPrinter;
        private Character[] _aobjCharacters;
        private CharacterSheetViewer _frmPrintView;

        #region Control Events

        public PrintMultipleCharacters()
        {
            _objGenericToken = _objGenericCancellationTokenSource.Token;
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
        }

        private async void PrintMultipleCharacters_Load(object sender, EventArgs e)
        {
            dlgOpenFile.Title = await LanguageManager.GetStringAsync("Title_PrintMultiple");
            dlgOpenFile.Filter = await LanguageManager.GetStringAsync("DialogFilter_Chum5") + '|' +
                                 await LanguageManager.GetStringAsync("DialogFilter_All");
        }

        private async void PrintMultipleCharacters_FormClosing(object sender, FormClosingEventArgs e)
        {
            CancellationTokenSource objTemp = Interlocked.Exchange(ref _objPrinterCancellationTokenSource, null);
            if (objTemp?.IsCancellationRequested == false)
            {
                objTemp.Cancel(false);
                objTemp.Dispose();
            }
            _objGenericCancellationTokenSource.Cancel(false);
            // ReSharper disable once MethodSupportsCancellation
            await CleanUpOldCharacters();
        }

        private async void cmdSelectCharacter_Click(object sender, EventArgs e)
        {
            // Add the selected Files to the list of characters to print.
            if (await this.DoThreadSafeFuncAsync(x => dlgOpenFile.ShowDialog(x), token: _objGenericToken) != DialogResult.OK)
                return;
            CancellationTokenSource objNewSource = new CancellationTokenSource();
            CancellationTokenSource objTemp = Interlocked.Exchange(ref _objPrinterCancellationTokenSource, objNewSource);
            if (objTemp?.IsCancellationRequested == false)
            {
                objTemp.Cancel(false);
                objTemp.Dispose();
            }
            try
            {
                if (_tskPrinter?.IsCompleted == false)
                    await Task.WhenAll(_tskPrinter, cmdPrint.DoThreadSafeAsync(x => x.Enabled = true, _objGenericToken),
                                       prgProgress.DoThreadSafeAsync(x => x.Value = 0, _objGenericToken));
                else
                    await Task.WhenAll(cmdPrint.DoThreadSafeAsync(x => x.Enabled = true, _objGenericToken),
                                       prgProgress.DoThreadSafeAsync(x => x.Value = 0, _objGenericToken));
                foreach (string strFileName in dlgOpenFile.FileNames)
                {
                    TreeNode objNode = new TreeNode
                    {
                        Text = Path.GetFileName(strFileName) ?? await LanguageManager.GetStringAsync("String_Unknown"),
                        Tag = strFileName
                    };
                    await treCharacters.DoThreadSafeAsync(x => x.Nodes.Add(objNode), _objGenericToken);
                }
            }
            catch (OperationCanceledException)
            {
                Interlocked.CompareExchange(ref _objPrinterCancellationTokenSource, null, objNewSource);
                objNewSource.Dispose();
                return;
            }

            if (_frmPrintView != null)
            {
                CancellationToken objToken = objNewSource.Token;
                _tskPrinter = Task.Run(() => DoPrint(objToken), objToken);
            }
            else
            {
                Interlocked.CompareExchange(ref _objPrinterCancellationTokenSource, null, objNewSource);
                objNewSource.Dispose();
            }
        }

        private async void cmdDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (await treCharacters.DoThreadSafeFuncAsync(x => x.SelectedNode, _objGenericToken) != null)
                {
                    CancellationTokenSource objNewSource = new CancellationTokenSource();
                    CancellationTokenSource objTemp = Interlocked.Exchange(ref _objPrinterCancellationTokenSource, objNewSource);
                    if (objTemp?.IsCancellationRequested == false)
                    {
                        objTemp.Cancel(false);
                        objTemp.Dispose();
                    }
                    try
                    {
                        if (_tskPrinter?.IsCompleted == false)
                            await Task.WhenAll(_tskPrinter, cmdPrint.DoThreadSafeAsync(x => x.Enabled = true, _objGenericToken),
                                               prgProgress.DoThreadSafeAsync(x => x.Value = 0, _objGenericToken));
                        else
                            await Task.WhenAll(cmdPrint.DoThreadSafeAsync(x => x.Enabled = true, _objGenericToken),
                                               prgProgress.DoThreadSafeAsync(x => x.Value = 0, _objGenericToken));
                        await treCharacters.DoThreadSafeAsync(x => x.SelectedNode.Remove(), _objGenericToken);
                    }
                    catch (OperationCanceledException)
                    {
                        Interlocked.CompareExchange(ref _objPrinterCancellationTokenSource, null, objNewSource);
                        objNewSource.Dispose();
                        return;
                    }
                    catch
                    {
                        Interlocked.CompareExchange(ref _objPrinterCancellationTokenSource, null, objNewSource);
                        objNewSource.Dispose();
                        throw;
                    }
                    if (_frmPrintView != null)
                    {
                        CancellationToken objToken = objNewSource.Token;
                        _tskPrinter = Task.Run(() => DoPrint(objToken), objToken);
                    }
                    else
                    {
                        Interlocked.CompareExchange(ref _objPrinterCancellationTokenSource, null, objNewSource);
                        objNewSource.Dispose();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                //swallow this
            }
        }

        private async void cmdPrint_Click(object sender, EventArgs e)
        {
            CancellationTokenSource objNewSource = new CancellationTokenSource();
            CancellationTokenSource objTemp = Interlocked.Exchange(ref _objPrinterCancellationTokenSource, objNewSource);
            if (objTemp?.IsCancellationRequested == false)
            {
                objTemp.Cancel(false);
                objTemp.Dispose();
            }
            try
            {
                if (_tskPrinter?.IsCompleted == false)
                    await Task.WhenAll(_tskPrinter, cmdPrint.DoThreadSafeAsync(x => x.Enabled = true, _objGenericToken),
                                       prgProgress.DoThreadSafeAsync(x => x.Value = 0, _objGenericToken));
                else
                    await Task.WhenAll(cmdPrint.DoThreadSafeAsync(x => x.Enabled = true, _objGenericToken),
                                       prgProgress.DoThreadSafeAsync(x => x.Value = 0, _objGenericToken));
            }
            catch (OperationCanceledException)
            {
                Interlocked.CompareExchange(ref _objPrinterCancellationTokenSource, null, objNewSource);
                objNewSource.Dispose();
                return;
            }
            catch
            {
                Interlocked.CompareExchange(ref _objPrinterCancellationTokenSource, null, objNewSource);
                objNewSource.Dispose();
                throw;
            }
            CancellationToken objToken = objNewSource.Token;
            _tskPrinter = Task.Run(() => DoPrint(objToken), objToken);
        }

        private async Task DoPrint(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            using (await CursorWait.NewAsync(this, true, token))
            {
                try
                {
                    token.ThrowIfCancellationRequested();
                    int intNodesCount = await treCharacters.DoThreadSafeFuncAsync(x => x.Nodes.Count, token);
                    await Task.WhenAll(cmdPrint.DoThreadSafeAsync(x => x.Enabled = false, token),
                                       prgProgress.DoThreadSafeAsync(objBar =>
                                       {
                                           objBar.Value = 0;
                                           objBar.Maximum = intNodesCount;
                                       }, token));
                    token.ThrowIfCancellationRequested();
                    // Parallelized load because this is one major bottleneck.
                    Character[] lstCharacters = new Character[intNodesCount];
                    Task<Character>[] tskLoadingTasks = new Task<Character>[intNodesCount];
                    for (int i = 0; i < tskLoadingTasks.Length; ++i)
                    {
                        string strLoopFile = await treCharacters.DoThreadSafeFuncAsync(x => x.Nodes[i].Tag.ToString(), token);
                        tskLoadingTasks[i]
                            = Task.Run(() => InnerLoad(strLoopFile, token), token);
                    }

                    async Task<Character> InnerLoad(string strLoopFile, CancellationToken innerToken = default)
                    {
                        innerToken.ThrowIfCancellationRequested();

                        Character objReturn;
                        using (LoadingBar frmLoadingBar = await Program.CreateAndShowProgressBarAsync(strLoopFile, Character.NumLoadingSections))
                            objReturn = await Program.LoadCharacterAsync(strLoopFile, string.Empty, false, false, frmLoadingBar, innerToken);
                        bool blnLoadSuccessful = objReturn != null;
                        innerToken.ThrowIfCancellationRequested();

                        if (blnLoadSuccessful)
                            await prgProgress.DoThreadSafeAsync(() => ++prgProgress.Value, innerToken);
                        return objReturn;
                    }

                    await Task.WhenAll(tskLoadingTasks);
                    token.ThrowIfCancellationRequested();
                    for (int i = 0; i < lstCharacters.Length; ++i)
                        lstCharacters[i] = await tskLoadingTasks[i];
                    token.ThrowIfCancellationRequested();
                    await CleanUpOldCharacters(token);
                    token.ThrowIfCancellationRequested();
                    _aobjCharacters = lstCharacters;

                    if (_frmPrintView == null)
                    {
                        _frmPrintView = await this.DoThreadSafeFuncAsync(() => new CharacterSheetViewer(), token);
                        await _frmPrintView.SetSelectedSheet("Game Master Summary", token);
                        await _frmPrintView.SetCharacters(token, _aobjCharacters);
                        await _frmPrintView.DoThreadSafeAsync(x => x.Show(), token);
                    }
                    else
                    {
                        await _frmPrintView.SetCharacters(token, _aobjCharacters);
                        await _frmPrintView.DoThreadSafeAsync(x => x.Activate(), token);
                    }
                }
                finally
                {
                    await Task.WhenAll(cmdPrint.DoThreadSafeAsync(x => x.Enabled = true, token),
                                       prgProgress.DoThreadSafeAsync(x => x.Value = 0, token));
                }
            }
        }

        private async ValueTask CleanUpOldCharacters(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!(_aobjCharacters?.Length > 0))
                return;
            // Dispose of any characters who were previous loaded but are no longer needed and don't have any linked characters
            bool blnAnyChanges = true;
            while (blnAnyChanges)
            {
                token.ThrowIfCancellationRequested();
                blnAnyChanges = false;
                foreach (Character objCharacter in _aobjCharacters)
                {
                    if (!await Program.OpenCharacters.ContainsAsync(objCharacter)
                        || await Program.OpenCharacters.AnyAsync(x => x.LinkedCharacters.Contains(objCharacter), token)
                        || Program.MainForm.OpenFormsWithCharacters.Any(x => x.CharacterObjects.Contains(objCharacter)))
                        continue;
                    blnAnyChanges = true;
                    await Program.OpenCharacters.RemoveAsync(objCharacter);
                    token.ThrowIfCancellationRequested();
                }
            }
        }

        #endregion Control Events
    }
}
