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
using System.Windows.Forms;

namespace Chummer.UI.Table
{
    public partial class TextTableCell : TableCell
    {
        private Label _label;
        
        public TextTableCell()
        {
            InitializeComponent();
            _label = new Label();
            contentField = _label;
            Controls.Add(_label);
            _label.AutoSize = true;
            MinimumSize = _label.Size;
        }

        protected internal override void UpdateValue(object newValue)
        {
            base.UpdateValue(newValue);
            _label.Text = newValue == null ? "" : newValue.ToString();
            MinimumSize = _label.Size;
        }
    }
}
