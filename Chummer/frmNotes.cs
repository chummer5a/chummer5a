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
﻿using System.Windows.Forms;

namespace Chummer
{
	public partial class frmNotes : Form
	{
		private static int s_IntWidth = 640;
		private static int s_IntHeight = 360;
	    private readonly bool _blnLoading;
        private string _strNotes;
        private string _strHtmlNotes;

        #region Control Events
        public frmNotes()
		{
            InitializeComponent();
			LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            htmNotes.OnBodyKeyDown += htmNotes_OnBodyKeyDown;
            _blnLoading = true;
			Width = s_IntWidth;
			Height = s_IntHeight;
            _blnLoading = false;
        }

		private void frmNotes_FormClosing(object sender, FormClosingEventArgs e)
		{
            Notes = htmNotes.BodyText;
            HtmlNotes = htmNotes.Html;
            DialogResult = DialogResult.OK;
		}

		private void htmNotes_OnBodyKeyDown(object sender, KeyEventArgs e)
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
                    htmNotes.BodyText = value;
                }
            }
        }

        /// <summary>
        /// Notes including Html tags
        /// </summary>
        public string HtmlNotes
        {
            get => _strHtmlNotes;
            set
            {
                if (_strHtmlNotes != value)
                {
                    _strHtmlNotes = value;
                    htmNotes.Html = value;
                }
            }
        }
		#endregion
	}
}
