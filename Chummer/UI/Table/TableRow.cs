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

namespace Chummer.UI.Table
{
    public partial class TableRow : UserControl
    {
        private int _intIndex;
        private bool _blnSelected;

        public TableRow()
        {
            InitializeComponent();
            Layout += (sender, evt) => DoLayout();
        }

        private void OnLoad(object sender, EventArgs eventArgs)
        {
            Update(Index, Selected);
        }

        protected virtual void DoLayout() {
        }

        protected virtual void Update(int intIndex, bool blnSelected)
        {
            if (blnSelected)
            {
                BackColor = SystemColors.Highlight;
            }
            else
            {
                BackColor = (intIndex & 1 ) == 0 ? SystemColors.ControlLightLight : SystemColors.Control;
            }
        }

        public int Index
        {
            get => _intIndex;
            set
            {
                if (_intIndex != value)
                {
                    _intIndex = value;
                    Update(Index, Selected);
                }
            }
        }

        public bool Selected {
            get => _blnSelected;
            set
            {
                if (_blnSelected != value)
                {
                    _blnSelected = value;
                    Update(Index, Selected);
                }
            }
        }
    }
}
