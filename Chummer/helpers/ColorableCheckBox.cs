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
    public sealed partial class ColorableCheckBox : CheckBox
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
            DoubleBuffered = true;
            FlatAppearance.MouseDownBackColor = ColorManager.ControlDarkest;
            FlatAppearance.MouseOverBackColor = ColorManager.ControlDarker;
            BackColorChanged += OnBackColorChanged;
        }

        private void OnBackColorChanged(object sender, EventArgs e)
        {
            if (Enabled)
                return;
            FlatAppearance.MouseDownBackColor = BackColor;
            FlatAppearance.MouseOverBackColor = BackColor;
        }

        private bool _blnEnabledBeingSetFromOutside = true;

        private bool _blnRealEnabled = true;

        protected override void OnEnabledChanged(EventArgs e)
        {
            // Safety check to make sure that if base Enabled property is set on the base class instead of this one, we still end up coloring things properly
            if (_blnEnabledBeingSetFromOutside)
            {
                _blnEnabledBeingSetFromOutside = false;
                Enabled = base.Enabled;
                _blnEnabledBeingSetFromOutside = true;
            }
            else
                base.OnEnabledChanged(e);
        }

        public new bool Enabled
        {
            get => _blnRealEnabled;
            set
            {
                if (_blnRealEnabled == value)
                    return;
                _blnRealEnabled = value;
                bool blnOldEnabledBeingSetFromOutside = _blnEnabledBeingSetFromOutside;
                _blnEnabledBeingSetFromOutside = false;
                if (DefaultColorScheme)
                {
                    base.Enabled = value;
                }
                else
                {
                    AutoCheck = value;
                    ForeColor = value ? ColorManager.ControlText : ColorManager.GrayText;
                    base.Enabled = true; // Makes sure we always enable the control so that it obeys color schemes
                }
                _blnEnabledBeingSetFromOutside = blnOldEnabledBeingSetFromOutside;
                if (value)
                {
                    FlatAppearance.MouseDownBackColor = ColorManager.ControlDarkest;
                    FlatAppearance.MouseOverBackColor = ColorManager.ControlDarker;
                }
                else
                {
                    FlatAppearance.MouseDownBackColor = BackColor;
                    FlatAppearance.MouseOverBackColor = BackColor;
                }
            }
        }

        private bool _blnDefaultColorScheme = ColorManager.IsLightMode;

        public bool DefaultColorScheme
        {
            get => _blnDefaultColorScheme;
            set
            {
                if (_blnDefaultColorScheme == value)
                    return;
                _blnDefaultColorScheme = value;
                bool blnOldEnabledBeingSetFromOutside = _blnEnabledBeingSetFromOutside;
                _blnEnabledBeingSetFromOutside = false;
                if (value)
                {
                    AutoCheck = true;
                    FlatStyle = FlatStyle.Standard;
                    ForeColor = ColorManager.ControlText;
                    base.Enabled = _blnRealEnabled;
                }
                else
                {
                    AutoCheck = _blnRealEnabled;
                    FlatStyle = FlatStyle.Flat; // Flat checkboxes' borders obey ForeColor
                    ForeColor = _blnRealEnabled ? ColorManager.ControlText : ColorManager.GrayText;
                    base.Enabled = true;
                }
                _blnEnabledBeingSetFromOutside = blnOldEnabledBeingSetFromOutside;
            }
        }
    }
}
