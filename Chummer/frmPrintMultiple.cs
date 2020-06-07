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
using System.ComponentModel;
using System.IO;
 using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chummer
{
    public partial class frmPrintMultiple : Form
    {
        private readonly BackgroundWorker _workerPrinter = new BackgroundWorker();
        List<Character> _lstCharacters;

        #region Control Events
        public frmPrintMultiple()
        {
            InitializeComponent();
            this.TranslateWinForm();
            dlgOpenFile.Filter = LanguageManager.GetString("DialogFilter_Chum5") + '|' + LanguageManager.GetString("DialogFilter_All");

            _workerPrinter.WorkerReportsProgress = true;
            _workerPrinter.WorkerSupportsCancellation = true;

            _workerPrinter.DoWork += DoPrint;
            _workerPrinter.RunWorkerCompleted += FinishPrint;
        }

        private void cmdSelectCharacter_Click(object sender, EventArgs e)
        {
            if (_workerPrinter.IsBusy)
            {
                _workerPrinter.CancelAsync();
                cmdPrint.Enabled = true;
                prgProgress.Value = 0;
            }
            // Add the selected Files to the list of characters to print.
            if (dlgOpenFile.ShowDialog(this) == DialogResult.OK)
            {
                foreach (string strFileName in dlgOpenFile.FileNames)
                {
                    TreeNode objNode = new TreeNode
                    {
                        Text = Path.GetFileName(strFileName) ?? LanguageManager.GetString("String_Unknown"),
                        Tag = strFileName
                    };
                    treCharacters.Nodes.Add(objNode);
                }
                if (_frmPrintView != null)
                {
                    cmdPrint_Click(sender, e);
                }
            }
        }

        private void cmdDelete_Click(object sender, EventArgs e)
        {
            if (treCharacters.SelectedNode != null)
            {
                if (_workerPrinter.IsBusy)
                {
                    _workerPrinter.CancelAsync();
                    cmdPrint.Enabled = true;
                    prgProgress.Value = 0;
                }
                treCharacters.SelectedNode.Remove();
                if (_frmPrintView != null)
                {
                    cmdPrint_Click(sender, e);
                }
            }
        }

        private void cmdPrint_Click(object sender, EventArgs e)
        {
            cmdPrint.Enabled = false;
            if (!_workerPrinter.IsBusy)
            {
                prgProgress.Value = 0;
                prgProgress.Maximum = treCharacters.Nodes.Count;
                _workerPrinter.RunWorkerAsync();
            }
        }

        private void DoPrint(object sender, DoWorkEventArgs e)
        {
            void FuncIncreaseProgress()
            {
                prgProgress.Value += 1;
            }

            Character[] lstCharacters = new Character[treCharacters.Nodes.Count];
            for (int i = 0; i < lstCharacters.Length; ++i)
            {
                if (_workerPrinter.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                lstCharacters[i] = new Character
                {
                    FileName = treCharacters.Nodes[i].Tag.ToString()
                };
            }

            // Parallelized load because this is one major bottleneck.
            Parallel.ForEach(lstCharacters, objCharacter =>
            {
                if (_workerPrinter.CancellationPending)
                    throw new OperationCanceledException();
                objCharacter.Load().RunSynchronously();
                prgProgress.Invoke((Action) FuncIncreaseProgress);
            });

            if (_workerPrinter.CancellationPending)
                e.Cancel = true;
            else
            {
                if (_lstCharacters?.Count > 0)
                    foreach (Character objCharacter in _lstCharacters)
                        objCharacter.Dispose();
                _lstCharacters = new List<Character>(lstCharacters);
            }
        }

        private frmViewer _frmPrintView;

        public frmViewer PrintViewForm => _frmPrintView;

        public IList<Character> CharacterList => _lstCharacters;

        private void FinishPrint(object sender, RunWorkerCompletedEventArgs e)
        {
            cmdPrint.Enabled = true;
            // Set the ProgressBar back to 0.
            prgProgress.Value = 0;

            if (!e.Cancelled)
            {
                if (_frmPrintView == null)
                {
                    _frmPrintView = new frmViewer();
                    _frmPrintView.SetSelectedSheet("Game Master Summary");
                    _frmPrintView.SetCharacters(_lstCharacters?.ToArray());
                    _frmPrintView.Show();
                }
                else
                {
                    _frmPrintView.Activate();
                }
                _frmPrintView.RefreshCharacters();
            }
        }
        #endregion
    }
}
