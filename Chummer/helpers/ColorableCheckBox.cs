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
using System.ComponentModel;
using System.Windows.Forms;

namespace Chummer
{
    public partial class ColorableCheckBox : CheckBox
    {
        // CheckBox doesn't let you control its colors when it is disabled, so this extended class exists as a hacky version of implementing that
        // It does so by never actually disabling the CheckBox control, instead just controlling the AutoCheck property and coloring the foreground accordingly
        public ColorableCheckBox() : this(null)
        {
        }

        public ColorableCheckBox(IContainer container)
        {
            container?.Add(this);

            InitializeComponent();
            FlatStyle = FlatStyle.Flat; // Flat checkboxes' borders obey ForeColor
            FlatAppearance.MouseDownBackColor = ColorManager.ControlDarkest;
            FlatAppearance.MouseOverBackColor = ColorManager.ControlDarker;
            BackColorChanged += OnBackColorChanged;
        }

        private void OnBackColorChanged(object sender, EventArgs e)
        {
            if (!Enabled)
            {
                FlatAppearance.MouseDownBackColor = BackColor;
                FlatAppearance.MouseOverBackColor = BackColor;
            }
        }

        private bool _blnRealEnabled = true;

        public new bool Enabled
        {
            get => _blnRealEnabled;
            set
            {
                if (_blnRealEnabled == value)
                    return;
                AutoCheck = _blnRealEnabled = value;
                if (value)
                {
                    ForeColor = ColorManager.ControlText;
                    FlatAppearance.MouseDownBackColor = ColorManager.ControlDarkest;
                    FlatAppearance.MouseOverBackColor = ColorManager.ControlDarker;
                }
                else
                {
                    ForeColor = ColorManager.GrayText;
                    FlatAppearance.MouseDownBackColor = BackColor;
                    FlatAppearance.MouseOverBackColor = BackColor;
                }
            }
        }
    }
}
