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
using System.Threading;
using System.Windows.Forms;

namespace Chummer
{
    public class ColorableCheckBox : CheckBoxWithToolTip
    {
        // CheckBox doesn't let you control its colors when it is disabled, so this extended class exists as a hacky version of implementing that
        // It does so by never actually disabling the CheckBox control, instead just controlling the AutoCheck property and coloring the foreground accordingly
        public ColorableCheckBox()
        {
            FlatAppearance.MouseDownBackColor = ColorManager.ControlDarkest;
            FlatAppearance.MouseOverBackColor = ColorManager.ControlDarker;
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            if (!Enabled)
            {
                FlatAppearance.MouseDownBackColor = BackColor;
                FlatAppearance.MouseOverBackColor = BackColor;
            }
        }

        private int _intEnabledBeingSetFromOutside = 1;

        private int _intRealEnabled = 1;

        protected override void OnEnabledChanged(EventArgs e)
        {
            // Safety check to make sure that if base Enabled property is set on the base class instead of this one, we still end up coloring things properly
            if (Interlocked.CompareExchange(ref _intEnabledBeingSetFromOutside, 0, 1) > 0)
            {
                try
                {
                    Enabled = base.Enabled;
                }
                finally
                {
                    Interlocked.Increment(ref _intEnabledBeingSetFromOutside);
                }
            }
            else
                base.OnEnabledChanged(e);
        }

        public new bool Enabled
        {
            get => _intRealEnabled > 0;
            set
            {
                int intNewValue = value.ToInt32();
                if (Interlocked.Exchange(ref _intRealEnabled, intNewValue) == intNewValue)
                    return;
                Interlocked.Decrement(ref _intEnabledBeingSetFromOutside);
                try
                {
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
                }
                finally
                {
                    Interlocked.Increment(ref _intEnabledBeingSetFromOutside);
                }

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

        private int _intDefaultColorScheme = ColorManager.IsLightMode.ToInt32();

        public bool DefaultColorScheme
        {
            get => _intDefaultColorScheme > 0;
            set
            {
                int intNewValue = value.ToInt32();
                if (Interlocked.Exchange(ref _intDefaultColorScheme, intNewValue) == intNewValue)
                    return;
                Interlocked.Decrement(ref _intEnabledBeingSetFromOutside);
                try
                {
                    bool blnRealEnabled = _intRealEnabled > 0;
                    if (value)
                    {
                        AutoCheck = true;
                        FlatStyle = FlatStyle.Standard;
                        ForeColor = ColorManager.ControlText;
                        base.Enabled = blnRealEnabled;
                    }
                    else
                    {
                        AutoCheck = blnRealEnabled;
                        FlatStyle = FlatStyle.Flat; // Flat checkboxes' borders obey ForeColor
                        ForeColor = blnRealEnabled ? ColorManager.ControlText : ColorManager.GrayText;
                        base.Enabled = true;
                    }
                }
                finally
                {
                    Interlocked.Increment(ref _intEnabledBeingSetFromOutside);
                }
            }
        }
    }
}
