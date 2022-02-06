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
    public partial class VersionHistory : Form
    {
        #region Control Events

        public VersionHistory()
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
        }

        private void VersionHistory_Load(object sender, EventArgs e)
        {
            // Display the contents of the changelog.txt file in the TextBox.
            try
            {
                txtHistory.Text = File.ReadAllText(Path.Combine(Utils.GetStartupPath, "changelog.txt"));
            }
            catch
            {
                Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_History_FileNotFound"), LanguageManager.GetString("MessageTitle_FileNotFound"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Close();
                return;
            }
            txtHistory.SelectionStart = 0;
            txtHistory.SelectionLength = 0;
        }

        #endregion Control Events
    }
}
