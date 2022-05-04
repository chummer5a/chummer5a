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
using System.Linq;
using System.Windows.Forms;
using ChummerDataViewer.Model;

namespace ChummerDataViewer
{
    public partial class SetupForm : Form
    {
        public string Id => txtId.Text;
        public string Key => txtKey.Text;
        public string BulkData => txtBulk.Text;

        public SetupForm()
        {
            InitializeComponent();
        }

        private void SetupForm_Load(object sender, EventArgs e)
        {
            lblDb.Text = Path.Combine(PersistentState.DatabaseFolder, "persistent.db");

            txtBulk.Text = $"{PersistentState.DatabaseFolder}";
        }

        private void AnyTextChanged(object sender, EventArgs e)
        {
            lblStatus.Visible = true;
            btnOK.Enabled = false;

            if (!txtId.Text.All(x => char.IsUpper(x) || char.IsDigit(x)))
            {
                lblStatus.Text = "Invalid Character in Access Id";
                return;
            }

            if (txtId.Text.Length != 20)
            {
                lblStatus.Text = "Access Id too " + (txtId.Text.Length > 20 ? "long" : "short");
                return;
            }

            if (txtKey.Text.Any(x => "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789+/".IndexOf(x) == -1))
            {
                lblStatus.Text = "Invalid Character in Access Key";
                return;
            }

            if (txtKey.Text.Length != 40)
            {
                lblStatus.Text = "Access Key too " + (txtKey.Text.Length > 40 ? "long" : "short");
                return;
            }

            lblStatus.Visible = false;
            btnOK.Enabled = true;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void btnCanel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}