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

namespace Chummer.UI.Table
{
    public partial class TextTableCell : TableCell
    {
        private readonly Label _lblText;
        
        public TextTableCell()
        {
            InitializeComponent();
            _lblText = new Label();
            contentField = _lblText;
            Controls.Add(_lblText);
            _lblText.AutoSize = true;
            _lblText.Click += CommonFunctions.OpenPDFFromControl;
        }

        private void OnLoad(object sender, EventArgs eventArgs)
        {
            MinimumSize = _lblText.Size;
        }

        protected internal override void UpdateValue(object newValue)
        {
            base.UpdateValue(newValue);
            _lblText.Text = newValue?.ToString() ?? string.Empty;
            MinimumSize = _lblText.Size;
        }
    }
}
