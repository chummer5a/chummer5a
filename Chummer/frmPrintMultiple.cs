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
﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    public partial class frmPrintMultiple : Form
    {
        BackgroundWorker _workerPrinter = new BackgroundWorker();
        List<Character> _lstCharacters = null;

        #region Control Events
        public frmPrintMultiple()
        {
            InitializeComponent();
            LanguageManager.Load(GlobalOptions.Language, this);
            MoveControls();

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
                    TreeNode objNode = new TreeNode();
                    objNode.Text = Path.GetFileName(strFileName);
                    objNode.Tag = strFileName;
                    treCharacters.Nodes.Add(objNode);
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
            }
        }

        private void cmdPrint_Click(object sender, EventArgs e)
        {
            cmdPrint.Enabled = false;
            if (!_workerPrinter.IsBusy)
                _workerPrinter.RunWorkerAsync();
        }

        private void DoPrint(object sender, EventArgs e)
        {
            prgProgress.Value = 0;
            prgProgress.Maximum = treCharacters.Nodes.Count;
            Action funcIncreaseProgress = new Action(() => prgProgress.Value += 1);

            Character[] lstCharacters = new Character[treCharacters.Nodes.Count];
            for (int i = 0; i < lstCharacters.Length; ++i)
            {
                Character objCharacter = lstCharacters[i];
                objCharacter = new Character();
                objCharacter.FileName = treCharacters.Nodes[i].Tag.ToString();
            }
            // Parallelized load because this is one major bottleneck.
            Parallel.ForEach(lstCharacters, objCharacter =>
            {
                objCharacter.Load();
                prgProgress.Invoke(funcIncreaseProgress);
            });

            _lstCharacters = new List<Character>(lstCharacters);
        }

        private frmViewer _frmPrintView;

        public frmViewer PrintViewForm
        {
            get
            {
                return _frmPrintView;
            }
        }

        public List<Character> CharacterList
        {
            get
            {
                return _lstCharacters;
            }
        }

        private void FinishPrint(object sender, EventArgs e)
        {
            cmdPrint.Enabled = true;
            // Set the ProgressBar back to 0.
            prgProgress.Value = 0;

            if (_frmPrintView == null)
            {
                frmViewer _frmPrintView = new frmViewer();
                _frmPrintView.Characters = _lstCharacters;
                _frmPrintView.SelectedSheet = "Game Master Summary";
                _frmPrintView.Show();
            }
            else
            {
                _frmPrintView.Activate();
            }
            _frmPrintView.RefreshView();
        }
        #endregion

        #region Methods
        private void MoveControls()
        {
            int intWidth = Math.Max(cmdSelectCharacter.Width, cmdPrint.Width);
            intWidth = Math.Max(intWidth, cmdDelete.Width);
            cmdSelectCharacter.AutoSize = false;
            cmdPrint.AutoSize = false;
            cmdDelete.AutoSize = false;

            cmdSelectCharacter.Width = intWidth;
            cmdPrint.Width = intWidth;
            cmdDelete.Width = intWidth;
            Width = cmdPrint.Left + cmdPrint.Width + 19;

            prgProgress.Width = Width - prgProgress.Left - 19;
        }
        #endregion
    }
}
