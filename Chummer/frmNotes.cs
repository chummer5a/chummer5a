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
﻿using System.IO;
﻿using System.Text;
﻿using System.Windows.Documents;
﻿using System.Windows.Forms;

namespace Chummer
{
	public partial class frmNotes : Form
	{
		private static int _intWidth = 534;
		private static int _intHeight = 278;
	    private readonly bool _blnLoading;
		private string _strNotes = "";
		RichTextBoxExtended objExtended = new RichTextBoxExtended();
		#region Control Events
		public frmNotes()
		{
			InitializeComponent();
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
			_blnLoading = true;
			Width = _intWidth;
			Height = _intHeight;
            objExtended.Dock = DockStyle.Fill;
            this.Controls.Add(objExtended);
            _blnLoading = false;
		}

		private void frmNotes_FormClosing(object sender, FormClosingEventArgs e)
		{
            Notes = objExtended.RichTextBox.Text;
            FormattedNotes = objExtended.RichTextBox.Rtf;
            DialogResult = DialogResult.OK;
		}

		private void txtNotes_KeyDown(object sender, KeyEventArgs e)
		{
            
			if (e.KeyCode == Keys.Escape)
				DialogResult = DialogResult.OK;

            if (e.Control && e.KeyCode == Keys.A)
            {
                e.SuppressKeyPress = true;
                if (sender != null)
                    ((TextBox)sender).SelectAll();
            }
        }

        private void frmNotes_Resize(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;

            _intWidth = Width;
            _intHeight = Height;
        }
        #endregion

		#region Properties
		/// <summary>
		/// Notes.
		/// </summary>
		public string Notes
		{
			get
			{
				return _strNotes;
			}
			set
			{
				_strNotes = value;
			}
		}

		/// <summary>
		/// RTF Formatted notes.
		/// </summary>
		public string FormattedNotes { get; set; }
		#endregion
	}
}