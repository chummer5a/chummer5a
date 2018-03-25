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
using System.Drawing;
using System.Windows.Forms;

namespace Chummer.UI.Table
{
    public partial class TableRow : UserControl
    {

        private int _index;
        private bool _selected = false;

        public TableRow()
        {
            InitializeComponent();
            Layout += (sender, evt) => DoLayout();
            Update(Index, Selected);
        }

        protected virtual void DoLayout() {
        }

        protected virtual void Update(int index, bool selected)
        {
            if (selected)
            {
                BackColor = SystemColors.Highlight;
            }
            else
            {
                BackColor = (index % 2 == 0) ? Color.White : Color.LightGray;
            }
        }

        public int Index
        {
            get => _index;
            set
            {
                if (_index != value)
                {
                    _index = value;
                    Update(Index, Selected);
                }
            }
        }

        public bool Selected {
            get => _selected;
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    Update(Index, Selected);
                }
            }
        }
    }
}
