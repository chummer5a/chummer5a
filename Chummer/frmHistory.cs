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
using System.IO;
using System.Windows.Forms;

namespace Chummer
{
    public partial class frmHistory : Form
    {
        #region Control Events
        public frmHistory()
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
        }

        private void frmHistory_Load(object sender, EventArgs e)
        {
            // Display the contents of the changelog.txt file in the TextBox.
            try
            {
                txtRevisionHistory.Text = File.ReadAllText(Path.Combine(Application.StartupPath, "changelog.txt"));
            }
            catch
            {
                MessageBox.Show(LanguageManager.Instance.GetString("Message_History_FileNotFound"), LanguageManager.Instance.GetString("MessageTitle_FileNotFound"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Close();
                return;
            }
            txtRevisionHistory.SelectionStart = 0;
            txtRevisionHistory.SelectionLength = 0;
        }
        #endregion
    }
}