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
using System.Drawing;
using System.Windows.Forms;

namespace Chummer
{
    // We use TextBox because notes are often displayed as TreeNode tooltips, and because TreeNode tooltips
    // only support plaintext and not any kind of formatting, frmNotes and Notes items in general have to use
    // plaintext instead of RTF or HTML formatted text.
    public partial class frmNotes : Form
	{
        // Set to DPI-based 640 in constructor, needs to be there because of DPI dependency
        private static int _intWidth = int.MinValue;
        // Set to DPI-based 360 in constructor, needs to be there because of DPI dependency
        private static int _intHeight = int.MinValue;
	    private readonly bool _blnLoading;
        private string _strNotes;
        private Color _colNotes;

        #region Control Events
        public frmNotes(string strOldNotes) : this(strOldNotes, ColorManager.HasNotesColor) { }

        public frmNotes(string strOldNotes, Color colNotes)
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            if (_intWidth <= 0 || _intHeight <= 0)
            {
                using (Graphics g = CreateGraphics())
                {
                    if (_intWidth <= 0)
                        _intWidth = (int)(640 * g.DpiX / 96.0f);
                    if (_intHeight <= 0)
                        _intHeight = (int)(360 * g.DpiY / 96.0f);
                }
            }
            _blnLoading = true;
            Width = _intWidth;
			Height = _intHeight;
            _blnLoading = false;
            txtNotes.Text = _strNotes = strOldNotes.NormalizeLineEndings();

            btnColorSelect.Enabled = txtNotes.Text.Length > 0;

            _colNotes = colNotes;
            if (_colNotes.IsEmpty)
                _colNotes = ColorManager.HasNotesColor;

            updateColorRepresentation();
        }

        private void txtNotes_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Close();
            if (e.KeyCode == Keys.Enter && e.Control)
            {
                btnOK_Click(sender, e);
            }
        }

        private void frmNotes_Resize(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;

            _intWidth = Width;
            _intHeight = Height;
        }

        private void frmNotes_Shown(object sender, EventArgs e)
        {
            txtNotes.Focus();
            txtNotes.SelectionLength = 0;
            txtNotes.SelectionStart = txtNotes.TextLength;
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            _strNotes = txtNotes.Text;
            DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void btnColorSelect_Click(object sender, EventArgs e)
        {
            //colorDialog1.Color = ColorManager.GenerateCurrentModeColor(_colNotes);
            _colNotes = colorDialog1.Color; //Selected color is always how it is shown in light mode, use the stored one for it.
            var resNewColor = colorDialog1.ShowDialog();
            if (resNewColor == DialogResult.OK)
            {
                _colNotes = ColorManager.GenerateModeIndependentColor(colorDialog1.Color);
                updateColorRepresentation();
            }
        }
        private void txtNotes_TextChanged(object sender, EventArgs e)
        {
            btnColorSelect.Enabled = txtNotes.TextLength > 0;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Notes.
        /// </summary>
        public string Notes => _strNotes;
        public Color NotesColor => _colNotes;

        #endregion

        private void updateColorRepresentation()
        {
            txtNotes.ForeColor = ColorManager.GenerateCurrentModeColor(_colNotes);
        }
    }
}
