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
using System.Windows.Forms;

namespace Chummer
{
    // We use RTFEditor for its shortcuts, but because notes are often displayed as TreeNode tooltips,
    // and because TreeNode tooltips only support plaintext and not any kind of formatting, frmNotes
    // and Notes items in general use plaintext instead of RTF or HTML formatted text.
    public partial class frmNotes : Form
	{
		private static int s_IntWidth = 640;
		private static int s_IntHeight = 360;
	    private readonly bool _blnLoading;
        private string _strNotes;

        #region Control Events
        public frmNotes()
		{
            InitializeComponent();
            rtfNotes.ContentKeyDown += rtfNotes_KeyDown;
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _blnLoading = true;
			Width = s_IntWidth;
			Height = s_IntHeight;
            _blnLoading = false;
        }

		private void frmNotes_FormClosing(object sender, FormClosingEventArgs e)
		{
            Notes = rtfNotes.Text;
            DialogResult = DialogResult.OK;
		}

        private void rtfNotes_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.OK;
        }

        private void frmNotes_Resize(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;

            s_IntWidth = Width;
            s_IntHeight = Height;
        }

        private void frmNotes_Shown(object sender, EventArgs e)
        {
            rtfNotes.FocusContent();
        }
        #endregion

        #region Properties

        /// <summary>
        /// Notes.
        /// </summary>
        public string Notes
        {
            get => _strNotes;
            set
            {
                if (_strNotes != value)
                {
                    _strNotes = value;
                    rtfNotes.Text = value;
                }
            }
        }
        #endregion
    }
}
